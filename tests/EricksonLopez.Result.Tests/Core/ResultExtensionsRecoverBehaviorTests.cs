using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace EricksonLopez.Result.Tests.Core;

public class ResultExtensionsRecoverBehaviorTests : ResultExtensionsTestsBase
{

    [Fact]
    public void Recover_1()
    {
        int invokeCount = 0;
        var r = Result.Success().Recover((e) => { invokeCount++; return Result.Success(); });
        Assert.Equal(0, invokeCount);
        Assert.True(r.IsSuccess);
    }

    [Fact]
    public void Recover_2()
    {
        int invokeCount = 0;
        var r = Result.Failure(TestError).Recover((e) => { invokeCount++; return Result.Success(); });
        Assert.Equal(1, invokeCount);
        Assert.True(r.IsSuccess);
    }

    [Fact]
    public void Recover_3()
    {
        int invokeCount = 0;
        var r = Result.Failure(TestError).Recover((e) => { invokeCount++; return Result.Failure(TestError2); });
        Assert.Equal(1, invokeCount);
        Assert.True(r.IsFailure);
        Assert.Same(TestError2, r.Error);
    }

    [Fact]
    public void Recover_4()
    {
        int invokeCount = 0;
        var r = Result.Success().Recover(99, (state, e) => { invokeCount++; return Result.Success(); });
        Assert.Equal(0, invokeCount);
        Assert.True(r.IsSuccess);
    }

    [Fact]
    public void Recover_5()
    {
        int invokeCount = 0;
        var r = Result.Failure(TestError).Recover(99, (state, e) => { invokeCount++; return Result.Success(); });
        Assert.Equal(1, invokeCount);
        Assert.True(r.IsSuccess);
    }

    [Fact]
    public void Recover_6()
    {
        int invokeCount = 0;
        var r = Result.Failure(TestError).Recover(99, (state, e) => { invokeCount++; return Result.Failure(TestError2); });
        Assert.Equal(1, invokeCount);
        Assert.True(r.IsFailure);
        Assert.Same(TestError2, r.Error);
    }

    [Fact]
    public void Recover_7()
    {
        int invokeCount = 0;
        var r = Result.Success(5).Recover((e) => { invokeCount++; return Result.Success(5); });
        Assert.Equal(0, invokeCount);
        Assert.True(r.IsSuccess);
        Assert.Equal(5, r.Value);
    }

    [Fact]
    public void Recover_8()
    {
        int invokeCount = 0;
        var r = Result.Failure<int>(TestError).Recover((e) => { invokeCount++; return Result.Success(5); });
        Assert.Equal(1, invokeCount);
        Assert.True(r.IsSuccess);
        Assert.Equal(5, r.Value);
    }

    [Fact]
    public void Recover_9()
    {
        int invokeCount = 0;
        var r = Result.Failure<int>(TestError).Recover((e) => { invokeCount++; return Result.Failure<int>(TestError2); });
        Assert.Equal(1, invokeCount);
        Assert.True(r.IsFailure);
        Assert.Same(TestError2, r.Error);
    }

    [Fact]
    public void Recover_10()
    {
        int invokeCount = 0;
        var r = Result.Success(5).Recover(99, (state, e) => { invokeCount++; return Result.Success(5); });
        Assert.Equal(0, invokeCount);
        Assert.True(r.IsSuccess);
        Assert.Equal(5, r.Value);
    }

    [Fact]
    public void Recover_11()
    {
        int invokeCount = 0;
        var r = Result.Failure<int>(TestError).Recover(99, (state, e) => { invokeCount++; return Result.Success(5); });
        Assert.Equal(1, invokeCount);
        Assert.True(r.IsSuccess);
        Assert.Equal(5, r.Value);
    }

    [Fact]
    public void Recover_12()
    {
        int invokeCount = 0;
        var r = Result.Failure<int>(TestError).Recover(99, (state, e) => { invokeCount++; return Result.Failure<int>(TestError2); });
        Assert.Equal(1, invokeCount);
        Assert.True(r.IsFailure);
        Assert.Same(TestError2, r.Error);
    }

