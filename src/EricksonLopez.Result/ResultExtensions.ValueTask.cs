using System;
using System.Threading;
using System.Threading.Tasks;

namespace EricksonLopez.Result;

/// <summary>
/// Async extension methods for composing <see cref="Result"/> and <see cref="Result{T}"/>
/// pipelines over <see cref="ValueTask"/>.
/// </summary>
public static partial class ResultExtensions
{
    // --------------------------------------------------------------------------
    //  ValueTask<Result<T>> extensions
    // --------------------------------------------------------------------------

    public static ValueTask<Result<TNext>> Map<T, TNext>(
        this ValueTask<Result<T>> resultTask, Func<T, TNext> mapper, CancellationToken cancellationToken = default)
    {
        // Stryker disable all : Fast path optimization
        if (resultTask.IsCompletedSuccessfully) return new ValueTask<Result<TNext>>(resultTask.Result.Map(mapper));
        return MapCore(resultTask, mapper, cancellationToken);
        // Stryker restore all
        // Stryker disable all : Excluded from coverage
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage] static async ValueTask<Result<TNext>> MapCore(ValueTask<Result<T>> t, Func<T, TNext> m, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            return (await t.ConfigureAwait(false)).Map(m);
        }
        // Stryker restore all
    }

    public static ValueTask<Result<TNext>> Map<TState, T, TNext>(
        this ValueTask<Result<T>> resultTask, TState state, Func<TState, T, TNext> mapper, CancellationToken cancellationToken = default)
    {
        // Stryker disable all : Fast path optimization
        if (resultTask.IsCompletedSuccessfully) return new ValueTask<Result<TNext>>(resultTask.Result.Map(state, mapper));
        return MapCore(resultTask, state, mapper, cancellationToken);
        // Stryker restore all
        // Stryker disable all : Excluded from coverage
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage] static async ValueTask<Result<TNext>> MapCore(ValueTask<Result<T>> t, TState s, Func<TState, T, TNext> m, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            return (await t.ConfigureAwait(false)).Map(s, m);
        }
        // Stryker restore all
    }

    public static ValueTask<Result<TNext>> Map<T, TNext>(
        this ValueTask<Result<T>> resultTask, Func<T, ValueTask<TNext>> mapper, CancellationToken cancellationToken = default)
    {
        // Stryker disable all : Fast path optimization
        if (resultTask.IsCompletedSuccessfully)
        {
            var result = resultTask.Result;
            if (result.IsFailure) return new ValueTask<Result<TNext>>(Result.Failure<TNext>(result.Error));
            return AwaitMapValue(mapper(result.Value));
        }
        // Stryker restore all
        return MapCore(resultTask, mapper, cancellationToken);

        // Stryker disable all : Excluded from coverage
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage] static async ValueTask<Result<TNext>> AwaitMapValue(ValueTask<TNext> v) => Result.Success(await v.ConfigureAwait(false));
        // Stryker restore all
        // Stryker disable all : Excluded from coverage
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage] static async ValueTask<Result<TNext>> MapCore(ValueTask<Result<T>> t, Func<T, ValueTask<TNext>> m, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            var r = await t.ConfigureAwait(false);
            if (r.IsFailure) return Result.Failure<TNext>(r.Error);
            var next = await m(r.Value).ConfigureAwait(false);
            return Result.Success(next);
        }
        // Stryker restore all
    }

    /// <summary>
    /// Maps the success value of a <see cref="ValueTask{TResult}"/> wrapping a <see cref="Result{T}"/> using
    /// an async mapper with captured <typeparamref name="TState"/> to avoid closure allocations.
    /// </summary>
    public static ValueTask<Result<TNext>> Map<TState, T, TNext>(
        this ValueTask<Result<T>> resultTask, TState state, Func<TState, T, ValueTask<TNext>> mapper, CancellationToken cancellationToken = default)
    {
        // Stryker disable all : Fast path optimization
        if (resultTask.IsCompletedSuccessfully)
        {
            var result = resultTask.Result;
            if (result.IsFailure) return new ValueTask<Result<TNext>>(Result.Failure<TNext>(result.Error));
            return AwaitMapStateValue(state, result.Value, mapper);
        }
        // Stryker restore all
        return MapStateCore(resultTask, state, mapper, cancellationToken);

        // Stryker disable all : Excluded from coverage
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage] static async ValueTask<Result<TNext>> AwaitMapStateValue(TState s, T value, Func<TState, T, ValueTask<TNext>> m)
            => Result.Success(await m(s, value).ConfigureAwait(false));
        // Stryker disable once all : Excluded from coverage
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage] static async ValueTask<Result<TNext>> MapStateCore(ValueTask<Result<T>> t, TState s, Func<TState, T, ValueTask<TNext>> m, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            var r = await t.ConfigureAwait(false);
            if (r.IsFailure) return Result.Failure<TNext>(r.Error);
            return Result.Success(await m(s, r.Value).ConfigureAwait(false));
        }
        // Stryker restore all
    }

    public static ValueTask<Result<TNext>> Bind<T, TNext>(
        this ValueTask<Result<T>> resultTask, Func<T, Result<TNext>> bind, CancellationToken cancellationToken = default)
    {
        // Stryker disable all : Fast path optimization
        if (resultTask.IsCompletedSuccessfully) return new ValueTask<Result<TNext>>(resultTask.Result.Bind(bind));
        return BindCore(resultTask, bind, cancellationToken);
        // Stryker restore all
        // Stryker disable all : Excluded from coverage
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage] static async ValueTask<Result<TNext>> BindCore(ValueTask<Result<T>> t, Func<T, Result<TNext>> b, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            return (await t.ConfigureAwait(false)).Bind(b);
        }
        // Stryker restore all
    }

    public static ValueTask<Result<TNext>> Bind<TState, T, TNext>(
        this ValueTask<Result<T>> resultTask, TState state, Func<TState, T, Result<TNext>> bind, CancellationToken cancellationToken = default)
    {
        // Stryker disable all : Fast path optimization
        if (resultTask.IsCompletedSuccessfully) return new ValueTask<Result<TNext>>(resultTask.Result.Bind(state, bind));
        return BindCore(resultTask, state, bind, cancellationToken);
        // Stryker restore all
        // Stryker disable all : Excluded from coverage
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage] static async ValueTask<Result<TNext>> BindCore(ValueTask<Result<T>> t, TState s, Func<TState, T, Result<TNext>> b, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            return (await t.ConfigureAwait(false)).Bind(s, b);
        }
        // Stryker restore all
    }

    public static ValueTask<Result<TNext>> Bind<T, TNext>(
        this ValueTask<Result<T>> resultTask, Func<T, ValueTask<Result<TNext>>> bind, CancellationToken cancellationToken = default)
    {
        // Stryker disable all : Fast path optimization
        if (resultTask.IsCompletedSuccessfully)
        {
            var result = resultTask.Result;
            if (result.IsFailure) return new ValueTask<Result<TNext>>(Result.Failure<TNext>(result.Error));
            return bind(result.Value);
        }
        // Stryker restore all
        return BindCore(resultTask, bind, cancellationToken);

        // Stryker disable all : Excluded from coverage
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage] static async ValueTask<Result<TNext>> BindCore(ValueTask<Result<T>> t, Func<T, ValueTask<Result<TNext>>> b, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            var r = await t.ConfigureAwait(false);
            if (r.IsFailure) return Result.Failure<TNext>(r.Error);
            return await b(r.Value).ConfigureAwait(false);
        }
        // Stryker restore all
    }

    public static ValueTask<Result> Bind<T>(
        this ValueTask<Result<T>> resultTask, Func<T, ValueTask<Result>> bind, CancellationToken cancellationToken = default)
    {
        // Stryker disable all : Fast path optimization
        if (resultTask.IsCompletedSuccessfully)
        {
            var r = resultTask.Result;
            if (r.IsFailure) return new ValueTask<Result>(Result.Failure(r.Error));
            return bind(r.Value);
        }
        // Stryker restore all
        return BindAsyncCore(resultTask, bind, cancellationToken);
        // Stryker disable all : Excluded from coverage
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage] static async ValueTask<Result> BindAsyncCore(ValueTask<Result<T>> t, Func<T, ValueTask<Result>> b, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            var r = await t.ConfigureAwait(false);
            if (r.IsFailure) return Result.Failure(r.Error);
            return await b(r.Value).ConfigureAwait(false);
        }
        // Stryker restore all
    }

    public static ValueTask<Result> Bind<T>(
        this ValueTask<Result<T>> resultTask, Func<T, Result> bind, CancellationToken cancellationToken = default)
    {
        // Stryker disable all : Fast path optimization
        if (resultTask.IsCompletedSuccessfully) return new ValueTask<Result>(resultTask.Result.Bind(bind));
        return BindCore(resultTask, bind, cancellationToken);
        // Stryker restore all
        // Stryker disable all : Excluded from coverage
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage] static async ValueTask<Result> BindCore(ValueTask<Result<T>> t, Func<T, Result> b, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            return (await t.ConfigureAwait(false)).Bind(b);
        }
        // Stryker restore all
    }

    public static ValueTask<Result> Bind<TState, T>(
        this ValueTask<Result<T>> resultTask, TState state, Func<TState, T, Result> bind, CancellationToken cancellationToken = default)
    {
        // Stryker disable all : Fast path optimization
        if (resultTask.IsCompletedSuccessfully) return new ValueTask<Result>(resultTask.Result.Bind(state, bind));
        return BindCore(resultTask, state, bind, cancellationToken);
        // Stryker restore all
        // Stryker disable all : Excluded from coverage
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage] static async ValueTask<Result> BindCore(ValueTask<Result<T>> t, TState s, Func<TState, T, Result> b, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            return (await t.ConfigureAwait(false)).Bind(s, b);
        }
        // Stryker restore all
    }

    public static ValueTask<TOut> Match<T, TOut>(
        this ValueTask<Result<T>> resultTask,
        Func<T, TOut> onSuccess, Func<Error, TOut> onFailure)
    {
        // Stryker disable all : Fast path optimization
        if (resultTask.IsCompletedSuccessfully) return new ValueTask<TOut>(resultTask.Result.Match(onSuccess, onFailure));
        return MatchCore(resultTask, onSuccess, onFailure);
        // Stryker restore all
        // Stryker disable all : Excluded from coverage
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage] static async ValueTask<TOut> MatchCore(ValueTask<Result<T>> t, Func<T, TOut> s, Func<Error, TOut> f)
            => (await t.ConfigureAwait(false)).Match(s, f);
    }

    public static ValueTask<TOut> Match<TState, T, TOut>(
        this ValueTask<Result<T>> resultTask, TState state,
        Func<TState, T, TOut> onSuccess, Func<TState, Error, TOut> onFailure)
    {
        // Stryker restore all
        // Stryker disable all : Fast path optimization
        if (resultTask.IsCompletedSuccessfully) return new ValueTask<TOut>(resultTask.Result.Match(state, onSuccess, onFailure));
        return MatchCore(resultTask, state, onSuccess, onFailure);
        // Stryker restore all
        // Stryker disable all : Excluded from coverage
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage] static async ValueTask<TOut> MatchCore(ValueTask<Result<T>> t, TState s, Func<TState, T, TOut> onSucc, Func<TState, Error, TOut> onFail)
            => (await t.ConfigureAwait(false)).Match(s, onSucc, onFail);
    }

    public static ValueTask Execute<T>(
        this ValueTask<Result<T>> resultTask,
        Action<T> onSuccess, Action<Error> onFailure, CancellationToken cancellationToken = default)
    {
        // Stryker restore all
        // Stryker disable all : Fast path optimization
        if (resultTask.IsCompletedSuccessfully)
        {
            resultTask.Result.Execute(onSuccess, onFailure);
            return default;
        }
        // Stryker restore all
        return ExecuteCore(resultTask, onSuccess, onFailure, cancellationToken);
        // Stryker disable all : Excluded from coverage
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage] static async ValueTask ExecuteCore(ValueTask<Result<T>> t, Action<T> s, Action<Error> f, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            (await t.ConfigureAwait(false)).Execute(s, f);
        }
        // Stryker restore all
    }

    public static ValueTask Execute<TState, T>(
        this ValueTask<Result<T>> resultTask, TState state,
        Action<TState, T> onSuccess, Action<TState, Error> onFailure, CancellationToken cancellationToken = default)
    {
        // Stryker disable all : Fast path optimization
        if (resultTask.IsCompletedSuccessfully)
        {
            resultTask.Result.Execute(state, onSuccess, onFailure);
            return default;
        }
        // Stryker restore all
        return ExecuteCore(resultTask, state, onSuccess, onFailure, cancellationToken);
        // Stryker disable all : Excluded from coverage
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage] static async ValueTask ExecuteCore(ValueTask<Result<T>> t, TState st, Action<TState, T> s, Action<TState, Error> f, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            (await t.ConfigureAwait(false)).Execute(st, s, f);
        }
        // Stryker restore all
    }

    public static ValueTask<Result<T>> TapOnSuccess<T>(
        this ValueTask<Result<T>> resultTask, Action<T> action, CancellationToken cancellationToken = default)
    {
        // Stryker disable all : Fast path optimization
        if (resultTask.IsCompletedSuccessfully) return new ValueTask<Result<T>>(resultTask.Result.TapOnSuccess(action));
        return TapCore(resultTask, action, cancellationToken);
        // Stryker restore all
        // Stryker disable all : Excluded from coverage
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage] static async ValueTask<Result<T>> TapCore(ValueTask<Result<T>> t, Action<T> a, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            return (await t.ConfigureAwait(false)).TapOnSuccess(a);
        }
        // Stryker restore all
    }

    public static ValueTask<Result<T>> TapOnSuccess<TState, T>(
        this ValueTask<Result<T>> resultTask, TState state, Action<TState, T> action, CancellationToken cancellationToken = default)
    {
        // Stryker disable all : Fast path optimization
        if (resultTask.IsCompletedSuccessfully) return new ValueTask<Result<T>>(resultTask.Result.TapOnSuccess(state, action));
        return TapCore(resultTask, state, action, cancellationToken);
        // Stryker restore all
        // Stryker disable all : Excluded from coverage
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage] static async ValueTask<Result<T>> TapCore(ValueTask<Result<T>> t, TState s, Action<TState, T> a, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            return (await t.ConfigureAwait(false)).TapOnSuccess(s, a);
        }
        // Stryker restore all
    }

    public static ValueTask<Result<T>> TapOnSuccess<T>(
        this ValueTask<Result<T>> resultTask, Func<T, ValueTask> action, CancellationToken cancellationToken = default)
    {
        // Stryker disable all : Fast path optimization
        if (resultTask.IsCompletedSuccessfully)
        {
            var result = resultTask.Result;
            if (result.IsFailure) return new ValueTask<Result<T>>(result);
            return AwaitTapValue(action(result.Value), result);
        }
        // Stryker restore all
        return TapCore(resultTask, action, cancellationToken);

        // Stryker disable all : Excluded from coverage
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage] static async ValueTask<Result<T>> AwaitTapValue(ValueTask v, Result<T> r)
        {
            await v.ConfigureAwait(false);
            return r;
        }
        // Stryker restore all

        // Stryker disable all : Excluded from coverage
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage] static async ValueTask<Result<T>> TapCore(ValueTask<Result<T>> t, Func<T, ValueTask> a, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            var r = await t.ConfigureAwait(false);
            if (r.IsSuccess) await a(r.Value).ConfigureAwait(false);
            return r;
        }
        // Stryker restore all
    }

    public static ValueTask<Result<T>> TapOnFailure<T>(
        this ValueTask<Result<T>> resultTask, Action<Error> action, CancellationToken cancellationToken = default)
    {
        // Stryker disable all : Fast path optimization
        if (resultTask.IsCompletedSuccessfully) return new ValueTask<Result<T>>(resultTask.Result.TapOnFailure(action));
        return TapOnFailureCore(resultTask, action, cancellationToken);
        // Stryker restore all
        // Stryker disable all : Excluded from coverage
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage] static async ValueTask<Result<T>> TapOnFailureCore(ValueTask<Result<T>> t, Action<Error> a, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            return (await t.ConfigureAwait(false)).TapOnFailure(a);
        }
        // Stryker restore all
    }

    public static ValueTask<Result<T>> TapOnFailure<TState, T>(
        this ValueTask<Result<T>> resultTask, TState state, Action<TState, Error> action, CancellationToken cancellationToken = default)
    {
        // Stryker disable all : Fast path optimization
        if (resultTask.IsCompletedSuccessfully) return new ValueTask<Result<T>>(resultTask.Result.TapOnFailure(state, action));
        return TapOnFailureCore(resultTask, state, action, cancellationToken);
        // Stryker restore all
        // Stryker disable all : Excluded from coverage
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage] static async ValueTask<Result<T>> TapOnFailureCore(ValueTask<Result<T>> t, TState s, Action<TState, Error> a, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            return (await t.ConfigureAwait(false)).TapOnFailure(s, a);
        }
        // Stryker restore all
    }

    public static ValueTask<Result<T>> TapOnFailure<T>(
        this ValueTask<Result<T>> resultTask, Func<Error, ValueTask> action, CancellationToken cancellationToken = default)
    {
        // Stryker disable all : Fast path optimization
        if (resultTask.IsCompletedSuccessfully)
        {
            var result = resultTask.Result;
            if (result.IsSuccess) return new ValueTask<Result<T>>(result);
            return AwaitTapOnFailureValue(action(result.Error), result);
        }
        // Stryker restore all
        return TapOnFailureCore(resultTask, action, cancellationToken);

        // Stryker disable all : Excluded from coverage
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage] static async ValueTask<Result<T>> AwaitTapOnFailureValue(ValueTask v, Result<T> r)
        {
            await v.ConfigureAwait(false);
            return r;
        }
        // Stryker restore all

        // Stryker disable all : Excluded from coverage
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage] static async ValueTask<Result<T>> TapOnFailureCore(ValueTask<Result<T>> t, Func<Error, ValueTask> a, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            var r = await t.ConfigureAwait(false);
            if (r.IsFailure) await a(r.Error).ConfigureAwait(false);
            return r;
        }
        // Stryker restore all
    }

    public static ValueTask<Result<T>> Ensure<T>(
        this ValueTask<Result<T>> resultTask, Func<T, bool> predicate, Error error, CancellationToken cancellationToken = default)
    {
        // Stryker disable all : Fast path optimization
        if (resultTask.IsCompletedSuccessfully) return new ValueTask<Result<T>>(resultTask.Result.Ensure(predicate, error));
        return EnsureCore(resultTask, predicate, error, cancellationToken);
        // Stryker restore all
        // Stryker disable all : Excluded from coverage
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage] static async ValueTask<Result<T>> EnsureCore(ValueTask<Result<T>> t, Func<T, bool> p, Error e, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            return (await t.ConfigureAwait(false)).Ensure(p, e);
        }
        // Stryker restore all
    }

    public static ValueTask<Result<T>> Ensure<TState, T>(
        this ValueTask<Result<T>> resultTask, TState state, Func<TState, T, bool> predicate, Error error, CancellationToken cancellationToken = default)
    {
        // Stryker disable all : Fast path optimization
        if (resultTask.IsCompletedSuccessfully) return new ValueTask<Result<T>>(resultTask.Result.Ensure(state, predicate, error));
        return EnsureCore(resultTask, state, predicate, error, cancellationToken);
        // Stryker restore all
        // Stryker disable all : Excluded from coverage
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage] static async ValueTask<Result<T>> EnsureCore(ValueTask<Result<T>> t, TState s, Func<TState, T, bool> p, Error e, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            return (await t.ConfigureAwait(false)).Ensure(s, p, e);
        }
        // Stryker restore all
    }

    public static ValueTask<Result<T>> Ensure<T>(
        this ValueTask<Result<T>> resultTask, Func<T, ValueTask<bool>> predicate, Error error, CancellationToken cancellationToken = default)
    {
        // Stryker disable all : Fast path optimization
        if (resultTask.IsCompletedSuccessfully)
        {
            var result = resultTask.Result;
            if (result.IsFailure) return new ValueTask<Result<T>>(result);
            return AwaitEnsureValue(predicate(result.Value), result, error);
        }
        // Stryker restore all
        return EnsureCore(resultTask, predicate, error, cancellationToken);

        // Stryker disable all : Excluded from coverage
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage] static async ValueTask<Result<T>> AwaitEnsureValue(ValueTask<bool> v, Result<T> r, Error e)
            => await v.ConfigureAwait(false) ? r : Result.Failure<T>(e);

        // Stryker disable once all : Excluded from coverage
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage] static async ValueTask<Result<T>> EnsureCore(ValueTask<Result<T>> t, Func<T, ValueTask<bool>> p, Error e, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            var r = await t.ConfigureAwait(false);
            if (r.IsFailure) return r;
            return await p(r.Value).ConfigureAwait(false) ? r : Result.Failure<T>(e);
        }
        // Stryker restore all
    }

    /// <summary>
    /// Applies an async predicate with captured <typeparamref name="TState"/> to the success value
    /// of a <see cref="ValueTask{TResult}"/> wrapping a <see cref="Result{T}"/>, avoiding closure allocations.
    /// </summary>
    public static ValueTask<Result<T>> Ensure<TState, T>(
        this ValueTask<Result<T>> resultTask, TState state, Func<TState, T, ValueTask<bool>> predicate, Error error, CancellationToken cancellationToken = default)
    {
        // Stryker disable all : Fast path optimization
        if (resultTask.IsCompletedSuccessfully)
        {
            var result = resultTask.Result;
            if (result.IsFailure) return new ValueTask<Result<T>>(result);
            return AwaitEnsureStateValue(state, result, predicate, error);
        }
        // Stryker restore all
        return EnsureStateCore(resultTask, state, predicate, error, cancellationToken);

        // Stryker disable all : Excluded from coverage
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage] static async ValueTask<Result<T>> AwaitEnsureStateValue(TState s, Result<T> r, Func<TState, T, ValueTask<bool>> p, Error e)
            => await p(s, r.Value).ConfigureAwait(false) ? r : Result.Failure<T>(e);

        // Stryker disable once all : Excluded from coverage
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage] static async ValueTask<Result<T>> EnsureStateCore(ValueTask<Result<T>> t, TState s, Func<TState, T, ValueTask<bool>> p, Error e, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            var r = await t.ConfigureAwait(false);
            if (r.IsFailure) return r;
            return await p(s, r.Value).ConfigureAwait(false) ? r : Result.Failure<T>(e);
        }
        // Stryker restore all
    }

    public static ValueTask<Result<T>> Recover<T>(
        this ValueTask<Result<T>> resultTask, Func<Error, Result<T>> recovery, CancellationToken cancellationToken = default)
    {
        // Stryker disable all : Fast path optimization
        if (resultTask.IsCompletedSuccessfully) return new ValueTask<Result<T>>(resultTask.Result.Recover(recovery));
        return RecoverCore(resultTask, recovery, cancellationToken);
        // Stryker restore all
        // Stryker disable all : Excluded from coverage
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage] static async ValueTask<Result<T>> RecoverCore(ValueTask<Result<T>> t, Func<Error, Result<T>> r, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            return (await t.ConfigureAwait(false)).Recover(r);
        }
        // Stryker restore all
    }

    public static ValueTask<Result<T>> Recover<TState, T>(
        this ValueTask<Result<T>> resultTask, TState state, Func<TState, Error, Result<T>> recovery, CancellationToken cancellationToken = default)
    {
        // Stryker disable all : Fast path optimization
        if (resultTask.IsCompletedSuccessfully) return new ValueTask<Result<T>>(resultTask.Result.Recover(state, recovery));
        return RecoverCore(resultTask, state, recovery, cancellationToken);
        // Stryker restore all
        // Stryker disable all : Excluded from coverage
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage] static async ValueTask<Result<T>> RecoverCore(ValueTask<Result<T>> t, TState s, Func<TState, Error, Result<T>> r, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            return (await t.ConfigureAwait(false)).Recover(s, r);
        }
        // Stryker restore all
    }

    public static ValueTask<Result<T>> Recover<T>(
        this ValueTask<Result<T>> resultTask, Func<Error, ValueTask<Result<T>>> recovery, CancellationToken cancellationToken = default)
    {
        // Stryker disable all : Fast path optimization
        if (resultTask.IsCompletedSuccessfully)
        {
            var result = resultTask.Result;
            if (result.IsSuccess) return new ValueTask<Result<T>>(result);
            return recovery(result.Error);
        }
        // Stryker restore all
        return RecoverCore(resultTask, recovery, cancellationToken);

        // Stryker disable all : Excluded from coverage
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage] static async ValueTask<Result<T>> RecoverCore(ValueTask<Result<T>> t, Func<Error, ValueTask<Result<T>>> r, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            var res = await t.ConfigureAwait(false);
            if (res.IsSuccess) return res;
            return await r(res.Error).ConfigureAwait(false);
        }
        // Stryker restore all
    }

    /// <summary>
    /// Attempts to recover from a failure using an async recovery function with captured
    /// <typeparamref name="TState"/>, avoiding closure allocations.
    /// </summary>
    public static ValueTask<Result<T>> Recover<TState, T>(
        this ValueTask<Result<T>> resultTask, TState state, Func<TState, Error, ValueTask<Result<T>>> recovery, CancellationToken cancellationToken = default)
    {
        // Stryker disable all : Fast path optimization
        if (resultTask.IsCompletedSuccessfully)
        {
            var result = resultTask.Result;
            if (result.IsSuccess) return new ValueTask<Result<T>>(result);
            return recovery(state, result.Error);
        }
        // Stryker restore all
        return RecoverStateCore(resultTask, state, recovery, cancellationToken);

        // Stryker disable all : Excluded from coverage
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage] static async ValueTask<Result<T>> RecoverStateCore(ValueTask<Result<T>> t, TState s, Func<TState, Error, ValueTask<Result<T>>> r, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            var res = await t.ConfigureAwait(false);
            if (res.IsSuccess) return res;
            return await r(s, res.Error).ConfigureAwait(false);
        }
        // Stryker restore all
    }

    public static ValueTask<Result<T>> MapError<T>(
        this ValueTask<Result<T>> resultTask, Func<Error, Error> mapper, CancellationToken cancellationToken = default)
    {
        // Stryker disable all : Fast path optimization
        if (resultTask.IsCompletedSuccessfully) return new ValueTask<Result<T>>(resultTask.Result.MapError(mapper));
        return MapErrorCore(resultTask, mapper, cancellationToken);
        // Stryker restore all
        // Stryker disable all : Excluded from coverage
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage] static async ValueTask<Result<T>> MapErrorCore(ValueTask<Result<T>> t, Func<Error, Error> m, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            return (await t.ConfigureAwait(false)).MapError(m);
        }
        // Stryker restore all
    }

    public static ValueTask<Result<T>> MapError<TState, T>(
        this ValueTask<Result<T>> resultTask, TState state, Func<TState, Error, Error> mapper, CancellationToken cancellationToken = default)
    {
        // Stryker disable all : Fast path optimization
        if (resultTask.IsCompletedSuccessfully) return new ValueTask<Result<T>>(resultTask.Result.MapError(state, mapper));
        return MapErrorCore(resultTask, state, mapper, cancellationToken);
        // Stryker restore all
        // Stryker disable all : Excluded from coverage
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage] static async ValueTask<Result<T>> MapErrorCore(ValueTask<Result<T>> t, TState s, Func<TState, Error, Error> m, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            return (await t.ConfigureAwait(false)).MapError(s, m);
        }
        // Stryker restore all
    }

    public static ValueTask<Result<T>> Inspect<T>(
        this ValueTask<Result<T>> resultTask, Action<Result<T>> action, CancellationToken cancellationToken = default)
    {
        // Stryker disable all : Fast path optimization
        if (resultTask.IsCompletedSuccessfully) return new ValueTask<Result<T>>(resultTask.Result.Inspect(action));
        return InspectCore(resultTask, action, cancellationToken);
        // Stryker restore all
        // Stryker disable all : Excluded from coverage
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage] static async ValueTask<Result<T>> InspectCore(ValueTask<Result<T>> t, Action<Result<T>> a, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            return (await t.ConfigureAwait(false)).Inspect(a);
        }
        // Stryker restore all
    }

    public static ValueTask<Result<T>> Inspect<TState, T>(
        this ValueTask<Result<T>> resultTask, TState state, Action<TState, Result<T>> action, CancellationToken cancellationToken = default)
    {
        // Stryker disable all : Fast path optimization
        if (resultTask.IsCompletedSuccessfully) return new ValueTask<Result<T>>(resultTask.Result.Inspect(state, action));
        return InspectCore(resultTask, state, action, cancellationToken);
        // Stryker restore all
        // Stryker disable all : Excluded from coverage
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage] static async ValueTask<Result<T>> InspectCore(ValueTask<Result<T>> t, TState s, Action<TState, Result<T>> a, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            return (await t.ConfigureAwait(false)).Inspect(s, a);
        }
        // Stryker restore all
    }

    // --------------------------------------------------------------------------
    //  ValueTask<Result> (non-generic) extensions
    // --------------------------------------------------------------------------

    public static ValueTask<Result> Bind(
        this ValueTask<Result> resultTask, Func<Result> bind, CancellationToken cancellationToken = default)
    {
        // Stryker disable all : Fast path optimization
        if (resultTask.IsCompletedSuccessfully) return new ValueTask<Result>(resultTask.Result.Bind(bind));
        return BindCore(resultTask, bind, cancellationToken);
        // Stryker restore all
        // Stryker disable all : Excluded from coverage
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage] static async ValueTask<Result> BindCore(ValueTask<Result> t, Func<Result> b, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            return (await t.ConfigureAwait(false)).Bind(b);
        }
        // Stryker restore all
    }

    public static ValueTask<Result> Bind<TState>(
        this ValueTask<Result> resultTask, TState state, Func<TState, Result> bind, CancellationToken cancellationToken = default)
    {
        // Stryker disable all : Fast path optimization
        if (resultTask.IsCompletedSuccessfully) return new ValueTask<Result>(resultTask.Result.Bind(state, bind));
        return BindCore(resultTask, state, bind, cancellationToken);
        // Stryker restore all
        // Stryker disable all : Excluded from coverage
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage] static async ValueTask<Result> BindCore(ValueTask<Result> t, TState s, Func<TState, Result> b, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            return (await t.ConfigureAwait(false)).Bind(s, b);
        }
        // Stryker restore all
    }

    public static ValueTask<Result> Bind(
        this ValueTask<Result> resultTask, Func<ValueTask<Result>> bind, CancellationToken cancellationToken = default)
    {
        // Stryker disable all : Fast path optimization
        if (resultTask.IsCompletedSuccessfully)
        {
            var result = resultTask.Result;
            if (result.IsFailure) return new ValueTask<Result>(result);
            return bind();
        }
        // Stryker restore all
        return BindCore(resultTask, bind, cancellationToken);

        // Stryker disable all : Excluded from coverage
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage] static async ValueTask<Result> BindCore(ValueTask<Result> t, Func<ValueTask<Result>> b, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            var r = await t.ConfigureAwait(false);
            if (r.IsFailure) return r;
            return await b().ConfigureAwait(false);
        }
        // Stryker restore all
    }

    public static ValueTask<Result<TNext>> Bind<TNext>(
        this ValueTask<Result> resultTask, Func<Result<TNext>> bind, CancellationToken cancellationToken = default)
    {
        // Stryker disable all : Fast path optimization
        if (resultTask.IsCompletedSuccessfully) return new ValueTask<Result<TNext>>(resultTask.Result.Bind(bind));
        return BindCore(resultTask, bind, cancellationToken);
        // Stryker restore all
        // Stryker disable all : Excluded from coverage
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage] static async ValueTask<Result<TNext>> BindCore(ValueTask<Result> t, Func<Result<TNext>> b, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            return (await t.ConfigureAwait(false)).Bind(b);
        }
        // Stryker restore all
    }

    public static ValueTask<Result<TNext>> Bind<TState, TNext>(
        this ValueTask<Result> resultTask, TState state, Func<TState, Result<TNext>> bind, CancellationToken cancellationToken = default)
    {
        // Stryker disable all : Fast path optimization
        if (resultTask.IsCompletedSuccessfully) return new ValueTask<Result<TNext>>(resultTask.Result.Bind(state, bind));
        return BindCore(resultTask, state, bind, cancellationToken);
        // Stryker restore all
        // Stryker disable all : Excluded from coverage
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage] static async ValueTask<Result<TNext>> BindCore(ValueTask<Result> t, TState s, Func<TState, Result<TNext>> b, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            return (await t.ConfigureAwait(false)).Bind(s, b);
        }
        // Stryker restore all
    }

    public static ValueTask<Result<TNext>> Bind<TNext>(
        this ValueTask<Result> resultTask, Func<ValueTask<Result<TNext>>> bind, CancellationToken cancellationToken = default)
    {
        // Stryker disable all : Fast path optimization
        if (resultTask.IsCompletedSuccessfully)
        {
            var result = resultTask.Result;
            if (result.IsFailure) return new ValueTask<Result<TNext>>(Result.Failure<TNext>(result.Error));
            return bind();
        }
        // Stryker restore all
        return BindCore(resultTask, bind, cancellationToken);

        // Stryker disable all : Excluded from coverage
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage] static async ValueTask<Result<TNext>> BindCore(ValueTask<Result> t, Func<ValueTask<Result<TNext>>> b, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            var r = await t.ConfigureAwait(false);
            if (r.IsFailure) return Result.Failure<TNext>(r.Error);
            return await b().ConfigureAwait(false);
        }
        // Stryker restore all
    }

    public static ValueTask<TOut> Match<TOut>(
        this ValueTask<Result> resultTask,
        Func<TOut> onSuccess, Func<Error, TOut> onFailure)
    {
        // Stryker disable all : Fast path optimization
        if (resultTask.IsCompletedSuccessfully) return new ValueTask<TOut>(resultTask.Result.Match(onSuccess, onFailure));
        return MatchCore(resultTask, onSuccess, onFailure);
        // Stryker restore all
        // Stryker disable all : Excluded from coverage
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage] static async ValueTask<TOut> MatchCore(ValueTask<Result> t, Func<TOut> s, Func<Error, TOut> f)
            => (await t.ConfigureAwait(false)).Match(s, f);
    }

    public static ValueTask<TOut> Match<TState, TOut>(
        this ValueTask<Result> resultTask, TState state,
        Func<TState, TOut> onSuccess, Func<TState, Error, TOut> onFailure)
    {
        // Stryker restore all
        // Stryker disable all : Fast path optimization
        if (resultTask.IsCompletedSuccessfully) return new ValueTask<TOut>(resultTask.Result.Match(state, onSuccess, onFailure));
        return MatchCore(resultTask, state, onSuccess, onFailure);
        // Stryker restore all
        // Stryker disable all : Excluded from coverage
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage] static async ValueTask<TOut> MatchCore(ValueTask<Result> t, TState st, Func<TState, TOut> s, Func<TState, Error, TOut> f)
            => (await t.ConfigureAwait(false)).Match(st, s, f);
    }

    public static ValueTask Execute(
        this ValueTask<Result> resultTask,
        Action onSuccess, Action<Error> onFailure, CancellationToken cancellationToken = default)
    {
        // Stryker restore all
        // Stryker disable all : Fast path optimization
        if (resultTask.IsCompletedSuccessfully)
        {
            resultTask.Result.Execute(onSuccess, onFailure);
            return default;
        }
        // Stryker restore all
        return ExecuteCore(resultTask, onSuccess, onFailure, cancellationToken);
        // Stryker disable all : Excluded from coverage
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage] static async ValueTask ExecuteCore(ValueTask<Result> t, Action s, Action<Error> f, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            (await t.ConfigureAwait(false)).Execute(s, f);
        }
        // Stryker restore all
    }

    public static ValueTask Execute<TState>(
        this ValueTask<Result> resultTask, TState state,
        Action<TState> onSuccess, Action<TState, Error> onFailure, CancellationToken cancellationToken = default)
    {
        // Stryker disable all : Fast path optimization
        if (resultTask.IsCompletedSuccessfully)
        {
            resultTask.Result.Execute(state, onSuccess, onFailure);
            return default;
        }
        // Stryker restore all
        return ExecuteCore(resultTask, state, onSuccess, onFailure, cancellationToken);
        // Stryker disable all : Excluded from coverage
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage] static async ValueTask ExecuteCore(ValueTask<Result> t, TState st, Action<TState> s, Action<TState, Error> f, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            (await t.ConfigureAwait(false)).Execute(st, s, f);
        }
        // Stryker restore all
    }

    public static ValueTask<Result> TapOnSuccess(
        this ValueTask<Result> resultTask, Action onSuccess, CancellationToken cancellationToken = default)
    {
        // Stryker disable all : Fast path optimization
        if (resultTask.IsCompletedSuccessfully) return new ValueTask<Result>(resultTask.Result.TapOnSuccess(onSuccess));
        return TapCore(resultTask, onSuccess, cancellationToken);
        // Stryker restore all
        // Stryker disable all : Excluded from coverage
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage] static async ValueTask<Result> TapCore(ValueTask<Result> t, Action a, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            return (await t.ConfigureAwait(false)).TapOnSuccess(a);
        }
        // Stryker restore all
    }

    public static ValueTask<Result> TapOnSuccess<TState>(
        this ValueTask<Result> resultTask, TState state, Action<TState> onSuccess, CancellationToken cancellationToken = default)
    {
        // Stryker disable all : Fast path optimization
        if (resultTask.IsCompletedSuccessfully) return new ValueTask<Result>(resultTask.Result.TapOnSuccess(state, onSuccess));
        return TapCore(resultTask, state, onSuccess, cancellationToken);
        // Stryker restore all
        // Stryker disable all : Excluded from coverage
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage] static async ValueTask<Result> TapCore(ValueTask<Result> t, TState s, Action<TState> a, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            return (await t.ConfigureAwait(false)).TapOnSuccess(s, a);
        }
        // Stryker restore all
    }

    public static ValueTask<Result> TapOnSuccess(
        this ValueTask<Result> resultTask, Func<ValueTask> onSuccess, CancellationToken cancellationToken = default)
    {
        // Stryker disable all : Fast path optimization
        if (resultTask.IsCompletedSuccessfully)
        {
            var result = resultTask.Result;
            if (result.IsFailure) return new ValueTask<Result>(result);
            return AwaitTapValue(onSuccess(), result);
        }
        // Stryker restore all
        return TapCore(resultTask, onSuccess, cancellationToken);

        // Stryker disable all : Excluded from coverage
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage] static async ValueTask<Result> AwaitTapValue(ValueTask v, Result r)
        {
            await v.ConfigureAwait(false);
            return r;
        }
        // Stryker restore all

        // Stryker disable all : Excluded from coverage
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage] static async ValueTask<Result> TapCore(ValueTask<Result> t, Func<ValueTask> a, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            var r = await t.ConfigureAwait(false);
            if (r.IsSuccess) await a().ConfigureAwait(false);
            return r;
        }
        // Stryker restore all
    }

    public static ValueTask<Result> TapOnFailure(
        this ValueTask<Result> resultTask, Action<Error> onFailure, CancellationToken cancellationToken = default)
    {
        // Stryker disable all : Fast path optimization
        if (resultTask.IsCompletedSuccessfully) return new ValueTask<Result>(resultTask.Result.TapOnFailure(onFailure));
        return TapOnFailureCore(resultTask, onFailure, cancellationToken);
        // Stryker restore all
        // Stryker disable all : Excluded from coverage
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage] static async ValueTask<Result> TapOnFailureCore(ValueTask<Result> t, Action<Error> a, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            return (await t.ConfigureAwait(false)).TapOnFailure(a);
        }
        // Stryker restore all
    }


    public static ValueTask<Result> TapOnFailure<TState>(
        this ValueTask<Result> resultTask, TState state, Action<TState, Error> onFailure, CancellationToken cancellationToken = default)
    {
        // Stryker disable all : Fast path optimization
        if (resultTask.IsCompletedSuccessfully) return new ValueTask<Result>(resultTask.Result.TapOnFailure(state, onFailure));
        return TapOnFailureCore(resultTask, state, onFailure, cancellationToken);
        // Stryker restore all
        // Stryker disable all : Excluded from coverage
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage] static async ValueTask<Result> TapOnFailureCore(ValueTask<Result> t, TState s, Action<TState, Error> a, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            return (await t.ConfigureAwait(false)).TapOnFailure(s, a);
        }
        // Stryker restore all
    }


    public static ValueTask<Result> TapOnFailure(
        this ValueTask<Result> resultTask, Func<Error, ValueTask> onFailure, CancellationToken cancellationToken = default)
    {
        // Stryker disable all : Fast path optimization
        if (resultTask.IsCompletedSuccessfully)
        {
            var result = resultTask.Result;
            if (result.IsSuccess) return new ValueTask<Result>(result);
            return AwaitTapOnFailureValue(onFailure(result.Error), result);
        }
        // Stryker restore all
        return TapOnFailureCore(resultTask, onFailure, cancellationToken);

        // Stryker disable all : Excluded from coverage
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage] static async ValueTask<Result> AwaitTapOnFailureValue(ValueTask v, Result r)
        {
            await v.ConfigureAwait(false);
            return r;
        }
        // Stryker restore all

        // Stryker disable all : Excluded from coverage
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage] static async ValueTask<Result> TapOnFailureCore(ValueTask<Result> t, Func<Error, ValueTask> a, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            var r = await t.ConfigureAwait(false);
            if (r.IsFailure) await a(r.Error).ConfigureAwait(false);
            return r;
        }
        // Stryker restore all
    }


    public static ValueTask<Result> Ensure(
        this ValueTask<Result> resultTask, Func<bool> predicate, Error error, CancellationToken cancellationToken = default)
    {
        // Stryker disable all : Fast path optimization
        if (resultTask.IsCompletedSuccessfully) return new ValueTask<Result>(resultTask.Result.Ensure(predicate, error));
        return EnsureCore(resultTask, predicate, error, cancellationToken);
        // Stryker restore all
        // Stryker disable all : Excluded from coverage
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage] static async ValueTask<Result> EnsureCore(ValueTask<Result> t, Func<bool> p, Error e, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            return (await t.ConfigureAwait(false)).Ensure(p, e);
        }
        // Stryker restore all
    }

    public static ValueTask<Result> Ensure<TState>(
        this ValueTask<Result> resultTask, TState state, Func<TState, bool> predicate, Error error, CancellationToken cancellationToken = default)
    {
        // Stryker disable all : Fast path optimization
        if (resultTask.IsCompletedSuccessfully) return new ValueTask<Result>(resultTask.Result.Ensure(state, predicate, error));
        return EnsureCore(resultTask, state, predicate, error, cancellationToken);
        // Stryker restore all
        // Stryker disable all : Excluded from coverage
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage] static async ValueTask<Result> EnsureCore(ValueTask<Result> t, TState s, Func<TState, bool> p, Error e, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            return (await t.ConfigureAwait(false)).Ensure(s, p, e);
        }
        // Stryker restore all
    }

    public static ValueTask<Result> MapError(
        this ValueTask<Result> resultTask, Func<Error, Error> mapper, CancellationToken cancellationToken = default)
    {
        // Stryker disable all : Fast path optimization
        if (resultTask.IsCompletedSuccessfully) return new ValueTask<Result>(resultTask.Result.MapError(mapper));
        return MapErrorCore(resultTask, mapper, cancellationToken);
        // Stryker restore all
        // Stryker disable all : Excluded from coverage
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage] static async ValueTask<Result> MapErrorCore(ValueTask<Result> t, Func<Error, Error> m, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            return (await t.ConfigureAwait(false)).MapError(m);
        }
        // Stryker restore all
    }

    public static ValueTask<Result> MapError<TState>(
        this ValueTask<Result> resultTask, TState state, Func<TState, Error, Error> mapper, CancellationToken cancellationToken = default)
    {
        // Stryker disable all : Fast path optimization
        if (resultTask.IsCompletedSuccessfully) return new ValueTask<Result>(resultTask.Result.MapError(state, mapper));
        return MapErrorCore(resultTask, state, mapper, cancellationToken);
        // Stryker restore all
        // Stryker disable all : Excluded from coverage
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage] static async ValueTask<Result> MapErrorCore(ValueTask<Result> t, TState s, Func<TState, Error, Error> m, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            return (await t.ConfigureAwait(false)).MapError(s, m);
        }
        // Stryker restore all
    }

    public static ValueTask<Result> Inspect(
        this ValueTask<Result> resultTask, Action<Result> action, CancellationToken cancellationToken = default)
    {
        // Stryker disable all : Fast path optimization
        if (resultTask.IsCompletedSuccessfully) return new ValueTask<Result>(resultTask.Result.Inspect(action));
        return InspectCore(resultTask, action, cancellationToken);
        // Stryker restore all
        // Stryker disable all : Excluded from coverage
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage] static async ValueTask<Result> InspectCore(ValueTask<Result> t, Action<Result> a, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            return (await t.ConfigureAwait(false)).Inspect(a);
        }
        // Stryker restore all
    }

    public static ValueTask<Result> Inspect<TState>(
        this ValueTask<Result> resultTask, TState state, Action<TState, Result> action, CancellationToken cancellationToken = default)
    {
        // Stryker disable all : Fast path optimization
        if (resultTask.IsCompletedSuccessfully) return new ValueTask<Result>(resultTask.Result.Inspect(state, action));
        return InspectCore(resultTask, state, action, cancellationToken);
        // Stryker restore all
        // Stryker disable all : Excluded from coverage
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage] static async ValueTask<Result> InspectCore(ValueTask<Result> t, TState s, Action<TState, Result> a, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            return (await t.ConfigureAwait(false)).Inspect(s, a);
        }
        // Stryker restore all
    }

    // --- Map (ValueTask<Result> non-generic) ---------------------------------

    /// <summary>
    /// Projects a successful <see cref="Result"/> into a <see cref="Result{TNext}"/> by executing
    /// <paramref name="mapper"/> on success. If the Result is a failure, returns a failure without invoking <paramref name="mapper"/>.
    /// </summary>
    public static ValueTask<Result<TNext>> Map<TNext>(
        this ValueTask<Result> resultTask, Func<TNext> mapper, CancellationToken cancellationToken = default)
    {
        // Stryker disable all : Fast path optimization
        if (resultTask.IsCompletedSuccessfully) return new ValueTask<Result<TNext>>(resultTask.Result.Map(mapper));
        return MapCore(resultTask, mapper, cancellationToken);
        // Stryker restore all
        // Stryker disable all : Excluded from coverage
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage] static async ValueTask<Result<TNext>> MapCore(ValueTask<Result> t, Func<TNext> m, CancellationToken ct)
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
    public static ValueTask<Result<TNext>> Map<TState, TNext>(
        this ValueTask<Result> resultTask, TState state, Func<TState, TNext> mapper, CancellationToken cancellationToken = default)
    {
        // Stryker disable all : Fast path optimization
        if (resultTask.IsCompletedSuccessfully) return new ValueTask<Result<TNext>>(resultTask.Result.Map(state, mapper));
        return MapStateCore(resultTask, state, mapper, cancellationToken);
        // Stryker restore all
        // Stryker disable all : Excluded from coverage
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage] static async ValueTask<Result<TNext>> MapStateCore(ValueTask<Result> t, TState s, Func<TState, TNext> m, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            return (await t.ConfigureAwait(false)).Map(s, m);
        }
        // Stryker restore all
    }

    // --- Recover (ValueTask<Result> non-generic) ------------------------------

    /// <summary>
    /// If the <see cref="Result"/> is a failure, invokes <paramref name="recovery"/> to attempt
    /// a corrective result. If the Result is a success, returns it unchanged.
    /// </summary>
    public static ValueTask<Result> Recover(
        this ValueTask<Result> resultTask, Func<Error, Result> recovery, CancellationToken cancellationToken = default)
    {
        // Stryker disable all : Fast path optimization
        if (resultTask.IsCompletedSuccessfully) return new ValueTask<Result>(resultTask.Result.Recover(recovery));
        return RecoverCore(resultTask, recovery, cancellationToken);
        // Stryker restore all
        // Stryker disable all : Excluded from coverage
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage] static async ValueTask<Result> RecoverCore(ValueTask<Result> t, Func<Error, Result> r, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            return (await t.ConfigureAwait(false)).Recover(r);
        }
        // Stryker restore all
    }

    /// <summary>
    /// If the <see cref="Result"/> is a failure, invokes <paramref name="recovery"/> with state to attempt
    /// a corrective result.
    /// </summary>
    public static ValueTask<Result> Recover<TState>(
        this ValueTask<Result> resultTask, TState state, Func<TState, Error, Result> recovery, CancellationToken cancellationToken = default)
    {
        // Stryker disable all : Fast path optimization
        if (resultTask.IsCompletedSuccessfully) return new ValueTask<Result>(resultTask.Result.Recover(state, recovery));
        return RecoverStateCore(resultTask, state, recovery, cancellationToken);
        // Stryker restore all
        // Stryker disable all : Excluded from coverage
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage] static async ValueTask<Result> RecoverStateCore(ValueTask<Result> t, TState s, Func<TState, Error, Result> r, CancellationToken ct)
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
    public static ValueTask<Result> Recover(
        this ValueTask<Result> resultTask, Func<Error, ValueTask<Result>> recovery, CancellationToken cancellationToken = default)
    {
        // Stryker disable all : Fast path optimization
        if (resultTask.IsCompletedSuccessfully)
        {
            var result = resultTask.Result;
            if (result.IsSuccess) return new ValueTask<Result>(result);
            return recovery(result.Error);
        }
        // Stryker restore all
        return RecoverAsyncCore(resultTask, recovery, cancellationToken);
        // Stryker disable all : Excluded from coverage
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage] static async ValueTask<Result> RecoverAsyncCore(ValueTask<Result> t, Func<Error, ValueTask<Result>> r, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            var res = await t.ConfigureAwait(false);
            if (res.IsSuccess) return res;
            return await r(res.Error).ConfigureAwait(false);
        }
        // Stryker restore all
    }
}
