using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace EricksonLopez.Result.Tests.Core;

public class ResultExtensionsBindBehaviorTests : ResultExtensionsTestsBase
{

    [Fact]
    public void Bind_1()
    {
        int invokeCount = 0;
        var r = Result.Success().Bind(() => { invokeCount++; return Result.Success(); });
        Assert.Equal(1, invokeCount);
        Assert.True(r.IsSuccess);
    }

    [Fact]
    public void Bind_2()
    {
        int invokeCount = 0;
        var r = Result.Failure(TestError).Bind(() => { invokeCount++; return Result.Success(); });
        Assert.Equal(0, invokeCount);
        Assert.True(r.IsFailure);
        Assert.Same(TestError, r.Error);
    }

    [Fact]
    public void Bind_3()
    {
        int invokeCount = 0;
        var r = Result.Success().Bind(() => { invokeCount++; return Result.Failure(TestError2); });
        Assert.Equal(1, invokeCount);
        Assert.True(r.IsFailure);
        Assert.Same(TestError2, r.Error);
    }

    [Fact]
    public void Bind_4()
    {
        int invokeCount = 0;
        var r = Result.Success().Bind(99, state => { invokeCount++; return Result.Success(); });
        Assert.Equal(1, invokeCount);
        Assert.True(r.IsSuccess);
    }

    [Fact]
    public void Bind_5()
    {
        int invokeCount = 0;
        var r = Result.Failure(TestError).Bind(99, state => { invokeCount++; return Result.Success(); });
        Assert.Equal(0, invokeCount);
        Assert.True(r.IsFailure);
        Assert.Same(TestError, r.Error);
    }

    [Fact]
    public void Bind_6()
    {
        int invokeCount = 0;
        var r = Result.Success().Bind(99, state => { invokeCount++; return Result.Failure(TestError2); });
        Assert.Equal(1, invokeCount);
        Assert.True(r.IsFailure);
        Assert.Same(TestError2, r.Error);
    }

    [Fact]
    public void Bind_7()
    {
        int invokeCount = 0;
        var r = Result.Success(5).Bind(v => { invokeCount++; return Result.Success("test"); });
        Assert.Equal(1, invokeCount);
        Assert.True(r.IsSuccess);
        Assert.Equal("test", r.Value);
    }

    [Fact]
    public void Bind_8()
    {
        int invokeCount = 0;
        var r = Result.Failure<int>(TestError).Bind(v => { invokeCount++; return Result.Success("test"); });
        Assert.Equal(0, invokeCount);
        Assert.True(r.IsFailure);
        Assert.Same(TestError, r.Error);
    }

    [Fact]
    public void Bind_9()
    {
        int invokeCount = 0;
        var r = Result.Success(5).Bind(v => { invokeCount++; return Result.Failure<string>(TestError2); });
        Assert.Equal(1, invokeCount);
        Assert.True(r.IsFailure);
        Assert.Same(TestError2, r.Error);
    }

    [Fact]
    public void Bind_10()
    {
        int invokeCount = 0;
        var r = Result.Success(5).Bind(99, (state, v) => { invokeCount++; return Result.Success("test"); });
        Assert.Equal(1, invokeCount);
        Assert.True(r.IsSuccess);
        Assert.Equal("test", r.Value);
    }

    [Fact]
    public void Bind_11()
    {
        int invokeCount = 0;
        var r = Result.Failure<int>(TestError).Bind(99, (state, v) => { invokeCount++; return Result.Success("test"); });
        Assert.Equal(0, invokeCount);
        Assert.True(r.IsFailure);
        Assert.Same(TestError, r.Error);
    }

    [Fact]
    public void Bind_12()
    {
        int invokeCount = 0;
        var r = Result.Success(5).Bind(99, (state, v) => { invokeCount++; return Result.Failure<string>(TestError2); });
        Assert.Equal(1, invokeCount);
        Assert.True(r.IsFailure);
        Assert.Same(TestError2, r.Error);
    }

    [Fact]
    public async Task Bind_13()
    {
        int invokeCount = 0;
        var r = await Task.FromResult(Result.Success()).Bind(() => { invokeCount++; return Result.Success(); });
        Assert.Equal(1, invokeCount);
        Assert.True(r.IsSuccess);
    }

