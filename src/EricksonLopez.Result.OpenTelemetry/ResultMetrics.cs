// Copyright © Erickson Lopez. MIT License.
using System;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Threading;
using EricksonLopez.Result;

namespace EricksonLopez.Result.OpenTelemetry;

/// <summary>
/// Provides OpenTelemetry metrics instrumentation for Result outcomes.
/// </summary>
/// <remarks>
/// <para>
/// This class has two usage modes:
/// <list type="number">
///   <item><b>DI mode (recommended)</b>: Construct with a <see cref="Meter"/> provided by your DI container
///   (e.g., via <c>IMeterFactory</c> in Microsoft.Extensions.Diagnostics). The caller is responsible for
///   disposing the meter. Call <c>services.AddResultMetrics()</c> for automatic registration.</item>
///   <item><b>Static mode</b>: Call <see cref="StaticTrackSuccess"/> / <see cref="StaticTrackFailure"/> directly.
///   This uses an internal static <see cref="Meter"/> that is lazily initialized and never disposed.
///   Static mode is suitable for simple scenarios without DI.</item>
/// </list>
/// </para>
/// <para>
/// <b>Important</b>: Do NOT mix both modes simultaneously. If you use DI-managed <see cref="ResultMetrics"/>
/// and also call the <c>TraceOutcome</c> / <c>TraceOnFailure</c> / <c>TraceOnSuccess</c> extension methods
/// without passing the instance via the <c>metrics</c> parameter, both the static and DI meters will emit
/// events, resulting in double-counting. Always pass the DI instance to extension methods when using DI mode.
/// </para>
/// </remarks>
public sealed class ResultMetrics : IDisposable
{
    /// <summary>The OpenTelemetry Meter name for all Result metrics.</summary>
    public const string MeterName = "EricksonLopez.Result";

    /// <summary>
    /// The assembly version string baked in at compile time by a source generator.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Performance philosophy:</b> This value is provided as a compile-time constant emitted by
    /// <c>EricksonLopez.Result.Serialization.Generators.ResultMetricsVersionGenerator</c>, which runs
    /// during the build of this package and produces a <c>ResultMetricsVersionConstants.g.cs</c> file
    /// with a <c>const string Version</c> field.
    /// </para>
    /// <para>
    /// This eliminates the previous reflection-based reading of <see cref="System.Reflection.AssemblyInformationalVersionAttribute"/>,
    /// providing:
    /// <list type="bullet">
    ///   <item>Zero runtime overhead — the value is a compile-time constant inlined by the JIT</item>
    ///   <item>100% NativeAOT-safe — no reflection, no trimmer annotations required for version lookup</item>
    ///   <item>No fallback path — the version is always correct and always available</item>
    /// </list>
    /// </para>
    /// </remarks>
    internal static readonly string AssemblyVersion = ResultMetricsVersionConstants.Version;

    // ─── Static meter (for legacy/simple usage without DI) ────────────────────
    // ⚠️ Lifecycle note: _staticMeter is intentionally NEVER DISPOSED.
    // It is a process-singleton that is lazily initialized on first static-mode call and remains
    // alive for the entire process lifetime. This is the standard pattern for static/ambient
    // OpenTelemetry Meter instances (analogous to ActivitySource singletons). If you need
    // explicit lifecycle control, use DI mode instead: services.AddResultMetrics() registers
    // an instance whose Meter is provided by IMeterFactory and managed by the DI container.
    // See the class-level remarks for details on DI mode vs. static mode.
    private static readonly object StaticLock = new();
    private static Meter? _staticMeter;
    private static Counter<long>? _staticOperationsCounter;

    // 0 = uninitialized, 1 = static mode, 2 = DI mode
    private static int _initializationMode;

    // ——— Instance meter (for DI-managed lifecycle) —————————————————
    private readonly Counter<long> _operationsCounter;
    private readonly Meter? _ownedMeter;
    // volatile ensures the disposed flag is visible across threads on all architectures,
    // including ARM64 (Azure, Graviton, Apple Silicon) which has a weaker memory model
    // than x86-64 and can reorder plain non-volatile reads/writes. Without volatile,
    // a thread calling Dispose() concurrently with TrackSuccess() on a different thread
    // could observe stale values due to store-load reordering.
    private volatile bool _disposed;

