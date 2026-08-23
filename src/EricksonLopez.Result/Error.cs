// Copyright © Erickson Lopez. MIT License.
// Attributes removed in favor of MSBuild <InternalsVisibleTo> items.
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;

namespace EricksonLopez.Result;

/// <summary>
/// Represents a structured, immutable domain error with support for deep equality, metadata, retryability, and distributed tracing.
/// </summary>
/// <remarks>
/// <para>
/// <b>Design — sealed class:</b> <see cref="Error"/> is <c>sealed</c> to guarantee correct equality semantics.
/// A non-sealed error class allows subclasses to add domain-specific fields without overriding
/// <see cref="Equals(Error?)"/> and <see cref="GetHashCode()"/>, which would silently break equality in
/// <c>HashSet&lt;Error&gt;</c>, <c>Dictionary&lt;Error, T&gt;</c>, and <c>Result.Equals</c> comparisons.
/// </para>
/// <para>
/// <b>Extensibility via composition:</b> Instead of subclassing, attach domain-specific context to an error
/// using <see cref="WithMetadata"/> or the fluent <see cref="ErrorBuilder"/> API:
/// <code>
/// // ✅ Preferred — composition via metadata:
/// var error = Error.Create("Payment.Declined", "Payment was declined.")
///     .WithType(ErrorType.Domain)
///     .WithMetadata("transactionId", transactionId)
///     .WithMetadata("gatewayCode", gatewayCode)
///     .Build();
///
/// // ✅ Or use domain-specific factory methods:
/// public static class PaymentErrors
/// {
///     public static Error Declined(string transactionId) =>
///         Error.Create("Payment.Declined", "Payment was declined.")
///             .WithType(ErrorType.Domain)
///             .WithMetadata("transactionId", transactionId)
///             .Build();
/// }
/// </code>
/// </para>
/// <para>
/// The constructor is <c>public</c> to allow direct construction. Prefer the static factory methods
/// (<see cref="Failure(string, string)"/>, <see cref="Validation(string, string)"/>, etc.) or
/// <see cref="Create"/> + <see cref="ErrorBuilder"/> for common patterns.
/// </para>
/// </remarks>
public sealed class Error : IEquatable<Error>
{
    // Single backing field for inner errors — always ImmutableArray<Error>.
    // Previously a dual-field design (_innerErrors: IReadOnlyList + _innerErrorsImmutable: ImmutableArray?)
    // was used because ImmutableArray<T> loses its concrete type when boxed into IReadOnlyList<T>,
    // making ErrorBuilder.FromError() unable to recover it without an O(n) copy.
    // Now we always store ImmutableArray<Error> and convert at construction time,
    // which simplifies the field layout, eliminates the dual-field complexity,
    // and makes InnerErrors return ImmutableArray<Error> directly (stronger type guarantee).
    private readonly ImmutableArray<Error> _innerErrors;
    private readonly ImmutableDictionary<string, object>? _metadata;

    // Stores the raw ActivityTraceId struct (32 bytes on stack, no heap allocation).
    // Stringified lazily in the TraceId property getter only when actually accessed.
    private readonly ActivityTraceId? _traceIdValue;

    // Holds an explicitly-provided trace ID string (from constructor or WithTraceId).
    // When non-null, takes precedence over _traceIdValue.
    private readonly string? _traceIdOverride;

    // Cache for the materialized TraceId string from _traceIdValue.
    // Written at most once via Interlocked.CompareExchange on first access.
    // Not readonly because it's lazily initialized; the field is effectively immutable
    // after first write (publish-once semantics). volatile ensures visibility across threads
    // on all architectures including ARM64.
    private volatile string? _cachedTraceIdString;

    // Internal accessors for ErrorBuilder.FromError() — avoids materializing the lazy TraceId string
    // when copying an Error into a builder for modification (e.g., adding metadata only).
    internal string? TraceIdOverride => _traceIdOverride;
    internal ActivityTraceId? TraceIdValue => _traceIdValue;
    internal ImmutableDictionary<string, object>? RawMetadata => _metadata;

    // Direct access to the immutable inner error array for ErrorBuilder.FromError().
    // Now always available (no longer nullable) — simplifies the fast-path in the builder.
    internal ImmutableArray<Error> RawInnerErrors => _innerErrors;

    /// <summary>Gets the unique machine-readable error code.</summary>
    public string Code { get; }

    /// <summary>Gets the technical description of the error for diagnostics and logging.</summary>
    public string Description { get; }

    /// <summary>Gets the optional localization resource key for the error message.</summary>
    public string? DescriptionKey { get; }

    /// <summary>Gets the classification category of the error.</summary>
    public ErrorType Type { get; }

    /// <summary>Gets the severity level of the error.</summary>
    public ErrorSeverity Severity { get; }

    /// <summary>Gets the retry classification indicating whether the operation may be retried.</summary>
    public ErrorRetryability Retryability { get; }

    /// <summary>
    /// Gets the OpenTelemetry trace identifier associated with this error.
    /// The string is materialized lazily on first access and cached to avoid repeated
    /// heap allocations for the 32-character trace ID string in concurrent scenarios.
    /// </summary>
    public string? TraceId
    {
        get
        {
            if (_traceIdOverride is not null) return _traceIdOverride;
            if (!_traceIdValue.HasValue) return null;

            // Fast path: already materialized
            var cached = _cachedTraceIdString;
            if (cached is not null) return cached;

            // Materialize once and cache. If two threads race, both produce identical strings
            // and one wins — the other's allocation is ephemeral but correctness is preserved.
            var materialized = _traceIdValue.Value.ToString();
            Interlocked.CompareExchange(ref _cachedTraceIdString, materialized, null);
            return _cachedTraceIdString!;
        }
    }