    [Fact]
    public async Task Bind_14()
    {
        int invokeCount = 0;
        var r = await Task.FromResult(Result.Failure(TestError)).Bind(() => { invokeCount++; return Result.Success(); });
        Assert.Equal(0, invokeCount);
        Assert.True(r.IsFailure);
        Assert.Same(TestError, r.Error);
    }

    [Fact]
    public async Task Bind_15()
    {
        int invokeCount = 0;
        var r = await Task.FromResult(Result.Success()).Bind(() => { invokeCount++; return Result.Failure(TestError2); });
        Assert.Equal(1, invokeCount);
        Assert.True(r.IsFailure);
        Assert.Same(TestError2, r.Error);
    }

    [Fact]
    public async Task Bind_16()
    {
        int invokeCount = 0;
        var r = await Task.FromResult(Result.Success()).Bind(99, state => { invokeCount++; return Result.Success(); });
        Assert.Equal(1, invokeCount);
        Assert.True(r.IsSuccess);
    }

    [Fact]
    public async Task Bind_17()
    {
        int invokeCount = 0;
        var r = await Task.FromResult(Result.Failure(TestError)).Bind(99, state => { invokeCount++; return Result.Success(); });
        Assert.Equal(0, invokeCount);
        Assert.True(r.IsFailure);
        Assert.Same(TestError, r.Error);
    }

    [Fact]
    public async Task Bind_18()
    {
        int invokeCount = 0;
        var r = await Task.FromResult(Result.Success()).Bind(99, state => { invokeCount++; return Result.Failure(TestError2); });
        Assert.Equal(1, invokeCount);
        Assert.True(r.IsFailure);
        Assert.Same(TestError2, r.Error);
    }

    [Fact]
    public async Task Bind_19()
    {
        int invokeCount = 0;
        var r = await Task.FromResult(Result.Success()).Bind(() => { invokeCount++; return Task.FromResult(Result.Success()); });
        Assert.Equal(1, invokeCount);
        Assert.True(r.IsSuccess);
    }

    [Fact]
    public async Task Bind_20()
    {
        int invokeCount = 0;
        var r = await Task.FromResult(Result.Failure(TestError)).Bind(() => { invokeCount++; return Task.FromResult(Result.Success()); });
        Assert.Equal(0, invokeCount);
        Assert.True(r.IsFailure);
        Assert.Same(TestError, r.Error);
    }

    [Fact]
    public async Task Bind_21()
    {
        int invokeCount = 0;
        var r = await Task.FromResult(Result.Success()).Bind(() => { invokeCount++; return Task.FromResult(Result.Failure(TestError2)); });
        Assert.Equal(1, invokeCount);
        Assert.True(r.IsFailure);
        Assert.Same(TestError2, r.Error);
    }

    [Fact]
    public async Task Bind_22()
    {
        int invokeCount = 0;
        var r = await Task.FromResult(Result.Success(5)).Bind(v => { invokeCount++; return Result.Success("test"); });
        Assert.Equal(1, invokeCount);
        Assert.True(r.IsSuccess);
        Assert.Equal("test", r.Value);
    }

    [Fact]
    public async Task Bind_23()
    {
        int invokeCount = 0;
        var r = await Task.FromResult(Result.Failure<int>(TestError)).Bind(v => { invokeCount++; return Result.Success("test"); });
        Assert.Equal(0, invokeCount);
        Assert.True(r.IsFailure);
        Assert.Same(TestError, r.Error);
    }

    [Fact]
    public async Task Bind_24()
    {
        int invokeCount = 0;
        var r = await Task.FromResult(Result.Success(5)).Bind(v => { invokeCount++; return Result.Failure<string>(TestError2); });
        Assert.Equal(1, invokeCount);
        Assert.True(r.IsFailure);
        Assert.Same(TestError2, r.Error);
    }

    [Fact]
    public async Task Bind_25()
    {
        int invokeCount = 0;
        var r = await Task.FromResult(Result.Success(5)).Bind(99, (state, v) => { invokeCount++; return Result.Success("test"); });
        Assert.Equal(1, invokeCount);
        Assert.True(r.IsSuccess);
        Assert.Equal("test", r.Value);
    }

