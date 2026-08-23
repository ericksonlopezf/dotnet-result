// Copyright © Erickson Lopez. MIT License.
using System;
using System.Threading;
using System.Threading.Tasks;
using EricksonLopez.Result;
using EricksonLopez.Result.Testing;
using Xunit;

namespace EricksonLopez.Result.Tests.Core;

public class ResultExtensionsMapBehaviorTests : ResultExtensionsTestsBase
{

    [Fact]
    public void Map_NonGeneric_OnSuccess_ReturnsSuccess()
    {
        int invokeCount = 0;
        var r = Result.Success().Map(() => { invokeCount++; return "test"; });
        Assert.Equal(1, invokeCount);
        r.ShouldBeSuccess();
        Assert.Equal("test", r.Value);
    }

    [Fact]
    public void Map_NonGeneric_OnFailure_ShortCircuits()
    {
        int invokeCount = 0;
        var r = Result.Failure(TestError).Map(() => { invokeCount++; return "test"; });
        Assert.Equal(0, invokeCount);
        r.ShouldBeFailure();
        Assert.Same(TestError, r.Error);
    }

    [Fact]
    public void Map_NonGeneric_WithState_OnSuccess_ReturnsSuccess()
    {
        int invokeCount = 0;
        var r = Result.Success().Map(99, state => { invokeCount++; return "test"; });
        Assert.Equal(1, invokeCount);
        r.ShouldBeSuccess();
        Assert.Equal("test", r.Value);
    }

    [Fact]
    public void Map_NonGeneric_WithState_OnFailure_ShortCircuits()
    {
        int invokeCount = 0;
        var r = Result.Failure(TestError).Map(99, state => { invokeCount++; return "test"; });
        Assert.Equal(0, invokeCount);
        r.ShouldBeFailure();
        Assert.Same(TestError, r.Error);
    }

    [Fact]
    public void Map_Generic_OnSuccess_ReturnsSuccess()
    {
        int invokeCount = 0;
        var r = Result.Success(5).Map(v => { invokeCount++; return "test"; });
        Assert.Equal(1, invokeCount);
        r.ShouldBeSuccess();
        Assert.Equal("test", r.Value);
    }

    [Fact]
    public void Map_Generic_OnFailure_ShortCircuits()
    {
        int invokeCount = 0;
        var r = Result.Failure<int>(TestError).Map(v => { invokeCount++; return "test"; });
        Assert.Equal(0, invokeCount);
        r.ShouldBeFailure();
        Assert.Same(TestError, r.Error);
    }

    [Fact]
    public void Map_Generic_WithState_OnSuccess_ReturnsSuccess()
    {
        int invokeCount = 0;
        var r = Result.Success(5).Map(99, (state, v) => { invokeCount++; return "test"; });
        Assert.Equal(1, invokeCount);
        r.ShouldBeSuccess();
        Assert.Equal("test", r.Value);
    }

    [Fact]
    public void Map_Generic_WithState_OnFailure_ShortCircuits()
    {
        int invokeCount = 0;
        var r = Result.Failure<int>(TestError).Map(99, (state, v) => { invokeCount++; return "test"; });
        Assert.Equal(0, invokeCount);
        r.ShouldBeFailure();
        Assert.Same(TestError, r.Error);
    }

    [Fact]
    public async Task Map_Task_NonGeneric_OnSuccess_ReturnsSuccess()
    {
        int invokeCount = 0;
        var r = await Task.FromResult(Result.Success()).Map(() => { invokeCount++; return "test"; });
        Assert.Equal(1, invokeCount);
        r.ShouldBeSuccess();
        Assert.Equal("test", r.Value);
    }

    [Fact]
    public async Task Map_Task_NonGeneric_OnFailure_ShortCircuits()
    {
        int invokeCount = 0;
        var r = await Task.FromResult(Result.Failure(TestError)).Map(() => { invokeCount++; return "test"; });
        Assert.Equal(0, invokeCount);
        r.ShouldBeFailure();
        Assert.Same(TestError, r.Error);
    }

    [Fact]
    public async Task Map_Task_NonGeneric_WithState_OnSuccess_ReturnsSuccess()
    {
        int invokeCount = 0;
        var r = await Task.FromResult(Result.Success()).Map(99, state => { invokeCount++; return "test"; });
        Assert.Equal(1, invokeCount);
        r.ShouldBeSuccess();
        Assert.Equal("test", r.Value);
    }