    [Fact]
    public async Task Recover_13()
    {
        int invokeCount = 0;
        var r = await Task.FromResult(Result.Success()).Recover((e) => { invokeCount++; return Result.Success(); });
        Assert.Equal(0, invokeCount);
        Assert.True(r.IsSuccess);
    }

    [Fact]
    public async Task Recover_14()
    {
        int invokeCount = 0;
        var r = await Task.FromResult(Result.Failure(TestError)).Recover((e) => { invokeCount++; return Result.Success(); });
        Assert.Equal(1, invokeCount);
        Assert.True(r.IsSuccess);
    }

    [Fact]
    public async Task Recover_15()
    {
        int invokeCount = 0;
        var r = await Task.FromResult(Result.Failure(TestError)).Recover((e) => { invokeCount++; return Result.Failure(TestError2); });
        Assert.Equal(1, invokeCount);
        Assert.True(r.IsFailure);
        Assert.Same(TestError2, r.Error);
    }

    [Fact]
    public async Task Recover_16()
    {
        int invokeCount = 0;
        var r = await Task.FromResult(Result.Success()).Recover(99, (state, e) => { invokeCount++; return Result.Success(); });
        Assert.Equal(0, invokeCount);
        Assert.True(r.IsSuccess);
    }

    [Fact]
    public async Task Recover_17()
    {
        int invokeCount = 0;
        var r = await Task.FromResult(Result.Failure(TestError)).Recover(99, (state, e) => { invokeCount++; return Result.Success(); });
        Assert.Equal(1, invokeCount);
        Assert.True(r.IsSuccess);
    }

    [Fact]
    public async Task Recover_18()
    {
        int invokeCount = 0;
        var r = await Task.FromResult(Result.Failure(TestError)).Recover(99, (state, e) => { invokeCount++; return Result.Failure(TestError2); });
        Assert.Equal(1, invokeCount);
        Assert.True(r.IsFailure);
        Assert.Same(TestError2, r.Error);
    }

    [Fact]
    public async Task Recover_19()
    {
        int invokeCount = 0;
        var r = await Task.FromResult(Result.Success()).Recover((e) => { invokeCount++; return Task.FromResult(Result.Success()); });
        Assert.Equal(0, invokeCount);
        Assert.True(r.IsSuccess);
    }

    [Fact]
    public async Task Recover_20()
    {
        int invokeCount = 0;
        var r = await Task.FromResult(Result.Failure(TestError)).Recover((e) => { invokeCount++; return Task.FromResult(Result.Success()); });
        Assert.Equal(1, invokeCount);
        Assert.True(r.IsSuccess);
    }

    [Fact]
    public async Task Recover_21()
    {
        int invokeCount = 0;
        var r = await Task.FromResult(Result.Failure(TestError)).Recover((e) => { invokeCount++; return Task.FromResult(Result.Failure(TestError2)); });
        Assert.Equal(1, invokeCount);
        Assert.True(r.IsFailure);
        Assert.Same(TestError2, r.Error);
    }

    [Fact]
    public async Task Recover_22()
    {
        int invokeCount = 0;
        var r = await Task.FromResult(Result.Success(5)).Recover((e) => { invokeCount++; return Result.Success(5); });
        Assert.Equal(0, invokeCount);
        Assert.True(r.IsSuccess);
        Assert.Equal(5, r.Value);
    }

    [Fact]
    public async Task Recover_23()
    {
        int invokeCount = 0;
        var r = await Task.FromResult(Result.Failure<int>(TestError)).Recover((e) => { invokeCount++; return Result.Success(5); });
        Assert.Equal(1, invokeCount);
        Assert.True(r.IsSuccess);
        Assert.Equal(5, r.Value);
    }

