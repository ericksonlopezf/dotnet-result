// Copyright © Erickson Lopez. MIT License.
using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using EricksonLopez.Result;
using Microsoft.AspNetCore.Http;

namespace EricksonLopez.Result.AspNetCore;

/// <summary>
/// Configurable options for HTTP mapping of Result error types to HTTP Status Codes and ProblemDetails parameters.
/// </summary>
/// <remarks>
/// <para>
/// <b>Configuration</b>: Use <see cref="ConfigureStatusCode"/> and the scalar properties
/// (<see cref="DefaultSuccessStatusCode"/>, <see cref="IncludeDescription"/>, <see cref="IncludeTraceId"/>,
/// <see cref="DefaultFallbackDescription"/>, <see cref="TypeUriBase"/>) to configure behavior.
/// All configuration must be done before the first request is processed. Any mutation attempt after
/// the first request throws an <see cref="InvalidOperationException"/>.
/// </para>
/// <para>
/// <b>Thread safety</b>: Configure all options during application startup before any requests are handled.
/// After the first call to <see cref="GetFrozenStatusCodeMap"/>, the options are fully frozen and ALL
/// mutation attempts (dictionaries and scalar properties alike) throw <see cref="InvalidOperationException"/>.
/// </para>
/// <para>
/// <b>NativeAOT / Trimming warning</b>: When using <see cref="ResultEndpointFilter"/> with
/// <see cref="Result{T}"/>, the success value is returned as <c>object?</c> via
/// <see cref="IResultOutcome.RawValue"/>. ASP.NET Core will serialize this using the configured
/// <see cref="System.Text.Json.JsonSerializerOptions"/>. For NativeAOT compatibility, ensure the
/// concrete type <c>T</c> is registered in your <see cref="System.Text.Json.Serialization.JsonSerializerContext"/>.
/// </para>
/// <para>
/// <b>⚠️ Performance trade-off (boxing)</b>: <see cref="Result{T}"/> is a <c>readonly struct</c>.
/// When the endpoint filter checks <c>result is IResultOutcome</c>, the struct is boxed to the managed
/// heap for the duration of the polymorphic dispatch. This is a single allocation per request on the
/// failure path and is negligible in typical web workloads. If extreme hot-path allocation avoidance
/// is required, call <see cref="ResultHttpExtensions.ToHttpResult{T}"/> directly from your handler
/// instead of using the filter.
/// </para>
/// </remarks>
public sealed class ResultHttpOptions
{
    private FrozenDictionary<ErrorType, int>? _frozenStatusCodeMap;
    // Frozen snapshot of TitleOverrides, captured alongside _frozenStatusCodeMap in GetFrozenStatusCodeMap().
    // Accessed on the hot path (every request) without locks via the volatile read.
    private volatile FrozenDictionary<ErrorType, string>? _frozenTitleOverrides;
    private volatile bool _isFrozen;
    private readonly object _freezeLock = new();
    private volatile ReadOnlyDictionary<ErrorType, int>? _cachedReadOnlyMap;
    // Cached ReadOnlyDictionary wrapper for TitleOverrides, preventing callers from casting back to
    // the underlying Dictionary<ErrorType, string> and bypassing the freeze guard.
    // Invalidated when ConfigureTitleOverride() is called (same pattern as _cachedReadOnlyMap).
    private volatile ReadOnlyDictionary<ErrorType, string>? _cachedReadOnlyTitleOverrides;
    private readonly Dictionary<ErrorType, string> _titleOverrides = new();

