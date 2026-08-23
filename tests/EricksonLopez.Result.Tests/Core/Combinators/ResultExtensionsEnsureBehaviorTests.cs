// Copyright © Erickson Lopez. MIT License.
using System;
using System.Threading;
using System.Threading.Tasks;
using EricksonLopez.Result;
using EricksonLopez.Result.Testing;
using Xunit;

namespace EricksonLopez.Result.Tests.Core;

public class ResultExtensionsEnsureBehaviorTests : ResultExtensionsTestsBase
{

    [Fact]
    public void Ensure_NonGeneric_OnSuccess_ReturnsSuccess()
    {
        int invokeCount = 0;
        var r = Result.Success().Ensure(() => { invokeCount++; return true; }, TestError2);
        Assert.Equal(1, invokeCount);
        r.ShouldBeSuccess();
    }

    [Fact]
    public void Ensure_NonGeneric_OnFailure_ShortCircuits()
    {
        int invokeCount = 0;
        var r = Result.Failure(TestError).Ensure(() => { invokeCount++; return true; }, TestError2);
        Assert.Equal(0, invokeCount);
        r.ShouldBeFailure();
        Assert.Same(TestError, r.Error);
    }

    [Fact]
    public void Ensure_NonGeneric_OnSuccess_ReturnsFailure()
    {
        int invokeCount = 0;
        var r = Result.Success().Ensure(() => { invokeCount++; return false; }, TestError2);
        Assert.Equal(1, invokeCount);
        r.ShouldBeFailure();
        Assert.Same(TestError2, r.Error);
    }

    [Fact]
    public void Ensure_NonGeneric_WithState_OnSuccess_ReturnsSuccess()
    {
        int invokeCount = 0;
        var r = Result.Success().Ensure(99, state => { invokeCount++; return true; }, TestError2);
        Assert.Equal(1, invokeCount);
        r.ShouldBeSuccess();
    }

    [Fact]
    public void Ensure_NonGeneric_WithState_OnFailure_ShortCircuits()
    {
        int invokeCount = 0;
        var r = Result.Failure(TestError).Ensure(99, state => { invokeCount++; return true; }, TestError2);
        Assert.Equal(0, invokeCount);
        r.ShouldBeFailure();
        Assert.Same(TestError, r.Error);
    }

    [Fact]
    public void Ensure_NonGeneric_WithState_OnSuccess_ReturnsFailure()
    {
        int invokeCount = 0;
        var r = Result.Success().Ensure(99, state => { invokeCount++; return false; }, TestError2);
        Assert.Equal(1, invokeCount);
        r.ShouldBeFailure();
        Assert.Same(TestError2, r.Error);
    }

    [Fact]
    public void Ensure_Generic_OnSuccess_ReturnsSuccess()
    {
        int invokeCount = 0;
        var r = Result.Success(5).Ensure(v => { invokeCount++; return true; }, TestError2);
        Assert.Equal(1, invokeCount);
        r.ShouldBeSuccess();
        Assert.Equal(5, r.Value);
    }

    [Fact]
    public void Ensure_Generic_OnFailure_ShortCircuits()
    {
        int invokeCount = 0;
        var r = Result.Failure<int>(TestError).Ensure(v => { invokeCount++; return true; }, TestError2);
        Assert.Equal(0, invokeCount);
        r.ShouldBeFailure();
        Assert.Same(TestError, r.Error);
    }

    [Fact]
    public void Ensure_Generic_OnSuccess_ReturnsFailure()
    {
        int invokeCount = 0;
        var r = Result.Success(5).Ensure(v => { invokeCount++; return false; }, TestError2);
        Assert.Equal(1, invokeCount);
        r.ShouldBeFailure();
        Assert.Same(TestError2, r.Error);
    }

    [Fact]
    public void Ensure_Generic_WithState_OnSuccess_ReturnsSuccess()
    {
        int invokeCount = 0;
        var r = Result.Success(5).Ensure(99, (state, v) => { invokeCount++; return true; }, TestError2);
        Assert.Equal(1, invokeCount);
        r.ShouldBeSuccess();
        Assert.Equal(5, r.Value);
    }

    [Fact]
    public void Ensure_Generic_WithState_OnFailure_ShortCircuits()
    {
        int invokeCount = 0;
        var r = Result.Failure<int>(TestError).Ensure(99, (state, v) => { invokeCount++; return true; }, TestError2);
        Assert.Equal(0, invokeCount);
        r.ShouldBeFailure();
        Assert.Same(TestError, r.Error);
    }