    /// <summary>
    /// Initializes a new instance of <see cref="ResultMetrics"/> using an externally provided <see cref="Meter"/>.
    /// </summary>
    /// <param name="meter">The meter to create instruments on. Typically obtained from an <c>IMeterFactory</c> in DI.</param>
    /// <param name="ownsMeter">
    /// When <see langword="true"/> (default), this instance will dispose <paramref name="meter"/> when <see cref="Dispose"/> is called.
    /// Set to <see langword="true"/> when you create the meter yourself (e.g., <c>new Meter(...)</c>) and want
    /// <see cref="ResultMetrics"/> to manage its lifetime via <c>using var</c>.
    /// When <see langword="false"/>, the caller retains ownership of the meter.
    /// Leave <see langword="false"/> when the meter is provided by <c>IMeterFactory</c> in DI —  the factory
    /// manages the meter lifecycle automatically. See <c>AddResultMetrics()</c> for the DI registration.
    /// </param>
    public ResultMetrics(Meter meter, bool ownsMeter = true)
    {
        ArgumentNullException.ThrowIfNull(meter);

        if (!ownsMeter)
        {
            if (Interlocked.CompareExchange(ref _initializationMode, 2, 0) == 1)
            {
                throw new InvalidOperationException("Cannot initialize DI ResultMetrics because static mode is already active. Mixing both modes causes double-counting.");
            }
        }

        _operationsCounter = CreateOperationsCounter(meter);
        _ownedMeter = ownsMeter ? meter : null;
    }

    // ——— DI instance methods —————————————————

