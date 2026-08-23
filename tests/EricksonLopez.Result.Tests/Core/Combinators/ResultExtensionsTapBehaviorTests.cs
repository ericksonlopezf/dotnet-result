// Copyright © Erickson Lopez. MIT License.
using System;
using System.Threading;
using System.Threading.Tasks;
using EricksonLopez.Result;
using EricksonLopez.Result.Testing;
using Xunit;

namespace EricksonLopez.Result.Tests.Core;

public class ResultExtensionsTapBehaviorTests : ResultExtensionsTestsBase
{

    [Fact]
    public void Tap_NonGeneric_OnSuccess_ReturnsSuccess()
    {
        int invokeCount = 0;
        var r = Result.Success().TapOnSuccess(() => { invokeCount++; });
        Assert.Equal(1, invokeCount);
        r.ShouldBeSuccess();
    }

    [Fact]
    public void Tap_NonGeneric_OnFailure_ShortCircuits()
    {
        int invokeCount = 0;
        var r = Result.Failure(TestError).TapOnSuccess(() => { invokeCount++; });
        Assert.Equal(0, invokeCount);
        r.ShouldBeFailure();
        Assert.Same(TestError, r.Error);
    }

    [Fact]
    public void Tap_NonGeneric_WithState_OnSuccess_ReturnsSuccess()
    {
        int invokeCount = 0;
        var r = Result.Success().TapOnSuccess(99, state => { invokeCount++; });
        Assert.Equal(1, invokeCount);
        r.ShouldBeSuccess();
    }

    [Fact]
    public void Tap_NonGeneric_WithState_OnFailure_ShortCircuits()
    {
        int invokeCount = 0;
        var r = Result.Failure(TestError).TapOnSuccess(99, state => { invokeCount++; });
        Assert.Equal(0, invokeCount);
        r.ShouldBeFailure();
        Assert.Same(TestError, r.Error);
    }

    [Fact]
    public void Tap_Generic_OnSuccess_ReturnsSuccess()
    {
        int invokeCount = 0;
        var r = Result.Success(5).TapOnSuccess(v => { invokeCount++; });
        Assert.Equal(1, invokeCount);
        r.ShouldBeSuccess();
        Assert.Equal(5, r.Value);
    }

    [Fact]
    public void Tap_Generic_OnFailure_ShortCircuits()
    {
        int invokeCount = 0;
        var r = Result.Failure<int>(TestError).TapOnSuccess(v => { invokeCount++; });
        Assert.Equal(0, invokeCount);
        r.ShouldBeFailure();
        Assert.Same(TestError, r.Error);
    }

    [Fact]
    public void Tap_Generic_WithState_OnSuccess_ReturnsSuccess()
    {
        int invokeCount = 0;
        var r = Result.Success(5).TapOnSuccess(99, (state, v) => { invokeCount++; });
        Assert.Equal(1, invokeCount);
        r.ShouldBeSuccess();
        Assert.Equal(5, r.Value);
    }

    [Fact]
    public void Tap_Generic_WithState_OnFailure_ShortCircuits()
    {
        int invokeCount = 0;
        var r = Result.Failure<int>(TestError).TapOnSuccess(99, (state, v) => { invokeCount++; });
        Assert.Equal(0, invokeCount);
        r.ShouldBeFailure();
        Assert.Same(TestError, r.Error);
    }

    [Fact]
    public async Task Tap_Task_NonGeneric_OnSuccess_ReturnsSuccess()
    {
        int invokeCount = 0;
        var r = await Task.FromResult(Result.Success()).TapOnSuccess(() => { invokeCount++; });
        Assert.Equal(1, invokeCount);
        r.ShouldBeSuccess();
    }

    [Fact]
    public async Task Tap_Task_NonGeneric_OnFailure_ShortCircuits()
    {
        int invokeCount = 0;
        var r = await Task.FromResult(Result.Failure(TestError)).TapOnSuccess(() => { invokeCount++; });
        Assert.Equal(0, invokeCount);
        r.ShouldBeFailure();
        Assert.Same(TestError, r.Error);
    }

    [Fact]
    public async Task Tap_Task_NonGeneric_WithState_OnSuccess_ReturnsSuccess()
    {
        int invokeCount = 0;
        var r = await Task.FromResult(Result.Success()).TapOnSuccess(99, state => { invokeCount++; });
        Assert.Equal(1, invokeCount);
        r.ShouldBeSuccess();
    }