    private readonly Dictionary<ErrorType, int> _statusCodeMap = new()
    {
        // ErrorType.Failure is a generic failure bucket that can represent server-side errors,
        // infrastructure failures, or unclassified domain errors. Using 500 is safer because it
        // does not incorrectly imply a client-side bad request. Use ErrorType.Validation (400) or
        // ErrorType.Domain (422) for errors that are explicitly caused by the caller's input.
        [ErrorType.Failure] = StatusCodes.Status500InternalServerError,
        [ErrorType.Validation] = StatusCodes.Status400BadRequest,
        [ErrorType.NotFound] = StatusCodes.Status404NotFound,
        [ErrorType.Conflict] = StatusCodes.Status409Conflict,
        [ErrorType.Unauthorized] = StatusCodes.Status401Unauthorized,
        [ErrorType.Forbidden] = StatusCodes.Status403Forbidden,
        [ErrorType.Unavailable] = StatusCodes.Status503ServiceUnavailable,
        [ErrorType.Unexpected] = StatusCodes.Status500InternalServerError,
        [ErrorType.Domain] = StatusCodes.Status422UnprocessableEntity,
        [ErrorType.Infrastructure] = StatusCodes.Status500InternalServerError,
        // ErrorType.Custom represents application-defined errors that may indicate server-side
        // or domain-specific failures. Defaulting to 500 avoids incorrectly implying client error
        // for semantically ambiguous custom error types. Override via ConfigureStatusCode if needed.
        [ErrorType.Custom] = StatusCodes.Status500InternalServerError
    };

    /// <summary>
    /// Gets a read-only view of the current status code map.
    /// To modify mappings, use <see cref="ConfigureStatusCode"/> before the first request.
    /// </summary>
    /// <remarks>
    /// Returns a cached <see cref="ReadOnlyDictionary{TKey,TValue}"/> wrapper to prevent consumers from
    /// casting back to the underlying <see cref="Dictionary{TKey,TValue}"/> and bypassing the
    /// <see cref="ConfigureStatusCode"/> freeze guard. The wrapper is cached to avoid heap allocation
    /// on every access. The cache is invalidated when <see cref="ConfigureStatusCode"/> is called.
    /// </remarks>
    public IReadOnlyDictionary<ErrorType, int> StatusCodeMap =>
        _cachedReadOnlyMap ??= new ReadOnlyDictionary<ErrorType, int>(_statusCodeMap);

    /// <summary>
    /// Overrides the HTTP status code for a specific <see cref="ErrorType"/>.
    /// </summary>
    /// <param name="type">The error type to configure.</param>
    /// <param name="statusCode">The HTTP status code to map to.</param>
    /// <returns>This instance for method chaining.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when called after the first request has been processed (options are frozen).
    /// </exception>
    /// <remarks>
    /// Thread safety: this method acquires the internal freeze lock to be mutually exclusive with
    /// <see cref="GetFrozenStatusCodeMap"/>. This prevents a TOCTOU race where a frozen snapshot
    /// could be captured between the guard check and the dictionary write.
    /// Configure all options before the first request; concurrent configuration is not supported.
    /// </remarks>
    public ResultHttpOptions ConfigureStatusCode(ErrorType type, int statusCode)
    {
        Monitor.Enter(_freezeLock);
        try
        {
            if (_isFrozen)
            {
                throw new InvalidOperationException(
                    "ResultHttpOptions cannot be modified after the first request has been processed. " +
                    "Call ConfigureStatusCode during application startup before any requests are handled.");
            }
            _statusCodeMap[type] = statusCode;
            // Invalidate the cached read-only wrapper so the next access reflects the updated map.
            // _cachedReadOnlyMap is volatile, which provides a release memory barrier on write —
            // required on ARM64 where plain non-volatile stores can be reordered relative to other
            // memory operations. The volatile write ensures all CPUs observe the null value promptly.
            _cachedReadOnlyMap = null;
        }
        finally
        {
            Monitor.Exit(_freezeLock);
        }
        return this;
    }

    // ─── Backing fields for scalar properties protected by the freeze guard ────────────────────────
    // These properties are mutable during startup (Configure* phase) and become read-only after the
    // first request. The freeze guard ensures that accidental mutation inside a handler (e.g., setting
    // IncludeDescription = true from a request pipeline) is caught at runtime rather than silently
    // changing behavior for all subsequent requests (information-disclosure vector, ARB finding F2-04-A).
    private int _defaultSuccessStatusCode = StatusCodes.Status204NoContent;
    private bool _includeTraceId;
    private bool _includeDescription;
    private string _defaultFallbackDescription = "An error occurred.";

