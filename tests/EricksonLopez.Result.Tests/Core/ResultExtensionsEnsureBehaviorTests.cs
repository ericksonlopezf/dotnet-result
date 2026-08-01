using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace EricksonLopez.Result.Tests.Core;

public class ResultExtensionsEnsureBehaviorTests : ResultExtensionsTestsBase
{

    [Fact]
    public void Ensure_1()
    {
        int invokeCount = 0;
        var r = Result.Success().Ensure(() => { invokeCount++; return true; }, TestError2);
        Assert.Equal(1, invokeCount);
        Assert.True(r.IsSuccess);
    }

    [Fact]
    public void Ensure_2()
    {
        int invokeCount = 0;
        var r = Result.Failure(TestError).Ensure(() => { invokeCount++; return true; }, TestError2);
        Assert.Equal(0, invokeCount);
        Assert.True(r.IsFailure);
        Assert.Same(TestError, r.Error);
    }

    [Fact]
    public void Ensure_3()
    {
        int invokeCount = 0;
        var r = Result.Success().Ensure(() => { invokeCount++; return false; }, TestError2);
        Assert.Equal(1, invokeCount);
        Assert.True(r.IsFailure);
        Assert.Same(TestError2, r.Error);
    }

    [Fact]
    public void Ensure_4()
    {
        int invokeCount = 0;
        var r = Result.Success().Ensure(99, state => { invokeCount++; return true; }, TestError2);
        Assert.Equal(1, invokeCount);
        Assert.True(r.IsSuccess);
    }

    [Fact]
    public void Ensure_5()
    {
        int invokeCount = 0;
        var r = Result.Failure(TestError).Ensure(99, state => { invokeCount++; return true; }, TestError2);
        Assert.Equal(0, invokeCount);
        Assert.True(r.IsFailure);
        Assert.Same(TestError, r.Error);
    }

    [Fact]
    public void Ensure_6()
    {
        int invokeCount = 0;
        var r = Result.Success().Ensure(99, state => { invokeCount++; return false; }, TestError2);
        Assert.Equal(1, invokeCount);
        Assert.True(r.IsFailure);
        Assert.Same(TestError2, r.Error);
    }

    [Fact]
    public void Ensure_7()
    {
        int invokeCount = 0;
        var r = Result.Success(5).Ensure(v => { invokeCount++; return true; }, TestError2);
        Assert.Equal(1, invokeCount);
        Assert.True(r.IsSuccess);
        Assert.Equal(5, r.Value);
    }

    [Fact]
    public void Ensure_8()
    {
        int invokeCount = 0;
        var r = Result.Failure<int>(TestError).Ensure(v => { invokeCount++; return true; }, TestError2);
        Assert.Equal(0, invokeCount);
        Assert.True(r.IsFailure);
        Assert.Same(TestError, r.Error);
    }

    [Fact]
    public void Ensure_9()
    {
        int invokeCount = 0;
        var r = Result.Success(5).Ensure(v => { invokeCount++; return false; }, TestError2);
        Assert.Equal(1, invokeCount);
        Assert.True(r.IsFailure);
        Assert.Same(TestError2, r.Error);
    }

    [Fact]
    public void Ensure_10()
    {
        int invokeCount = 0;
        var r = Result.Success(5).Ensure(99, (state, v) => { invokeCount++; return true; }, TestError2);
        Assert.Equal(1, invokeCount);
        Assert.True(r.IsSuccess);
        Assert.Equal(5, r.Value);
    }

    [Fact]
    public void Ensure_11()
    {
        int invokeCount = 0;
        var r = Result.Failure<int>(TestError).Ensure(99, (state, v) => { invokeCount++; return true; }, TestError2);
        Assert.Equal(0, invokeCount);
        Assert.True(r.IsFailure);
        Assert.Same(TestError, r.Error);
    }

    [Fact]
    public void Ensure_12()
    {
        int invokeCount = 0;
        var r = Result.Success(5).Ensure(99, (state, v) => { invokeCount++; return false; }, TestError2);
        Assert.Equal(1, invokeCount);
        Assert.True(r.IsFailure);
        Assert.Same(TestError2, r.Error);
    }

    [Fact]
    public async Task Ensure_13()
    {
        int invokeCount = 0;
        var r = await Task.FromResult(Result.Success()).Ensure(() => { invokeCount++; return true; }, TestError2);
        Assert.Equal(1, invokeCount);
        Assert.True(r.IsSuccess);
    }