    [Fact]
    public async Task Tap_Task_NonGeneric_WithState_OnFailure_ShortCircuits()
    {
        int invokeCount = 0;
        var r = await Task.FromResult(Result.Failure(TestError)).TapOnSuccess(99, state => { invokeCount++; });
        Assert.Equal(0, invokeCount);
        r.ShouldBeFailure();
        Assert.Same(TestError, r.Error);
    }

    [Fact]
    public async Task Tap_Task_NonGeneric_OnSuccess_ReturnsSuccess_Variant2()
    {
        int invokeCount = 0;
        var r = await Task.FromResult(Result.Success()).TapOnSuccess(() => { invokeCount++; return Task.CompletedTask; });
        Assert.Equal(1, invokeCount);
        r.ShouldBeSuccess();
    }

    [Fact]
    public async Task Tap_Task_NonGeneric_OnFailure_ShortCircuits_Variant2()
    {
        int invokeCount = 0;
        var r = await Task.FromResult(Result.Failure(TestError)).TapOnSuccess(() => { invokeCount++; return Task.CompletedTask; });
        Assert.Equal(0, invokeCount);
        r.ShouldBeFailure();
        Assert.Same(TestError, r.Error);
    }

    [Fact]
    public async Task Tap_Task_Generic_OnSuccess_ReturnsSuccess()
    {
        int invokeCount = 0;
        var r = await Task.FromResult(Result.Success(5)).TapOnSuccess(v => { invokeCount++; });
        Assert.Equal(1, invokeCount);
        r.ShouldBeSuccess();
        Assert.Equal(5, r.Value);
    }

    [Fact]
    public async Task Tap_Task_Generic_OnFailure_ShortCircuits()
    {
        int invokeCount = 0;
        var r = await Task.FromResult(Result.Failure<int>(TestError)).TapOnSuccess(v => { invokeCount++; });
        Assert.Equal(0, invokeCount);
        r.ShouldBeFailure();
        Assert.Same(TestError, r.Error);
    }

    [Fact]
    public async Task Tap_Task_Generic_WithState_OnSuccess_ReturnsSuccess()
    {
        int invokeCount = 0;
        var r = await Task.FromResult(Result.Success(5)).TapOnSuccess(99, (state, v) => { invokeCount++; });
        Assert.Equal(1, invokeCount);
        r.ShouldBeSuccess();
        Assert.Equal(5, r.Value);
    }

    [Fact]
    public async Task Tap_Task_Generic_WithState_OnFailure_ShortCircuits()
    {
        int invokeCount = 0;
        var r = await Task.FromResult(Result.Failure<int>(TestError)).TapOnSuccess(99, (state, v) => { invokeCount++; });
        Assert.Equal(0, invokeCount);
        r.ShouldBeFailure();
        Assert.Same(TestError, r.Error);
    }

    [Fact]
    public async Task Tap_Task_Generic_OnSuccess_ReturnsSuccess_Variant2()
    {
        int invokeCount = 0;
        var r = await Task.FromResult(Result.Success(5)).TapOnSuccess(v => { invokeCount++; return Task.CompletedTask; });
        Assert.Equal(1, invokeCount);
        r.ShouldBeSuccess();
        Assert.Equal(5, r.Value);
    }

    [Fact]
    public async Task Tap_Task_Generic_OnFailure_ShortCircuits_Variant2()
    {
        int invokeCount = 0;
        var r = await Task.FromResult(Result.Failure<int>(TestError)).TapOnSuccess(v => { invokeCount++; return Task.CompletedTask; });
        Assert.Equal(0, invokeCount);
        r.ShouldBeFailure();
        Assert.Same(TestError, r.Error);
    }

    [Fact]
    public async Task Tap_ValueTask_NonGeneric_OnSuccess_ReturnsSuccess()
    {
        int invokeCount = 0;
        var r = await new ValueTask<Result>(Result.Success()).TapOnSuccess(() => { invokeCount++; });
        Assert.Equal(1, invokeCount);
        r.ShouldBeSuccess();
    }

    [Fact]
    public async Task Tap_ValueTask_NonGeneric_OnFailure_ShortCircuits()
    {
        int invokeCount = 0;
        var r = await new ValueTask<Result>(Result.Failure(TestError)).TapOnSuccess(() => { invokeCount++; });
        Assert.Equal(0, invokeCount);
        r.ShouldBeFailure();
        Assert.Same(TestError, r.Error);
    }

