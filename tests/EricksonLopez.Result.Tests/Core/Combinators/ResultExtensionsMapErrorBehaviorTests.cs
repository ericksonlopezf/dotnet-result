// Copyright © Erickson Lopez. MIT License.
using System;
using System.Threading;
using System.Threading.Tasks;
using EricksonLopez.Result;
using EricksonLopez.Result.Testing;
using Xunit;

namespace EricksonLopez.Result.Tests.Core;

public class ResultExtensionsMapErrorBehaviorTests : ResultExtensionsTestsBase
{

    [Fact]
    public void MapError_NonGeneric_OnSuccess_ShortCircuits()
    {
        int invokeCount = 0;
        var r = Result.Success().MapError((e) => { invokeCount++; return TestError2; });
        Assert.Equal(0, invokeCount);
        r.ShouldBeSuccess();
    }

    [Fact]
    public void MapError_NonGeneric_OnFailure_ReturnsFailure()
    {
        int invokeCount = 0;
        var r = Result.Failure(TestError).MapError((e) => { invokeCount++; return TestError2; });
        Assert.Equal(1, invokeCount);
        r.ShouldBeFailure();
        Assert.Same(TestError2, r.Error);
    }

    [Fact]
    public void MapError_NonGeneric_WithState_OnSuccess_ShortCircuits()
    {
        int invokeCount = 0;
        var r = Result.Success().MapError(99, (state, e) => { invokeCount++; return TestError2; });
        Assert.Equal(0, invokeCount);
        r.ShouldBeSuccess();
    }

    [Fact]
    public void MapError_NonGeneric_WithState_OnFailure_ReturnsFailure()
    {
        int invokeCount = 0;
        var r = Result.Failure(TestError).MapError(99, (state, e) => { invokeCount++; return TestError2; });
        Assert.Equal(1, invokeCount);
        r.ShouldBeFailure();
        Assert.Same(TestError2, r.Error);
    }

    [Fact]
    public void MapError_Generic_OnSuccess_ShortCircuits()
    {
        int invokeCount = 0;
        var r = Result.Success(5).MapError((e) => { invokeCount++; return TestError2; });
        Assert.Equal(0, invokeCount);
        r.ShouldBeSuccess();
        Assert.Equal(5, r.Value);
    }

    [Fact]
    public void MapError_Generic_OnFailure_ReturnsFailure()
    {
        int invokeCount = 0;
        var r = Result.Failure<int>(TestError).MapError((e) => { invokeCount++; return TestError2; });
        Assert.Equal(1, invokeCount);
        r.ShouldBeFailure();
        Assert.Same(TestError2, r.Error);
    }

    [Fact]
    public void MapError_Generic_WithState_OnSuccess_ShortCircuits()
    {
        int invokeCount = 0;
        var r = Result.Success(5).MapError(99, (state, e) => { invokeCount++; return TestError2; });
        Assert.Equal(0, invokeCount);
        r.ShouldBeSuccess();
        Assert.Equal(5, r.Value);
    }

    [Fact]
    public void MapError_Generic_WithState_OnFailure_ReturnsFailure()
    {
        int invokeCount = 0;
        var r = Result.Failure<int>(TestError).MapError(99, (state, e) => { invokeCount++; return TestError2; });
        Assert.Equal(1, invokeCount);
        r.ShouldBeFailure();
        Assert.Same(TestError2, r.Error);
    }

    [Fact]
    public async Task MapError_Task_NonGeneric_OnSuccess_ShortCircuits()
    {
        int invokeCount = 0;
        var r = await Task.FromResult(Result.Success()).MapError((e) => { invokeCount++; return TestError2; });
        Assert.Equal(0, invokeCount);
        r.ShouldBeSuccess();
    }

    [Fact]
    public async Task MapError_Task_NonGeneric_OnFailure_ReturnsFailure()
    {
        int invokeCount = 0;
        var r = await Task.FromResult(Result.Failure(TestError)).MapError((e) => { invokeCount++; return TestError2; });
        Assert.Equal(1, invokeCount);
        r.ShouldBeFailure();
        Assert.Same(TestError2, r.Error);
    }

    [Fact]
    public async Task MapError_Task_NonGeneric_WithState_OnSuccess_ShortCircuits()
    {
        int invokeCount = 0;
        var r = await Task.FromResult(Result.Success()).MapError(99, (state, e) => { invokeCount++; return TestError2; });
        Assert.Equal(0, invokeCount);
        r.ShouldBeSuccess();
    }

    [Fact]
    public async Task MapError_Task_NonGeneric_WithState_OnFailure_ReturnsFailure()
    {
        int invokeCount = 0;
        var r = await Task.FromResult(Result.Failure(TestError)).MapError(99, (state, e) => { invokeCount++; return TestError2; });
        Assert.Equal(1, invokeCount);
        r.ShouldBeFailure();
        Assert.Same(TestError2, r.Error);
    }

