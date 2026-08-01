using System;
using System.Threading.Tasks;
using Xunit;

namespace EricksonLopez.Result.Tests.Core;

public class ResultExtensionsMatchBehaviorTests
{
    private static readonly Error TestError = Error.Failure("Test.Error", "Test error message");
    private static readonly Error TestError2 = Error.Failure("Test.Error2", "Test error message 2");


    [Fact]
    public void Match_WhenSuccess_InvokesSuccessAndReturnsValue()
    {
        var result = Result.Success(5);
        int successCount = 0;
        int failureCount = 0;
        
        var value = result.Match(
            onSuccess: v => { successCount++; return v * 2; },
            onFailure: e => { failureCount++; return -1; }
        );
        
        Assert.Equal(1, successCount);
        Assert.Equal(0, failureCount);
        Assert.Equal(10, value);
    }

    [Fact]
    public void Match_WhenFailure_InvokesFailureAndReturnsValue()
    {
        var result = Result.Failure<int>(TestError);
        int successCount = 0;
        int failureCount = 0;
        
        var value = result.Match(
            onSuccess: v => { successCount++; return v * 2; },
            onFailure: e => { failureCount++; return -1; }
        );
        
        Assert.Equal(0, successCount);
        Assert.Equal(1, failureCount);
        Assert.Equal(-1, value);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Match_TaskResult_WhenSuccess_InvokesSuccess(bool fastPath)
    {
        int successCount = 0;
        int failureCount = 0;
        var tcs = new TaskCompletionSource<Result<int>>();
        if (fastPath) tcs.SetResult(Result.Success(5));

        var task = tcs.Task.Match(
            onSuccess: v => { successCount++; return v * 2; },
            onFailure: e => { failureCount++; return -1; }
        );
        
        if (!fastPath)
        {
            Assert.False(task.IsCompleted);
            tcs.SetResult(Result.Success(5));
        }

        var val = await task;
        Assert.Equal(1, successCount);
        Assert.Equal(0, failureCount);
        Assert.Equal(10, val);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Match_TaskResult_WhenFailure_InvokesFailure(bool fastPath)
    {
        int successCount = 0;
        int failureCount = 0;
        var tcs = new TaskCompletionSource<Result<int>>();
        if (fastPath) tcs.SetResult(Result.Failure<int>(TestError));

        var task = tcs.Task.Match(
            onSuccess: v => { successCount++; return v * 2; },
            onFailure: e => { failureCount++; return -1; }
        );
        
        if (!fastPath)
        {
            Assert.False(task.IsCompleted);
            tcs.SetResult(Result.Failure<int>(TestError));
        }

        var val = await task;
        Assert.Equal(0, successCount);
        Assert.Equal(1, failureCount);
        Assert.Equal(-1, val);
    }

}
