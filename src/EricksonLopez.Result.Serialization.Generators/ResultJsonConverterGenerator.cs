using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace EricksonLopez.Result.Serialization.Generators;

/// <summary>
/// Incremental source generator that scans for <c>[JsonSerializable(typeof(Result&lt;T&gt;))]</c> attributes
/// on <c>JsonSerializerContext</c> subclasses and generates a companion extension method that registers
/// the required <c>ResultOfTJsonConverter&lt;T&gt;</c> instances — eliminating the reflection-based
/// <c>MakeGenericType</c> + <c>Activator.CreateInstance</c> in <c>ResultJsonConverterFactory</c>.
/// </summary>
/// <remarks>
/// <para>
/// <b>Usage:</b> Consumers add <c>[JsonSerializable(typeof(Result&lt;MyDto&gt;))]</c> to their
/// <c>JsonSerializerContext</c> subclass and this generator produces:
/// </para>
/// <code>
/// public static class MyContextResultExtensions
/// {
///     public static JsonSerializerOptions AddResultConverters(this JsonSerializerOptions options)
///     {
///         options.Converters.Add(new ResultJsonConverter());
///         options.Converters.Add(new ErrorJsonConverter());
///         options.Converters.Add(new ResultOfTJsonConverter&lt;MyDto&gt;());
///         return options;
///     }
/// }
/// </code>
/// </remarks>
[Generator(LanguageNames.CSharp)]
public sealed class ResultJsonConverterGenerator : IIncrementalGenerator
{
    private const string ResultFullName = "EricksonLopez.Result.Result";
    private const string ResultMetadataName = "Result`1";
    private const string JsonSerializableAttribute = "System.Text.Json.Serialization.JsonSerializableAttribute";
    private const string JsonSerializerContext = "System.Text.Json.Serialization.JsonSerializerContext";

    /// <summary>
    /// The version of this generator assembly, read once at class initialization time.
    /// Used in <c>[GeneratedCode]</c> attributes of generated files so consumers can identify
    /// which version of the generator produced a given file.
    /// </summary>
    private static readonly string GeneratorVersion = typeof(ResultJsonConverterGenerator).Assembly.GetName().Version?.ToString() ?? "1.0.0";

