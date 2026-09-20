// Copyright © Erickson Lopez. MIT License.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace EricksonLopez.Result.EntityFrameworkCore;

/// <summary>
/// Provides extension methods for Entity Framework Core <see cref="DbContext"/> and <see cref="IQueryable{T}"/>
/// enabling direct, exception-safe conversion of database operations into <see cref="Result{TValue}"/>.
/// </summary>
public static class EntityFrameworkResultExtensions
{
    /// <summary>
    /// Asynchronously saves all changes made in this context to the database, mapping common persistence
    /// exceptions to strongly-typed domain <see cref="Result{TValue}"/> failures.
    /// </summary>
    /// <param name="context">The database context instance.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>
    /// A task representing the asynchronous save operation. The result contains the number of state entries
    /// written to the database on success, or a mapped domain <see cref="Error"/> on failure.
    /// </returns>
    public static async Task<Result<int>> SaveChangesAsyncToResult(
        this DbContext context,
        CancellationToken cancellationToken = default)
    {
        if (context is null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        try
        {
            int recordsWritten = await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return Result<int>.Success(recordsWritten);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            return Result<int>.Failure(
                Error.Conflict(EntityFrameworkErrorCodes.ConcurrencyConflict, ex.Message));
        }
        catch (DbUpdateException ex)
        {
            return Result<int>.Failure(
                Error.Failure(EntityFrameworkErrorCodes.UpdateFailed, ex.Message));
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (TimeoutException ex)
        {
            return Result<int>.Failure(
                Error.Unavailable(EntityFrameworkErrorCodes.Timeout, ex.Message)
                     .ToBuilder().WithRetryability(ErrorRetryability.Transient).Build());
        }
        catch (Exception ex)
        {
            return Result<int>.Failure(
                Error.Unexpected(EntityFrameworkErrorCodes.Unexpected, ex.Message));
        }
    }

    /// <summary>
    /// Asynchronously returns the first element of a sequence that satisfies a specified condition,
    /// or a domain <see cref="Result{TValue}"/> failure with the specified <paramref name="notFoundError"/> if no such element is found.
    /// </summary>
    /// <typeparam name="T">The type of the elements of <paramref name="source"/>.</typeparam>
    /// <param name="source">An <see cref="IQueryable{T}"/> to return an element from.</param>
    /// <param name="predicate">A function to test each element for a condition.</param>
    /// <param name="notFoundError">The error to return if no matching element is found.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous operation containing the matching entity or a failure.</returns>
    public static async Task<Result<T>> FirstOrDefaultToResultAsync<T>(
        this IQueryable<T> source,
        Expression<Func<T, bool>> predicate,
        Error notFoundError,
        CancellationToken cancellationToken = default)
        where T : class
    {
        if (source is null) throw new ArgumentNullException(nameof(source));
        if (predicate is null) throw new ArgumentNullException(nameof(predicate));

        try
        {
            T? entity = await source.FirstOrDefaultAsync(predicate, cancellationToken).ConfigureAwait(false);
            if (entity is null)
            {
                return Result<T>.Failure(notFoundError);
            }

            return Result<T>.Success(entity);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            return Result<T>.Failure(Error.Unexpected(EntityFrameworkErrorCodes.Unexpected, ex.Message));
        }
    }

    /// <summary>
    /// Asynchronously returns the single, specific element of a sequence that satisfies a specified condition,
    /// or a domain <see cref="Result{TValue}"/> failure if no such element or multiple elements are found.
    /// </summary>
    /// <typeparam name="T">The type of the elements of <paramref name="source"/>.</typeparam>
    /// <param name="source">An <see cref="IQueryable{T}"/> to return a single element from.</param>
    /// <param name="predicate">A function to test each element for a condition.</param>
    /// <param name="notFoundError">The error to return if no matching element is found.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A task containing the matching entity or a failure.</returns>
    public static async Task<Result<T>> SingleOrDefaultToResultAsync<T>(
        this IQueryable<T> source,
        Expression<Func<T, bool>> predicate,
        Error notFoundError,
        CancellationToken cancellationToken = default)
        where T : class
    {
        if (source is null) throw new ArgumentNullException(nameof(source));
        if (predicate is null) throw new ArgumentNullException(nameof(predicate));

        try
        {
            T? entity = await source.SingleOrDefaultAsync(predicate, cancellationToken).ConfigureAwait(false);
            if (entity is null)
            {
                return Result<T>.Failure(notFoundError);
            }

            return Result<T>.Success(entity);
        }
        catch (InvalidOperationException ex)
        {
            return Result<T>.Failure(Error.Conflict(EntityFrameworkErrorCodes.MultipleEntitiesFound, ex.Message));
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            return Result<T>.Failure(Error.Unexpected(EntityFrameworkErrorCodes.Unexpected, ex.Message));
        }
    }

    /// <summary>
    /// Asynchronously creates a <see cref="List{T}"/> from an <see cref="IQueryable{T}"/> wrapped in a <see cref="Result{TValue}"/>.
    /// </summary>
    /// <typeparam name="T">The type of the elements of <paramref name="source"/>.</typeparam>
    /// <param name="source">An <see cref="IQueryable{T}"/> to create a list from.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A task containing the materialized list or an unexpected error on failure.</returns>
    public static async Task<Result<List<T>>> ToListToResultAsync<T>(
        this IQueryable<T> source,
        CancellationToken cancellationToken = default)
    {
        if (source is null) throw new ArgumentNullException(nameof(source));

        try
        {
            List<T> list = await source.ToListAsync(cancellationToken).ConfigureAwait(false);
            return Result<List<T>>.Success(list);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            return Result<List<T>>.Failure(Error.Unexpected(EntityFrameworkErrorCodes.Unexpected, ex.Message));
        }
    }
}
