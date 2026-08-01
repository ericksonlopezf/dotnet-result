using System.Diagnostics;

namespace EricksonLopez.Result.OpenTelemetry;

/// <summary>
/// Extension methods for recording <see cref="Result"/> and <see cref="Result{T}"/> outcomes as OpenTelemetry activities.
/// </summary>
/// <remarks>
/// Tag names follow the OpenTelemetry Semantic Conventions.
/// Standard attribute <c>error.type</c> is used per the semconv specification.
/// Library-specific attributes use the <c>ericksonlopez.result.*</c> namespace to avoid future conflicts.
/// See https://opentelemetry.io/docs/specs/semconv/attributes-registry/error/
/// <para>
/// <b>Design Note — Activity annotation vs. span creation:</b> These extension methods annotate the
/// <em>existing</em> ambient <see cref="Activity.Current"/> (or an explicitly-provided activity) rather
/// than creating new child spans. This means your application code is responsible for starting a parent
/// activity (e.g., via ASP.NET Core request middleware or a manually-created <see cref="ActivitySource.StartActivity"/>).
/// This is intentional — the Result library should not create opinionated span boundaries; it should enrich
/// whatever span the application has already decided to create.
/// </para>
/// <para>
/// If you want the <see cref="ResultSource"/> to be visible in your tracing backend, call
/// <c>tracerProviderBuilder.AddSource(<see cref="ActivitySourceName"/>)</c> during OTel setup.
/// </para>
/// <para>
/// <b>⚠️ DOUBLE-COUNTING WARNING:</b> If you register <see cref="ResultMetrics"/> via DI
/// (using <c>services.AddResultMetrics()</c>), you <b>MUST</b> pass the DI-resolved instance
/// to the <c>metrics</c> parameter of <c>TraceOutcome</c>, <c>TraceOnFailure</c>, and
/// <c>TraceOnSuccess</c>. If you call these methods without the <c>metrics</c> parameter,
/// no DI metrics will be recorded. However, if you ALSO call <see cref="ResultMetrics.StaticTrackSuccess"/>
/// or <see cref="ResultMetrics.RecordFailure"/> separately (static mode), both the static and DI meters
/// will emit events, resulting in double-counted metrics. Choose one mode:
/// <list type="bullet">
///   <item><b>DI mode:</b> Use <c>services.AddResultMetrics()</c> + pass the instance via the <c>metrics</c> parameter.</item>
///   <item><b>Static mode:</b> Call <c>ResultMetrics.StaticTrackSuccess()</c>/<c>StaticTrackFailure()</c> directly without DI.</item>
/// </list>
/// </para>
/// </remarks>

public static class ResultActivityExtensions
{
    /// <summary>
    /// The name of the ActivitySource used for tracing Result outcomes.
    /// Register this source with your OpenTelemetry TracerProvider:
    /// <code>tracerProviderBuilder.AddSource(ResultActivityExtensions.ActivitySourceName);</code>
    /// </summary>
    public const string ActivitySourceName = "EricksonLopez.Result";

    /// <summary>
    /// The <see cref="ActivitySource"/> for creating Result-related spans.
    /// Primarily used to allow OpenTelemetry to subscribe to this source via
    /// <c>tracerProviderBuilder.AddSource(<see cref="ActivitySourceName"/>)</c>.
    /// Application code annotates the existing <see cref="Activity.Current"/> via the
    /// <c>TraceOutcome</c> / <c>TraceOnFailure</c> / <c>TraceOnSuccess</c> extension methods.
    /// </summary>
    public static readonly ActivitySource ResultSource;

    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    static ResultActivityExtensions()
    {
        ResultSource = new(ActivitySourceName, ResultMetrics.AssemblyVersion);
    }


    private const string AttrErrorType = "error.type";