    [Fact]
    public async Task Bind_26()
    {
        int invokeCount = 0;
        var r = await Task.FromResult(Result.Failure<int>(TestError)).Bind(99, (state, v) => { invokeCount++; return Result.Success("test"); });
        Assert.Equal(0, invokeCount);
        Assert.True(r.IsFailure);
        Assert.Same(TestError, r.Error);
    }

    [Fact]
    public async Task Bind_27()
    {
        int invokeCount = 0;
        var r = await Task.FromResult(Result.Success(5)).Bind(99, (state, v) => { invokeCount++; return Result.Failure<string>(TestError2); });
        Assert.Equal(1, invokeCount);
        Assert.True(r.IsFailure);
        Assert.Same(TestError2, r.Error);
    }

    [Fact]
    public async Task Bind_28()
    {
        int invokeCount = 0;
        var r = await Task.FromResult(Result.Success(5)).Bind(v => { invokeCount++; return Task.FromResult(Result.Success("test")); });
        Assert.Equal(1, invokeCount);
        Assert.True(r.IsSuccess);
        Assert.Equal("test", r.Value);
    }

    [Fact]
    public async Task Bind_29()
    {
        int invokeCount = 0;
        var r = await Task.FromResult(Result.Failure<int>(TestError)).Bind(v => { invokeCount++; return Task.FromResult(Result.Success("test")); });
        Assert.Equal(0, invokeCount);
        Assert.True(r.IsFailure);
        Assert.Same(TestError, r.Error);
    }

    [Fact]
    public async Task Bind_30()
    {
        int invokeCount = 0;
        var r = await Task.FromResult(Result.Success(5)).Bind(v => { invokeCount++; return Task.FromResult(Result.Failure<string>(TestError2)); });
        Assert.Equal(1, invokeCount);
        Assert.True(r.IsFailure);
        Assert.Same(TestError2, r.Error);
    }

    [Fact]
    public async Task Bind_31()
    {
        int invokeCount = 0;
        var r = await new ValueTask<Result>(Result.Success()).Bind(() => { invokeCount++; return Result.Success(); });
        Assert.Equal(1, invokeCount);
        Assert.True(r.IsSuccess);
    }

    [Fact]
    public async Task Bind_32()
    {
        int invokeCount = 0;
        var r = await new ValueTask<Result>(Result.Failure(TestError)).Bind(() => { invokeCount++; return Result.Success(); });
        Assert.Equal(0, invokeCount);
        Assert.True(r.IsFailure);
        Assert.Same(TestError, r.Error);
    }

    [Fact]
    public async Task Bind_33()
    {
        int invokeCount = 0;
        var r = await new ValueTask<Result>(Result.Success()).Bind(() => { invokeCount++; return Result.Failure(TestError2); });
        Assert.Equal(1, invokeCount);
        Assert.True(r.IsFailure);
        Assert.Same(TestError2, r.Error);
    }

    [Fact]
    public async Task Bind_34()
    {
        int invokeCount = 0;
        var r = await new ValueTask<Result>(Result.Success()).Bind(99, state => { invokeCount++; return Result.Success(); });
        Assert.Equal(1, invokeCount);
        Assert.True(r.IsSuccess);
    }

    [Fact]
    public async Task Bind_35()
    {
        int invokeCount = 0;
        var r = await new ValueTask<Result>(Result.Failure(TestError)).Bind(99, state => { invokeCount++; return Result.Success(); });
        Assert.Equal(0, invokeCount);
        Assert.True(r.IsFailure);
        Assert.Same(TestError, r.Error);
    }

    [Fact]
    public async Task Bind_36()
    {
        int invokeCount = 0;
        var r = await new ValueTask<Result>(Result.Success()).Bind(99, state => { invokeCount++; return Result.Failure(TestError2); });
        Assert.Equal(1, invokeCount);
        Assert.True(r.IsFailure);
        Assert.Same(TestError2, r.Error);
    }

    [Fact]
    public async Task Bind_37()
    {
        int invokeCount = 0;
        var r = await new ValueTask<Result>(Result.Success()).Bind(() => { invokeCount++; return new ValueTask<Result>(Result.Success()); });
        Assert.Equal(1, invokeCount);
        Assert.True(r.IsSuccess);
    }