    [Fact]
    public void Ensure_Generic_WithState_OnSuccess_ReturnsFailure()
    {
        int invokeCount = 0;
        var r = Result.Success(5).Ensure(99, (state, v) => { invokeCount++; return false; }, TestError2);
        Assert.Equal(1, invokeCount);
        r.ShouldBeFailure();
        Assert.Same(TestError2, r.Error);
    }

    [Fact]
    public async Task Ensure_Task_NonGeneric_OnSuccess_ReturnsSuccess()
    {
        int invokeCount = 0;
        var r = await Task.FromResult(Result.Success()).Ensure(() => { invokeCount++; return true; }, TestError2);
        Assert.Equal(1, invokeCount);
        r.ShouldBeSuccess();
    }

    [Fact]
    public async Task Ensure_Task_NonGeneric_OnFailure_ShortCircuits()
    {
        int invokeCount = 0;
        var r = await Task.FromResult(Result.Failure(TestError)).Ensure(() => { invokeCount++; return true; }, TestError2);
        Assert.Equal(0, invokeCount);
        r.ShouldBeFailure();
        Assert.Same(TestError, r.Error);
    }

    [Fact]
    public async Task Ensure_Task_NonGeneric_OnSuccess_ReturnsFailure()
    {
        int invokeCount = 0;
        var r = await Task.FromResult(Result.Success()).Ensure(() => { invokeCount++; return false; }, TestError2);
        Assert.Equal(1, invokeCount);
        r.ShouldBeFailure();
        Assert.Same(TestError2, r.Error);
    }

    [Fact]
    public async Task Ensure_Task_NonGeneric_WithState_OnSuccess_ReturnsSuccess()
    {
        int invokeCount = 0;
        var r = await Task.FromResult(Result.Success()).Ensure(99, state => { invokeCount++; return true; }, TestError2);
        Assert.Equal(1, invokeCount);
        r.ShouldBeSuccess();
    }

    [Fact]
    public async Task Ensure_Task_NonGeneric_WithState_OnFailure_ShortCircuits()
    {
        int invokeCount = 0;
        var r = await Task.FromResult(Result.Failure(TestError)).Ensure(99, state => { invokeCount++; return true; }, TestError2);
        Assert.Equal(0, invokeCount);
        r.ShouldBeFailure();
        Assert.Same(TestError, r.Error);
    }

    [Fact]
    public async Task Ensure_Task_NonGeneric_WithState_OnSuccess_ReturnsFailure()
    {
        int invokeCount = 0;
        var r = await Task.FromResult(Result.Success()).Ensure(99, state => { invokeCount++; return false; }, TestError2);
        Assert.Equal(1, invokeCount);
        r.ShouldBeFailure();
        Assert.Same(TestError2, r.Error);
    }

    [Fact]
    public async Task Ensure_Task_Generic_OnSuccess_ReturnsSuccess()
    {
        int invokeCount = 0;
        var r = await Task.FromResult(Result.Success(5)).Ensure(v => { invokeCount++; return true; }, TestError2);
        Assert.Equal(1, invokeCount);
        r.ShouldBeSuccess();
        Assert.Equal(5, r.Value);
    }

    [Fact]
    public async Task Ensure_Task_Generic_OnFailure_ShortCircuits()
    {
        int invokeCount = 0;
        var r = await Task.FromResult(Result.Failure<int>(TestError)).Ensure(v => { invokeCount++; return true; }, TestError2);
        Assert.Equal(0, invokeCount);
        r.ShouldBeFailure();
        Assert.Same(TestError, r.Error);
    }

    [Fact]
    public async Task Ensure_Task_Generic_OnSuccess_ReturnsFailure()
    {
        int invokeCount = 0;
        var r = await Task.FromResult(Result.Success(5)).Ensure(v => { invokeCount++; return false; }, TestError2);
        Assert.Equal(1, invokeCount);
        r.ShouldBeFailure();
        Assert.Same(TestError2, r.Error);
    }

    [Fact]
    public async Task Ensure_Task_Generic_WithState_OnSuccess_ReturnsSuccess()
    {
        int invokeCount = 0;
        var r = await Task.FromResult(Result.Success(5)).Ensure(99, (state, v) => { invokeCount++; return true; }, TestError2);
        Assert.Equal(1, invokeCount);
        r.ShouldBeSuccess();
        Assert.Equal(5, r.Value);
    }