    /// <summary>
    /// Tag key for the application-specific error code string.
    /// </summary>
    /// <remarks>
    /// <b>⚠️ CARDINALITY WARNING:</b> This tag uses the application's custom error code string as a metric dimension.
    /// If your error codes contain high-cardinality values (e.g., user IDs, request IDs, record IDs,
    /// timestamps, or free-form strings), this will cause a <em>metrics explosion</em> in backends such as
    /// Prometheus, OTLP collectors, and Azure Monitor — potentially thousands of distinct time series per error type.<br/>
    /// <br/>
    /// <b>Recommendation:</b> Ensure your error codes are bounded enumerations with a finite, low-cardinality
    /// set (e.g., <c>"VALIDATION_ERROR"</c>, <c>"NOT_FOUND"</c>, <c>"PAYMENT_DECLINED"</c>). Never include
    /// request-scoped identifiers in error codes. Error codes should be static, shared vocabulary across all
    /// invocations — not per-request identifiers. See the OTel guidance on
    /// <a href="https://opentelemetry.io/docs/specs/semconv/general/attribute-naming/#cardinality">tag cardinality</a>.
    /// </remarks>
    private const string AttrErrorCode = "ericksonlopez.result.error.code";
    private const string AttrErrorSeverity = "ericksonlopez.result.error.severity";
    /// <summary>Standard OTel semantic convention attribute for the operation name.</summary>
    /// <remarks>
    /// Prefixed with <c>ericksonlopez.result.*</c> to avoid conflicts with future OTel semconv additions.
    /// The generic <c>operation.name</c> attribute is not a registered OTel semantic convention for
    /// general application operations; library-specific attributes must use a namespace prefix.
    /// See https://opentelemetry.io/docs/specs/semconv/general/attribute-naming/
    /// </remarks>
    private const string AttrOperationName = "ericksonlopez.result.operation.name";
    private const string AttrOutcome = "ericksonlopez.result.outcome";

    // ─── Result (non-generic) ─────────────────────────────────────────────────

    /// <summary>
    /// Records the result outcome (success or failure) to an Activity and updates metrics.
    /// For failure, sets activity status to Error and records error attributes.
    /// For success, sets activity status to Ok and records success metrics.
    /// </summary>
    /// <param name="result">The result to trace.</param>
    /// <param name="operationName">The name of the operation being traced.</param>
    /// <param name="targetActivity">The activity to record on. Defaults to <see cref="Activity.Current"/> if null.</param>
    /// <param name="metrics">
    /// Optional <see cref="ResultMetrics"/> instance for recording metrics. When provided, records
    /// success or failure using the instance meter. When <see langword="null"/>, no metrics are recorded.
    /// To use the static (non-DI) meter, call <see cref="ResultMetrics.StaticTrackSuccess"/> /
    /// <see cref="ResultMetrics.RecordFailure"/> explicitly after this method.
    /// </param>
    public static Result TraceOutcome(this in Result result, string operationName, Activity? targetActivity = null, ResultMetrics? metrics = null)
    {
        var activity = targetActivity ?? Activity.Current;

        if (result.IsFailure)
        {
            var errorType = ErrorTypeToOTelString(result.Error.Type);
            activity?.SetStatus(ActivityStatusCode.Error, result.Error.Description);
            activity?.SetTag(AttrErrorType, errorType);
            activity?.SetTag(AttrErrorCode, result.Error.Code);
            activity?.SetTag(AttrErrorSeverity, ErrorSeverityToString(result.Error.Severity));
            activity?.SetTag(AttrOperationName, operationName);
            activity?.SetTag(AttrOutcome, "failure");

            metrics?.TrackFailure(operationName, result.Error.Code, errorType);
        }
        else
        {
            activity?.SetStatus(ActivityStatusCode.Ok);
            activity?.SetTag(AttrOperationName, operationName);
            activity?.SetTag(AttrOutcome, "success");

            metrics?.TrackSuccess(operationName);
        }

        return result;
    }

