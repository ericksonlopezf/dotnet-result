// Copyright © Erickson Lopez. MIT License.
using System;
using System.Threading;
using System.Threading.Tasks;
using EricksonLopez.Result;
using EricksonLopez.Result.Testing;
using Xunit;

namespace EricksonLopez.Result.Tests.Core;

public class ResultExtensionsInspectBehaviorTests : ResultExtensionsTestsBase
{

    [Fact]
    public void Inspect_Generic_OnSuccess_ReturnsSuccess()
    {
        int invokeCount = 0;
        var r = Result.Success().Inspect(v => { invokeCount++; });
        Assert.Equal(1, invokeCount);
        r.ShouldBeSuccess();
    }

    [Fact]
    public void Inspect_Generic_OnFailure_ReturnsFailure()
    {
        int invokeCount = 0;
        var r = Result.Failure(TestError).Inspect(v => { invokeCount++; });
        Assert.Equal(1, invokeCount);
        r.ShouldBeFailure();
        Assert.Same(TestError, r.Error);
    }

    [Fact]
    public void Inspect_Generic_WithState_OnSuccess_ReturnsSuccess()
    {
        int invokeCount = 0;
        var r = Result.Success().Inspect(99, (state, v) => { invokeCount++; });
        Assert.Equal(1, invokeCount);
        r.ShouldBeSuccess();
    }

    [Fact]
    public void Inspect_Generic_WithState_OnFailure_ReturnsFailure()
    {
        int invokeCount = 0;
        var r = Result.Failure(TestError).Inspect(99, (state, v) => { invokeCount++; });
        Assert.Equal(1, invokeCount);
        r.ShouldBeFailure();
        Assert.Same(TestError, r.Error);
    }

    [Fact]
    public void Inspect_Generic_OnSuccess_ReturnsSuccess_Variant2()
    {
        int invokeCount = 0;
        var r = Result.Success(5).Inspect(v => { invokeCount++; });
        Assert.Equal(1, invokeCount);
        r.ShouldBeSuccess();
        Assert.Equal(5, r.Value);
    }

    [Fact]
    public void Inspect_Generic_OnFailure_ReturnsFailure_Variant2()
    {
        int invokeCount = 0;
        var r = Result.Failure<int>(TestError).Inspect(v => { invokeCount++; });
        Assert.Equal(1, invokeCount);
        r.ShouldBeFailure();
        Assert.Same(TestError, r.Error);
    }

    [Fact]
    public void Inspect_Generic_WithState_OnSuccess_ReturnsSuccess_Variant2()
    {
        int invokeCount = 0;
        var r = Result.Success(5).Inspect(99, (state, v) => { invokeCount++; });
        Assert.Equal(1, invokeCount);
        r.ShouldBeSuccess();
        Assert.Equal(5, r.Value);
    }

    [Fact]
    public void Inspect_Generic_WithState_OnFailure_ReturnsFailure_Variant2()
    {
        int invokeCount = 0;
        var r = Result.Failure<int>(TestError).Inspect(99, (state, v) => { invokeCount++; });
        Assert.Equal(1, invokeCount);
        r.ShouldBeFailure();
        Assert.Same(TestError, r.Error);
    }

    [Fact]
    public async Task Inspect_Task_Generic_OnSuccess_ReturnsSuccess()
    {
        int invokeCount = 0;
        var r = await Task.FromResult(Result.Success()).Inspect(v => { invokeCount++; });
        Assert.Equal(1, invokeCount);
        r.ShouldBeSuccess();
    }

    [Fact]
    public async Task Inspect_Task_Generic_OnFailure_ReturnsFailure()
    {
        int invokeCount = 0;
        var r = await Task.FromResult(Result.Failure(TestError)).Inspect(v => { invokeCount++; });
        Assert.Equal(1, invokeCount);
        r.ShouldBeFailure();
        Assert.Same(TestError, r.Error);
    }

    [Fact]
    public async Task Inspect_Task_Generic_WithState_OnSuccess_ReturnsSuccess()
    {
        int invokeCount = 0;
        var r = await Task.FromResult(Result.Success()).Inspect(99, (state, v) => { invokeCount++; });
        Assert.Equal(1, invokeCount);
        r.ShouldBeSuccess();
    }

    [Fact]
    public async Task Inspect_Task_Generic_WithState_OnFailure_ReturnsFailure()
    {
        int invokeCount = 0;
        var r = await Task.FromResult(Result.Failure(TestError)).Inspect(99, (state, v) => { invokeCount++; });
        Assert.Equal(1, invokeCount);
        r.ShouldBeFailure();
        Assert.Same(TestError, r.Error);
    }

