using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace EricksonLopez.Result.Tests.Core;

public class ResultExtensionsMapBehaviorTests : ResultExtensionsTestsBase
{

    [Fact]
    public void Map_1()
    {
        int invokeCount = 0;
        var r = Result.Success().Map(() => { invokeCount++; return "test"; });
        Assert.Equal(1, invokeCount);
        Assert.True(r.IsSuccess);
        Assert.Equal("test", r.Value);
    }

    [Fact]
    public void Map_2()
    {
        int invokeCount = 0;
        var r = Result.Failure(TestError).Map(() => { invokeCount++; return "test"; });
        Assert.Equal(0, invokeCount);
        Assert.True(r.IsFailure);
        Assert.Same(TestError, r.Error);
    }

    [Fact]
    public void Map_3()
    {
        int invokeCount = 0;
        var r = Result.Success().Map(99, state => { invokeCount++; return "test"; });
        Assert.Equal(1, invokeCount);
        Assert.True(r.IsSuccess);
        Assert.Equal("test", r.Value);
    }

    [Fact]
    public void Map_4()
    {
        int invokeCount = 0;
        var r = Result.Failure(TestError).Map(99, state => { invokeCount++; return "test"; });
        Assert.Equal(0, invokeCount);
        Assert.True(r.IsFailure);
        Assert.Same(TestError, r.Error);
    }

    [Fact]
    public void Map_5()
    {
        int invokeCount = 0;
        var r = Result.Success(5).Map(v => { invokeCount++; return "test"; });
        Assert.Equal(1, invokeCount);
        Assert.True(r.IsSuccess);
        Assert.Equal("test", r.Value);
    }

    [Fact]
    public void Map_6()
    {
        int invokeCount = 0;
        var r = Result.Failure<int>(TestError).Map(v => { invokeCount++; return "test"; });
        Assert.Equal(0, invokeCount);
        Assert.True(r.IsFailure);
        Assert.Same(TestError, r.Error);
    }

    [Fact]
    public void Map_7()
    {
        int invokeCount = 0;
        var r = Result.Success(5).Map(99, (state, v) => { invokeCount++; return "test"; });
        Assert.Equal(1, invokeCount);
        Assert.True(r.IsSuccess);
        Assert.Equal("test", r.Value);
    }

    [Fact]
    public void Map_8()
    {
        int invokeCount = 0;
        var r = Result.Failure<int>(TestError).Map(99, (state, v) => { invokeCount++; return "test"; });
        Assert.Equal(0, invokeCount);
        Assert.True(r.IsFailure);
        Assert.Same(TestError, r.Error);
    }

    [Fact]
    public async Task Map_9()
    {
        int invokeCount = 0;
        var r = await Task.FromResult(Result.Success()).Map(() => { invokeCount++; return "test"; });
        Assert.Equal(1, invokeCount);
        Assert.True(r.IsSuccess);
        Assert.Equal("test", r.Value);
    }

    [Fact]
    public async Task Map_10()
    {
        int invokeCount = 0;
        var r = await Task.FromResult(Result.Failure(TestError)).Map(() => { invokeCount++; return "test"; });
        Assert.Equal(0, invokeCount);
        Assert.True(r.IsFailure);
        Assert.Same(TestError, r.Error);
    }

    [Fact]
    public async Task Map_11()
    {
        int invokeCount = 0;
        var r = await Task.FromResult(Result.Success()).Map(99, state => { invokeCount++; return "test"; });
        Assert.Equal(1, invokeCount);
        Assert.True(r.IsSuccess);
        Assert.Equal("test", r.Value);
    }

    [Fact]
    public async Task Map_12()
    {
        int invokeCount = 0;
        var r = await Task.FromResult(Result.Failure(TestError)).Map(99, state => { invokeCount++; return "test"; });
        Assert.Equal(0, invokeCount);
        Assert.True(r.IsFailure);
        Assert.Same(TestError, r.Error);
    }

    [Fact]
    public async Task Map_13()
    {
        int invokeCount = 0;
        var r = await Task.FromResult(Result.Success(5)).Map(v => { invokeCount++; return "test"; });
        Assert.Equal(1, invokeCount);
        Assert.True(r.IsSuccess);
        Assert.Equal("test", r.Value);
    }

    [Fact]
    public async Task Map_14()
    {
        int invokeCount = 0;
        var r = await Task.FromResult(Result.Failure<int>(TestError)).Map(v => { invokeCount++; return "test"; });
        Assert.Equal(0, invokeCount);
        Assert.True(r.IsFailure);
        Assert.Same(TestError, r.Error);
    }

    [Fact]
    public async Task Map_15()
    {
        int invokeCount = 0;
        var r = await Task.FromResult(Result.Success(5)).Map(99, (state, v) => { invokeCount++; return "test"; });
        Assert.Equal(1, invokeCount);
        Assert.True(r.IsSuccess);
        Assert.Equal("test", r.Value);
    }

    [Fact]
    public async Task Map_16()
    {
        int invokeCount = 0;
        var r = await Task.FromResult(Result.Failure<int>(TestError)).Map(99, (state, v) => { invokeCount++; return "test"; });
        Assert.Equal(0, invokeCount);
        Assert.True(r.IsFailure);
        Assert.Same(TestError, r.Error);
    }

    [Fact]
    public async Task Map_17()
    {
        int invokeCount = 0;
        var r = await Task.FromResult(Result.Success(5)).Map(v => { invokeCount++; return Task.FromResult("test"); });
        Assert.Equal(1, invokeCount);
        Assert.True(r.IsSuccess);
        Assert.Equal("test", r.Value);
    }

