using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace EricksonLopez.Result.Tests.Core;

public class ResultExtensionsInspectBehaviorTests : ResultExtensionsTestsBase
{

    [Fact]
    public void Inspect_1()
    {
        int invokeCount = 0;
        var r = Result.Success().Inspect(v => { invokeCount++; });
        Assert.Equal(1, invokeCount);
        Assert.True(r.IsSuccess);
    }

    [Fact]
    public void Inspect_2()
    {
        int invokeCount = 0;
        var r = Result.Failure(TestError).Inspect(v => { invokeCount++; });
        Assert.Equal(1, invokeCount);
        Assert.True(r.IsFailure);
        Assert.Same(TestError, r.Error);
    }

    [Fact]
    public void Inspect_3()
    {
        int invokeCount = 0;
        var r = Result.Success().Inspect(99, (state, v) => { invokeCount++; });
        Assert.Equal(1, invokeCount);
        Assert.True(r.IsSuccess);
    }

    [Fact]
    public void Inspect_4()
    {
        int invokeCount = 0;
        var r = Result.Failure(TestError).Inspect(99, (state, v) => { invokeCount++; });
        Assert.Equal(1, invokeCount);
        Assert.True(r.IsFailure);
        Assert.Same(TestError, r.Error);
    }

    [Fact]
    public void Inspect_5()
    {
        int invokeCount = 0;
        var r = Result.Success(5).Inspect(v => { invokeCount++; });
        Assert.Equal(1, invokeCount);
        Assert.True(r.IsSuccess);
        Assert.Equal(5, r.Value);
    }

    [Fact]
    public void Inspect_6()
    {
        int invokeCount = 0;
        var r = Result.Failure<int>(TestError).Inspect(v => { invokeCount++; });
        Assert.Equal(1, invokeCount);
        Assert.True(r.IsFailure);
        Assert.Same(TestError, r.Error);
    }

    [Fact]
    public void Inspect_7()
    {
        int invokeCount = 0;
        var r = Result.Success(5).Inspect(99, (state, v) => { invokeCount++; });
        Assert.Equal(1, invokeCount);
        Assert.True(r.IsSuccess);
        Assert.Equal(5, r.Value);
    }

    [Fact]
    public void Inspect_8()
    {
        int invokeCount = 0;
        var r = Result.Failure<int>(TestError).Inspect(99, (state, v) => { invokeCount++; });
        Assert.Equal(1, invokeCount);
        Assert.True(r.IsFailure);
        Assert.Same(TestError, r.Error);
    }

    [Fact]
    public async Task Inspect_9()
    {
        int invokeCount = 0;
        var r = await Task.FromResult(Result.Success()).Inspect(v => { invokeCount++; });
        Assert.Equal(1, invokeCount);
        Assert.True(r.IsSuccess);
    }

    [Fact]
    public async Task Inspect_10()
    {
        int invokeCount = 0;
        var r = await Task.FromResult(Result.Failure(TestError)).Inspect(v => { invokeCount++; });
        Assert.Equal(1, invokeCount);
        Assert.True(r.IsFailure);
        Assert.Same(TestError, r.Error);
    }

    [Fact]
    public async Task Inspect_11()
    {
        int invokeCount = 0;
        var r = await Task.FromResult(Result.Success()).Inspect(99, (state, v) => { invokeCount++; });
        Assert.Equal(1, invokeCount);
        Assert.True(r.IsSuccess);
    }

    [Fact]
    public async Task Inspect_12()
    {
        int invokeCount = 0;
        var r = await Task.FromResult(Result.Failure(TestError)).Inspect(99, (state, v) => { invokeCount++; });
        Assert.Equal(1, invokeCount);
        Assert.True(r.IsFailure);
        Assert.Same(TestError, r.Error);
    }

