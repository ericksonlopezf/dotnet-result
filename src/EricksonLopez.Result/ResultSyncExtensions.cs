// Copyright © Erickson Lopez. MIT License.
using System;
using System.Diagnostics.Contracts;
using System.Linq;

namespace EricksonLopez.Result;

/// <summary>
/// Provides high-performance synchronous extension methods for <see cref="Result{TValue}"/>
/// that accept the struct by reference (<c>in</c>) to avoid copying large value types.
/// </summary>
/// <remarks>
/// <para>
/// <b>Performance rationale:</b> When the value type parameter of <c>Result&lt;T&gt;</c> is a large value type
/// (e.g., <c>decimal</c> at 16 bytes, <c>Guid</c> at 16 bytes, or value tuples beyond 8 bytes),
/// passing <c>Result&lt;T&gt;</c> by value incurs a struct copy on the call site.
/// These <c>in</c>-parameter overloads eliminate that copy by passing the struct by readonly reference.
/// </para>
/// <para>
/// <b>When to use:</b> These extension methods are identical in behavior to the instance methods
/// on <see cref="Result{TValue}"/> (e.g., <see cref="Result{TValue}.Map{TNext}"/>).
/// They are provided as an explicit optimization path for scenarios where:
/// <list type="bullet">
///   <item>The value type parameter is a value type with size &gt;= 16 bytes (e.g., <c>decimal</c>, <c>Guid</c>, large structs)</item>
///   <item>The result is passed across many layers and each layer calls at least one pipeline method</item>
///   <item>Profiling confirms struct-copy overhead in a hot path</item>
/// </list>
/// For reference types (<c>class</c>, <c>string</c>, <c>object</c>), <c>in</c> provides no measurable benefit
/// because reference types are already passed as 8-byte pointers.
/// </para>
/// <para>
/// <b>Uninitialized contract:</b> All monadic methods in this class (<c>Map</c>, <c>Bind</c>,
/// <c>Ensure</c>, <c>Match</c>) throw <see cref="InvalidOperationException"/> when called on an
/// uninitialized <c>default(Result&lt;T&gt;)</c>, identical to the corresponding instance methods
/// on <see cref="Result{TValue}"/>. The only exceptions are
/// <see cref="TryGetValue{TValue}(in Result{TValue}, out TValue)"/> and
/// <see cref="GetValueOrDefault{TValue}(in Result{TValue}, TValue)"/>, which follow the BCL
/// <c>*OrDefault</c> convention and never throw — they return <c>false</c> / the default value
/// for any non-Success state including Uninitialized.
/// </para>
/// <para>
/// <b>Naming:</b> These are named identically to the instance methods to minimize friction.
/// The compiler selects the <c>in</c> extension method when the caller has a local variable or
/// field of type <c>Result&lt;TValue&gt;</c> and the code is <c>result.Map(...)</c> or
/// directly called as <c>ResultSyncExtensions.Map(in result, ...)</c>.
/// </para>
/// </remarks>
public static class ResultSyncExtensions
{
    // ─── Map — transform the success value without copying the struct ─────────

    /// <summary>
    /// Maps the success value of <paramref name="result"/> using <paramref name="mapper"/>.
    /// The <c>in</c> parameter avoids copying the struct — use when <typeparamref name="TValue"/>
    /// is a large value type (≥ 16 bytes).
    /// </summary>
    /// <typeparam name="TValue">The source value type.</typeparam>
    /// <typeparam name="TNext">The projected output type.</typeparam>
    /// <param name="result">The result to transform, passed by readonly reference.</param>
    /// <param name="mapper">The projection function applied to the success value.</param>
    /// <returns>A new result with the mapped value on success, or the original error on failure.</returns>
    /// <exception cref="InvalidOperationException"><paramref name="result"/> is an uninitialized default value</exception>
    [Pure]
    public static Result<TNext> Map<TValue, TNext>(
        this in Result<TValue> result,
        Func<TValue, TNext> mapper)
    {
        if (result.IsUninitialized) ResultThrowHelper.ThrowUninitializedOfT();
        return result.IsSuccess
            ? Result.Success(mapper(result.Value))
            : Result.Failure<TNext>(result.Error);
    }

