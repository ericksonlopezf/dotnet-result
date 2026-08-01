using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace EricksonLopez.Result.Tests.Core;

public class ResultExtensionsMapErrorBehaviorTests : ResultExtensionsTestsBase
{

    [Fact]
    public void MapError_1()
    {
        int invokeCount = 0;
        var r = Result.Success().MapError((e) => { invokeCount++; return TestError2; });
        Assert.Equal(0, invokeCount);
        Assert.True(r.IsSuccess);
    }

    [Fact]
    public void MapError_2()
    {
        int invokeCount = 0;
        var r = Result.Failure(TestError).MapError((e) => { invokeCount++; return TestError2; });
        Assert.Equal(1, invokeCount);
        Assert.True(r.IsFailure);
        Assert.Same(TestError2, r.Error);
    }

    [Fact]
    public void MapError_3()
    {
        int invokeCount = 0;
        var r = Result.Success().MapError(99, (state, e) => { invokeCount++; return TestError2; });
        Assert.Equal(0, invokeCount);
        Assert.True(r.IsSuccess);
    }

    [Fact]
    public void MapError_4()
    {
        int invokeCount = 0;
        var r = Result.Failure(TestError).MapError(99, (state, e) => { invokeCount++; return TestError2; });
        Assert.Equal(1, invokeCount);
        Assert.True(r.IsFailure);
        Assert.Same(TestError2, r.Error);
    }

    [Fact]
    public void MapError_5()
    {
        int invokeCount = 0;
        var r = Result.Success(5).MapError((e) => { invokeCount++; return TestError2; });
        Assert.Equal(0, invokeCount);
        Assert.True(r.IsSuccess);
        Assert.Equal(5, r.Value);
    }

    [Fact]
    public void MapError_6()
    {
        int invokeCount = 0;
        var r = Result.Failure<int>(TestError).MapError((e) => { invokeCount++; return TestError2; });
        Assert.Equal(1, invokeCount);
        Assert.True(r.IsFailure);
        Assert.Same(TestError2, r.Error);
    }

    [Fact]
    public void MapError_7()
    {
        int invokeCount = 0;
        var r = Result.Success(5).MapError(99, (state, e) => { invokeCount++; return TestError2; });
        Assert.Equal(0, invokeCount);
        Assert.True(r.IsSuccess);
        Assert.Equal(5, r.Value);
    }

    [Fact]
    public void MapError_8()
    {
        int invokeCount = 0;
        var r = Result.Failure<int>(TestError).MapError(99, (state, e) => { invokeCount++; return TestError2; });
        Assert.Equal(1, invokeCount);
        Assert.True(r.IsFailure);
        Assert.Same(TestError2, r.Error);
    }

    [Fact]
    public async Task MapError_9()
    {
        int invokeCount = 0;
        var r = await Task.FromResult(Result.Success()).MapError((e) => { invokeCount++; return TestError2; });
        Assert.Equal(0, invokeCount);
        Assert.True(r.IsSuccess);
    }

    [Fact]
    public async Task MapError_10()
    {
        int invokeCount = 0;
        var r = await Task.FromResult(Result.Failure(TestError)).MapError((e) => { invokeCount++; return TestError2; });
        Assert.Equal(1, invokeCount);
        Assert.True(r.IsFailure);
        Assert.Same(TestError2, r.Error);
    }

    [Fact]
    public async Task MapError_11()
    {
        int invokeCount = 0;
        var r = await Task.FromResult(Result.Success()).MapError(99, (state, e) => { invokeCount++; return TestError2; });
        Assert.Equal(0, invokeCount);
        Assert.True(r.IsSuccess);
    }

    [Fact]
    public async Task MapError_12()
    {
        int invokeCount = 0;
        var r = await Task.FromResult(Result.Failure(TestError)).MapError(99, (state, e) => { invokeCount++; return TestError2; });
        Assert.Equal(1, invokeCount);
        Assert.True(r.IsFailure);
        Assert.Same(TestError2, r.Error);
    }