    /// <summary>
    /// Default HTTP status code returned for successful non-generic Result instances.
    /// Defaults to <see cref="StatusCodes.Status204NoContent"/> for command-style operations.
    /// Must be configured before the first request — throws <see cref="InvalidOperationException"/> if set after freeze.
    /// </summary>
    public int DefaultSuccessStatusCode
    {
        get => _defaultSuccessStatusCode;
        set
        {
            ThrowIfFrozen(nameof(DefaultSuccessStatusCode));
            _defaultSuccessStatusCode = value;
        }
    }

    /// <summary>
    /// Gets or sets whether to include <c>traceId</c> in ProblemDetails error extensions.
    /// Defaults to <see langword="false"/> to avoid exposing distributed trace identifiers
    /// to external clients, which can aid in correlating attacks or revealing infrastructure details.
    /// Set to <see langword="true"/> when building internal APIs where trace correlation is valuable.
    /// Must be configured before the first request — throws <see cref="InvalidOperationException"/> if set after freeze.
    /// </summary>
    /// <remarks>
    /// When <see langword="false"/>, the <c>traceId</c> field is omitted from <c>ErrorDetailDto</c>
    /// in the ProblemDetails extensions. The trace ID is still captured and available via
    /// <see cref="Error.TraceId"/> for internal logging and observability.
    /// </remarks>
    public bool IncludeTraceId
    {
        get => _includeTraceId;
        set
        {
            ThrowIfFrozen(nameof(IncludeTraceId));
            _includeTraceId = value;
        }
    }

    /// <summary>
    /// Gets or sets whether to include the full <c>description</c> in ProblemDetails error extensions.
    /// Defaults to <see langword="false"/> for secure-by-default behavior.
    /// Must be configured before the first request — throws <see cref="InvalidOperationException"/> if set after freeze.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>⚠ Security warning:</b> Error descriptions may contain sensitive infrastructure details
    /// (connection strings, internal paths, service names, stack trace fragments). This option
    /// defaults to <see langword="false"/> to prevent information leakage in production environments.
    /// </para>
    /// <para>
    /// When <see langword="false"/>, the description field in <c>ErrorDetailDto</c> is replaced with
    /// a generic message (<c>"An error occurred."</c>). The full description remains available via
    /// <see cref="Error.Description"/> for internal logging and observability.
    /// </para>
    /// <para>
    /// <b>Enabling for development only (recommended pattern):</b>
    /// <code>
    /// // Option 1 — fluent convenience method:
    /// services.Configure&lt;ResultHttpOptions&gt;(options =&gt;
    ///     options.IncludeDescriptionInDevelopment(env));
    ///
    /// // Option 2 — manual conditional:
    /// services.Configure&lt;ResultHttpOptions&gt;(options =&gt;
    ///     options.IncludeDescription = env.IsDevelopment());
    /// </code>
    /// </para>
    /// </remarks>
    public bool IncludeDescription
    {
        get => _includeDescription;
        set
        {
            ThrowIfFrozen(nameof(IncludeDescription));
            _includeDescription = value;
        }
    }

    /// <summary>
    /// Gets or sets the generic description used in ProblemDetails when <see cref="IncludeDescription"/>
    /// is <see langword="false"/> (the secure default). Defaults to <c>"An error occurred."</c>.
    /// Must be configured before the first request — throws <see cref="InvalidOperationException"/> if set after freeze.
    /// </summary>
    /// <remarks>
    /// Override this to customise the fallback message for branding, internationalisation,
    /// or security policy requirements. For example:
    /// <code>
    /// services.Configure&lt;ResultHttpOptions&gt;(options =>
    ///     options.DefaultFallbackDescription = "An unexpected error occurred. Please contact support.");
    /// </code>
    /// </remarks>
    public string DefaultFallbackDescription
    {
        get => _defaultFallbackDescription;
        set
        {
            ThrowIfFrozen(nameof(DefaultFallbackDescription));
            _defaultFallbackDescription = value;
        }
    }

