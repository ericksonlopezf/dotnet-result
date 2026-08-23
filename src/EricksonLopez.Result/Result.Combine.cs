// Copyright © Erickson Lopez. MIT License.
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

#pragma warning disable RESULT001 // Result.Combine creates large ValueTuples, which is acceptable
namespace EricksonLopez.Result;

public readonly partial struct Result
{
    // ─── Combine (Zero Allocation with ReadOnlySpan and ArrayPool) ────────────

    /// <summary>
    /// Aggregates multiple results using a read-only span. Returns success if all succeed,
    /// or a compound failure containing all errors when one or more fail.
    /// </summary>
    /// <param name="results">The span of results to aggregate.</param>
    /// <returns>A successful <see cref="Result"/> if all inputs succeed; otherwise, a compound failure containing all errors.</returns>
    [Pure]
    public static Result Combine(params ReadOnlySpan<Result> results)
    {
        int failureCount = 0;
        Error firstError = default!;
        Error[]? pooledArray = null;
        Span<Error> errors = default;

        try
        {
            foreach (ref readonly var result in results)
            {
                // Stryker disable once Statement : Equivalent mutation, Value property also throws InvalidOperationException
                if (result.IsUninitialized) ResultThrowHelper.ThrowUninitialized();
                if (result.IsFailure)
                {
                    failureCount++;
                    if (failureCount == 1)
                    {
                        firstError = result.Error;
                    }
                    else
                    {
                        if (failureCount == 2)
                        {
                            pooledArray = ArrayPool<Error>.Shared.Rent(results.Length);
                            errors = pooledArray.AsSpan();
                            errors[0] = firstError;
                        }
                        errors[failureCount - 1] = result.Error;
                    }
                }
            }

            if (failureCount == 0) return Success();
            if (failureCount == 1) return Failure(firstError);

            var finalErrors = new Error[failureCount];
            errors.Slice(0, failureCount).CopyTo(finalErrors);

            return Failure(Error.Failure(
                WellKnownErrors.CombinedFailuresCode,
                $"{failureCount} errors occurred",
                finalErrors));
        }
        // Stryker disable all : ArrayPool cleanup (cannot assert ArrayPool internal state)
        finally
        {
            if (pooledArray is not null)
            {
                // clearArray: true — Error is a reference type; clearing slots allows the GC to reclaim
                // Error objects (and their InnerErrors/Metadata graphs) promptly after returning to the pool.
                // Leaving references in pooled arrays can cause unintentional root retention.
                ArrayPool<Error>.Shared.Return(pooledArray, clearArray: true);
            }
        }
        // Stryker restore all
    }

    /// <summary>
    /// Aggregates homogeneous typed results using a read-only span.
    /// Returns all values on success, or a compound failure on any error.
    /// </summary>
    /// <typeparam name="T">The type of the successful values.</typeparam>
    /// <param name="results">The span of typed results to aggregate.</param>
    /// <returns>A successful <see cref="Result{TValue}"/> containing a list of all values if all inputs succeed; otherwise, a compound failure containing all errors.</returns>
    [Pure]
    public static Result<IReadOnlyList<T>> Combine<T>(params ReadOnlySpan<Result<T>> results)
    {
        int failureCount = 0;
        int successCount = 0;
        Error firstError = default!;
        Error[]? pooledErrors = null;
        Span<Error> errors = default;
        // Rent from pool to accumulate success values. If there are failures, the rented array is
        // returned to the pool without allocating a permanent T[] — this avoids GC waste on failure paths.
        T[]? rentedValues = null;

        try
        {
            for (int i = 0; i < results.Length; i++)
            {
                ref readonly var result = ref results[i];
                // Stryker disable once Statement : Equivalent mutation, Value property also throws InvalidOperationException
                if (result.IsUninitialized) ResultThrowHelper.ThrowUninitialized();
                if (result.IsFailure)
                {
                    failureCount++;
                    if (failureCount == 1)
                    {
                        firstError = result.Error;
                    }
                    else
                    {
                        if (failureCount == 2)
                        {
                            pooledErrors = ArrayPool<Error>.Shared.Rent(results.Length);
                            errors = pooledErrors.AsSpan();
                            errors[0] = firstError;
                        }
                        errors[failureCount - 1] = result.Error;
                    }
                }
                else if (failureCount == 0)
                {
                    if (rentedValues is null)
                    {
                        rentedValues = ArrayPool<T>.Shared.Rent(results.Length);
                    }
                    rentedValues[successCount++] = result.Value;
                }
            }

            if (failureCount == 0)
            {
                IReadOnlyList<T> list;
                if (successCount == 0)
                {
                    list = Array.Empty<T>();
                }
                else
                {
                    // Copy to an exact-sized array for the return value — we cannot hand the pooled array
                    // directly to callers since it will be returned to the pool in the finally block.
                    var exactValues = new T[successCount];
                    rentedValues!.AsSpan(0, successCount).CopyTo(exactValues);
                    list = exactValues;
                }
                return Success<IReadOnlyList<T>>(list);
            }
            if (failureCount == 1) return Failure<IReadOnlyList<T>>(firstError);

            var finalErrors = new Error[failureCount];
            errors.Slice(0, failureCount).CopyTo(finalErrors);

            return Failure<IReadOnlyList<T>>(Error.Failure(
                WellKnownErrors.CombinedFailuresCode,
                $"{failureCount} errors occurred",
                finalErrors));
        }
        finally
        {
            if (pooledErrors is not null)
            // Stryker disable once all : Equivalent mutation
            {
                // Stryker disable once all
                ArrayPool<Error>.Shared.Return(pooledErrors, clearArray: true);
            }
            if (rentedValues is not null)
            // Stryker disable once all : Equivalent mutation
            {
                // clearArray: true — T may be a reference type; clear slots to allow GC
                // to reclaim objects promptly after returning to the pool.
                // Stryker disable once all
                ArrayPool<T>.Shared.Return(rentedValues, clearArray: true);
            }
        }
    }

    /// <summary>
    /// Aggregates multiple results. Returns success if all succeed,
    /// or a compound failure containing all errors when one or more fail.
    /// </summary>
    /// <param name="results">The array of results to aggregate.</param>
    /// <returns>A successful <see cref="Result"/> if all inputs succeed; otherwise, a compound failure containing all errors.</returns>
    /// <remarks>
    /// This overload accepts <c>params Result[]</c> for compatibility with C# 12 and earlier,
    /// which do not support <c>params ReadOnlySpan&lt;T&gt;</c>. It delegates to the
    /// <see cref="Combine(ReadOnlySpan{Result})"/> overload.
    /// </remarks>
    [Pure]
    public static Result Combine(params Result[] results)
        => Combine(results.AsSpan());

    /// <summary>
    /// Aggregates homogeneous typed results.
    /// Returns all values on success, or a compound failure on any error.
    /// </summary>
    /// <typeparam name="T">The type of the successful values.</typeparam>
    /// <param name="results">The array of typed results to aggregate.</param>
    /// <returns>A successful <see cref="Result{TValue}"/> containing a list of all values if all inputs succeed; otherwise, a compound failure containing all errors.</returns>
    /// <remarks>
    /// This overload accepts <c>params Result&lt;T&gt;[]</c> for compatibility with C# 12 and earlier,
    /// which do not support <c>params ReadOnlySpan&lt;T&gt;</c>. It delegates to the
    /// <see cref="Combine{T}(ReadOnlySpan{Result{T}})"/> overload.
    /// </remarks>
    [Pure]
    public static Result<IReadOnlyList<T>> Combine<T>(params Result<T>[] results)
        => Combine(results.AsSpan());

    /// <summary>
    /// Aggregates two typed results into a value tuple.
    /// </summary>
    /// <typeparam name="T1">The type of the first value.</typeparam>
    /// <typeparam name="T2">The type of the second value.</typeparam>
    /// <param name="r1">The first result, passed by readonly reference.</param>
    /// <param name="r2">The second result, passed by readonly reference.</param>
    /// <returns>A successful <see cref="Result{TValue}"/> containing a tuple of values if all inputs succeed; otherwise, a compound failure containing all errors.</returns>
    [Pure]
    public static Result<(T1, T2)> Combine<T1, T2>(in Result<T1> r1, in Result<T2> r2)
    {
        // Stryker disable once all : Equivalent mutation, properties also throw InvalidOperationException
        if (r1.IsUninitialized || r2.IsUninitialized) ResultThrowHelper.ThrowUninitialized();
        if (r1.IsSuccess && r2.IsSuccess)
            return Success((r1.Value, r2.Value));

        if (r1.IsFailure && r2.IsSuccess) return Failure<(T1, T2)>(r1.Error);
        if (r1.IsSuccess && r2.IsFailure) return Failure<(T1, T2)>(r2.Error);

        var errors = System.Buffers.ArrayPool<Error>.Shared.Rent(2);
        errors[0] = r1.Error!;
        errors[1] = r2.Error!;

        var error = new Error(
            WellKnownErrors.CombinedFailuresCode,
            $"2 errors occurred",
            innerErrors: System.Collections.Immutable.ImmutableArray.Create(errors, 0, 2));

        // Stryker disable once all : Equivalent mutation
        System.Buffers.ArrayPool<Error>.Shared.Return(errors, clearArray: true);
        return Failure<(T1, T2)>(error);
    }

    /// <summary>
    /// Aggregates three typed results into a value tuple.
    /// </summary>
    /// <typeparam name="T1">The type of the first value.</typeparam>
    /// <typeparam name="T2">The type of the second value.</typeparam>
    /// <typeparam name="T3">The type of the third value.</typeparam>
    /// <param name="r1">The first result, passed by readonly reference.</param>
    /// <param name="r2">The second result, passed by readonly reference.</param>
    /// <param name="r3">The third result, passed by readonly reference.</param>
    /// <returns>A successful <see cref="Result{TValue}"/> containing a tuple of values if all inputs succeed; otherwise, a compound failure containing all errors.</returns>
    [Pure]
    public static Result<(T1, T2, T3)> Combine<T1, T2, T3>(
        in Result<T1> r1, in Result<T2> r2, in Result<T3> r3)
    {
        // Stryker disable once all : Equivalent mutation, properties also throw InvalidOperationException
        if (r1.IsUninitialized || r2.IsUninitialized || r3.IsUninitialized) ResultThrowHelper.ThrowUninitialized();
        if (r1.IsSuccess && r2.IsSuccess && r3.IsSuccess)
            return Success((r1.Value, r2.Value, r3.Value));

        // Count failures first to allocate a single exact-size array, avoiding the
        // double-allocation (temp Error[N] + final Error[failureCount] + Array.Copy) pattern.
        int failureCount = (r1.IsFailure ? 1 : 0) + (r2.IsFailure ? 1 : 0) + (r3.IsFailure ? 1 : 0);
        if (failureCount == 1)
        {
            var single = r1.IsFailure ? r1.Error : r2.IsFailure ? r2.Error : r3.Error;
            return Failure<(T1, T2, T3)>(single);
        }

        var errors = System.Buffers.ArrayPool<Error>.Shared.Rent(failureCount);
        int idx = 0;
        if (r1.IsFailure) errors[idx++] = r1.Error!;
        if (r2.IsFailure) errors[idx++] = r2.Error!;
        // Stryker disable once all : Equivalent mutation
        if (r3.IsFailure) errors[idx++] = r3.Error!;

        var error = new Error(
            WellKnownErrors.CombinedFailuresCode,
            $"{failureCount} errors occurred",
            innerErrors: System.Collections.Immutable.ImmutableArray.Create(errors, 0, failureCount));

        // Stryker disable once all : Equivalent mutation
        System.Buffers.ArrayPool<Error>.Shared.Return(errors, clearArray: true);
        return Failure<(T1, T2, T3)>(error);
    }

    /// <summary>
    /// Aggregates four typed results into a value tuple.
    /// </summary>
    /// <typeparam name="T1">The type of the first value.</typeparam>
    /// <typeparam name="T2">The type of the second value.</typeparam>
    /// <typeparam name="T3">The type of the third value.</typeparam>
    /// <typeparam name="T4">The type of the fourth value.</typeparam>
    /// <param name="r1">The first result, passed by readonly reference.</param>
    /// <param name="r2">The second result, passed by readonly reference.</param>
    /// <param name="r3">The third result, passed by readonly reference.</param>
    /// <param name="r4">The fourth result, passed by readonly reference.</param>
    /// <returns>A successful <see cref="Result{TValue}"/> containing a tuple of values if all inputs succeed; otherwise, a compound failure containing all errors.</returns>
    [Pure]
    public static Result<(T1, T2, T3, T4)> Combine<T1, T2, T3, T4>(
        in Result<T1> r1, in Result<T2> r2, in Result<T3> r3, in Result<T4> r4)
    {
        // Stryker disable once all : Equivalent mutation, properties also throw InvalidOperationException
        if (r1.IsUninitialized || r2.IsUninitialized || r3.IsUninitialized || r4.IsUninitialized) ResultThrowHelper.ThrowUninitialized();
        if (r1.IsSuccess && r2.IsSuccess && r3.IsSuccess && r4.IsSuccess)
            return Success((r1.Value, r2.Value, r3.Value, r4.Value));

        // Count failures first to allocate a single exact-size array.
        int failureCount = (r1.IsFailure ? 1 : 0) + (r2.IsFailure ? 1 : 0) + (r3.IsFailure ? 1 : 0) + (r4.IsFailure ? 1 : 0);
        if (failureCount == 1)
        {
            var single = r1.IsFailure ? r1.Error : r2.IsFailure ? r2.Error : r3.IsFailure ? r3.Error : r4.Error;
            return Failure<(T1, T2, T3, T4)>(single);
        }

        var errors = System.Buffers.ArrayPool<Error>.Shared.Rent(failureCount);
        int idx = 0;
        if (r1.IsFailure) errors[idx++] = r1.Error!;
        if (r2.IsFailure) errors[idx++] = r2.Error!;
        if (r3.IsFailure) errors[idx++] = r3.Error!;
        // Stryker disable once all : Equivalent mutation
        if (r4.IsFailure) errors[idx++] = r4.Error!;

        var error = new Error(
            WellKnownErrors.CombinedFailuresCode,
            $"{failureCount} errors occurred",
            innerErrors: System.Collections.Immutable.ImmutableArray.Create(errors, 0, failureCount));

        // Stryker disable once all : Equivalent mutation
        System.Buffers.ArrayPool<Error>.Shared.Return(errors, clearArray: true);
        return Failure<(T1, T2, T3, T4)>(error);
    }

    /// <summary>
    /// Aggregates five typed results into a value tuple.
    /// </summary>
    /// <typeparam name="T1">The type of the first value.</typeparam>
    /// <typeparam name="T2">The type of the second value.</typeparam>
    /// <typeparam name="T3">The type of the third value.</typeparam>
    /// <typeparam name="T4">The type of the fourth value.</typeparam>
    /// <typeparam name="T5">The type of the fifth value.</typeparam>
    /// <param name="r1">The first result, passed by readonly reference.</param>
    /// <param name="r2">The second result, passed by readonly reference.</param>
    /// <param name="r3">The third result, passed by readonly reference.</param>
    /// <param name="r4">The fourth result, passed by readonly reference.</param>
    /// <param name="r5">The fifth result, passed by readonly reference.</param>
    /// <returns>A successful <see cref="Result{TValue}"/> containing a tuple of values if all inputs succeed; otherwise, a compound failure containing all errors.</returns>
    [Pure]
    public static Result<(T1, T2, T3, T4, T5)> Combine<T1, T2, T3, T4, T5>(
        in Result<T1> r1, in Result<T2> r2, in Result<T3> r3, in Result<T4> r4, in Result<T5> r5)
    {
        // Stryker disable once all : Equivalent mutation, properties also throw InvalidOperationException
        if (r1.IsUninitialized || r2.IsUninitialized || r3.IsUninitialized || r4.IsUninitialized || r5.IsUninitialized) ResultThrowHelper.ThrowUninitialized();
        if (r1.IsSuccess && r2.IsSuccess && r3.IsSuccess && r4.IsSuccess && r5.IsSuccess)
            return Success((r1.Value, r2.Value, r3.Value, r4.Value, r5.Value));

        // Count failures first to allocate a single exact-size array.
        int failureCount = (r1.IsFailure ? 1 : 0) + (r2.IsFailure ? 1 : 0) + (r3.IsFailure ? 1 : 0)
                         + (r4.IsFailure ? 1 : 0) + (r5.IsFailure ? 1 : 0);
        if (failureCount == 1)
        {
            var single = r1.IsFailure ? r1.Error : r2.IsFailure ? r2.Error : r3.IsFailure ? r3.Error
                       : r4.IsFailure ? r4.Error : r5.Error;
            return Failure<(T1, T2, T3, T4, T5)>(single);
        }

        var errors = System.Buffers.ArrayPool<Error>.Shared.Rent(failureCount);
        int idx = 0;
        if (r1.IsFailure) errors[idx++] = r1.Error!;
        if (r2.IsFailure) errors[idx++] = r2.Error!;
        if (r3.IsFailure) errors[idx++] = r3.Error!;
        if (r4.IsFailure) errors[idx++] = r4.Error!;
        // Stryker disable once all : Equivalent mutation
        if (r5.IsFailure) errors[idx++] = r5.Error!;

        var error = new Error(
            WellKnownErrors.CombinedFailuresCode,
            $"{failureCount} errors occurred",
            innerErrors: System.Collections.Immutable.ImmutableArray.Create(errors, 0, failureCount));

        // Stryker disable once all : Equivalent mutation
        System.Buffers.ArrayPool<Error>.Shared.Return(errors, clearArray: true);
        return Failure<(T1, T2, T3, T4, T5)>(error);
    }

    /// <summary>
    /// Merges a guard non-generic result with a typed result, returning the typed result if the guard succeeds.
    /// </summary>
    /// <typeparam name="T">The value type of the typed result.</typeparam>
    /// <param name="guard">The guard result to check first, passed by readonly reference.</param>
    /// <param name="next">The typed result to return if the guard succeeds, passed by readonly reference.</param>
    /// <returns>The failure from <paramref name="guard"/> if it failed; otherwise, <paramref name="next"/>.</returns>
    [Pure]
    public static Result<T> Merge<T>(in Result guard, in Result<T> next)
    {
        if (guard.IsUninitialized || next.IsUninitialized) ResultThrowHelper.ThrowUninitialized();
        return guard.IsFailure ? Failure<T>(guard.Error) : next;
    }
}