    [Fact]
    public async Task Bind_38()
    {
        int invokeCount = 0;
        var r = await new ValueTask<Result>(Result.Failure(TestError)).Bind(() => { invokeCount++; return new ValueTask<Result>(Result.Success()); });
        Assert.Equal(0, invokeCount);
        Assert.True(r.IsFailure);
        Assert.Same(TestError, r.Error);
    }

    [Fact]
    public async Task Bind_39()
    {
        int invokeCount = 0;
        var r = await new ValueTask<Result>(Result.Success()).Bind(() => { invokeCount++; return new ValueTask<Result>(Result.Failure(TestError2)); });
        Assert.Equal(1, invokeCount);
        Assert.True(r.IsFailure);
        Assert.Same(TestError2, r.Error);
    }

    [Fact]
    public async Task Bind_40()
    {
        int invokeCount = 0;
        var r = await new ValueTask<Result<int>>(Result.Success(5)).Bind(v => { invokeCount++; return Result.Success("test"); });
        Assert.Equal(1, invokeCount);
        Assert.True(r.IsSuccess);
        Assert.Equal("test", r.Value);
    }

    [Fact]
    public async Task Bind_41()
    {
        int invokeCount = 0;
        var r = await new ValueTask<Result<int>>(Result.Failure<int>(TestError)).Bind(v => { invokeCount++; return Result.Success("test"); });
        Assert.Equal(0, invokeCount);
        Assert.True(r.IsFailure);
        Assert.Same(TestError, r.Error);
    }

    [Fact]
    public async Task Bind_42()
    {
        int invokeCount = 0;
        var r = await new ValueTask<Result<int>>(Result.Success(5)).Bind(v => { invokeCount++; return Result.Failure<string>(TestError2); });
        Assert.Equal(1, invokeCount);
        Assert.True(r.IsFailure);
        Assert.Same(TestError2, r.Error);
    }

    [Fact]
    public async Task Bind_43()
    {
        int invokeCount = 0;
        var r = await new ValueTask<Result<int>>(Result.Success(5)).Bind(99, (state, v) => { invokeCount++; return Result.Success("test"); });
        Assert.Equal(1, invokeCount);
        Assert.True(r.IsSuccess);
        Assert.Equal("test", r.Value);
    }

    [Fact]
    public async Task Bind_44()
    {
        int invokeCount = 0;
        var r = await new ValueTask<Result<int>>(Result.Failure<int>(TestError)).Bind(99, (state, v) => { invokeCount++; return Result.Success("test"); });
        Assert.Equal(0, invokeCount);
        Assert.True(r.IsFailure);
        Assert.Same(TestError, r.Error);
    }

    [Fact]
    public async Task Bind_45()
    {
        int invokeCount = 0;
        var r = await new ValueTask<Result<int>>(Result.Success(5)).Bind(99, (state, v) => { invokeCount++; return Result.Failure<string>(TestError2); });
        Assert.Equal(1, invokeCount);
        Assert.True(r.IsFailure);
        Assert.Same(TestError2, r.Error);
    }

    [Fact]
    public async Task Bind_46()
    {
        int invokeCount = 0;
        var r = await new ValueTask<Result<int>>(Result.Success(5)).Bind(v => { invokeCount++; return new ValueTask<Result<string>>(Result.Success("test")); });
        Assert.Equal(1, invokeCount);
        Assert.True(r.IsSuccess);
        Assert.Equal("test", r.Value);
    }

    [Fact]
    public async Task Bind_47()
    {
        int invokeCount = 0;
        var r = await new ValueTask<Result<int>>(Result.Failure<int>(TestError)).Bind(v => { invokeCount++; return new ValueTask<Result<string>>(Result.Success("test")); });
        Assert.Equal(0, invokeCount);
        Assert.True(r.IsFailure);
        Assert.Same(TestError, r.Error);
    }

    [Fact]
    public async Task Bind_48()
    {
        int invokeCount = 0;
        var r = await new ValueTask<Result<int>>(Result.Success(5)).Bind(v => { invokeCount++; return new ValueTask<Result<string>>(Result.Failure<string>(TestError2)); });
        Assert.Equal(1, invokeCount);
        Assert.True(r.IsFailure);
        Assert.Same(TestError2, r.Error);
    }
}