    /// <summary>
    /// Enables <see cref="IncludeDescription"/> when the application is running in the development
    /// environment, and leaves it disabled (the default) in all other environments.
    /// </summary>
    /// <param name="environment">The current <see cref="Microsoft.Extensions.Hosting.IHostEnvironment"/>.</param>
    /// <returns>This <see cref="ResultHttpOptions"/> instance for fluent chaining.</returns>
    /// <remarks>
    /// This is a convenience method to apply the recommended pattern for description exposure:
    /// show descriptions in development for faster debugging, hide them in staging/production
    /// to avoid information leakage.
    /// <code>
    /// services.Configure&lt;ResultHttpOptions&gt;(options =&gt;
    ///     options.IncludeDescriptionInDevelopment(env));
    /// </code>
    /// </remarks>
    public ResultHttpOptions IncludeDescriptionInDevelopment(Microsoft.Extensions.Hosting.IHostEnvironment environment)
    {
        ArgumentNullException.ThrowIfNull(environment);
        ThrowIfFrozen(nameof(IncludeDescriptionInDevelopment));

        // IsDevelopment() is an extension method in Microsoft.Extensions.Hosting.HostEnvironmentEnvExtensions.
        // Compare EnvironmentName directly to avoid a using directive dependency that could cause
        // trimming issues in AOT scenarios where extension methods may not be inlined.
        _includeDescription = string.Equals(
            environment.EnvironmentName,
            Microsoft.Extensions.Hosting.Environments.Development,
            StringComparison.OrdinalIgnoreCase);
        return this;
    }

    /// <summary>
    /// Gets the dictionary of custom ProblemDetails <c>title</c> values for specific <see cref="ErrorType"/> values.
    /// When an entry is present for an <see cref="ErrorType"/>, it overrides the default title.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Use <see cref="ConfigureTitleOverride"/> to set domain-specific titles for error types.
    /// This property exposes the overrides as a read-only view for inspection. Mutations
    /// are protected by the same freeze guard as <see cref="ConfigureStatusCode"/>.
    /// </para>
    /// <para>
    /// <b>Thread safety and cast protection:</b> This property returns a cached
    /// <see cref="ReadOnlyDictionary{TKey,TValue}"/> wrapper that prevents consumers from casting back to
    /// the underlying <see cref="Dictionary{TKey,TValue}"/> and bypassing the freeze guard.
    /// All mutations must go through <see cref="ConfigureTitleOverride"/> during application startup.
    /// </para>
    /// </remarks>
    public IReadOnlyDictionary<ErrorType, string> TitleOverrides =>
        _cachedReadOnlyTitleOverrides ??= new ReadOnlyDictionary<ErrorType, string>(_titleOverrides);

    /// <summary>
    /// Configures a title override for the specified <see cref="ErrorType"/>.
    /// </summary>
    /// <param name="type">The error type to override the title for.</param>
    /// <param name="title">The custom title to use in ProblemDetails.</param>
    /// <returns>This instance for fluent chaining.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when called after the first request has been processed (options are frozen).
    /// </exception>
    /// <remarks>
    /// <code>
    /// options.ConfigureTitleOverride(ErrorType.Custom, "Payment Error")
    ///        .ConfigureTitleOverride(ErrorType.Domain, "Business Rule Violation");
    /// </code>
    /// </remarks>
    public ResultHttpOptions ConfigureTitleOverride(ErrorType type, string title)
    {
        Monitor.Enter(_freezeLock);
        try
        {
            if (_isFrozen)
            {
                throw new InvalidOperationException(
                    "ResultHttpOptions cannot be modified after the first request has been processed. " +
                    "Call ConfigureTitleOverride during application startup before any requests are handled.");
            }
            _titleOverrides[type] = title;
            // Invalidate the cached read-only wrapper so the next access reflects the updated map.
            // _cachedReadOnlyTitleOverrides is volatile, providing a release memory barrier on write.
            _cachedReadOnlyTitleOverrides = null;
        }
        finally
        {
            Monitor.Exit(_freezeLock);
        }
        return this;
    }

    // Backing field for TypeUriBase — protected by freeze guard (ARB finding F2-04-A).
    private string _typeUriBase = "about:blank";

