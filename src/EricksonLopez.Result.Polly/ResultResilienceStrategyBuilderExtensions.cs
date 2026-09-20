// Copyright © Erickson Lopez. MIT License.
using System;
using Polly;
using Polly.Retry;

namespace EricksonLopez.Result.Polly;

/// <summary>
/// Provides extension methods for <see cref="ResiliencePipelineBuilder"/> and <see cref="PredicateBuilder"/>
/// to configure retry and resilience strategies tailored for <see cref="Result"/> and <see cref="Result{TValue}"/>.
/// </summary>
public static class ResultResilienceStrategyBuilderExtensions
{
    /// <summary>
    /// Configures a predicate that matches any failed <see cref="Result{TValue}"/> whose error has <see cref="ErrorRetryability.Transient"/>.
    /// </summary>
    /// <typeparam name="T">The result value type.</typeparam>
    /// <param name="predicateBuilder">The predicate builder instance.</param>
    /// <returns>The predicate builder configured for retryable errors.</returns>
    public static PredicateBuilder<Result<T>> HandleRetryableError<T>(this PredicateBuilder<Result<T>> predicateBuilder)
    {
        if (predicateBuilder is null) throw new ArgumentNullException(nameof(predicateBuilder));

        return predicateBuilder.HandleResult(static r => r.IsFailure && r.Error.Retryability == ErrorRetryability.Transient);
    }

    /// <summary>
    /// Configures a predicate that matches any failed <see cref="Result"/> whose error has <see cref="ErrorRetryability.Transient"/>.
    /// </summary>
    /// <param name="predicateBuilder">The predicate builder instance.</param>
    /// <returns>The predicate builder configured for retryable errors.</returns>
    public static PredicateBuilder<Result> HandleRetryableError(this PredicateBuilder<Result> predicateBuilder)
    {
        if (predicateBuilder is null) throw new ArgumentNullException(nameof(predicateBuilder));

        return predicateBuilder.HandleResult(static r => r.IsFailure && r.Error.Retryability == ErrorRetryability.Transient);
    }

    /// <summary>
    /// Adds a retry strategy to the pipeline configured to retry when an operation returns a failed <see cref="Result{TValue}"/> with <see cref="ErrorRetryability.Transient"/>.
    /// </summary>
    /// <typeparam name="T">The result value type.</typeparam>
    /// <param name="builder">The resilience pipeline builder.</param>
    /// <param name="maxRetryAttempts">The maximum number of retry attempts. Defaults to 3.</param>
    /// <param name="delay">The delay between retries. If <see langword="null"/>, a default constant delay is applied.</param>
    /// <returns>The resilience pipeline builder with the retry strategy configured.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="builder"/> is <see langword="null"/></exception>
    public static ResiliencePipelineBuilder<Result<T>> AddResultRetry<T>(
        this ResiliencePipelineBuilder<Result<T>> builder,
        int maxRetryAttempts = 3,
        TimeSpan? delay = null)
    {
        if (builder is null) throw new ArgumentNullException(nameof(builder));

        var options = new RetryStrategyOptions<Result<T>>
        {
            MaxRetryAttempts = maxRetryAttempts,
            Delay = delay ?? TimeSpan.FromMilliseconds(200),
            ShouldHandle = new PredicateBuilder<Result<T>>().HandleRetryableError()
        };

        return builder.AddRetry(options);
    }

    /// <summary>
    /// Adds a retry strategy to the pipeline configured to retry when an operation returns a failed <see cref="Result"/> with <see cref="ErrorRetryability.Transient"/>.
    /// </summary>
    /// <param name="builder">The resilience pipeline builder.</param>
    /// <param name="maxRetryAttempts">The maximum number of retry attempts. Defaults to 3.</param>
    /// <param name="delay">The delay between retries. If <see langword="null"/>, a default constant delay is applied.</param>
    /// <returns>The resilience pipeline builder with the retry strategy configured.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="builder"/> is <see langword="null"/></exception>
    public static ResiliencePipelineBuilder<Result> AddResultRetry(
        this ResiliencePipelineBuilder<Result> builder,
        int maxRetryAttempts = 3,
        TimeSpan? delay = null)
    {
        if (builder is null) throw new ArgumentNullException(nameof(builder));

        var options = new RetryStrategyOptions<Result>
        {
            MaxRetryAttempts = maxRetryAttempts,
            Delay = delay ?? TimeSpan.FromMilliseconds(200),
            ShouldHandle = new PredicateBuilder<Result>().HandleRetryableError()
        };

        return builder.AddRetry(options);
    }
}
