// Copyright © Erickson Lopez. MIT License.
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Threading;
using System.Threading.Tasks;

namespace EricksonLopez.Result;

public readonly partial struct Result
{
    // ─── ValidateAll (Cumulative Validation with ReadOnlySpan and ArrayPool) ───

    /// <summary>
    /// Executes all provided validation functions and accumulates any errors.
    /// Returns <see cref="Success()"/> if all validators succeed, or a compound failure
    /// containing all encountered errors if one or more validators fail.
    /// </summary>
    /// <param name="validators">The validation functions to execute.</param>
    /// <returns>A successful Result if all validations pass; otherwise, a Failure containing all accumulated validation errors.</returns>
    [Pure]
    public static Result ValidateAll(params ReadOnlySpan<Func<Result>> validators)
    {
        int failureCount = 0;
        Error firstError = default!;
        Error[]? pooledArray = null;
        Span<Error> errors = default;

        try
        {
            for (int i = 0; i < validators.Length; i++)
            {
                var result = validators[i]();
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
                            pooledArray = ArrayPool<Error>.Shared.Rent(validators.Length);
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

            return Failure(Error.Validation(
                WellKnownErrors.CombinedFailuresCode,
                $"{failureCount} validation errors occurred",
                finalErrors));
        }
        // Stryker disable all : ArrayPool cleanup (cannot assert ArrayPool internal state)
        finally
        {
            if (pooledArray is not null)
            {
                ArrayPool<Error>.Shared.Return(pooledArray, clearArray: true);
            }
        }
        // Stryker restore all
    }

    /// <summary>
    /// Executes all validation functions against <paramref name="value"/> and accumulates any errors.
    /// Returns <see cref="Success{T}(T)"/> with <paramref name="value"/> if all validators pass,
    /// or a compound failure containing all encountered errors if one or more fail.
    /// </summary>
    /// <typeparam name="T">The type of the validated value.</typeparam>
    /// <param name="value">The target value to validate.</param>
    /// <param name="validators">The validation functions to execute.</param>
    /// <returns>A successful <see cref="Result{T}"/> containing <paramref name="value"/> if all pass; otherwise, a failure.</returns>
    [Pure]
    public static Result<T> ValidateAll<T>(T value, params ReadOnlySpan<Func<T, Result>> validators)
    {
        int failureCount = 0;
        Error firstError = default!;
        Error[]? pooledArray = null;
        Span<Error> errors = default;

        try
        {
            for (int i = 0; i < validators.Length; i++)
            {
                var result = validators[i](value);
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
                            pooledArray = ArrayPool<Error>.Shared.Rent(validators.Length);
                            errors = pooledArray.AsSpan();
                            errors[0] = firstError;
                        }
                        errors[failureCount - 1] = result.Error;
                    }
                }
            }

            if (failureCount == 0) return Success(value);
            if (failureCount == 1) return Failure<T>(firstError);

            var finalErrors = new Error[failureCount];
            errors.Slice(0, failureCount).CopyTo(finalErrors);

            return Failure<T>(Error.Validation(
                WellKnownErrors.CombinedFailuresCode,
                $"{failureCount} validation errors occurred",
                finalErrors));
        }
        // Stryker disable all : ArrayPool cleanup (cannot assert ArrayPool internal state)
        finally
        {
            if (pooledArray is not null)
            {
                ArrayPool<Error>.Shared.Return(pooledArray, clearArray: true);
            }
        }
        // Stryker restore all
    }

    /// <summary>
    /// Asynchronously executes all validation functions and accumulates any errors.
    /// </summary>
    /// <param name="validators">The collection of asynchronous validation functions to execute.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the validation operations.</param>
    /// <returns>A task representing the asynchronous operation. The task result contains a successful <see cref="Result"/> if all validations pass, or a compound failure if one or more fail.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="validators"/> is <see langword="null"/></exception>
    /// <exception cref="OperationCanceledException">The operation was canceled</exception>
    public static async Task<Result> ValidateAllAsync(
        IReadOnlyList<Func<CancellationToken, Task<Result>>> validators,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(validators);

        var failedErrors = new List<Error>();

        for (int i = 0; i < validators.Count; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var result = await validators[i](cancellationToken).ConfigureAwait(false);
            if (result.IsUninitialized) ResultThrowHelper.ThrowUninitialized();
            if (result.IsFailure)
            {
                failedErrors.Add(result.Error);
            }
        }

        if (failedErrors.Count == 0) return Success();
        if (failedErrors.Count == 1) return Failure(failedErrors[0]);

        return Failure(Error.Validation(
            WellKnownErrors.CombinedFailuresCode,
            $"{failedErrors.Count} validation errors occurred",
            failedErrors.ToArray()));
    }

    /// <summary>
    /// Asynchronously executes all validation functions against the specified value and accumulates any errors.
    /// </summary>
    /// <typeparam name="T">The type of the validated value.</typeparam>
    /// <param name="value">The target value to validate.</param>
    /// <param name="validators">The collection of asynchronous validation functions to execute.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the validation operations.</param>
    /// <returns>A task representing the asynchronous operation. The task result contains a successful <see cref="Result{T}"/> containing <paramref name="value"/> if all pass, or a compound failure if one or more fail.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="validators"/> is <see langword="null"/></exception>
    /// <exception cref="OperationCanceledException">The operation was canceled</exception>
    public static async Task<Result<T>> ValidateAllAsync<T>(
        T value,
        IReadOnlyList<Func<T, CancellationToken, Task<Result>>> validators,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(validators);

        var failedErrors = new List<Error>();

        for (int i = 0; i < validators.Count; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var result = await validators[i](value, cancellationToken).ConfigureAwait(false);
            if (result.IsUninitialized) ResultThrowHelper.ThrowUninitialized();
            if (result.IsFailure)
            {
                failedErrors.Add(result.Error);
            }
        }

        if (failedErrors.Count == 0) return Success(value);
        if (failedErrors.Count == 1) return Failure<T>(failedErrors[0]);

        return Failure<T>(Error.Validation(
            WellKnownErrors.CombinedFailuresCode,
            $"{failedErrors.Count} validation errors occurred",
            failedErrors.ToArray()));
    }

    /// <summary>
    /// Asynchronously executes all validation functions returning <see cref="ValueTask{Result}"/> and accumulates any errors.
    /// </summary>
    /// <param name="validators">The collection of asynchronous validation functions to execute.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the validation operations.</param>
    /// <returns>A value task representing the asynchronous operation. The task result contains a successful <see cref="Result"/> if all validations pass, or a compound failure if one or more fail.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="validators"/> is <see langword="null"/></exception>
    /// <exception cref="OperationCanceledException">The operation was canceled</exception>
    public static async ValueTask<Result> ValidateAllAsync(
        IReadOnlyList<Func<CancellationToken, ValueTask<Result>>> validators,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(validators);

        var failedErrors = new List<Error>();

        for (int i = 0; i < validators.Count; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var result = await validators[i](cancellationToken).ConfigureAwait(false);
            if (result.IsUninitialized) ResultThrowHelper.ThrowUninitialized();
            if (result.IsFailure)
            {
                failedErrors.Add(result.Error);
            }
        }

        if (failedErrors.Count == 0) return Success();
        if (failedErrors.Count == 1) return Failure(failedErrors[0]);

        return Failure(Error.Validation(
            WellKnownErrors.CombinedFailuresCode,
            $"{failedErrors.Count} validation errors occurred",
            failedErrors.ToArray()));
    }

    /// <summary>
    /// Asynchronously executes all validation functions against the specified value returning <see cref="ValueTask{Result}"/> and accumulates any errors.
    /// </summary>
    /// <typeparam name="T">The type of the validated value.</typeparam>
    /// <param name="value">The target value to validate.</param>
    /// <param name="validators">The collection of asynchronous validation functions to execute.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the validation operations.</param>
    /// <returns>A value task representing the asynchronous operation. The task result contains a successful <see cref="Result{T}"/> containing <paramref name="value"/> if all pass, or a compound failure if one or more fail.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="validators"/> is <see langword="null"/></exception>
    /// <exception cref="OperationCanceledException">The operation was canceled</exception>
    public static async ValueTask<Result<T>> ValidateAllAsync<T>(
        T value,
        IReadOnlyList<Func<T, CancellationToken, ValueTask<Result>>> validators,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(validators);

        var failedErrors = new List<Error>();

        for (int i = 0; i < validators.Count; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var result = await validators[i](value, cancellationToken).ConfigureAwait(false);
            if (result.IsUninitialized) ResultThrowHelper.ThrowUninitialized();
            if (result.IsFailure)
            {
                failedErrors.Add(result.Error);
            }
        }

        if (failedErrors.Count == 0) return Success(value);
        if (failedErrors.Count == 1) return Failure<T>(failedErrors[0]);

        return Failure<T>(Error.Validation(
            WellKnownErrors.CombinedFailuresCode,
            $"{failedErrors.Count} validation errors occurred",
            failedErrors.ToArray()));
    }
}

