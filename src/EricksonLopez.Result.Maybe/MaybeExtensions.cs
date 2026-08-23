// Copyright © Erickson Lopez. MIT License.
using System;
using System.Threading;
using System.Threading.Tasks;
using EricksonLopez.Result;

namespace EricksonLopez.Result.Maybe;

/// <summary>
/// Asynchronous extension methods for <see cref="Maybe{T}"/>.
/// </summary>
public static class MaybeExtensions
{
    /// <summary>
    /// Projects the value of a <c>Task&lt;Maybe&lt;T&gt;&gt;</c> using <paramref name="mapper"/>.
    /// </summary>
    /// <typeparam name="T">The source value type.</typeparam>
    /// <typeparam name="TNext">The projected value type.</typeparam>
    /// <param name="maybeTask">The task returning the maybe instance.</param>
    /// <param name="mapper">The projection function applied to the value.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the asynchronous operation.</param>
    /// <returns>A task representing the asynchronous operation. The task result contains a <see cref="Maybe{TNext}"/> with the mapped value if present; otherwise, <see cref="Maybe{TNext}.None"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="maybeTask"/> or <paramref name="mapper"/> is <see langword="null"/></exception>
    /// <exception cref="OperationCanceledException">The operation was canceled</exception>
    public static async Task<Maybe<TNext>> Map<T, TNext>(
        this Task<Maybe<T>> maybeTask,
        Func<T, TNext> mapper,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(maybeTask);
        ArgumentNullException.ThrowIfNull(mapper);
        cancellationToken.ThrowIfCancellationRequested();

        var maybe = await maybeTask.ConfigureAwait(false);
        return maybe.Map(mapper);
    }

    /// <summary>
    /// Binds the value of a <c>Task&lt;Maybe&lt;T&gt;&gt;</c> using <paramref name="bind"/>.
    /// </summary>
    /// <typeparam name="T">The source value type.</typeparam>
    /// <typeparam name="TNext">The target value type.</typeparam>
    /// <param name="maybeTask">The task returning the maybe instance.</param>
    /// <param name="bind">The asynchronous operation to execute with the value if present.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the asynchronous operation.</param>
    /// <returns>A task representing the asynchronous operation. The task result contains the bound <see cref="Maybe{TNext}"/> if present; otherwise, <see cref="Maybe{TNext}.None"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="maybeTask"/> or <paramref name="bind"/> is <see langword="null"/></exception>
    /// <exception cref="OperationCanceledException">The operation was canceled</exception>
    public static async Task<Maybe<TNext>> Bind<T, TNext>(
        this Task<Maybe<T>> maybeTask,
        Func<T, Task<Maybe<TNext>>> bind,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(maybeTask);
        ArgumentNullException.ThrowIfNull(bind);
        cancellationToken.ThrowIfCancellationRequested();

        var maybe = await maybeTask.ConfigureAwait(false);
        if (maybe.HasNoValue)
            return Maybe<TNext>.None;

        return await bind(maybe.Value).ConfigureAwait(false);
    }

    /// <summary>
    /// Converts a <c>Task&lt;Maybe&lt;T&gt;&gt;</c> to <c>Task&lt;Result&lt;T&gt;&gt;</c>.
    /// </summary>
    /// <typeparam name="T">The underlying value type.</typeparam>
    /// <param name="maybeTask">The task returning the maybe instance.</param>
    /// <param name="notFoundError">The error to return if no value is present.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the asynchronous operation.</param>
    /// <returns>A task representing the asynchronous operation. The task result contains a successful <see cref="Result{T}"/> if present; otherwise, a failure with <paramref name="notFoundError"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="maybeTask"/> or <paramref name="notFoundError"/> is <see langword="null"/></exception>
    /// <exception cref="OperationCanceledException">The operation was canceled</exception>
    public static async Task<Result<T>> ToResult<T>(
        this Task<Maybe<T>> maybeTask,
        Error notFoundError,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(maybeTask);
        ArgumentNullException.ThrowIfNull(notFoundError);
        cancellationToken.ThrowIfCancellationRequested();

        var maybe = await maybeTask.ConfigureAwait(false);
        return maybe.ToResult(notFoundError);
    }
}
