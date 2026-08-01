using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace EricksonLopez.Result.Analyzers;

/// <summary>
/// Roslyn diagnostic analyzer (RESULT007) that warns when:
/// <list type="bullet">
///   <item><c>HashSet&lt;Error&gt;</c> or <c>Dictionary&lt;Error, ...&gt;</c> is instantiated
///   without an explicit <see cref="System.Collections.Generic.IEqualityComparer{T}"/> of type <c>Error</c>.</item>
///   <item>LINQ <c>Enumerable.Distinct()</c> or <c>Enumerable.Distinct(IEqualityComparer&lt;Error&gt;)</c>
///   is called without <c>ErrorEqualityComparer.Strict</c> on an <c>IEnumerable&lt;Error&gt;</c>.</item>
///   <item>LINQ <c>Enumerable.DistinctBy()</c>, <c>Enumerable.GroupBy()</c>, or <c>Enumerable.GroupBy(keySelector, comparer)</c>
///   use <c>Error</c> as the key type without an explicit strict comparer.</item>
/// </list>
/// </summary>
/// <remarks>
/// <para>
/// <b>Root cause:</b> <c>Error.Equals(Error?)</c> performs a
/// <i>shallow</i> equality check on 5 semantic fields (<c>Code</c>, <c>Description</c>, <c>Type</c>,
/// <c>Severity</c>, <c>Retryability</c>). Fields like <c>TraceId</c>, <c>CorrelationId</c>,
/// and <c>Metadata</c> are intentionally excluded because they vary per request.
/// </para>
/// <para>
/// This means that when two errors share the same semantic fields but differ in <c>TraceId</c>
/// or <c>Metadata</c>, any collection or LINQ operation using the default comparer will silently
/// deduplicate them — losing one of the errors.
/// </para>
/// <para>
/// <b>Fix:</b> Use <c>ErrorEqualityComparer.Strict</c> to include all fields in equality checks,
/// or use <c>ErrorEqualityComparer.Default</c> if semantic deduplication is intentional.
/// </para>
/// </remarks>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class HashSetErrorEqualityAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "RESULT007";

    private const string HashSetMetadataName = "HashSet`1";
    private const string DictionaryMetadataName = "Dictionary`2";
    private const string ErrorFullName = "EricksonLopez.Result.Error";

    // LINQ method names that are subject to silent deduplication
    private static readonly ImmutableHashSet<string> LinqDeduplicationMethods = ImmutableHashSet.Create(
        System.StringComparer.Ordinal,
        "Distinct",
        "DistinctBy",
        "GroupBy",
        "ToHashSet");

    private static readonly DiagnosticDescriptor Rule = new(
        id: DiagnosticId,
        title: "Missing ErrorEqualityComparer.Strict in collection or LINQ deduplication",
        messageFormat: "'{0}' uses Error with the default equality comparer, which performs a shallow comparison " +
                       "(Code, Description, Type, Severity, Retryability only). " +
                       "Errors with identical semantic fields but different TraceId, CorrelationId, or Metadata will be silently deduplicated. " +
                       "Use ErrorEqualityComparer.Strict for deep structural equality, or ErrorEqualityComparer.Default if semantic deduplication is intentional.",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Error.Equals ignores contextual fields like TraceId and Metadata. If you use Error as a key in a HashSet, " +
                     "Dictionary, or a LINQ operation like Distinct(), GroupBy(), or ToHashSet() without ErrorEqualityComparer.Strict, " +
                     "you may lose data through unintended deduplication. Two errors with the same Code and Description but different " +
                     "TraceId or Metadata will be treated as identical.",
        helpLinkUri: "https://github.com/ericksonlopez/dotnet-result/blob/main/docs/analyzers.md#RESULT007");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

        // Check 1: HashSet<Error>/Dictionary<Error,...> instantiation without comparer
        context.RegisterOperationAction(AnalyzeObjectCreation, OperationKind.ObjectCreation);

        // Check 2: LINQ Distinct(), DistinctBy(), GroupBy(), ToHashSet() without explicit Error comparer
        context.RegisterOperationAction(AnalyzeLinqInvocation, OperationKind.Invocation);
    }

    private static void AnalyzeObjectCreation(OperationAnalysisContext context)
    {
        var objectCreation = (IObjectCreationOperation)context.Operation;
        var type = objectCreation.Type as INamedTypeSymbol;

        if (type == null) return;

        bool isHashSet = type.OriginalDefinition.MetadataName == HashSetMetadataName;
        bool isDictionary = type.OriginalDefinition.MetadataName == DictionaryMetadataName;

        if (!isHashSet && !isDictionary) return;

        var typeArgs = type.TypeArguments;
        if (typeArgs.Length == 0) return;

        // For HashSet<T>, the Error type is at index 0. For Dictionary<K,V>, it's at index 0 (the key).
        if (typeArgs[0].ToDisplayString() != ErrorFullName) return;

        // Check if any argument passed to the constructor is an IEqualityComparer<Error>
        bool hasComparer = false;
        foreach (var argument in objectCreation.Arguments)
        {
            var paramType = argument.Parameter?.Type as INamedTypeSymbol;
            if (paramType != null && paramType.OriginalDefinition.MetadataName == "IEqualityComparer`1")
            {
                hasComparer = true;
                break;
            }
        }

        if (!hasComparer)
        {
            var collectionType = isHashSet ? "HashSet<Error>" : "Dictionary<Error, ...>";
            var diagnostic = Diagnostic.Create(Rule, objectCreation.Syntax.GetLocation(), collectionType);
            context.ReportDiagnostic(diagnostic);
        }
    }

    private static void AnalyzeLinqInvocation(OperationAnalysisContext context)
    {
        var invocation = (IInvocationOperation)context.Operation;
        var method = invocation.TargetMethod;

        // Quick name-based pre-filter
        if (!LinqDeduplicationMethods.Contains(method.Name))
            return;

        // Must be in System.Linq namespace
        var containingNamespace = method.ContainingNamespace?.ToDisplayString();
        if (containingNamespace is null ||
            !string.Equals(containingNamespace, "System.Linq", System.StringComparison.Ordinal))
            return;

        // The first type parameter of the method must be Error, OR
        // the source sequence element type must be Error.
        // For Distinct<Error>(), DistinctBy<Error,...>(), GroupBy<Error,...>(), ToHashSet<Error>()
        // the first type parameter is the source element type.
        var typeArgs = method.TypeArguments;
        if (typeArgs.Length == 0)
            return;

        // For Distinct<T>(): T is at index 0
        // For DistinctBy<TSource,TKey>(): TSource at index 0
        // For GroupBy<TSource,TKey>(): TSource at index 0
        // For ToHashSet<TSource>(): TSource at index 0
        var sourceType = typeArgs[0];
        if (sourceType.ToDisplayString() != ErrorFullName)
            return;

        // Check if an IEqualityComparer<Error> argument was explicitly provided
        // (ArgumentKind.DefaultValue means the parameter was omitted)
        bool hasComparer = false;
        foreach (var arg in invocation.Arguments)
        {
            if (arg.Parameter is null) continue;

            var paramType = arg.Parameter.Type as INamedTypeSymbol;
            if (paramType is null) continue;

            var paramTypeMetadataName = paramType.OriginalDefinition.MetadataName;
            if (paramTypeMetadataName == "IEqualityComparer`1" &&
                arg.ArgumentKind == ArgumentKind.Explicit)
            {
                // Even if explicit, check if it's a constant null (which means "no comparer")
                if (arg.Value.ConstantValue.HasValue &&
                    arg.Value.ConstantValue.Value is null)
                {
                    // Explicit null — treat as no comparer provided
                    hasComparer = false;
                }
                else
                {
                    hasComparer = true;
                }
                break;
            }
        }

        if (!hasComparer)
        {
            var diagnostic = Diagnostic.Create(Rule, invocation.Syntax.GetLocation(), $"{method.Name}<Error>");
            context.ReportDiagnostic(diagnostic);
        }
    }
}