    [Fact]
    public async Task Inspect_Task_Generic_OnSuccess_ReturnsSuccess_Variant2()
    {
        int invokeCount = 0;
        var r = await Task.FromResult(Result.Success(5)).Inspect(v => { invokeCount++; });
        Assert.Equal(1, invokeCount);
        r.ShouldBeSuccess();
        Assert.Equal(5, r.Value);
    }

    [Fact]
    public async Task Inspect_Task_Generic_OnFailure_ReturnsFailure_Variant2()
    {
        int invokeCount = 0;
        var r = await Task.FromResult(Result.Failure<int>(TestError)).Inspect(v => { invokeCount++; });
        Assert.Equal(1, invokeCount);
        r.ShouldBeFailure();
        Assert.Same(TestError, r.Error);
    }

    [Fact]
    public async Task Inspect_Task_Generic_WithState_OnSuccess_ReturnsSuccess_Variant2()
    {
        int invokeCount = 0;
        var r = await Task.FromResult(Result.Success(5)).Inspect(99, (state, v) => { invokeCount++; });
        Assert.Equal(1, invokeCount);
        r.ShouldBeSuccess();
        Assert.Equal(5, r.Value);
    }

    [Fact]
    public async Task Inspect_Task_Generic_WithState_OnFailure_ReturnsFailure_Variant2()
    {
        int invokeCount = 0;
        var r = await Task.FromResult(Result.Failure<int>(TestError)).Inspect(99, (state, v) => { invokeCount++; });
        Assert.Equal(1, invokeCount);
        r.ShouldBeFailure();
        Assert.Same(TestError, r.Error);
    }

    [Fact]
    public async Task Inspect_ValueTask_Generic_OnSuccess_ReturnsSuccess()
    {
        int invokeCount = 0;
        var r = await new ValueTask<Result>(Result.Success()).Inspect(v => { invokeCount++; });
        Assert.Equal(1, invokeCount);
        r.ShouldBeSuccess();
    }

    [Fact]
    public async Task Inspect_ValueTask_Generic_OnFailure_ReturnsFailure()
    {
        int invokeCount = 0;
        var r = await new ValueTask<Result>(Result.Failure(TestError)).Inspect(v => { invokeCount++; });
        Assert.Equal(1, invokeCount);
        r.ShouldBeFailure();
        Assert.Same(TestError, r.Error);
    }

    [Fact]
    public async Task Inspect_ValueTask_Generic_WithState_OnSuccess_ReturnsSuccess()
    {
        int invokeCount = 0;
        var r = await new ValueTask<Result>(Result.Success()).Inspect(99, (state, v) => { invokeCount++; });
        Assert.Equal(1, invokeCount);
        r.ShouldBeSuccess();
    }

    [Fact]
    public async Task Inspect_ValueTask_Generic_WithState_OnFailure_ReturnsFailure()
    {
        int invokeCount = 0;
        var r = await new ValueTask<Result>(Result.Failure(TestError)).Inspect(99, (state, v) => { invokeCount++; });
        Assert.Equal(1, invokeCount);
        r.ShouldBeFailure();
        Assert.Same(TestError, r.Error);
    }

    [Fact]
    public async Task Inspect_ValueTask_Generic_OnSuccess_ReturnsSuccess_Variant2()
    {
        int invokeCount = 0;
        var r = await new ValueTask<Result<int>>(Result.Success(5)).Inspect(v => { invokeCount++; });
        Assert.Equal(1, invokeCount);
        r.ShouldBeSuccess();
        Assert.Equal(5, r.Value);
    }

    [Fact]
    public async Task Inspect_ValueTask_Generic_OnFailure_ReturnsFailure_Variant2()
    {
        int invokeCount = 0;
        var r = await new ValueTask<Result<int>>(Result.Failure<int>(TestError)).Inspect(v => { invokeCount++; });
        Assert.Equal(1, invokeCount);
        r.ShouldBeFailure();
        Assert.Same(TestError, r.Error);
    }

    [Fact]
    public async Task Inspect_ValueTask_Generic_WithState_OnSuccess_ReturnsSuccess_Variant2()
    {
        int invokeCount = 0;
        var r = await new ValueTask<Result<int>>(Result.Success(5)).Inspect(99, (state, v) => { invokeCount++; });
        Assert.Equal(1, invokeCount);
        r.ShouldBeSuccess();
        Assert.Equal(5, r.Value);
    }

    [Fact]
    public async Task Inspect_ValueTask_Generic_WithState_OnFailure_ReturnsFailure_Variant2()
    {
        int invokeCount = 0;
        var r = await new ValueTask<Result<int>>(Result.Failure<int>(TestError)).Inspect(99, (state, v) => { invokeCount++; });
        Assert.Equal(1, invokeCount);
        r.ShouldBeFailure();
        Assert.Same(TestError, r.Error);
    }
}




