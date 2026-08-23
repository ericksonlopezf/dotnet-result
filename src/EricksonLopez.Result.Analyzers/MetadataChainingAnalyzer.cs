// Copyright © Erickson Lopez. MIT License.
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using EricksonLopez.Result;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace EricksonLopez.Result.Analyzers;

/// <summary>
/// Roslyn diagnostic analyzer that warns when <c>Error.WithMetadata(string, object)</c>
/// or <c>ErrorBuilder.WithMetadata(string, object?)</c> is chained 3 or more times
/// consecutively, creating multiple intermediate allocations.
/// </summary>
/// <remarks>
/// <para>
/// Rule RESULT005 covers two related patterns:
/// <list type="bullet">
///   <item>
///     <b>Error.WithMetadata chaining:</b> Each call creates a <em>new</em> <c>Error</c>
///     heap object. Chaining 3+ calls allocates N intermediate <c>Error</c> instances
///     that are immediately discarded.
///   </item>
///   <item>
///     <b>ErrorBuilder.WithMetadata chaining:</b> Each single-key call performs an
///     O(log k) AVL-tree mutation on the backing <c>ImmutableDictionary</c>, creating
///     N intermediate dictionary nodes. Using the batch overload
///     <c>WithMetadata(IEnumerable{KeyValuePair{string, object}})</c> or
///     <c>WithMetadata(IReadOnlyDictionary{string, object?})</c> applies all entries
///     with a single <c>AddRange</c> call.
///   </item>
/// </list>
/// </para>
/// <para>
/// Example for Error (3+ chained calls warned):
/// <code>
/// var error = Error.NotFound("code", "desc")
///     .WithMetadata("a", 1)
///     .WithMetadata("b", 2)
///     .WithMetadata("c", 3); // 3 intermediate Error copies created and discarded
/// </code>
/// Recommended -- single allocation:
/// <code>
/// var error = Error.NotFound("code", "desc")
///     .WithMetadata(new Dictionary{string, object?} { ["a"] = 1, ["b"] = 2, ["c"] = 3 });
/// </code>
/// </para>
/// <para>
/// Example for ErrorBuilder (3+ chained calls warned):
/// <code>
/// var error = Error.Create("code", "desc")
///     .WithMetadata("a", 1)   // each call mutates ImmutableDictionary independently
///     .WithMetadata("b", 2)
///     .WithMetadata("c", 3)
///     .Build();
/// </code>
/// Recommended -- single batch call:
/// <code>
/// var error = Error.Create("code", "desc")
///     .WithMetadata(new Dictionary{string, object?} { ["a"] = 1, ["b"] = 2, ["c"] = 3 })
///     .Build();
/// </code>
/// </para>
/// </remarks>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class MetadataChainingAnalyzer : DiagnosticAnalyzer
{
    /// <summary>The diagnostic identifier for this analyzer rule.</summary>
    public const string DiagnosticId = "RESULT005";

    private const string ErrorFullName = "EricksonLopez.Result.Error";
    private const string ErrorBuilderFullName = "EricksonLopez.Result.ErrorBuilder";
    private const string WithMetadataMethodName = "WithMetadata";

    // Warn when 3 or more WithMetadata(string, object) calls are chained.
    // 1 or 2 calls are acceptable (common case); 3+ indicates a loop-worthy pattern.
    private const int ChainLengthThreshold = 3;

    private static readonly DiagnosticDescriptor RuleError = new(
        id: DiagnosticId,
        title: "Chained Error.WithMetadata() calls create multiple Error copies",
        messageFormat: "{0} chained Error.WithMetadata() calls create {0} intermediate Error heap copies; use WithMetadata(IReadOnlyDictionary<string, object?>) or ToBuilder() to apply all entries in a single allocation",
        category: "Performance",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description:
            "Error is an immutable class. Each Error.WithMetadata(string, object) call returns a new Error " +
            "with the entry added, creating N intermediate copies for N chained calls. When 3 or more " +
            "WithMetadata calls are chained, use the batch overload WithMetadata(IReadOnlyDictionary<string, object?>) " +
            "or ToBuilder() to apply all entries efficiently in a single allocation.",
        helpLinkUri: "https://github.com/ericksonlopezf/dotnet-result/blob/main/docs/error-builder.md#batch-metadata");

    private static readonly DiagnosticDescriptor RuleBuilder = new(
        id: DiagnosticId,
        title: "Chained ErrorBuilder.WithMetadata() calls create multiple ImmutableDictionary mutations",
        messageFormat: "{0} chained ErrorBuilder.WithMetadata() calls each perform an O(log k) AVL-tree mutation; use WithMetadata(IReadOnlyDictionary<string, object?>) or WithMetadata(IEnumerable<KeyValuePair<string, object>>) to batch all entries in a single AddRange call",
        category: "Performance",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description:
            "ErrorBuilder is a readonly struct. Each ErrorBuilder.WithMetadata(string, object?) call performs " +
            "an O(log k) mutation on the backing ImmutableDictionary, creating N intermediate dictionary nodes " +
            "for N chained calls. When 3 or more WithMetadata calls are chained, use the batch overload " +
            "WithMetadata(IReadOnlyDictionary<string, object?>) or WithMetadata(IEnumerable<KeyValuePair<string, object>>) " +
            "to apply all entries with a single AddRange call.",
        helpLinkUri: "https://github.com/ericksonlopezf/dotnet-result/blob/main/docs/error-builder.md#batch-metadata");

    /// <inheritdoc/>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        => ImmutableArray.Create(RuleError, RuleBuilder);

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterOperationAction(AnalyzeInvocation, OperationKind.Invocation);
    }

    private void AnalyzeInvocation(OperationAnalysisContext context)
    {
        var invocation = (IInvocationOperation)context.Operation;
        var method = invocation.TargetMethod;

        var containingTypeName = method.ContainingType.ToDisplayString();
        bool isErrorMethod = containingTypeName == ErrorFullName;
        bool isBuilderMethod = containingTypeName == ErrorBuilderFullName;

        if (!isErrorMethod && !isBuilderMethod) return;

        if (!string.Equals(method.Name, WithMetadataMethodName, StringComparison.Ordinal) ||
            method.Parameters.Length != 2)
        {
            return;
        }

        // Walk up the chain to count consecutive WithMetadata(string, object) calls.
        // We only report on the outermost call of a chain that meets the threshold.
        // Check if the parent is also a WithMetadata call on the same containing type --
        // if so, this is an inner call in the chain; the outermost call will do the reporting.
        if (IsInnerCallInChain(invocation, containingTypeName)) return;

        // Count the chain length starting from this (the outermost) call
        int chainLength = CountChainLength(invocation, containingTypeName);

        if (chainLength < ChainLengthThreshold) return;

        var rule = isErrorMethod ? RuleError : RuleBuilder;
        var diagnostic = Diagnostic.Create(
            rule,
            invocation.Syntax.GetLocation(),
            chainLength);
        context.ReportDiagnostic(diagnostic);
    }

    /// <summary>
    /// Returns true if this invocation is an inner call in a chain (i.e., its result is
    /// consumed by another WithMetadata call on the same type), meaning the outermost call
    /// will do the reporting.
    /// </summary>
    private static bool IsInnerCallInChain(IInvocationOperation invocation, string containingTypeName)
    {
        // Check if this invocation is the receiver of a parent WithMetadata call on the same type.
        var parent = invocation.Parent;
        if (parent is IInvocationOperation parentInvocation
            && string.Equals(parentInvocation.TargetMethod.Name, WithMetadataMethodName, StringComparison.Ordinal)
            && parentInvocation.TargetMethod.Parameters.Length == 2
            && parentInvocation.TargetMethod.ContainingType.ToDisplayString() == containingTypeName)
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// Counts the total length of a consecutive WithMetadata(string, object) chain
    /// starting from the outermost call and walking inward through the receiver chain.
    /// Only counts calls on the same containing type (Error or ErrorBuilder).
    /// </summary>
    private static int CountChainLength(IInvocationOperation outermost, string containingTypeName)
    {
        int count = 1;
        var current = outermost;

        while (true)
        {
            // The receiver (Instance) of this WithMetadata call -- walk into it
            var receiver = current.Instance;
            if (receiver is IInvocationOperation receiverInvocation
                && string.Equals(receiverInvocation.TargetMethod.Name, WithMetadataMethodName, StringComparison.Ordinal)
                && receiverInvocation.TargetMethod.Parameters.Length == 2
                && receiverInvocation.TargetMethod.ContainingType.ToDisplayString() == containingTypeName)
            {
                count++;
                current = receiverInvocation;
            }
            else
            {
                break;
            }
        }

        return count;
    }
}


