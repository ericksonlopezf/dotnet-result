using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace EricksonLopez.Result.Tests.Core;

public class ResultExtensionsTapBehaviorTests : ResultExtensionsTestsBase
{

    [Fact]
    public void Tap_1()
    {
        int invokeCount = 0;
        var r = Result.Success().TapOnSuccess(() => { invokeCount++; });
        Assert.Equal(1, invokeCount);
        Assert.True(r.IsSuccess);
    }

    [Fact]
    public void Tap_2()
    {
        int invokeCount = 0;
        var r = Result.Failure(TestError).TapOnSuccess(() => { invokeCount++; });
        Assert.Equal(0, invokeCount);
        Assert.True(r.IsFailure);
        Assert.Same(TestError, r.Error);
    }

    [Fact]
    public void Tap_3()
    {
        int invokeCount = 0;
        var r = Result.Success().TapOnSuccess(99, state => { invokeCount++; });
        Assert.Equal(1, invokeCount);
        Assert.True(r.IsSuccess);
    }

    [Fact]
    public void Tap_4()
    {
        int invokeCount = 0;
        var r = Result.Failure(TestError).TapOnSuccess(99, state => { invokeCount++; });
        Assert.Equal(0, invokeCount);
        Assert.True(r.IsFailure);
        Assert.Same(TestError, r.Error);
    }

    [Fact]
    public void Tap_5()
    {
        int invokeCount = 0;
        var r = Result.Success(5).TapOnSuccess(v => { invokeCount++; });
        Assert.Equal(1, invokeCount);
        Assert.True(r.IsSuccess);
        Assert.Equal(5, r.Value);
    }

    [Fact]
    public void Tap_6()
    {
        int invokeCount = 0;
        var r = Result.Failure<int>(TestError).TapOnSuccess(v => { invokeCount++; });
        Assert.Equal(0, invokeCount);
        Assert.True(r.IsFailure);
        Assert.Same(TestError, r.Error);
    }

    [Fact]
    public void Tap_7()
    {
        int invokeCount = 0;
        var r = Result.Success(5).TapOnSuccess(99, (state, v) => { invokeCount++; });
        Assert.Equal(1, invokeCount);
        Assert.True(r.IsSuccess);
        Assert.Equal(5, r.Value);
    }

    [Fact]
    public void Tap_8()
    {
        int invokeCount = 0;
        var r = Result.Failure<int>(TestError).TapOnSuccess(99, (state, v) => { invokeCount++; });
        Assert.Equal(0, invokeCount);
        Assert.True(r.IsFailure);
        Assert.Same(TestError, r.Error);
    }

    [Fact]
    public async Task Tap_9()
    {
        int invokeCount = 0;
        var r = await Task.FromResult(Result.Success()).TapOnSuccess(() => { invokeCount++; });
        Assert.Equal(1, invokeCount);
        Assert.True(r.IsSuccess);
    }

    [Fact]
    public async Task Tap_10()
    {
        int invokeCount = 0;
        var r = await Task.FromResult(Result.Failure(TestError)).TapOnSuccess(() => { invokeCount++; });
        Assert.Equal(0, invokeCount);
        Assert.True(r.IsFailure);
        Assert.Same(TestError, r.Error);
    }

    [Fact]
    public async Task Tap_11()
    {
        int invokeCount = 0;
        var r = await Task.FromResult(Result.Success()).TapOnSuccess(99, state => { invokeCount++; });
        Assert.Equal(1, invokeCount);
        Assert.True(r.IsSuccess);
    }

    [Fact]
    public async Task Tap_12()
    {
        int invokeCount = 0;
        var r = await Task.FromResult(Result.Failure(TestError)).TapOnSuccess(99, state => { invokeCount++; });
        Assert.Equal(0, invokeCount);
        Assert.True(r.IsFailure);
        Assert.Same(TestError, r.Error);
    }

    [Fact]
    public async Task Tap_13()
    {
        int invokeCount = 0;
        var r = await Task.FromResult(Result.Success()).TapOnSuccess(() => { invokeCount++; return Task.CompletedTask; });
        Assert.Equal(1, invokeCount);
        Assert.True(r.IsSuccess);
    }

    [Fact]
    public async Task Tap_14()
    {
        int invokeCount = 0;
        var r = await Task.FromResult(Result.Failure(TestError)).TapOnSuccess(() => { invokeCount++; return Task.CompletedTask; });
        Assert.Equal(0, invokeCount);
        Assert.True(r.IsFailure);
        Assert.Same(TestError, r.Error);
    }

    [Fact]
    public async Task Tap_15()
    {
        int invokeCount = 0;
        var r = await Task.FromResult(Result.Success(5)).TapOnSuccess(v => { invokeCount++; });
        Assert.Equal(1, invokeCount);
        Assert.True(r.IsSuccess);
        Assert.Equal(5, r.Value);
    }

    [Fact]
    public async Task Tap_16()
    {
        int invokeCount = 0;
        var r = await Task.FromResult(Result.Failure<int>(TestError)).TapOnSuccess(v => { invokeCount++; });
        Assert.Equal(0, invokeCount);
        Assert.True(r.IsFailure);
        Assert.Same(TestError, r.Error);
    }

    [Fact]
    public async Task Tap_17()
    {
        int invokeCount = 0;
        var r = await Task.FromResult(Result.Success(5)).TapOnSuccess(99, (state, v) => { invokeCount++; });
        Assert.Equal(1, invokeCount);
        Assert.True(r.IsSuccess);
        Assert.Equal(5, r.Value);
    }

