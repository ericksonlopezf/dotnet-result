// Copyright © Erickson Lopez. MIT License.
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;

namespace EricksonLopez.Result;

/// <summary>
/// A fluent, stack-allocated builder for constructing <see cref="Error"/> instances with multiple optional properties.
/// The builder struct itself incurs no heap allocation; however, <see cref="WithMetadata(string, object)"/> and
/// <see cref="WithInnerError"/> allocate heap nodes in the backing <see cref="System.Collections.Immutable.ImmutableDictionary{TKey,TValue}"/>
/// and <see cref="System.Collections.Immutable.ImmutableArray{T}"/> respectively (O(log k) per entry for metadata).
/// Prefer this over the <see cref="Error"/> constructor when setting more than two optional fields,
/// or use <see cref="Error.ToBuilder"/> to create a modified copy of an existing <see cref="Error"/>.
/// </summary>
/// <remarks>
/// <para>
/// <b>Value type semantics:</b> <see cref="ErrorBuilder"/> is a <c>readonly struct</c> to avoid heap allocation.
/// Each <c>With*</c> method returns a modified <em>copy</em> of the builder. Always use it in a fluent chain
/// or reassign the return value:
/// <code>
/// var error = Error.Create("Order.Expired", "The order has expired.")
///     .WithType(ErrorType.Domain)
///     .WithSeverity(ErrorSeverity.Warning)
///     .WithRetryability(ErrorRetryability.Permanent)
///     .WithCorrelationId(correlationId)
///     .WithMetadata("orderId", orderId)
///     .Build();
/// </code>
/// </para>
/// <para>
/// <b>⚠ Copy cost per <c>With*()</c> call:</b> <see cref="ErrorBuilder"/> is approximately 96–104 bytes
/// in size (11 fields including an <c>ImmutableDictionary</c> reference, an <c>ImmutableArray</c>
/// reference, two string references for trace ID, and several enum byte fields). Each <c>With*()</c>
/// call copies the entire struct to produce a new instance. A fluent chain of N calls copies the struct
/// N times (e.g., a 5-step chain copies ~500 bytes total on the stack). This is cheaper than N heap
/// allocations but is measurably more expensive than a single mutation. For hot paths where error
/// construction occurs in a tight loop, consider:
/// <list type="bullet">
///   <item>Pre-building the <see cref="Error"/> once (outside the loop) if the error is reused.</item>
///   <item>Using <see cref="WithMetadata(IEnumerable{KeyValuePair{string, object}})"/>
///   to batch multiple metadata entries in a single call instead of chaining individual
///   <c>WithMetadata(string, object)</c> calls.</item>
///   <item>Using <see cref="WithInnerErrors(IEnumerable{Error})"/> instead of
///   multiple <see cref="WithInnerError(Error)"/> calls to avoid O(n²) <c>ImmutableArray</c> copying.</item>
/// </list>
/// For non-hot-path code (startup, exception handling, validation), the copy cost is negligible.
/// </para>
/// <para>
/// Because this is a <c>readonly struct</c>, discarding the return value of a <c>With*</c> method
/// has no effect on the builder instance — the mutated copy is silently lost.
/// The <c>RESULT003</c> Roslyn analyzer (from the <c>EricksonLopez.Result.Analyzers</c> package)
/// detects this pattern and reports an error diagnostic. A companion <c>CodeFixProvider</c>
/// provides an Alt+Enter fix to rewrite the statement as an assignment.
/// Without the analyzer installed, discarding a <c>With*</c> return value compiles without any diagnostic.
/// </para>
/// <para>
/// Obtain an instance via <see cref="Error.Create(string, string)"/> for new errors,
/// or via <see cref="Error.ToBuilder"/> to mutate an existing error efficiently.
/// </para>
/// </remarks>
public readonly struct ErrorBuilder
{
    private readonly string _code;
    private readonly string _description;
    private readonly ErrorType _type;
    private readonly ErrorSeverity _severity;
    private readonly ErrorRetryability _retryability;
    private readonly string? _descriptionKey;
    private readonly string? _traceId;               // string override (user-supplied or pre-materialized)
    private readonly ActivityTraceId? _traceIdValue; // raw struct — avoids ToString() allocation until Build()
    private readonly string? _correlationId;
    // ImmutableArray<Error> avoids the O(n²) copy-on-write cost of List<Error>.
    // Add() on ImmutableArray uses a builder internally with amortized O(1) per call.
    // IsDefault (not IsEmpty) is used to represent "no inner errors" so that an empty list
    // is distinguishable from the unset state when _innerErrors.IsDefault == true.
    private readonly ImmutableArray<Error> _innerErrors;
    private readonly ImmutableDictionary<string, object> _metadata;

    internal ErrorBuilder(string code, string description)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        ArgumentException.ThrowIfNullOrWhiteSpace(description);
        _code = code;
        _description = description;
        _type = ErrorType.Failure;
        _severity = ErrorSeverity.Error;
        _retryability = ErrorRetryability.NotApplicable;
        _metadata = ImmutableDictionary<string, object>.Empty;
    }

    // Private ctor used by With* methods and FromError to create a modified copy without re-validating.
    private ErrorBuilder(
        string code,
        string description,
        ErrorType type,
        ErrorSeverity severity,
        ErrorRetryability retryability,
        string? descriptionKey,
        string? traceId,
        ActivityTraceId? traceIdValue,
        string? correlationId,
        ImmutableArray<Error> innerErrors,
        ImmutableDictionary<string, object> metadata)
    {
        _code = code;
        _description = description;
        _type = type;
        _severity = severity;
        _retryability = retryability;
        _descriptionKey = descriptionKey;
        _traceId = traceId;
        _traceIdValue = traceIdValue;
        _correlationId = correlationId;
        _innerErrors = innerErrors;
        _metadata = metadata;
    }

    /// <summary>
    /// Creates an <see cref="ErrorBuilder"/> pre-seeded from an existing <see cref="Error"/>.
    /// Called by <see cref="Error.ToBuilder"/>.
    /// </summary>
    /// <remarks>
    /// Uses internal Error accessors to propagate the raw <c>ActivityTraceId</c> struct without calling
    /// <see cref="ActivityTraceId.ToString"/>, avoiding a heap allocation when the error was captured from
    /// <see cref="Activity.Current"/> and no builder modification of the trace ID is needed.
    /// The <c>ActivityTraceId</c> struct is materialized to a <see langword="string"/> only in
    /// <see cref="Build"/> when the final <see cref="Error"/> is constructed.
    /// </remarks>
    internal static ErrorBuilder FromError(Error source)
    {
        // Prefer string override first (no allocation). If only an ActivityTraceId struct is present,
        // store it as-is — defer ToString() until Build() time.
        //
        // Fast-path: RawInnerErrors is now always ImmutableArray<Error> (no longer nullable),
        // so we always get the backing array directly with zero allocation.
        return new ErrorBuilder(
            source.Code,
            source.Description,
            source.Type,
            source.Severity,
            source.Retryability,
            source.DescriptionKey,
            traceId: source.TraceIdOverride,     // null if only struct value present
            traceIdValue: source.TraceIdValue,   // raw struct — no .ToString() allocation
            source.CorrelationId,
            source.RawInnerErrors,               // always ImmutableArray<Error>, zero copy
            source.RawMetadata ?? ImmutableDictionary<string, object>.Empty);
    }

    /// <summary>Sets the category type for the error being built.</summary>
    /// <param name="type">The error category type to assign.</param>
    /// <returns>A new <see cref="ErrorBuilder"/> instance with the specified error type.</returns>
    public ErrorBuilder WithType(ErrorType type)
        => new(_code, _description, type, _severity, _retryability, _descriptionKey, _traceId, _traceIdValue, _correlationId, _innerErrors, _metadata);

    /// <summary>Sets the severity level for the error being built.</summary>
    /// <param name="severity">The error severity level to assign.</param>
    /// <returns>A new <see cref="ErrorBuilder"/> instance with the specified severity level.</returns>
    public ErrorBuilder WithSeverity(ErrorSeverity severity)
        => new(_code, _description, _type, severity, _retryability, _descriptionKey, _traceId, _traceIdValue, _correlationId, _innerErrors, _metadata);

    /// <summary>Sets the retry classification for the error being built.</summary>
    /// <param name="retryability">The retry classification to assign.</param>
    /// <returns>A new <see cref="ErrorBuilder"/> instance with the specified retryability.</returns>
    public ErrorBuilder WithRetryability(ErrorRetryability retryability)
        => new(_code, _description, _type, _severity, retryability, _descriptionKey, _traceId, _traceIdValue, _correlationId, _innerErrors, _metadata);

    /// <summary>Sets an explicit distributed trace identifier string on the error being built.</summary>
    /// <param name="traceId">The distributed trace identifier string, or <see langword="null"/> to clear the override.</param>
    /// <returns>A new <see cref="ErrorBuilder"/> instance with the specified trace identifier.</returns>
    /// <remarks>
    /// Prefer the <see cref="WithTraceId(ActivityTraceId)"/> overload when working with
    /// <see cref="Activity.Current"/> to avoid a premature <see cref="ActivityTraceId.ToString"/> allocation.
    /// </remarks>
    public ErrorBuilder WithTraceId(string? traceId)
        => new(_code, _description, _type, _severity, _retryability, _descriptionKey, traceId, traceIdValue: null, _correlationId, _innerErrors, _metadata);

    /// <summary>
    /// Sets a strongly-typed <see cref="ActivityTraceId"/> on the error being built without
    /// allocating a string — the struct is materialized to a string only when <see cref="Build"/> is called.
    /// </summary>
    /// <param name="traceId">The strongly-typed OpenTelemetry trace identifier.</param>
    /// <returns>A new <see cref="ErrorBuilder"/> instance with the specified trace identifier.</returns>
    public ErrorBuilder WithTraceId(ActivityTraceId traceId)
        => new(_code, _description, _type, _severity, _retryability, _descriptionKey, traceId: null, traceIdValue: traceId, _correlationId, _innerErrors, _metadata);

    /// <summary>Sets the distributed correlation identifier on the error being built.</summary>
    /// <param name="correlationId">The correlation identifier to associate with the error, or <see langword="null"/> to clear it.</param>
    /// <returns>A new <see cref="ErrorBuilder"/> instance with the specified correlation identifier.</returns>
    public ErrorBuilder WithCorrelationId(string? correlationId)
        => new(_code, _description, _type, _severity, _retryability, _descriptionKey, _traceId, _traceIdValue, correlationId, _innerErrors, _metadata);

    /// <summary>Sets the localization resource key on the error being built.</summary>
    /// <param name="descriptionKey">The localization resource key, or <see langword="null"/> to clear it.</param>
    /// <returns>A new <see cref="ErrorBuilder"/> instance with the specified description key.</returns>
    public ErrorBuilder WithDescriptionKey(string? descriptionKey)
        => new(_code, _description, _type, _severity, _retryability, descriptionKey, _traceId, _traceIdValue, _correlationId, _innerErrors, _metadata);

    /// <summary>Adds or updates a single metadata key-value pair on the error being built.</summary>
    /// <param name="key">The metadata key to add or update.</param>
    /// <param name="value">The metadata value to store.</param>
    /// <returns>A new <see cref="ErrorBuilder"/> instance containing the updated metadata.</returns>
    public ErrorBuilder WithMetadata(string key, object value)
        => new(_code, _description, _type, _severity, _retryability, _descriptionKey, _traceId, _traceIdValue, _correlationId, _innerErrors, _metadata.SetItem(key, value));

    /// <summary>Adds multiple metadata key-value pairs in a single operation, avoiding repeated intermediate allocations.</summary>
    /// <param name="entries">The collection of metadata entries to add.</param>
    /// <returns>A new <see cref="ErrorBuilder"/> instance containing the merged metadata.</returns>
    public ErrorBuilder WithMetadata(IEnumerable<KeyValuePair<string, object>> entries)
        => new(_code, _description, _type, _severity, _retryability, _descriptionKey, _traceId, _traceIdValue, _correlationId, _innerErrors, _metadata.SetItems(entries));

    /// <summary>Appends a child error to the error being built.</summary>
    /// <param name="innerError">The child error to add.</param>
    /// <returns>A new <see cref="ErrorBuilder"/> instance containing the added child error.</returns>
    /// <remarks>
    /// Uses <see cref="ImmutableArray{T}.Add"/> which creates a new array of size n+1 and copies
    /// all existing elements — this is <b>O(n) per call</b>, resulting in O(n²) total cost when
    /// chaining multiple <c>WithInnerError</c> calls. For bulk inner error construction, prefer
    /// <see cref="WithInnerErrors"/> which uses <see cref="ImmutableArray{T}.AddRange"/> for a
    /// single copy operation.
    /// <para>
    /// This is still preferable to the previous <c>new List&lt;Error&gt;(existing)</c> approach
    /// because ImmutableArray avoids the mutable List overhead and integrates cleanly with the
    /// copy-on-write ErrorBuilder semantics.
    /// </para>
    /// </remarks>
    public ErrorBuilder WithInnerError(Error innerError)
    {
        var newInnerErrors = _innerErrors.IsDefaultOrEmpty
            ? ImmutableArray.Create(innerError)
            : _innerErrors.Add(innerError);
        return new(_code, _description, _type, _severity, _retryability, _descriptionKey, _traceId, _traceIdValue, _correlationId, newInnerErrors, _metadata);
    }

    /// <summary>Appends a collection of child errors to the error being built.</summary>
    /// <param name="innerErrors">The collection of child errors to add.</param>
    /// <returns>A new <see cref="ErrorBuilder"/> instance containing the added child errors.</returns>
    /// <remarks>
    /// Uses <see cref="ImmutableArray{T}.AddRange"/> to append all elements in a single
    /// structural operation, avoiding repeated intermediate copies.
    /// </remarks>
    public ErrorBuilder WithInnerErrors(IEnumerable<Error> innerErrors)
    {
        var newInnerErrors = _innerErrors.IsDefaultOrEmpty
            ? ImmutableArray.CreateRange(innerErrors)
            : _innerErrors.AddRange(innerErrors);
        return new(_code, _description, _type, _severity, _retryability, _descriptionKey, _traceId, _traceIdValue, _correlationId, newInnerErrors, _metadata);
    }

    /// <summary>
    /// Constructs and returns the configured immutable <see cref="Error"/> instance.
    /// </summary>
    /// <returns>A new <see cref="Error"/> instance configured with the builder state.</returns>
    /// <remarks>
    /// The builder can be called multiple times; each call creates a new <see cref="Error"/> snapshot.
    /// The builder state is not mutated by <c>Build()</c>.
    /// <para>
    /// Uses an internal fast path (<c>Error.CreateFromBuilder</c>) that skips argument validation
    /// (already enforced by the factory method that created the builder) and avoids re-capturing
    /// <see cref="Activity.Current"/>. If the builder holds a raw <see cref="ActivityTraceId"/>
    /// struct (from <see cref="FromError"/> or <see cref="WithTraceId(ActivityTraceId)"/>),
    /// it is materialized to a string at this point — exactly once per <c>Build()</c> call.
    /// </para>
    /// </remarks>
    public Error Build()
    {
        string? traceId = _traceId;
        if (traceId is null && _traceIdValue is { } traceStruct)
        {
            traceId = traceStruct.ToString();
        }

        return Error.CreateFromBuilder(
            _code, _description, _type, _severity, _retryability, _descriptionKey,
            traceId,
            _correlationId,
            _innerErrors,
            _metadata);
    }
}