    [Fact]
    public async Task Map_Task_NonGeneric_WithState_OnFailure_ShortCircuits()
    {
        int invokeCount = 0;
        var r = await Task.FromResult(Result.Failure(TestError)).Map(99, state => { invokeCount++; return "test"; });
        Assert.Equal(0, invokeCount);
        r.ShouldBeFailure();
        Assert.Same(TestError, r.Error);
    }

    [Fact]
    public async Task Map_Task_Generic_OnSuccess_ReturnsSuccess()
    {
        int invokeCount = 0;
        var r = await Task.FromResult(Result.Success(5)).Map(v => { invokeCount++; return "test"; });
        Assert.Equal(1, invokeCount);
        r.ShouldBeSuccess();
        Assert.Equal("test", r.Value);
    }

    [Fact]
    public async Task Map_Task_Generic_OnFailure_ShortCircuits()
    {
        int invokeCount = 0;
        var r = await Task.FromResult(Result.Failure<int>(TestError)).Map(v => { invokeCount++; return "test"; });
        Assert.Equal(0, invokeCount);
        r.ShouldBeFailure();
        Assert.Same(TestError, r.Error);
    }

    [Fact]
    public async Task Map_Task_Generic_WithState_OnSuccess_ReturnsSuccess()
    {
        int invokeCount = 0;
        var r = await Task.FromResult(Result.Success(5)).Map(99, (state, v) => { invokeCount++; return "test"; });
        Assert.Equal(1, invokeCount);
        r.ShouldBeSuccess();
        Assert.Equal("test", r.Value);
    }

    [Fact]
    public async Task Map_Task_Generic_WithState_OnFailure_ShortCircuits()
    {
        int invokeCount = 0;
        var r = await Task.FromResult(Result.Failure<int>(TestError)).Map(99, (state, v) => { invokeCount++; return "test"; });
        Assert.Equal(0, invokeCount);
        r.ShouldBeFailure();
        Assert.Same(TestError, r.Error);
    }

    [Fact]
    public async Task Map_Task_Generic_TaskCallback_OnSuccess_ReturnsSuccess()
    {
        int invokeCount = 0;
        var r = await Task.FromResult(Result.Success(5)).Map(v => { invokeCount++; return Task.FromResult("test"); });
        Assert.Equal(1, invokeCount);
        r.ShouldBeSuccess();
        Assert.Equal("test", r.Value);
    }

    [Fact]
    public async Task Map_Task_Generic_TaskCallback_OnFailure_ShortCircuits()
    {
        int invokeCount = 0;
        var r = await Task.FromResult(Result.Failure<int>(TestError)).Map(v => { invokeCount++; return Task.FromResult("test"); });
        Assert.Equal(0, invokeCount);
        r.ShouldBeFailure();
        Assert.Same(TestError, r.Error);
    }

    [Fact]
    public async Task Map_Task_Generic_WithState_TaskCallback_OnSuccess_ReturnsSuccess()
    {
        int invokeCount = 0;
        var r = await Task.FromResult(Result.Success(5)).Map(99, (state, v) => { invokeCount++; return Task.FromResult("test"); });
        Assert.Equal(1, invokeCount);
        r.ShouldBeSuccess();
        Assert.Equal("test", r.Value);
    }

    [Fact]
    public async Task Map_Task_Generic_WithState_TaskCallback_OnFailure_ShortCircuits()
    {
        int invokeCount = 0;
        var r = await Task.FromResult(Result.Failure<int>(TestError)).Map(99, (state, v) => { invokeCount++; return Task.FromResult("test"); });
        Assert.Equal(0, invokeCount);
        r.ShouldBeFailure();
        Assert.Same(TestError, r.Error);
    }

    [Fact]
    public async Task Map_ValueTask_NonGeneric_OnSuccess_ReturnsSuccess()
    {
        int invokeCount = 0;
        var r = await new ValueTask<Result>(Result.Success()).Map(() => { invokeCount++; return "test"; });
        Assert.Equal(1, invokeCount);
        r.ShouldBeSuccess();
        Assert.Equal("test", r.Value);
    }

    [Fact]
    public async Task Map_ValueTask_NonGeneric_OnFailure_ShortCircuits()
    {
        int invokeCount = 0;
        var r = await new ValueTask<Result>(Result.Failure(TestError)).Map(() => { invokeCount++; return "test"; });
        Assert.Equal(0, invokeCount);
        r.ShouldBeFailure();
        Assert.Same(TestError, r.Error);
    }