    [Fact]
    public async Task Tap_ValueTask_NonGeneric_WithState_OnSuccess_ReturnsSuccess()
    {
        int invokeCount = 0;
        var r = await new ValueTask<Result>(Result.Success()).TapOnSuccess(99, state => { invokeCount++; });
        Assert.Equal(1, invokeCount);
        r.ShouldBeSuccess();
    }

    [Fact]
    public async Task Tap_ValueTask_NonGeneric_WithState_OnFailure_ShortCircuits()
    {
        int invokeCount = 0;
        var r = await new ValueTask<Result>(Result.Failure(TestError)).TapOnSuccess(99, state => { invokeCount++; });
        Assert.Equal(0, invokeCount);
        r.ShouldBeFailure();
        Assert.Same(TestError, r.Error);
    }

    [Fact]
    public async Task Tap_ValueTask_NonGeneric_OnSuccess_ReturnsSuccess_Variant2()
    {
        int invokeCount = 0;
        var r = await new ValueTask<Result>(Result.Success()).TapOnSuccess(() => { invokeCount++; return default(ValueTask); });
        Assert.Equal(1, invokeCount);
        r.ShouldBeSuccess();
    }

    [Fact]
    public async Task Tap_ValueTask_NonGeneric_OnFailure_ShortCircuits_Variant2()
    {
        int invokeCount = 0;
        var r = await new ValueTask<Result>(Result.Failure(TestError)).TapOnSuccess(() => { invokeCount++; return default(ValueTask); });
        Assert.Equal(0, invokeCount);
        r.ShouldBeFailure();
        Assert.Same(TestError, r.Error);
    }

    [Fact]
    public async Task Tap_ValueTask_Generic_OnSuccess_ReturnsSuccess()
    {
        int invokeCount = 0;
        var r = await new ValueTask<Result<int>>(Result.Success(5)).TapOnSuccess(v => { invokeCount++; });
        Assert.Equal(1, invokeCount);
        r.ShouldBeSuccess();
        Assert.Equal(5, r.Value);
    }

    [Fact]
    public async Task Tap_ValueTask_Generic_OnFailure_ShortCircuits()
    {
        int invokeCount = 0;
        var r = await new ValueTask<Result<int>>(Result.Failure<int>(TestError)).TapOnSuccess(v => { invokeCount++; });
        Assert.Equal(0, invokeCount);
        r.ShouldBeFailure();
        Assert.Same(TestError, r.Error);
    }

    [Fact]
    public async Task Tap_ValueTask_Generic_WithState_OnSuccess_ReturnsSuccess()
    {
        int invokeCount = 0;
        var r = await new ValueTask<Result<int>>(Result.Success(5)).TapOnSuccess(99, (state, v) => { invokeCount++; });
        Assert.Equal(1, invokeCount);
        r.ShouldBeSuccess();
        Assert.Equal(5, r.Value);
    }

    [Fact]
    public async Task Tap_ValueTask_Generic_WithState_OnFailure_ShortCircuits()
    {
        int invokeCount = 0;
        var r = await new ValueTask<Result<int>>(Result.Failure<int>(TestError)).TapOnSuccess(99, (state, v) => { invokeCount++; });
        Assert.Equal(0, invokeCount);
        r.ShouldBeFailure();
        Assert.Same(TestError, r.Error);
    }

    [Fact]
    public async Task Tap_ValueTask_Generic_OnSuccess_ReturnsSuccess_Variant2()
    {
        int invokeCount = 0;
        var r = await new ValueTask<Result<int>>(Result.Success(5)).TapOnSuccess(v => { invokeCount++; return default(ValueTask); });
        Assert.Equal(1, invokeCount);
        r.ShouldBeSuccess();
        Assert.Equal(5, r.Value);
    }

    [Fact]
    public async Task Tap_ValueTask_Generic_OnFailure_ShortCircuits_Variant2()
    {
        int invokeCount = 0;
        var r = await new ValueTask<Result<int>>(Result.Failure<int>(TestError)).TapOnSuccess(v => { invokeCount++; return default(ValueTask); });
        Assert.Equal(0, invokeCount);
        r.ShouldBeFailure();
        Assert.Same(TestError, r.Error);
    }
}