    [Fact]
    public async Task MapError_Task_Generic_OnSuccess_ShortCircuits()
    {
        int invokeCount = 0;
        var r = await Task.FromResult(Result.Success(5)).MapError((e) => { invokeCount++; return TestError2; });
        Assert.Equal(0, invokeCount);
        r.ShouldBeSuccess();
        Assert.Equal(5, r.Value);
    }

    [Fact]
    public async Task MapError_Task_Generic_OnFailure_ReturnsFailure()
    {
        int invokeCount = 0;
        var r = await Task.FromResult(Result.Failure<int>(TestError)).MapError((e) => { invokeCount++; return TestError2; });
        Assert.Equal(1, invokeCount);
        r.ShouldBeFailure();
        Assert.Same(TestError2, r.Error);
    }

    [Fact]
    public async Task MapError_Task_Generic_WithState_OnSuccess_ShortCircuits()
    {
        int invokeCount = 0;
        var r = await Task.FromResult(Result.Success(5)).MapError(99, (state, e) => { invokeCount++; return TestError2; });
        Assert.Equal(0, invokeCount);
        r.ShouldBeSuccess();
        Assert.Equal(5, r.Value);
    }

    [Fact]
    public async Task MapError_Task_Generic_WithState_OnFailure_ReturnsFailure()
    {
        int invokeCount = 0;
        var r = await Task.FromResult(Result.Failure<int>(TestError)).MapError(99, (state, e) => { invokeCount++; return TestError2; });
        Assert.Equal(1, invokeCount);
        r.ShouldBeFailure();
        Assert.Same(TestError2, r.Error);
    }

    [Fact]
    public async Task MapError_ValueTask_NonGeneric_OnSuccess_ShortCircuits()
    {
        int invokeCount = 0;
        var r = await new ValueTask<Result>(Result.Success()).MapError((e) => { invokeCount++; return TestError2; });
        Assert.Equal(0, invokeCount);
        r.ShouldBeSuccess();
    }

    [Fact]
    public async Task MapError_ValueTask_NonGeneric_OnFailure_ReturnsFailure()
    {
        int invokeCount = 0;
        var r = await new ValueTask<Result>(Result.Failure(TestError)).MapError((e) => { invokeCount++; return TestError2; });
        Assert.Equal(1, invokeCount);
        r.ShouldBeFailure();
        Assert.Same(TestError2, r.Error);
    }

    [Fact]
    public async Task MapError_ValueTask_NonGeneric_WithState_OnSuccess_ShortCircuits()
    {
        int invokeCount = 0;
        var r = await new ValueTask<Result>(Result.Success()).MapError(99, (state, e) => { invokeCount++; return TestError2; });
        Assert.Equal(0, invokeCount);
        r.ShouldBeSuccess();
    }

    [Fact]
    public async Task MapError_ValueTask_NonGeneric_WithState_OnFailure_ReturnsFailure()
    {
        int invokeCount = 0;
        var r = await new ValueTask<Result>(Result.Failure(TestError)).MapError(99, (state, e) => { invokeCount++; return TestError2; });
        Assert.Equal(1, invokeCount);
        r.ShouldBeFailure();
        Assert.Same(TestError2, r.Error);
    }

    [Fact]
    public async Task MapError_ValueTask_Generic_OnSuccess_ShortCircuits()
    {
        int invokeCount = 0;
        var r = await new ValueTask<Result<int>>(Result.Success(5)).MapError((e) => { invokeCount++; return TestError2; });
        Assert.Equal(0, invokeCount);
        r.ShouldBeSuccess();
        Assert.Equal(5, r.Value);
    }

    [Fact]
    public async Task MapError_ValueTask_Generic_OnFailure_ReturnsFailure()
    {
        int invokeCount = 0;
        var r = await new ValueTask<Result<int>>(Result.Failure<int>(TestError)).MapError((e) => { invokeCount++; return TestError2; });
        Assert.Equal(1, invokeCount);
        r.ShouldBeFailure();
        Assert.Same(TestError2, r.Error);
    }

    [Fact]
    public async Task MapError_ValueTask_Generic_WithState_OnSuccess_ShortCircuits()
    {
        int invokeCount = 0;
        var r = await new ValueTask<Result<int>>(Result.Success(5)).MapError(99, (state, e) => { invokeCount++; return TestError2; });
        Assert.Equal(0, invokeCount);
        r.ShouldBeSuccess();
        Assert.Equal(5, r.Value);
    }

    [Fact]
    public async Task MapError_ValueTask_Generic_WithState_OnFailure_ReturnsFailure()
    {
        int invokeCount = 0;
        var r = await new ValueTask<Result<int>>(Result.Failure<int>(TestError)).MapError(99, (state, e) => { invokeCount++; return TestError2; });
        Assert.Equal(1, invokeCount);
        r.ShouldBeFailure();
        Assert.Same(TestError2, r.Error);
    }
}