    /// <summary>
    /// Reported when [JsonSerializable(typeof(Result))] without a type argument is found on a
    /// JsonSerializerContext subclass. The non-generic Result does not require generated converters
    /// (it is handled by ResultJsonConverter, not ResultOfTJsonConverter), but the presence of
    /// [JsonSerializable(typeof(Result))] alongside [JsonSerializable(typeof(Result&lt;T&gt;))] is
    /// often a mistake: the developer likely intended Result&lt;T&gt;, not non-generic Result.
    /// </summary>
    private static readonly DiagnosticDescriptor NonGenericResultWarning = new(
        id: "RESULT_GEN_001",
        title: "[JsonSerializable(typeof(Result))] has no effect for converter generation",
        messageFormat: "[JsonSerializable(typeof(Result))] on '{0}' does not generate a ResultJsonConverter. " +
                       "Use [JsonSerializable(typeof(Result<YourType>))] to generate a ResultOfTJsonConverter<YourType>. " +
                       "The non-generic ResultJsonConverter is added automatically by AddResultConverters() and does not require [JsonSerializable].",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description:
            "The source generator only generates ResultOfTJsonConverter<T> registrations for Result<T> types. " +
            "Non-generic Result is handled by ResultJsonConverter which is always registered by AddResultConverters(). " +
            "Add [JsonSerializable(typeof(Result<YourDto>))] to register the typed converter for Result<YourDto>.",
        helpLinkUri: "https://github.com/ericksonlopez/dotnet-result/blob/main/docs/serialization.md");

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Find all class declarations that derive from JsonSerializerContext
        var contextClasses = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (node, _) => IsCandidateClass(node),
                transform: static (ctx, ct) => GetContextInfo(ctx, ct))
            .Where(static info => info is not null)
            .Select(static (info, _) => info!.Value);

        // Combine with compilation
        var compilationAndContexts = context.CompilationProvider.Combine(contextClasses.Collect());

        context.RegisterSourceOutput(compilationAndContexts, static (spc, source) =>
        {
            var (compilation, contexts) = source;
            Execute(compilation, contexts, spc);
        });
    }

    private static bool IsCandidateClass(SyntaxNode node)
    {
        // Quick syntactic filter: must be a partial class with attributes
        return node is ClassDeclarationSyntax classDecl
            && classDecl.AttributeLists.Count > 0
            && classDecl.Modifiers.Any(m => m.ValueText == "partial");
    }

    private static ContextInfo? GetContextInfo(GeneratorSyntaxContext context, CancellationToken ct)
    {
        var classDecl = (ClassDeclarationSyntax)context.Node;
        var classSymbol = context.SemanticModel.GetDeclaredSymbol(classDecl, ct) as INamedTypeSymbol;
        if (classSymbol is null) return null;

        // Check if it inherits from JsonSerializerContext
        if (!InheritsFrom(classSymbol, JsonSerializerContext)) return null;

        // Collect all Result<T> types from [JsonSerializable] attributes
        var resultTypes = ImmutableArray.CreateBuilder<ResultTypeInfo>();
        foreach (var attrData in classSymbol.GetAttributes())
        {
            if (attrData.AttributeClass?.ToDisplayString() != JsonSerializableAttribute)
                continue;

            if (attrData.ConstructorArguments.Length == 0)
                continue;

            var typeArg = attrData.ConstructorArguments[0];
            if (typeArg.Value is not INamedTypeSymbol typeSymbol)
                continue;

            // Detect [JsonSerializable(typeof(Result))] without a generic type argument.
            // This has no effect for converter generation and is likely a developer mistake.
            // Emit RESULT_GEN_001 to guide them toward [JsonSerializable(typeof(Result<T>))].
            if (typeSymbol.OriginalDefinition.ToDisplayString() == ResultFullName
                && !typeSymbol.IsGenericType)
            {
                // We cannot easily emit a Diagnostic from GetContextInfo (transform phase);
                // record a sentinel in resultTypes and handle it in Execute().
                // Use a special marker type name to signal the non-generic Result detection.
                resultTypes.Add(new ResultTypeInfo(
                    "__NonGenericResult__",
                    classSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                    attrData.ApplicationSyntaxReference?.GetSyntax(ct)?.GetLocation()));
                continue;
            }

            // Check if it's Result<T>
            if (typeSymbol.OriginalDefinition.MetadataName == ResultMetadataName &&
                typeSymbol.ContainingNamespace.ToDisplayString() == "EricksonLopez.Result")
            {
                var innerType = typeSymbol.TypeArguments[0];
                var fullyQualified = innerType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

                // Compute the STJ-compatible JsonTypeInfo property name.
                // STJ generates the property name on the context class by mangling generic type names:
                //   - Simple types: use Name directly (e.g., "OrderDto")
                //   - Generic types: concatenate outer name + mangled inner names recursively
                //     (e.g., List<OrderDto> -> "ListOrderDto", Dictionary<string,int> -> "DictionaryStringInt32")
                //   - Nullable<T>: "NullableT" (e.g., Nullable<int> -> "NullableInt32")
                // innerType.Name alone is wrong for generics — it returns "List" instead of "ListOrderDto".
                var typeInfoPropertyName = GetStjTypeInfoPropertyName(innerType);
                resultTypes.Add(new ResultTypeInfo(fullyQualified, typeInfoPropertyName));
            }
        }

        if (resultTypes.Count == 0) return null;

        return new ContextInfo(
            ClassName: classSymbol.Name,
            FullyQualifiedClassName: classSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
            Namespace: classSymbol.ContainingNamespace.IsGlobalNamespace
                ? null
                : classSymbol.ContainingNamespace.ToDisplayString(),
            ResultValueTypes: resultTypes.ToImmutable());
    }

    /// <summary>
    /// Computes the name that System.Text.Json source generation uses for the JsonTypeInfo property
    /// on the JsonSerializerContext subclass. STJ mangles generic type names by concatenating the
    /// outer type's <see cref="ISymbol.Name"/> with each type argument's mangled name, recursively.
    /// </summary>
    /// <remarks>
    /// Examples:
    /// <list type="bullet">
    ///   <item><c>OrderDto</c> → <c>"OrderDto"</c></item>
    ///   <item><c>List&lt;OrderDto&gt;</c> → <c>"ListOrderDto"</c></item>
    ///   <item><c>int?</c> (Nullable&lt;int&gt;) → <c>"NullableInt32"</c></item>
    ///   <item><c>Dictionary&lt;string, int&gt;</c> → <c>"DictionaryStringInt32"</c></item>
    /// </list>
    /// <para>
    /// <b>Namespace collision prevention:</b> When two types share the same simple name but live in
    /// different namespaces (e.g., <c>MyNsA.OrderDto</c> and <c>MyNsB.OrderDto</c>), the simple name
    /// <c>"OrderDto"</c> would collide. In that case, the fully-qualified name with dots replaced by
    /// underscores is used as the property name suffix to guarantee uniqueness.
    /// </para>
    /// </remarks>
    private static string GetStjTypeInfoPropertyName(ITypeSymbol typeSymbol)
    {
        if (typeSymbol is INamedTypeSymbol named && named.IsGenericType)
        {
            // STJ concatenates outer name + each type argument's mangled name
            var sb = new StringBuilder(named.Name);
            foreach (var typeArg in named.TypeArguments)
            {
                sb.Append(GetStjTypeInfoPropertyName(typeArg));
            }
            return sb.ToString();
        }

        // For arrays: STJ uses element type name + "Array" (e.g., int[] -> "Int32Array")
        if (typeSymbol is IArrayTypeSymbol array)
        {
            return GetStjTypeInfoPropertyName(array.ElementType) + "Array";
        }

        // For simple types and primitives: use the metadata name
        // Special-case C# keyword type aliases to their CLR names (string -> String, int -> Int32, etc.)
        // because STJ uses CLR names, not C# aliases.
        return typeSymbol.MetadataName;
    }

    /// <summary>
    /// Computes a collision-safe, unique suffix for the generated STJ type info property name.
    /// Uses the simple <see cref="GetStjTypeInfoPropertyName"/> as the primary name, and falls back to
    /// a fully-qualified name (dots replaced with underscores) when a collision is detected
    /// within the same <see cref="ContextInfo"/>.
    /// </summary>
    private static ImmutableArray<ResultTypeInfo> DeduplicateTypeInfoPropertyNames(ImmutableArray<ResultTypeInfo> types)
    {
        // Check if any two entries share the same TypeInfoPropertyName.
        var seen = new Dictionary<string, int>(StringComparer.Ordinal);
        bool hasCollision = false;
        foreach (var t in types)
        {
            if (t.FullyQualifiedName == "__NonGenericResult__") continue;
            if (seen.TryGetValue(t.TypeInfoPropertyName, out _))
            {
                hasCollision = true;
                break;
            }
            seen[t.TypeInfoPropertyName] = 1;
        }

        if (!hasCollision) return types;

        // Rebuild with fully-qualified names (using underscores) as the property name to avoid collisions.
        var builder = ImmutableArray.CreateBuilder<ResultTypeInfo>(types.Length);
        foreach (var t in types)
        {
            if (t.FullyQualifiedName == "__NonGenericResult__")
            {
                builder.Add(t);
                continue;
            }
            // Convert "global::MyNs.Sub.OrderDto" -> "MyNs_Sub_OrderDto" (valid C# identifier suffix).
            var uniqueName = t.FullyQualifiedName
                .Replace("global::", string.Empty)
                .Replace('.', '_')
                .Replace('<', '_')
                .Replace('>', '_')
                .Replace(',', '_')
                .Replace(' ', '_');
            builder.Add(new ResultTypeInfo(t.FullyQualifiedName, uniqueName, t.DiagnosticLocation));
        }
        return builder.ToImmutable();
    }

    private static bool InheritsFrom(INamedTypeSymbol symbol, string baseFullName)
    {
        var current = symbol.BaseType;
        while (current != null)
        {
            if (current.ToDisplayString() == baseFullName)
                return true;
            current = current.BaseType;
        }
        return false;
    }

    private static void Execute(
        Compilation compilation,
        ImmutableArray<ContextInfo> contexts,
        SourceProductionContext spc)
    {
        if (contexts.IsDefaultOrEmpty) return;

        foreach (var info in contexts.Distinct())
        {
            // Separate non-generic Result markers from actual Result<T> entries.
            // Non-generic Result entries were detected in GetContextInfo() and stored with
            // a sentinel FullyQualifiedName of "__NonGenericResult__" and the class name
            // in TypeInfoPropertyName, plus the attribute location in DiagnosticLocation.
            var realTypes = info.ResultValueTypes.Where(t => t.FullyQualifiedName != "__NonGenericResult__").ToImmutableArray();
            var nonGenericMarkers = info.ResultValueTypes.Where(t => t.FullyQualifiedName == "__NonGenericResult__");

            foreach (var marker in nonGenericMarkers)
            {
                spc.ReportDiagnostic(Diagnostic.Create(
                    NonGenericResultWarning,
                    marker.DiagnosticLocation ?? Location.None,
                    info.ClassName));
            }

            if (realTypes.IsDefaultOrEmpty) continue;

            // Deduplicate TypeInfoPropertyNames to prevent CS0102 when two types share the same
            // simple name but live in different namespaces (e.g. MyNsA.OrderDto + MyNsB.OrderDto).
            var deduplicatedTypes = DeduplicateTypeInfoPropertyNames(realTypes);

            var sourceInfo = new ContextInfo(
                info.ClassName,
                info.FullyQualifiedClassName,
                info.Namespace,
                deduplicatedTypes);

            var source = GenerateSource(sourceInfo);
            var hintName = $"{info.ClassName}ResultConverters.g.cs";
            spc.AddSource(hintName, SourceText.From(source, Encoding.UTF8));
        }
    }

    private static string GenerateSource(ContextInfo info)
    {
        var sb = new StringBuilder();
        sb.AppendLine("// <auto-generated />");
        sb.AppendLine("#nullable enable");
        // Note: #pragma warning disable CS0618 is NOT needed here — this generator
        // uses the AOT-safe ResultOfTJsonConverter<T> constructor that accepts JsonTypeInfo<T>,
        // not the [Obsolete] reflection-based parameterless constructor.
        sb.AppendLine();

        if (info.Namespace != null)
        {
            sb.AppendLine($"namespace {info.Namespace};");
            sb.AppendLine();
        }

        sb.AppendLine("/// <summary>");
        sb.AppendLine($"/// Generated extension methods for <see cref=\"{info.ClassName}\"/> that register");
        sb.AppendLine("/// AOT-compatible Result JSON converters without reflection.");
        sb.AppendLine("/// </summary>");
        sb.AppendLine($"[System.CodeDom.Compiler.GeneratedCodeAttribute(\"EricksonLopez.Result.Serialization.Generators\", \"{GeneratorVersion}\")]");
        sb.AppendLine($"public static class {info.ClassName}ResultExtensions");
        sb.AppendLine("{");
        sb.AppendLine("    /// <summary>");
        sb.AppendLine("    /// Adds all required Result JSON converters to the specified options.");
        sb.AppendLine("    /// This method is AOT-safe and does not use reflection.");
        sb.AppendLine("    /// Uses the source-generated JsonTypeInfo from the context for each Result&lt;T&gt; type.");
        sb.AppendLine("    /// </summary>");
        sb.AppendLine("    /// <param name=\"options\">The serializer options to configure.</param>");
        sb.AppendLine("    /// <returns>The same options instance for chaining.</returns>");
        sb.AppendLine("    public static System.Text.Json.JsonSerializerOptions AddResultConverters(this System.Text.Json.JsonSerializerOptions options)");
        sb.AppendLine("    {");
        sb.AppendLine("        options.Converters.Add(new EricksonLopez.Result.Serialization.ResultJsonConverter());");
        sb.AppendLine("        options.Converters.Add(new EricksonLopez.Result.Serialization.ErrorJsonConverter());");

        foreach (var typeInfo in info.ResultValueTypes)
        {
            // Use the AOT-safe constructor with JsonTypeInfo<T> from the context's Default instance.
            // This avoids the [Obsolete] reflection-based constructor and ensures NativeAOT compatibility.
            sb.AppendLine($"        options.Converters.Add(new EricksonLopez.Result.Serialization.ResultOfTJsonConverter<{typeInfo.FullyQualifiedName}>({info.FullyQualifiedClassName}.Default.{typeInfo.TypeInfoPropertyName}));");
        }

        sb.AppendLine("        return options;");
        sb.AppendLine("    }");
        sb.AppendLine("}");

        return sb.ToString();
    }

    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    private readonly struct ResultTypeInfo : IEquatable<ResultTypeInfo>
    {
        public readonly string FullyQualifiedName;
        public readonly string TypeInfoPropertyName;
        // Location of the [JsonSerializable] attribute for diagnostic reporting.
        // Only populated for non-generic Result markers (sentinel entries).
        public readonly Location? DiagnosticLocation;

        public ResultTypeInfo(string fullyQualifiedName, string typeInfoPropertyName, Location? diagnosticLocation = null)
        {
            FullyQualifiedName = fullyQualifiedName;
            TypeInfoPropertyName = typeInfoPropertyName;
            DiagnosticLocation = diagnosticLocation;
        }

        public bool Equals(ResultTypeInfo other)
            => FullyQualifiedName == other.FullyQualifiedName
            && TypeInfoPropertyName == other.TypeInfoPropertyName;

        public override bool Equals(object? obj) => obj is ResultTypeInfo other && Equals(other);
        public override int GetHashCode() => (FullyQualifiedName, TypeInfoPropertyName).GetHashCode();

        public static bool operator ==(ResultTypeInfo left, ResultTypeInfo right) => left.Equals(right);
        public static bool operator !=(ResultTypeInfo left, ResultTypeInfo right) => !left.Equals(right);
    }

    /// <summary>
    /// Represents a JsonSerializerContext class that has Result&lt;T&gt; types registered via
    /// [JsonSerializable] attributes. Implemented as a readonly record struct so that incremental
    /// generator equality checks work correctly — a mutable struct with List&lt;T&gt; would risk
    /// hash instability if the list were modified after use as a dictionary key.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    private readonly record struct ContextInfo(
        string ClassName,
        string FullyQualifiedClassName,
        string? Namespace,
        ImmutableArray<ResultTypeInfo> ResultValueTypes) : IEquatable<ContextInfo>
    {
        public bool Equals(ContextInfo other)
        {
            if (ClassName != other.ClassName || Namespace != other.Namespace)
                return false;
            if (ResultValueTypes.Length != other.ResultValueTypes.Length)
                return false;
            for (int i = 0; i < ResultValueTypes.Length; i++)
            {
                if (ResultValueTypes[i] != other.ResultValueTypes[i])
                    return false;
            }
            return true;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hash = 17;
                hash = hash * 31 + (ClassName?.GetHashCode() ?? 0);
                hash = hash * 31 + (Namespace?.GetHashCode() ?? 0);
                foreach (var t in ResultValueTypes)
                    hash = hash * 31 + t.FullyQualifiedName.GetHashCode();
                return hash;
            }
        }
    }
}