    [Fact]
    public async Task Ensure_14()
    {
        int invokeCount = 0;
        var r = await Task.FromResult(Result.Failure(TestError)).Ensure(() => { invokeCount++; return true; }, TestError2);
        Assert.Equal(0, invokeCount);
        Assert.True(r.IsFailure);
        Assert.Same(TestError, r.Error);
    }

    [Fact]
    public async Task Ensure_15()
    {
        int invokeCount = 0;
        var r = await Task.FromResult(Result.Success()).Ensure(() => { invokeCount++; return false; }, TestError2);
        Assert.Equal(1, invokeCount);
        Assert.True(r.IsFailure);
        Assert.Same(TestError2, r.Error);
    }

    [Fact]
    public async Task Ensure_16()
    {
        int invokeCount = 0;
        var r = await Task.FromResult(Result.Success()).Ensure(99, state => { invokeCount++; return true; }, TestError2);
        Assert.Equal(1, invokeCount);
        Assert.True(r.IsSuccess);
    }

    [Fact]
    public async Task Ensure_17()
    {
        int invokeCount = 0;
        var r = await Task.FromResult(Result.Failure(TestError)).Ensure(99, state => { invokeCount++; return true; }, TestError2);
        Assert.Equal(0, invokeCount);
        Assert.True(r.IsFailure);
        Assert.Same(TestError, r.Error);
    }

    [Fact]
    public async Task Ensure_18()
    {
        int invokeCount = 0;
        var r = await Task.FromResult(Result.Success()).Ensure(99, state => { invokeCount++; return false; }, TestError2);
        Assert.Equal(1, invokeCount);
        Assert.True(r.IsFailure);
        Assert.Same(TestError2, r.Error);
    }

    [Fact]
    public async Task Ensure_19()
    {
        int invokeCount = 0;
        var r = await Task.FromResult(Result.Success(5)).Ensure(v => { invokeCount++; return true; }, TestError2);
        Assert.Equal(1, invokeCount);
        Assert.True(r.IsSuccess);
        Assert.Equal(5, r.Value);
    }

    [Fact]
    public async Task Ensure_20()
    {
        int invokeCount = 0;
        var r = await Task.FromResult(Result.Failure<int>(TestError)).Ensure(v => { invokeCount++; return true; }, TestError2);
        Assert.Equal(0, invokeCount);
        Assert.True(r.IsFailure);
        Assert.Same(TestError, r.Error);
    }

    [Fact]
    public async Task Ensure_21()
    {
        int invokeCount = 0;
        var r = await Task.FromResult(Result.Success(5)).Ensure(v => { invokeCount++; return false; }, TestError2);
        Assert.Equal(1, invokeCount);
        Assert.True(r.IsFailure);
        Assert.Same(TestError2, r.Error);
    }

    [Fact]
    public async Task Ensure_22()
    {
        int invokeCount = 0;
        var r = await Task.FromResult(Result.Success(5)).Ensure(99, (state, v) => { invokeCount++; return true; }, TestError2);
        Assert.Equal(1, invokeCount);
        Assert.True(r.IsSuccess);
        Assert.Equal(5, r.Value);
    }

    [Fact]
    public async Task Ensure_23()
    {
        int invokeCount = 0;
        var r = await Task.FromResult(Result.Failure<int>(TestError)).Ensure(99, (state, v) => { invokeCount++; return true; }, TestError2);
        Assert.Equal(0, invokeCount);
        Assert.True(r.IsFailure);
        Assert.Same(TestError, r.Error);
    }

    [Fact]
    public async Task Ensure_24()
    {
        int invokeCount = 0;
        var r = await Task.FromResult(Result.Success(5)).Ensure(99, (state, v) => { invokeCount++; return false; }, TestError2);
        Assert.Equal(1, invokeCount);
        Assert.True(r.IsFailure);
        Assert.Same(TestError2, r.Error);
    }

    [Fact]
    public async Task Ensure_25()
    {
        int invokeCount = 0;
        var r = await Task.FromResult(Result.Success(5)).Ensure(v => { invokeCount++; return Task.FromResult(true); }, TestError2);
        Assert.Equal(1, invokeCount);
        Assert.True(r.IsSuccess);
        Assert.Equal(5, r.Value);
    }

    [Fact]
    public async Task Ensure_26()
    {
        int invokeCount = 0;
        var r = await Task.FromResult(Result.Failure<int>(TestError)).Ensure(v => { invokeCount++; return Task.FromResult(true); }, TestError2);
        Assert.Equal(0, invokeCount);
        Assert.True(r.IsFailure);
        Assert.Same(TestError, r.Error);
    }