    [Fact]
    public async Task MapError_13()
    {
        int invokeCount = 0;
        var r = await Task.FromResult(Result.Success(5)).MapError((e) => { invokeCount++; return TestError2; });
        Assert.Equal(0, invokeCount);
        Assert.True(r.IsSuccess);
        Assert.Equal(5, r.Value);
    }

    [Fact]
    public async Task MapError_14()
    {
        int invokeCount = 0;
        var r = await Task.FromResult(Result.Failure<int>(TestError)).MapError((e) => { invokeCount++; return TestError2; });
        Assert.Equal(1, invokeCount);
        Assert.True(r.IsFailure);
        Assert.Same(TestError2, r.Error);
    }

    [Fact]
    public async Task MapError_15()
    {
        int invokeCount = 0;
        var r = await Task.FromResult(Result.Success(5)).MapError(99, (state, e) => { invokeCount++; return TestError2; });
        Assert.Equal(0, invokeCount);
        Assert.True(r.IsSuccess);
        Assert.Equal(5, r.Value);
    }

    [Fact]
    public async Task MapError_16()
    {
        int invokeCount = 0;
        var r = await Task.FromResult(Result.Failure<int>(TestError)).MapError(99, (state, e) => { invokeCount++; return TestError2; });
        Assert.Equal(1, invokeCount);
        Assert.True(r.IsFailure);
        Assert.Same(TestError2, r.Error);
    }

    [Fact]
    public async Task MapError_17()
    {
        int invokeCount = 0;
        var r = await new ValueTask<Result>(Result.Success()).MapError((e) => { invokeCount++; return TestError2; });
        Assert.Equal(0, invokeCount);
        Assert.True(r.IsSuccess);
    }

    [Fact]
    public async Task MapError_18()
    {
        int invokeCount = 0;
        var r = await new ValueTask<Result>(Result.Failure(TestError)).MapError((e) => { invokeCount++; return TestError2; });
        Assert.Equal(1, invokeCount);
        Assert.True(r.IsFailure);
        Assert.Same(TestError2, r.Error);
    }

    [Fact]
    public async Task MapError_19()
    {
        int invokeCount = 0;
        var r = await new ValueTask<Result>(Result.Success()).MapError(99, (state, e) => { invokeCount++; return TestError2; });
        Assert.Equal(0, invokeCount);
        Assert.True(r.IsSuccess);
    }

    [Fact]
    public async Task MapError_20()
    {
        int invokeCount = 0;
        var r = await new ValueTask<Result>(Result.Failure(TestError)).MapError(99, (state, e) => { invokeCount++; return TestError2; });
        Assert.Equal(1, invokeCount);
        Assert.True(r.IsFailure);
        Assert.Same(TestError2, r.Error);
    }

    [Fact]
    public async Task MapError_21()
    {
        int invokeCount = 0;
        var r = await new ValueTask<Result<int>>(Result.Success(5)).MapError((e) => { invokeCount++; return TestError2; });
        Assert.Equal(0, invokeCount);
        Assert.True(r.IsSuccess);
        Assert.Equal(5, r.Value);
    }

    [Fact]
    public async Task MapError_22()
    {
        int invokeCount = 0;
        var r = await new ValueTask<Result<int>>(Result.Failure<int>(TestError)).MapError((e) => { invokeCount++; return TestError2; });
        Assert.Equal(1, invokeCount);
        Assert.True(r.IsFailure);
        Assert.Same(TestError2, r.Error);
    }

    [Fact]
    public async Task MapError_23()
    {
        int invokeCount = 0;
        var r = await new ValueTask<Result<int>>(Result.Success(5)).MapError(99, (state, e) => { invokeCount++; return TestError2; });
        Assert.Equal(0, invokeCount);
        Assert.True(r.IsSuccess);
        Assert.Equal(5, r.Value);
    }

    [Fact]
    public async Task MapError_24()
    {
        int invokeCount = 0;
        var r = await new ValueTask<Result<int>>(Result.Failure<int>(TestError)).MapError(99, (state, e) => { invokeCount++; return TestError2; });
        Assert.Equal(1, invokeCount);
        Assert.True(r.IsFailure);
        Assert.Same(TestError2, r.Error);
    }
}