    [Fact]
    public async Task Map_ValueTask_NonGeneric_WithState_OnSuccess_ReturnsSuccess()
    {
        int invokeCount = 0;
        var r = await new ValueTask<Result>(Result.Success()).Map(99, state => { invokeCount++; return "test"; });
        Assert.Equal(1, invokeCount);
        r.ShouldBeSuccess();
        Assert.Equal("test", r.Value);
    }

    [Fact]
    public async Task Map_ValueTask_NonGeneric_WithState_OnFailure_ShortCircuits()
    {
        int invokeCount = 0;
        var r = await new ValueTask<Result>(Result.Failure(TestError)).Map(99, state => { invokeCount++; return "test"; });
        Assert.Equal(0, invokeCount);
        r.ShouldBeFailure();
        Assert.Same(TestError, r.Error);
    }

    [Fact]
    public async Task Map_ValueTask_Generic_OnSuccess_ReturnsSuccess()
    {
        int invokeCount = 0;
        var r = await new ValueTask<Result<int>>(Result.Success(5)).Map(v => { invokeCount++; return "test"; });
        Assert.Equal(1, invokeCount);
        r.ShouldBeSuccess();
        Assert.Equal("test", r.Value);
    }

    [Fact]
    public async Task Map_ValueTask_Generic_OnFailure_ShortCircuits()
    {
        int invokeCount = 0;
        var r = await new ValueTask<Result<int>>(Result.Failure<int>(TestError)).Map(v => { invokeCount++; return "test"; });
        Assert.Equal(0, invokeCount);
        r.ShouldBeFailure();
        Assert.Same(TestError, r.Error);
    }

    [Fact]
    public async Task Map_ValueTask_Generic_WithState_OnSuccess_ReturnsSuccess()
    {
        int invokeCount = 0;
        var r = await new ValueTask<Result<int>>(Result.Success(5)).Map(99, (state, v) => { invokeCount++; return "test"; });
        Assert.Equal(1, invokeCount);
        r.ShouldBeSuccess();
        Assert.Equal("test", r.Value);
    }

    [Fact]
    public async Task Map_ValueTask_Generic_WithState_OnFailure_ShortCircuits()
    {
        int invokeCount = 0;
        var r = await new ValueTask<Result<int>>(Result.Failure<int>(TestError)).Map(99, (state, v) => { invokeCount++; return "test"; });
        Assert.Equal(0, invokeCount);
        r.ShouldBeFailure();
        Assert.Same(TestError, r.Error);
    }

    [Fact]
    public async Task Map_ValueTask_Generic_TaskCallback_OnSuccess_ReturnsSuccess()
    {
        int invokeCount = 0;
        var r = await new ValueTask<Result<int>>(Result.Success(5)).Map(v => { invokeCount++; return new ValueTask<string>("test"); });
        Assert.Equal(1, invokeCount);
        r.ShouldBeSuccess();
        Assert.Equal("test", r.Value);
    }

    [Fact]
    public async Task Map_ValueTask_Generic_TaskCallback_OnFailure_ShortCircuits()
    {
        int invokeCount = 0;
        var r = await new ValueTask<Result<int>>(Result.Failure<int>(TestError)).Map(v => { invokeCount++; return new ValueTask<string>("test"); });
        Assert.Equal(0, invokeCount);
        r.ShouldBeFailure();
        Assert.Same(TestError, r.Error);
    }

    [Fact]
    public async Task Map_ValueTask_Generic_WithState_TaskCallback_OnSuccess_ReturnsSuccess()
    {
        int invokeCount = 0;
        var r = await new ValueTask<Result<int>>(Result.Success(5)).Map(99, (state, v) => { invokeCount++; return new ValueTask<string>("test"); });
        Assert.Equal(1, invokeCount);
        r.ShouldBeSuccess();
        Assert.Equal("test", r.Value);
    }

    [Fact]
    public async Task Map_ValueTask_Generic_WithState_TaskCallback_OnFailure_ShortCircuits()
    {
        int invokeCount = 0;
        var r = await new ValueTask<Result<int>>(Result.Failure<int>(TestError)).Map(99, (state, v) => { invokeCount++; return new ValueTask<string>("test"); });
        Assert.Equal(0, invokeCount);
        r.ShouldBeFailure();
        Assert.Same(TestError, r.Error);
    }


    [Fact]
    public async Task Map_WhenCancellationTokenCanceled_ThrowsOperationCanceledException()
    {
        var ct = new CancellationToken(true);
        var t = Task.FromResult(Result.Success());
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => t.Map(_ => Task.FromResult(5), ct));
    }
}




