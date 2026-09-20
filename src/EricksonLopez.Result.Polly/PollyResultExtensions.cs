// Copyright © Erickson Lopez. MIT License.
using System;
using System.Threading;
using System.Threading.Tasks;
using Polly;

namespace EricksonLopez.Result.Polly;

/// <summary>
/// Provides execution and composition extension methods on <see cref="ResiliencePipeline"/> and <see cref="ResiliencePipeline{T}"/>
/// for executing monadic pipelines returning <see cref="Result"/> and <see cref="Result{TValue}"/>.
/// </summary>
public static class PollyResultExtensions
{
    /// <summary>
    /// Executes the specified callback through the resilience pipeline.
    /// </summary>
    /// <typeparam name="T">The result value type.</typeparam>
    /// <param name="pipeline">The resilience pipeline.</param>
    /// <param name="callback">The callback to execute.</param>
    /// <returns>The executed <see cref="Result{TValue}"/>.</returns>
    public static Result<T> ExecuteResult<T>(
        this ResiliencePipeline pipeline,
        Func<Result<T>> callback)
    {
        if (pipeline is null) throw new ArgumentNullException(nameof(pipeline));
        if (callback is null) throw new ArgumentNullException(nameof(callback));

        return pipeline.Execute(static state => state(), callback);
    }

    /// <summary>
    /// Executes the specified callback through the resilience pipeline with state to avoid closure allocation.
    /// </summary>
    /// <typeparam name="T">The result value type.</typeparam>
    /// <typeparam name="TState">The state type passed to the callback.</typeparam>
    /// <param name="pipeline">The resilience pipeline.</param>
    /// <param name="state">The state instance.</param>
    /// <param name="callback">The callback to execute.</param>
    /// <returns>The executed <see cref="Result{TValue}"/>.</returns>
    public static Result<T> ExecuteResult<T, TState>(
        this ResiliencePipeline pipeline,
        TState state,
        Func<TState, Result<T>> callback)
    {
        if (pipeline is null) throw new ArgumentNullException(nameof(pipeline));
        if (callback is null) throw new ArgumentNullException(nameof(callback));

        return pipeline.Execute(static s => s.callback(s.state), (callback, state));
    }

    /// <summary>
    /// Asynchronously executes the specified callback through the resilience pipeline.
    /// </summary>
    /// <typeparam name="T">The result value type.</typeparam>
    /// <param name="pipeline">The resilience pipeline.</param>
    /// <param name="callback">The asynchronous callback to execute.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A value task containing the executed <see cref="Result{TValue}"/>.</returns>
    public static ValueTask<Result<T>> ExecuteResultAsync<T>(
        this ResiliencePipeline pipeline,
        Func<CancellationToken, ValueTask<Result<T>>> callback,
        CancellationToken cancellationToken = default)
    {
        if (pipeline is null) throw new ArgumentNullException(nameof(pipeline));
        if (callback is null) throw new ArgumentNullException(nameof(callback));

        return pipeline.ExecuteAsync(static async (cb, ct) => await cb(ct).ConfigureAwait(false), callback, cancellationToken);
    }

    /// <summary>
    /// Asynchronously executes the specified generic resilience pipeline callback.
    /// </summary>
    /// <typeparam name="T">The result value type.</typeparam>
    /// <param name="pipeline">The generic resilience pipeline.</param>
    /// <param name="callback">The asynchronous callback to execute.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A value task containing the executed <see cref="Result{TValue}"/>.</returns>
    public static ValueTask<Result<T>> ExecuteResultAsync<T>(
        this ResiliencePipeline<Result<T>> pipeline,
        Func<CancellationToken, ValueTask<Result<T>>> callback,
        CancellationToken cancellationToken = default)
    {
        if (pipeline is null) throw new ArgumentNullException(nameof(pipeline));
        if (callback is null) throw new ArgumentNullException(nameof(callback));

        return pipeline.ExecuteAsync(static async (cb, ct) => await cb(ct).ConfigureAwait(false), callback, cancellationToken);
    }

    /// <summary>
    /// Executes the specified non-generic callback through the resilience pipeline.
    /// </summary>
    /// <param name="pipeline">The resilience pipeline.</param>
    /// <param name="callback">The callback to execute.</param>
    /// <returns>The executed <see cref="Result"/>.</returns>
    public static Result ExecuteResult(
        this ResiliencePipeline pipeline,
        Func<Result> callback)
    {
        if (pipeline is null) throw new ArgumentNullException(nameof(pipeline));
        if (callback is null) throw new ArgumentNullException(nameof(callback));

        return pipeline.Execute(static state => state(), callback);
    }

    /// <summary>
    /// Executes the specified non-generic callback through the resilience pipeline with state to avoid closure allocation.
    /// </summary>
    /// <typeparam name="TState">The state type passed to the callback.</typeparam>
    /// <param name="pipeline">The resilience pipeline.</param>
    /// <param name="state">The state instance.</param>
    /// <param name="callback">The callback to execute.</param>
    /// <returns>The executed <see cref="Result"/>.</returns>
    public static Result ExecuteResult<TState>(
        this ResiliencePipeline pipeline,
        TState state,
        Func<TState, Result> callback)
    {
        if (pipeline is null) throw new ArgumentNullException(nameof(pipeline));
        if (callback is null) throw new ArgumentNullException(nameof(callback));

        return pipeline.Execute(static s => s.callback(s.state), (callback, state));
    }

    /// <summary>
    /// Asynchronously executes the specified non-generic callback through the resilience pipeline.
    /// </summary>
    /// <param name="pipeline">The resilience pipeline.</param>
    /// <param name="callback">The asynchronous callback to execute.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A value task containing the executed <see cref="Result"/>.</returns>
    public static ValueTask<Result> ExecuteResultAsync(
        this ResiliencePipeline pipeline,
        Func<CancellationToken, ValueTask<Result>> callback,
        CancellationToken cancellationToken = default)
    {
        if (pipeline is null) throw new ArgumentNullException(nameof(pipeline));
        if (callback is null) throw new ArgumentNullException(nameof(callback));

        return pipeline.ExecuteAsync(static async (cb, ct) => await cb(ct).ConfigureAwait(false), callback, cancellationToken);
    }
}
