// Copyright © Erickson Lopez. MIT License.
using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace EricksonLopez.Result;

/// <summary>
/// Provides LINQ extension support for <see cref="Result{TValue}"/> pipelines.
/// </summary>
public static class ResultLinqExtensions
{
    /// <summary>
    /// Cached error instance returned when a LINQ <c>where</c> predicate filters out a value.
    /// Avoids allocating a new <see cref="Error"/> on every filtered-out result.
    /// </summary>
    private static readonly Error FilteredOutError =
        Error.Validation("Result.FilteredOut", "The result value did not satisfy the filter predicate.");

    /// <summary>
    /// Projects the success value of a Result into a new form using LINQ query syntax.
    /// </summary>
    /// <typeparam name="TSource">The source value type.</typeparam>
    /// <typeparam name="TResult">The target value type produced by the selector.</typeparam>
    /// <param name="source">The source result to project, passed by readonly reference.</param>
    /// <param name="selector">A transform function to apply to the source value.</param>
    /// <returns>A new <see cref="Result{TResult}"/> containing the transformed value on success, or the original error on failure.</returns>
    /// <exception cref="InvalidOperationException"><paramref name="source"/> is an uninitialized default value</exception>
    [Pure]
    public static Result<TResult> Select<TSource, TResult>(
        this in Result<TSource> source,
        Func<TSource, TResult> selector)
    {
        if (source.IsUninitialized)
        {
            // Stryker disable all : Exception message
            throw new InvalidOperationException(
                "Cannot use LINQ Select on an uninitialized default Result<TSource>. " +
                "Always construct Result<TSource> via Result.Success(value) or Result.Failure(error).");
            // Stryker restore all
        }

        return source.IsSuccess
            ? Result.Success(selector(source.Value))
            : Result.Failure<TResult>(source.Error);
    }

    /// <summary>
    /// Projects and flattens monadic Result pipelines using LINQ query syntax (from x in a from y in b select z).
    /// </summary>
    /// <typeparam name="TSource">The source value type.</typeparam>
    /// <typeparam name="TCollection">The intermediate value type produced by the collection selector.</typeparam>
    /// <typeparam name="TResult">The target value type produced by the result selector.</typeparam>
    /// <param name="source">The source result to project, passed by readonly reference.</param>
    /// <param name="collectionSelector">A transform function to apply to the source value, returning an intermediate result.</param>
    /// <param name="resultSelector">A transform function to apply to the source value and intermediate value.</param>
    /// <returns>A new <see cref="Result{TResult}"/> containing the combined value on success, or the first encountered failure error.</returns>
    /// <exception cref="InvalidOperationException"><paramref name="source"/> is an uninitialized default value</exception>
    [Pure]
    public static Result<TResult> SelectMany<TSource, TCollection, TResult>(
        this in Result<TSource> source,
        Func<TSource, Result<TCollection>> collectionSelector,
        Func<TSource, TCollection, TResult> resultSelector)
    {
        if (source.IsUninitialized)
        // Stryker disable once all : Equivalent mutation
        {
            // Stryker disable all : Exception message
            throw new InvalidOperationException(
                "Cannot use LINQ SelectMany on an uninitialized default Result<TSource>. " +
                "Always construct Result<TSource> via Result.Success(value) or Result.Failure(error).");
            // Stryker restore all
        }

        if (source.IsFailure) return Result.Failure<TResult>(source.Error);
        var collectionResult = collectionSelector(source.Value);
        if (collectionResult.IsFailure) return Result.Failure<TResult>(collectionResult.Error);
        return Result.Success(resultSelector(source.Value, collectionResult.Value));
    }

    /// <summary>
    /// Filters the success value of a Result using a predicate, enabling LINQ <c>where</c> clause syntax.
    /// Returns a failure if the predicate returns <see langword="false"/> or if the source is already a failure.
    /// </summary>
    /// <typeparam name="TSource">The value type of the result.</typeparam>
    /// <param name="source">The result to filter, passed by readonly reference.</param>
    /// <param name="predicate">A function that tests the success value. Returns a failure with a generic
    /// validation error if the predicate returns <see langword="false"/>.</param>
    /// <returns>
    /// The original result if it is a success and the predicate returns <see langword="true"/>;
    /// a failure result with a <see cref="ErrorType.Validation"/> error otherwise.
    /// </returns>
    /// <remarks>
    /// This method enables the LINQ <c>where</c> clause on <see cref="Result{TValue}"/> pipelines:
    /// <code>
    /// var result =
    ///     from order in GetOrder(id)
    ///     where order.IsActive
    ///     select order.Total;
    /// </code>
    /// For richer control over the failure error (custom code, description, type), use
    /// <see cref="Result{TValue}.Ensure(Func{TValue, bool}, Error)"/> directly.
    /// </remarks>
    /// <exception cref="InvalidOperationException"><paramref name="source"/> is an uninitialized default value</exception>
    [Pure]
    public static Result<TSource> Where<TSource>(
        this in Result<TSource> source,
        Func<TSource, bool> predicate)
    {
        if (source.IsUninitialized)
        // Stryker disable once all : Equivalent mutation
        {
            // Stryker disable all : Exception message
            throw new InvalidOperationException(
                "Cannot use LINQ Where on an uninitialized default Result<TSource>. " +
                "Always construct Result<TSource> via Result.Success(value) or Result.Failure(error).");
            // Stryker restore all
        }

        return source.Ensure(predicate, FilteredOutError);
    }
}