    [Fact]
    public async Task Recover_24()
    {
        int invokeCount = 0;
        var r = await Task.FromResult(Result.Failure<int>(TestError)).Recover((e) => { invokeCount++; return Result.Failure<int>(TestError2); });
        Assert.Equal(1, invokeCount);
        Assert.True(r.IsFailure);
        Assert.Same(TestError2, r.Error);
    }

    [Fact]
    public async Task Recover_25()
    {
        int invokeCount = 0;
        var r = await Task.FromResult(Result.Success(5)).Recover(99, (state, e) => { invokeCount++; return Result.Success(5); });
        Assert.Equal(0, invokeCount);
        Assert.True(r.IsSuccess);
        Assert.Equal(5, r.Value);
    }

    [Fact]
    public async Task Recover_26()
    {
        int invokeCount = 0;
        var r = await Task.FromResult(Result.Failure<int>(TestError)).Recover(99, (state, e) => { invokeCount++; return Result.Success(5); });
        Assert.Equal(1, invokeCount);
        Assert.True(r.IsSuccess);
        Assert.Equal(5, r.Value);
    }

    [Fact]
    public async Task Recover_27()
    {
        int invokeCount = 0;
        var r = await Task.FromResult(Result.Failure<int>(TestError)).Recover(99, (state, e) => { invokeCount++; return Result.Failure<int>(TestError2); });
        Assert.Equal(1, invokeCount);
        Assert.True(r.IsFailure);
        Assert.Same(TestError2, r.Error);
    }

    [Fact]
    public async Task Recover_28()
    {
        int invokeCount = 0;
        var r = await Task.FromResult(Result.Success(5)).Recover((e) => { invokeCount++; return Task.FromResult(Result.Success(5)); });
        Assert.Equal(0, invokeCount);
        Assert.True(r.IsSuccess);
        Assert.Equal(5, r.Value);
    }

    [Fact]
    public async Task Recover_29()
    {
        int invokeCount = 0;
        var r = await Task.FromResult(Result.Failure<int>(TestError)).Recover((e) => { invokeCount++; return Task.FromResult(Result.Success(5)); });
        Assert.Equal(1, invokeCount);
        Assert.True(r.IsSuccess);
        Assert.Equal(5, r.Value);
    }

    [Fact]
    public async Task Recover_30()
    {
        int invokeCount = 0;
        var r = await Task.FromResult(Result.Failure<int>(TestError)).Recover((e) => { invokeCount++; return Task.FromResult(Result.Failure<int>(TestError2)); });
        Assert.Equal(1, invokeCount);
        Assert.True(r.IsFailure);
        Assert.Same(TestError2, r.Error);
    }

    [Fact]
    public async Task Recover_31()
    {
        int invokeCount = 0;
        var r = await Task.FromResult(Result.Success(5)).Recover(99, (state, e) => { invokeCount++; return Task.FromResult(Result.Success(5)); });
        Assert.Equal(0, invokeCount);
        Assert.True(r.IsSuccess);
        Assert.Equal(5, r.Value);
    }

    [Fact]
    public async Task Recover_32()
    {
        int invokeCount = 0;
        var r = await Task.FromResult(Result.Failure<int>(TestError)).Recover(99, (state, e) => { invokeCount++; return Task.FromResult(Result.Success(5)); });
        Assert.Equal(1, invokeCount);
        Assert.True(r.IsSuccess);
        Assert.Equal(5, r.Value);
    }

    [Fact]
    public async Task Recover_33()
    {
        int invokeCount = 0;
        var r = await Task.FromResult(Result.Failure<int>(TestError)).Recover(99, (state, e) => { invokeCount++; return Task.FromResult(Result.Failure<int>(TestError2)); });
        Assert.Equal(1, invokeCount);
        Assert.True(r.IsFailure);
        Assert.Same(TestError2, r.Error);
    }