    /// <summary>
    /// Default base URI format for RFC 9457 problem details types.
    /// Must be configured before the first request — throws <see cref="InvalidOperationException"/> if set after freeze.
    /// </summary>
    /// <remarks>
    /// <para>
    /// RFC 9457 §4.2.1 recommends <c>"about:blank"</c> as the <c>type</c> URI when there is no
    /// specific URI describing the problem type. When this property is set to the default
    /// <c>"about:blank"</c>, the <c>type</c> field in ProblemDetails will be exactly
    /// <c>"about:blank"</c> (no section suffix appended), conforming to the RFC.
    /// </para>
    /// <para>
    /// To include RFC 9110 section links in <c>type</c>, set:
    /// <code>options.TypeUriBase = "https://www.rfc-editor.org/rfc/rfc9110#section-";</code>
    /// This will produce URIs like <c>"https://www.rfc-editor.org/rfc/rfc9110#section-15.5.1"</c>.
    /// To use application-specific problem type URIs, set the base URL of your own error catalog:
    /// <code>options.TypeUriBase = "https://api.myapp.com/errors/";</code>
    /// </para>
    /// <para>
    /// <b>Note:</b> When this value is exactly <c>"about:blank"</c>, the HTTP status section
    /// suffix is NOT appended to avoid producing the invalid URI <c>"about:blank#15.5.1"</c>.
    /// For any other base URI, the RFC 9110 status section suffix is appended normally.
    /// </para>
    /// </remarks>
    public string TypeUriBase
    {
        get => _typeUriBase;
        set
        {
            ThrowIfFrozen(nameof(TypeUriBase));
            _typeUriBase = value;
        }
    }

    /// <summary>
    /// Gets a frozen representation of the StatusCodeMap for thread-safe lock-free reads on the hot path.
    /// After the first call, the map cannot be modified.
    /// </summary>
    /// <remarks>
    /// Thread safety: uses a double-check lock with <c>volatile</c> field read for the outer check.
    /// The <c>volatile</c> modifier on <see cref="_frozenStatusCodeMap"/> ensures a full memory barrier
    /// on every read, making the pattern safe on all architectures including ARM64 which has a weaker
    /// memory model than x86-64. <see cref="IsFrozen"/> is set to <see langword="true"/> before the
    /// snapshot is taken so that any concurrent <see cref="ConfigureStatusCode"/> call that passes the
    /// guard will throw rather than mutate the underlying dictionary after the snapshot has been captured.
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Major Bug", "S2583:Change this condition so that it does not always evaluate to 'False'", Justification = "Double-checked locking pattern requires re-checking volatile field inside lock")]
    internal FrozenDictionary<ErrorType, int> GetFrozenStatusCodeMap()
    {
        var frozen = Volatile.Read(ref _frozenStatusCodeMap);
        if (frozen != null) return frozen;

        Monitor.Enter(_freezeLock);
        try
        {
#pragma warning disable S2583 // Double-checked locking pattern requires re-checking volatile field inside lock
            frozen = Volatile.Read(ref _frozenStatusCodeMap);
            if (frozen != null) return frozen;
#pragma warning restore S2583

            // Create both snapshots into local variables FIRST. If either ToFrozenDictionary() call
            // throws (pathological edge case), _isFrozen stays false and options remain mutable.
            // Both maps are captured atomically inside the lock so no thread can see a partially-frozen state.
            // TitleOverrides is included here so it is also protected against post-first-request mutation
            // (previously only _statusCodeMap had freeze protection).
            var statusSnapshot = _statusCodeMap.ToFrozenDictionary();
            var titleSnapshot = _titleOverrides.ToFrozenDictionary();

            // Set _isFrozen = true BEFORE assigning the snapshot fields, so that any concurrent
            // ConfigureStatusCode() call that acquires the lock after this point sees _isFrozen = true
            // and throws rather than mutating the underlying dictionary after the snapshot was taken.
            _isFrozen = true;

            _frozenTitleOverrides = titleSnapshot;
            _frozenStatusCodeMap = statusSnapshot;
            return statusSnapshot;
        }
        finally
        {
            Monitor.Exit(_freezeLock);
        }
    }