    [Fact]
    public async Task Inspect_13()
    {
        int invokeCount = 0;
        var r = await Task.FromResult(Result.Success(5)).Inspect(v => { invokeCount++; });
        Assert.Equal(1, invokeCount);
        Assert.True(r.IsSuccess);
        Assert.Equal(5, r.Value);
    }

    [Fact]
    public async Task Inspect_14()
    {
        int invokeCount = 0;
        var r = await Task.FromResult(Result.Failure<int>(TestError)).Inspect(v => { invokeCount++; });
        Assert.Equal(1, invokeCount);
        Assert.True(r.IsFailure);
        Assert.Same(TestError, r.Error);
    }

    [Fact]
    public async Task Inspect_15()
    {
        int invokeCount = 0;
        var r = await Task.FromResult(Result.Success(5)).Inspect(99, (state, v) => { invokeCount++; });
        Assert.Equal(1, invokeCount);
        Assert.True(r.IsSuccess);
        Assert.Equal(5, r.Value);
    }

    [Fact]
    public async Task Inspect_16()
    {
        int invokeCount = 0;
        var r = await Task.FromResult(Result.Failure<int>(TestError)).Inspect(99, (state, v) => { invokeCount++; });
        Assert.Equal(1, invokeCount);
        Assert.True(r.IsFailure);
        Assert.Same(TestError, r.Error);
    }

    [Fact]
    public async Task Inspect_17()
    {
        int invokeCount = 0;
        var r = await new ValueTask<Result>(Result.Success()).Inspect(v => { invokeCount++; });
        Assert.Equal(1, invokeCount);
        Assert.True(r.IsSuccess);
    }

    [Fact]
    public async Task Inspect_18()
    {
        int invokeCount = 0;
        var r = await new ValueTask<Result>(Result.Failure(TestError)).Inspect(v => { invokeCount++; });
        Assert.Equal(1, invokeCount);
        Assert.True(r.IsFailure);
        Assert.Same(TestError, r.Error);
    }

    [Fact]
    public async Task Inspect_19()
    {
        int invokeCount = 0;
        var r = await new ValueTask<Result>(Result.Success()).Inspect(99, (state, v) => { invokeCount++; });
        Assert.Equal(1, invokeCount);
        Assert.True(r.IsSuccess);
    }

    [Fact]
    public async Task Inspect_20()
    {
        int invokeCount = 0;
        var r = await new ValueTask<Result>(Result.Failure(TestError)).Inspect(99, (state, v) => { invokeCount++; });
        Assert.Equal(1, invokeCount);
        Assert.True(r.IsFailure);
        Assert.Same(TestError, r.Error);
    }

    [Fact]
    public async Task Inspect_21()
    {
        int invokeCount = 0;
        var r = await new ValueTask<Result<int>>(Result.Success(5)).Inspect(v => { invokeCount++; });
        Assert.Equal(1, invokeCount);
        Assert.True(r.IsSuccess);
        Assert.Equal(5, r.Value);
    }

    [Fact]
    public async Task Inspect_22()
    {
        int invokeCount = 0;
        var r = await new ValueTask<Result<int>>(Result.Failure<int>(TestError)).Inspect(v => { invokeCount++; });
        Assert.Equal(1, invokeCount);
        Assert.True(r.IsFailure);
        Assert.Same(TestError, r.Error);
    }

    [Fact]
    public async Task Inspect_23()
    {
        int invokeCount = 0;
        var r = await new ValueTask<Result<int>>(Result.Success(5)).Inspect(99, (state, v) => { invokeCount++; });
        Assert.Equal(1, invokeCount);
        Assert.True(r.IsSuccess);
        Assert.Equal(5, r.Value);
    }

    [Fact]
    public async Task Inspect_24()
    {
        int invokeCount = 0;
        var r = await new ValueTask<Result<int>>(Result.Failure<int>(TestError)).Inspect(99, (state, v) => { invokeCount++; });
        Assert.Equal(1, invokeCount);
        Assert.True(r.IsFailure);
        Assert.Same(TestError, r.Error);
    }
}