    /// <summary>
    /// Maps the success value of <paramref name="result"/> using <paramref name="mapper"/>
    /// with captured <paramref name="state"/> to avoid closure allocation.
    /// The <c>in</c> parameter avoids copying the struct.
    /// </summary>
    /// <typeparam name="TValue">The source value type.</typeparam>
    /// <typeparam name="TState">The type of the state object passed to the mapper.</typeparam>
    /// <typeparam name="TNext">The projected output type.</typeparam>
    /// <param name="result">The result to transform, passed by readonly reference.</param>
    /// <param name="state">The state value passed to the mapper function.</param>
    /// <param name="mapper">The projection function applied to state and the success value.</param>
    /// <returns>A new result with the mapped value on success, or the original error on failure.</returns>
    /// <exception cref="InvalidOperationException"><paramref name="result"/> is an uninitialized default value</exception>
    [Pure]
    public static Result<TNext> Map<TValue, TState, TNext>(
        this in Result<TValue> result,
        TState state,
        Func<TState, TValue, TNext> mapper)
    {
        if (result.IsUninitialized) ResultThrowHelper.ThrowUninitializedOfT();
        return result.IsSuccess
            ? Result.Success(mapper(state, result.Value))
            : Result.Failure<TNext>(result.Error);
    }

    // ─── Bind — monadic chaining without copying the struct ───────────────────

    /// <summary>
    /// Chains a monadic bind operation on <paramref name="result"/>.
    /// The <c>in</c> parameter avoids copying the struct — use when <typeparamref name="TValue"/>
    /// is a large value type (≥ 16 bytes).
    /// </summary>
    /// <typeparam name="TValue">The source value type.</typeparam>
    /// <typeparam name="TNext">The output value type of the chained operation.</typeparam>
    /// <param name="result">The source result, passed by readonly reference.</param>
    /// <param name="bind">The operation to execute with the success value.</param>
    /// <returns>The result of executing <paramref name="bind"/> on success; otherwise, a failure with the original error.</returns>
    /// <exception cref="InvalidOperationException"><paramref name="result"/> is an uninitialized default value</exception>
    [Pure]
    public static Result<TNext> Bind<TValue, TNext>(
        this in Result<TValue> result,
        Func<TValue, Result<TNext>> bind)
    {
        if (result.IsUninitialized) ResultThrowHelper.ThrowUninitializedOfT();
        return result.IsSuccess
            ? bind(result.Value)
            : Result.Failure<TNext>(result.Error);
    }

    /// <summary>
    /// Chains a monadic bind operation on <paramref name="result"/> with captured
    /// <paramref name="state"/> to avoid closure allocation.
    /// The <c>in</c> parameter avoids copying the struct.
    /// </summary>
    /// <typeparam name="TValue">The source value type.</typeparam>
    /// <typeparam name="TState">The type of the state object passed to the bind function.</typeparam>
    /// <typeparam name="TNext">The output value type of the chained operation.</typeparam>
    /// <param name="result">The source result, passed by readonly reference.</param>
    /// <param name="state">The state value passed to the bind function.</param>
    /// <param name="bind">The operation to execute with state and the success value.</param>
    /// <returns>The result of executing <paramref name="bind"/> on success; otherwise, a failure with the original error.</returns>
    /// <exception cref="InvalidOperationException"><paramref name="result"/> is an uninitialized default value</exception>
    [Pure]
    public static Result<TNext> Bind<TValue, TState, TNext>(
        this in Result<TValue> result,
        TState state,
        Func<TState, TValue, Result<TNext>> bind)
    {
        if (result.IsUninitialized) ResultThrowHelper.ThrowUninitializedOfT();
        return result.IsSuccess
            ? bind(state, result.Value)
            : Result.Failure<TNext>(result.Error);
    }

    /// <summary>
    /// Validates the success value of <paramref name="result"/> using <paramref name="predicate"/>.
    /// Returns a failure with <paramref name="error"/> if the predicate returns <see langword="false"/>.
    /// The <c>in</c> parameter avoids copying the struct — use when <typeparamref name="TValue"/>
    /// is a large value type (≥ 16 bytes).
    /// </summary>
    /// <typeparam name="TValue">The source value type.</typeparam>
    /// <param name="result">The result to validate, passed by readonly reference.</param>
    /// <param name="predicate">The condition to test on the success value.</param>
    /// <param name="error">The error to return if the condition evaluates to <see langword="false"/>.</param>
    /// <returns>The original result if valid or failed; otherwise, a failure with <paramref name="error"/>.</returns>
    /// <exception cref="InvalidOperationException"><paramref name="result"/> is an uninitialized default value</exception>
    [Pure]
    public static Result<TValue> Ensure<TValue>(
        this in Result<TValue> result,
        Func<TValue, bool> predicate,
        Error error)
    {
        if (result.IsUninitialized) ResultThrowHelper.ThrowUninitializedOfT();
        return result.IsFailure ? result : (predicate(result.Value) ? result : Result.Failure<TValue>(error));
    }

