using System;
using System.Threading.Tasks;

namespace EricksonLopez.Result.Testing;

/// <summary>
/// Fluent testing assertion extensions for <see cref="Result"/> and <see cref="Result{T}"/>.
/// </summary>
[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
public static class ResultAssertions
{
    private static async Task<TOut> AwaitAndAssert<TIn, TOut>(Task<TIn> task, Func<TIn, TOut> assert)
    {
        var result = await task.ConfigureAwait(false);
        return assert(result);
    }

    /// <summary>
    /// Asserts that the Result is successful.
    /// </summary>
    public static Result ShouldBeSuccess(this in Result result, string? message = null)
    {
        if (result.IsFailure)
        throw new ResultAssertionException(message ?? $"Expected Result to be Success, but failed with Error: {result.Error.Code} - {result.Error.Description}");
        
        if (result.IsUninitialized)
        throw new ResultAssertionException("Expected Result to be Success, but it was uninitialized default.");
        
        return result;
    }

    /// <summary>
    /// Asserts that the Result&lt;T&gt; is successful and returns its value.
    /// </summary>
    public static T ShouldBeSuccess<T>(this in Result<T> result, string? message = null)
    {
        var typeName = GetFriendlyTypeName(typeof(T));
        if (result.IsFailure)
        throw new ResultAssertionException(message ?? $"Expected Result<{typeName}> to be Success, but failed with Error: {result.Error.Code} - {result.Error.Description}");
        
        if (result.IsUninitialized)
        throw new ResultAssertionException($"Expected Result<{typeName}> to be Success, but it was uninitialized default.");
        
        return result.Value;
    }

    /// <summary>
    /// Asserts that the Result&lt;T&gt; is successful and has the expected value.
    /// </summary>
    public static T ShouldHaveValue<T>(this in Result<T> result, T expectedValue, string? message = null)
    {
        var value = result.ShouldBeSuccess(message);
        if (!EqualityComparer<T>.Default.Equals(value, expectedValue))
        throw new ResultAssertionException(message ?? $"Expected Result to have value '{expectedValue}', but got '{value}'.");
        
        return value;
    }

    /// <summary>
    /// Asserts that the Result&lt;T&gt; is successful and that its value satisfies the given predicate.
    /// </summary>
    /// <typeparam name="T">The value type of the result.</typeparam>
    /// <param name="result">The result to inspect.</param>
    /// <param name="predicate">A predicate the value must satisfy.</param>
    /// <param name="message">Optional custom failure message.</param>
    /// <returns>The value of the successful result.</returns>
    /// <example>
    /// <code>
    /// result.ShouldHaveValue(order => order.Total > 0);
    /// result.ShouldHaveValue(list => list.Count == 3, "Expected 3 items in the list.");
    /// </code>
    /// </example>
    public static T ShouldHaveValue<T>(this in Result<T> result, Func<T, bool> predicate, string? message = null)
    {
        var value = result.ShouldBeSuccess(message);
        if (!predicate(value))
        throw new ResultAssertionException(message ?? $"Expected Result value to satisfy the predicate, but it did not. Value was: '{value}'.");
        
        return value;
    }

    /// <summary>
    /// Asserts that the Result is a failure and returns its Error.
    /// </summary>
    public static Error ShouldBeFailure(this in Result result, string? message = null)
    {
        if (result.IsSuccess)
        throw new ResultAssertionException(message ?? "Expected Result to be Failure, but it was Success.");
        
        if (result.IsUninitialized)
        throw new ResultAssertionException("Expected Result to be Failure, but it was uninitialized default.");
        
        return result.Error;
    }

    /// <summary>
    /// Asserts that the Result&lt;T&gt; is a failure and returns its Error.
    /// </summary>
    public static Error ShouldBeFailure<T>(this in Result<T> result, string? message = null)
    {
        var typeName = GetFriendlyTypeName(typeof(T));
        if (result.IsSuccess)
        throw new ResultAssertionException(message ?? $"Expected Result<{typeName}> to be Failure, but it was Success.");
        
        if (result.IsUninitialized)
        throw new ResultAssertionException($"Expected Result<{typeName}> to be Failure, but it was uninitialized default.");
        
        return result.Error;
    }

    /// <summary>
    /// Asserts that the Result failed with a specific error code.
    /// </summary>
    public static Error ShouldHaveErrorCode(this in Result result, string expectedErrorCode)
    {
        var error = result.ShouldBeFailure();
        if (error.Code != expectedErrorCode)
        throw new ResultAssertionException($"Expected error code '{expectedErrorCode}', but got '{error.Code}'.");
        
        return error;
    }

    /// <summary>
    /// Asserts that the Result&lt;T&gt; failed with a specific error code.
    /// </summary>
    public static Error ShouldHaveErrorCode<T>(this in Result<T> result, string expectedErrorCode)
    {
        var error = result.ShouldBeFailure();
        if (error.Code != expectedErrorCode)
        throw new ResultAssertionException($"Expected error code '{expectedErrorCode}', but got '{error.Code}'.");
        
        return error;
    }

    /// <summary>
    /// Asserts that the Result failed with a specific ErrorType.
    /// </summary>
    public static Error ShouldHaveErrorType(this in Result result, ErrorType expectedType)
    {
        var error = result.ShouldBeFailure();
        if (error.Type != expectedType)
        throw new ResultAssertionException($"Expected ErrorType '{expectedType}', but got '{error.Type}'.");
        
        return error;
    }

    /// <summary>
    /// Asserts that the Result&lt;T&gt; failed with a specific ErrorType.
    /// </summary>
    public static Error ShouldHaveErrorType<T>(this in Result<T> result, ErrorType expectedType)
    {
        var error = result.ShouldBeFailure();
        if (error.Type != expectedType)
        throw new ResultAssertionException($"Expected ErrorType '{expectedType}', but got '{error.Type}'.");
        
        return error;
    }

    /// <summary>
    /// Asserts that the Result failed with a specific ErrorSeverity.
    /// </summary>
    public static Error ShouldHaveSeverity(this in Result result, ErrorSeverity expectedSeverity)
    {
        var error = result.ShouldBeFailure();
        if (error.Severity != expectedSeverity)
        throw new ResultAssertionException($"Expected ErrorSeverity '{expectedSeverity}', but got '{error.Severity}'.");
        
        return error;
    }

    /// <summary>
    /// Asserts that the Result&lt;T&gt; failed with a specific ErrorSeverity.
    /// </summary>
    public static Error ShouldHaveSeverity<T>(this in Result<T> result, ErrorSeverity expectedSeverity)
    {
        var error = result.ShouldBeFailure();
        if (error.Severity != expectedSeverity)
        throw new ResultAssertionException($"Expected ErrorSeverity '{expectedSeverity}', but got '{error.Severity}'.");
        
        return error;
    }

    /// <summary>
    /// Asserts that the Result failed with a specific metadata key and value.
    /// </summary>
    public static Error ShouldHaveMetadata(this in Result result, string key, object expectedValue)
    {
        var error = result.ShouldBeFailure();
        if (!error.Metadata.TryGetValue(key, out var val) || !Equals(val, expectedValue))
        throw new ResultAssertionException($"Expected metadata key '{key}' with value '{expectedValue}', but got '{val}'.");
        
        return error;
    }

    /// <summary>
    /// Asserts that the Result&lt;T&gt; failed with a specific metadata key and value.
    /// </summary>
    public static Error ShouldHaveMetadata<T>(this in Result<T> result, string key, object expectedValue)
    {
        var error = result.ShouldBeFailure();
        if (!error.Metadata.TryGetValue(key, out var val) || !Equals(val, expectedValue))
        throw new ResultAssertionException($"Expected metadata key '{key}' with value '{expectedValue}', but got '{val}'.");
        
        return error;
    }

    // --- ShouldHaveErrorMatching ----------------------------------------------

    /// <summary>
    /// Asserts that the Result is a failure and that its error satisfies the given predicate.
    /// </summary>
    /// <param name="result">The result to inspect.</param>
    /// <param name="predicate">A predicate the error must satisfy.</param>
    /// <param name="message">Optional custom failure message.</param>
    /// <returns>The error from the failed result.</returns>
    /// <remarks>
    /// Use this when you need to verify a combination of error properties in a single assertion
    /// without breaking the fluent chain. Unlike multiple individual <c>ShouldHave*</c> assertions,
    /// this method lets you express complex conditions in a single readable predicate.
    /// </remarks>
    /// <example>
    /// <code>
    /// result.ShouldHaveErrorMatching(e => e.Code == "Order.NotFound" &amp;&amp; e.Severity == ErrorSeverity.Critical);
    /// result.ShouldHaveErrorMatching(e => e.HasMetadata &amp;&amp; e.Metadata.ContainsKey("orderId"), "Expected orderId metadata");
    /// </code>
    /// </example>
    public static Error ShouldHaveErrorMatching(this in Result result, Func<Error, bool> predicate, string? message = null)
    {
        var error = result.ShouldBeFailure(message);
        if (!predicate(error))
        throw new ResultAssertionException(message ?? $"Expected error to satisfy the predicate, but it did not. Error: [{error.Type}] {error.Code}: {error.Description}");
        
        return error;
    }

    /// <summary>
    /// Asserts that the Result&lt;T&gt; is a failure and that its error satisfies the given predicate.
    /// </summary>
    /// <typeparam name="T">The value type of the result.</typeparam>
    /// <param name="result">The result to inspect.</param>
    /// <param name="predicate">A predicate the error must satisfy.</param>
    /// <param name="message">Optional custom failure message.</param>
    /// <returns>The error from the failed result.</returns>
    public static Error ShouldHaveErrorMatching<T>(this in Result<T> result, Func<Error, bool> predicate, string? message = null)
    {
        var error = result.ShouldBeFailure(message);
        if (!predicate(error))
        throw new ResultAssertionException(message ?? $"Expected Result<{GetFriendlyTypeName(typeof(T))}> error to satisfy the predicate, but it did not. Error: [{error.Type}] {error.Code}: {error.Description}");
        
        return error;
    }

    /// <summary>
    /// Asserts that the Result is a failure and has the exact number of inner errors.
    /// </summary>
    public static Error ShouldHaveErrorCount(this in Result result, int expectedCount, string? message = null)
    {
        var error = result.ShouldBeFailure(message);
        if (error.InnerErrors.Length != expectedCount)
            throw new ResultAssertionException(message ?? $"Expected Result to have {expectedCount} inner errors, but found {error.InnerErrors.Length}.");
        return error;
    }

    /// <summary>
    /// Asserts that the Result&lt;T&gt; is a failure and has the exact number of inner errors.
    /// </summary>
    public static Error ShouldHaveErrorCount<T>(this in Result<T> result, int expectedCount, string? message = null)
    {
        var error = result.ShouldBeFailure(message);
        if (error.InnerErrors.Length != expectedCount)
            throw new ResultAssertionException(message ?? $"Expected Result to have {expectedCount} inner errors, but found {error.InnerErrors.Length}.");
        return error;
    }

    /// <summary>
    /// Asserts that the Result is a failure and its inner errors satisfy the given predicate.
    /// </summary>
    public static Error ShouldHaveInnerErrorsMatching(this in Result result, Func<System.Collections.Immutable.ImmutableArray<Error>, bool> predicate, string? message = null)
    {
        var error = result.ShouldBeFailure(message);
        if (!predicate(error.InnerErrors))
            throw new ResultAssertionException(message ?? "Expected Result inner errors to satisfy the predicate, but they did not.");
        return error;
    }

    /// <summary>
    /// Asserts that the Result&lt;T&gt; is a failure and its inner errors satisfy the given predicate.
    /// </summary>
    public static Error ShouldHaveInnerErrorsMatching<T>(this in Result<T> result, Func<System.Collections.Immutable.ImmutableArray<Error>, bool> predicate, string? message = null)
    {
        var error = result.ShouldBeFailure(message);
        if (!predicate(error.InnerErrors))
            throw new ResultAssertionException(message ?? "Expected Result inner errors to satisfy the predicate, but they did not.");
        return error;
    }


    /// <summary>Asserts that the Task&lt;Result&gt; error satisfies the given predicate.</summary>
    public static Task<Error> ShouldHaveErrorMatchingAsync(this Task<Result> task, Func<Error, bool> predicate, string? message = null)
    {
        if (task.IsCompletedSuccessfully) return Task.FromResult(task.Result.ShouldHaveErrorMatching(predicate, message));
        return AwaitAndAssert(task, r => r.ShouldHaveErrorMatching(predicate, message));
    }

    /// <summary>Asserts that the ValueTask&lt;Result&gt; error satisfies the given predicate.</summary>
    public static ValueTask<Error> ShouldHaveErrorMatchingAsync(this ValueTask<Result> task, Func<Error, bool> predicate, string? message = null)
    {
        if (task.IsCompletedSuccessfully) return new ValueTask<Error>(task.Result.ShouldHaveErrorMatching(predicate, message));
        return new ValueTask<Error>(AwaitAndAssert(task.AsTask(), r => r.ShouldHaveErrorMatching(predicate, message)));
    }

    /// <summary>Asserts that the Task&lt;Result&lt;T&gt;&gt; error satisfies the given predicate.</summary>
    public static Task<Error> ShouldHaveErrorMatchingAsync<T>(this Task<Result<T>> task, Func<Error, bool> predicate, string? message = null)
    {
        if (task.IsCompletedSuccessfully) return Task.FromResult(task.Result.ShouldHaveErrorMatching(predicate, message));
        return AwaitAndAssert(task, r => r.ShouldHaveErrorMatching(predicate, message));
    }

    /// <summary>Asserts that the ValueTask&lt;Result&lt;T&gt;&gt; error satisfies the given predicate.</summary>
    public static ValueTask<Error> ShouldHaveErrorMatchingAsync<T>(this ValueTask<Result<T>> task, Func<Error, bool> predicate, string? message = null)
    {
        if (task.IsCompletedSuccessfully) return new ValueTask<Error>(task.Result.ShouldHaveErrorMatching(predicate, message));
        return new ValueTask<Error>(AwaitAndAssert(task.AsTask(), r => r.ShouldHaveErrorMatching(predicate, message)));
    }

    // --- ShouldHaveMetadataValue<TValue> -------------------------------------

    /// <summary>
    /// Asserts that the Result is a failure and that its error metadata contains
    /// <paramref name="key"/> with a value that can be cast to <typeparamref name="TValue"/>
    /// and that equals <paramref name="expectedValue"/>.
    /// </summary>
    /// <typeparam name="TValue">The expected type of the metadata value.</typeparam>
    /// <param name="result">The result to inspect.</param>
    /// <param name="key">The metadata key to look up.</param>
    /// <param name="expectedValue">The expected value after casting to <typeparamref name="TValue"/>.</param>
    /// <param name="message">Optional custom failure message.</param>
    /// <returns>The error from the failed result.</returns>
    /// <remarks>
    /// Unlike <see cref="ShouldHaveMetadata(in Result, string, object)"/> which uses <c>object</c> equality,
    /// this method performs a typed cast first. This is necessary for value types (e.g., <c>long</c>, <c>int</c>)
    /// where boxing differences could cause <c>Equals</c> to return <c>false</c> even with identical values.
    /// </remarks>
    /// <example>
    /// <code>
    /// result.ShouldHaveMetadataValue&lt;long&gt;("orderId", 42L);
    /// result.ShouldHaveMetadataValue&lt;string&gt;("region", "us-east-1");
    /// </code>
    /// </example>
    public static Error ShouldHaveMetadataValue<TValue>(this in Result result, string key, TValue expectedValue, string? message = null)
    {
        var error = result.ShouldBeFailure(message);
        if (!error.Metadata.TryGetValue(key, out var rawVal))
        throw new ResultAssertionException(message ?? $"Expected metadata key '{key}' to be present, but it was not found. Available keys: [{string.Join(", ", error.Metadata.Keys)}]");
        
        if (rawVal is not TValue typedVal)
        throw new ResultAssertionException(message ?? $"Expected metadata key '{key}' to be of type '{typeof(TValue).Name}', but got '{rawVal?.GetType().Name ?? "<null>"}'.");
        
        else if (!EqualityComparer<TValue>.Default.Equals(typedVal, expectedValue))
        throw new ResultAssertionException(message ?? $"Expected metadata['{key}'] = '{expectedValue}' ({typeof(TValue).Name}), but got '{typedVal}'.");
        
        return error;
    }

    /// <summary>
    /// Asserts that the Result&lt;T&gt; is a failure and that its error metadata contains
    /// <paramref name="key"/> with a typed value equal to <paramref name="expectedValue"/>.
    /// </summary>
    public static Error ShouldHaveMetadataValue<T, TValue>(this in Result<T> result, string key, TValue expectedValue, string? message = null)
    {
        var error = result.ShouldBeFailure(message);
        if (!error.Metadata.TryGetValue(key, out var rawVal))
        throw new ResultAssertionException(message ?? $"Expected metadata key '{key}' to be present, but it was not found. Available keys: [{string.Join(", ", error.Metadata.Keys)}]");
        
        if (rawVal is not TValue typedVal)
        throw new ResultAssertionException(message ?? $"Expected metadata key '{key}' to be of type '{typeof(TValue).Name}', but got '{rawVal?.GetType().Name ?? "<null>"}'.");
        
        else if (!EqualityComparer<TValue>.Default.Equals(typedVal, expectedValue))
        throw new ResultAssertionException(message ?? $"Expected metadata['{key}'] = '{expectedValue}' ({typeof(TValue).Name}), but got '{typedVal}'.");
        
        return error;
    }

    // --- ShouldBeCombinedFailure ----------------------------------------------

    /// <summary>
    /// Asserts that the Result is a failure produced by <see cref="Result.Combine(ReadOnlySpan{Result})"/>
    /// (or equivalent) and that the root error contains exactly <paramref name="expectedErrorCount"/> inner errors.
    /// </summary>
    /// <param name="result">The result to inspect.</param>
    /// <param name="expectedErrorCount">The expected number of combined failures (inner errors).</param>
    /// <param name="message">Optional custom failure message.</param>
    /// <returns>The root error with its inner errors.</returns>
    /// <remarks>
    /// A combined failure is identified by its root error code being <c>WellKnownErrors.CombinedFailuresCode</c>
    /// (typically <c>"Result.Combine.Failures"</c>). If you expect a failure from <c>Combine</c> but the
    /// result has a different code (e.g., only one operation failed so it returned a direct failure),
    /// this assertion will also fail to guide you toward the correct assertion.
    /// </remarks>
    /// <example>
    /// <code>
    /// var result = Result.Combine(op1, op2, op3);
    /// var error = result.ShouldBeCombinedFailure(2); // exactly 2 of 3 operations failed
    /// </code>
    /// </example>
    public static Error ShouldBeCombinedFailure(this in Result result, int expectedErrorCount, string? message = null)
    {
        var error = result.ShouldBeFailure(message);
        if (!string.Equals(error.Code, WellKnownErrors.CombinedFailuresCode, StringComparison.Ordinal))
        {
            ResultAssertionException.Throw(
                message ?? $"Expected a combined failure (code '{WellKnownErrors.CombinedFailuresCode}'), " +
                           $"but got error code '{error.Code}'. " +
                           $"If only one operation failed, Result.Combine returns a direct failure — use ShouldHaveErrorCode() instead.");
        }
        var actualCount = error.HasInnerErrors ? error.InnerErrors.Length : 0;
        if (actualCount != expectedErrorCount)
        {
            ResultAssertionException.Throw(
                message ?? $"Expected combined failure to contain {expectedErrorCount} inner error(s), but found {actualCount}.");
        }
        return error;
    }

    /// <summary>
    /// Asserts that the Result&lt;T&gt; is a combined failure with exactly <paramref name="expectedErrorCount"/> inner errors.
    /// </summary>
    public static Error ShouldBeCombinedFailure<T>(this in Result<T> result, int expectedErrorCount, string? message = null)
    {
        var error = result.ShouldBeFailure(message);
        if (!string.Equals(error.Code, WellKnownErrors.CombinedFailuresCode, StringComparison.Ordinal))
        {
            ResultAssertionException.Throw(
                message ?? $"Expected a combined failure (code '{WellKnownErrors.CombinedFailuresCode}'), " +
                           $"but got error code '{error.Code}' in Result<{GetFriendlyTypeName(typeof(T))}>.");
        }
        var actualCount = error.HasInnerErrors ? error.InnerErrors.Length : 0;
        if (actualCount != expectedErrorCount)
        {
            ResultAssertionException.Throw(
                message ?? $"Expected Result<{GetFriendlyTypeName(typeof(T))}> combined failure to contain {expectedErrorCount} inner error(s), but found {actualCount}.");
        }
        return error;
    }

    /// <summary>Asserts that the Task&lt;Result&gt; is a combined failure with the expected inner error count.</summary>
    public static Task<Error> ShouldBeCombinedFailureAsync(this Task<Result> task, int expectedErrorCount, string? message = null)
    {
        if (task.IsCompletedSuccessfully) return Task.FromResult(task.Result.ShouldBeCombinedFailure(expectedErrorCount, message));
        return AwaitAndAssert(task, r => r.ShouldBeCombinedFailure(expectedErrorCount, message));
    }

    /// <summary>Asserts that the ValueTask&lt;Result&gt; is a combined failure with the expected inner error count.</summary>
    public static ValueTask<Error> ShouldBeCombinedFailureAsync(this ValueTask<Result> task, int expectedErrorCount, string? message = null)
    {
        if (task.IsCompletedSuccessfully) return new ValueTask<Error>(task.Result.ShouldBeCombinedFailure(expectedErrorCount, message));
        return new ValueTask<Error>(AwaitAndAssert(task.AsTask(), r => r.ShouldBeCombinedFailure(expectedErrorCount, message)));
    }

    /// <summary>Asserts that the Task&lt;Result&lt;T&gt;&gt; is a combined failure with the expected inner error count.</summary>
    public static Task<Error> ShouldBeCombinedFailureAsync<T>(this Task<Result<T>> task, int expectedErrorCount, string? message = null)
    {
        if (task.IsCompletedSuccessfully) return Task.FromResult(task.Result.ShouldBeCombinedFailure(expectedErrorCount, message));
        return AwaitAndAssert(task, r => r.ShouldBeCombinedFailure(expectedErrorCount, message));
    }

    /// <summary>Asserts that the ValueTask&lt;Result&lt;T&gt;&gt; is a combined failure with the expected inner error count.</summary>
    public static ValueTask<Error> ShouldBeCombinedFailureAsync<T>(this ValueTask<Result<T>> task, int expectedErrorCount, string? message = null)
    {
        if (task.IsCompletedSuccessfully) return new ValueTask<Error>(task.Result.ShouldBeCombinedFailure(expectedErrorCount, message));
        return new ValueTask<Error>(AwaitAndAssert(task.AsTask(), r => r.ShouldBeCombinedFailure(expectedErrorCount, message)));
    }

    /// <summary>
    /// Asserts that the Result failed with inner errors of expected count.
    /// </summary>
    public static Error ShouldHaveInnerErrors(this in Result result, int expectedCount)
    {
        var error = result.ShouldBeFailure();
        if (error.InnerErrors.Length != expectedCount)
        throw new ResultAssertionException($"Expected {expectedCount} inner errors, but got {error.InnerErrors.Length}.");
        
        return error;
    }

    /// <summary>
    /// Asserts that the Result&lt;T&gt; failed with inner errors of expected count.
    /// </summary>
    public static Error ShouldHaveInnerErrors<T>(this in Result<T> result, int expectedCount)
    {
        var error = result.ShouldBeFailure();
        if (error.InnerErrors.Length != expectedCount)
        throw new ResultAssertionException($"Expected {expectedCount} inner errors, but got {error.InnerErrors.Length}.");
        
        return error;
    }

    /// <summary>
    /// Asserts that the Result failure has no inner errors.
    /// </summary>
    /// <remarks>
    /// This is the complement of <see cref="ShouldHaveInnerErrors(in Result, int)"/>.
    /// Use it when you expect a failure with a single root error and no nested errors.
    /// </remarks>
    public static Error ShouldHaveNoInnerErrors(this in Result result, string? message = null)
    {
        var error = result.ShouldBeFailure();
        if (error.HasInnerErrors)
        throw new ResultAssertionException(message ?? $"Expected no inner errors, but got {error.InnerErrors.Length} inner error(s).");
        
        return error;
    }

    /// <summary>
    /// Asserts that the Result&lt;T&gt; failure has no inner errors.
    /// </summary>
    /// <remarks>
    /// This is the complement of <see cref="ShouldHaveInnerErrors{T}(in Result{T}, int)"/>.
    /// Use it when you expect a failure with a single root error and no nested errors.
    /// </remarks>
    public static Error ShouldHaveNoInnerErrors<T>(this in Result<T> result, string? message = null)
    {
        var error = result.ShouldBeFailure();
        if (error.HasInnerErrors)
        throw new ResultAssertionException(message ?? $"Expected no inner errors, but got {error.InnerErrors.Length} inner error(s).");
        
        return error;
    }

    // --- ShouldHaveErrorCount -------------------------------------------------

    /// <summary>
    /// Asserts that the failure result's root error has exactly <paramref name="expectedCount"/> inner errors
    /// (i.e., errors aggregated from a <see cref="Result.Combine"/> call or similar multi-error operation).
    /// </summary>
    /// <remarks>
    /// This is a named alias for <see cref="ShouldHaveInnerErrors(in Result, int)"/> for scenarios where
    /// the intent is to verify the number of combined failures without examining each individual error.
    /// <code>
    /// // Assert that 3 operations failed when combined:
    /// result.ShouldHaveErrorCount(3);
    /// </code>
    /// </remarks>
    public static Error ShouldHaveErrorCount(this in Result result, int expectedCount)
    {
        var error = result.ShouldBeFailure();
        var actualCount = error.HasInnerErrors ? error.InnerErrors.Length : 0;
        if (actualCount != expectedCount)
        {
            ResultAssertionException.Throw(
                $"Expected the result error to have {expectedCount} inner error(s), but found {actualCount}.");
        }
        return error;
    }

    /// <summary>
    /// Asserts that the Result&lt;T&gt; failure's root error has exactly <paramref name="expectedCount"/> inner errors.
    /// </summary>
    public static Error ShouldHaveErrorCount<T>(this in Result<T> result, int expectedCount)
    {
        var error = result.ShouldBeFailure();
        var actualCount = error.HasInnerErrors ? error.InnerErrors.Length : 0;
        if (actualCount != expectedCount)
        {
            ResultAssertionException.Throw(
                $"Expected the Result<{GetFriendlyTypeName(typeof(T))}> error to have {expectedCount} inner error(s), but found {actualCount}.");
        }
        return error;
    }

    // --- ShouldStrictlyEqual --------------------------------------------------

    /// <summary>
    /// Asserts that the Result failure's error is strictly equal to <paramref name="expected"/> using
    /// <see cref="Error.StrictEquals"/>, which compares ALL fields including
    /// <see cref="Error.TraceId"/>, <see cref="Error.CorrelationId"/>,
    /// <see cref="Error.InnerErrors"/>, and <see cref="Error.Metadata"/>.
    /// </summary>
    /// <remarks>
    /// Use this instead of <see cref="ShouldHaveErrorCode"/> when you need to verify
    /// that all properties of an error match exactly — for example in tests that construct errors with
    /// specific trace IDs or metadata and need to verify the complete enrichment pipeline.
    /// <para>
    /// For ordinary equality (Code, Description, Type, Severity, Retryability), use
    /// <see cref="ShouldHaveErrorCode"/> or the specialized <c>ShouldHave*</c> methods.
    /// To compare shallow equality across all five semantic fields,
    /// use <see cref="Error.Equals(Error?)"/> directly in a <see cref="ShouldSatisfyError"/> callback.
    /// </para>
    /// </remarks>
    public static Error ShouldStrictlyEqual(this in Result result, Error expected, string? message = null)
    {
        var error = result.ShouldBeFailure(message);
        if (!error.StrictEquals(expected))
        {
            ResultAssertionException.Throw(
                message ?? $"Expected error to strictly equal '{expected.Code}' (all fields), " +
                           $"but the errors differ in one or more diagnostic fields (TraceId, CorrelationId, Metadata, InnerErrors).");
        }
        return error;
    }

    /// <summary>
    /// Asserts that the Result&lt;T&gt; failure's error is strictly equal to <paramref name="expected"/>
    /// using <see cref="Error.StrictEquals"/>.
    /// </summary>
    public static Error ShouldStrictlyEqual<T>(this in Result<T> result, Error expected, string? message = null)
    {
        var error = result.ShouldBeFailure(message);
        if (!error.StrictEquals(expected))
        {
            ResultAssertionException.Throw(
                message ?? $"Expected Result<{GetFriendlyTypeName(typeof(T))}> error to strictly equal '{expected.Code}' (all fields), " +
                           $"but the errors differ in one or more diagnostic fields (TraceId, CorrelationId, Metadata, InnerErrors).");
        }
        return error;
    }

    /// <summary>
    /// Asserts that the Result error is retryable (Transient).
    /// </summary>
    public static Error ShouldBeRetryable(this in Result result)
    {
        var error = result.ShouldBeFailure();
        if (error.Retryability != ErrorRetryability.Transient)
        throw new ResultAssertionException($"Expected error to be Transient retryable, but got '{error.Retryability}'.");
        
        return error;
    }

    /// <summary>
    /// Asserts that the Result&lt;T&gt; error is retryable (Transient).
    /// </summary>
    public static Error ShouldBeRetryable<T>(this in Result<T> result)
    {
        var error = result.ShouldBeFailure();
        if (error.Retryability != ErrorRetryability.Transient)
        throw new ResultAssertionException($"Expected error to be Transient retryable, but got '{error.Retryability}'.");
        
        return error;
    }

    /// <summary>
    /// Asserts that the Result error is permanent (not retryable).
    /// </summary>
    public static Error ShouldBePermanent(this in Result result)
    {
        var error = result.ShouldBeFailure();
        if (error.Retryability != ErrorRetryability.Permanent)
        throw new ResultAssertionException($"Expected error to be Permanent (not retryable), but got '{error.Retryability}'.");
        
        return error;
    }

    /// <summary>
    /// Asserts that the Result&lt;T&gt; error is permanent (not retryable).
    /// </summary>
    public static Error ShouldBePermanent<T>(this in Result<T> result)
    {
        var error = result.ShouldBeFailure();
        if (error.Retryability != ErrorRetryability.Permanent)
        throw new ResultAssertionException($"Expected error to be Permanent (not retryable), but got '{error.Retryability}'.");
        
        return error;
    }

    /// <summary>
    /// Asserts that the Result error has the expected OpenTelemetry TraceId.
    /// </summary>
    public static Error ShouldHaveTraceId(this in Result result, string expectedTraceId)
    {
        var error = result.ShouldBeFailure();
        if (!string.Equals(error.TraceId, expectedTraceId, StringComparison.Ordinal))
        throw new ResultAssertionException($"Expected error TraceId to be '{expectedTraceId}', but got '{error.TraceId ?? "<null>"}'");
        
        return error;
    }

    /// <summary>
    /// Asserts that the Result&lt;T&gt; error has the expected OpenTelemetry TraceId.
    /// </summary>
    public static Error ShouldHaveTraceId<T>(this in Result<T> result, string expectedTraceId)
    {
        var error = result.ShouldBeFailure();
        if (!string.Equals(error.TraceId, expectedTraceId, StringComparison.Ordinal))
        throw new ResultAssertionException($"Expected error TraceId to be '{expectedTraceId}', but got '{error.TraceId ?? "<null>"}'");
        
        return error;
    }

    /// <summary>
    /// Asserts that the Result error has the expected CorrelationId.
    /// </summary>
    public static Error ShouldHaveCorrelationId(this in Result result, string expectedCorrelationId)
    {
        var error = result.ShouldBeFailure();
        if (!string.Equals(error.CorrelationId, expectedCorrelationId, StringComparison.Ordinal))
        throw new ResultAssertionException($"Expected error CorrelationId to be '{expectedCorrelationId}', but got '{error.CorrelationId ?? "<null>"}'");
        
        return error;
    }

    /// <summary>
    /// Asserts that the Result&lt;T&gt; error has the expected CorrelationId.
    /// </summary>
    public static Error ShouldHaveCorrelationId<T>(this in Result<T> result, string expectedCorrelationId)
    {
        var error = result.ShouldBeFailure();
        if (!string.Equals(error.CorrelationId, expectedCorrelationId, StringComparison.Ordinal))
        throw new ResultAssertionException($"Expected error CorrelationId to be '{expectedCorrelationId}', but got '{error.CorrelationId ?? "<null>"}'");
        
        return error;
    }

    /// <summary>
    /// Asserts that the Result error has the expected description.
    /// </summary>
    public static Error ShouldHaveDescription(this in Result result, string expectedDescription)
    {
        var error = result.ShouldBeFailure();
        if (!string.Equals(error.Description, expectedDescription, StringComparison.Ordinal))
        throw new ResultAssertionException($"Expected error Description to be '{expectedDescription}', but got '{error.Description}'.");
        
        return error;
    }

    /// <summary>
    /// Asserts that the Result&lt;T&gt; error has the expected description.
    /// </summary>
    public static Error ShouldHaveDescription<T>(this in Result<T> result, string expectedDescription)
    {
        var error = result.ShouldBeFailure();
        if (!string.Equals(error.Description, expectedDescription, StringComparison.Ordinal))
        throw new ResultAssertionException($"Expected error Description to be '{expectedDescription}', but got '{error.Description}'.");
        
        return error;
    }

    /// <summary>
    /// Asserts that the Result error has at least one inner error with the specified error code.
    /// </summary>
    public static Error ShouldContainInnerError(this in Result result, string errorCode)
    {
        var error = result.ShouldBeFailure();
        if (!error.HasInnerErrors)
            throw new ResultAssertionException($"Expected at least one inner error with code '{errorCode}', but none was found.");

        var innerErrors = error.InnerErrors;
        for (int i = 0; i < innerErrors.Length; i++)
        {
            if (string.Equals(innerErrors[i].Code, errorCode, StringComparison.Ordinal))
                return error;
        }

        throw new ResultAssertionException($"Expected at least one inner error with code '{errorCode}', but none was found.");
    }

    /// <summary>
    /// Asserts that the Result&lt;T&gt; error has at least one inner error with the specified error code.
    /// </summary>
    public static Error ShouldContainInnerError<T>(this in Result<T> result, string errorCode)
    {
        var error = result.ShouldBeFailure();
        if (!error.HasInnerErrors)
            throw new ResultAssertionException($"Expected at least one inner error with code '{errorCode}', but none was found.");

        var innerErrors = error.InnerErrors;
        for (int i = 0; i < innerErrors.Length; i++)
        {
            if (string.Equals(innerErrors[i].Code, errorCode, StringComparison.Ordinal))
                return error;
        }

        throw new ResultAssertionException($"Expected at least one inner error with code '{errorCode}', but none was found.");
    }

    // --- ShouldBeUninitialized ------------------------------------------------

    /// <summary>
    /// Asserts that the Result is in the uninitialized default state
    /// (i.e., <c>default(Result)</c> was never replaced by <see cref="Result.Success()"/> or <see cref="Result.Failure"/>).
    /// </summary>
    /// <remarks>
    /// Use this assertion to verify that code paths that should always initialize a Result
    /// do not accidentally return <c>default(Result)</c>. The uninitialized state is neither
    /// Success nor Failure — it is a third sentinel state that typically indicates a programming error.
    /// </remarks>
    public static Result ShouldBeUninitialized(this in Result result, string? message = null)
    {
        if (!result.IsUninitialized)
        {
            var state = result.IsSuccess ? "Success" : "Failure";
            throw new ResultAssertionException(message ?? $"Expected Result to be Uninitialized, but it was {state}.");
        }
        return result;
    }

    /// <summary>
    /// Asserts that the Result&lt;T&gt; is in the uninitialized default state.
    /// </summary>
    /// <remarks>
    /// Use this assertion to verify that code paths that should always initialize a Result&lt;T&gt;
    /// do not accidentally return <c>default(Result&lt;T&gt;)</c>.
    /// </remarks>
    public static Result<T> ShouldBeUninitialized<T>(this in Result<T> result, string? message = null)
    {
        if (!result.IsUninitialized)
        {
            var typeName = GetFriendlyTypeName(typeof(T));
            var state = result.IsSuccess ? "Success" : "Failure";
            throw new ResultAssertionException(message ?? $"Expected Result<{typeName}> to be Uninitialized, but it was {state}.");
        }
        return result;
    }

    // --- ShouldNotHaveInnerErrors ---------------------------------------------

    /// <summary>
    /// Asserts that the Result failure's error has no inner errors.
    /// This is the inverse of <see cref="ShouldHaveInnerErrors(in Result, int)"/>.
    /// </summary>
    public static Error ShouldNotHaveInnerErrors(this in Result result, string? message = null)
    {
        var error = result.ShouldBeFailure(message);
        if (error.HasInnerErrors)
        {
            ResultAssertionException.Throw(
                message ?? $"Expected error '{error.Code}' to have no inner errors, but found {error.InnerErrors.Length}.");
        }
        return error;
    }

    /// <summary>
    /// Asserts that the Result&lt;T&gt; failure's error has no inner errors.
    /// </summary>
    public static Error ShouldNotHaveInnerErrors<T>(this in Result<T> result, string? message = null)
    {
        var error = result.ShouldBeFailure(message);
        if (error.HasInnerErrors)
        {
            ResultAssertionException.Throw(
                message ?? $"Expected Result<{GetFriendlyTypeName(typeof(T))}> error '{error.Code}' to have no inner errors, but found {error.InnerErrors.Length}.");
        }
        return error;
    }

    // --- ShouldHaveInnerErrorCount --------------------------------------------

    /// <summary>
    /// Asserts that the Result failure's error has exactly <paramref name="expectedCount"/> inner errors.
    /// Useful for verifying <see cref="Result.Combine(ReadOnlySpan{Result})"/> produced the expected number of collected failures.
    /// </summary>
    public static Error ShouldHaveInnerErrorCount(this in Result result, int expectedCount, string? message = null)
    {
        var error = result.ShouldBeFailure(message);
        var actualCount = error.HasInnerErrors ? error.InnerErrors.Length : 0;
        if (actualCount != expectedCount)
        {
            ResultAssertionException.Throw(
                message ?? $"Expected error '{error.Code}' to have exactly {expectedCount} inner error(s), but found {actualCount}.");
        }
        return error;
    }

    /// <summary>
    /// Asserts that the Result&lt;T&gt; failure's error has exactly <paramref name="expectedCount"/> inner errors.
    /// </summary>
    public static Error ShouldHaveInnerErrorCount<T>(this in Result<T> result, int expectedCount, string? message = null)
    {
        var error = result.ShouldBeFailure(message);
        var actualCount = error.HasInnerErrors ? error.InnerErrors.Length : 0;
        if (actualCount != expectedCount)
        {
            ResultAssertionException.Throw(
                message ?? $"Expected Result<{GetFriendlyTypeName(typeof(T))}> error '{error.Code}' to have exactly {expectedCount} inner error(s), but found {actualCount}.");
        }
        return error;
    }

    /// <summary>Asserts that the Task&lt;Result&gt; is in the uninitialized default state.</summary>
    public static Task<Result> ShouldBeUninitializedAsync(this Task<Result> task, string? message = null)
    {
        if (task.IsCompletedSuccessfully) return Task.FromResult(task.Result.ShouldBeUninitialized(message));
        return AwaitAndAssert(task, r => r.ShouldBeUninitialized(message));
    }

    /// <summary>Asserts that the ValueTask&lt;Result&gt; is in the uninitialized default state.</summary>
    public static ValueTask<Result> ShouldBeUninitializedAsync(this ValueTask<Result> task, string? message = null)
    {
        if (task.IsCompletedSuccessfully) return new ValueTask<Result>(task.Result.ShouldBeUninitialized(message));
        return new ValueTask<Result>(AwaitAndAssert(task.AsTask(), r => r.ShouldBeUninitialized(message)));
    }

    /// <summary>Asserts that the Task&lt;Result&lt;T&gt;&gt; is in the uninitialized default state.</summary>
    public static Task<Result<T>> ShouldBeUninitializedAsync<T>(this Task<Result<T>> task, string? message = null)
    {
        if (task.IsCompletedSuccessfully) return Task.FromResult(task.Result.ShouldBeUninitialized(message));
        return AwaitAndAssert(task, r => r.ShouldBeUninitialized(message));
    }

    /// <summary>Asserts that the ValueTask&lt;Result&lt;T&gt;&gt; is in the uninitialized default state.</summary>
    public static ValueTask<Result<T>> ShouldBeUninitializedAsync<T>(this ValueTask<Result<T>> task, string? message = null)
    {
        if (task.IsCompletedSuccessfully) return new ValueTask<Result<T>>(task.Result.ShouldBeUninitialized(message));
        return new ValueTask<Result<T>>(AwaitAndAssert(task.AsTask(), r => r.ShouldBeUninitialized(message)));
    }

    /// <summary>Asserts that the Task&lt;Result&gt; failure's error has no inner errors.</summary>
    public static Task<Error> ShouldNotHaveInnerErrorsAsync(this Task<Result> task, string? message = null)
    {
        if (task.IsCompletedSuccessfully) return Task.FromResult(task.Result.ShouldNotHaveInnerErrors(message));
        return AwaitAndAssert(task, r => r.ShouldNotHaveInnerErrors(message));
    }

    /// <summary>Asserts that the ValueTask&lt;Result&gt; failure's error has no inner errors.</summary>
    public static ValueTask<Error> ShouldNotHaveInnerErrorsAsync(this ValueTask<Result> task, string? message = null)
    {
        if (task.IsCompletedSuccessfully) return new ValueTask<Error>(task.Result.ShouldNotHaveInnerErrors(message));
        return new ValueTask<Error>(AwaitAndAssert(task.AsTask(), r => r.ShouldNotHaveInnerErrors(message)));
    }

    /// <summary>Asserts that the Task&lt;Result&lt;T&gt;&gt; failure's error has no inner errors.</summary>
    public static Task<Error> ShouldNotHaveInnerErrorsAsync<T>(this Task<Result<T>> task, string? message = null)
    {
        if (task.IsCompletedSuccessfully) return Task.FromResult(task.Result.ShouldNotHaveInnerErrors(message));
        return AwaitAndAssert(task, r => r.ShouldNotHaveInnerErrors(message));
    }

    /// <summary>Asserts that the ValueTask&lt;Result&lt;T&gt;&gt; failure's error has no inner errors.</summary>
    public static ValueTask<Error> ShouldNotHaveInnerErrorsAsync<T>(this ValueTask<Result<T>> task, string? message = null)
    {
        if (task.IsCompletedSuccessfully) return new ValueTask<Error>(task.Result.ShouldNotHaveInnerErrors(message));
        return new ValueTask<Error>(AwaitAndAssert(task.AsTask(), r => r.ShouldNotHaveInnerErrors(message)));
    }

    /// <summary>Asserts that the Task&lt;Result&gt; failure's error has exactly <paramref name="expectedCount"/> inner errors.</summary>
    public static Task<Error> ShouldHaveInnerErrorCountAsync(this Task<Result> task, int expectedCount, string? message = null)
    {
        if (task.IsCompletedSuccessfully) return Task.FromResult(task.Result.ShouldHaveInnerErrorCount(expectedCount, message));
        return AwaitAndAssert(task, r => r.ShouldHaveInnerErrorCount(expectedCount, message));
    }

    /// <summary>Asserts that the ValueTask&lt;Result&gt; failure's error has exactly <paramref name="expectedCount"/> inner errors.</summary>
    public static ValueTask<Error> ShouldHaveInnerErrorCountAsync(this ValueTask<Result> task, int expectedCount, string? message = null)
    {
        if (task.IsCompletedSuccessfully) return new ValueTask<Error>(task.Result.ShouldHaveInnerErrorCount(expectedCount, message));
        return new ValueTask<Error>(AwaitAndAssert(task.AsTask(), r => r.ShouldHaveInnerErrorCount(expectedCount, message)));
    }

    /// <summary>Asserts that the Task&lt;Result&lt;T&gt;&gt; failure's error has exactly <paramref name="expectedCount"/> inner errors.</summary>
    public static Task<Error> ShouldHaveInnerErrorCountAsync<T>(this Task<Result<T>> task, int expectedCount, string? message = null)
    {
        if (task.IsCompletedSuccessfully) return Task.FromResult(task.Result.ShouldHaveInnerErrorCount(expectedCount, message));
        return AwaitAndAssert(task, r => r.ShouldHaveInnerErrorCount(expectedCount, message));
    }

    /// <summary>Asserts that the ValueTask&lt;Result&lt;T&gt;&gt; failure's error has exactly <paramref name="expectedCount"/> inner errors.</summary>
    public static ValueTask<Error> ShouldHaveInnerErrorCountAsync<T>(this ValueTask<Result<T>> task, int expectedCount, string? message = null)
    {
        if (task.IsCompletedSuccessfully) return new ValueTask<Error>(task.Result.ShouldHaveInnerErrorCount(expectedCount, message));
        return new ValueTask<Error>(AwaitAndAssert(task.AsTask(), r => r.ShouldHaveInnerErrorCount(expectedCount, message)));
    }

    // --- ShouldHaveMetadataKey / ShouldNotHaveMetadata ------------------------

    /// <summary>
    /// Asserts that the Result error contains a metadata entry with the specified key,
    /// regardless of the value. Use <see cref="ShouldHaveMetadata(in Result, string, object)"/>
    /// to also assert the value.
    /// </summary>
    public static Error ShouldHaveMetadataKey(this in Result result, string key)
    {
        var error = result.ShouldBeFailure();
        if (!error.HasMetadata || !error.Metadata.ContainsKey(key))
        throw new ResultAssertionException($"Expected error metadata to contain key '{key}', but it was not found.");
        
        return error;
    }

    /// <summary>
    /// Asserts that the Result&lt;T&gt; error contains a metadata entry with the specified key,
    /// regardless of the value. Use <see cref="ShouldHaveMetadata{T}(in Result{T}, string, object)"/>
    /// to also assert the value.
    /// </summary>
    public static Error ShouldHaveMetadataKey<T>(this in Result<T> result, string key)
    {
        var error = result.ShouldBeFailure();
        if (!error.HasMetadata || !error.Metadata.ContainsKey(key))
        throw new ResultAssertionException($"Expected error metadata to contain key '{key}', but it was not found.");
        
        return error;
    }

    /// <summary>
    /// Asserts that the Result error does <b>not</b> contain a metadata entry with the specified key.
    /// </summary>
    public static Error ShouldNotHaveMetadata(this in Result result, string key)
    {
        var error = result.ShouldBeFailure();
        if (error.HasMetadata && error.Metadata.ContainsKey(key))
        throw new ResultAssertionException($"Expected error metadata to NOT contain key '{key}', but it was present with value '{error.Metadata[key]}'.");
        
        return error;
    }

    /// <summary>
    /// Asserts that the Result&lt;T&gt; error does <b>not</b> contain a metadata entry with the specified key.
    /// </summary>
    public static Error ShouldNotHaveMetadata<T>(this in Result<T> result, string key)
    {
        var error = result.ShouldBeFailure();
        if (error.HasMetadata && error.Metadata.ContainsKey(key))
        throw new ResultAssertionException($"Expected error metadata to NOT contain key '{key}', but it was present with value '{error.Metadata[key]}'.");
        
        return error;
    }

    // --- ShouldSatisfy --------------------------------------------------------

    /// <summary>
    /// Asserts that the non-generic Result is successful and executes a custom assertion action.
    /// </summary>
    /// <param name="result">The result to assert on.</param>
    /// <param name="assertion">
    /// An action that performs custom assertions on the result. Throw any exception to indicate failure.
    /// </param>
    /// <param name="message">Optional failure message if the result is not successful.</param>
    /// <returns>The result for further assertions.</returns>
    /// <example>
    /// <code>
    /// result.ShouldSatisfy(r =>
    /// {
    ///     Assert.True(r.IsSuccess);
    /// });
    /// </code>
    /// </example>
    public static Result ShouldSatisfy(this in Result result, Action<Result> assertion, string? message = null)
    {
        result.ShouldBeSuccess(message);
        assertion(result);
        return result;
    }

    /// <summary>
    /// Asserts that the Task&lt;Result&gt; is successful and executes a custom assertion action.
    /// </summary>
    public static Task<Result> ShouldSatisfyAsync(this Task<Result> task, Action<Result> assertion, string? message = null)
    {
        if (task.IsCompletedSuccessfully) return Task.FromResult(task.Result.ShouldSatisfy(assertion, message));
        return AwaitAndAssert(task, r => r.ShouldSatisfy(assertion, message));
    }

    /// <summary>
    /// Asserts that the ValueTask&lt;Result&gt; is successful and executes a custom assertion action.
    /// </summary>
    public static ValueTask<Result> ShouldSatisfyAsync(this ValueTask<Result> task, Action<Result> assertion, string? message = null)
    {
        if (task.IsCompletedSuccessfully) return new ValueTask<Result>(task.Result.ShouldSatisfy(assertion, message));
        return new ValueTask<Result>(AwaitAndAssert(task.AsTask(), r => r.ShouldSatisfy(assertion, message)));
    }

    /// <summary>
    /// Asserts that the Result&lt;T&gt; is successful and that its value satisfies the specified assertion action.
    /// </summary>
    /// <typeparam name="T">The result value type.</typeparam>
    /// <param name="result">The result to assert on.</param>
    /// <param name="assertion">
    /// An action that performs custom assertions on the value. Throw any exception to indicate failure.
    /// </param>
    /// <param name="message">Optional failure message if the result is not successful.</param>
    /// <returns>The value for further assertions.</returns>
    /// <example>
    /// <code>
    /// result.ShouldSatisfy&lt;OrderDto&gt;(order =>
    /// {
    ///     Assert.Equal("ORD-001", order.OrderId);
    ///     Assert.True(order.Items.Count > 0);
    /// });
    /// </code>
    /// </example>
    public static T ShouldSatisfy<T>(this in Result<T> result, Action<T> assertion, string? message = null)
    {
        var value = result.ShouldBeSuccess(message);
        assertion(value);
        return value;
    }

    /// <summary>
    /// Asserts that the Task&lt;Result&lt;T&gt;&gt; is successful and that its value satisfies the specified assertion action.
    /// </summary>
    public static Task<T> ShouldSatisfyAsync<T>(this Task<Result<T>> task, Action<T> assertion, string? message = null)
    {
        if (task.IsCompletedSuccessfully) return Task.FromResult(task.Result.ShouldSatisfy(assertion, message));
        return AwaitAndAssert(task, r => r.ShouldSatisfy(assertion, message));
    }

    /// <summary>
    /// Asserts that the ValueTask&lt;Result&lt;T&gt;&gt; is successful and that its value satisfies the specified assertion action.
    /// </summary>
    public static ValueTask<T> ShouldSatisfyAsync<T>(this ValueTask<Result<T>> task, Action<T> assertion, string? message = null)
    {
        if (task.IsCompletedSuccessfully) return new ValueTask<T>(task.Result.ShouldSatisfy(assertion, message));
        return new ValueTask<T>(AwaitAndAssert(task.AsTask(), r => r.ShouldSatisfy(assertion, message)));
    }

    // --- ShouldSatisfyError --------------------------------------------------

    /// <summary>
    /// Asserts that the non-generic Result is a failure and executes a custom assertion action on the error.
    /// </summary>
    /// <param name="result">The result to assert on.</param>
    /// <param name="assertion">
    /// An action that performs custom assertions on the error. Throw any exception to indicate failure.
    /// </param>
    /// <param name="message">Optional failure message if the result is not a failure.</param>
    /// <returns>The error for further chained assertions.</returns>
    /// <example>
    /// <code>
    /// result.ShouldSatisfyError(error =>
    /// {
    ///     Assert.Equal("NOT_FOUND", error.Code);
    ///     Assert.True(error.HasMetadata);
    /// });
    /// </code>
    /// </example>
    public static Error ShouldSatisfyError(this in Result result, Action<Error> assertion, string? message = null)
    {
        var error = result.ShouldBeFailure(message);
        assertion(error);
        return error;
    }

    /// <summary>
    /// Asserts that the Result&lt;T&gt; is a failure and executes a custom assertion action on the error.
    /// </summary>
    /// <typeparam name="T">The result value type.</typeparam>
    /// <param name="result">The result to assert on.</param>
    /// <param name="assertion">
    /// An action that performs custom assertions on the error. Throw any exception to indicate failure.
    /// </param>
    /// <param name="message">Optional failure message if the result is not a failure.</param>
    /// <returns>The error for further chained assertions.</returns>
    public static Error ShouldSatisfyError<T>(this in Result<T> result, Action<Error> assertion, string? message = null)
    {
        var error = result.ShouldBeFailure(message);
        assertion(error);
        return error;
    }

    /// <summary>Asserts that Task&lt;Result&gt; is a failure and the error satisfies the assertion.</summary>
    public static Task<Error> ShouldSatisfyErrorAsync(this Task<Result> task, Action<Error> assertion, string? message = null)
    {
        if (task.IsCompletedSuccessfully) return Task.FromResult(task.Result.ShouldSatisfyError(assertion, message));
        return AwaitAndAssert(task, r => r.ShouldSatisfyError(assertion, message));
    }

    /// <summary>Asserts that Task&lt;Result&lt;T&gt;&gt; is a failure and the error satisfies the assertion.</summary>
    public static Task<Error> ShouldSatisfyErrorAsync<T>(this Task<Result<T>> task, Action<Error> assertion, string? message = null)
    {
        if (task.IsCompletedSuccessfully) return Task.FromResult(task.Result.ShouldSatisfyError(assertion, message));
        return AwaitAndAssert(task, r => r.ShouldSatisfyError(assertion, message));
    }

    /// <summary>Asserts that ValueTask&lt;Result&gt; is a failure and the error satisfies the assertion.</summary>
    public static ValueTask<Error> ShouldSatisfyErrorAsync(this ValueTask<Result> task, Action<Error> assertion, string? message = null)
    {
        if (task.IsCompletedSuccessfully) return new ValueTask<Error>(task.Result.ShouldSatisfyError(assertion, message));
        return new ValueTask<Error>(AwaitAndAssert(task.AsTask(), r => r.ShouldSatisfyError(assertion, message)));
    }

    /// <summary>Asserts that ValueTask&lt;Result&lt;T&gt;&gt; is a failure and the error satisfies the assertion.</summary>
    public static ValueTask<Error> ShouldSatisfyErrorAsync<T>(this ValueTask<Result<T>> task, Action<Error> assertion, string? message = null)
    {
        if (task.IsCompletedSuccessfully) return new ValueTask<Error>(task.Result.ShouldSatisfyError(assertion, message));
        return new ValueTask<Error>(AwaitAndAssert(task.AsTask(), r => r.ShouldSatisfyError(assertion, message)));
    }

    // --- Async Assertions -----------------------------------------------------

    /// <summary>
    /// Async assertion that Task&lt;Result&gt; is successful.
    /// </summary>
    public static Task<Result> ShouldBeSuccessAsync(this Task<Result> task, string? message = null)
    {
        if (task.IsCompletedSuccessfully) return Task.FromResult(task.Result.ShouldBeSuccess(message));
        return AwaitAndAssert(task, r => r.ShouldBeSuccess(message));
    }

    /// <summary>
    /// Async assertion that Task&lt;Result&lt;T&gt;&gt; is successful and returns value.
    /// </summary>
    public static Task<T> ShouldBeSuccessAsync<T>(this Task<Result<T>> task, string? message = null)
    {
        if (task.IsCompletedSuccessfully) return Task.FromResult(task.Result.ShouldBeSuccess(message));
        return AwaitAndAssert(task, r => r.ShouldBeSuccess(message));
    }

    /// <summary>
    /// Async assertion that Task&lt;Result&gt; is a failure.
    /// </summary>
    public static Task<Error> ShouldBeFailureAsync(this Task<Result> task, string? message = null)
    {
        if (task.IsCompletedSuccessfully) return Task.FromResult(task.Result.ShouldBeFailure(message));
        return AwaitAndAssert(task, r => r.ShouldBeFailure(message));
    }

    /// <summary>
    /// Async assertion that Task&lt;Result&lt;T&gt;&gt; is a failure.
    /// </summary>
    public static Task<Error> ShouldBeFailureAsync<T>(this Task<Result<T>> task, string? message = null)
    {
        if (task.IsCompletedSuccessfully) return Task.FromResult(task.Result.ShouldBeFailure(message));
        return AwaitAndAssert(task, r => r.ShouldBeFailure(message));
    }

    /// <summary>
    /// Async assertion that ValueTask&lt;Result&gt; is successful.
    /// </summary>
    public static ValueTask<Result> ShouldBeSuccessAsync(this ValueTask<Result> task, string? message = null)
    {
        if (task.IsCompletedSuccessfully) return new ValueTask<Result>(task.Result.ShouldBeSuccess(message));
        return new ValueTask<Result>(AwaitAndAssert(task.AsTask(), r => r.ShouldBeSuccess(message)));
    }

    /// <summary>
    /// Async assertion that ValueTask&lt;Result&lt;T&gt;&gt; is successful and returns value.
    /// </summary>
    public static ValueTask<T> ShouldBeSuccessAsync<T>(this ValueTask<Result<T>> task, string? message = null)
    {
        if (task.IsCompletedSuccessfully) return new ValueTask<T>(task.Result.ShouldBeSuccess(message));
        return new ValueTask<T>(AwaitAndAssert(task.AsTask(), r => r.ShouldBeSuccess(message)));
    }

    /// <summary>
    /// Async assertion that ValueTask&lt;Result&gt; is a failure.
    /// </summary>
    public static ValueTask<Error> ShouldBeFailureAsync(this ValueTask<Result> task, string? message = null)
    {
        if (task.IsCompletedSuccessfully) return new ValueTask<Error>(task.Result.ShouldBeFailure(message));
        return new ValueTask<Error>(AwaitAndAssert(task.AsTask(), r => r.ShouldBeFailure(message)));
    }

    /// <summary>
    /// Async assertion that ValueTask&lt;Result&lt;T&gt;&gt; is a failure.
    /// </summary>
    public static ValueTask<Error> ShouldBeFailureAsync<T>(this ValueTask<Result<T>> task, string? message = null)
    {
        if (task.IsCompletedSuccessfully) return new ValueTask<Error>(task.Result.ShouldBeFailure(message));
        return new ValueTask<Error>(AwaitAndAssert(task.AsTask(), r => r.ShouldBeFailure(message)));
    }

    // --- Chained Async Error Assertions --------------------------------------
    // Eliminates the two-step "await result; result.ShouldHave*()" pattern.

    /// <summary>Asserts that Task&lt;Result&gt; is a failure with the given error code.</summary>
    public static Task<Error> ShouldHaveErrorCodeAsync(this Task<Result> task, string expectedCode)
    {
        if (task.IsCompletedSuccessfully) return Task.FromResult(task.Result.ShouldHaveErrorCode(expectedCode));
        return AwaitAndAssert(task, r => r.ShouldHaveErrorCode(expectedCode));
    }

    /// <summary>Asserts that Task&lt;Result&lt;T&gt;&gt; is a failure with the given error code.</summary>
    public static Task<Error> ShouldHaveErrorCodeAsync<T>(this Task<Result<T>> task, string expectedCode)
    {
        if (task.IsCompletedSuccessfully) return Task.FromResult(task.Result.ShouldHaveErrorCode(expectedCode));
        return AwaitAndAssert(task, r => r.ShouldHaveErrorCode(expectedCode));
    }

    /// <summary>Asserts that ValueTask&lt;Result&gt; is a failure with the given error code.</summary>
    public static ValueTask<Error> ShouldHaveErrorCodeAsync(this ValueTask<Result> task, string expectedCode)
    {
        if (task.IsCompletedSuccessfully) return new ValueTask<Error>(task.Result.ShouldHaveErrorCode(expectedCode));
        return new ValueTask<Error>(AwaitAndAssert(task.AsTask(), r => r.ShouldHaveErrorCode(expectedCode)));
    }

    /// <summary>Asserts that ValueTask&lt;Result&lt;T&gt;&gt; is a failure with the given error code.</summary>
    public static ValueTask<Error> ShouldHaveErrorCodeAsync<T>(this ValueTask<Result<T>> task, string expectedCode)
    {
        if (task.IsCompletedSuccessfully) return new ValueTask<Error>(task.Result.ShouldHaveErrorCode(expectedCode));
        return new ValueTask<Error>(AwaitAndAssert(task.AsTask(), r => r.ShouldHaveErrorCode(expectedCode)));
    }

    /// <summary>Asserts that Task&lt;Result&gt; is a failure with the given error type.</summary>
    public static Task<Error> ShouldHaveErrorTypeAsync(this Task<Result> task, ErrorType expectedType)
    {
        if (task.IsCompletedSuccessfully) return Task.FromResult(task.Result.ShouldHaveErrorType(expectedType));
        return AwaitAndAssert(task, r => r.ShouldHaveErrorType(expectedType));
    }

    /// <summary>Asserts that Task&lt;Result&lt;T&gt;&gt; is a failure with the given error type.</summary>
    public static Task<Error> ShouldHaveErrorTypeAsync<T>(this Task<Result<T>> task, ErrorType expectedType)
    {
        if (task.IsCompletedSuccessfully) return Task.FromResult(task.Result.ShouldHaveErrorType(expectedType));
        return AwaitAndAssert(task, r => r.ShouldHaveErrorType(expectedType));
    }

    /// <summary>Asserts that ValueTask&lt;Result&gt; is a failure with the given error type.</summary>
    public static ValueTask<Error> ShouldHaveErrorTypeAsync(this ValueTask<Result> task, ErrorType expectedType)
    {
        if (task.IsCompletedSuccessfully) return new ValueTask<Error>(task.Result.ShouldHaveErrorType(expectedType));
        return new ValueTask<Error>(AwaitAndAssert(task.AsTask(), r => r.ShouldHaveErrorType(expectedType)));
    }

    /// <summary>Asserts that ValueTask&lt;Result&lt;T&gt;&gt; is a failure with the given error type.</summary>
    public static ValueTask<Error> ShouldHaveErrorTypeAsync<T>(this ValueTask<Result<T>> task, ErrorType expectedType)
    {
        if (task.IsCompletedSuccessfully) return new ValueTask<Error>(task.Result.ShouldHaveErrorType(expectedType));
        return new ValueTask<Error>(AwaitAndAssert(task.AsTask(), r => r.ShouldHaveErrorType(expectedType)));
    }

    /// <summary>Asserts that Task&lt;Result&lt;T&gt;&gt; is successful and has the expected value.</summary>
    public static Task<T> ShouldHaveValueAsync<T>(this Task<Result<T>> task, T expectedValue, string? message = null)
    {
        if (task.IsCompletedSuccessfully) return Task.FromResult(task.Result.ShouldHaveValue(expectedValue, message));
        return AwaitAndAssert(task, r => r.ShouldHaveValue(expectedValue, message));
    }

    /// <summary>Asserts that ValueTask&lt;Result&lt;T&gt;&gt; is successful and has the expected value.</summary>
    public static ValueTask<T> ShouldHaveValueAsync<T>(this ValueTask<Result<T>> task, T expectedValue, string? message = null)
    {
        if (task.IsCompletedSuccessfully) return new ValueTask<T>(task.Result.ShouldHaveValue(expectedValue, message));
        return new ValueTask<T>(AwaitAndAssert(task.AsTask(), r => r.ShouldHaveValue(expectedValue, message)));
    }

    /// <summary>Asserts that Task&lt;Result&gt; is a failure and the error has the given metadata key and value.</summary>
    public static Task<Error> ShouldHaveMetadataAsync(this Task<Result> task, string key, object expectedValue)
    {
        if (task.IsCompletedSuccessfully) return Task.FromResult(task.Result.ShouldHaveMetadata(key, expectedValue));
        return AwaitAndAssert(task, r => r.ShouldHaveMetadata(key, expectedValue));
    }

    /// <summary>Asserts that Task&lt;Result&lt;T&gt;&gt; is a failure and the error has the given metadata key and value.</summary>
    public static Task<Error> ShouldHaveMetadataAsync<T>(this Task<Result<T>> task, string key, object expectedValue)
    {
        if (task.IsCompletedSuccessfully) return Task.FromResult(task.Result.ShouldHaveMetadata(key, expectedValue));
        return AwaitAndAssert(task, r => r.ShouldHaveMetadata(key, expectedValue));
    }

    /// <summary>Asserts that ValueTask&lt;Result&gt; is a failure and the error has the given metadata key and value.</summary>
    public static ValueTask<Error> ShouldHaveMetadataAsync(this ValueTask<Result> task, string key, object expectedValue)
    {
        if (task.IsCompletedSuccessfully) return new ValueTask<Error>(task.Result.ShouldHaveMetadata(key, expectedValue));
        return new ValueTask<Error>(AwaitAndAssert(task.AsTask(), r => r.ShouldHaveMetadata(key, expectedValue)));
    }

    /// <summary>Asserts that ValueTask&lt;Result&lt;T&gt;&gt; is a failure and the error has the given metadata key and value.</summary>
    public static ValueTask<Error> ShouldHaveMetadataAsync<T>(this ValueTask<Result<T>> task, string key, object expectedValue)
    {
        if (task.IsCompletedSuccessfully) return new ValueTask<Error>(task.Result.ShouldHaveMetadata(key, expectedValue));
        return new ValueTask<Error>(AwaitAndAssert(task.AsTask(), r => r.ShouldHaveMetadata(key, expectedValue)));
    }

    // --- Additional Async Assertions -----------------------------------------

    /// <summary>Asserts that Task&lt;Result&gt; is a failure with the given severity.</summary>
    public static Task<Error> ShouldHaveSeverityAsync(this Task<Result> task, ErrorSeverity expectedSeverity)
    {
        if (task.IsCompletedSuccessfully) return Task.FromResult(task.Result.ShouldHaveSeverity(expectedSeverity));
        return AwaitAndAssert(task, r => r.ShouldHaveSeverity(expectedSeverity));
    }

    /// <summary>Asserts that Task&lt;Result&lt;T&gt;&gt; is a failure with the given severity.</summary>
    public static Task<Error> ShouldHaveSeverityAsync<T>(this Task<Result<T>> task, ErrorSeverity expectedSeverity)
    {
        if (task.IsCompletedSuccessfully) return Task.FromResult(task.Result.ShouldHaveSeverity(expectedSeverity));
        return AwaitAndAssert(task, r => r.ShouldHaveSeverity(expectedSeverity));
    }

    /// <summary>Asserts that ValueTask&lt;Result&gt; is a failure with the given severity.</summary>
    public static ValueTask<Error> ShouldHaveSeverityAsync(this ValueTask<Result> task, ErrorSeverity expectedSeverity)
    {
        if (task.IsCompletedSuccessfully) return new ValueTask<Error>(task.Result.ShouldHaveSeverity(expectedSeverity));
        return new ValueTask<Error>(AwaitAndAssert(task.AsTask(), r => r.ShouldHaveSeverity(expectedSeverity)));
    }

    /// <summary>Asserts that ValueTask&lt;Result&lt;T&gt;&gt; is a failure with the given severity.</summary>
    public static ValueTask<Error> ShouldHaveSeverityAsync<T>(this ValueTask<Result<T>> task, ErrorSeverity expectedSeverity)
    {
        if (task.IsCompletedSuccessfully) return new ValueTask<Error>(task.Result.ShouldHaveSeverity(expectedSeverity));
        return new ValueTask<Error>(AwaitAndAssert(task.AsTask(), r => r.ShouldHaveSeverity(expectedSeverity)));
    }

    /// <summary>Asserts that Task&lt;Result&gt; error is retryable (Transient).</summary>
    public static Task<Error> ShouldBeRetryableAsync(this Task<Result> task)
    {
        if (task.IsCompletedSuccessfully) return Task.FromResult(task.Result.ShouldBeRetryable());
        return AwaitAndAssert(task, r => r.ShouldBeRetryable());
    }

    /// <summary>Asserts that Task&lt;Result&lt;T&gt;&gt; error is retryable (Transient).</summary>
    public static Task<Error> ShouldBeRetryableAsync<T>(this Task<Result<T>> task)
    {
        if (task.IsCompletedSuccessfully) return Task.FromResult(task.Result.ShouldBeRetryable());
        return AwaitAndAssert(task, r => r.ShouldBeRetryable());
    }

    /// <summary>Asserts that ValueTask&lt;Result&gt; error is retryable (Transient).</summary>
    public static ValueTask<Error> ShouldBeRetryableAsync(this ValueTask<Result> task)
    {
        if (task.IsCompletedSuccessfully) return new ValueTask<Error>(task.Result.ShouldBeRetryable());
        return new ValueTask<Error>(AwaitAndAssert(task.AsTask(), r => r.ShouldBeRetryable()));
    }

    /// <summary>Asserts that ValueTask&lt;Result&lt;T&gt;&gt; error is retryable (Transient).</summary>
    public static ValueTask<Error> ShouldBeRetryableAsync<T>(this ValueTask<Result<T>> task)
    {
        if (task.IsCompletedSuccessfully) return new ValueTask<Error>(task.Result.ShouldBeRetryable());
        return new ValueTask<Error>(AwaitAndAssert(task.AsTask(), r => r.ShouldBeRetryable()));
    }

    /// <summary>Asserts that Task&lt;Result&gt; error is permanent (not retryable).</summary>
    public static Task<Error> ShouldBePermanentAsync(this Task<Result> task)
    {
        if (task.IsCompletedSuccessfully) return Task.FromResult(task.Result.ShouldBePermanent());
        return AwaitAndAssert(task, r => r.ShouldBePermanent());
    }

    /// <summary>Asserts that Task&lt;Result&lt;T&gt;&gt; error is permanent (not retryable).</summary>
    public static Task<Error> ShouldBePermanentAsync<T>(this Task<Result<T>> task)
    {
        if (task.IsCompletedSuccessfully) return Task.FromResult(task.Result.ShouldBePermanent());
        return AwaitAndAssert(task, r => r.ShouldBePermanent());
    }

    /// <summary>Asserts that ValueTask&lt;Result&gt; error is permanent (not retryable).</summary>
    public static ValueTask<Error> ShouldBePermanentAsync(this ValueTask<Result> task)
    {
        if (task.IsCompletedSuccessfully) return new ValueTask<Error>(task.Result.ShouldBePermanent());
        return new ValueTask<Error>(AwaitAndAssert(task.AsTask(), r => r.ShouldBePermanent()));
    }

    /// <summary>Asserts that ValueTask&lt;Result&lt;T&gt;&gt; error is permanent (not retryable).</summary>
    public static ValueTask<Error> ShouldBePermanentAsync<T>(this ValueTask<Result<T>> task)
    {
        if (task.IsCompletedSuccessfully) return new ValueTask<Error>(task.Result.ShouldBePermanent());
        return new ValueTask<Error>(AwaitAndAssert(task.AsTask(), r => r.ShouldBePermanent()));
    }

    /// <summary>Asserts that Task&lt;Result&gt; has at least one inner error with the specified error code.</summary>
    public static Task<Error> ShouldContainInnerErrorAsync(this Task<Result> task, string errorCode)
    {
        if (task.IsCompletedSuccessfully) return Task.FromResult(task.Result.ShouldContainInnerError(errorCode));
        return AwaitAndAssert(task, r => r.ShouldContainInnerError(errorCode));
    }

    /// <summary>Asserts that Task&lt;Result&lt;T&gt;&gt; has at least one inner error with the specified error code.</summary>
    public static Task<Error> ShouldContainInnerErrorAsync<T>(this Task<Result<T>> task, string errorCode)
    {
        if (task.IsCompletedSuccessfully) return Task.FromResult(task.Result.ShouldContainInnerError(errorCode));
        return AwaitAndAssert(task, r => r.ShouldContainInnerError(errorCode));
    }

    /// <summary>Asserts that ValueTask&lt;Result&gt; has at least one inner error with the specified error code.</summary>
    public static ValueTask<Error> ShouldContainInnerErrorAsync(this ValueTask<Result> task, string errorCode)
    {
        if (task.IsCompletedSuccessfully) return new ValueTask<Error>(task.Result.ShouldContainInnerError(errorCode));
        return new ValueTask<Error>(AwaitAndAssert(task.AsTask(), r => r.ShouldContainInnerError(errorCode)));
    }

    /// <summary>Asserts that ValueTask&lt;Result&lt;T&gt;&gt; has at least one inner error with the specified error code.</summary>
    public static ValueTask<Error> ShouldContainInnerErrorAsync<T>(this ValueTask<Result<T>> task, string errorCode)
    {
        if (task.IsCompletedSuccessfully) return new ValueTask<Error>(task.Result.ShouldContainInnerError(errorCode));
        return new ValueTask<Error>(AwaitAndAssert(task.AsTask(), r => r.ShouldContainInnerError(errorCode)));
    }

    /// <summary>Asserts that Task&lt;Result&gt; has inner errors of expected count.</summary>
    public static Task<Error> ShouldHaveInnerErrorsAsync(this Task<Result> task, int expectedCount)
    {
        if (task.IsCompletedSuccessfully) return Task.FromResult(task.Result.ShouldHaveInnerErrors(expectedCount));
        return AwaitAndAssert(task, r => r.ShouldHaveInnerErrors(expectedCount));
    }

    /// <summary>Asserts that Task&lt;Result&lt;T&gt;&gt; has inner errors of expected count.</summary>
    public static Task<Error> ShouldHaveInnerErrorsAsync<T>(this Task<Result<T>> task, int expectedCount)
    {
        if (task.IsCompletedSuccessfully) return Task.FromResult(task.Result.ShouldHaveInnerErrors(expectedCount));
        return AwaitAndAssert(task, r => r.ShouldHaveInnerErrors(expectedCount));
    }

    /// <summary>Asserts that ValueTask&lt;Result&gt; has inner errors of expected count.</summary>
    public static ValueTask<Error> ShouldHaveInnerErrorsAsync(this ValueTask<Result> task, int expectedCount)
    {
        if (task.IsCompletedSuccessfully) return new ValueTask<Error>(task.Result.ShouldHaveInnerErrors(expectedCount));
        return new ValueTask<Error>(AwaitAndAssert(task.AsTask(), r => r.ShouldHaveInnerErrors(expectedCount)));
    }

    /// <summary>Asserts that ValueTask&lt;Result&lt;T&gt;&gt; has inner errors of expected count.</summary>
    public static ValueTask<Error> ShouldHaveInnerErrorsAsync<T>(this ValueTask<Result<T>> task, int expectedCount)
    {
        if (task.IsCompletedSuccessfully) return new ValueTask<Error>(task.Result.ShouldHaveInnerErrors(expectedCount));
        return new ValueTask<Error>(AwaitAndAssert(task.AsTask(), r => r.ShouldHaveInnerErrors(expectedCount)));
    }

    // ????????????????????????????????????????????????????????????????????????????
    //  Async ShouldHaveDescription
    // ????????????????????????????????????????????????????????????????????????????

    /// <summary>Asserts that Task&lt;Result&gt; error has the expected description.</summary>
    public static Task<Error> ShouldHaveDescriptionAsync(this Task<Result> task, string expectedDescription)
    {
        if (task.IsCompletedSuccessfully) return Task.FromResult(task.Result.ShouldHaveDescription(expectedDescription));
        return AwaitAndAssert(task, r => r.ShouldHaveDescription(expectedDescription));
    }

    /// <summary>Asserts that Task&lt;Result&lt;T&gt;&gt; error has the expected description.</summary>
    public static Task<Error> ShouldHaveDescriptionAsync<T>(this Task<Result<T>> task, string expectedDescription)
    {
        if (task.IsCompletedSuccessfully) return Task.FromResult(task.Result.ShouldHaveDescription(expectedDescription));
        return AwaitAndAssert(task, r => r.ShouldHaveDescription(expectedDescription));
    }

    /// <summary>Asserts that ValueTask&lt;Result&gt; error has the expected description.</summary>
    public static ValueTask<Error> ShouldHaveDescriptionAsync(this ValueTask<Result> task, string expectedDescription)
    {
        if (task.IsCompletedSuccessfully) return new ValueTask<Error>(task.Result.ShouldHaveDescription(expectedDescription));
        return new ValueTask<Error>(AwaitAndAssert(task.AsTask(), r => r.ShouldHaveDescription(expectedDescription)));
    }

    /// <summary>Asserts that ValueTask&lt;Result&lt;T&gt;&gt; error has the expected description.</summary>
    public static ValueTask<Error> ShouldHaveDescriptionAsync<T>(this ValueTask<Result<T>> task, string expectedDescription)
    {
        if (task.IsCompletedSuccessfully) return new ValueTask<Error>(task.Result.ShouldHaveDescription(expectedDescription));
        return new ValueTask<Error>(AwaitAndAssert(task.AsTask(), r => r.ShouldHaveDescription(expectedDescription)));
    }

    // ????????????????????????????????????????????????????????????????????????????
    //  Async ShouldHaveTraceId
    // ????????????????????????????????????????????????????????????????????????????

    /// <summary>Asserts that Task&lt;Result&gt; error has the expected trace ID.</summary>
    public static Task<Error> ShouldHaveTraceIdAsync(this Task<Result> task, string expectedTraceId)
    {
        if (task.IsCompletedSuccessfully) return Task.FromResult(task.Result.ShouldHaveTraceId(expectedTraceId));
        return AwaitAndAssert(task, r => r.ShouldHaveTraceId(expectedTraceId));
    }

    /// <summary>Asserts that Task&lt;Result&lt;T&gt;&gt; error has the expected trace ID.</summary>
    public static Task<Error> ShouldHaveTraceIdAsync<T>(this Task<Result<T>> task, string expectedTraceId)
    {
        if (task.IsCompletedSuccessfully) return Task.FromResult(task.Result.ShouldHaveTraceId(expectedTraceId));
        return AwaitAndAssert(task, r => r.ShouldHaveTraceId(expectedTraceId));
    }

    /// <summary>Asserts that ValueTask&lt;Result&gt; error has the expected trace ID.</summary>
    public static ValueTask<Error> ShouldHaveTraceIdAsync(this ValueTask<Result> task, string expectedTraceId)
    {
        if (task.IsCompletedSuccessfully) return new ValueTask<Error>(task.Result.ShouldHaveTraceId(expectedTraceId));
        return new ValueTask<Error>(AwaitAndAssert(task.AsTask(), r => r.ShouldHaveTraceId(expectedTraceId)));
    }

    /// <summary>Asserts that ValueTask&lt;Result&lt;T&gt;&gt; error has the expected trace ID.</summary>
    public static ValueTask<Error> ShouldHaveTraceIdAsync<T>(this ValueTask<Result<T>> task, string expectedTraceId)
    {
        if (task.IsCompletedSuccessfully) return new ValueTask<Error>(task.Result.ShouldHaveTraceId(expectedTraceId));
        return new ValueTask<Error>(AwaitAndAssert(task.AsTask(), r => r.ShouldHaveTraceId(expectedTraceId)));
    }

    // ????????????????????????????????????????????????????????????????????????????
    //  Async ShouldHaveCorrelationId
    // ????????????????????????????????????????????????????????????????????????????

    /// <summary>Asserts that Task&lt;Result&gt; error has the expected correlation ID.</summary>
    public static Task<Error> ShouldHaveCorrelationIdAsync(this Task<Result> task, string expectedCorrelationId)
    {
        if (task.IsCompletedSuccessfully) return Task.FromResult(task.Result.ShouldHaveCorrelationId(expectedCorrelationId));
        return AwaitAndAssert(task, r => r.ShouldHaveCorrelationId(expectedCorrelationId));
    }

    /// <summary>Asserts that Task&lt;Result&lt;T&gt;&gt; error has the expected correlation ID.</summary>
    public static Task<Error> ShouldHaveCorrelationIdAsync<T>(this Task<Result<T>> task, string expectedCorrelationId)
    {
        if (task.IsCompletedSuccessfully) return Task.FromResult(task.Result.ShouldHaveCorrelationId(expectedCorrelationId));
        return AwaitAndAssert(task, r => r.ShouldHaveCorrelationId(expectedCorrelationId));
    }

    /// <summary>Asserts that ValueTask&lt;Result&gt; error has the expected correlation ID.</summary>
    public static ValueTask<Error> ShouldHaveCorrelationIdAsync(this ValueTask<Result> task, string expectedCorrelationId)
    {
        if (task.IsCompletedSuccessfully) return new ValueTask<Error>(task.Result.ShouldHaveCorrelationId(expectedCorrelationId));
        return new ValueTask<Error>(AwaitAndAssert(task.AsTask(), r => r.ShouldHaveCorrelationId(expectedCorrelationId)));
    }

    /// <summary>Asserts that ValueTask&lt;Result&lt;T&gt;&gt; error has the expected correlation ID.</summary>
    public static ValueTask<Error> ShouldHaveCorrelationIdAsync<T>(this ValueTask<Result<T>> task, string expectedCorrelationId)
    {
        if (task.IsCompletedSuccessfully) return new ValueTask<Error>(task.Result.ShouldHaveCorrelationId(expectedCorrelationId));
        return new ValueTask<Error>(AwaitAndAssert(task.AsTask(), r => r.ShouldHaveCorrelationId(expectedCorrelationId)));
    }

    // Maps CLR type names ? C# keyword aliases for readable assertion messages.
    // e.g. "Int32" ? "int", "String" ? "string", "Boolean" ? "bool"
    private static readonly System.Collections.Generic.Dictionary<string, string> ClrToKeywordAlias =
        new(16, System.StringComparer.Ordinal)
        {
            { "Boolean", "bool" },
            { "Byte",    "byte" },
            { "SByte",   "sbyte" },
            { "Int16",   "short" },
            { "UInt16",  "ushort" },
            { "Int32",   "int" },
            { "UInt32",  "uint" },
            { "Int64",   "long" },
            { "UInt64",  "ulong" },
            { "Single",  "float" },
            { "Double",  "double" },
            { "Decimal", "decimal" },
            { "Char",    "char" },
            { "String",  "string" },
            { "Object",  "object" },
            { "Void",    "void" },
        };

    [System.Diagnostics.CodeAnalysis.UnconditionalSuppressMessage(
        "ReflectionAnalysis", "IL2070",
        Justification = "GetFriendlyTypeName is only called with typeof(T) where T is a closed generic type " +
                        "known at compile time. GetGenericArguments() is safe in this context.")]
    private static string GetFriendlyTypeName(Type type)
    {
        if (!type.IsGenericType)
        {
            // Return C# keyword alias when available (e.g. "int" instead of "Int32")
            return ClrToKeywordAlias.TryGetValue(type.Name, out var alias) ? alias : type.Name;
        }

        var genericName = type.Name;
        var backtickIdx = genericName.IndexOf('`');
        if (backtickIdx > 0)
        {
            genericName = genericName[..backtickIdx];
        }

        // Use manual concatenation instead of LINQ to avoid enumerator allocation
        var args = type.GetGenericArguments();

        // Fast path: single generic argument (e.g. Result<T>, List<T>) — avoid StringBuilder allocation
        if (args.Length == 1)
        {
            return string.Concat(genericName, "<", GetFriendlyTypeName(args[0]), ">");
        }

        var sb = new System.Text.StringBuilder();
        sb.Append(genericName);
        sb.Append('<');
        for (int i = 0; i < args.Length; i++)
        {
            if (i > 0) sb.Append(", ");
            sb.Append(GetFriendlyTypeName(args[i]));
        }
        sb.Append('>');
        return sb.ToString();
    }
}




