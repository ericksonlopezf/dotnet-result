// Copyright © Erickson Lopez. MIT License.
using System;
using System.Threading;
using System.Threading.Tasks;
using EricksonLopez.Result;

namespace EricksonLopez.Result.Tests.Core;

public static class ResultTestHelpers
{
    public static async Task<T> AsAsync<T>(this T value)
    {
        await Task.Yield();
        return value;
    }

    public static async ValueTask<T> AsAsyncValueTask<T>(this T value)
    {
        await Task.Yield();
        return value;
    }

    public static async Task<Result<T>> AsAsyncResult<T>(this T value)
    {
        await Task.Yield();
        return Result.Success(value);
    }

    public static async ValueTask<Result<T>> AsAsyncValueTaskResult<T>(this T value)
    {
        await Task.Yield();
        return Result.Success(value);
    }

    public static async Task<Result<T>> AsAsyncFailedResult<T>(this Error error)
    {
        await Task.Yield();
        return Result.Failure<T>(error);
    }

    public static async ValueTask<Result<T>> AsAsyncFailedValueTaskResult<T>(this Error error)
    {
        await Task.Yield();
        return Result.Failure<T>(error);
    }

    public static async Task<Result> AsAsyncResult(this Result result)
    {
        await Task.Yield();
        return result;
    }

    public static async ValueTask<Result> AsAsyncValueTaskResult(this Result result)
    {
        await Task.Yield();
        return result;
    }

    public static Task<Result> IncompleteTask(this Result result)
    {
        var tcs = new TaskCompletionSource<Result>(TaskCreationOptions.RunContinuationsAsynchronously);
        Task.Run(async () =>
        {
            await Task.Delay(50).ConfigureAwait(false);
            tcs.TrySetResult(result);
        });
        return tcs.Task;
    }

    public static Task<Result<T>> IncompleteTask<T>(this Result<T> result)
    {
        var tcs = new TaskCompletionSource<Result<T>>(TaskCreationOptions.RunContinuationsAsynchronously);
        Task.Run(async () =>
        {
            await Task.Delay(50).ConfigureAwait(false);
            tcs.TrySetResult(result);
        });
        return tcs.Task;
    }

    public static ValueTask<Result> IncompleteValueTask(this Result result)
    {
        var tcs = new TaskCompletionSource<Result>(TaskCreationOptions.RunContinuationsAsynchronously);
        Task.Run(async () =>
        {
            await Task.Delay(50).ConfigureAwait(false);
            tcs.TrySetResult(result);
        });
        return new ValueTask<Result>(tcs.Task);
    }

    public static ValueTask<Result<T>> IncompleteValueTask<T>(this Result<T> result)
    {
        var tcs = new TaskCompletionSource<Result<T>>(TaskCreationOptions.RunContinuationsAsynchronously);
        Task.Run(async () =>
        {
            await Task.Delay(50).ConfigureAwait(false);
            tcs.TrySetResult(result);
        });
        return new ValueTask<Result<T>>(tcs.Task);
    }
}
