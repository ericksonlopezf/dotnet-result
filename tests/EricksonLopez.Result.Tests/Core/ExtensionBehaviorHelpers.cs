using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace EricksonLopez.Result.Tests.Core;

public static class ExtensionBehaviorHelpers
{
    public static readonly Error TestError = Error.Failure("Test.Error", "Test error message");
    public static readonly Error TestError2 = Error.Failure("Test.Error2", "Test error message 2");

    public static async Task AssertBindBehaviorAsync<TIn, TOut>(
        Func<Result<TIn>, Func<TIn, Task<Result<TOut>>>, Task<Result<TOut>>> bindUnderTest,
        TIn successValue,
        TOut expectedMappedValue)
    {
        int invokeCount = 0;
        var successResult = Result.Success(successValue);
        var res1 = await bindUnderTest(successResult, v => { invokeCount++; return Task.FromResult(Result.Success(expectedMappedValue)); });
        
        Assert.Equal(1, invokeCount);
        Assert.True(res1.IsSuccess);
        Assert.Equal(expectedMappedValue, res1.Value);

        invokeCount = 0;
        var failureResult = Result.Failure<TIn>(TestError);
        var res2 = await bindUnderTest(failureResult, v => { invokeCount++; return Task.FromResult(Result.Success(expectedMappedValue)); });
        
        Assert.Equal(0, invokeCount);
        Assert.True(res2.IsFailure);
        Assert.Same(TestError, res2.Error);

        invokeCount = 0;
        var res3 = await bindUnderTest(successResult, v => { invokeCount++; return Task.FromResult(Result.Failure<TOut>(TestError2)); });
        
        Assert.Equal(1, invokeCount);
        Assert.True(res3.IsFailure);
        Assert.Same(TestError2, res3.Error);
    }

    public static async Task AssertBindBehaviorAsync<TOut>(
        Func<Result, Func<Task<Result<TOut>>>, Task<Result<TOut>>> bindUnderTest,
        TOut expectedMappedValue)
    {
        int invokeCount = 0;
        var successResult = Result.Success();
        var res1 = await bindUnderTest(successResult, () => { invokeCount++; return Task.FromResult(Result.Success(expectedMappedValue)); });
        
        Assert.Equal(1, invokeCount);
        Assert.True(res1.IsSuccess);
        Assert.Equal(expectedMappedValue, res1.Value);

        invokeCount = 0;
        var failureResult = Result.Failure(TestError);
        var res2 = await bindUnderTest(failureResult, () => { invokeCount++; return Task.FromResult(Result.Success(expectedMappedValue)); });
        
        Assert.Equal(0, invokeCount);
        Assert.True(res2.IsFailure);
        Assert.Same(TestError, res2.Error);

        invokeCount = 0;
        var res3 = await bindUnderTest(successResult, () => { invokeCount++; return Task.FromResult(Result.Failure<TOut>(TestError2)); });
        
        Assert.Equal(1, invokeCount);
        Assert.True(res3.IsFailure);
        Assert.Same(TestError2, res3.Error);
    }
}