    [Fact]
    public async Task Recover_34()
    {
        int invokeCount = 0;
        var r = await new ValueTask<Result>(Result.Success()).Recover((e) => { invokeCount++; return Result.Success(); });
        Assert.Equal(0, invokeCount);
        Assert.True(r.IsSuccess);
    }

    [Fact]
    public async Task Recover_35()
    {
        int invokeCount = 0;
        var r = await new ValueTask<Result>(Result.Failure(TestError)).Recover((e) => { invokeCount++; return Result.Success(); });
        Assert.Equal(1, invokeCount);
        Assert.True(r.IsSuccess);
    }

    [Fact]
    public async Task Recover_36()
    {
        int invokeCount = 0;
        var r = await new ValueTask<Result>(Result.Failure(TestError)).Recover((e) => { invokeCount++; return Result.Failure(TestError2); });
        Assert.Equal(1, invokeCount);
        Assert.True(r.IsFailure);
        Assert.Same(TestError2, r.Error);
    }

    [Fact]
    public async Task Recover_37()
    {
        int invokeCount = 0;
        var r = await new ValueTask<Result>(Result.Success()).Recover(99, (state, e) => { invokeCount++; return Result.Success(); });
        Assert.Equal(0, invokeCount);
        Assert.True(r.IsSuccess);
    }

    [Fact]
    public async Task Recover_38()
    {
        int invokeCount = 0;
        var r = await new ValueTask<Result>(Result.Failure(TestError)).Recover(99, (state, e) => { invokeCount++; return Result.Success(); });
        Assert.Equal(1, invokeCount);
        Assert.True(r.IsSuccess);
    }

    [Fact]
    public async Task Recover_39()
    {
        int invokeCount = 0;
        var r = await new ValueTask<Result>(Result.Failure(TestError)).Recover(99, (state, e) => { invokeCount++; return Result.Failure(TestError2); });
        Assert.Equal(1, invokeCount);
        Assert.True(r.IsFailure);
        Assert.Same(TestError2, r.Error);
    }

    [Fact]
    public async Task Recover_40()
    {
        int invokeCount = 0;
        var r = await new ValueTask<Result>(Result.Success()).Recover((e) => { invokeCount++; return new ValueTask<Result>(Result.Success()); });
        Assert.Equal(0, invokeCount);
        Assert.True(r.IsSuccess);
    }

    [Fact]
    public async Task Recover_41()
    {
        int invokeCount = 0;
        var r = await new ValueTask<Result>(Result.Failure(TestError)).Recover((e) => { invokeCount++; return new ValueTask<Result>(Result.Success()); });
        Assert.Equal(1, invokeCount);
        Assert.True(r.IsSuccess);
    }

    [Fact]
    public async Task Recover_42()
    {
        int invokeCount = 0;
        var r = await new ValueTask<Result>(Result.Failure(TestError)).Recover((e) => { invokeCount++; return new ValueTask<Result>(Result.Failure(TestError2)); });
        Assert.Equal(1, invokeCount);
        Assert.True(r.IsFailure);
        Assert.Same(TestError2, r.Error);
    }

    [Fact]
    public async Task Recover_43()
    {
        int invokeCount = 0;
        var r = await new ValueTask<Result<int>>(Result.Success(5)).Recover((e) => { invokeCount++; return Result.Success(5); });
        Assert.Equal(0, invokeCount);
        Assert.True(r.IsSuccess);
        Assert.Equal(5, r.Value);
    }

    [Fact]
    public async Task Recover_44()
    {
        int invokeCount = 0;
        var r = await new ValueTask<Result<int>>(Result.Failure<int>(TestError)).Recover((e) => { invokeCount++; return Result.Success(5); });
        Assert.Equal(1, invokeCount);
        Assert.True(r.IsSuccess);
        Assert.Equal(5, r.Value);
    }