    /// <summary>Records a success outcome using the instance meter.</summary>
    /// <param name="operationName">The name of the operation being recorded.</param>
    /// <exception cref="ObjectDisposedException">This instance has been disposed</exception>
    public void TrackSuccess(string operationName)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        _operationsCounter.Add(1, new TagList
        {
            { "ericksonlopez.result.operation.name", operationName },
            { "ericksonlopez.result.outcome", "success" }
        });
    }

    /// <summary>Records a failure outcome using the instance meter.</summary>
    /// <param name="operationName">The name of the operation being recorded.</param>
    /// <param name="errorCode">The application error code associated with the failure.</param>
    /// <param name="errorType">The string representation of the error type.</param>
    /// <exception cref="ObjectDisposedException">This instance has been disposed</exception>
    /// <remarks>
    /// <b>⚠️ CARDINALITY WARNING:</b> The <paramref name="errorCode"/> parameter is recorded as a
    /// metric tag dimension (<c>ericksonlopez.result.error.code</c>). Ensure error codes are
    /// low-cardinality bounded enumerations, not per-request identifiers such as user IDs or record IDs.
    /// High-cardinality values will cause a metrics explosion in your backend (Prometheus, OTLP, Azure Monitor).
    /// </remarks>
    public void TrackFailure(string operationName, string errorCode, string errorType)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        _operationsCounter.Add(1, new TagList
        {
            { "ericksonlopez.result.operation.name", operationName },
            { "ericksonlopez.result.outcome", "failure" },
            { "error.type", errorType },
            { "ericksonlopez.result.error.code", errorCode }
        });
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        _disposed = true;
        // Note: A benign TOCTOU race exists where TrackSuccess/Failure might pass
        // the _disposed check and call Add() concurrently with or after _ownedMeter.Dispose().
        // In System.Diagnostics.Metrics, calling Add() on a counter from a disposed Meter
        // is safe (it acts as a no-op or is ignored by listeners). 
        // Dispose the meter only if this instance was created with ownsMeter: true.
        // When the meter is provided by IMeterFactory in DI, the factory manages
        // the meter lifecycle — do not dispose it here to avoid double-dispose.
        _ownedMeter?.Dispose();
    }

    // ——— Static methods (simple scenarios / backward compat) —————————————————

    /// <summary>
    /// Records a success outcome using the static (non-DI) meter.
    /// Consistent naming with the instance <see cref="TrackSuccess(string)"/> method.
    /// </summary>
    /// <param name="operationName">The name of the operation being recorded.</param>
    /// <exception cref="InvalidOperationException">DI mode is already active</exception>
    /// <remarks>
    /// <para>
    /// This is the primary API for scenarios without a DI container: console apps, AWS Lambda, Azure Functions v1,
    /// test harnesses, and other non-hosted environments. Uses a static meter that is lazily initialized and
    /// never disposed (standard pattern for ambient OpenTelemetry singletons).
    /// </para>
    /// <para>
    /// For applications using <c>Microsoft.Extensions.DependencyInjection</c>, prefer
    /// <c>services.AddResultMetrics()</c> and injecting the <see cref="ResultMetrics"/> instance
    /// to avoid double-counting when both static and DI modes are active simultaneously.
    /// </para>
    /// <para>
    /// <b>Testing:</b> When using static methods in unit tests, call
    /// <see cref="ResetStaticMeterForTesting"/> in your test cleanup (e.g., <c>Dispose</c> or
    /// <c>[AssemblyCleanup]</c>) to prevent meter state from leaking across test runs.
    /// </para>
    /// </remarks>
    public static void StaticTrackSuccess(string operationName)
    {
        EnsureStaticInstruments();
        _staticOperationsCounter!.Add(1, new TagList
        {
            { "ericksonlopez.result.operation.name", operationName },
            { "ericksonlopez.result.outcome", "success" }
        });
    }

    /// <summary>
    /// Records a failure outcome using the static (non-DI) meter.
    /// Consistent naming with the instance <see cref="TrackFailure(string, string, string)"/> method.
    /// </summary>
    /// <param name="operationName">The name of the operation being recorded.</param>
    /// <param name="errorCode">The application error code associated with the failure.</param>
    /// <param name="errorType">The string representation of the error type.</param>
    /// <exception cref="InvalidOperationException">DI mode is already active</exception>
    /// <remarks>
    /// <para>
    /// This is the primary API for scenarios without a DI container: console apps, AWS Lambda, Azure Functions v1,
    /// test harnesses, and other non-hosted environments.
    /// </para>
    /// <para>
    /// For applications using <c>Microsoft.Extensions.DependencyInjection</c>, prefer
    /// <c>services.AddResultMetrics()</c> and injecting the <see cref="ResultMetrics"/> instance
    /// to avoid double-counting when both static and DI modes are active simultaneously.
    /// </para>
    /// <para>
    /// <b>⚠️ CARDINALITY WARNING:</b> The <paramref name="errorCode"/> parameter is recorded as a
    /// metric tag dimension (<c>ericksonlopez.result.error.code</c>). If error codes are high-cardinality
    /// (e.g., contain user IDs, request IDs, or record IDs), your metrics backend (Prometheus, OTLP, Azure Monitor)
    /// will create a distinct time series for each unique value, potentially causing a metrics explosion.<br/>
    /// Ensure error codes are low-cardinality bounded enumerations (e.g., <c>"NOT_FOUND"</c>,
    /// <c>"VALIDATION_ERROR"</c>), not per-request identifiers.
    /// </para>
    /// <para>
    /// <b>Testing:</b> When using static methods in unit tests, call
    /// <see cref="ResetStaticMeterForTesting"/> in your test cleanup (e.g., <c>Dispose</c> or
    /// <c>[AssemblyCleanup]</c>) to prevent meter state from leaking across test runs.
    /// </para>
    /// </remarks>
    public static void StaticTrackFailure(string operationName, string errorCode, string errorType)
    {
        EnsureStaticInstruments();
        _staticOperationsCounter!.Add(1, new TagList
        {
            { "ericksonlopez.result.operation.name", operationName },
            { "ericksonlopez.result.outcome", "failure" },
            { "error.type", errorType },
            { "ericksonlopez.result.error.code", errorCode }
        });
    }

    private static void EnsureStaticInstruments()
    {
        if (_staticOperationsCounter is not null)
        {
            return;
        }

        lock (StaticLock)
        {
            if (_staticOperationsCounter is not null)
            {
                return;
            }

            if (Interlocked.CompareExchange(ref _initializationMode, 1, 0) == 2)
            {
                throw new InvalidOperationException("Cannot initialize static ResultMetrics because DI mode (services.AddResultMetrics) is already active. Mixing both modes causes double-counting.");
            }

            var meter = new Meter(MeterName, AssemblyVersion);
            var counter = CreateOperationsCounter(meter);

            _staticMeter = meter;
            _staticOperationsCounter = counter;
        }
    }

    private static Counter<long> CreateOperationsCounter(Meter meter) =>
        meter.CreateCounter<long>(
            "ericksonlopez.result.operations",
            unit: "{count}",
            description: "Number of Result outcomes (success and failure).");

    // ─── Testing support ──────────────────────────────────────────────────────

    /// <summary>
    /// Resets the static meter for testing purposes. This disposes the existing static meter
    /// and clears the instruments so the next call to <see cref="StaticTrackSuccess"/> or
    /// <see cref="StaticTrackFailure"/> will create fresh instruments.
    /// </summary>
    /// <remarks>
    /// This method is <c>internal</c> and intended only for test scenarios where tests need
    /// isolated static meter state. Access is granted via <c>InternalsVisibleTo</c>.
    /// <para>
    /// <b>Thread safety:</b> This method acquires the static lock. Do not call concurrently
    /// with <see cref="StaticTrackSuccess"/> or <see cref="StaticTrackFailure"/>.
    /// </para>
    /// </remarks>
    internal static void ResetStaticMeterForTesting()
    {
        lock (StaticLock)
        {
            _staticOperationsCounter = null;
            _staticMeter?.Dispose();
            _staticMeter = null;
            _initializationMode = 0;
        }
    }

    internal Meter? OwnedMeterForTesting => _ownedMeter;
    internal static Meter? StaticMeterForTesting => _staticMeter;
    internal static Counter<long>? StaticOperationsCounterForTesting => _staticOperationsCounter;
    internal static object StaticLockForTesting => StaticLock;
}






