using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace EricksonLopez.Result.Analyzers;

/// <summary>
/// Roslyn diagnostic analyzer (RESULT_OTEL_001) that reports an informational hint when
/// <c>TraceOutcome</c>, <c>TraceOnFailure</c>, or <c>TraceOnSuccess</c> are called without
/// the optional <c>metrics</c> parameter, meaning no <c>ResultMetrics</c> instance will record
/// metrics for this trace call.
/// </summary>
/// <remarks>
/// <para>
/// These three methods all have the signature pattern:
/// <code>
/// public static Result TraceOutcome(this in Result result, string operationName,
///     Activity? targetActivity = null, ResultMetrics? metrics = null)
/// </code>
/// When called without the <c>metrics</c> argument (or with <c>null</c>), only the Activity
/// is annotated — no metrics counters are incremented. This is intentional in static mode,
/// but is a common mistake when using DI-registered <c>ResultMetrics</c>.
/// </para>
/// <para>
/// Severity: <see cref="DiagnosticSeverity.Info"/> — this is a style/correctness hint,
/// not an error or warning. Users may suppress it if using static mode intentionally.
/// </para>
/// <para>
/// Example — no metrics will be recorded:
/// <code>
/// result.TraceOutcome("PlaceOrder"); // RESULT_OTEL_001
/// </code>
/// Correct — metrics are recorded via DI instance:
/// <code>
/// result.TraceOutcome("PlaceOrder", metrics: _metrics);
/// </code>
/// Correct — intentional static mode (suppress if needed):
/// <code>
/// result.TraceOutcome("PlaceOrder"); // metrics recorded separately via ResultMetrics.StaticTrackSuccess()
/// </code>
/// </para>
/// </remarks>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class TraceOutcomeWithoutMetricsAnalyzer : DiagnosticAnalyzer
{
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
        helpLinkUri: "https://github.com/ericksonlopez/dotnet-result/blob/main/docs/analyzers.md#RESULT_OTEL_001");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        => ImmutableArray.Create(Rule);

    // The set of method names to check (all extension methods on ResultActivityExtensions)
    private static readonly ImmutableHashSet<string> TargetMethodNames = ImmutableHashSet.Create(
        System.StringComparer.Ordinal,
        "TraceOutcome",
        "TraceOnFailure",
        "TraceOnSuccess");

    // Fully-qualified containing type for the extension class
    private const string ResultActivityExtensionsTypeName = "EricksonLopez.Result.OpenTelemetry.ResultActivityExtensions";

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

        // Quick name-based pre-filter to avoid heavy symbol resolution on every call
        if (!TargetMethodNames.Contains(method.Name))
            return;

        // Confirm the method belongs to the correct type
        var containingType = method.ContainingType;
        if (containingType is null)
            return;

        var typeName = containingType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)
                                     .Replace("global::", string.Empty);
        if (!string.Equals(typeName, ResultActivityExtensionsTypeName, System.StringComparison.Ordinal))
            return;

        // Check if the 'metrics' parameter was explicitly provided (not using default null)
        // We look for a named or positional argument bound to the 'metrics' parameter.
        bool metricsProvided = false;
        foreach (var arg in invocation.Arguments)
        {
            // ArgumentKind.Explicit means the user explicitly wrote the argument
            // ArgumentKind.DefaultValue means the user omitted it (using the default null)
            if (arg.Parameter is not null &&
                string.Equals(arg.Parameter.Name, "metrics", System.StringComparison.Ordinal) &&
                arg.ArgumentKind == ArgumentKind.Explicit)
            {
                // Even if explicitly provided, check if it's a constant null (including conversions)
                if (arg.Value.ConstantValue.HasValue && arg.Value.ConstantValue.Value is null)
                {
                    // Caller wrote metrics: null explicitly — treat as omitted
                    metricsProvided = false;
                }
                else
                {
                    metricsProvided = true;
                }
                break;
            }
        }

        if (!metricsProvided)
        {
            var diagnostic = Diagnostic.Create(Rule, invocation.Syntax.GetLocation(), method.Name);
            context.ReportDiagnostic(diagnostic);
        }
    }
}
