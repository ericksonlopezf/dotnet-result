using System;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace EricksonLopez.Result.Analyzers;

/// <summary>
/// Roslyn diagnostic analyzer that warns when <c>Result&lt;T&gt;</c> is used with a struct type
/// whose estimated size exceeds 64 bytes. Large structs cause excessive copying on every
/// pipeline operation (Map, Bind, Tap, etc.) because <c>Result&lt;T&gt;</c> is a readonly struct
/// that copies by value.
/// </summary>
/// <remarks>
/// Rule RESULT001: "Result&lt;T&gt; with T = '{TypeName}' (estimated {Size} bytes). Consider using
/// a class or wrapping in a class to avoid excessive struct copying in Result pipeline operations."
/// </remarks>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class LargeResultValueAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "RESULT001";

    // The .NET Runtime Team recommends pass-by-value structs be at most ~16 bytes for optimal
    // register allocation on x86-64. Result<T> adds 9 bytes of overhead (ResultState byte +
    // Error? reference at 8 bytes), so the effective threshold for TValue is ~23 bytes, giving
    // a total struct size of ~32 bytes as a practical warning threshold.
    // The threshold is set to 32 (not 24) to avoid noisy false positives on common types:
    //   decimal = 16B → Result<decimal> ≈ 25B (below 32, no warning)
    //   Guid    = 16B → Result<Guid>    ≈ 25B (below 32, no warning)
    //   DateTimeOffset = 16B → Result<DateTimeOffset> ≈ 25B (below 32, no warning)
    // Types that DO exceed 32B (e.g., 3 longs = 24B → Result ≈ 33B) will still be warned.
    private const int MaxRecommendedStructSize = 32;
    private const string ResultOfTMetadataName = "Result`1";
    private const string ResultNamespace = "EricksonLopez.Result";

    private static readonly DiagnosticDescriptor Rule = new(
        id: DiagnosticId,
        title: "Result<T> value type is excessively large",
        messageFormat: "Result<{0}> uses a struct value type estimated at {1}+ bytes. Structs larger than 32 bytes cause noticeable copying in Result pipeline operations (Map, Bind, Tap, etc.) because Result<T> is a readonly struct that copies TValue on every call. Consider using a class or a smaller struct.",
        category: "Performance",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Result<T> is a readonly struct that copies the entire TValue on every pipeline operation. The .NET Runtime Team recommends structs intended for pass-by-value be at most ~16 bytes. When TValue causes the total Result<T> size to exceed 32 bytes (the practical warning threshold), copying overhead becomes significant in hot-path code. Common types like decimal, Guid, and DateTimeOffset (all 16B) do NOT trigger this warning. Consider wrapping larger value types in a class.",
        helpLinkUri: "https://github.com/ericksonlopez/dotnet-result/blob/main/docs/performance.md#large-struct-warning");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        => ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        // Analyze generic name syntax to find Result<LargeStruct> usages
        context.RegisterSymbolAction(AnalyzeNamedType, SymbolKind.Method, SymbolKind.Property, SymbolKind.Field);
    }

    private void AnalyzeNamedType(SymbolAnalysisContext context)
    {
        // Check return types and parameter types for Result<T> with large structs
        switch (context.Symbol)
        {
            case IMethodSymbol method:
                CheckType(method.ReturnType, method.Locations, context);
                foreach (var param in method.Parameters)
                    CheckType(param.Type, param.Locations, context);
                break;

            case IPropertySymbol property:
                CheckType(property.Type, property.Locations, context);
                break;

            case IFieldSymbol field:
                CheckType(field.Type, field.Locations, context);
                break;
        }
    }

    private static void CheckType(ITypeSymbol type, ImmutableArray<Location> locations, SymbolAnalysisContext context)
    {
        if (type is not INamedTypeSymbol namedType) return;
        if (namedType.OriginalDefinition.MetadataName != ResultOfTMetadataName) return;
        if (namedType.OriginalDefinition.ContainingNamespace?.ToDisplayString() != ResultNamespace) return;
        if (namedType.TypeArguments.Length != 1) return;

        var valueType = namedType.TypeArguments[0];

        // Only warn for struct types
        if (!valueType.IsValueType) return;

        // Skip primitive types — they're always small
        if (valueType.SpecialType != SpecialType.None) return;

        // Estimate the struct size
        var estimatedSize = EstimateStructSize(valueType);
        if (estimatedSize > MaxRecommendedStructSize)
        {
            var location = locations.FirstOrDefault() ?? Location.None;
            var diagnostic = Diagnostic.Create(
                Rule,
                location,
                valueType.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat),
                estimatedSize);
            context.ReportDiagnostic(diagnostic);
        }
    }

    /// <summary>
    /// Estimates the memory layout size of a struct type by summing field sizes with alignment.
    /// Accounts for padding between fields based on natural alignment rules.
    /// This is a conservative heuristic that approximates the CLR's actual layout.
    /// </summary>
    private static int EstimateStructSize(ITypeSymbol type)
    {
        if (type is not INamedTypeSymbol namedType) return 0;

        int currentOffset = 0;
        int maxAlignment = 1;

        foreach (var member in namedType.GetMembers())
        {
            if (member is not IFieldSymbol field) continue;
            if (field.IsStatic) continue;
            if (field.IsConst) continue;

            var fieldSize = GetFieldSize(field.Type);
            // Natural alignment: fields are aligned to their own size (capped at 8 for pointer-sized)
            var fieldAlignment = System.Math.Min(fieldSize, 8);
            if (fieldAlignment > 0)
            {
                // Round up currentOffset to the next multiple of fieldAlignment
                currentOffset = (currentOffset + fieldAlignment - 1) / fieldAlignment * fieldAlignment;
            }
            currentOffset += fieldSize;
            if (fieldAlignment > maxAlignment) maxAlignment = fieldAlignment;
        }

        // Round up total size to the largest field alignment (struct alignment)
        if (maxAlignment > 0)
        {
            currentOffset = (currentOffset + maxAlignment - 1) / maxAlignment * maxAlignment;
        }

        return currentOffset;
    }

    private static int GetFieldSize(ITypeSymbol type)
    {
        // Reference types are pointer-sized
        if (!type.IsValueType) return 8;

        return type.SpecialType switch
        {
            SpecialType.System_Boolean => 1,
            SpecialType.System_Byte => 1,
            SpecialType.System_SByte => 1,
            SpecialType.System_Char => 2,
            SpecialType.System_Int16 => 2,
            SpecialType.System_UInt16 => 2,
            SpecialType.System_Int32 => 4,
            SpecialType.System_UInt32 => 4,
            SpecialType.System_Single => 4,
            SpecialType.System_Int64 => 8,
            SpecialType.System_UInt64 => 8,
            SpecialType.System_Double => 8,
            SpecialType.System_Decimal => 16,
            SpecialType.System_DateTime => 8,
            _ => type.IsValueType ? EstimateStructSize(type) : 8,
        };
    }
}