    /// <summary>Gets the distributed correlation identifier associated with this error.</summary>
    public string? CorrelationId { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Error"/> class.
    /// </summary>
    /// <param name="code">The unique machine-readable error code. Must not be null or whitespace.</param>
    /// <param name="description">The human-readable description for diagnostic logging. Must not be null or whitespace.</param>
    /// <param name="type">The error category type. Defaults to <see cref="ErrorType.Failure"/>.</param>
    /// <param name="severity">The error severity level. Defaults to <see cref="ErrorSeverity.Error"/>.</param>
    /// <param name="retryability">The retry classification. Defaults to <see cref="ErrorRetryability.NotApplicable"/>.</param>
    /// <param name="descriptionKey">The optional resource key for localized messages.</param>
    /// <param name="traceId">The optional distributed trace identifier string.</param>
    /// <param name="correlationId">The optional distributed correlation identifier string.</param>
    /// <param name="innerErrors">The optional collection of child errors composing this error.</param>
    /// <param name="metadata">The optional key-value metadata dictionary.</param>
    /// <exception cref="ArgumentException"><paramref name="code"/> or <paramref name="description"/> is <see langword="null"/>, empty, or consists only of white-space characters</exception>
    /// <remarks>
    /// <para>
    /// <b>⚠️ Prefer factory methods over this constructor.</b>
    /// Use <see cref="Failure(string, string)"/>, <see cref="Validation(string, string)"/>,
    /// <see cref="NotFound(string, string)"/>, or <see cref="Create(string, string)"/> + <see cref="ErrorBuilder"/>
    /// for fluent construction. This 10-parameter constructor is hidden from IntelliSense to avoid confusion,
    /// but remains public for advanced and serialization scenarios.
    /// </para>
    /// <para>
    /// When <paramref name="traceId"/> is null and an ambient <see cref="Activity"/> is active,
    /// the current <see cref="Activity.TraceId"/> is captured as an <see cref="ActivityTraceId"/> struct
    /// (zero heap allocation). The string representation is materialized lazily only when
    /// <see cref="TraceId"/> is actually accessed.
    /// </para>
    /// </remarks>
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public Error(
        string code,
        string description,
        ErrorType type = ErrorType.Failure,
        ErrorSeverity severity = ErrorSeverity.Error,
        ErrorRetryability retryability = ErrorRetryability.NotApplicable,
        string? descriptionKey = null,
        string? traceId = null,
        string? correlationId = null,
        IReadOnlyList<Error>? innerErrors = null,
        IReadOnlyDictionary<string, object>? metadata = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        ArgumentException.ThrowIfNullOrWhiteSpace(description);

        Code = code;
        Description = description;
        Type = type;
        Severity = severity;
        Retryability = retryability;
        DescriptionKey = descriptionKey;
        CorrelationId = correlationId;

        _innerErrors = innerErrors switch
        {
            null or { Count: 0 } => ImmutableArray<Error>.Empty,
            ImmutableArray<Error> arr => arr,
            _ => ImmutableArray.CreateRange(innerErrors)
        };
        _metadata = metadata switch
        {
            null or { Count: 0 } => null,
            _ => ImmutableDictionary.CreateRange(metadata)
        };

        if (traceId is not null)
        {
            // Caller explicitly provided a string — store as override, no struct needed.
            _traceIdOverride = traceId;
        }
        else if (Activity.Current is { } act)
        {
            // Capture the struct (zero heap allocation); stringify only if TraceId is accessed.
            _traceIdValue = act.TraceId;
        }
        // Otherwise both remain null → TraceId returns null.
    }

    // Private constructor used by fluent With* methods and CreateFromBuilder to avoid re-capturing
    // Activity.Current when copying an existing Error — the trace ID is already captured.
    private Error(
        string code,
        string description,
        ErrorType type,
        ErrorSeverity severity,
        ErrorRetryability retryability,
        string? descriptionKey,
        string? traceIdOverride,
        ActivityTraceId? traceIdValue,
        string? correlationId,
        ImmutableArray<Error> innerErrors,
        ImmutableDictionary<string, object>? metadata)
    {
        Code = code;
        Description = description;
        Type = type;
        Severity = severity;
        Retryability = retryability;
        DescriptionKey = descriptionKey;
        CorrelationId = correlationId;
        _traceIdOverride = traceIdOverride;
        _traceIdValue = traceIdValue;
        _innerErrors = innerErrors;
        _metadata = metadata;
    }

    /// <summary>
    /// Creates an <see cref="Error"/> from <see cref="ErrorBuilder"/> state, bypassing argument
    /// validation (already validated by the builder) and avoiding re-capture of
    /// <see cref="Activity.Current"/> (the builder holds an explicit <paramref name="traceId"/> string
    /// if one was needed, or null if not set).
    /// </summary>
    /// <remarks>
    /// This is intentionally <see langword="internal"/> — it is only safe to call from
    /// <see cref="ErrorBuilder.Build()"/> where all invariants have already been enforced.
    /// The builder's <see cref="ImmutableArray{T}"/> of inner errors is stored directly.
    /// </remarks>
    internal static Error CreateFromBuilder(
        string code,
        string description,
        ErrorType type,
        ErrorSeverity severity,
        ErrorRetryability retryability,
        string? descriptionKey,
        string? traceId,
        string? correlationId,
        ImmutableArray<Error> innerErrors,
        ImmutableDictionary<string, object>? metadata)
    {
        return new Error(
            code, description, type, severity, retryability,
            descriptionKey,
            traceIdOverride: traceId,
            traceIdValue: null,
            correlationId,
            innerErrors.IsDefaultOrEmpty ? ImmutableArray<Error>.Empty : innerErrors,
            metadata switch
            {
                null or { Count: 0 } => null,
                _ => metadata
            });
    }

    /// <summary>
    /// Gets the immutable list of inner errors composing this error.
    /// </summary>
    /// <remarks>
    /// Returns <see cref="ImmutableArray{T}.Empty"/> when there are no inner errors — never null.
    /// <para>
    /// Common patterns:
    /// <list type="bullet">
    ///   <item>Check for inner errors: <c>if (error.HasInnerErrors)</c> (preferred) or <c>if (!error.InnerErrors.IsEmpty)</c></item>
    ///   <item>Iterate: <c>foreach (var inner in error.InnerErrors)</c></item>
    ///   <item>LINQ: <c>error.InnerErrors.Any(e => e.Code == "...")</c> — note: requires <c>using System.Linq</c></item>
    /// </list>
    /// </para>
    /// <para>
    /// ⚠️ Migration note: this property changed from <c>IReadOnlyList&lt;Error&gt;</c> to
    /// <c>ImmutableArray&lt;Error&gt;</c>. If you store the result in a variable, update the
    /// declared type from <c>IReadOnlyList&lt;Error&gt;</c> to <c>ImmutableArray&lt;Error&gt;</c>
    /// or <c>var</c>. The <c>Count</c>, indexer, and <c>foreach</c> patterns remain unchanged.
    /// </para>
    /// </remarks>
    public ImmutableArray<Error> InnerErrors => _innerErrors;

    /// <summary>Gets the immutable key-value metadata dictionary associated with this error.</summary>
    /// <remarks>
    /// <para>
    /// Metadata values accept any <see langword="object"/> and are stored as-is in memory.
    /// Prefer simple, serializable types (<see langword="string"/>, <see langword="int"/>,
    /// <see langword="bool"/>, <see cref="Guid"/>) for values you intend to serialize.
    /// </para>
    /// <para>
    /// <b>⚠️ Serialization type-loss warning:</b> When this error is serialized and deserialized
    /// using the <c>EricksonLopez.Result.Serialization</c> package (<c>ErrorJsonConverter</c>),
    /// metadata values undergo type narrowing:
    /// <list type="bullet">
    ///   <item>JSON numbers are deserialized as <see langword="long"/> or <see langword="double"/> —
    ///         casting <c>(int)error.Metadata["orderId"]</c> will throw <see cref="System.InvalidCastException"/>.</item>
    ///   <item><see cref="Guid"/>, <see cref="DateTime"/>, and other complex types
    ///         are serialized as strings and deserialized as <see langword="string"/> —
    ///         casting back to the original type will throw.</item>
    ///   <item>Only <see langword="bool"/>, <see langword="long"/>, <see langword="double"/>,
    ///         and <see langword="string"/> round-trip without loss.</item>
    /// </list>
    /// Design metadata with this constraint in mind, or avoid relying on type-specific casts
    /// after deserialization.
    /// </para>
    /// </remarks>
    public IReadOnlyDictionary<string, object> Metadata => _metadata ?? ImmutableDictionary<string, object>.Empty;

    /// <summary>Gets a value indicating whether this error contains one or more child errors.</summary>
    public bool HasInnerErrors => !_innerErrors.IsDefaultOrEmpty;

    /// <summary>Gets a value indicating whether this error contains metadata entries.</summary>
    public bool HasMetadata => _metadata is not null;

    /// <summary>
    /// Attempts to retrieve a metadata value by key and cast it to the specified type.
    /// </summary>
    /// <typeparam name="T">The expected type of the metadata value.</typeparam>
    /// <param name="key">The metadata key to look up. Must not be null or whitespace.</param>
    /// <param name="value">
    /// When this method returns, contains the metadata value cast to <typeparamref name="T"/> if found and valid;
    /// otherwise, the default value for the type.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the key exists; otherwise, <see langword="false"/>.
    /// </returns>
    /// <exception cref="ArgumentException"><paramref name="key"/> is <see langword="null"/>, empty, or consists only of white-space characters</exception>
    /// <exception cref="InvalidCastException">The metadata key exists but the stored value cannot be cast to <typeparamref name="T"/></exception>
    /// <remarks>
    /// <para>
    /// <b>⚠️ Post-deserialization type narrowing:</b> JSON deserialization via
    /// <c>EricksonLopez.Result.Serialization</c> narrows metadata types:
    /// <list type="bullet">
    ///   <item>JSON integers → <see langword="long"/> (not <see langword="int"/>)</item>
    ///   <item><see cref="Guid"/>, <see cref="DateTime"/> → <see langword="string"/></item>
    ///   <item>Only <see langword="bool"/>, <see langword="long"/>, <see langword="double"/>,
    ///         and <see langword="string"/> survive a JSON round-trip without type loss.</item>
    /// </list>
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // After JSON round-trip, int is stored as long:
    /// if (error.TryGetMetadata&lt;long&gt;("orderId", out var id))
    ///     Console.WriteLine(id);
    ///
    /// // In-memory (no round-trip), original types are preserved:
    /// if (error.TryGetMetadata&lt;Guid&gt;("correlationId", out var guid))
    ///     Console.WriteLine(guid);
    /// </code>
    /// </example>
    [System.Diagnostics.Contracts.Pure]
    public bool TryGetMetadata<T>(string key, out T? value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        if (_metadata is null || !_metadata.TryGetValue(key, out var raw) || raw is null)
        {
            value = default;
            return false;
        }

        if (raw is T typed)
        {
            value = typed;
            return true;
        }

        throw new InvalidCastException(
            $"Metadata key '{key}' has value of type '{raw.GetType().FullName}' which cannot be cast to '{typeof(T).FullName}'. " +
            "If this Error was deserialized from JSON, numeric types are narrowed to 'long'/'double' and " +
            "complex types (Guid, DateTime) are narrowed to 'string'. " +
            "Use the narrowed type in TryGetMetadata<T>, or avoid type-specific casts after deserialization.");
    }

    /// <summary>
    /// Retrieves a metadata value by key and casts it to the specified type.
    /// </summary>
    /// <typeparam name="T">The expected type of the metadata value.</typeparam>
    /// <param name="key">The metadata key to look up. Must not be null or whitespace.</param>
    /// <returns>The metadata value cast to <typeparamref name="T"/>.</returns>
    /// <exception cref="ArgumentException"><paramref name="key"/> is <see langword="null"/>, empty, or consists only of white-space characters</exception>
    /// <exception cref="KeyNotFoundException">The metadata key does not exist in this error</exception>
    /// <exception cref="InvalidCastException">The metadata key exists but the stored value is <see langword="null"/> or cannot be cast to <typeparamref name="T"/></exception>
    [System.Diagnostics.Contracts.Pure]
    public T GetMetadata<T>(string key)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        if (_metadata is null || !_metadata.TryGetValue(key, out var raw))
            throw new KeyNotFoundException($"Metadata key '{key}' was not found in this Error.");

        if (raw is T typed)
            return typed;

        if (raw is null)
            throw new InvalidCastException(
                $"Metadata key '{key}' has a null value which cannot be cast to '{typeof(T).FullName}'.");

        throw new InvalidCastException(
            $"Metadata key '{key}' has value of type '{raw.GetType().FullName}' which cannot be cast to '{typeof(T).FullName}'. " +
            "If this Error was deserialized from JSON, numeric types are narrowed to 'long'/'double' and " +
            "complex types (Guid, DateTime) are narrowed to 'string'. " +
            "Use the narrowed type in GetMetadata<T>, or avoid type-specific casts after deserialization.");
    }