    [Fact]
    public async Task Ensure_27()
    {
        int invokeCount = 0;
        var r = await Task.FromResult(Result.Success(5)).Ensure(v => { invokeCount++; return Task.FromResult(false); }, TestError2);
        Assert.Equal(1, invokeCount);
        Assert.True(r.IsFailure);
        Assert.Same(TestError2, r.Error);
    }

    [Fact]
    public async Task Ensure_28()
    {
        int invokeCount = 0;
        var r = await new ValueTask<Result>(Result.Success()).Ensure(() => { invokeCount++; return true; }, TestError2);
        Assert.Equal(1, invokeCount);
        Assert.True(r.IsSuccess);
    }

    [Fact]
    public async Task Ensure_29()
    {
        int invokeCount = 0;
        var r = await new ValueTask<Result>(Result.Failure(TestError)).Ensure(() => { invokeCount++; return true; }, TestError2);
        Assert.Equal(0, invokeCount);
        Assert.True(r.IsFailure);
        Assert.Same(TestError, r.Error);
    }

    [Fact]
    public async Task Ensure_30()
    {
        int invokeCount = 0;
        var r = await new ValueTask<Result>(Result.Success()).Ensure(() => { invokeCount++; return false; }, TestError2);
        Assert.Equal(1, invokeCount);
        Assert.True(r.IsFailure);
        Assert.Same(TestError2, r.Error);
    }

    [Fact]
    public async Task Ensure_31()
    {
        int invokeCount = 0;
        var r = await new ValueTask<Result>(Result.Success()).Ensure(99, state => { invokeCount++; return true; }, TestError2);
        Assert.Equal(1, invokeCount);
        Assert.True(r.IsSuccess);
    }

    [Fact]
    public async Task Ensure_32()
    {
        int invokeCount = 0;
        var r = await new ValueTask<Result>(Result.Failure(TestError)).Ensure(99, state => { invokeCount++; return true; }, TestError2);
        Assert.Equal(0, invokeCount);
        Assert.True(r.IsFailure);
        Assert.Same(TestError, r.Error);
    }

    [Fact]
    public async Task Ensure_33()
    {
        int invokeCount = 0;
        var r = await new ValueTask<Result>(Result.Success()).Ensure(99, state => { invokeCount++; return false; }, TestError2);
        Assert.Equal(1, invokeCount);
        Assert.True(r.IsFailure);
        Assert.Same(TestError2, r.Error);
    }

    [Fact]
    public async Task Ensure_34()
    {
        int invokeCount = 0;
        var r = await new ValueTask<Result<int>>(Result.Success(5)).Ensure(v => { invokeCount++; return true; }, TestError2);
        Assert.Equal(1, invokeCount);
        Assert.True(r.IsSuccess);
        Assert.Equal(5, r.Value);
    }

    [Fact]
    public async Task Ensure_35()
    {
        int invokeCount = 0;
        var r = await new ValueTask<Result<int>>(Result.Failure<int>(TestError)).Ensure(v => { invokeCount++; return true; }, TestError2);
        Assert.Equal(0, invokeCount);
        Assert.True(r.IsFailure);
        Assert.Same(TestError, r.Error);
    }

    [Fact]
    public async Task Ensure_36()
    {
        int invokeCount = 0;
        var r = await new ValueTask<Result<int>>(Result.Success(5)).Ensure(v => { invokeCount++; return false; }, TestError2);
        Assert.Equal(1, invokeCount);
        Assert.True(r.IsFailure);
        Assert.Same(TestError2, r.Error);
    }

    [Fact]
    public async Task Ensure_37()
    {
        int invokeCount = 0;
        var r = await new ValueTask<Result<int>>(Result.Success(5)).Ensure(99, (state, v) => { invokeCount++; return true; }, TestError2);
        Assert.Equal(1, invokeCount);
        Assert.True(r.IsSuccess);
        Assert.Equal(5, r.Value);
    }

    [Fact]
    public async Task Ensure_38()
    {
        int invokeCount = 0;
        var r = await new ValueTask<Result<int>>(Result.Failure<int>(TestError)).Ensure(99, (state, v) => { invokeCount++; return true; }, TestError2);
        Assert.Equal(0, invokeCount);
        Assert.True(r.IsFailure);
        Assert.Same(TestError, r.Error);
    }

    [Fact]
    public async Task Ensure_39()
    {
        int invokeCount = 0;
        var r = await new ValueTask<Result<int>>(Result.Success(5)).Ensure(99, (state, v) => { invokeCount++; return false; }, TestError2);
        Assert.Equal(1, invokeCount);
        Assert.True(r.IsFailure);
        Assert.Same(TestError2, r.Error);
    }
}
