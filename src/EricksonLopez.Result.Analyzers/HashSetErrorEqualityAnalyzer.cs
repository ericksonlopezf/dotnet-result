// Copyright © Erickson Lopez. MIT License.
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using EricksonLopez.Result;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace EricksonLopez.Result.Analyzers;

/// <summary>
/// Roslyn diagnostic analyzer that warns when:
/// <list type="bullet">
///   <item><c>HashSet&lt;Error&gt;</c> or <c>Dictionary&lt;Error, TValue&gt;</c> is instantiated without
///   <c>ErrorEqualityComparer.Strict</c> (or <c>ErrorEqualityComparer.Default</c> if intentional).</item>
///   <item>LINQ <c>Enumerable.Distinct()</c>, <c>Enumerable.ToHashSet()</c>
///   is called without <c>ErrorEqualityComparer.Strict</c> on an <c>IEnumerable&lt;Error&gt;</c>.</item>
///   <item>LINQ <c>Enumerable.DistinctBy()</c>, <c>Enumerable.GroupBy()</c>
///   use <c>Error</c> as the source type without an explicit strict comparer.</item>
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
/// deduplicate them - losing one of the errors.
/// </para>
/// <para>
/// <b>Fix:</b> Use <c>ErrorEqualityComparer.Strict</c> to include all fields in equality checks,
/// or use <c>ErrorEqualityComparer.Default</c> if semantic deduplication is intentional.
/// </para>
/// </remarks>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class HashSetErrorEqualityAnalyzer : DiagnosticAnalyzer
{
    /// <summary>The diagnostic identifier for this analyzer rule.</summary>
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
        messageFormat: "'{0}' uses Error equality without an explicit comparer. Error.Equals is shallow (semantic fields only) and ignores TraceId and Metadata. Use ErrorEqualityComparer.Strict to prevent silent deduplication, or ErrorEqualityComparer.Default if semantic deduplication is intentional.",
        category: "Reliability",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description:
            "Error.Equals() only compares Code, Description, Type, Severity, and Retryability. " +
            "It intentionally ignores TraceId, CorrelationId, and Metadata. When errors are stored in a " +
            "HashSet<Error>, used as Dictionary keys, or processed with LINQ Distinct/GroupBy/ToHashSet, " +
            "errors with identical codes but different TraceIds will be silently deduplicated. " +
            "Pass ErrorEqualityComparer.Strict to compare all fields, or ErrorEqualityComparer.Default " +
            "if semantic deduplication is explicitly desired.",
        helpLinkUri: "https://github.com/ericksonlopezf/dotnet-result/blob/main/docs/analyzers.md#RESULT007");

    /// <inheritdoc/>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        => ImmutableArray.Create(Rule);

    /// <inheritdoc/>
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
        if (objectCreation.Type is not INamedTypeSymbol type) return;

        bool isHashSet = type.OriginalDefinition.MetadataName == HashSetMetadataName;
        bool isDictionary = type.OriginalDefinition.MetadataName == DictionaryMetadataName;

        if (!isHashSet && !isDictionary) return;

        // For HashSet<T>, the Error type is at index 0. For Dictionary<K,V>, it's at index 0 (the key).
        if (type.TypeArguments[0].ToDisplayString() != ErrorFullName) return;

        // Check if any argument passed to the constructor is an IEqualityComparer<Error>
        bool hasComparer = objectCreation.Arguments.Any(arg =>
            arg.Parameter!.Type is INamedTypeSymbol paramType &&
            paramType.OriginalDefinition.MetadataName == "IEqualityComparer`1" &&
            arg.ArgumentKind == ArgumentKind.Explicit &&
            (!arg.Value.ConstantValue.HasValue || arg.Value.ConstantValue.Value is not null));

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
        if (method.ContainingNamespace.ToDisplayString() != "System.Linq")
            return;

        if (method.TypeArguments[0].ToDisplayString() != ErrorFullName)
            return;

        bool hasComparer = invocation.Arguments.Any(arg =>
            arg.Parameter!.Type is INamedTypeSymbol paramType &&
            paramType.OriginalDefinition.MetadataName == "IEqualityComparer`1" &&
            arg.ArgumentKind == ArgumentKind.Explicit &&
            (!arg.Value.ConstantValue.HasValue || arg.Value.ConstantValue.Value is not null));

        if (!hasComparer)
        {
            var diagnostic = Diagnostic.Create(Rule, invocation.Syntax.GetLocation(), $"{method.Name}<Error>");
            context.ReportDiagnostic(diagnostic);
        }
    }
}



