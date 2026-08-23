// Copyright © Erickson Lopez. MIT License.
using System;
using System.Collections.Immutable;
using System.Linq;
using EricksonLopez.Result;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace EricksonLopez.Result.Analyzers;

/// <summary>
/// Roslyn diagnostic analyzer (RESULT_OTEL_001) that reports an informational hint when
/// <c>TraceOutcome</c>, <c>TraceOnFailure</c>, or <c>TraceOnSuccess</c> are called without
/// the optional <c>metrics</c> parameter, meaning no <c>ResultMetrics</c> instance will record
/// metrics for this trace call.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class TraceOutcomeWithoutMetricsAnalyzer : DiagnosticAnalyzer
{
    /// <summary>The diagnostic identifier for this analyzer rule.</summary>
    public const string DiagnosticId = "RESULT_OTEL_001";

    private static readonly DiagnosticDescriptor Rule = new(
        id: DiagnosticId,
        title: "TraceOutcome/TraceOnFailure/TraceOnSuccess called without metrics instance",
        messageFormat: "'{0}' is called without a 'metrics' argument — no ResultMetrics counters will be recorded. " +
                       "Pass your DI-injected ResultMetrics instance via the 'metrics' parameter, or suppress this " +
                       "hint if using static mode (ResultMetrics.StaticTrackSuccess/StaticTrackFailure) separately.",
        category: "EricksonLopez.Result.OpenTelemetry",
        defaultSeverity: DiagnosticSeverity.Info,
        isEnabledByDefault: true,
        description: "When TraceOutcome, TraceOnFailure, or TraceOnSuccess are called without the 'metrics' parameter, " +
                     "only the Activity is annotated — no metrics counters are incremented. " +
                     "If you use services.AddResultMetrics() (DI mode), you must pass the injected ResultMetrics " +
                     "instance via the 'metrics' parameter to record metrics. " +
                     "If you use static mode (ResultMetrics.StaticTrack*), suppress this hint.",
        helpLinkUri: "https://github.com/ericksonlopezf/dotnet-result/blob/main/docs/analyzers.md#RESULT_OTEL_001");

    /// <inheritdoc/>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        => ImmutableArray.Create(Rule);

    private static readonly ImmutableHashSet<string> TargetMethodNames = ImmutableHashSet.Create(
        StringComparer.Ordinal,
        "TraceOutcome",
        "TraceOnFailure",
        "TraceOnSuccess");

    private const string ExtensionClassName = "ResultActivityExtensions";
    private const string ExtensionNamespace = "EricksonLopez.Result.OpenTelemetry";

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterOperationAction(AnalyzeInvocation, OperationKind.Invocation);
    }

    private static void AnalyzeInvocation(OperationAnalysisContext context)
    {
        var invocation = (IInvocationOperation)context.Operation;
        var method = invocation.TargetMethod;

        if (!TargetMethodNames.Contains(method.Name))
            return;

        var containingType = method.ContainingType;
        if (!string.Equals(containingType.Name, ExtensionClassName, StringComparison.Ordinal) ||
            !string.Equals(containingType.ContainingNamespace!.ToDisplayString(), ExtensionNamespace, StringComparison.Ordinal))
        {
            return;
        }

        var metricsProvided = invocation.Arguments.Any(arg =>
            arg.ArgumentKind == ArgumentKind.Explicit &&
            string.Equals(arg.Parameter!.Name, "metrics", StringComparison.Ordinal) &&
            arg.Value.ConstantValue is not { HasValue: true, Value: null });

        if (!metricsProvided)
        {
            var diagnostic = Diagnostic.Create(Rule, invocation.Syntax.GetLocation(), method.Name);
            context.ReportDiagnostic(diagnostic);
        }
    }
}