    /// <summary>
    /// Creates a sentinel Error for use in static readonly fields (e.g., <see cref="WellKnownErrors"/>)
    /// where the trace ID must be explicitly null and must NOT capture <see cref="Activity.Current"/>
    /// at static initialization time.
    /// </summary>
    internal static Error CreateSentinel(
        string code,
        string description,
        ErrorType type,
        ErrorSeverity severity,
        ErrorRetryability retryability)
        => new(code, description, type, severity, retryability,
               descriptionKey: null,
               traceIdOverride: null,
               traceIdValue: null,
               correlationId: null,
               innerErrors: ImmutableArray<Error>.Empty,
               metadata: null);

    // ─── Factory Methods ──────────────────────────────────────────────────────

    /// <summary>
    /// Creates an <see cref="ErrorBuilder"/> initialized with the specified error code and description.
    /// </summary>
    /// <param name="code">The unique machine-readable error code. Must not be null or whitespace.</param>
    /// <param name="description">The human-readable technical description. Must not be null or whitespace.</param>
    /// <returns>An <see cref="ErrorBuilder"/> pre-seeded with the specified code and description.</returns>
    /// <example>
    /// <code>
    /// var error = Error.Create("Order.Expired", "The order has expired.")
    ///     .WithType(ErrorType.Domain)
    ///     .WithSeverity(ErrorSeverity.Warning)
    ///     .WithRetryability(ErrorRetryability.Permanent)
    ///     .WithCorrelationId(correlationId)
    ///     .WithMetadata("orderId", orderId)
    ///     .Build();
    /// </code>
    /// </example>
    [Pure]
    public static ErrorBuilder Create(string code, string description)
        => new(code, description);