    /// <summary>
    /// Records only a failure outcome to an Activity and updates failure metrics.
    /// If the result is successful, no recording occurs.
    /// </summary>
    /// <param name="result">The result to trace.</param>
    /// <param name="operationName">The name of the operation being traced.</param>
    /// <param name="targetActivity">The activity to record on. Defaults to <see cref="Activity.Current"/> if null.</param>
    /// <param name="metrics">
    /// Optional <see cref="ResultMetrics"/> instance for recording metrics. When provided, records
    /// failure using the instance meter. When <see langword="null"/>, <b>no metrics are recorded</b>
    /// — only the activity tags are set.
    /// <para>
    /// <b>⚠️ DI mode:</b> If you use <c>services.AddResultMetrics()</c>, you MUST pass the injected
    /// <see cref="ResultMetrics"/> instance here, otherwise metrics will be silently omitted:
    /// <code>
    /// // With DI:
    /// result.TraceOnFailure("MyOperation", metrics: _metrics);
    ///
    /// // Without DI (static mode):
    /// result.TraceOnFailure("MyOperation");
    /// ResultMetrics.StaticTrackFailure("MyOperation", error.Code, errorType);
    /// </code>
    /// </para>
    /// </param>
    public static Result TraceOnFailure(this in Result result, string operationName, Activity? targetActivity = null, ResultMetrics? metrics = null)
    {
        if (!result.IsFailure) return result;

        var activity = targetActivity ?? Activity.Current;
        var errorType = ErrorTypeToOTelString(result.Error.Type);
        activity?.SetStatus(ActivityStatusCode.Error, result.Error.Description);
        activity?.SetTag(AttrErrorType, errorType);
        activity?.SetTag(AttrErrorCode, result.Error.Code);
        activity?.SetTag(AttrErrorSeverity, ErrorSeverityToString(result.Error.Severity));
        activity?.SetTag(AttrOperationName, operationName);
        activity?.SetTag(AttrOutcome, "failure");

        metrics?.TrackFailure(operationName, result.Error.Code, errorType);

        return result;
    }

    /// <summary>
    /// Records only a success outcome to an Activity and updates success metrics.
    /// If the result is a failure, no recording occurs.
    /// </summary>
    /// <param name="result">The result to trace.</param>
    /// <param name="operationName">The name of the operation being traced.</param>
    /// <param name="targetActivity">The activity to record on. Defaults to <see cref="Activity.Current"/> if null.</param>
    /// <param name="metrics">
    /// Optional <see cref="ResultMetrics"/> instance for recording metrics. When provided, records
    /// success using the instance meter. When <see langword="null"/>, <b>no metrics are recorded</b>
    /// — only the activity tags are set.
    /// <para>
    /// <b>⚠️ DI mode:</b> If you use <c>services.AddResultMetrics()</c>, you MUST pass the injected
    /// <see cref="ResultMetrics"/> instance here, otherwise <b>success metrics will be silently omitted</b>:
    /// <code>
    /// // With DI — metrics ARE recorded:
    /// result.TraceOnSuccess("MyOperation", metrics: _metrics);
    ///
    /// // Without metrics param — metrics are NOT recorded (activity tags only):
    /// result.TraceOnSuccess("MyOperation");
    /// </code>
    /// </para>
    /// <para>
    /// Prefer <see cref="TraceOutcome(in Result, string, Activity?, ResultMetrics?)"/> when you want
    /// both success and failure to be recorded in a single call.
    /// </para>
    /// </param>
    public static Result TraceOnSuccess(this in Result result, string operationName, Activity? targetActivity = null, ResultMetrics? metrics = null)
    {
        if (!result.IsSuccess) return result;

        var activity = targetActivity ?? Activity.Current;
        activity?.SetStatus(ActivityStatusCode.Ok);
        activity?.SetTag(AttrOperationName, operationName);
        activity?.SetTag(AttrOutcome, "success");

        metrics?.TrackSuccess(operationName);

        return result;
    }

    // ─── Result<T> (generic) ────────────────────────────────────────────────

    /// <summary>
    /// Records the result outcome (success or failure) to an Activity and updates metrics.
    /// For failure, sets activity status to Error and records error attributes.
    /// For success, sets activity status to Ok and records success metrics.
    /// </summary>
    /// <param name="result">The result to trace.</param>
    /// <param name="operationName">The name of the operation being traced.</param>
    /// <param name="targetActivity">The activity to record on. Defaults to <see cref="Activity.Current"/> if null.</param>
    /// <param name="metrics">
    /// Optional <see cref="ResultMetrics"/> instance for recording metrics. When provided, records
    /// success or failure using the instance meter. When <see langword="null"/>, <b>no metrics are recorded</b>
    /// — only the activity tags are set.
    /// <para>
    /// <b>⚠️ DI mode:</b> If you use <c>services.AddResultMetrics()</c>, you MUST pass the injected
    /// <see cref="ResultMetrics"/> instance here, otherwise metrics will be silently omitted:
    /// <code>
    /// // With DI — both tracing and metrics are recorded:
    /// result.TraceOutcome("MyOperation", metrics: _metrics);
    ///
    /// // Without metrics param — only activity tags are set, metrics are NOT recorded:
    /// result.TraceOutcome("MyOperation");
    /// </code>
    /// </para>
    /// </param>
    public static Result<T> TraceOutcome<T>(this in Result<T> result, string operationName, Activity? targetActivity = null, ResultMetrics? metrics = null)
    {
        var activity = targetActivity ?? Activity.Current;

        if (result.IsFailure)
        {
            var errorType = ErrorTypeToOTelString(result.Error.Type);
            activity?.SetStatus(ActivityStatusCode.Error, result.Error.Description);
            activity?.SetTag(AttrErrorType, errorType);
            activity?.SetTag(AttrErrorCode, result.Error.Code);
            activity?.SetTag(AttrErrorSeverity, ErrorSeverityToString(result.Error.Severity));
            activity?.SetTag(AttrOperationName, operationName);
            activity?.SetTag(AttrOutcome, "failure");

            metrics?.TrackFailure(operationName, result.Error.Code, errorType);
        }
        else
        {
            activity?.SetStatus(ActivityStatusCode.Ok);
            activity?.SetTag(AttrOperationName, operationName);
            activity?.SetTag(AttrOutcome, "success");

            metrics?.TrackSuccess(operationName);
        }

        return result;
    }

