using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace EricksonLopez.Result;

/// <summary>
/// Represents the outcome of an operation that may succeed or fail.
/// Uses a struct layout to avoid heap allocation for the Result envelope itself;
/// note that the <see cref="Error"/> object (when present) is heap-allocated by its nature as a class.
/// </summary>
[DebuggerDisplay("{GetDebuggerDisplay(),nq}")]
[StructLayout(LayoutKind.Auto)]
public readonly partial struct Result : IResultOutcome, IEquatable<Result>
{
    private readonly ResultState _state;
    private readonly Error? _error;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal Result(ResultState state, Error? error)
    {
        _state = state;
        _error = error;
    }

    /// <summary>Gets a value indicating whether the operation succeeded.</summary>
    /// <remarks>
    /// <b>⚠ Uninitialized default:</b> Returns <see langword="false"/> for an uninitialized
    /// <c>default(Result)</c> — the same as a failure, but without an error.
    /// Always construct results via <see cref="Success()"/> or <see cref="Failure(Error)"/>.
    /// Use <see cref="IsUninitialized"/> to detect an uninitialized default explicitly.
    /// </remarks>
    public bool IsSuccess => _state == ResultState.Success;

    /// <summary>Gets a value indicating whether the operation failed.</summary>
    /// <remarks>
    /// <b>⚠ Uninitialized default:</b> Returns <see langword="false"/> for an uninitialized
    /// <c>default(Result)</c> — the same as a success (neither true nor false corresponds to
    /// the uninitialized state). Use <see cref="IsUninitialized"/> to distinguish.
    /// </remarks>
    public bool IsFailure => _state == ResultState.Failure;

    /// <summary>Gets a value indicating whether the struct is an uninitialized default value.</summary>
    public bool IsUninitialized => _state == ResultState.Uninitialized;

    /// <summary>
    /// Gets the error associated with this result.
    /// Throws an InvalidOperationException if the result is successful.
    /// </summary>
    public Error Error => _state switch
    {
        ResultState.Failure => _error!,
        ResultState.Success => throw new InvalidOperationException("Cannot access the Error of a successful result."),
        _ => WellKnownErrors.UninitializedError
    };

    // ─── Implicit Operators & Casts ──────────────────────────────────────────────

    /// <summary>Implicitly converts an Error into a failed Result.</summary>
    public static implicit operator Result(Error error) => Failure(error);

    /// <summary>
    /// Allows using Result in boolean contexts (e.g., if(result)). Returns <see langword="true"/> if successful.
    /// </summary>
    /// <remarks>
    /// <b>⚠ Uninitialized gotcha:</b> A <see langword="default"/><c>(Result)</c> (state = Uninitialized)
    /// returns <see langword="false"/> here — it does NOT throw. This means an uninitialized
    /// <c>Result</c> silently evaluates as failure in <c>if(result)</c> without any diagnostic.
    /// Always construct <c>Result</c> via <see cref="Success()"/> or <see cref="Failure(Error)"/>.
    /// </remarks>
    [Pure]
    public static bool operator true(Result result) => result.IsSuccess;

    /// <summary>Allows using Result in boolean contexts. Returns <see langword="true"/> if failed, <see langword="false"/> if successful or uninitialized.</summary>
    /// <remarks>
    /// <b>⚠ Uninitialized gotcha:</b> A <see langword="default"/><c>(Result)</c> (state = Uninitialized)
    /// returns <see langword="false"/> here — the same as <see cref="IsFailure"/>, because
    /// <c>IsFailure</c> checks <c>_state == ResultState.Failure</c>, which evaluates to
    /// <see langword="false"/> for the Uninitialized state (byte value 0). An uninitialized
    /// result is <em>neither</em> success nor failure in boolean context: both <c>operator true</c>
    /// and <c>operator false</c> return <see langword="false"/>. Prefer using
    /// <see cref="IsSuccess"/> and <see cref="IsFailure"/> properties with explicit checks, or use
    /// <see cref="Match{TOut}(Func{TOut}, Func{Error, TOut})"/> which throws an
    /// <see cref="InvalidOperationException"/> for Uninitialized results.
    /// </remarks>
    [Pure]
    public static bool operator false(Result result) => result.IsFailure;

    // ─── Factory methods ──────────────────────────────────────────────────────

    /// <summary>Returns a success result.</summary>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result Success() => new(ResultState.Success, null);

    /// <summary>Creates a failure result with the specified error.</summary>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result Failure(Error error) => new(ResultState.Failure, error ?? throw new ArgumentNullException(nameof(error)));

    /// <summary>Creates a success result wrapping the specified value.</summary>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result<TValue> Success<TValue>(TValue value) => Result<TValue>.Success(value);

    /// <summary>Creates a typed failure result with the specified error.</summary>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result<TValue> Failure<TValue>(Error error) => Result<TValue>.Failure(error);

    // ─── Match & Switch ───────────────────────────────────────────────────────

    [Pure]
    public TOut Match<TOut>(Func<TOut> onSuccess, Func<Error, TOut> onFailure)
    {
        ThrowIfUninitialized();
        return _state == ResultState.Success ? onSuccess() : onFailure(_error!);
    }

    [Pure]
    public TOut Match<TState, TOut>(TState state, Func<TState, TOut> onSuccess, Func<TState, Error, TOut> onFailure)
    {
        ThrowIfUninitialized();
        return _state == ResultState.Success ? onSuccess(state) : onFailure(state, _error!);
    }

    /// <summary>
    /// Invokes <paramref name="onSuccess"/> when this result is successful,
    /// or <paramref name="onFailure"/> when it is a failure — without returning a value.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <c>Execute</c> is the void side-effect counterpart to <see cref="Match{TOut}"/>:
    /// use <c>Match</c> when you need a return value, and <c>Execute</c> for pure side-effects
    /// (logging, dispatching events, updating state).
    /// </para>
    /// <para>
    /// <b>Naming note:</b> The name <c>Execute</c> is borrowed from functional programming (e.g. F# <c>match</c>
    /// expressions used for side-effects). In the .NET idiom, a close equivalent would be <c>Visit</c>
    /// or <c>Execute</c>. <c>Execute</c> was chosen for conciseness and consistency with functional Result
    /// libraries; it does <em>not</em> refer to the C# <c>Execute</c> statement.
    /// </para>
    /// </remarks>
    /// <param name="onSuccess">Action invoked when the result is <see cref="IsSuccess">successful</see>.</param>
    /// <param name="onFailure">Action invoked with the <see cref="Error"/> when the result is a failure.</param>
    public void Execute(Action onSuccess, Action<Error> onFailure)
    {
        ThrowIfUninitialized();
        // Stryker disable once all : Equivalent mutation
        if (_state == ResultState.Success) onSuccess();
        else onFailure(_error!);
    }

    /// <summary>
    /// Invokes <paramref name="onSuccess"/> or <paramref name="onFailure"/> for their side-effects,
    /// forwarding <paramref name="state"/> to avoid a closure allocation.
    /// </summary>
    /// <remarks>
    /// Use this overload in hot paths where capturing variables in a closure would cause heap allocation.
    /// See <see cref="Execute(Action, Action{Error})"/> for the naming rationale.
    /// </remarks>
    public void Execute<TState>(TState state, Action<TState> onSuccess, Action<TState, Error> onFailure)
    {
        ThrowIfUninitialized();
        if (_state == ResultState.Success) onSuccess(state);
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
    /// Use this when you only need to handle the failure branch. For transforming both branches,
    /// use <see cref="Match{TOut}(Func{TOut}, Func{Error, TOut})"/>.
    /// <para>
    /// <b>💡 Allocation tip:</b> If <paramref name="onFailure"/> captures variables from an outer scope,
    /// use <c>MapFailure&lt;TState, TOut&gt;(TState, Func&lt;TState, Error, TOut&gt;, TOut)</c>
    /// to pass the captured values as a <c>TState</c> parameter and avoid closure allocation.
    /// </para>
    /// </remarks>
    [Pure]
    public TOut MapFailure<TOut>(Func<Error, TOut> onFailure, TOut successDefault)
    {
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

    // ─── Monadic Operations ───────────────────────────────────────────────────

    /// <summary>
    /// Projects a successful Result into a <see cref="Result{TNext}"/> by executing <paramref name="mapper"/> on success.
    /// If the Result is a failure, returns <c>Result.Failure&lt;TNext&gt;</c> with the existing error.
    /// </summary>
    /// <typeparam name="TNext">The output value type.</typeparam>
    /// <param name="mapper">A function that produces the value for the success case.</param>
    /// <returns>A <see cref="Result{TNext}"/> with the mapped value on success, or the original error on failure.</returns>
    /// <remarks>
    /// <b>💡 Allocation tip:</b> If <paramref name="mapper"/> captures variables from an outer scope (closure),
    /// prefer the <c>Map&lt;TState, TNext&gt;(TState, Func&lt;TState, TNext&gt;)</c> overload to pass context
    /// without allocating a closure object on the heap.
    /// </remarks>
    [Pure]
    public Result<TNext> Map<TNext>(Func<TNext> mapper)
    {
        ThrowIfUninitialized();
        return _state == ResultState.Success ? Result.Success(mapper()) : Result.Failure<TNext>(_error!);
    }

    /// <summary>
    /// Projects a successful Result into a <see cref="Result{TNext}"/> using a state argument to avoid closure allocations.
    /// </summary>
    /// <typeparam name="TState">The type of the state argument passed to <paramref name="mapper"/>.</typeparam>
    /// <typeparam name="TNext">The output value type.</typeparam>
    /// <param name="state">An external value passed to <paramref name="mapper"/> to avoid a closure.</param>
    /// <param name="mapper">A function that receives the state and produces the result value.</param>
    [Pure]
    public Result<TNext> Map<TState, TNext>(TState state, Func<TState, TNext> mapper)
    {
        ThrowIfUninitialized();
        return _state == ResultState.Success ? Result.Success(mapper(state)) : Result.Failure<TNext>(_error!);
    }

    public Result Bind(Func<Result> bind)
    {
        ThrowIfUninitialized();
        return _state == ResultState.Success ? bind() : Failure(_error!);
    }
    public Result Bind<TState>(TState state, Func<TState, Result> bind)
    {
        ThrowIfUninitialized();
        return _state == ResultState.Success ? bind(state) : Failure(_error!);
    }

    public Result<TNext> Bind<TNext>(Func<Result<TNext>> bind)
    {
        ThrowIfUninitialized();
        return _state == ResultState.Success ? bind() : Result.Failure<TNext>(_error!);
    }
    /// <remarks>
    /// <b>💡 Allocation tip:</b> This overload accepts captured state to avoid closure allocations.
    /// Prefer this over <c>Bind(Func&lt;Result&lt;TNext&gt;&gt;)</c> when you would otherwise capture
    /// a variable from an outer scope.
    /// </remarks>
    public Result<TNext> Bind<TState, TNext>(TState state, Func<TState, Result<TNext>> bind)
    {
        ThrowIfUninitialized();
        return _state == ResultState.Success ? bind(state) : Result.Failure<TNext>(_error!);
    }

    // ─── Side effects ─────────────────────────────────────────────────────────

    /// <summary>
    /// Executes <paramref name="onSuccess"/> if this result is successful, then returns this result unchanged.
    /// </summary>
    /// <remarks>
    /// This method executes the action <b>only on success</b> and is symmetric with <see cref="TapOnFailure(Action{Error})"/>.
    /// Use <see cref="Inspect(Action{Result})"/> if you need unconditional execution (both success and failure).
    /// <para>
    /// <b>💡 Allocation tip:</b> Use <c>TapOnSuccess&lt;TState&gt;(TState, Action&lt;TState&gt;)</c> to avoid
    /// closure allocations when capturing outer variables.
    /// </para>
    /// </remarks>
    public Result TapOnSuccess(Action onSuccess)
    {
        ThrowIfUninitialized();
        if (_state == ResultState.Success) onSuccess();
        return this;
    }

    /// <remarks>
    /// <b>💡 Allocation tip:</b> This overload passes context via <paramref name="state"/> instead of
    /// a captured closure.
    /// </remarks>
    public Result TapOnSuccess<TState>(TState state, Action<TState> onSuccess)
    {
        ThrowIfUninitialized();
        if (_state == ResultState.Success) onSuccess(state);
        return this;
    }

    /// <summary>Executes <paramref name="onFailure"/> if this result is a failure, then returns this result unchanged.</summary>
    /// <remarks>
    /// This method executes the action <b>only on failure</b> and is symmetric with <see cref="TapOnSuccess(Action)"/>.
    /// Use <see cref="Inspect(Action{Result})"/> if you need unconditional execution (both success and failure).
    /// <para>
    /// <b>💡 Allocation tip:</b> Use <c>TapOnFailure&lt;TState&gt;(TState, Action&lt;TState, Error&gt;)</c> to avoid
    /// closure allocations when capturing outer variables.
    /// </para>
    /// </remarks>
    public Result TapOnFailure(Action<Error> onFailure)
    {
        // Stryker disable once all : Equivalent mutation
        ThrowIfUninitialized();
        // Stryker disable once all : Equivalent mutation
        if (_state == ResultState.Failure) onFailure(_error!);
        return this;
    }

    /// <summary>Executes <paramref name="onFailure"/> with captured state if this result is a failure, then returns this result unchanged.</summary>
    /// <remarks>
    /// <b>💡 Allocation tip:</b> This overload passes context via <paramref name="state"/> instead of a captured closure.
    /// </remarks>
    public Result TapOnFailure<TState>(TState state, Action<TState, Error> onFailure)
    {
        ThrowIfUninitialized();
        // Stryker disable once all : Equivalent mutation
        if (_state == ResultState.Failure) onFailure(state, _error!);
        return this;
    }

    // ─── Composition ──────────────────────────────────────────────────────────

    public Result Ensure(Func<bool> predicate, Error error)
    {
        ThrowIfUninitialized();
        if (_state == ResultState.Success) return predicate() ? this : Failure(error);
        return this; // Failure: short-circuit
    }
    public Result Ensure<TState>(TState state, Func<TState, bool> predicate, Error error)
    {
        ThrowIfUninitialized();
        if (_state == ResultState.Success) return predicate(state) ? this : Failure(error);
        return this; // Failure: short-circuit
    }

    /// <summary>
    /// Validates a successful result using <paramref name="predicate"/>, constructing the error lazily
    /// via <paramref name="errorFactory"/> only when the predicate fails.
    /// </summary>
    /// <remarks>
    /// Prefer this overload over <see cref="Ensure(Func{bool}, Error)"/> when error construction is
    /// expensive (e.g., captures <see cref="System.Diagnostics.Activity.Current"/>, allocates metadata,
    /// or involves string formatting). The error factory is never invoked on the success path.
    /// </remarks>
    public Result Ensure(Func<bool> predicate, Func<Error> errorFactory)
    {
        ThrowIfUninitialized();
        if (_state == ResultState.Success) return predicate() ? this : Failure(errorFactory());
        return this; // Failure: short-circuit
    }

    /// <summary>
    /// Validates a successful result using <paramref name="predicate"/> with captured state,
    /// constructing the error lazily via <paramref name="errorFactory"/> only when the predicate fails.
    /// </summary>
    public Result Ensure<TState>(TState state, Func<TState, bool> predicate, Func<Error> errorFactory)
    {
        ThrowIfUninitialized();
        if (_state == ResultState.Success) return predicate(state) ? this : Failure(errorFactory());
        return this; // Failure: short-circuit
    }

    /// <summary>Gets the error associated with a failed result, or null if successful.</summary>
    // Stryker disable once all : Equivalent mutation
    Error? IResultOutcome.Error => _state == ResultState.Failure ? _error : null;

    /// <summary>Gets the raw underlying value of a successful result, which is always null for non-generic Result.</summary>
    object? IResultOutcome.RawValue => null;

    /// <summary>
    /// Executes <paramref name="action"/> unconditionally with the current result (success or failure),
    /// then returns this result unchanged. Useful for logging, debugging, or auditing the pipeline at any point.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Inspect vs TapOnSuccess:</b>
    /// <list type="table">
    ///   <listheader><term>Method</term><description>When it runs / What it receives</description></listheader>
    ///   <item><term><see cref="Inspect(Action{Result})"/></term><description>Always (success or failure). Receives the full <see cref="Result"/>.</description></item>
    ///   <item><term><see cref="TapOnSuccess(Action)"/></term><description>Only when the result is a success. Receives nothing.</description></item>
    ///   <item><term><see cref="TapOnFailure(Action{Error})"/></term><description>Only when the result is a failure. Receives the <see cref="Error"/>.</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// Use <see cref="Inspect(Action{Result})"/> when you need to observe <i>any</i> outcome (e.g., structured logging that
    /// records both success and failure in a single callback). Use <see cref="TapOnSuccess(Action)"/> when you only
    /// care about the success path and do not need access to the <see cref="Result"/> wrapper.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Log every outcome (success + failure) without branching:
    /// result
    ///     .Inspect(r => logger.LogInformation("Pipeline step completed. IsSuccess={IsSuccess}", r.IsSuccess))
    ///     .TapOnFailure(err => logger.LogError("Failure detail: {Code}", err.Code));
    /// </code>
    /// </example>
    public Result Inspect(Action<Result> action)
    {
        ThrowIfUninitialized();
        action(this);
        return this;
    }

    /// <summary>
    /// Executes <paramref name="action"/> unconditionally with captured <paramref name="state"/> and the current result,
    /// then returns this result unchanged.
    /// </summary>
    /// <remarks>
    /// <b>💡 Allocation tip:</b> This overload passes context via <paramref name="state"/> instead of a captured closure.
    /// See <see cref="Inspect(Action{Result})"/> for the distinction between <c>Inspect</c> and <c>TapOnSuccess</c>.
    /// </remarks>
    public Result Inspect<TState>(TState state, Action<TState, Result> action)
    {
        ThrowIfUninitialized();
        action(state, this);
        return this;
    }

    [Obsolete("Use Inspect instead to clarify that the pipeline continues after execution.", error: true)]
    public Result Finally(Action<Result> action) => Inspect(action);

    // ─── Recovery ─────────────────────────────────────────────────────────────

    /// <summary>If this result is a failure, invokes <paramref name="recovery"/> to attempt a corrective result.</summary>
    [Pure]
    public Result Recover(Func<Error, Result> recovery)
    {
        ThrowIfUninitialized();
        return _state == ResultState.Failure ? recovery(_error!) : this;
    }

    /// <summary>If this result is a failure, invokes <paramref name="recovery"/> with captured state to attempt a corrective result.</summary>
    [Pure]
    public Result Recover<TState>(TState state, Func<TState, Error, Result> recovery)
    {
        ThrowIfUninitialized();
        return _state == ResultState.Failure ? recovery(state, _error!) : this;
    }

    // ─── Deconstruct ──────────────────────────────────────────────────────────

    public void Deconstruct(out bool isSuccess, out Error? error)
    {
        isSuccess = _state == ResultState.Success;
        error = _state == ResultState.Success ? null : _error ?? WellKnownErrors.UninitializedError;
    }

    // ─── Exception bridge ─────────────────────────────────────────────────────

    /// <summary>
    /// Executes <paramref name="action"/> and wraps any non-fatal exception into a failure result.
    /// </summary>
    /// <param name="action">The action to execute.</param>
    /// <param name="errorHandler">Maps a caught exception to an <see cref="Error"/>.</param>
    /// <returns>Success if <paramref name="action"/> completes without throwing; otherwise a failure result.</returns>
    /// <remarks>
    /// <para>
    /// <b>OperationCanceledException handling:</b> <see cref="OperationCanceledException"/> (including
    /// <see cref="System.Threading.Tasks.TaskCanceledException"/>) is caught and converted into a
    /// <c>Result.Failure</c> via <paramref name="errorHandler"/>. This is intentional: cancellation is
    /// normal async control flow, and the caller may want to return a specific error (e.g.,
    /// <c>Error.Unavailable("Op.Cancelled", "The operation was cancelled.")</c>).
    /// </para>
    /// <para>
    /// If you need to propagate cancellation as an exception instead of a failure result, use a
    /// <c>when</c> filter on the <paramref name="errorHandler"/> to re-throw:
    /// <code>
    /// Result.Try(
    ///     () => DoWork(),
    ///     ex => ex is OperationCanceledException oce
    ///         ? throw oce  // re-propagates cancellation
    ///         : Error.Failure("Op.Failed", ex.Message));
    /// </code>
    /// </para>
    /// <para>
    /// Fatal exceptions (<see cref="OutOfMemoryException"/>, <see cref="StackOverflowException"/>,
    /// <see cref="AccessViolationException"/>) are never caught and always propagate.
    /// </para>
    /// </remarks>
    public static Result Try(Action action, Func<Exception, Error> errorHandler)
    {
        try
        {
            action();
            return Success();
        }
        catch (Exception ex) when (!IsFatal(ex))
        {
            return Failure(errorHandler(ex));
        }
    }

    /// <summary>
    /// Executes <paramref name="action"/> and wraps any non-fatal exception into a failure result,
    /// forwarding <paramref name="state"/> to the error handler to avoid a closure allocation.
    /// </summary>
    /// <typeparam name="TState">The type of the state argument passed to <paramref name="errorHandler"/>.</typeparam>
    /// <param name="state">An external value passed to <paramref name="errorHandler"/> to avoid capturing it in a closure.</param>
    /// <param name="action">The synchronous action to execute.</param>
    /// <param name="errorHandler">Maps the caught exception and state to an <see cref="Error"/>.</param>
    /// <returns>Success if <paramref name="action"/> completes without throwing; otherwise a failure result.</returns>
    /// <remarks>
    /// <b>💡 Allocation tip:</b> Use this overload when <paramref name="errorHandler"/> would otherwise
    /// capture a variable from an outer scope (causing a closure allocation). For example:
    /// <code>
    /// var operationName = "SaveOrder";
    /// // Without TState: errorHandler captures operationName — heap allocation
    /// var result = Result.Try(() => DoWork(), ex => Error.Failure($"{operationName}.Failed", ex.Message));
    ///
    /// // With TState: no closure — zero heap allocation for the handler
    /// var result = Result.Try(operationName, () => DoWork(), (state, ex) => Error.Failure($"{state}.Failed", ex.Message));
    /// </code>
    /// </remarks>
    public static Result Try<TState>(TState state, Action action, Func<TState, Exception, Error> errorHandler)
    {
        try
        {
            action();
            return Success();
        }
        catch (Exception ex) when (!IsFatal(ex))
        {
            return Failure(errorHandler(state, ex));
        }
    }

    /// <summary>
    /// Executes an async <paramref name="action"/> and wraps any non-fatal exception into a failure result.
    /// </summary>
    /// <param name="action">The async action to execute.</param>
    /// <param name="errorHandler">Maps a caught exception to an <see cref="Error"/>.</param>
    /// <returns>Success if <paramref name="action"/> completes without throwing; otherwise a failure result.</returns>
    /// <remarks>
    /// <b>OperationCanceledException handling:</b> <see cref="OperationCanceledException"/> is caught and
    /// converted into a failure via <paramref name="errorHandler"/>. To re-propagate cancellation instead,
    /// check for <see cref="OperationCanceledException"/> in <paramref name="errorHandler"/> and re-throw it.
    /// See <see cref="Try(Action, Func{Exception, Error})"/> for a code example.
    /// </remarks>
    public static async System.Threading.Tasks.Task<Result> TryAsync(Func<System.Threading.Tasks.Task> action, Func<Exception, Error> errorHandler)
    {
        try
        {
            // Stryker disable once boolean
            await action().ConfigureAwait(false);
            return Success();
        }
        catch (Exception ex) when (!IsFatal(ex))
        {
            return Failure(errorHandler(ex));
        }
    }

    /// <summary>Executes an async action with cancellation support, wrapping exceptions into a failure result.</summary>
    public static async System.Threading.Tasks.Task<Result> TryAsync(
        Func<System.Threading.CancellationToken, System.Threading.Tasks.Task> action,
        Func<Exception, Error> errorHandler,
        System.Threading.CancellationToken cancellationToken = default)
    {
        try
        {
            // Stryker disable once boolean
            await action(cancellationToken).ConfigureAwait(false);
            return Success();
        }
        catch (Exception ex) when (!IsFatal(ex))
        {
            return Failure(errorHandler(ex));
        }
    }

    public static Result<T> Try<T>(Func<T> func, Func<Exception, Error> errorHandler)
    {
        try
        {
            return Success(func());
        }
        catch (Exception ex) when (!IsFatal(ex))
        {
            return Failure<T>(errorHandler(ex));
        }
    }

    /// <summary>
    /// Executes <paramref name="func"/> and wraps any non-fatal exception into a failure result,
    /// forwarding <paramref name="state"/> to the error handler to avoid a closure allocation.
    /// </summary>
    /// <typeparam name="TState">The type of the state argument passed to <paramref name="errorHandler"/>.</typeparam>
    /// <typeparam name="T">The success value type.</typeparam>
    /// <param name="state">An external value passed to <paramref name="errorHandler"/> to avoid capturing it in a closure.</param>
    /// <param name="func">The synchronous function to execute.</param>
    /// <param name="errorHandler">Maps the caught exception and state to an <see cref="Error"/>.</param>
    /// <remarks>
    /// <b>💡 Allocation tip:</b> Use this overload when <paramref name="errorHandler"/> would otherwise
    /// capture a variable from an outer scope (causing a closure allocation).
    /// </remarks>
    public static Result<T> Try<TState, T>(TState state, Func<T> func, Func<TState, Exception, Error> errorHandler)
    {
        try
        {
            return Success(func());
        }
        catch (Exception ex) when (!IsFatal(ex))
        {
            return Failure<T>(errorHandler(state, ex));
        }
    }

    public static async System.Threading.Tasks.Task<Result<T>> TryAsync<T>(Func<System.Threading.Tasks.Task<T>> func, Func<Exception, Error> errorHandler)
    {
        try
        {
            // Stryker disable once boolean
            var value = await func().ConfigureAwait(false);
            return Success(value);
        }
        catch (Exception ex) when (!IsFatal(ex))
        {
            return Failure<T>(errorHandler(ex));
        }
    }

    /// <summary>Executes an async function with cancellation support, wrapping exceptions into a failure result.</summary>
    public static async System.Threading.Tasks.Task<Result<T>> TryAsync<T>(
        Func<System.Threading.CancellationToken, System.Threading.Tasks.Task<T>> func,
        Func<Exception, Error> errorHandler,
        System.Threading.CancellationToken cancellationToken = default)
    {
        try
        {
            // Stryker disable once boolean
            var value = await func(cancellationToken).ConfigureAwait(false);
            return Success(value);
        }
        catch (Exception ex) when (!IsFatal(ex))
        {
            return Failure<T>(errorHandler(ex));
        }
    }

    // ─── ValueTask-returning TryAsync overloads ─────────────────────────────────
    // These overloads accept ValueTask-producing funcs AND return ValueTask<Result[<T>]>
    // so that callers composing end-to-end ValueTask pipelines are not forced to
    // allocate a Task state machine for every exception-wrapping boundary.

    /// <summary>
    /// Executes an async action returning <see cref="System.Threading.Tasks.ValueTask"/>,
    /// wrapping any non-fatal exception into a <see cref="ValueTask{Result}"/>.
    /// </summary>
    /// <remarks>
    /// Prefer this overload over the <c>Task&lt;Result&gt;</c> variant when composing
    /// end-to-end <see cref="System.Threading.Tasks.ValueTask"/> pipelines to avoid
    /// unnecessary <see cref="System.Threading.Tasks.Task"/> state machine allocation.
    /// </remarks>
    public static async System.Threading.Tasks.ValueTask<Result> TryAsyncValue(
        Func<System.Threading.Tasks.ValueTask> action,
        Func<Exception, Error> errorHandler)
    {
        try
        {
            // Stryker disable once boolean
            await action().ConfigureAwait(false);
            return Success();
        }
        catch (Exception ex) when (!IsFatal(ex))
        {
            return Failure(errorHandler(ex));
        }
    }

    /// <summary>
    /// Executes an async action returning <see cref="System.Threading.Tasks.ValueTask"/>
    /// with cancellation support, wrapping any non-fatal exception into a <see cref="ValueTask{Result}"/>.
    /// </summary>
    public static async System.Threading.Tasks.ValueTask<Result> TryAsyncValue(
        Func<System.Threading.CancellationToken, System.Threading.Tasks.ValueTask> action,
        Func<Exception, Error> errorHandler,
        System.Threading.CancellationToken cancellationToken = default)
    {
        try
        {
            // Stryker disable once boolean
            await action(cancellationToken).ConfigureAwait(false);
            return Success();
        }
        catch (Exception ex) when (!IsFatal(ex))
        {
            return Failure(errorHandler(ex));
        }
    }

    /// <summary>
    /// Executes an async function returning <see cref="System.Threading.Tasks.ValueTask{T}"/>,
    /// wrapping any non-fatal exception into a <c>ValueTask&lt;Result&lt;T&gt;&gt;</c>.
    /// </summary>
    /// <typeparam name="T">The value type of the result.</typeparam>
    /// <remarks>
    /// Prefer this overload over the <c>Task&lt;Result&lt;T&gt;&gt;</c> variant when composing
    /// end-to-end <see cref="System.Threading.Tasks.ValueTask"/> pipelines.
    /// </remarks>
    public static async System.Threading.Tasks.ValueTask<Result<T>> TryAsyncValue<T>(
        Func<System.Threading.Tasks.ValueTask<T>> func,
        Func<Exception, Error> errorHandler)
    {
        try
        {
            // Stryker disable once boolean
            var value = await func().ConfigureAwait(false);
            return Success(value);
        }
        catch (Exception ex) when (!IsFatal(ex))
        {
            return Failure<T>(errorHandler(ex));
        }
    }

    /// <summary>
    /// Executes an async function returning <see cref="System.Threading.Tasks.ValueTask{T}"/>
    /// with cancellation support, wrapping any non-fatal exception into a <c>ValueTask&lt;Result&lt;T&gt;&gt;</c>.
    /// </summary>
    /// <typeparam name="T">The value type of the result.</typeparam>
    public static async System.Threading.Tasks.ValueTask<Result<T>> TryAsyncValue<T>(
        Func<System.Threading.CancellationToken, System.Threading.Tasks.ValueTask<T>> func,
        Func<Exception, Error> errorHandler,
        System.Threading.CancellationToken cancellationToken = default)
    {
        try
        {
            // Stryker disable once boolean
            var value = await func(cancellationToken).ConfigureAwait(false);
            return Success(value);
        }
        catch (Exception ex) when (!IsFatal(ex))
        {
            return Failure<T>(errorHandler(ex));
        }
    }

    /// <summary>
    /// Executes an async function returning <see cref="System.Threading.Tasks.ValueTask{T}"/>,
    /// wrapping any non-fatal exception into a <c>ValueTask&lt;Result&lt;T&gt;&gt;</c>,
    /// forwarding <paramref name="state"/> to the error handler to avoid a closure allocation.
    /// </summary>
    public static async System.Threading.Tasks.ValueTask<Result<T>> TryAsyncValue<TState, T>(
        TState state,
        Func<System.Threading.Tasks.ValueTask<T>> func,
        Func<TState, Exception, Error> errorHandler)
    {
        try
        {
            // Stryker disable once boolean
            var value = await func().ConfigureAwait(false);
            return Success(value);
        }
        catch (Exception ex) when (!IsFatal(ex))
        {
            return Failure<T>(errorHandler(state, ex));
        }
    }

    /// <summary>
    /// Executes an async function returning <see cref="System.Threading.Tasks.ValueTask{T}"/>
    /// with cancellation support, wrapping any non-fatal exception into a <c>ValueTask&lt;Result&lt;T&gt;&gt;</c>,
    /// forwarding <paramref name="state"/> to the error handler to avoid a closure allocation.
    /// </summary>
    public static async System.Threading.Tasks.ValueTask<Result<T>> TryAsyncValue<TState, T>(
        TState state,
        Func<System.Threading.CancellationToken, System.Threading.Tasks.ValueTask<T>> func,
        Func<TState, Exception, Error> errorHandler,
        System.Threading.CancellationToken cancellationToken = default)
    {
        try
        {
            // Stryker disable once boolean
            var value = await func(cancellationToken).ConfigureAwait(false);
            return Success(value);
        }
        catch (Exception ex) when (!IsFatal(ex))
        {
            return Failure<T>(errorHandler(state, ex));
        }
    }

    /// <summary>
    /// Returns true for exceptions that represent unrecoverable CLR/OS failures that should never be swallowed.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <see cref="OperationCanceledException"/> is intentionally NOT treated as fatal: cancellation is
    /// normal async control flow. Callers of <c>Try</c>/<c>TryAsync</c> that pass a cancellable operation
    /// should handle it as a failure result or explicitly re-throw via the <c>errorHandler</c>.
    /// See <see cref="Try(Action, Func{Exception, Error})"/> for a pattern to re-propagate cancellation.
    /// </para>
    /// <para>
    /// Fatal exceptions that are always re-thrown (never caught):
    /// <see cref="OutOfMemoryException"/>, <see cref="StackOverflowException"/>, <see cref="AccessViolationException"/>.
    /// </para>
    /// </remarks>
    private static bool IsFatal(Exception ex)
        => ex is OutOfMemoryException or StackOverflowException or AccessViolationException;

    /// <summary>
    /// Throws <see cref="InvalidOperationException"/> if this Result is an uninitialized default value.
    /// Call at the top of any method that should not silently propagate an uninitialized result.
    /// </summary>
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    private void ThrowIfUninitialized()
    {
        if (_state == ResultState.Uninitialized)
            ResultThrowHelper.ThrowUninitialized();
    }

    // ─── Try-pattern ──────────────────────────────────────────────────────────

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
    /// <c>default(Result)</c> without indicating that the result is in an invalid state.
    /// If you need to distinguish between Success and Uninitialized, use
    /// <see cref="TryGetError(out Error, out bool)"/> which provides an <c>isUninitialized</c> output.
    /// Always construct results via <see cref="Success()"/> or <see cref="Failure(Error)"/>.
    /// </remarks>
    [Pure]
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

    // ─── Error transformation ─────────────────────────────────────────────────

    /// <summary>Transforms the error of a failed result using the provided <paramref name="mapper"/>. Returns the same success result unchanged.</summary>
    [Pure]
    public Result MapError(Func<Error, Error> mapper)
    {
        ThrowIfUninitialized();
        return _state == ResultState.Failure ? Failure(mapper(_error!)) : this;
    }

    /// <summary>Transforms the error of a failed result using the provided <paramref name="mapper"/> and captured state. Returns the same success result unchanged.</summary>
    [Pure]
    public Result MapError<TState>(TState state, Func<TState, Error, Error> mapper)
    {
        ThrowIfUninitialized();
        return _state == ResultState.Failure ? Failure(mapper(state, _error!)) : this;
    }

    // ─── Equality ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Determines whether the specified <see cref="Result"/> is equal to the current instance.
    /// </summary>
    /// <remarks>
    /// Two results are equal when they have the same state and — on failure — the same error
    /// (using <see cref="Error.Equals(Error?)"/> shallow equality).
    /// The parameter is passed by reference (<c>in</c>) to avoid copying the 16-byte struct
    /// on every call; this is especially important since <c>operator ==</c> uses <c>in</c> parameters.
    /// </remarks>
    public bool Equals(in Result other)
    {
        if (_state != other._state) return false;
        if (_state == ResultState.Failure) return Equals(_error, other._error);
        return true;
    }

    /// <inheritdoc/>
    bool IEquatable<Result>.Equals(Result other) => Equals(in other);

    public override bool Equals(object? obj) => obj is Result other && Equals(in other);
    // Stryker disable once all : Equivalent mutation
    public override int GetHashCode() => HashCode.Combine(_state, _state == ResultState.Failure ? _error!.GetHashCode() : 0);
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public static bool operator ==(in Result left, in Result right) => left.Equals(in right);
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public static bool operator !=(in Result left, in Result right) => !left.Equals(in right);

    // ─── Implicit conversions ───────────────────────────────────────────────────

    private string GetDebuggerDisplay() => _state switch
    {
        ResultState.Success => "Success",
        ResultState.Failure => $"Failure ({_error?.Code})",
        _ => "Uninitialized"
    };
}