    /// <summary>Creates a custom error with specified attributes.</summary>
    /// <param name="code">The unique machine-readable error code.</param>
    /// <param name="description">The human-readable technical description.</param>
    /// <param name="type">The error category type.</param>
    /// <param name="severity">The error severity level. Defaults to <see cref="ErrorSeverity.Error"/>.</param>
    /// <param name="retryability">The retry classification. Defaults to <see cref="ErrorRetryability.NotApplicable"/>.</param>
    /// <param name="descriptionKey">The optional resource key for localized messages.</param>
    /// <param name="traceId">The optional distributed trace identifier string.</param>
    /// <param name="correlationId">The optional distributed correlation identifier string.</param>
    /// <param name="innerErrors">The optional collection of child errors composing this error.</param>
    /// <param name="metadata">The optional key-value metadata dictionary.</param>
    /// <returns>A new <see cref="Error"/> instance configured with the specified attributes.</returns>
    /// <remarks>
    /// For most use cases, prefer <see cref="Create(string, string)"/> with the fluent <see cref="ErrorBuilder"/> pattern.
    /// </remarks>
    [Pure]
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Advanced)]
    public static Error Custom(
        string code,
        string description,
        ErrorType type,
        ErrorSeverity severity = ErrorSeverity.Error,
        ErrorRetryability retryability = ErrorRetryability.NotApplicable,
        string? descriptionKey = null,
        string? traceId = null,
        string? correlationId = null,
        IReadOnlyList<Error>? innerErrors = null,
        IReadOnlyDictionary<string, object>? metadata = null)
        => new(code, description, type, severity, retryability, descriptionKey, traceId, correlationId, innerErrors, metadata);

    /// <summary>Creates a custom error with specified metadata and child errors.</summary>
    /// <param name="code">The unique machine-readable error code.</param>
    /// <param name="description">The human-readable technical description.</param>
    /// <param name="type">The error category type.</param>
    /// <param name="severity">The error severity level.</param>
    /// <param name="retryability">The retry classification.</param>
    /// <param name="metadata">The optional key-value metadata dictionary.</param>
    /// <param name="innerErrors">The child errors composing this error.</param>
    /// <returns>A new <see cref="Error"/> instance configured with the specified attributes.</returns>
    /// <remarks>
    /// For most use cases, prefer <see cref="Create(string, string)"/> with the fluent <see cref="ErrorBuilder"/> pattern.
    /// </remarks>
    [Pure]
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Advanced)]
    public static Error Custom(
        string code,
        string description,
        ErrorType type,
        ErrorSeverity severity,
        ErrorRetryability retryability,
        IReadOnlyDictionary<string, object>? metadata,
        params Error[] innerErrors)
        => new(code, description, type, severity, retryability, metadata: metadata, innerErrors: innerErrors);

    /// <summary>Creates a new <see cref="Error"/> representing a general failure.</summary>
    /// <param name="code">The unique machine-readable error code.</param>
    /// <param name="description">The human-readable technical description.</param>
    /// <returns>A new <see cref="Error"/> instance with <see cref="ErrorType.Failure"/> type and <see cref="ErrorSeverity.Error"/> severity.</returns>
    [Pure]
    public static Error Failure(string code, string description)
        => new(code, description, ErrorType.Failure, ErrorSeverity.Error);

    /// <summary>Creates a new <see cref="Error"/> representing a general failure wrapping child errors.</summary>
    /// <param name="code">The unique machine-readable error code.</param>
    /// <param name="description">The human-readable technical description.</param>
    /// <param name="innerErrors">The child errors caused by or contributing to this failure.</param>
    /// <returns>A new <see cref="Error"/> instance containing the specified child errors.</returns>
    [Pure]
    public static Error Failure(string code, string description, params Error[] innerErrors)
        => new(code, description, ErrorType.Failure, ErrorSeverity.Error, innerErrors: innerErrors);

    /// <summary>Creates a new <see cref="Error"/> representing an input validation failure.</summary>
    /// <param name="code">The unique machine-readable error code.</param>
    /// <param name="description">The human-readable technical description.</param>
    /// <returns>A new <see cref="Error"/> instance with <see cref="ErrorType.Validation"/> type and <see cref="ErrorSeverity.Warning"/> severity.</returns>
    [Pure]
    public static Error Validation(string code, string description)
        => new(code, description, ErrorType.Validation, ErrorSeverity.Warning);

    /// <summary>Creates a new <see cref="Error"/> representing an input validation failure wrapping child errors.</summary>
    /// <param name="code">The unique machine-readable error code.</param>
    /// <param name="description">The human-readable technical description.</param>
    /// <param name="innerErrors">The validation child errors.</param>
    /// <returns>A new <see cref="Error"/> instance containing the specified child errors.</returns>
    [Pure]
    public static Error Validation(string code, string description, params Error[] innerErrors)
        => new(code, description, ErrorType.Validation, ErrorSeverity.Warning, innerErrors: innerErrors);

    /// <summary>Creates a new <see cref="Error"/> representing a missing or nonexistent resource.</summary>
    /// <param name="code">The unique machine-readable error code.</param>
    /// <param name="description">The human-readable technical description.</param>
    /// <returns>A new <see cref="Error"/> instance with <see cref="ErrorType.NotFound"/> type and <see cref="ErrorSeverity.Warning"/> severity.</returns>
    [Pure]
    public static Error NotFound(string code, string description)
        => new(code, description, ErrorType.NotFound, ErrorSeverity.Warning);

    /// <summary>Creates a new <see cref="Error"/> representing a state conflict with existing resources.</summary>
    /// <param name="code">The unique machine-readable error code.</param>
    /// <param name="description">The human-readable technical description.</param>
    /// <returns>A new <see cref="Error"/> instance with <see cref="ErrorType.Conflict"/> type and <see cref="ErrorSeverity.Warning"/> severity.</returns>
    [Pure]
    public static Error Conflict(string code, string description)
        => new(code, description, ErrorType.Conflict, ErrorSeverity.Warning);

    /// <summary>Creates a new <see cref="Error"/> representing an unauthenticated request requiring authentication.</summary>
    /// <param name="code">The unique machine-readable error code.</param>
    /// <param name="description">The human-readable technical description.</param>
    /// <returns>A new <see cref="Error"/> instance with <see cref="ErrorType.Unauthorized"/> type and <see cref="ErrorSeverity.Error"/> severity.</returns>
    [Pure]
    public static Error Unauthorized(string code, string description)
        => new(code, description, ErrorType.Unauthorized, ErrorSeverity.Error);

    /// <summary>Creates a new <see cref="Error"/> representing an authorization or permission denial failure.</summary>
    /// <param name="code">The unique machine-readable error code.</param>
    /// <param name="description">The human-readable technical description.</param>
    /// <returns>A new <see cref="Error"/> instance with <see cref="ErrorType.Forbidden"/> type and <see cref="ErrorSeverity.Error"/> severity.</returns>
    [Pure]
    public static Error Forbidden(string code, string description)
        => new(code, description, ErrorType.Forbidden, ErrorSeverity.Error);

    /// <summary>Creates a new <see cref="Error"/> representing a transient external service unavailability failure.</summary>
    /// <param name="code">The unique machine-readable error code.</param>
    /// <param name="description">The human-readable technical description.</param>
    /// <returns>A new <see cref="Error"/> instance with <see cref="ErrorType.Unavailable"/> type, <see cref="ErrorSeverity.Error"/> severity, and <see cref="ErrorRetryability.Transient"/> retryability.</returns>
    [Pure]
    public static Error Unavailable(string code, string description)
        => new(code, description, ErrorType.Unavailable, ErrorSeverity.Error, retryability: ErrorRetryability.Transient);

    /// <summary>Creates a new <see cref="Error"/> representing an unexpected critical system failure.</summary>
    /// <param name="code">The unique machine-readable error code.</param>
    /// <param name="description">The human-readable technical description.</param>
    /// <returns>A new <see cref="Error"/> instance with <see cref="ErrorType.Unexpected"/> type and <see cref="ErrorSeverity.Critical"/> severity.</returns>
    [Pure]
    public static Error Unexpected(string code, string description)
        => new(code, description, ErrorType.Unexpected, ErrorSeverity.Critical);

    /// <summary>Creates a new <see cref="Error"/> representing a business rule or domain invariant violation.</summary>
    /// <param name="code">The unique machine-readable error code.</param>
    /// <param name="description">The human-readable technical description.</param>
    /// <returns>A new <see cref="Error"/> instance with <see cref="ErrorType.Domain"/> type and <see cref="ErrorSeverity.Error"/> severity.</returns>
    [Pure]
    public static Error Domain(string code, string description)
        => new(code, description, ErrorType.Domain, ErrorSeverity.Error);

    /// <summary>Creates a new <see cref="Error"/> representing an infrastructure, network, or database connectivity failure.</summary>
    /// <param name="code">The unique machine-readable error code.</param>
    /// <param name="description">The human-readable technical description.</param>
    /// <returns>A new <see cref="Error"/> instance with <see cref="ErrorType.Infrastructure"/> type, <see cref="ErrorSeverity.Error"/> severity, and <see cref="ErrorRetryability.Transient"/> retryability.</returns>
    [Pure]
    public static Error Infrastructure(string code, string description)
        => new(code, description, ErrorType.Infrastructure, ErrorSeverity.Error, retryability: ErrorRetryability.Transient);

    // ─── Fluent Builders ──────────────────────────────────────────────────────

    /// <summary>Creates a new <see cref="Error"/> copy containing the specified metadata entry.</summary>
    /// <param name="key">The metadata key to add or update. Must not be null or whitespace.</param>
    /// <param name="value">The metadata value to associate with the key.</param>
    /// <returns>A new <see cref="Error"/> instance with the added or updated metadata entry.</returns>
    /// <exception cref="ArgumentException"><paramref name="key"/> is <see langword="null"/>, empty, or consists only of white-space characters</exception>
    /// <remarks>
    /// <para>
    /// <b>⚠️ Performance Warning:</b> Each call to <c>WithMetadata</c> creates a new <see cref="Error"/>
    /// instance and a new <see cref="System.Collections.Immutable.ImmutableDictionary{TKey,TValue}"/> snapshot.
    /// Internally, this uses <c>ImmutableDictionary.Builder</c>: the add operation is O(log k) and
    /// <c>ToImmutable()</c> is O(k) where k is the current metadata count. Additionally, each call
    /// copies all 10 fields of <c>Error</c>. Chaining n calls (e.g.,
    /// <c>.WithMetadata("a", 1).WithMetadata("b", 2).WithMetadata("c", 3)</c>) creates n intermediate
    /// Error copies — O(n·k) total cost, not O(n²) in the dictionary.
    /// For multiple metadata entries, use one of these alternatives:
    /// </para>
    /// <list type="bullet">
    ///   <item><see cref="WithMetadata(IReadOnlyDictionary{string, object})"/> — batch overload, single Error copy.</item>
    ///   <item><see cref="WithMetadata(IEnumerable{KeyValuePair{string, object}})"/> — enumerable overload, single Error copy.</item>
    ///   <item><see cref="ToBuilder"/> → <see cref="ErrorBuilder.WithMetadata(string, object)"/> (repeated) → <see cref="ErrorBuilder.Build"/> — zero intermediate Error copies.</item>
    /// </list>
    /// </remarks>
    public Error WithMetadata(string key, object value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        // Use direct ToBuilder() on the ImmutableDictionary field to avoid intermediate allocation
        var builder = _metadata is not null
            ? _metadata.ToBuilder()
            : ImmutableDictionary.CreateBuilder<string, object>();
        builder[key] = value;
        return new Error(Code, Description, Type, Severity, Retryability, DescriptionKey, _traceIdOverride, _traceIdValue, CorrelationId, _innerErrors, builder.ToImmutable());
    }

    /// <summary>Creates a new <see cref="Error"/> copy containing the specified metadata entries.</summary>
    /// <param name="metadata">The dictionary of metadata entries to add. Cannot be null.</param>
    /// <returns>A new <see cref="Error"/> instance containing the merged metadata entries.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="metadata"/> is <see langword="null"/></exception>
    /// <remarks>
    /// This overload adds all entries in a single Error copy, using <c>ImmutableDictionary.Builder</c>
    /// to batch all additions before calling <c>ToImmutable()</c> once. Prefer this over chaining
    /// individual <see cref="WithMetadata(string, object)"/> calls when adding multiple entries.
    /// </remarks>
    public Error WithMetadata(IReadOnlyDictionary<string, object> metadata)
    {
        ArgumentNullException.ThrowIfNull(metadata);
        if (metadata.Count == 0) return this;
        var builder = _metadata is not null
            ? _metadata.ToBuilder()
            : ImmutableDictionary.CreateBuilder<string, object>();

        foreach (var kvp in metadata)
        {
            builder[kvp.Key] = kvp.Value;
        }

        return new Error(Code, Description, Type, Severity, Retryability, DescriptionKey, _traceIdOverride, _traceIdValue, CorrelationId, _innerErrors, builder.ToImmutable());
    }

    /// <summary>Creates a new <see cref="Error"/> copy containing the specified metadata entries from an enumerable source.</summary>
    /// <param name="metadata">The sequence of metadata entries to add. Cannot be null.</param>
    /// <returns>A new <see cref="Error"/> instance containing the merged metadata entries.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="metadata"/> is <see langword="null"/></exception>
    /// <remarks>
    /// This overload adds all entries in a single Error copy, using <c>ImmutableDictionary.Builder</c>
    /// to batch all additions before calling <c>ToImmutable()</c> once. Prefer this over chaining
    /// individual <see cref="WithMetadata(string, object)"/> calls when adding multiple entries.
    /// </remarks>
    public Error WithMetadata(IEnumerable<KeyValuePair<string, object>> metadata)
    {
        ArgumentNullException.ThrowIfNull(metadata);
        var builder = _metadata is not null
            ? _metadata.ToBuilder()
            : ImmutableDictionary.CreateBuilder<string, object>();
        bool added = false;
        foreach (var kvp in metadata)
        {
            builder[kvp.Key] = kvp.Value;
            added = true;
        }
        if (!added) return this;
        return new Error(Code, Description, Type, Severity, Retryability, DescriptionKey, _traceIdOverride, _traceIdValue, CorrelationId, _innerErrors, builder.ToImmutable());
    }

    /// <summary>
    /// Creates a new <see cref="Error"/> copy with the specified OpenTelemetry trace identifier string.
    /// This overrides both the ambient <see cref="Activity"/> trace ID and any previously captured value.
    /// </summary>
    /// <param name="traceId">The distributed trace identifier string, or <see langword="null"/> to clear the override.</param>
    /// <returns>A new <see cref="Error"/> instance with the updated trace identifier.</returns>
    /// <remarks>
    /// <b>⚠️ Note:</b> Passing <see langword="null"/> clears the trace ID override, removing it from
    /// serialized output and reverting to the ambient <see cref="Activity.Current"/> behavior.
    /// To explicitly clear the trace ID with a more intention-revealing call, use <see cref="ClearTraceId()"/>.
    /// <para>
    /// To set a strongly-typed <see cref="ActivityTraceId"/> directly without string allocation, use
    /// <see cref="WithTraceId(ActivityTraceId)"/>.
    /// </para>
    /// </remarks>
    public Error WithTraceId(string? traceId)
        => new(Code, Description, Type, Severity, Retryability, DescriptionKey, traceId, null, CorrelationId, _innerErrors, _metadata);

    /// <summary>
    /// Creates a new <see cref="Error"/> copy with the specified strongly-typed <see cref="ActivityTraceId"/>.
    /// Avoids string allocation compared to <see cref="WithTraceId(string?)"/>.
    /// </summary>
    /// <param name="traceId">The strongly-typed OpenTelemetry trace identifier.</param>
    /// <returns>A new <see cref="Error"/> instance with the updated trace identifier.</returns>
    public Error WithTraceId(ActivityTraceId traceId)
        => new(Code, Description, Type, Severity, Retryability, DescriptionKey, null, traceId, CorrelationId, _innerErrors, _metadata);

    /// <summary>
    /// Creates a new <see cref="Error"/> copy with the trace ID override cleared.
    /// After this call, <see cref="TraceId"/> will return the ambient
    /// <see cref="Activity.Current"/> trace ID (if any), or <see langword="null"/>.
    /// </summary>
    /// <returns>A new <see cref="Error"/> instance without a trace identifier override.</returns>
    /// <remarks>
    /// This is a more intention-revealing alternative to <c>WithTraceId(null)</c>.
    /// </remarks>
    public Error ClearTraceId()
        => new(Code, Description, Type, Severity, Retryability, DescriptionKey, null, null, CorrelationId, _innerErrors, _metadata);

    /// <summary>Creates a new <see cref="Error"/> copy with the specified correlation identifier.</summary>
    /// <param name="correlationId">The correlation identifier to associate with the error, or <see langword="null"/> to clear it.</param>
    /// <returns>A new <see cref="Error"/> instance with the updated correlation identifier.</returns>
    public Error WithCorrelationId(string? correlationId)
        => new(Code, Description, Type, Severity, Retryability, DescriptionKey, _traceIdOverride, _traceIdValue, correlationId, _innerErrors, _metadata);

    /// <summary>Creates a new <see cref="Error"/> copy with the specified localization description key.</summary>
    /// <param name="descriptionKey">The localization resource key, or <see langword="null"/> to clear it.</param>
    /// <returns>A new <see cref="Error"/> instance with the updated description key.</returns>
    public Error WithDescriptionKey(string? descriptionKey)
        => new(Code, Description, Type, Severity, Retryability, descriptionKey, _traceIdOverride, _traceIdValue, CorrelationId, _innerErrors, _metadata);

    /// <summary>Creates a new <see cref="Error"/> copy with the specified retryability classification.</summary>
    /// <param name="retryability">The retry classification to apply.</param>
    /// <returns>A new <see cref="Error"/> instance with the updated retryability classification.</returns>
    public Error WithRetryability(ErrorRetryability retryability)
        => new(Code, Description, Type, Severity, retryability, DescriptionKey, _traceIdOverride, _traceIdValue, CorrelationId, _innerErrors, _metadata);

    /// <summary>
    /// Creates an <see cref="ErrorBuilder"/> pre-seeded with all fields of this <see cref="Error"/>,
    /// allowing efficient construction of a modified copy without chaining multiple <c>With*</c> calls
    /// (which each copy the full Error state).
    /// </summary>
    /// <returns>An <see cref="ErrorBuilder"/> configured with this error's state.</returns>
    /// <example>
    /// <code>
    /// var enriched = existingError.ToBuilder()
    ///     .WithMetadata("requestId", requestId)
    ///     .WithMetadata("userId", userId)
    ///     .Build();
    /// </code>
    /// </example>
    [Pure]
    public ErrorBuilder ToBuilder() => ErrorBuilder.FromError(this);

    // ─── Equality ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Determines whether this error is semantically equal to another <see cref="Error"/> based on
    /// <see cref="Code"/>, <see cref="Description"/>, <see cref="Type"/>, <see cref="Severity"/>,
    /// and <see cref="Retryability"/>.
    /// </summary>
    /// <param name="other">The other <see cref="Error"/> to compare with this instance.</param>
    /// <returns><see langword="true"/> if both errors share the same semantic values; otherwise, <see langword="false"/>.</returns>
    /// <remarks>
    /// <b>This is a shallow equality check.</b> The following diagnostic/contextual fields are
    /// intentionally excluded because they vary per request and do not determine whether two errors
    /// represent the same logical failure:
    /// <see cref="TraceId"/>, <see cref="CorrelationId"/>,
    /// <see cref="DescriptionKey"/>, <see cref="InnerErrors"/>, and <see cref="Metadata"/>.
    /// <para>
    /// <see cref="Retryability"/> IS included because it is a semantic property of the error type
    /// itself, not a per-request identifier. Two errors that differ only in retry classification
    /// (e.g., one <see cref="ErrorRetryability.Transient"/>, one <see cref="ErrorRetryability.Permanent"/>)
    /// represent fundamentally different failure semantics and must not be considered equal in
    /// collections such as <see cref="HashSet{T}"/>.
    /// </para>
    /// <para>
    /// Use <see cref="StrictEquals"/> for deep structural equality that includes all fields
    /// including <see cref="TraceId"/>, <see cref="CorrelationId"/>, <see cref="Metadata"/>, and
    /// <see cref="InnerErrors"/>.
    /// </para>
    /// </remarks>
    public bool Equals(Error? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;

        return string.Equals(Code, other.Code, StringComparison.Ordinal)
               && string.Equals(Description, other.Description, StringComparison.Ordinal)
               && Type == other.Type
               && Severity == other.Severity
               && Retryability == other.Retryability;
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => Equals(obj as Error);

    /// <inheritdoc/>
    /// <remarks>
    /// <para>
    /// <b>⚠️ Collection semantics trap:</b> <see cref="GetHashCode"/> is computed from the same 5 fields as
    /// <see cref="Equals(Error?)"/>: <see cref="Code"/>, <see cref="Description"/>, <see cref="Type"/>,
    /// <see cref="Severity"/>, and <see cref="Retryability"/>. Fields like <see cref="TraceId"/>,
    /// <see cref="CorrelationId"/>, <see cref="Metadata"/>, and <see cref="InnerErrors"/> are NOT included.
    /// </para>
    /// <para>
    /// This means that a <see cref="HashSet{T}"/> or
    /// <see cref="Dictionary{TKey,TValue}"/> using the default
    /// <see cref="IEqualityComparer{T}"/> will treat two <see cref="Error"/> instances as
    /// <i>equal</i> (and deduplicate them) when they share the same 5 semantic fields but
    /// differ in trace ID or metadata. For example:
    /// <code>
    /// var e1 = Error.NotFound("Order.NotFound", "Order 1 not found").WithTraceId("trace-1");
    /// var e2 = Error.NotFound("Order.NotFound", "Order 2 not found");
    /// // If Code+Description+Type+Severity+Retryability match, HashSet deduplicates:
    /// var set = new HashSet&lt;Error&gt; { e1, e2 }; // might contain only one if Descriptions match!
    /// </code>
    /// This is particularly dangerous in a pipeline that calls <c>.Distinct()</c> or groups errors
    /// by code after a <see cref="Result.Combine(ReadOnlySpan{Result})"/> — errors that are
    /// semantically distinct (same code, different trace/metadata) will be silently deduplicated.
    /// </para>
    /// <para>
    /// Use <see cref="ErrorEqualityComparer"/> with <see cref="ErrorEqualityComparer.Strict"/> for
    /// deep structural equality in collections, or use <see cref="StrictEquals"/> when comparing
    /// individual error instances.
    /// </para>
    /// </remarks>
    public override int GetHashCode()
        => HashCode.Combine(Code, Description, Type, Severity, Retryability);

    /// <summary>
    /// Performs deep structural equality checking including all fields:
    /// <see cref="Code"/>, <see cref="Description"/>, <see cref="Type"/>, <see cref="Severity"/>,
    /// <see cref="Retryability"/>, <see cref="TraceId"/>, <see cref="CorrelationId"/>,
    /// <see cref="DescriptionKey"/>, <see cref="InnerErrors"/>, and <see cref="Metadata"/>.
    /// </summary>
    /// <param name="other">The other <see cref="Error"/> to compare with this instance.</param>
    /// <returns><see langword="true"/> if all fields and metadata match exactly; otherwise, <see langword="false"/>.</returns>
    /// <remarks>
    /// Delegates the shallow check (<see cref="Code"/>, <see cref="Description"/>, <see cref="Type"/>,
    /// <see cref="Severity"/>, and <see cref="Retryability"/>) to <see cref="Equals(Error?)"/> and
    /// additionally compares the diagnostic/contextual fields excluded from shallow equality.
    /// </remarks>
    [Pure]
    public bool StrictEquals(Error? other)
    {
        if (!Equals(other)) return false;

        // At this point Code, Description, Type, Severity, and Retryability are already verified
        // by Equals(). Only compare the diagnostic/contextual fields excluded from shallow equality.
        // Compare trace ID fields directly to avoid materializing the lazy
        // ActivityTraceId.ToString() string via the TraceId property.
        bool isDiff = IsStrictContextDifferent(other!);
        if (isDiff)
            return false;

        if (HasInnerErrors != other.HasInnerErrors || HasMetadata != other.HasMetadata)
            return false;

        if (HasInnerErrors)
        {
            // Index loop to avoid enumerator allocation.
            // ImmutableArray<Error> exposes .Length directly as a struct property — no interface boxing.
            var a = InnerErrors;
            var b = other.InnerErrors;
            if (a.Length != b.Length) return false;
            for (int i = 0; i < a.Length; i++)
            {
                if (!a[i].StrictEquals(b[i])) return false;
            }
        }

        if (HasMetadata)
        {
            // Manual foreach to avoid SequenceEqual on KeyValuePair<string, object> which
            // would require IEqualityComparer<KeyValuePair<string, object>> boxing.
            var a = Metadata;
            var b = other.Metadata;
            if (a.Count != b.Count) return false;
            foreach (var kvp in a)
            {
                if (!b.TryGetValue(kvp.Key, out var bVal) || !Equals(kvp.Value, bVal))
                    return false;
            }
        }

        return true;
    }

    /// <summary>Determines whether two <see cref="Error"/> instances are semantically equal.</summary>
    /// <param name="left">The first error to compare.</param>
    /// <param name="right">The second error to compare.</param>
    /// <returns><see langword="true"/> if both instances are equal; otherwise, <see langword="false"/>.</returns>
    /// <remarks>
    /// Checks <c>ReferenceEquals</c> first (fast path for same-instance comparisons),
    /// then delegates to <see cref="Equals(Error?)"/> for field-by-field shallow equality.
    /// </remarks>
    public static bool operator ==(Error? left, Error? right)
    {
        if (ReferenceEquals(left, right)) return true;
        if (left is null || right is null) return false;
        return left.Equals(right);
    }

    /// <summary>Determines whether two <see cref="Error"/> instances are not equal.</summary>
    /// <param name="left">The first error to compare.</param>
    /// <param name="right">The second error to compare.</param>
    /// <returns><see langword="true"/> if the instances are not equal; otherwise, <see langword="false"/>.</returns>
    public static bool operator !=(Error? left, Error? right)
    {
        if (ReferenceEquals(left, right)) return false;
        if (left is null || right is null) return true;
        return !left.Equals(right);
    }

    /// <inheritdoc/>
    public override string ToString() => $"[{Type}] {Code}: {Description}";

    private bool IsStrictContextDifferent(Error other)
    {
        return DescriptionKey != other.DescriptionKey
            || _traceIdOverride != other._traceIdOverride
            || _traceIdValue != other._traceIdValue
            || CorrelationId != other.CorrelationId;
    }
}