    [Fact]
    public async Task Ensure_Task_Generic_WithState_OnFailure_ShortCircuits()
    {
        int invokeCount = 0;
        var r = await Task.FromResult(Result.Failure<int>(TestError)).Ensure(99, (state, v) => { invokeCount++; return true; }, TestError2);
        Assert.Equal(0, invokeCount);
        r.ShouldBeFailure();
        Assert.Same(TestError, r.Error);
    }

    [Fact]
    public async Task Ensure_Task_Generic_WithState_OnSuccess_ReturnsFailure()
    {
        int invokeCount = 0;
        var r = await Task.FromResult(Result.Success(5)).Ensure(99, (state, v) => { invokeCount++; return false; }, TestError2);
        Assert.Equal(1, invokeCount);
        r.ShouldBeFailure();
        Assert.Same(TestError2, r.Error);
    }

    [Fact]
    public async Task Ensure_Task_Generic_TaskCallback_OnSuccess_ReturnsSuccess()
    {
        int invokeCount = 0;
        var r = await Task.FromResult(Result.Success(5)).Ensure(v => { invokeCount++; return Task.FromResult(true); }, TestError2);
        Assert.Equal(1, invokeCount);
        r.ShouldBeSuccess();
        Assert.Equal(5, r.Value);
    }

    [Fact]
    public async Task Ensure_Task_Generic_TaskCallback_OnFailure_ShortCircuits()
    {
        int invokeCount = 0;
        var r = await Task.FromResult(Result.Failure<int>(TestError)).Ensure(v => { invokeCount++; return Task.FromResult(true); }, TestError2);
        Assert.Equal(0, invokeCount);
        r.ShouldBeFailure();
        Assert.Same(TestError, r.Error);
    }

    [Fact]
    public async Task Ensure_Task_Generic_TaskCallback_OnSuccess_ReturnsFailure()
    {
        int invokeCount = 0;
        var r = await Task.FromResult(Result.Success(5)).Ensure(v => { invokeCount++; return Task.FromResult(false); }, TestError2);
        Assert.Equal(1, invokeCount);
        r.ShouldBeFailure();
        Assert.Same(TestError2, r.Error);
    }

    [Fact]
    public async Task Ensure_ValueTask_NonGeneric_OnSuccess_ReturnsSuccess()
    {
        int invokeCount = 0;
        var r = await new ValueTask<Result>(Result.Success()).Ensure(() => { invokeCount++; return true; }, TestError2);
        Assert.Equal(1, invokeCount);
        r.ShouldBeSuccess();
    }

    [Fact]
    public async Task Ensure_ValueTask_NonGeneric_OnFailure_ShortCircuits()
    {
        int invokeCount = 0;
        var r = await new ValueTask<Result>(Result.Failure(TestError)).Ensure(() => { invokeCount++; return true; }, TestError2);
        Assert.Equal(0, invokeCount);
        r.ShouldBeFailure();
        Assert.Same(TestError, r.Error);
    }

    [Fact]
    public async Task Ensure_ValueTask_NonGeneric_OnSuccess_ReturnsFailure()
    {
        int invokeCount = 0;
        var r = await new ValueTask<Result>(Result.Success()).Ensure(() => { invokeCount++; return false; }, TestError2);
        Assert.Equal(1, invokeCount);
        r.ShouldBeFailure();
        Assert.Same(TestError2, r.Error);
    }

    [Fact]
    public async Task Ensure_ValueTask_NonGeneric_WithState_OnSuccess_ReturnsSuccess()
    {
        int invokeCount = 0;
        var r = await new ValueTask<Result>(Result.Success()).Ensure(99, state => { invokeCount++; return true; }, TestError2);
        Assert.Equal(1, invokeCount);
        r.ShouldBeSuccess();
    }

    [Fact]
    public async Task Ensure_ValueTask_NonGeneric_WithState_OnFailure_ShortCircuits()
    {
        int invokeCount = 0;
        var r = await new ValueTask<Result>(Result.Failure(TestError)).Ensure(99, state => { invokeCount++; return true; }, TestError2);
        Assert.Equal(0, invokeCount);
        r.ShouldBeFailure();
        Assert.Same(TestError, r.Error);
    }