    [Fact]
    public async Task Tap_18()
    {
        int invokeCount = 0;
        var r = await Task.FromResult(Result.Failure<int>(TestError)).TapOnSuccess(99, (state, v) => { invokeCount++; });
        Assert.Equal(0, invokeCount);
        Assert.True(r.IsFailure);
        Assert.Same(TestError, r.Error);
    }

    [Fact]
    public async Task Tap_19()
    {
        int invokeCount = 0;
        var r = await Task.FromResult(Result.Success(5)).TapOnSuccess(v => { invokeCount++; return Task.CompletedTask; });
        Assert.Equal(1, invokeCount);
        Assert.True(r.IsSuccess);
        Assert.Equal(5, r.Value);
    }

    [Fact]
    public async Task Tap_20()
    {
        int invokeCount = 0;
        var r = await Task.FromResult(Result.Failure<int>(TestError)).TapOnSuccess(v => { invokeCount++; return Task.CompletedTask; });
        Assert.Equal(0, invokeCount);
        Assert.True(r.IsFailure);
        Assert.Same(TestError, r.Error);
    }

    [Fact]
    public async Task Tap_21()
    {
        int invokeCount = 0;
        var r = await new ValueTask<Result>(Result.Success()).TapOnSuccess(() => { invokeCount++; });
        Assert.Equal(1, invokeCount);
        Assert.True(r.IsSuccess);
    }

    [Fact]
    public async Task Tap_22()
    {
        int invokeCount = 0;
        var r = await new ValueTask<Result>(Result.Failure(TestError)).TapOnSuccess(() => { invokeCount++; });
        Assert.Equal(0, invokeCount);
        Assert.True(r.IsFailure);
        Assert.Same(TestError, r.Error);
    }

    [Fact]
    public async Task Tap_23()
    {
        int invokeCount = 0;
        var r = await new ValueTask<Result>(Result.Success()).TapOnSuccess(99, state => { invokeCount++; });
        Assert.Equal(1, invokeCount);
        Assert.True(r.IsSuccess);
    }

    [Fact]
    public async Task Tap_24()
    {
        int invokeCount = 0;
        var r = await new ValueTask<Result>(Result.Failure(TestError)).TapOnSuccess(99, state => { invokeCount++; });
        Assert.Equal(0, invokeCount);
        Assert.True(r.IsFailure);
        Assert.Same(TestError, r.Error);
    }

    [Fact]
    public async Task Tap_25()
    {
        int invokeCount = 0;
        var r = await new ValueTask<Result>(Result.Success()).TapOnSuccess(() => { invokeCount++; return default(ValueTask); });
        Assert.Equal(1, invokeCount);
        Assert.True(r.IsSuccess);
    }

    [Fact]
    public async Task Tap_26()
    {
        int invokeCount = 0;
        var r = await new ValueTask<Result>(Result.Failure(TestError)).TapOnSuccess(() => { invokeCount++; return default(ValueTask); });
        Assert.Equal(0, invokeCount);
        Assert.True(r.IsFailure);
        Assert.Same(TestError, r.Error);
    }

    [Fact]
    public async Task Tap_27()
    {
        int invokeCount = 0;
        var r = await new ValueTask<Result<int>>(Result.Success(5)).TapOnSuccess(v => { invokeCount++; });
        Assert.Equal(1, invokeCount);
        Assert.True(r.IsSuccess);
        Assert.Equal(5, r.Value);
    }

    [Fact]
    public async Task Tap_28()
    {
        int invokeCount = 0;
        var r = await new ValueTask<Result<int>>(Result.Failure<int>(TestError)).TapOnSuccess(v => { invokeCount++; });
        Assert.Equal(0, invokeCount);
        Assert.True(r.IsFailure);
        Assert.Same(TestError, r.Error);
    }

    [Fact]
    public async Task Tap_29()
    {
        int invokeCount = 0;
        var r = await new ValueTask<Result<int>>(Result.Success(5)).TapOnSuccess(99, (state, v) => { invokeCount++; });
        Assert.Equal(1, invokeCount);
        Assert.True(r.IsSuccess);
        Assert.Equal(5, r.Value);
    }

    [Fact]
    public async Task Tap_30()
    {
        int invokeCount = 0;
        var r = await new ValueTask<Result<int>>(Result.Failure<int>(TestError)).TapOnSuccess(99, (state, v) => { invokeCount++; });
        Assert.Equal(0, invokeCount);
        Assert.True(r.IsFailure);
        Assert.Same(TestError, r.Error);
    }

    [Fact]
    public async Task Tap_31()
    {
        int invokeCount = 0;
        var r = await new ValueTask<Result<int>>(Result.Success(5)).TapOnSuccess(v => { invokeCount++; return default(ValueTask); });
        Assert.Equal(1, invokeCount);
        Assert.True(r.IsSuccess);
        Assert.Equal(5, r.Value);
    }

    [Fact]
    public async Task Tap_32()
    {
        int invokeCount = 0;
        var r = await new ValueTask<Result<int>>(Result.Failure<int>(TestError)).TapOnSuccess(v => { invokeCount++; return default(ValueTask); });
        Assert.Equal(0, invokeCount);
        Assert.True(r.IsFailure);
        Assert.Same(TestError, r.Error);
    }
}
