using System;
using System.Threading;
using System.Threading.Tasks;

namespace EricksonLopez.Result;

/// <summary>
/// Async extension methods for composing <see cref="Result"/> and <see cref="Result{T}"/>
/// pipelines over <see cref="Task"/>.
/// </summary>
public static partial class ResultExtensions
{
    // --------------------------------------------------------------------------
    //  Task<Result<T>> extensions
    // --------------------------------------------------------------------------

    public static Task<Result<TNext>> Map<T, TNext>(
        this Task<Result<T>> resultTask, Func<T, TNext> mapper, CancellationToken cancellationToken = default)
    {
        // Stryker disable all : Fast path optimization
        if (resultTask.IsCompletedSuccessfully)
            return Task.FromResult(resultTask.Result.Map(mapper));
        // Stryker restore all
        return MapCore(resultTask, mapper, cancellationToken);
        // Stryker disable all : Excluded from coverage
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage] static async Task<Result<TNext>> MapCore(Task<Result<T>> t, Func<T, TNext> m, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            return (await t.ConfigureAwait(false)).Map(m);
        }
        // Stryker restore all
    }

    public static Task<Result<TNext>> Map<TState, T, TNext>(
        this Task<Result<T>> resultTask, TState state, Func<TState, T, TNext> mapper, CancellationToken cancellationToken = default)
    {
        // Stryker disable all : Fast path optimization
        if (resultTask.IsCompletedSuccessfully)
            return Task.FromResult(resultTask.Result.Map(state, mapper));
        // Stryker restore all
        return MapStateCore(resultTask, state, mapper, cancellationToken);
        // Stryker disable all : Excluded from coverage
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage] static async Task<Result<TNext>> MapStateCore(Task<Result<T>> t, TState s, Func<TState, T, TNext> m, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            return (await t.ConfigureAwait(false)).Map(s, m);
        }
        // Stryker restore all
    }

    public static Task<Result<TNext>> Map<T, TNext>(
        this Task<Result<T>> resultTask, Func<T, Task<TNext>> mapper, CancellationToken cancellationToken = default)
    {
        // Stryker disable all : Fast path optimization
        if (resultTask.IsCompletedSuccessfully)
        {
            var r = resultTask.Result;
            if (r.IsFailure) return Task.FromResult(Result.Failure<TNext>(r.Error));
            return MapAsyncCore(mapper(r.Value), cancellationToken);
        }
        // Stryker restore all
        return MapFullAsync(resultTask, mapper, cancellationToken);
        // Stryker disable all : Excluded from coverage
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage] static async Task<Result<TNext>> MapAsyncCore(Task<TNext> mapTask, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            return Result.Success(await mapTask.ConfigureAwait(false));
        }
        // Stryker restore all
        // Stryker disable all : Excluded from coverage
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage] static async Task<Result<TNext>> MapFullAsync(Task<Result<T>> t, Func<T, Task<TNext>> m, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            var result = await t.ConfigureAwait(false);
            if (result.IsFailure) return Result.Failure<TNext>(result.Error);
            var next = await m(result.Value).ConfigureAwait(false);
            return Result.Success(next);
        }
        // Stryker restore all
    }

    /// <summary>
    /// Maps the success value of a <see cref="Task{TResult}"/> wrapping a <see cref="Result{T}"/> using
    /// an async mapper with captured <typeparamref name="TState"/> to avoid closure allocations.
    /// </summary>
    public static Task<Result<TNext>> Map<TState, T, TNext>(
        this Task<Result<T>> resultTask, TState state, Func<TState, T, Task<TNext>> mapper, CancellationToken cancellationToken = default)
    {
        // Stryker disable all : Fast path optimization
        if (resultTask.IsCompletedSuccessfully)
        {
            var r = resultTask.Result;
            if (r.IsFailure) return Task.FromResult(Result.Failure<TNext>(r.Error));
            return MapStateAsyncCore(state, r.Value, mapper, cancellationToken);
        }
        // Stryker restore all
        return MapStateFullAsync(resultTask, state, mapper, cancellationToken);
        // Stryker disable all : Excluded from coverage
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage] static async Task<Result<TNext>> MapStateAsyncCore(TState s, T value, Func<TState, T, Task<TNext>> m, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            return Result.Success(await m(s, value).ConfigureAwait(false));
        }
        // Stryker restore all
        // Stryker disable all : Excluded from coverage
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage] static async Task<Result<TNext>> MapStateFullAsync(Task<Result<T>> t, TState s, Func<TState, T, Task<TNext>> m, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            var result = await t.ConfigureAwait(false);
            if (result.IsFailure) return Result.Failure<TNext>(result.Error);
            return Result.Success(await m(s, result.Value).ConfigureAwait(false));
        }
        // Stryker restore all
    }

    public static Task<Result<TNext>> Bind<T, TNext>(
        this Task<Result<T>> resultTask, Func<T, Result<TNext>> bind, CancellationToken cancellationToken = default)
    {
        // Stryker disable all : Fast path optimization
        if (resultTask.IsCompletedSuccessfully)
            return Task.FromResult(resultTask.Result.Bind(bind));
        // Stryker restore all
        return BindCore(resultTask, bind, cancellationToken);
        // Stryker disable all : Excluded from coverage
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage] static async Task<Result<TNext>> BindCore(Task<Result<T>> t, Func<T, Result<TNext>> b, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            return (await t.ConfigureAwait(false)).Bind(b);
        }
        // Stryker restore all
    }

    public static Task<Result<TNext>> Bind<TState, T, TNext>(
        this Task<Result<T>> resultTask, TState state, Func<TState, T, Result<TNext>> bind, CancellationToken cancellationToken = default)
    {
        // Stryker disable all : Fast path optimization
        if (resultTask.IsCompletedSuccessfully)
            return Task.FromResult(resultTask.Result.Bind(state, bind));
        // Stryker restore all
        return BindStateCore(resultTask, state, bind, cancellationToken);
        // Stryker disable all : Excluded from coverage
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage] static async Task<Result<TNext>> BindStateCore(Task<Result<T>> t, TState s, Func<TState, T, Result<TNext>> b, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            return (await t.ConfigureAwait(false)).Bind(s, b);
        }
        // Stryker restore all
    }

    public static Task<Result<TNext>> Bind<T, TNext>(
        this Task<Result<T>> resultTask, Func<T, Task<Result<TNext>>> bind, CancellationToken cancellationToken = default)
    {
        // Stryker disable all : Fast path optimization
        if (resultTask.IsCompletedSuccessfully)
        {
            var r = resultTask.Result;
            if (r.IsFailure) return Task.FromResult(Result.Failure<TNext>(r.Error));
            return bind(r.Value);
        }
        // Stryker restore all
        return BindAsyncCore(resultTask, bind, cancellationToken);
        // Stryker disable all : Excluded from coverage
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage] static async Task<Result<TNext>> BindAsyncCore(Task<Result<T>> t, Func<T, Task<Result<TNext>>> b, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            var result = await t.ConfigureAwait(false);
            if (result.IsFailure) return Result.Failure<TNext>(result.Error);
            return await b(result.Value).ConfigureAwait(false);
        }
        // Stryker restore all
    }

    public static Task<Result> Bind<T>(
        this Task<Result<T>> resultTask, Func<T, Task<Result>> bind, CancellationToken cancellationToken = default)
    {
        // Stryker disable all : Fast path optimization
        if (resultTask.IsCompletedSuccessfully)
        {
            var r = resultTask.Result;
            if (r.IsFailure) return Task.FromResult(Result.Failure(r.Error));
            return bind(r.Value);
        }
        // Stryker restore all
        return BindAsyncCore(resultTask, bind, cancellationToken);
        // Stryker disable all : Excluded from coverage
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage] static async Task<Result> BindAsyncCore(Task<Result<T>> t, Func<T, Task<Result>> b, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            var result = await t.ConfigureAwait(false);
            if (result.IsFailure) return Result.Failure(result.Error);
            return await b(result.Value).ConfigureAwait(false);
        }
        // Stryker restore all
    }

    public static Task<Result> Bind<T>(
        this Task<Result<T>> resultTask, Func<T, Result> bind, CancellationToken cancellationToken = default)
    {
        // Stryker disable all : Fast path optimization
        if (resultTask.IsCompletedSuccessfully) return Task.FromResult(resultTask.Result.Bind(bind));
        return BindSyncCore(resultTask, bind, cancellationToken);
        // Stryker restore all
        // Stryker disable all : Excluded from coverage
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage] static async Task<Result> BindSyncCore(Task<Result<T>> t, Func<T, Result> b, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            return (await t.ConfigureAwait(false)).Bind(b);
        }
        // Stryker restore all
    }

    public static Task<Result> Bind<TState, T>(
        this Task<Result<T>> resultTask, TState state, Func<TState, T, Result> bind, CancellationToken cancellationToken = default)
    {
        // Stryker disable all : Fast path optimization
        if (resultTask.IsCompletedSuccessfully) return Task.FromResult(resultTask.Result.Bind(state, bind));
        return BindStateSyncCore(resultTask, state, bind, cancellationToken);
        // Stryker restore all
        // Stryker disable all : Excluded from coverage
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage] static async Task<Result> BindStateSyncCore(Task<Result<T>> t, TState s, Func<TState, T, Result> b, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            return (await t.ConfigureAwait(false)).Bind(s, b);
        }
        // Stryker restore all
    }

    public static Task<TOut> Match<T, TOut>(
        this Task<Result<T>> resultTask,
        Func<T, TOut> onSuccess, Func<Error, TOut> onFailure)
    {
        // Stryker disable all : Fast path optimization
        if (resultTask.IsCompletedSuccessfully) return Task.FromResult(resultTask.Result.Match(onSuccess, onFailure));
        return MatchCore(resultTask, onSuccess, onFailure);
        // Stryker restore all
        // Stryker disable all : Excluded from coverage
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage] static async Task<TOut> MatchCore(Task<Result<T>> t, Func<T, TOut> s, Func<Error, TOut> f)
            => (await t.ConfigureAwait(false)).Match(s, f);
    }

    public static Task<TOut> Match<TState, T, TOut>(
        this Task<Result<T>> resultTask, TState state,
        Func<TState, T, TOut> onSuccess, Func<TState, Error, TOut> onFailure)
    {
        // Stryker restore all
        // Stryker disable all : Fast path optimization
        if (resultTask.IsCompletedSuccessfully) return Task.FromResult(resultTask.Result.Match(state, onSuccess, onFailure));
        return MatchCore(resultTask, state, onSuccess, onFailure);
        // Stryker restore all
        // Stryker disable all : Excluded from coverage
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage] static async Task<TOut> MatchCore(Task<Result<T>> t, TState st, Func<TState, T, TOut> s, Func<TState, Error, TOut> f)
            => (await t.ConfigureAwait(false)).Match(st, s, f);
    }

    public static Task Execute<T>(
        this Task<Result<T>> resultTask,
        Action<T> onSuccess, Action<Error> onFailure, CancellationToken cancellationToken = default)
    {
        // Stryker restore all
        // Stryker disable all : Fast path optimization
        if (resultTask.IsCompletedSuccessfully)
        {
            resultTask.Result.Execute(onSuccess, onFailure);
            return Task.CompletedTask;
        }
        // Stryker restore all
        return ExecuteCore(resultTask, onSuccess, onFailure, cancellationToken);
        // Stryker disable all : Excluded from coverage
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage] static async Task ExecuteCore(Task<Result<T>> t, Action<T> s, Action<Error> f, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            (await t.ConfigureAwait(false)).Execute(s, f);
        }
        // Stryker restore all
    }

    public static Task Execute<TState, T>(
        this Task<Result<T>> resultTask, TState state,
        Action<TState, T> onSuccess, Action<TState, Error> onFailure, CancellationToken cancellationToken = default)
    {
        // Stryker disable all : Fast path optimization
        if (resultTask.IsCompletedSuccessfully)
        {
            resultTask.Result.Execute(state, onSuccess, onFailure);
            return Task.CompletedTask;
        }
        // Stryker restore all
        return ExecuteCore(resultTask, state, onSuccess, onFailure, cancellationToken);
        // Stryker disable all : Excluded from coverage
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage] static async Task ExecuteCore(Task<Result<T>> t, TState st, Action<TState, T> s, Action<TState, Error> f, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            (await t.ConfigureAwait(false)).Execute(st, s, f);
        }
        // Stryker restore all
    }

    public static Task<Result<T>> TapOnSuccess<T>(
        this Task<Result<T>> resultTask, Action<T> action, CancellationToken cancellationToken = default)
    {
        // Stryker disable all : Fast path optimization
        if (resultTask.IsCompletedSuccessfully) return Task.FromResult(resultTask.Result.TapOnSuccess(action));
        return TapCore(resultTask, action, cancellationToken);
        // Stryker restore all
        // Stryker disable all : Excluded from coverage
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage] static async Task<Result<T>> TapCore(Task<Result<T>> t, Action<T> a, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            return (await t.ConfigureAwait(false)).TapOnSuccess(a);
        }
        // Stryker restore all
    }

    public static Task<Result<T>> TapOnSuccess<TState, T>(
        this Task<Result<T>> resultTask, TState state, Action<TState, T> action, CancellationToken cancellationToken = default)
    {
        // Stryker disable all : Fast path optimization
        if (resultTask.IsCompletedSuccessfully) return Task.FromResult(resultTask.Result.TapOnSuccess(state, action));
        return TapCore(resultTask, state, action, cancellationToken);
        // Stryker restore all
        // Stryker disable all : Excluded from coverage
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage] static async Task<Result<T>> TapCore(Task<Result<T>> t, TState s, Action<TState, T> a, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            return (await t.ConfigureAwait(false)).TapOnSuccess(s, a);
        }
        // Stryker restore all
    }

    public static Task<Result<T>> TapOnSuccess<T>(
        this Task<Result<T>> resultTask, Func<T, Task> action, CancellationToken cancellationToken = default)
    {
        // Stryker disable all : Fast path optimization
        if (resultTask.IsCompletedSuccessfully)
        {
            var r = resultTask.Result;
            if (r.IsFailure) return Task.FromResult(r);
            return TapAsyncCore(r, action(r.Value), cancellationToken);
        }
        // Stryker restore all
        return TapFullAsync(resultTask, action, cancellationToken);
        // Stryker disable all : Excluded from coverage
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage] static async Task<Result<T>> TapAsyncCore(Result<T> r, Task actionTask, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            await actionTask.ConfigureAwait(false);
            return r;
        }
        // Stryker restore all
        // Stryker disable all : Excluded from coverage
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage] static async Task<Result<T>> TapFullAsync(Task<Result<T>> t, Func<T, Task> a, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            var result = await t.ConfigureAwait(false);
            if (result.IsSuccess) await a(result.Value).ConfigureAwait(false);
            return result;
        }
        // Stryker restore all
    }

    public static Task<Result<T>> TapOnFailure<T>(
        this Task<Result<T>> resultTask, Action<Error> action, CancellationToken cancellationToken = default)
    {
        // Stryker disable all : Fast path optimization
        if (resultTask.IsCompletedSuccessfully) return Task.FromResult(resultTask.Result.TapOnFailure(action));
        return TapOnFailureCore(resultTask, action, cancellationToken);
        // Stryker restore all
        // Stryker disable all : Excluded from coverage
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage] static async Task<Result<T>> TapOnFailureCore(Task<Result<T>> t, Action<Error> a, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            return (await t.ConfigureAwait(false)).TapOnFailure(a);
        }
        // Stryker restore all
    }

    public static Task<Result<T>> TapOnFailure<TState, T>(
        this Task<Result<T>> resultTask, TState state, Action<TState, Error> action, CancellationToken cancellationToken = default)
    {
        // Stryker disable all : Fast path optimization
        if (resultTask.IsCompletedSuccessfully) return Task.FromResult(resultTask.Result.TapOnFailure(state, action));
        return TapOnFailureCore(resultTask, state, action, cancellationToken);
        // Stryker restore all
        // Stryker disable all : Excluded from coverage
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage] static async Task<Result<T>> TapOnFailureCore(Task<Result<T>> t, TState s, Action<TState, Error> a, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            return (await t.ConfigureAwait(false)).TapOnFailure(s, a);
        }
        // Stryker restore all
    }

    public static Task<Result<T>> TapOnFailure<T>(
        this Task<Result<T>> resultTask, Func<Error, Task> action, CancellationToken cancellationToken = default)
    {
        // Stryker disable all : Fast path optimization
        if (!resultTask.IsCompletedSuccessfully)
            return TapOnFailureFullAsync(resultTask, action, cancellationToken);
        // Stryker restore all

        var r = resultTask.Result;
        return r.IsSuccess ? Task.FromResult(r) : TapOnFailureAsyncCore(r, action(r.Error), cancellationToken);

        // Stryker disable all : Excluded from coverage
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage] static async Task<Result<T>> TapOnFailureAsyncCore(Result<T> r, Task actionTask, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            await actionTask.ConfigureAwait(false);
            return r;
        }
        // Stryker restore all
        // Stryker disable all : Excluded from coverage
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage] static async Task<Result<T>> TapOnFailureFullAsync(Task<Result<T>> t, Func<Error, Task> a, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            var result = await t.ConfigureAwait(false);
            if (result.IsFailure) await a(result.Error).ConfigureAwait(false);
            return result;
        }
        // Stryker restore all
    }

    public static Task<Result<T>> Ensure<T>(
        this Task<Result<T>> resultTask, Func<T, bool> predicate, Error error, CancellationToken cancellationToken = default)
    {
        // Stryker disable all : Fast path optimization
        if (resultTask.IsCompletedSuccessfully) return Task.FromResult(resultTask.Result.Ensure(predicate, error));
        return EnsureCore(resultTask, predicate, error, cancellationToken);
        // Stryker restore all
        // Stryker disable all : Excluded from coverage
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage] static async Task<Result<T>> EnsureCore(Task<Result<T>> t, Func<T, bool> p, Error e, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            return (await t.ConfigureAwait(false)).Ensure(p, e);
        }
        // Stryker restore all
    }

    public static Task<Result<T>> Ensure<TState, T>(
        this Task<Result<T>> resultTask, TState state, Func<TState, T, bool> predicate, Error error, CancellationToken cancellationToken = default)
    {
        // Stryker disable all : Fast path optimization
        if (resultTask.IsCompletedSuccessfully) return Task.FromResult(resultTask.Result.Ensure(state, predicate, error));
        return EnsureCore(resultTask, state, predicate, error, cancellationToken);
        // Stryker restore all
        // Stryker disable all : Excluded from coverage
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage] static async Task<Result<T>> EnsureCore(Task<Result<T>> t, TState s, Func<TState, T, bool> p, Error e, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            return (await t.ConfigureAwait(false)).Ensure(s, p, e);
        }
        // Stryker restore all
    }

    public static Task<Result<T>> Ensure<T>(
        this Task<Result<T>> resultTask, Func<T, Task<bool>> predicate, Error error, CancellationToken cancellationToken = default)
    {
        // Stryker disable all : Fast path optimization
        if (resultTask.IsCompletedSuccessfully)
        {
            var r = resultTask.Result;
            if (r.IsFailure) return Task.FromResult(r);
            return EnsureAsyncCore(r, predicate(r.Value), error, cancellationToken);
        }
        // Stryker restore all
        return EnsureFullAsync(resultTask, predicate, error, cancellationToken);
        // Stryker disable all : Excluded from coverage
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage] static async Task<Result<T>> EnsureAsyncCore(Result<T> r, Task<bool> predicateTask, Error err, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            return await predicateTask.ConfigureAwait(false) ? r : Result.Failure<T>(err);
        }
        // Stryker restore all
        // Stryker disable all : Excluded from coverage
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage] static async Task<Result<T>> EnsureFullAsync(Task<Result<T>> t, Func<T, Task<bool>> p, Error err, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            var result = await t.ConfigureAwait(false);
            if (result.IsFailure) return result;
            return await p(result.Value).ConfigureAwait(false)
                ? result
                : Result.Failure<T>(err);
        }
        // Stryker restore all
    }

    /// <summary>
    /// Applies an async predicate with captured <typeparamref name="TState"/> to the success value
    /// of a <see cref="Task{TResult}"/> wrapping a <see cref="Result{T}"/>, avoiding closure allocations.
    /// </summary>
    public static Task<Result<T>> Ensure<TState, T>(
        this Task<Result<T>> resultTask, TState state, Func<TState, T, Task<bool>> predicate, Error error, CancellationToken cancellationToken = default)
    {
        // Stryker disable all : Fast path optimization
        if (resultTask.IsCompletedSuccessfully)
        {
            var r = resultTask.Result;
            if (r.IsFailure) return Task.FromResult(r);
            return EnsureStateAsyncCore(state, r, predicate, error, cancellationToken);
        }
        // Stryker restore all
        return EnsureStateFullAsync(resultTask, state, predicate, error, cancellationToken);
        // Stryker disable all : Excluded from coverage
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage] static async Task<Result<T>> EnsureStateAsyncCore(TState s, Result<T> r, Func<TState, T, Task<bool>> p, Error err, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            return await p(s, r.Value).ConfigureAwait(false) ? r : Result.Failure<T>(err);
        }
        // Stryker restore all
        // Stryker disable all : Excluded from coverage
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage] static async Task<Result<T>> EnsureStateFullAsync(Task<Result<T>> t, TState s, Func<TState, T, Task<bool>> p, Error err, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            var result = await t.ConfigureAwait(false);
            if (result.IsFailure) return result;
            return await p(s, result.Value).ConfigureAwait(false)
                ? result
                : Result.Failure<T>(err);
        }
        // Stryker restore all
    }

    public static Task<Result<T>> Recover<T>(
        this Task<Result<T>> resultTask, Func<Error, Result<T>> recovery, CancellationToken cancellationToken = default)
    {
        // Stryker disable all : Fast path optimization
        if (resultTask.IsCompletedSuccessfully) return Task.FromResult(resultTask.Result.Recover(recovery));
        return RecoverCore(resultTask, recovery, cancellationToken);
        // Stryker restore all
        // Stryker disable all : Excluded from coverage
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage] static async Task<Result<T>> RecoverCore(Task<Result<T>> t, Func<Error, Result<T>> r, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            return (await t.ConfigureAwait(false)).Recover(r);
        }
        // Stryker restore all
    }

    public static Task<Result<T>> Recover<TState, T>(
        this Task<Result<T>> resultTask, TState state, Func<TState, Error, Result<T>> recovery, CancellationToken cancellationToken = default)
    {
        // Stryker disable all : Fast path optimization
        if (resultTask.IsCompletedSuccessfully) return Task.FromResult(resultTask.Result.Recover(state, recovery));
        return RecoverCore(resultTask, state, recovery, cancellationToken);
        // Stryker restore all
        // Stryker disable all : Excluded from coverage
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage] static async Task<Result<T>> RecoverCore(Task<Result<T>> t, TState s, Func<TState, Error, Result<T>> r, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            return (await t.ConfigureAwait(false)).Recover(s, r);
        }
        // Stryker restore all
    }

    public static Task<Result<T>> Recover<T>(
        this Task<Result<T>> resultTask, Func<Error, Task<Result<T>>> recovery, CancellationToken cancellationToken = default)
    {
        // Stryker disable all : Fast path optimization
        if (resultTask.IsCompletedSuccessfully)
        {
            var r = resultTask.Result;
            if (r.IsSuccess) return Task.FromResult(r);
            return recovery(r.Error);
        }
        // Stryker restore all
        return RecoverAsyncCore(resultTask, recovery, cancellationToken);
        // Stryker disable all : Excluded from coverage
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage] static async Task<Result<T>> RecoverAsyncCore(Task<Result<T>> t, Func<Error, Task<Result<T>>> r, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            var result = await t.ConfigureAwait(false);
            if (result.IsSuccess) return result;
            return await r(result.Error).ConfigureAwait(false);
        }
        // Stryker restore all
    }

    /// <summary>
    /// Attempts to recover from a failure using an async recovery function with captured
    /// <typeparamref name="TState"/>, avoiding closure allocations.
    /// </summary>
    public static Task<Result<T>> Recover<TState, T>(
        this Task<Result<T>> resultTask, TState state, Func<TState, Error, Task<Result<T>>> recovery, CancellationToken cancellationToken = default)
    {
        // Stryker disable all : Fast path optimization
        if (!resultTask.IsCompletedSuccessfully)
            return RecoverStateAsyncCore(resultTask, state, recovery, cancellationToken);
        // Stryker restore all

        var r = resultTask.Result;
        // Stryker disable once all : Fast path optimization
        return r.IsSuccess ? Task.FromResult(r) : RecoverStateAsyncCore(resultTask, state, recovery, cancellationToken);
        // Stryker disable all : Excluded from coverage
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage] static async Task<Result<T>> RecoverStateAsyncCore(Task<Result<T>> t, TState s, Func<TState, Error, Task<Result<T>>> r, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            var result = await t.ConfigureAwait(false);
            if (result.IsSuccess) return result;
            return await r(s, result.Error).ConfigureAwait(false);
        }
        // Stryker restore all
    }

    public static Task<Result<T>> MapError<T>(
        this Task<Result<T>> resultTask, Func<Error, Error> mapper, CancellationToken cancellationToken = default)
    {
        // Stryker disable all : Fast path optimization
        if (resultTask.IsCompletedSuccessfully) return Task.FromResult(resultTask.Result.MapError(mapper));
        return MapErrorCore(resultTask, mapper, cancellationToken);
        // Stryker restore all
        // Stryker disable all : Excluded from coverage
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage] static async Task<Result<T>> MapErrorCore(Task<Result<T>> t, Func<Error, Error> m, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            return (await t.ConfigureAwait(false)).MapError(m);
        }
        // Stryker restore all
    }

    public static Task<Result<T>> MapError<TState, T>(
        this Task<Result<T>> resultTask, TState state, Func<TState, Error, Error> mapper, CancellationToken cancellationToken = default)
    {
        // Stryker disable all : Fast path optimization
        if (resultTask.IsCompletedSuccessfully) return Task.FromResult(resultTask.Result.MapError(state, mapper));
        return MapErrorCore(resultTask, state, mapper, cancellationToken);
        // Stryker restore all
        // Stryker disable all : Excluded from coverage
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage] static async Task<Result<T>> MapErrorCore(Task<Result<T>> t, TState s, Func<TState, Error, Error> m, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            return (await t.ConfigureAwait(false)).MapError(s, m);
        }
        // Stryker restore all
    }

    public static Task<Result<T>> Inspect<T>(
        this Task<Result<T>> resultTask, Action<Result<T>> action, CancellationToken cancellationToken = default)
    {
        // Stryker disable all : Fast path optimization
        if (resultTask.IsCompletedSuccessfully) return Task.FromResult(resultTask.Result.Inspect(action));
        return InspectCore(resultTask, action, cancellationToken);
        // Stryker restore all
        // Stryker disable all : Excluded from coverage
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage] static async Task<Result<T>> InspectCore(Task<Result<T>> t, Action<Result<T>> a, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            return (await t.ConfigureAwait(false)).Inspect(a);
        }
        // Stryker restore all
    }

    public static Task<Result<T>> Inspect<TState, T>(
        this Task<Result<T>> resultTask, TState state, Action<TState, Result<T>> action, CancellationToken cancellationToken = default)
    {
        // Stryker disable all : Fast path optimization
        if (resultTask.IsCompletedSuccessfully) return Task.FromResult(resultTask.Result.Inspect(state, action));
        return InspectCore(resultTask, state, action, cancellationToken);
        // Stryker restore all
        // Stryker disable all : Excluded from coverage
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage] static async Task<Result<T>> InspectCore(Task<Result<T>> t, TState s, Action<TState, Result<T>> a, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            return (await t.ConfigureAwait(false)).Inspect(s, a);
        }
        // Stryker restore all
    }

    [Obsolete("Use Inspect instead to clarify that the pipeline continues after execution.", error: true)]
    public static Task<Result<T>> Finally<T>(
        this Task<Result<T>> resultTask, Action<Result<T>> action, CancellationToken cancellationToken = default)
        => resultTask.Inspect(action, cancellationToken);

    // --------------------------------------------------------------------------
    //  Task<Result> (non-generic) extensions
    // --------------------------------------------------------------------------

    public static Task<Result> Bind(
        this Task<Result> resultTask, Func<Result> bind, CancellationToken cancellationToken = default)
    {
        // Stryker disable all : Fast path optimization
        if (resultTask.IsCompletedSuccessfully) return Task.FromResult(resultTask.Result.Bind(bind));
        return BindCore(resultTask, bind, cancellationToken);
        // Stryker restore all
        // Stryker disable all : Excluded from coverage
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage] static async Task<Result> BindCore(Task<Result> t, Func<Result> b, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            return (await t.ConfigureAwait(false)).Bind(b);
        }
        // Stryker restore all
    }

    public static Task<Result> Bind<TState>(
        this Task<Result> resultTask, TState state, Func<TState, Result> bind, CancellationToken cancellationToken = default)
    {
        // Stryker disable all : Fast path optimization
        if (resultTask.IsCompletedSuccessfully) return Task.FromResult(resultTask.Result.Bind(state, bind));
        return BindCore(resultTask, state, bind, cancellationToken);
        // Stryker restore all
        // Stryker disable all : Excluded from coverage
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage] static async Task<Result> BindCore(Task<Result> t, TState s, Func<TState, Result> b, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            return (await t.ConfigureAwait(false)).Bind(s, b);
        }
        // Stryker restore all
    }

    public static Task<Result> Bind(
        this Task<Result> resultTask, Func<Task<Result>> bind, CancellationToken cancellationToken = default)
    {
        // Stryker disable all : Fast path optimization
        if (resultTask.IsCompletedSuccessfully)
        {
            var r = resultTask.Result;
            if (r.IsFailure) return Task.FromResult(r);
            return bind();
        }
        // Stryker restore all
        return BindAsyncCore(resultTask, bind, cancellationToken);
        // Stryker disable all : Excluded from coverage
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage] static async Task<Result> BindAsyncCore(Task<Result> t, Func<Task<Result>> b, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            var result = await t.ConfigureAwait(false);
            if (result.IsFailure) return result;
            return await b().ConfigureAwait(false);
        }
        // Stryker restore all
    }

    public static Task<Result<TNext>> Bind<TNext>(
        this Task<Result> resultTask, Func<Result<TNext>> bind, CancellationToken cancellationToken = default)
    {
        // Stryker disable all : Fast path optimization
        if (resultTask.IsCompletedSuccessfully) return Task.FromResult(resultTask.Result.Bind(bind));
        return BindCore(resultTask, bind, cancellationToken);
        // Stryker restore all
        // Stryker disable all : Excluded from coverage
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage] static async Task<Result<TNext>> BindCore(Task<Result> t, Func<Result<TNext>> b, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            return (await t.ConfigureAwait(false)).Bind(b);
        }
        // Stryker restore all
    }

    public static Task<Result<TNext>> Bind<TState, TNext>(
        this Task<Result> resultTask, TState state, Func<TState, Result<TNext>> bind, CancellationToken cancellationToken = default)
    {
        // Stryker disable all : Fast path optimization
        if (resultTask.IsCompletedSuccessfully) return Task.FromResult(resultTask.Result.Bind(state, bind));
        return BindCore(resultTask, state, bind, cancellationToken);
        // Stryker restore all
        // Stryker disable all : Excluded from coverage
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage] static async Task<Result<TNext>> BindCore(Task<Result> t, TState s, Func<TState, Result<TNext>> b, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            return (await t.ConfigureAwait(false)).Bind(s, b);
        }
        // Stryker restore all
    }

    public static Task<Result<TNext>> Bind<TNext>(
        this Task<Result> resultTask, Func<Task<Result<TNext>>> bind, CancellationToken cancellationToken = default)
    {
        // Stryker disable all : Fast path optimization
        if (resultTask.IsCompletedSuccessfully)
        {
            var r = resultTask.Result;
            if (r.IsFailure) return Task.FromResult(Result.Failure<TNext>(r.Error));
            return bind();
        }
        // Stryker restore all
        return BindAsyncCore(resultTask, bind, cancellationToken);
        // Stryker disable all : Excluded from coverage
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage] static async Task<Result<TNext>> BindAsyncCore(Task<Result> t, Func<Task<Result<TNext>>> b, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            var result = await t.ConfigureAwait(false);
            if (result.IsFailure) return Result.Failure<TNext>(result.Error);
            return await b().ConfigureAwait(false);
        }
        // Stryker restore all
    }

    /// <summary>
    /// If the <see cref="Result"/> is a success, invokes the async <paramref name="bind"/> delegate
    /// with a <see cref="CancellationToken"/>, returning its result. If the Result is a failure, returns
    /// the failure without invoking <paramref name="bind"/>.
    /// </summary>
    public static Task<Result> Bind(
        this Task<Result> resultTask, Func<CancellationToken, Task<Result>> bind, CancellationToken cancellationToken = default)
    {
        // Stryker disable all : Fast path optimization
        if (resultTask.IsCompletedSuccessfully)
        {
            var r = resultTask.Result;
            if (r.IsFailure) return Task.FromResult(r);
            return bind(cancellationToken);
        }
        // Stryker restore all
        return BindCtAsyncCore(resultTask, bind, cancellationToken);
        // Stryker disable all : Excluded from coverage
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage] static async Task<Result> BindCtAsyncCore(Task<Result> t, Func<CancellationToken, Task<Result>> b, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            var result = await t.ConfigureAwait(false);
            if (result.IsFailure) return result;
            return await b(ct).ConfigureAwait(false);
        }
        // Stryker restore all
    }

    /// <summary>
    /// If the <see cref="Result"/> is a success, invokes the async <paramref name="bind"/> delegate
    /// with a <see cref="CancellationToken"/>, returning a typed Result. If the Result is a failure,
    /// returns the failure without invoking <paramref name="bind"/>.
    /// </summary>
    public static Task<Result<TNext>> Bind<TNext>(
        this Task<Result> resultTask, Func<CancellationToken, Task<Result<TNext>>> bind, CancellationToken cancellationToken = default)
    {
        // Stryker disable all : Fast path optimization
        if (resultTask.IsCompletedSuccessfully)
        {
            var r = resultTask.Result;
            if (r.IsFailure) return Task.FromResult(Result.Failure<TNext>(r.Error));
            return bind(cancellationToken);
        }
        // Stryker restore all
        return BindCtAsyncCore(resultTask, bind, cancellationToken);
        // Stryker disable all : Excluded from coverage
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage] static async Task<Result<TNext>> BindCtAsyncCore(Task<Result> t, Func<CancellationToken, Task<Result<TNext>>> b, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            var result = await t.ConfigureAwait(false);
            if (result.IsFailure) return Result.Failure<TNext>(result.Error);
            return await b(ct).ConfigureAwait(false);
        }
        // Stryker restore all
    }

    /// <summary>
    /// Projects a successful <see cref="Result"/> into a <see cref="Result{TNext}"/> by executing
    /// an async <paramref name="mapper"/> that receives a <see cref="CancellationToken"/>.
    /// If the Result is a failure, returns a failure without invoking <paramref name="mapper"/>.
    /// </summary>
    public static Task<Result<TNext>> Map<TNext>(
        this Task<Result> resultTask, Func<CancellationToken, Task<TNext>> mapper, CancellationToken cancellationToken = default)
    {
        // Stryker disable all : Fast path optimization
        if (resultTask.IsCompletedSuccessfully)
        {
            if (cancellationToken.IsCancellationRequested) return Task.FromCanceled<Result<TNext>>(cancellationToken);
            var r = resultTask.Result;
            if (r.IsFailure) return Task.FromResult(Result.Failure<TNext>(r.Error));
            return MapCtAsyncCore(mapper(cancellationToken));
        }
        // Stryker restore all
        return MapCtFullAsync(resultTask, mapper, cancellationToken);
        // Stryker disable all : Excluded from coverage
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage] static async Task<Result<TNext>> MapCtAsyncCore(Task<TNext> mapTask)
        {
            return Result.Success(await mapTask.ConfigureAwait(false));
        }
        // Stryker restore all
        // Stryker disable all : Excluded from coverage
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage] static async Task<Result<TNext>> MapCtFullAsync(Task<Result> t, Func<CancellationToken, Task<TNext>> m, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            var result = await t.ConfigureAwait(false);
            if (result.IsFailure) return Result.Failure<TNext>(result.Error);
            return Result.Success(await m(ct).ConfigureAwait(false));
        }
        // Stryker restore all
    }

    public static Task<TOut> Match<TOut>(
        this Task<Result> resultTask,
        Func<TOut> onSuccess, Func<Error, TOut> onFailure)
    {
        // Stryker disable all : Fast path optimization
        if (resultTask.IsCompletedSuccessfully) return Task.FromResult(resultTask.Result.Match(onSuccess, onFailure));
        return MatchCore(resultTask, onSuccess, onFailure);
        // Stryker restore all
        // Stryker disable all : Excluded from coverage
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage] static async Task<TOut> MatchCore(Task<Result> t, Func<TOut> s, Func<Error, TOut> f)
            => (await t.ConfigureAwait(false)).Match(s, f);
    }

    public static Task<TOut> Match<TState, TOut>(
        this Task<Result> resultTask, TState state,
        Func<TState, TOut> onSuccess, Func<TState, Error, TOut> onFailure)
    {
        // Stryker restore all
        // Stryker disable all : Fast path optimization
        if (resultTask.IsCompletedSuccessfully) return Task.FromResult(resultTask.Result.Match(state, onSuccess, onFailure));
        return MatchCore(resultTask, state, onSuccess, onFailure);
        // Stryker restore all
        // Stryker disable all : Excluded from coverage
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage] static async Task<TOut> MatchCore(Task<Result> t, TState st, Func<TState, TOut> s, Func<TState, Error, TOut> f)
            => (await t.ConfigureAwait(false)).Match(st, s, f);
    }

    public static Task Execute(
        this Task<Result> resultTask,
        Action onSuccess, Action<Error> onFailure, CancellationToken cancellationToken = default)
    {
        // Stryker restore all
        // Stryker disable all : Fast path optimization
        if (resultTask.IsCompletedSuccessfully)
        {
            resultTask.Result.Execute(onSuccess, onFailure);
            return Task.CompletedTask;
        }
        // Stryker restore all
        return ExecuteCore(resultTask, onSuccess, onFailure, cancellationToken);
        // Stryker disable all : Excluded from coverage
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage] static async Task ExecuteCore(Task<Result> t, Action s, Action<Error> f, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            (await t.ConfigureAwait(false)).Execute(s, f);
        }
        // Stryker restore all
    }

    public static Task Execute<TState>(
        this Task<Result> resultTask, TState state,
        Action<TState> onSuccess, Action<TState, Error> onFailure, CancellationToken cancellationToken = default)
    {
        // Stryker disable all : Fast path optimization
        if (resultTask.IsCompletedSuccessfully)
        {
            resultTask.Result.Execute(state, onSuccess, onFailure);
            return Task.CompletedTask;
        }
        // Stryker restore all
        return ExecuteCore(resultTask, state, onSuccess, onFailure, cancellationToken);
        // Stryker disable all : Excluded from coverage
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage] static async Task ExecuteCore(Task<Result> t, TState st, Action<TState> s, Action<TState, Error> f, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            (await t.ConfigureAwait(false)).Execute(st, s, f);
        }
        // Stryker restore all
    }

    public static Task<Result> TapOnSuccess(
        this Task<Result> resultTask, Action onSuccess, CancellationToken cancellationToken = default)
    {
        // Stryker disable all : Fast path optimization
        if (resultTask.IsCompletedSuccessfully) return Task.FromResult(resultTask.Result.TapOnSuccess(onSuccess));
        return TapCore(resultTask, onSuccess, cancellationToken);
        // Stryker restore all
        // Stryker disable all : Excluded from coverage
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage] static async Task<Result> TapCore(Task<Result> t, Action a, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            return (await t.ConfigureAwait(false)).TapOnSuccess(a);
        }
        // Stryker restore all
    }

    public static Task<Result> TapOnSuccess<TState>(
        this Task<Result> resultTask, TState state, Action<TState> onSuccess, CancellationToken cancellationToken = default)
    {
        // Stryker disable all : Fast path optimization
        if (resultTask.IsCompletedSuccessfully) return Task.FromResult(resultTask.Result.TapOnSuccess(state, onSuccess));
        return TapCore(resultTask, state, onSuccess, cancellationToken);
        // Stryker restore all
        // Stryker disable all : Excluded from coverage
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage] static async Task<Result> TapCore(Task<Result> t, TState s, Action<TState> a, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            return (await t.ConfigureAwait(false)).TapOnSuccess(s, a);
        }
        // Stryker restore all
    }

    public static Task<Result> TapOnSuccess(
        this Task<Result> resultTask, Func<Task> onSuccess, CancellationToken cancellationToken = default)
    {
        // Stryker disable all : Fast path optimization
        if (!resultTask.IsCompletedSuccessfully)
            return TapFullAsync(resultTask, onSuccess, cancellationToken);
        // Stryker restore all

        var r = resultTask.Result;
        return r.IsFailure ? Task.FromResult(r) : TapAsyncCore(r, onSuccess(), cancellationToken);
        // Stryker disable all : Excluded from coverage
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage] static async Task<Result> TapAsyncCore(Result r, Task actionTask, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            await actionTask.ConfigureAwait(false);
            return r;
        }
        // Stryker restore all
        // Stryker disable all : Excluded from coverage
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage] static async Task<Result> TapFullAsync(Task<Result> t, Func<Task> a, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            var result = await t.ConfigureAwait(false);
            if (result.IsSuccess) await a().ConfigureAwait(false);
            return result;
        }
        // Stryker restore all
    }

    public static Task<Result> TapOnFailure(
        this Task<Result> resultTask, Action<Error> onFailure, CancellationToken cancellationToken = default)
    {
        // Stryker disable all : Fast path optimization
        if (resultTask.IsCompletedSuccessfully) return Task.FromResult(resultTask.Result.TapOnFailure(onFailure));
        return TapOnFailureCore(resultTask, onFailure, cancellationToken);
        // Stryker restore all
        // Stryker disable all : Excluded from coverage
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage] static async Task<Result> TapOnFailureCore(Task<Result> t, Action<Error> a, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            return (await t.ConfigureAwait(false)).TapOnFailure(a);
        }
        // Stryker restore all
    }


    public static Task<Result> TapOnFailure<TState>(
        this Task<Result> resultTask, TState state, Action<TState, Error> onFailure, CancellationToken cancellationToken = default)
    {
        // Stryker disable all : Fast path optimization
        if (resultTask.IsCompletedSuccessfully) return Task.FromResult(resultTask.Result.TapOnFailure(state, onFailure));
        return TapOnFailureCore(resultTask, state, onFailure, cancellationToken);
        // Stryker restore all
        // Stryker disable all : Excluded from coverage
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage] static async Task<Result> TapOnFailureCore(Task<Result> t, TState s, Action<TState, Error> a, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            return (await t.ConfigureAwait(false)).TapOnFailure(s, a);
        }
        // Stryker restore all
    }


    public static Task<Result> TapOnFailure(
        this Task<Result> resultTask, Func<Error, Task> onFailure, CancellationToken cancellationToken = default)
    {
        // Stryker disable all : Fast path optimization
        if (!resultTask.IsCompletedSuccessfully)
            return TapOnFailureFullAsync(resultTask, onFailure, cancellationToken);
        // Stryker restore all

        var r = resultTask.Result;
        return r.IsSuccess ? Task.FromResult(r) : TapOnFailureAsyncCore(r, onFailure(r.Error), cancellationToken);
        // Stryker disable all : Excluded from coverage
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage] static async Task<Result> TapOnFailureAsyncCore(Result r, Task actionTask, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            await actionTask.ConfigureAwait(false);
            return r;
        }
        // Stryker restore all
        // Stryker disable all : Excluded from coverage
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage] static async Task<Result> TapOnFailureFullAsync(Task<Result> t, Func<Error, Task> a, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            var result = await t.ConfigureAwait(false);
            if (result.IsFailure) await a(result.Error).ConfigureAwait(false);
            return result;
        }
        // Stryker restore all
    }



    public static Task<Result> Ensure(
        this Task<Result> resultTask, Func<bool> predicate, Error error, CancellationToken cancellationToken = default)
    {
        // Stryker disable all : Fast path optimization
        if (resultTask.IsCompletedSuccessfully) return Task.FromResult(resultTask.Result.Ensure(predicate, error));
        return EnsureCore(resultTask, predicate, error, cancellationToken);
        // Stryker restore all
        // Stryker disable all : Excluded from coverage
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage] static async Task<Result> EnsureCore(Task<Result> t, Func<bool> p, Error e, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            return (await t.ConfigureAwait(false)).Ensure(p, e);
        }
        // Stryker restore all
    }

    public static Task<Result> Ensure<TState>(
        this Task<Result> resultTask, TState state, Func<TState, bool> predicate, Error error, CancellationToken cancellationToken = default)
    {
        // Stryker disable all : Fast path optimization
        if (resultTask.IsCompletedSuccessfully) return Task.FromResult(resultTask.Result.Ensure(state, predicate, error));
        return EnsureCore(resultTask, state, predicate, error, cancellationToken);
        // Stryker restore all
        // Stryker disable all : Excluded from coverage
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage] static async Task<Result> EnsureCore(Task<Result> t, TState s, Func<TState, bool> p, Error e, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            return (await t.ConfigureAwait(false)).Ensure(s, p, e);
        }
        // Stryker restore all
    }

    public static Task<Result> MapError(
        this Task<Result> resultTask, Func<Error, Error> mapper, CancellationToken cancellationToken = default)
    {
        // Stryker disable all : Fast path optimization
        if (resultTask.IsCompletedSuccessfully) return Task.FromResult(resultTask.Result.MapError(mapper));
        return MapErrorCore(resultTask, mapper, cancellationToken);
        // Stryker restore all
        // Stryker disable all : Excluded from coverage
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage] static async Task<Result> MapErrorCore(Task<Result> t, Func<Error, Error> m, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            return (await t.ConfigureAwait(false)).MapError(m);
        }
        // Stryker restore all
    }

    public static Task<Result> MapError<TState>(
        this Task<Result> resultTask, TState state, Func<TState, Error, Error> mapper, CancellationToken cancellationToken = default)
    {
        // Stryker disable all : Fast path optimization
        if (resultTask.IsCompletedSuccessfully) return Task.FromResult(resultTask.Result.MapError(state, mapper));
        return MapErrorCore(resultTask, state, mapper, cancellationToken);
        // Stryker restore all
        // Stryker disable all : Excluded from coverage
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage] static async Task<Result> MapErrorCore(Task<Result> t, TState s, Func<TState, Error, Error> m, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            return (await t.ConfigureAwait(false)).MapError(s, m);
        }
        // Stryker restore all
    }

    public static Task<Result> Inspect(
        this Task<Result> resultTask, Action<Result> action, CancellationToken cancellationToken = default)
    {
        // Stryker disable all : Fast path optimization
        if (resultTask.IsCompletedSuccessfully) return Task.FromResult(resultTask.Result.Inspect(action));
        return InspectCore(resultTask, action, cancellationToken);
        // Stryker restore all
        // Stryker disable all : Excluded from coverage
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage] static async Task<Result> InspectCore(Task<Result> t, Action<Result> a, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            return (await t.ConfigureAwait(false)).Inspect(a);
        }
        // Stryker restore all
    }

    public static Task<Result> Inspect<TState>(
        this Task<Result> resultTask, TState state, Action<TState, Result> action, CancellationToken cancellationToken = default)
    {
        // Stryker disable all : Fast path optimization
        if (resultTask.IsCompletedSuccessfully) return Task.FromResult(resultTask.Result.Inspect(state, action));
        return InspectCore(resultTask, state, action, cancellationToken);
        // Stryker restore all
        // Stryker disable all : Excluded from coverage
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage] static async Task<Result> InspectCore(Task<Result> t, TState s, Action<TState, Result> a, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            return (await t.ConfigureAwait(false)).Inspect(s, a);
        }
        // Stryker restore all
    }

    [Obsolete("Use Inspect instead to clarify that the pipeline continues after execution.", error: true)]
    public static Task<Result> Finally(
        this Task<Result> resultTask, Action<Result> action, CancellationToken cancellationToken = default)
        => resultTask.Inspect(action, cancellationToken);

    // --- Map (Task<Result> non-generic) --------------------------------------

    /// <summary>
    /// Projects a successful <see cref="Result"/> into a <see cref="Result{TNext}"/> by executing
    /// <paramref name="mapper"/> on success. If the Result is a failure, returns a failure without
    /// invoking <paramref name="mapper"/>.
    /// </summary>
    public static Task<Result<TNext>> Map<TNext>(
        this Task<Result> resultTask, Func<TNext> mapper, CancellationToken cancellationToken = default)
    {
        // Stryker disable all : Fast path optimization
        if (resultTask.IsCompletedSuccessfully)
            return Task.FromResult(resultTask.Result.Map(mapper));
        // Stryker restore all
        return MapCore(resultTask, mapper, cancellationToken);
        // Stryker disable all : Excluded from coverage
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage] static async Task<Result<TNext>> MapCore(Task<Result> t, Func<TNext> m, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            return (await t.ConfigureAwait(false)).Map(m);
        }
        // Stryker restore all
    }

    /// <summary>
    /// Projects a successful <see cref="Result"/> into a <see cref="Result{TNext}"/> using a state argument
    /// to avoid closure allocations.
    /// </summary>
    public static Task<Result<TNext>> Map<TState, TNext>(
        this Task<Result> resultTask, TState state, Func<TState, TNext> mapper, CancellationToken cancellationToken = default)
    {
        // Stryker disable all : Fast path optimization
        if (resultTask.IsCompletedSuccessfully)
            return Task.FromResult(resultTask.Result.Map(state, mapper));
        // Stryker restore all
        return MapStateCore(resultTask, state, mapper, cancellationToken);
        // Stryker disable all : Excluded from coverage
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage] static async Task<Result<TNext>> MapStateCore(Task<Result> t, TState s, Func<TState, TNext> m, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            return (await t.ConfigureAwait(false)).Map(s, m);
        }
        // Stryker restore all
    }

    // --- Recover (Task<Result> non-generic) ----------------------------------

    /// <summary>
    /// If the <see cref="Result"/> is a failure, invokes <paramref name="recovery"/> to attempt
    /// a corrective result. If the Result is a success, returns it unchanged.
    /// </summary>
    public static Task<Result> Recover(
        this Task<Result> resultTask, Func<Error, Result> recovery, CancellationToken cancellationToken = default)
    {
        // Stryker disable all : Fast path optimization
        if (!resultTask.IsCompletedSuccessfully)
            return RecoverCore(resultTask, recovery, cancellationToken);
        // Stryker restore all
        return Task.FromResult(resultTask.Result.Recover(recovery));
        // Stryker disable all : Excluded from coverage
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage] static async Task<Result> RecoverCore(Task<Result> t, Func<Error, Result> r, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            return (await t.ConfigureAwait(false)).Recover(r);
        }
        // Stryker restore all
    }

    /// <summary>
    /// If the <see cref="Result"/> is a failure, invokes <paramref name="recovery"/> with captured
    /// state to attempt a corrective result.
    /// </summary>
    public static Task<Result> Recover<TState>(
        this Task<Result> resultTask, TState state, Func<TState, Error, Result> recovery, CancellationToken cancellationToken = default)
    {
        // Stryker disable all : Fast path optimization
        if (!resultTask.IsCompletedSuccessfully)
            return RecoverCore(resultTask, state, recovery, cancellationToken);
        // Stryker restore all
        return Task.FromResult(resultTask.Result.Recover(state, recovery));
        // Stryker disable all : Excluded from coverage
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage] static async Task<Result> RecoverCore(Task<Result> t, TState s, Func<TState, Error, Result> r, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            return (await t.ConfigureAwait(false)).Recover(s, r);
        }
        // Stryker restore all
    }

    /// <summary>
    /// If the <see cref="Result"/> is a failure, invokes the async <paramref name="recovery"/> delegate
    /// to attempt a corrective result.
    /// </summary>
    public static Task<Result> Recover(
        this Task<Result> resultTask, Func<Error, Task<Result>> recovery, CancellationToken cancellationToken = default)
    {
        // Stryker disable all : Fast path optimization
        if (resultTask.IsCompletedSuccessfully)
        {
            var r = resultTask.Result;
            if (r.IsSuccess) return Task.FromResult(r);
            return recovery(r.Error);
        }
        // Stryker restore all
        return RecoverAsyncCore(resultTask, recovery, cancellationToken);
        // Stryker disable all : Excluded from coverage
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage] static async Task<Result> RecoverAsyncCore(Task<Result> t, Func<Error, Task<Result>> r, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            var result = await t.ConfigureAwait(false);
            if (result.IsSuccess) return result;
            return await r(result.Error).ConfigureAwait(false);
        }
        // Stryker restore all
    }

    // --- MapFailure (Task<Result> non-generic) --------------------------------

    /// <summary>
    /// Maps a failure error into <typeparamref name="TOut"/> using <paramref name="onFailure"/>,
    /// or returns <paramref name="successDefault"/> when the result is successful.
    /// Async overload for <c>Task&lt;Result&gt;</c> pipelines.
    /// </summary>
    public static Task<TOut> MapFailure<TOut>(
        this Task<Result> resultTask, Func<Error, TOut> onFailure, TOut successDefault, CancellationToken cancellationToken = default)
    {
        // Stryker disable all : Fast path optimization
        if (resultTask.IsCompletedSuccessfully)
            return Task.FromResult(resultTask.Result.MapFailure(onFailure, successDefault));
        // Stryker restore all
        return MapFailureCore(resultTask, onFailure, successDefault, cancellationToken);
        // Stryker disable all : Excluded from coverage
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage] static async Task<TOut> MapFailureCore(Task<Result> t, Func<Error, TOut> f, TOut def, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            return (await t.ConfigureAwait(false)).MapFailure(f, def);
        }
        // Stryker restore all
    }


    /// <summary>
    /// Maps a failure error into <typeparamref name="TOut"/> using captured state to avoid closure allocations.
    /// Async overload for <c>Task&lt;Result&gt;</c> pipelines.
    /// </summary>
    public static Task<TOut> MapFailure<TState, TOut>(
        this Task<Result> resultTask, TState state, Func<TState, Error, TOut> onFailure, TOut successDefault, CancellationToken cancellationToken = default)
    {
        // Stryker disable all : Fast path optimization
        if (resultTask.IsCompletedSuccessfully)
            return Task.FromResult(resultTask.Result.MapFailure(state, onFailure, successDefault));
        // Stryker restore all
        return MapFailureCore(resultTask, state, onFailure, successDefault, cancellationToken);
        // Stryker disable all : Excluded from coverage
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage] static async Task<TOut> MapFailureCore(Task<Result> t, TState s, Func<TState, Error, TOut> f, TOut def, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            return (await t.ConfigureAwait(false)).MapFailure(s, f, def);
        }
        // Stryker restore all
    }


    // --- MapFailure (Task<Result<T>>) -----------------------------------------

    /// <summary>
    /// Maps a failure error into <typeparamref name="TOut"/> using <paramref name="onFailure"/>,
    /// or returns <paramref name="successDefault"/> when the result is successful.
    /// Async overload for <c>Task&lt;Result&lt;T&gt;&gt;</c> pipelines.
    /// </summary>
    public static Task<TOut> MapFailure<T, TOut>(
        this Task<Result<T>> resultTask, Func<Error, TOut> onFailure, TOut successDefault, CancellationToken cancellationToken = default)
    {
        // Stryker disable all : Fast path optimization
        if (resultTask.IsCompletedSuccessfully)
            return Task.FromResult(resultTask.Result.MapFailure(onFailure, successDefault));
        // Stryker restore all
        return MapFailureCore(resultTask, onFailure, successDefault, cancellationToken);
        // Stryker disable all : Excluded from coverage
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage] static async Task<TOut> MapFailureCore(Task<Result<T>> t, Func<Error, TOut> f, TOut def, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            return (await t.ConfigureAwait(false)).MapFailure(f, def);
        }
        // Stryker restore all
    }


    /// <summary>
    /// Maps a failure error into <typeparamref name="TOut"/> using captured state to avoid closure allocations.
    /// Async overload for <c>Task&lt;Result&lt;T&gt;&gt;</c> pipelines.
    /// </summary>
    public static Task<TOut> MapFailure<TState, T, TOut>(
        this Task<Result<T>> resultTask, TState state, Func<TState, Error, TOut> onFailure, TOut successDefault, CancellationToken cancellationToken = default)
    {
        // Stryker disable all : Fast path optimization
        if (resultTask.IsCompletedSuccessfully)
            return Task.FromResult(resultTask.Result.MapFailure(state, onFailure, successDefault));
        // Stryker restore all
        return MapFailureCore(resultTask, state, onFailure, successDefault, cancellationToken);
        // Stryker disable all : Excluded from coverage
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage] static async Task<TOut> MapFailureCore(Task<Result<T>> t, TState s, Func<TState, Error, TOut> f, TOut def, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            return (await t.ConfigureAwait(false)).MapFailure(s, f, def);
        }
        // Stryker restore all
    }


}