    [Fact]
    public async Task Ensure_ValueTask_NonGeneric_WithState_OnSuccess_ReturnsFailure()
    {
        int invokeCount = 0;
        var r = await new ValueTask<Result>(Result.Success()).Ensure(99, state => { invokeCount++; return false; }, TestError2);
        Assert.Equal(1, invokeCount);
        r.ShouldBeFailure();
        Assert.Same(TestError2, r.Error);
    }

    [Fact]
    public async Task Ensure_ValueTask_Generic_OnSuccess_ReturnsSuccess()
    {
        int invokeCount = 0;
        var r = await new ValueTask<Result<int>>(Result.Success(5)).Ensure(v => { invokeCount++; return true; }, TestError2);
        Assert.Equal(1, invokeCount);
        r.ShouldBeSuccess();
        Assert.Equal(5, r.Value);
    }

    [Fact]
    public async Task Ensure_ValueTask_Generic_OnFailure_ShortCircuits()
    {
        int invokeCount = 0;
        var r = await new ValueTask<Result<int>>(Result.Failure<int>(TestError)).Ensure(v => { invokeCount++; return true; }, TestError2);
        Assert.Equal(0, invokeCount);
        r.ShouldBeFailure();
        Assert.Same(TestError, r.Error);
    }

    [Fact]
    public async Task Ensure_ValueTask_Generic_OnSuccess_ReturnsFailure()
    {
        int invokeCount = 0;
        var r = await new ValueTask<Result<int>>(Result.Success(5)).Ensure(v => { invokeCount++; return false; }, TestError2);
        Assert.Equal(1, invokeCount);
        r.ShouldBeFailure();
        Assert.Same(TestError2, r.Error);
    }

    [Fact]
    public async Task Ensure_ValueTask_Generic_WithState_OnSuccess_ReturnsSuccess()
    {
        int invokeCount = 0;
        var r = await new ValueTask<Result<int>>(Result.Success(5)).Ensure(99, (state, v) => { invokeCount++; return true; }, TestError2);
        Assert.Equal(1, invokeCount);
        r.ShouldBeSuccess();
        Assert.Equal(5, r.Value);
    }

    [Fact]
    public async Task Ensure_ValueTask_Generic_WithState_OnFailure_ShortCircuits()
    {
        int invokeCount = 0;
        var r = await new ValueTask<Result<int>>(Result.Failure<int>(TestError)).Ensure(99, (state, v) => { invokeCount++; return true; }, TestError2);
        Assert.Equal(0, invokeCount);
        r.ShouldBeFailure();
        Assert.Same(TestError, r.Error);
    }

    [Fact]
    public async Task Ensure_ValueTask_Generic_WithState_OnSuccess_ReturnsFailure()
    {
        int invokeCount = 0;
        var r = await new ValueTask<Result<int>>(Result.Success(5)).Ensure(99, (state, v) => { invokeCount++; return false; }, TestError2);
        Assert.Equal(1, invokeCount);
        r.ShouldBeFailure();
        Assert.Same(TestError2, r.Error);
    }


    [Fact]
    public void Ensure_WhenPredicateReturnsTrue_ReturnsOriginalSuccess()
    {
        var s = Result.Success();
        Assert.True(s.Ensure(() => true, () => Error.Failure("X", "X")).IsSuccess);
    }

    [Fact]
    public void Ensure_WhenPredicateReturnsFalse_ReturnsFailure()
    {
        var s = Result.Success();
        Assert.True(s.Ensure(() => false, () => Error.Failure("X", "X")).IsFailure);
    }

    [Fact]
    public void Ensure_WithState_WhenPredicateReturnsTrue_ReturnsOriginalSuccess()
    {
        var s = Result.Success();
        Assert.True(s.Ensure(10, _ => true, () => Error.Failure("X", "X")).IsSuccess);
    }

    [Fact]
    public void Ensure_WithState_WhenPredicateReturnsFalse_ReturnsFailure()
    {
        var s = Result.Success();
        Assert.True(s.Ensure(10, _ => false, () => Error.Failure("X", "X")).IsFailure);
    }

    [Fact]
    public void Ensure_WhenSourceIsFailure_ShortCircuits()
    {
        var f = Result.Failure(Error.Failure("X", "X"));
        Assert.True(f.Ensure(() => true, () => Error.Failure("Y", "Y")).IsFailure);
        Assert.True(f.Ensure(10, _ => true, () => Error.Failure("Y", "Y")).IsFailure);
    }
}