    /// <summary>
    /// Records only a failure outcome to an Activity and updates failure metrics.
    /// If the result is successful, no recording occurs.
    /// </summary>
    /// <param name="result">The result to trace.</param>
    /// <param name="operationName">The name of the operation being traced.</param>
    /// <param name="targetActivity">The activity to record on. Defaults to <see cref="Activity.Current"/> if null.</param>
    /// <param name="metrics">
    /// Optional <see cref="ResultMetrics"/> instance for recording metrics. When provided, records
    /// failure using the instance meter. When <see langword="null"/>, no metrics are recorded.
    /// </param>
    public static Result<T> TraceOnFailure<T>(this in Result<T> result, string operationName, Activity? targetActivity = null, ResultMetrics? metrics = null)
    {
        if (!result.IsFailure) return result;

        var activity = targetActivity ?? Activity.Current;
        var errorType = ErrorTypeToOTelString(result.Error.Type);
        activity?.SetStatus(ActivityStatusCode.Error, result.Error.Description);
        activity?.SetTag(AttrErrorType, errorType);
        activity?.SetTag(AttrErrorCode, result.Error.Code);
        activity?.SetTag(AttrErrorSeverity, ErrorSeverityToString(result.Error.Severity));
        activity?.SetTag(AttrOperationName, operationName);
        activity?.SetTag(AttrOutcome, "failure");

        metrics?.TrackFailure(operationName, result.Error.Code, errorType);

        return result;
    }

    /// <summary>
    /// Records only a success outcome to an Activity and updates success metrics.
    /// If the result is a failure, no recording occurs.
    /// </summary>
    /// <param name="result">The result to trace.</param>
    /// <param name="operationName">The name of the operation being traced.</param>
    /// <param name="targetActivity">The activity to record on. Defaults to <see cref="Activity.Current"/> if null.</param>
    /// <param name="metrics">
    /// Optional <see cref="ResultMetrics"/> instance for recording metrics. When provided, records
    /// success using the instance meter. When <see langword="null"/>, no metrics are recorded.
    /// </param>
    public static Result<T> TraceOnSuccess<T>(this in Result<T> result, string operationName, Activity? targetActivity = null, ResultMetrics? metrics = null)
    {
        if (!result.IsSuccess) return result;

        var activity = targetActivity ?? Activity.Current;
        activity?.SetStatus(ActivityStatusCode.Ok);
        activity?.SetTag(AttrOperationName, operationName);
        activity?.SetTag(AttrOutcome, "success");

        metrics?.TrackSuccess(operationName);

        return result;
    }

    // ─── OTel Semantic Convention string helpers — delegates to shared ErrorEnumStrings ──

    /// <summary>
    /// Returns the OTel <c>error.type</c>-compatible string for an <see cref="ErrorType"/>.
    /// Follows the naming convention where error types are lowercase dot-separated strings.
    /// </summary>
    internal static string ErrorTypeToOTelString(ErrorType type)
        => ErrorEnumStrings.ErrorTypeToOTelString(type);

    private static string ErrorSeverityToString(ErrorSeverity severity)
        => ErrorEnumStrings.ErrorSeverityToOTelString(severity);
}




