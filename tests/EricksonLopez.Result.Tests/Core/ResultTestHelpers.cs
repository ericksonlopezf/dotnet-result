using System;
using System.Threading.Tasks;

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
}
