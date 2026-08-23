// Copyright © Erickson Lopez. MIT License.
using System;
using System.Threading;
using System.Threading.Tasks;
using EricksonLopez.Result;
using EricksonLopez.Result.Testing;
using Xunit;

namespace EricksonLopez.Result.Tests.Core;

public static class ExtensionBehaviorHelpers
{
    public static readonly Error TestError = ResultExtensionsTestsBase.TestError;
    public static readonly Error TestError2 = ResultExtensionsTestsBase.TestError2;

    public static async Task AssertBindBehaviorAsync<TIn, TOut>(
        Func<Result<TIn>, Func<TIn, Task<Result<TOut>>>, Task<Result<TOut>>> bindUnderTest,
        TIn successValue,
        TOut expectedMappedValue)
    {
        int invokeCount = 0;
        var successResult = Result.Success(successValue);
        var res1 = await bindUnderTest(successResult, v => { invokeCount++; return Task.FromResult(Result.Success(expectedMappedValue)); });

        Assert.Equal(1, invokeCount);
        res1.ShouldBeSuccess();
        Assert.Equal(expectedMappedValue, res1.Value);

        invokeCount = 0;
        var failureResult = Result.Failure<TIn>(TestError);
        var res2 = await bindUnderTest(failureResult, v => { invokeCount++; return Task.FromResult(Result.Success(expectedMappedValue)); });

        Assert.Equal(0, invokeCount);
        res2.ShouldBeFailure();
        Assert.Same(TestError, res2.Error);

        invokeCount = 0;
        var res3 = await bindUnderTest(successResult, v => { invokeCount++; return Task.FromResult(Result.Failure<TOut>(TestError2)); });

        Assert.Equal(1, invokeCount);
        res3.ShouldBeFailure();
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
        res1.ShouldBeSuccess();
        Assert.Equal(expectedMappedValue, res1.Value);

        invokeCount = 0;
        var failureResult = Result.Failure(TestError);
        var res2 = await bindUnderTest(failureResult, () => { invokeCount++; return Task.FromResult(Result.Success(expectedMappedValue)); });

        Assert.Equal(0, invokeCount);
        res2.ShouldBeFailure();
        Assert.Same(TestError, res2.Error);

        invokeCount = 0;
        var res3 = await bindUnderTest(successResult, () => { invokeCount++; return Task.FromResult(Result.Failure<TOut>(TestError2)); });

        Assert.Equal(1, invokeCount);
        res3.ShouldBeFailure();
        Assert.Same(TestError2, res3.Error);
    }
}