    /// <summary>
    /// Validates the success value of <paramref name="result"/> using <paramref name="predicate"/>.
    /// Returns a failure produced by <paramref name="errorFactory"/> if the predicate returns <see langword="false"/>.
    /// The error factory is only invoked when the predicate fails — avoids allocating <see cref="Error"/> on the success path.
    /// The <c>in</c> parameter avoids copying the struct — use when <typeparamref name="TValue"/>
    /// is a large value type (≥ 16 bytes).
    /// </summary>
    /// <typeparam name="TValue">The source value type.</typeparam>
    /// <param name="result">The result to validate, passed by readonly reference.</param>
    /// <param name="predicate">The condition to test on the success value.</param>
    /// <param name="errorFactory">A factory that generates the error if the condition evaluates to <see langword="false"/>.</param>
    /// <returns>The original result if valid or failed; otherwise, a failure with the generated error.</returns>
    /// <exception cref="InvalidOperationException"><paramref name="result"/> is an uninitialized default value</exception>
    [Pure]
    public static Result<TValue> Ensure<TValue>(
        this in Result<TValue> result,
        Func<TValue, bool> predicate,
        Func<Error> errorFactory)
    {
        if (result.IsUninitialized) ResultThrowHelper.ThrowUninitializedOfT();
        return result.IsFailure ? result : (predicate(result.Value) ? result : Result.Failure<TValue>(errorFactory()));
    }

    /// <summary>
    /// Validates the success value of <paramref name="result"/> using <paramref name="predicate"/>.
    /// Returns a failure produced by <paramref name="errorFactory"/> (which receives the value) if the predicate returns <see langword="false"/>.
    /// Allows constructing context-aware errors (e.g. including the invalid value) without allocating on the success path.
    /// The <c>in</c> parameter avoids copying the struct — use when <typeparamref name="TValue"/>
    /// is a large value type (≥ 16 bytes).
    /// </summary>
    /// <typeparam name="TValue">The source value type.</typeparam>
    /// <param name="result">The result to validate, passed by readonly reference.</param>
    /// <param name="predicate">The condition to test on the success value.</param>
    /// <param name="errorFactory">A factory that receives the value and generates the error if the condition evaluates to <see langword="false"/>.</param>
    /// <returns>The original result if valid or failed; otherwise, a failure with the generated error.</returns>
    /// <exception cref="InvalidOperationException"><paramref name="result"/> is an uninitialized default value</exception>
    [Pure]
    public static Result<TValue> Ensure<TValue>(
        this in Result<TValue> result,
        Func<TValue, bool> predicate,
        Func<TValue, Error> errorFactory)
    {
        if (result.IsUninitialized) ResultThrowHelper.ThrowUninitializedOfT();
        return result.IsFailure ? result : (predicate(result.Value) ? result : Result.Failure<TValue>(errorFactory(result.Value)));
    }

    /// <summary>
    /// Validates the success value of <paramref name="result"/> using <paramref name="predicate"/>
    /// with captured <paramref name="state"/> to avoid closure allocation.
    /// The <c>in</c> parameter avoids copying the struct.
    /// </summary>
    /// <typeparam name="TValue">The source value type.</typeparam>
    /// <typeparam name="TState">The type of the state object passed to the predicate.</typeparam>
    /// <param name="result">The result to validate, passed by readonly reference.</param>
    /// <param name="state">The state value passed to the predicate.</param>
    /// <param name="predicate">The condition to test on state and the success value.</param>
    /// <param name="error">The error to return if the condition evaluates to <see langword="false"/>.</param>
    /// <returns>The original result if valid or failed; otherwise, a failure with <paramref name="error"/>.</returns>
    /// <exception cref="InvalidOperationException"><paramref name="result"/> is an uninitialized default value</exception>
    [Pure]
    public static Result<TValue> Ensure<TValue, TState>(
        this in Result<TValue> result,
        TState state,
        Func<TState, TValue, bool> predicate,
        Error error)
    {
        if (result.IsUninitialized) ResultThrowHelper.ThrowUninitializedOfT();
        return result.IsFailure ? result : (predicate(state, result.Value) ? result : Result.Failure<TValue>(error));
    }