    [Fact]
    public async Task Map_18()
    {
        int invokeCount = 0;
        var r = await Task.FromResult(Result.Failure<int>(TestError)).Map(v => { invokeCount++; return Task.FromResult("test"); });
        Assert.Equal(0, invokeCount);
        Assert.True(r.IsFailure);
        Assert.Same(TestError, r.Error);
    }

    [Fact]
    public async Task Map_19()
    {
        int invokeCount = 0;
        var r = await Task.FromResult(Result.Success(5)).Map(99, (state, v) => { invokeCount++; return Task.FromResult("test"); });
        Assert.Equal(1, invokeCount);
        Assert.True(r.IsSuccess);
        Assert.Equal("test", r.Value);
    }

    [Fact]
    public async Task Map_20()
    {
        int invokeCount = 0;
        var r = await Task.FromResult(Result.Failure<int>(TestError)).Map(99, (state, v) => { invokeCount++; return Task.FromResult("test"); });
        Assert.Equal(0, invokeCount);
        Assert.True(r.IsFailure);
        Assert.Same(TestError, r.Error);
    }

    [Fact]
    public async Task Map_21()
    {
        int invokeCount = 0;
        var r = await new ValueTask<Result>(Result.Success()).Map(() => { invokeCount++; return "test"; });
        Assert.Equal(1, invokeCount);
        Assert.True(r.IsSuccess);
        Assert.Equal("test", r.Value);
    }

    [Fact]
    public async Task Map_22()
    {
        int invokeCount = 0;
        var r = await new ValueTask<Result>(Result.Failure(TestError)).Map(() => { invokeCount++; return "test"; });
        Assert.Equal(0, invokeCount);
        Assert.True(r.IsFailure);
        Assert.Same(TestError, r.Error);
    }

    [Fact]
    public async Task Map_23()
    {
        int invokeCount = 0;
        var r = await new ValueTask<Result>(Result.Success()).Map(99, state => { invokeCount++; return "test"; });
        Assert.Equal(1, invokeCount);
        Assert.True(r.IsSuccess);
        Assert.Equal("test", r.Value);
    }

    [Fact]
    public async Task Map_24()
    {
        int invokeCount = 0;
        var r = await new ValueTask<Result>(Result.Failure(TestError)).Map(99, state => { invokeCount++; return "test"; });
        Assert.Equal(0, invokeCount);
        Assert.True(r.IsFailure);
        Assert.Same(TestError, r.Error);
    }

    [Fact]
    public async Task Map_25()
    {
        int invokeCount = 0;
        var r = await new ValueTask<Result<int>>(Result.Success(5)).Map(v => { invokeCount++; return "test"; });
        Assert.Equal(1, invokeCount);
        Assert.True(r.IsSuccess);
        Assert.Equal("test", r.Value);
    }

    [Fact]
    public async Task Map_26()
    {
        int invokeCount = 0;
        var r = await new ValueTask<Result<int>>(Result.Failure<int>(TestError)).Map(v => { invokeCount++; return "test"; });
        Assert.Equal(0, invokeCount);
        Assert.True(r.IsFailure);
        Assert.Same(TestError, r.Error);
    }

    [Fact]
    public async Task Map_27()
    {
        int invokeCount = 0;
        var r = await new ValueTask<Result<int>>(Result.Success(5)).Map(99, (state, v) => { invokeCount++; return "test"; });
        Assert.Equal(1, invokeCount);
        Assert.True(r.IsSuccess);
        Assert.Equal("test", r.Value);
    }

    [Fact]
    public async Task Map_28()
    {
        int invokeCount = 0;
        var r = await new ValueTask<Result<int>>(Result.Failure<int>(TestError)).Map(99, (state, v) => { invokeCount++; return "test"; });
        Assert.Equal(0, invokeCount);
        Assert.True(r.IsFailure);
        Assert.Same(TestError, r.Error);
    }

    [Fact]
    public async Task Map_29()
    {
        int invokeCount = 0;
        var r = await new ValueTask<Result<int>>(Result.Success(5)).Map(v => { invokeCount++; return new ValueTask<string>("test"); });
        Assert.Equal(1, invokeCount);
        Assert.True(r.IsSuccess);
        Assert.Equal("test", r.Value);
    }

    [Fact]
    public async Task Map_30()
    {
        int invokeCount = 0;
        var r = await new ValueTask<Result<int>>(Result.Failure<int>(TestError)).Map(v => { invokeCount++; return new ValueTask<string>("test"); });
        Assert.Equal(0, invokeCount);
        Assert.True(r.IsFailure);
        Assert.Same(TestError, r.Error);
    }

    [Fact]
    public async Task Map_31()
    {
        int invokeCount = 0;
        var r = await new ValueTask<Result<int>>(Result.Success(5)).Map(99, (state, v) => { invokeCount++; return new ValueTask<string>("test"); });
        Assert.Equal(1, invokeCount);
        Assert.True(r.IsSuccess);
        Assert.Equal("test", r.Value);
    }

    [Fact]
    public async Task Map_32()
    {
        int invokeCount = 0;
        var r = await new ValueTask<Result<int>>(Result.Failure<int>(TestError)).Map(99, (state, v) => { invokeCount++; return new ValueTask<string>("test"); });
        Assert.Equal(0, invokeCount);
        Assert.True(r.IsFailure);
        Assert.Same(TestError, r.Error);
    }
}