    [Fact]
    public async Task Recover_45()
    {
        int invokeCount = 0;
        var r = await new ValueTask<Result<int>>(Result.Failure<int>(TestError)).Recover((e) => { invokeCount++; return Result.Failure<int>(TestError2); });
        Assert.Equal(1, invokeCount);
        Assert.True(r.IsFailure);
        Assert.Same(TestError2, r.Error);
    }

    [Fact]
    public async Task Recover_46()
    {
        int invokeCount = 0;
        var r = await new ValueTask<Result<int>>(Result.Success(5)).Recover(99, (state, e) => { invokeCount++; return Result.Success(5); });
        Assert.Equal(0, invokeCount);
        Assert.True(r.IsSuccess);
        Assert.Equal(5, r.Value);
    }

    [Fact]
    public async Task Recover_47()
    {
        int invokeCount = 0;
        var r = await new ValueTask<Result<int>>(Result.Failure<int>(TestError)).Recover(99, (state, e) => { invokeCount++; return Result.Success(5); });
        Assert.Equal(1, invokeCount);
        Assert.True(r.IsSuccess);
        Assert.Equal(5, r.Value);
    }

    [Fact]
    public async Task Recover_48()
    {
        int invokeCount = 0;
        var r = await new ValueTask<Result<int>>(Result.Failure<int>(TestError)).Recover(99, (state, e) => { invokeCount++; return Result.Failure<int>(TestError2); });
        Assert.Equal(1, invokeCount);
        Assert.True(r.IsFailure);
        Assert.Same(TestError2, r.Error);
    }

    [Fact]
    public async Task Recover_49()
    {
        int invokeCount = 0;
        var r = await new ValueTask<Result<int>>(Result.Success(5)).Recover((e) => { invokeCount++; return new ValueTask<Result<int>>(Result.Success(5)); });
        Assert.Equal(0, invokeCount);
        Assert.True(r.IsSuccess);
        Assert.Equal(5, r.Value);
    }

    [Fact]
    public async Task Recover_50()
    {
        int invokeCount = 0;
        var r = await new ValueTask<Result<int>>(Result.Failure<int>(TestError)).Recover((e) => { invokeCount++; return new ValueTask<Result<int>>(Result.Success(5)); });
        Assert.Equal(1, invokeCount);
        Assert.True(r.IsSuccess);
        Assert.Equal(5, r.Value);
    }

    [Fact]
    public async Task Recover_51()
    {
        int invokeCount = 0;
        var r = await new ValueTask<Result<int>>(Result.Failure<int>(TestError)).Recover((e) => { invokeCount++; return new ValueTask<Result<int>>(Result.Failure<int>(TestError2)); });
        Assert.Equal(1, invokeCount);
        Assert.True(r.IsFailure);
        Assert.Same(TestError2, r.Error);
    }

    [Fact]
    public async Task Recover_52()
    {
        int invokeCount = 0;
        var r = await new ValueTask<Result<int>>(Result.Success(5)).Recover(99, (state, e) => { invokeCount++; return new ValueTask<Result<int>>(Result.Success(5)); });
        Assert.Equal(0, invokeCount);
        Assert.True(r.IsSuccess);
        Assert.Equal(5, r.Value);
    }

    [Fact]
    public async Task Recover_53()
    {
        int invokeCount = 0;
        var r = await new ValueTask<Result<int>>(Result.Failure<int>(TestError)).Recover(99, (state, e) => { invokeCount++; return new ValueTask<Result<int>>(Result.Success(5)); });
        Assert.Equal(1, invokeCount);
        Assert.True(r.IsSuccess);
        Assert.Equal(5, r.Value);
    }

    [Fact]
    public async Task Recover_54()
    {
        int invokeCount = 0;
        var r = await new ValueTask<Result<int>>(Result.Failure<int>(TestError)).Recover(99, (state, e) => { invokeCount++; return new ValueTask<Result<int>>(Result.Failure<int>(TestError2)); });
        Assert.Equal(1, invokeCount);
        Assert.True(r.IsFailure);
        Assert.Same(TestError2, r.Error);
    }
}