    /// <summary>
    /// Validates the success value of <paramref name="result"/> using <paramref name="predicate"/>
    /// with captured <paramref name="state"/> to avoid closure allocation.
    /// The error factory is only invoked when the predicate fails.
    /// The <c>in</c> parameter avoids copying the struct.
    /// </summary>
    /// <typeparam name="TValue">The source value type.</typeparam>
    /// <typeparam name="TState">The type of the state object passed to the predicate and error factory.</typeparam>
    /// <param name="result">The result to validate, passed by readonly reference.</param>
    /// <param name="state">The state value passed to the predicate and error factory.</param>
    /// <param name="predicate">The condition to test on state and the success value.</param>
    /// <param name="errorFactory">A factory that receives state and generates the error if the condition evaluates to <see langword="false"/>.</param>
    /// <returns>The original result if valid or failed; otherwise, a failure with the generated error.</returns>
    /// <exception cref="InvalidOperationException"><paramref name="result"/> is an uninitialized default value</exception>
    [Pure]
    public static Result<TValue> Ensure<TValue, TState>(
        this in Result<TValue> result,
        TState state,
        Func<TState, TValue, bool> predicate,
        Func<TState, Error> errorFactory)
    {
        if (result.IsUninitialized) ResultThrowHelper.ThrowUninitializedOfT();
        return result.IsFailure ? result : (predicate(state, result.Value) ? result : Result.Failure<TValue>(errorFactory(state)));
    }

    /// <summary>
    /// Validates the success value of <paramref name="result"/> using <paramref name="predicate"/>
    /// with captured <paramref name="state"/> to avoid closure allocation.
    /// The error factory receives both the state and the value for context-aware error construction.
    /// The <c>in</c> parameter avoids copying the struct.
    /// </summary>
    /// <typeparam name="TValue">The source value type.</typeparam>
    /// <typeparam name="TState">The type of the state object passed to the predicate and error factory.</typeparam>
    /// <param name="result">The result to validate, passed by readonly reference.</param>
    /// <param name="state">The state value passed to the predicate and error factory.</param>
    /// <param name="predicate">The condition to test on state and the success value.</param>
    /// <param name="errorFactory">A factory that receives state and value to generate the error if the condition evaluates to <see langword="false"/>.</param>
    /// <returns>The original result if valid or failed; otherwise, a failure with the generated error.</returns>
    /// <exception cref="InvalidOperationException"><paramref name="result"/> is an uninitialized default value</exception>
    [Pure]
    public static Result<TValue> Ensure<TValue, TState>(
        this in Result<TValue> result,
        TState state,
        Func<TState, TValue, bool> predicate,
        Func<TState, TValue, Error> errorFactory)
    {
        if (result.IsUninitialized) ResultThrowHelper.ThrowUninitializedOfT();
        return result.IsFailure ? result : (predicate(state, result.Value) ? result : Result.Failure<TValue>(errorFactory(state, result.Value)));
    }

    // ─── Match — branch and return a value without copying the struct ─────────

    /// <summary>
    /// Evaluates <paramref name="onSuccess"/> or <paramref name="onFailure"/> based on the result state
    /// and returns the result of the chosen branch.
    /// The <c>in</c> parameter avoids copying the struct — use when <typeparamref name="TValue"/>
    /// is a large value type (≥ 16 bytes).
    /// </summary>
    /// <typeparam name="TValue">The source value type.</typeparam>
    /// <typeparam name="TOut">The output value type produced by the matching functions.</typeparam>
    /// <param name="result">The result to match on, passed by readonly reference.</param>
    /// <param name="onSuccess">The function to evaluate with the value if successful.</param>
    /// <param name="onFailure">The function to evaluate with the error if failed.</param>
    /// <returns>The value produced by the invoked branch.</returns>
    /// <exception cref="InvalidOperationException"><paramref name="result"/> is an uninitialized default value</exception>
    [Pure]
    public static TOut Match<TValue, TOut>(
        this in Result<TValue> result,
        Func<TValue, TOut> onSuccess,
        Func<Error, TOut> onFailure)
    {
        if (result.IsUninitialized) ResultThrowHelper.ThrowUninitializedOfT();
        return result.IsSuccess ? onSuccess(result.Value) : onFailure(result.Error);
    }