    /// <summary>
    /// Gets the frozen (lock-free) snapshot of <see cref="TitleOverrides"/> for use on the hot path.
    /// Returns <see langword="null"/> before the first request (before freeze).
    /// </summary>
    internal FrozenDictionary<ErrorType, string>? GetFrozenTitleOverrides() => _frozenTitleOverrides;

    /// <summary>
    /// Returns the title override for the specified <see cref="ErrorType"/>, or <see langword="null"/>
    /// if no override is configured for that type. Thread-safe in both pre-freeze and post-freeze states.
    /// </summary>
    /// <remarks>
    /// Post-freeze: uses the lock-free <see cref="_frozenTitleOverrides"/> snapshot.
    /// Pre-freeze: acquires <see cref="_freezeLock"/> to safely read from the mutable
    /// <see cref="_titleOverrides"/> dictionary, which may be concurrently written by
    /// <see cref="ConfigureTitleOverride"/> during application startup.
    /// </remarks>
    internal string? GetTitleOverride(ErrorType type)
    {
        // Post-freeze fast path: lock-free FrozenDictionary read.
        var frozen = _frozenTitleOverrides;
        if (frozen != null)
        {
            return frozen.TryGetValue(type, out var frozenTitle) ? frozenTitle : null;
        }

        // Pre-freeze path: must acquire the lock because ConfigureTitleOverride() may be writing
        // to _titleOverrides concurrently (e.g., during ASP.NET Core startup parallel initialization).
        Monitor.Enter(_freezeLock);
        try
        {
            return _titleOverrides.TryGetValue(type, out var overrideTitle) ? overrideTitle : null;
        }
        finally
        {
            Monitor.Exit(_freezeLock);
        }
    }

    /// <summary>
    /// For internal test observation of the race condition logic, internal callers can check
    /// whether the snapshot is currently initialized.
    /// </summary>
    internal FrozenDictionary<ErrorType, int>? GetInternalFrozenStatusCodeMap() => _frozenStatusCodeMap;

    /// <summary>
    /// Returns whether the options have already been frozen (i.e., first request was processed).
    /// </summary>
    /// <remarks>
    /// This property reflects internal lifecycle state. It is <c>internal</c> to avoid
    /// consumers depending on the freeze timing as part of the public contract.
    /// </remarks>
    internal bool IsFrozen => _isFrozen;

    /// <summary>
    /// Throws <see cref="InvalidOperationException"/> if the options have been frozen (i.e., the first
    /// request has been processed). Called by every property setter and mutating method to enforce
    /// the startup-only configuration contract across both dictionary and scalar options.
    /// </summary>
    /// <param name="memberName">Name of the property or method being mutated, for the error message.</param>
    /// <remarks>
    /// This helper is introduced as part of ARB audit finding F2-04-A: previously, scalar properties
    /// (<see cref="IncludeDescription"/>, <see cref="IncludeTraceId"/>, <see cref="DefaultSuccessStatusCode"/>,
    /// <see cref="DefaultFallbackDescription"/>, <see cref="TypeUriBase"/>) were unprotected auto-properties
    /// that could be mutated post-freeze, creating an inconsistency with the dictionary freeze guard and a
    /// potential information-disclosure vector (e.g., <see cref="IncludeDescription"/> set to
    /// <see langword="true"/> from inside a request handler would silently expose error details to all
    /// subsequent API clients).
    /// </remarks>
    private void ThrowIfFrozen(string memberName)
    {
        // Volatile read — no lock needed for the guard check; worst case is we allow a write
        // during the race window before the first freeze, which is the expected startup behavior.
        // The lock is only needed inside ConfigureStatusCode/ConfigureTitleOverride for the TOCTOU
        // race between the guard check and the dictionary write.
        if (_isFrozen)
        {
            throw new InvalidOperationException(
                $"ResultHttpOptions.{memberName} cannot be set after the first request has been processed. " +
                "Configure all options during application startup before any requests are handled.");
        }
    }
}




