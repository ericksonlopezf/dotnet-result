using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace EricksonLopez.Result;

/// <summary>
/// Represents the result of an operation that returns a value of type <typeparamref name="TValue"/>.
/// The state can be either Success (containing the value) or Failure (containing an <see cref="Error"/>).
/// </summary>
/// <remarks>
/// <para>
/// <b>Performance Note:</b> Because this is a `readonly struct`, the entire struct is copied by value
/// when passed around. If <typeparamref name="TValue"/> is a large value type (like a large struct
/// or tuple), this can incur significant copy overhead during method calls or when chaining methods.
/// </para>
/// </remarks>
/// <typeparam name="TValue">The type of the successful value.</typeparam>
[DebuggerDisplay("{GetDebuggerDisplay(),nq}")]
[StructLayout(LayoutKind.Auto)]
public readonly partial struct Result<TValue> : IResultOutcome, IEquatable<Result<TValue>>
{
    private readonly ResultState _state;
    private readonly TValue? _value;
    private readonly Error? _error;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal Result(ResultState state, TValue? value, Error? error)
    {
        _state = state;
        _value = value;
        _error = error;
    }

    /// <summary>Gets a value indicating whether the operation succeeded.</summary>
    /// <remarks>
    /// <b>⚠ Uninitialized default:</b> Returns <see langword="false"/> for an uninitialized
    /// <c>default(Result&lt;TValue&gt;)</c> — the same as a failure, but without an error.
    /// Always construct results via <see cref="Result.Success{TValue}(TValue)"/> or <see cref="Result.Failure{TValue}(Error)"/>.
    /// Use <see cref="IsUninitialized"/> to detect an uninitialized default explicitly.
    /// </remarks>
    public bool IsSuccess => _state == ResultState.Success;

    /// <summary>Gets a value indicating whether the operation failed.</summary>
    /// <remarks>
    /// <b>⚠ Uninitialized default:</b> Returns <see langword="false"/> for an uninitialized
    /// <c>default(Result&lt;TValue&gt;)</c> — the same as a success (neither true nor false
    /// corresponds to the uninitialized state). Use <see cref="IsUninitialized"/> to distinguish.
    /// </remarks>
    public bool IsFailure => _state == ResultState.Failure;

    /// <summary>Gets a value indicating whether the struct is an uninitialized default value.</summary>
    public bool IsUninitialized => _state == ResultState.Uninitialized;


    /// <summary>
    /// The success value. Throws if accessed on a failed or uninitialized result.
    /// </summary>
    /// <exception cref="InvalidOperationException">When accessed on a failure or uninitialized result.</exception>
    // Stryker disable String : Exception messages
    public TValue Value => _state switch
    {
        ResultState.Success => _value!,
        ResultState.Failure => throw new InvalidOperationException($"Cannot access Value of a failed result. Error: {_error}"),
        _ => throw new InvalidOperationException("Cannot access Value on an uninitialized default Result<T>.")
    };
    // Stryker restore String

    /// <summary>
    /// Gets the error associated with this result.
    /// Throws an InvalidOperationException if the result is successful.
    /// </summary>
    // Stryker disable String : Exception messages
    public Error Error => _state switch
    {
        ResultState.Failure => _error!,
        ResultState.Success => throw new InvalidOperationException("Cannot access the Error of a successful result."),
        _ => WellKnownErrors.UninitializedError
    };
    // Stryker restore String

    // ─── Factory methods ──────────────────────────────────────────────────────

    /// <summary>
    /// Creates a success result wrapping <paramref name="value"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Null values:</b> If <typeparamref name="TValue"/> is a nullable reference type
    /// (e.g., <c>Result&lt;string?&gt;</c>), <see langword="null"/> is a valid success value
    /// and will not throw. The .NET nullable type system provides compile-time protection
    /// for non-nullable reference types; no runtime null check is applied.
    /// </para>
    /// <para>
    /// For value types, <see langword="null"/> is prevented at compile time by the type system.
    /// </para>
    /// </remarks>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result<TValue> Success(TValue value) => new(ResultState.Success, value, null);

    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result<TValue> Failure(Error error) => new(ResultState.Failure, default, error ?? throw new ArgumentNullException(nameof(error)));

    // ─── Monadic operations ───────────────────────────────────────────────────

    [Pure]
    public Result<TNext> Map<TNext>(Func<TValue, TNext> mapper)
    {
        ThrowIfUninitialized();
        return _state == ResultState.Success ? Result.Success(mapper(_value!)) : Result.Failure<TNext>(_error!);
    }

    [Pure]
    public Result<TNext> Map<TState, TNext>(TState state, Func<TState, TValue, TNext> mapper)
    {
        ThrowIfUninitialized();
        return _state == ResultState.Success ? Result.Success(mapper(state, _value!)) : Result.Failure<TNext>(_error!);
    }

    [Pure]
    public Result<TNext> Bind<TNext>(Func<TValue, Result<TNext>> bind)
    {
        ThrowIfUninitialized();
        return _state == ResultState.Success ? bind(_value!) : Result.Failure<TNext>(_error!);
    }

    [Pure]
    public Result<TNext> Bind<TState, TNext>(TState state, Func<TState, TValue, Result<TNext>> bind)
    {
        ThrowIfUninitialized();
        return _state == ResultState.Success ? bind(state, _value!) : Result.Failure<TNext>(_error!);
    }

    [Pure]
    public Result Bind(Func<TValue, Result> bind)
    {
        ThrowIfUninitialized();
        return _state == ResultState.Success ? bind(_value!) : Result.Failure(_error!);
    }

    [Pure]
    public Result Bind<TState>(TState state, Func<TState, TValue, Result> bind)
    {
        ThrowIfUninitialized();
        return _state == ResultState.Success ? bind(state, _value!) : Result.Failure(_error!);
    }

    // ─── Match & Switch ───────────────────────────────────────────────────────

    [Pure]
    public TOut Match<TOut>(Func<TValue, TOut> onSuccess, Func<Error, TOut> onFailure)
    {
        ThrowIfUninitialized();
        return _state == ResultState.Success ? onSuccess(_value!) : onFailure(_error!);
    }

    [Pure]
    public TOut Match<TState, TOut>(TState state, Func<TState, TValue, TOut> onSuccess, Func<TState, Error, TOut> onFailure)
    {
        ThrowIfUninitialized();
        return _state == ResultState.Success ? onSuccess(state, _value!) : onFailure(state, _error!);
    }

    /// <summary>
    /// Invokes <paramref name="onSuccess"/> with the value when successful,
    /// or <paramref name="onFailure"/> with the error when failed — without returning a value.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <c>Execute</c> is the void side-effect counterpart to <see cref="Match{TOut}"/>:
    /// use <c>Match</c> when you need a return value, and <c>Execute</c> for pure side-effects
    /// (logging, dispatching events, updating state).
    /// </para>
    /// <para>
    /// <b>Naming note:</b> The name <c>Execute</c> is borrowed from functional programming.
    /// See <see cref="Result.Execute(Action, Action{Error})"/> for the full rationale.
    /// </para>
    /// </remarks>
    public void Execute(Action<TValue> onSuccess, Action<Error> onFailure)
    {
        ThrowIfUninitialized();
        // Stryker disable once all : Equivalent mutation
        if (_state == ResultState.Success) onSuccess(_value!);
        else onFailure(_error!);
    }

    /// <summary>
    /// Invokes <paramref name="onSuccess"/> or <paramref name="onFailure"/> for their side-effects,
    /// forwarding <paramref name="state"/> to avoid a closure allocation.
    /// </summary>
    /// <remarks>
    /// Use this overload in hot paths where capturing variables in a closure would cause heap allocation.
    /// </remarks>
    public void Execute<TState>(TState state, Action<TState, TValue> onSuccess, Action<TState, Error> onFailure)
    {
        ThrowIfUninitialized();
        if (_state == ResultState.Success) onSuccess(state, _value!);
        else onFailure(state, _error!);
    }

    /// <summary>
    /// Maps a failure <see cref="Error"/> to a value of type <typeparamref name="TOut"/>,
    /// or returns <paramref name="successDefault"/> when this result is successful.
    /// </summary>
    /// <typeparam name="TOut">The output type of the transformation.</typeparam>
    /// <param name="onFailure">A function that produces a <typeparamref name="TOut"/> from the <see cref="Error"/>.</param>
    /// <param name="successDefault">The value to return when the result is successful.</param>
    /// <returns>
    /// The result of invoking <paramref name="onFailure"/> on failure, or <paramref name="successDefault"/> on success.
    /// </returns>
    /// <remarks>
    /// Use this when you only need to handle the failure branch without mapping the success value.
    /// For transforming both branches, use <see cref="Match{TOut}(Func{TValue, TOut}, Func{Error, TOut})"/>.
    /// <para>
    /// <b>💡 Allocation tip:</b> If <paramref name="onFailure"/> captures variables from an outer scope,
    /// use <c>MapFailure&lt;TState, TOut&gt;(TState, Func&lt;TState, Error, TOut&gt;, TOut)</c>
    /// to pass the captured values as a <c>TState</c> parameter and avoid closure allocation.
    /// </para>
    /// </remarks>
    [Pure]
    public TOut MapFailure<TOut>(Func<Error, TOut> onFailure, TOut successDefault)
    {
        // Stryker disable once all : Equivalent mutation
        ThrowIfUninitialized();
        return _state == ResultState.Failure ? onFailure(_error!) : successDefault;
    }

    /// <summary>
    /// Maps a failure <see cref="Error"/> to a value of type <typeparamref name="TOut"/> using
    /// captured <paramref name="state"/> to avoid closure allocation.
    /// Returns <paramref name="successDefault"/> when this result is successful.
    /// </summary>
    [Pure]
    public TOut MapFailure<TState, TOut>(TState state, Func<TState, Error, TOut> onFailure, TOut successDefault)
    {
        // Stryker disable once all : Equivalent mutation
        ThrowIfUninitialized();
        return _state == ResultState.Failure ? onFailure(state, _error!) : successDefault;
    }

    /// <summary>Obsolete. Use <see cref="MapFailure{TOut}(Func{Error, TOut}, TOut)"/> instead.</summary>
    [Pure]
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    [Obsolete("FoldError is replaced by MapFailure, which provides identical semantics with a more discoverable .NET-idiomatic name. Replace FoldError(onFailure, successDefault) with MapFailure(onFailure, successDefault).", error: true)]
    public TOut FoldError<TOut>(Func<Error, TOut> onFailure, TOut successDefault)
        => MapFailure(onFailure, successDefault);

    /// <summary>Obsolete. Use <see cref="MapFailure{TState, TOut}(TState, Func{TState, Error, TOut}, TOut)"/> instead.</summary>
    [Pure]
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    [Obsolete("FoldError is replaced by MapFailure, which provides identical semantics with a more discoverable .NET-idiomatic name. Replace FoldError(state, onFailure, successDefault) with MapFailure(state, onFailure, successDefault).", error: true)]
    public TOut FoldError<TState, TOut>(TState state, Func<TState, Error, TOut> onFailure, TOut successDefault)
        => MapFailure(state, onFailure, successDefault);

    // ——— Side effects —————————————————————————————————————————————————————————

    /// <summary>
    /// Executes <paramref name="action"/> if this result is successful, then returns this result unchanged.
    /// </summary>
    /// <remarks>
    /// This method executes the action <b>only on success</b> and is symmetric with <see cref="TapOnFailure(Action{Error})"/>.
    /// Use <see cref="Inspect(Action{Result{TValue}})"/> for unconditional execution (both success and failure).
    /// <para>
    /// <b>💡 Allocation tip:</b> Use <c>TapOnSuccess&lt;TState&gt;(TState, Action&lt;TState, TValue&gt;)</c> to avoid
    /// closure allocations when capturing outer variables.
    /// </para>
    /// </remarks>
    public Result<TValue> TapOnSuccess(Action<TValue> action)
    {
        ThrowIfUninitialized();
        if (_state == ResultState.Success) action(_value!);
        return this;
    }

    /// <remarks>
    /// <b>💡 Allocation tip:</b> This overload passes context via <paramref name="state"/> instead of
    /// a captured closure.
    /// </remarks>
    public Result<TValue> TapOnSuccess<TState>(TState state, Action<TState, TValue> action)
    {
        ThrowIfUninitialized();
        if (_state == ResultState.Success) action(state, _value!);
        return this;
    }

    /// <summary>Executes <paramref name="action"/> if this result is a failure, then returns this result unchanged.</summary>
    /// <remarks>
    /// This method executes the action <b>only on failure</b> and is symmetric with <see cref="TapOnSuccess(Action{TValue})"/>.
    /// <para>
    /// <b>💡 Allocation tip:</b> Use <c>TapOnFailure&lt;TState&gt;(TState, Action&lt;TState, Error&gt;)</c> to avoid
    /// closure allocations when capturing outer variables.
    /// </para>
    /// </remarks>
    public Result<TValue> TapOnFailure(Action<Error> action)
    {
        ThrowIfUninitialized();
        if (_state == ResultState.Failure) action(_error!);
        return this;
    }

    /// <summary>Executes <paramref name="action"/> with captured state if this result is a failure, then returns this result unchanged.</summary>
    /// <remarks>
    /// <b>💡 Allocation tip:</b> This overload passes context via <paramref name="state"/> instead of a captured closure.
    /// </remarks>
    public Result<TValue> TapOnFailure<TState>(TState state, Action<TState, Error> action)
    {
        ThrowIfUninitialized();
        if (_state == ResultState.Failure) action(state, _error!);
        return this;
    }

    // ─── Composition ──────────────────────────────────────────────────────────

    public Result<TValue> Ensure(Func<TValue, bool> predicate, Error error)
    {
        ThrowIfUninitialized();
        if (_state != ResultState.Success) return this;
        return predicate(_value!) ? this : Failure(error);
    }
    public Result<TValue> Ensure<TState>(TState state, Func<TState, TValue, bool> predicate, Error error)
    {
        ThrowIfUninitialized();
        if (_state != ResultState.Success) return this;
        return predicate(state, _value!) ? this : Failure(error);
    }

    /// <summary>
    /// Validates a successful result using <paramref name="predicate"/>, constructing the error lazily
    /// via <paramref name="errorFactory"/> only when the predicate fails.
    /// </summary>
    /// <remarks>
    /// Prefer this overload over <see cref="Ensure(Func{TValue, bool}, Error)"/> when error construction is
    /// expensive (e.g., captures <see cref="System.Diagnostics.Activity.Current"/>, allocates metadata,
    /// or involves string formatting). The error factory is never invoked on the success path.
    /// </remarks>
    public Result<TValue> Ensure(Func<TValue, bool> predicate, Func<Error> errorFactory)
    {
        ThrowIfUninitialized();
        if (_state != ResultState.Success) return this;
        return predicate(_value!) ? this : Failure(errorFactory());
    }

    /// <summary>
    /// Validates a successful result using <paramref name="predicate"/> with captured state,
    /// constructing the error lazily via <paramref name="errorFactory"/> only when the predicate fails.
    /// </summary>
    public Result<TValue> Ensure<TState>(TState state, Func<TState, TValue, bool> predicate, Func<Error> errorFactory)
    {
        ThrowIfUninitialized();
        if (_state != ResultState.Success) return this;
        return predicate(state, _value!) ? this : Failure(errorFactory());
    }

    /// <summary>
    /// Validates a successful result using <paramref name="predicate"/>, constructing the error lazily
    /// via <paramref name="errorFactory"/> only when the predicate fails. The error factory receives
    /// the current value to allow contextual error messages.
    /// </summary>
    /// <remarks>
    /// Use this overload when the error message depends on the value, for example:
    /// <code>
    /// result.Ensure(
    ///     order => order.Total > 0,
    ///     order => Error.Validation("Order.InvalidTotal", $"Order {order.Id} has invalid total {order.Total}"));
    /// </code>
    /// For error construction that does not need the value, use
    /// <see cref="Ensure(Func{TValue, bool}, Func{Error})"/> instead.
    /// <para>
    /// <b>💡 Allocation tip:</b> Use <c>Ensure&lt;TState&gt;(TState, Func&lt;TState, TValue, bool&gt;, Func&lt;TState, TValue, Error&gt;)</c>
    /// to pass external context without a closure.
    /// </para>
    /// </remarks>
    public Result<TValue> Ensure(Func<TValue, bool> predicate, Func<TValue, Error> errorFactory)
    {
        ThrowIfUninitialized();
        if (_state != ResultState.Success) return this;
        return predicate(_value!) ? this : Failure(errorFactory(_value!));
    }

    /// <summary>
    /// Validates a successful result using <paramref name="predicate"/> with captured state,
    /// constructing the error lazily via <paramref name="errorFactory"/> only when the predicate fails.
    /// The error factory receives both the state and the current value.
    /// </summary>
    public Result<TValue> Ensure<TState>(TState state, Func<TState, TValue, bool> predicate, Func<TState, TValue, Error> errorFactory)
    {
        ThrowIfUninitialized();
        if (_state != ResultState.Success) return this;
        return predicate(state, _value!) ? this : Failure(errorFactory(state, _value!));
    }

    // ─── Recovery ─────────────────────────────────────────────────────────────

    /// <summary>If this result is a failure, invokes <paramref name="recovery"/> to attempt a corrective result.</summary>
    [Pure]
    public Result<TValue> Recover(Func<Error, Result<TValue>> recovery)
    {
        ThrowIfUninitialized();
        return _state == ResultState.Failure ? recovery(_error!) : this;
    }

    /// <summary>If this result is a failure, invokes <paramref name="recovery"/> with captured state to attempt a corrective result.</summary>
    [Pure]
    public Result<TValue> Recover<TState>(TState state, Func<TState, Error, Result<TValue>> recovery)
    {
        ThrowIfUninitialized();
        return _state == ResultState.Failure ? recovery(state, _error!) : this;
    }

    /// <summary>Gets the error associated with a failed result, or null if successful.</summary>
    // Stryker disable once all : Equivalent mutation
    Error? IResultOutcome.Error => _state == ResultState.Failure ? _error : null;

    /// <summary>Gets the raw underlying value of a successful result, or null if failed.</summary>
    object? IResultOutcome.RawValue => _state == ResultState.Success ? _value : null;

    // ─── Inspection ───────────────────────────────────────────────────────────

    public Result<TValue> Inspect(Action<Result<TValue>> action)
    {
        ThrowIfUninitialized();
        action(this);
        return this;
    }
    public Result<TValue> Inspect<TState>(TState state, Action<TState, Result<TValue>> action)
    {
        ThrowIfUninitialized();
        action(state, this);
        return this;
    }

    [Obsolete("Use Inspect instead to clarify that the pipeline continues after execution.", error: true)]
    public Result<TValue> Finally(Action<Result<TValue>> action) => Inspect(action);

    // ─── Try-pattern ──────────────────────────────────────────────────────────

    /// <summary>
    /// Attempts to get the success value if this result is successful.
    /// Returns <see langword="false"/> for both <see cref="IsFailure"/> and uninitialized default results.
    /// </summary>
    /// <param name="value">The success value, or <see langword="default"/> if the result is not successful.</param>
    /// <returns>
    /// <see langword="true"/> if this result is successful and <paramref name="value"/> has been set;
    /// <see langword="false"/> for both failed results and <b>uninitialized default results</b>.
    /// </returns>
    /// <remarks>
    /// <b>⚠ Uninitialized results:</b> This overload returns <see langword="false"/> silently for
    /// <c>default(Result&lt;TValue&gt;)</c> — indistinguishable from a failure result.
    /// If you need to distinguish between Failure and Uninitialized, use
    /// <see cref="TryGetValue(out TValue, out bool)"/> which provides an <c>isUninitialized</c> output.
    /// Always construct results via <see cref="Result.Success{TValue}(TValue)"/> or <see cref="Result.Failure{TValue}(Error)"/>.
    /// </remarks>
    public bool TryGetValue([MaybeNullWhen(false)] out TValue value)
    {
        value = _value;
        return _state == ResultState.Success;
    }

    /// <summary>
    /// Attempts to get the success value, distinguishing between Success, Failure, and Uninitialized states.
    /// </summary>
    /// <param name="value">The success value, or <see langword="default"/> if not successful.</param>
    /// <param name="isUninitialized">
    /// <see langword="true"/> if this result is an uninitialized default value;
    /// <see langword="false"/> for properly constructed results (either success or failure).
    /// </param>
    /// <returns>
    /// <see langword="true"/> if this result is successful and <paramref name="value"/> has been set;
    /// <see langword="false"/> if the result is a failure or uninitialized.
    /// </returns>
    /// <remarks>
    /// Use this overload when you need to distinguish between a legitimate failure and an accidental
    /// <c>default(Result&lt;TValue&gt;)</c>. This mirrors the <see cref="TryGetError(out Error, out bool)"/>
    /// pattern already available on the failure path.
    /// </remarks>
    public bool TryGetValue(
        [MaybeNullWhen(false)] out TValue value,
        out bool isUninitialized)
    {
        // Stryker disable once all : Equivalent mutation
        isUninitialized = _state == ResultState.Uninitialized;
        // Stryker disable once all : Equivalent mutation
        value = _state == ResultState.Success ? _value : default;
        return _state == ResultState.Success;
    }

    /// <summary>
    /// Attempts to get the error if this result is a failure. Returns <see langword="false"/> for both
    /// <see cref="IsSuccess"/> and uninitialized default results.
    /// </summary>
    /// <param name="error">The error, or <see langword="null"/> if the result is not a failure.</param>
    /// <returns>
    /// <see langword="true"/> if this result is a failure and <paramref name="error"/> has been set;
    /// <see langword="false"/> for both successful results and <b>uninitialized default results</b>.
    /// </returns>
    /// <remarks>
    /// <b>⚠ Uninitialized results:</b> This overload returns <see langword="false"/> silently for
    /// <c>default(Result&lt;TValue&gt;)</c> without indicating that the result is in an invalid state.
    /// If you need to distinguish between Success and Uninitialized, use
    /// <see cref="TryGetError(out Error, out bool)"/> which provides an <c>isUninitialized</c> output.
    /// Always construct results via <see cref="Result.Success{TValue}(TValue)"/> or <see cref="Result.Failure{TValue}(Error)"/>.
    /// </remarks>
    public bool TryGetError([System.Diagnostics.CodeAnalysis.MaybeNullWhen(false)] out Error error)
    {
        if (_state == ResultState.Failure)
        {
            error = _error ?? WellKnownErrors.UninitializedError;
            return true;
        }
        error = null;
        return false; // Returns false for both Success and Uninitialized
    }

    /// <summary>
    /// Attempts to get the error, distinguishing between Failure and Uninitialized states.
    /// </summary>
    public bool TryGetError(
        [System.Diagnostics.CodeAnalysis.MaybeNullWhen(false)] out Error error,
        out bool isUninitialized)
    {
        isUninitialized = _state == ResultState.Uninitialized;
        if (_state == ResultState.Failure)
        {
            error = _error ?? WellKnownErrors.UninitializedError;
            return true;
        }
        error = null;
        return false;
    }

    // ─── Safe access ──────────────────────────────────────────────────────────

    /// <remarks>
    /// Returns <paramref name="defaultValue"/> for failure and uninitialized states.
    /// This method <b>never throws</b>, consistent with the BCL convention for
    /// <c>*OrDefault</c> methods (e.g., <see cref="System.Nullable{T}.GetValueOrDefault()"/>,
    /// <c>Dictionary.TryGetValue</c>).
    /// Use <see cref="GetValueOrFallback(Func{Error, TValue})"/> if you need to distinguish
    /// failure from uninitialized, or if you need access to the error to produce the fallback.
    /// </remarks>
    [Pure]
    public TValue GetValueOrDefault(TValue defaultValue)
    {
        // NOTE: Intentionally does NOT call ThrowIfUninitialized().
        // Per BCL convention, *OrDefault methods never throw — they return the default
        // value for any non-Success state (Failure or Uninitialized).
        return _state == ResultState.Success ? _value! : defaultValue;
    }

    /// <summary>
    /// Returns the success value, or invokes <paramref name="fallback"/> with the error to produce a default.
    /// Named <c>GetValueOrFallback</c> (not an overload of <c>GetValueOrDefault</c>) to avoid ambiguity
    /// when <typeparamref name="TValue"/> is itself a delegate type.
    /// </summary>
    [Pure]
    public TValue GetValueOrFallback(Func<Error, TValue> fallback)
    {
        ThrowIfUninitialized();
        return _state == ResultState.Success ? _value! : fallback(_error!);
    }

    [Pure]
    public TValue GetValueOrFallback<TState>(TState state, Func<TState, Error, TValue> fallback)
    {
        ThrowIfUninitialized();
        return _state == ResultState.Success ? _value! : fallback(state, _error!);
    }

    // ─── Error transformation ─────────────────────────────────────────────────

    /// <summary>Transforms the error of a failed result using the provided <paramref name="mapper"/>. Returns the same success result unchanged.</summary>
    [Pure]
    public Result<TValue> MapError(Func<Error, Error> mapper)
    {
        ThrowIfUninitialized();
        return _state == ResultState.Failure ? Failure(mapper(_error!)) : this;
    }

    /// <summary>Transforms the error of a failed result using the provided <paramref name="mapper"/> and captured state. Returns the same success result unchanged.</summary>
    [Pure]
    public Result<TValue> MapError<TState>(TState state, Func<TState, Error, Error> mapper)
    {
        ThrowIfUninitialized();
        return _state == ResultState.Failure ? Failure(mapper(state, _error!)) : this;
    }

    // ─── Conversion ───────────────────────────────────────────────────────────

    /// <summary>
    /// Discards the typed value and returns a non-generic <see cref="Result"/>.
    /// Preserves success or failure state and any associated <see cref="Error"/>.
    /// </summary>
    /// <remarks>
    /// Use this when you need to pass a result to a method that accepts a non-generic <see cref="Result"/>,
    /// for example when aggregating results with <see cref="Result.Combine(System.ReadOnlySpan{Result})"/>.
    /// </remarks>
    [Pure]
    public Result DiscardValue()
    {
        ThrowIfUninitialized();
        return _state == ResultState.Success ? Result.Success() : Result.Failure(_error!);
    }

    /// <inheritdoc cref="DiscardValue"/>
    [Pure]
    [Obsolete("Use DiscardValue() instead. ToResult() is ambiguous because Result<T> is already a result; DiscardValue() clearly expresses that the value is being dropped.", error: true)]
    public Result ToResult() => DiscardValue();

    [Pure]
    [Obsolete("Use DiscardValue() instead. WithoutValue() was an alias and has been removed for API clarity.", error: true)]
    public Result WithoutValue() => DiscardValue();

    // ─── Deconstruct ──────────────────────────────────────────────────────────

    public void Deconstruct(out bool isSuccess, out TValue? value, out Error? error)
    {
        isSuccess = _state == ResultState.Success;
        value = _value;
        error = _state == ResultState.Success ? null : _error ?? WellKnownErrors.UninitializedError;
    }

    /// <summary>Deconstructs the result into success flag and error, ignoring the value. Useful when the value is not needed.</summary>
    public void Deconstruct(out bool isSuccess, out Error? error)
    {
        isSuccess = _state == ResultState.Success;
        error = _state == ResultState.Success ? null : _error ?? WellKnownErrors.UninitializedError;
    }

    // ─── Equality ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Determines whether this result is equal to another result by comparing state,
    /// error (when both are failures), and value (when both are successes).
    /// </summary>
    /// <remarks>
    /// The parameter is passed by reference (<c>in</c>) to avoid copying the struct,
    /// which is important for large value types such as <c>Result&lt;decimal&gt;</c> or
    /// <c>Result&lt;(T1, T2, T3)&gt;</c>.
    /// </remarks>
    public bool Equals(in Result<TValue> other)
    {
        if (_state != other._state) return false;
        if (_state == ResultState.Failure) return Equals(_error, other._error);
        if (_state == ResultState.Success) return System.Collections.Generic.EqualityComparer<TValue>.Default.Equals(_value, other._value);
        return true;
    }

    /// <summary>
    /// Satisfies <see cref="IEquatable{T}"/> by forwarding to the <c>in</c> overload.
    /// </summary>
    /// <remarks>
    /// This overload exists solely to satisfy the <see cref="IEquatable{T}"/> interface contract,
    /// which does not allow <c>in</c> parameters. The actual comparison logic lives in
    /// <see cref="Equals(in Result{TValue})"/>, which avoids copying the struct.
    /// Prefer calling the <c>in</c> overload directly, or use the <c>==</c> operator.
    /// </remarks>
    public bool Equals(Result<TValue> other) => Equals(in other);

    public override bool Equals(object? obj) => obj is Result<TValue> other && Equals(in other);

    public override int GetHashCode()
    {
        return _state switch
        {
            // Stryker disable once all : Equivalent mutation
            ResultState.Failure => HashCode.Combine(_state, _error?.GetHashCode() ?? 0),
            // Stryker disable once all : Equivalent mutation
            ResultState.Success => HashCode.Combine(_state, _value is null ? 0 : System.Collections.Generic.EqualityComparer<TValue>.Default.GetHashCode(_value)),
            _ => HashCode.Combine(_state)
        };
    }

    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public static bool operator ==(in Result<TValue> left, in Result<TValue> right) => left.Equals(in right);
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public static bool operator !=(in Result<TValue> left, in Result<TValue> right) => !left.Equals(in right);

    // ─── Implicit conversions ─────────────────────────────────────────────────

    public static implicit operator Result<TValue>(TValue value) => Success(value);
    public static implicit operator Result<TValue>(Error error) => Failure(error);

    private string GetDebuggerDisplay() => _state switch
    {
        ResultState.Success => $"Success ({_value})",
        ResultState.Failure => $"Failure ({_error?.Code})",
        _ => "Uninitialized"
    };

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    private void ThrowIfUninitialized()
    {
        if (_state == ResultState.Uninitialized)
            ResultThrowHelper.ThrowUninitializedOfT();
    }
}