    /// <summary>
    /// Evaluates <paramref name="onSuccess"/> or <paramref name="onFailure"/> with captured
    /// <paramref name="state"/> to avoid closure allocation.
    /// The <c>in</c> parameter avoids copying the struct.
    /// </summary>
    /// <typeparam name="TValue">The source value type.</typeparam>
    /// <typeparam name="TState">The type of the state object passed to the matching functions.</typeparam>
    /// <typeparam name="TOut">The output value type produced by the matching functions.</typeparam>
    /// <param name="result">The result to match on, passed by readonly reference.</param>
    /// <param name="state">The state value passed to the matching delegates.</param>
    /// <param name="onSuccess">The function to evaluate with state and value if successful.</param>
    /// <param name="onFailure">The function to evaluate with state and error if failed.</param>
    /// <returns>The value produced by the invoked branch.</returns>
    /// <exception cref="InvalidOperationException"><paramref name="result"/> is an uninitialized default value</exception>
    [Pure]
    public static TOut Match<TValue, TState, TOut>(
        this in Result<TValue> result,
        TState state,
        Func<TState, TValue, TOut> onSuccess,
        Func<TState, Error, TOut> onFailure)
    {
        if (result.IsUninitialized) ResultThrowHelper.ThrowUninitializedOfT();
        return result.IsSuccess ? onSuccess(state, result.Value) : onFailure(state, result.Error);
    }

    // ─── TryGetValue — safe extraction without copying the struct ─────────────

    /// <summary>
    /// Attempts to extract the success value without throwing.
    /// The <c>in</c> parameter avoids copying the struct — use when <typeparamref name="TValue"/>
    /// is a large value type (≥ 16 bytes).
    /// </summary>
    /// <typeparam name="TValue">The source value type.</typeparam>
    /// <param name="result">The result to inspect, passed by readonly reference.</param>
    /// <param name="value">When this method returns <see langword="true"/>, contains the success value.</param>
    /// <returns><see langword="true"/> if the result is a success; otherwise <see langword="false"/>.</returns>
    /// <remarks>
    /// Per BCL <c>Try*</c> convention, this method never throws — it returns <see langword="false"/>
    /// and sets <paramref name="value"/> to <c>default!</c> for both Failure and Uninitialized states.
    /// Use <see cref="Match{TValue,TOut}(in Result{TValue},Func{TValue,TOut},Func{Error,TOut})"/>
    /// if you need to distinguish Failure from Uninitialized and want an exception for the latter.
    /// </remarks>
    public static bool TryGetValue<TValue>(
        this in Result<TValue> result,
        out TValue value)
    {
        // NOTE: Intentionally does NOT call ThrowIfUninitialized().
        // Per BCL Try* convention, this method returns false without throwing for
        // non-Success states (Failure or Uninitialized). This mirrors the design of
        // Result<TValue>.GetValueOrDefault(), which also omits ThrowIfUninitialized().
        if (result.IsSuccess)
        {
            value = result.Value;
            return true;
        }
        value = default!;
        return false;
    }

    // ─── GetValueOrDefault — safe fallback without copying the struct ─────────

    /// <summary>
    /// Returns the success value, or <paramref name="defaultValue"/> if the result is not a success.
    /// The <c>in</c> parameter avoids copying the struct — use when <typeparamref name="TValue"/>
    /// is a large value type (≥ 16 bytes).
    /// Per BCL convention, this method never throws, even for uninitialized results.
    /// </summary>
    /// <typeparam name="TValue">The source value type.</typeparam>
    /// <param name="result">The result to inspect, passed by readonly reference.</param>
    /// <param name="defaultValue">The fallback value to return if the result is not a success.</param>
    /// <returns>The success value if successful; otherwise, <paramref name="defaultValue"/>.</returns>
    /// <remarks>
    /// This method intentionally does NOT throw for an uninitialized <c>default(Result&lt;TValue&gt;)</c>.
    /// Per BCL <c>*OrDefault</c> convention (e.g., <c>Enumerable.FirstOrDefault</c>,
    /// <c>Dictionary.GetValueOrDefault</c>), methods suffixed with <c>OrDefault</c> return
    /// a fallback value rather than throw for any non-Success state.
    /// If you need to throw on Uninitialized, use
    /// <see cref="Match{TValue,TOut}(in Result{TValue},Func{TValue,TOut},Func{Error,TOut})"/> instead.
    /// </remarks>
    [Pure]
    public static TValue GetValueOrDefault<TValue>(
        this in Result<TValue> result,
        TValue defaultValue)
    {
        // NOTE: Intentionally does NOT call ThrowIfUninitialized().
        // Per BCL *OrDefault convention, this method never throws — it returns defaultValue
        // for any non-Success state (Failure or Uninitialized).
        return result.IsSuccess ? result.Value : defaultValue;
    }
}

