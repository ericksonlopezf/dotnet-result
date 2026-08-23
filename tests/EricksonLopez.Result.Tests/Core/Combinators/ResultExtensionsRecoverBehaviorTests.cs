// Copyright © Erickson Lopez. MIT License.
using System;
using System.Threading;
using System.Threading.Tasks;
using EricksonLopez.Result;
using EricksonLopez.Result.Testing;
using Xunit;

namespace EricksonLopez.Result.Tests.Core;

public class ResultExtensionsRecoverBehaviorTests : ResultExtensionsTestsBase
{

    [Fact]
    public void Recover_NonGeneric_OnSuccess_SkipsCallback()
    {
        int invokeCount = 0;
        var r = Result.Success().Recover((e) => { invokeCount++; return Result.Success(); });
        Assert.Equal(0, invokeCount);
        r.ShouldBeSuccess();
    }

    [Fact]
    public void Recover_NonGeneric_OnFailure_ReturnsSuccess()
    {
        int invokeCount = 0;
        var r = Result.Failure(TestError).Recover((e) => { invokeCount++; return Result.Success(); });
        Assert.Equal(1, invokeCount);
        r.ShouldBeSuccess();
    }

    [Fact]
    public void Recover_NonGeneric_OnFailure_ReturnsFailure()
    {
        int invokeCount = 0;
        var r = Result.Failure(TestError).Recover((e) => { invokeCount++; return Result.Failure(TestError2); });
        Assert.Equal(1, invokeCount);
        r.ShouldBeFailure();
        Assert.Same(TestError2, r.Error);
    }

    [Fact]
    public void Recover_NonGeneric_WithState_OnSuccess_SkipsCallback()
    {
        int invokeCount = 0;
        var r = Result.Success().Recover(99, (state, e) => { invokeCount++; return Result.Success(); });
        Assert.Equal(0, invokeCount);
        r.ShouldBeSuccess();
    }

    [Fact]
    public void Recover_NonGeneric_WithState_OnFailure_ReturnsSuccess()
    {
        int invokeCount = 0;
        var r = Result.Failure(TestError).Recover(99, (state, e) => { invokeCount++; return Result.Success(); });
        Assert.Equal(1, invokeCount);
        r.ShouldBeSuccess();
    }

    [Fact]
    public void Recover_NonGeneric_WithState_OnFailure_ReturnsFailure()
    {
        int invokeCount = 0;
        var r = Result.Failure(TestError).Recover(99, (state, e) => { invokeCount++; return Result.Failure(TestError2); });
        Assert.Equal(1, invokeCount);
        r.ShouldBeFailure();
        Assert.Same(TestError2, r.Error);
    }

    [Fact]
    public void Recover_Generic_OnSuccess_SkipsCallback()
    {
        int invokeCount = 0;
        var r = Result.Success(5).Recover((e) => { invokeCount++; return Result.Success(5); });
        Assert.Equal(0, invokeCount);
        r.ShouldBeSuccess();
        Assert.Equal(5, r.Value);
    }

    [Fact]
    public void Recover_Generic_OnFailure_ReturnsSuccess()
    {
        int invokeCount = 0;
        var r = Result.Failure<int>(TestError).Recover((e) => { invokeCount++; return Result.Success(5); });
        Assert.Equal(1, invokeCount);
        r.ShouldBeSuccess();
        Assert.Equal(5, r.Value);
    }

    [Fact]
    public void Recover_Generic_OnFailure_ReturnsFailure()
    {
        int invokeCount = 0;
        var r = Result.Failure<int>(TestError).Recover((e) => { invokeCount++; return Result.Failure<int>(TestError2); });
        Assert.Equal(1, invokeCount);
        r.ShouldBeFailure();
        Assert.Same(TestError2, r.Error);
    }

    [Fact]
    public void Recover_Generic_WithState_OnSuccess_SkipsCallback()
    {
        int invokeCount = 0;
        var r = Result.Success(5).Recover(99, (state, e) => { invokeCount++; return Result.Success(5); });
        Assert.Equal(0, invokeCount);
        r.ShouldBeSuccess();
        Assert.Equal(5, r.Value);
    }

    [Fact]
    public void Recover_Generic_WithState_OnFailure_ReturnsSuccess()
    {
        int invokeCount = 0;
        var r = Result.Failure<int>(TestError).Recover(99, (state, e) => { invokeCount++; return Result.Success(5); });
        Assert.Equal(1, invokeCount);
        r.ShouldBeSuccess();
        Assert.Equal(5, r.Value);
    }

    [Fact]
    public void Recover_Generic_WithState_OnFailure_ReturnsFailure()
    {
        int invokeCount = 0;
        var r = Result.Failure<int>(TestError).Recover(99, (state, e) => { invokeCount++; return Result.Failure<int>(TestError2); });
        Assert.Equal(1, invokeCount);
        r.ShouldBeFailure();
        Assert.Same(TestError2, r.Error);
    }

    [Fact]
    public async Task Recover_Task_NonGeneric_OnSuccess_SkipsCallback()
    {
        int invokeCount = 0;
        var r = await Task.FromResult(Result.Success()).Recover((e) => { invokeCount++; return Result.Success(); });
        Assert.Equal(0, invokeCount);
        r.ShouldBeSuccess();
    }

    [Fact]
    public async Task Recover_Task_NonGeneric_OnFailure_ReturnsSuccess()
    {
        int invokeCount = 0;
        var r = await Task.FromResult(Result.Failure(TestError)).Recover((e) => { invokeCount++; return Result.Success(); });
        Assert.Equal(1, invokeCount);
        r.ShouldBeSuccess();
    }

    [Fact]
    public async Task Recover_Task_NonGeneric_OnFailure_ReturnsFailure()
    {
        int invokeCount = 0;
        var r = await Task.FromResult(Result.Failure(TestError)).Recover((e) => { invokeCount++; return Result.Failure(TestError2); });
        Assert.Equal(1, invokeCount);
        r.ShouldBeFailure();
        Assert.Same(TestError2, r.Error);
    }

    [Fact]
    public async Task Recover_Task_NonGeneric_WithState_OnSuccess_SkipsCallback()
    {
        int invokeCount = 0;
        var r = await Task.FromResult(Result.Success()).Recover(99, (state, e) => { invokeCount++; return Result.Success(); });
        Assert.Equal(0, invokeCount);
        r.ShouldBeSuccess();
    }

    [Fact]
    public async Task Recover_Task_NonGeneric_WithState_OnFailure_ReturnsSuccess()
    {
        int invokeCount = 0;
        var r = await Task.FromResult(Result.Failure(TestError)).Recover(99, (state, e) => { invokeCount++; return Result.Success(); });
        Assert.Equal(1, invokeCount);
        r.ShouldBeSuccess();
    }

    [Fact]
    public async Task Recover_Task_NonGeneric_WithState_OnFailure_ReturnsFailure()
    {
        int invokeCount = 0;
        var r = await Task.FromResult(Result.Failure(TestError)).Recover(99, (state, e) => { invokeCount++; return Result.Failure(TestError2); });
        Assert.Equal(1, invokeCount);
        r.ShouldBeFailure();
        Assert.Same(TestError2, r.Error);
    }

    [Fact]
    public async Task Recover_Task_NonGeneric_TaskCallback_OnSuccess_SkipsCallback()
    {
        int invokeCount = 0;
        var r = await Task.FromResult(Result.Success()).Recover((e) => { invokeCount++; return Task.FromResult(Result.Success()); });
        Assert.Equal(0, invokeCount);
        r.ShouldBeSuccess();
    }

    [Fact]
    public async Task Recover_Task_NonGeneric_TaskCallback_OnFailure_ReturnsSuccess()
    {
        int invokeCount = 0;
        var r = await Task.FromResult(Result.Failure(TestError)).Recover((e) => { invokeCount++; return Task.FromResult(Result.Success()); });
        Assert.Equal(1, invokeCount);
        r.ShouldBeSuccess();
    }

    [Fact]
    public async Task Recover_Task_NonGeneric_TaskCallback_OnFailure_ReturnsFailure()
    {
        int invokeCount = 0;
        var r = await Task.FromResult(Result.Failure(TestError)).Recover((e) => { invokeCount++; return Task.FromResult(Result.Failure(TestError2)); });
        Assert.Equal(1, invokeCount);
        r.ShouldBeFailure();
        Assert.Same(TestError2, r.Error);
    }

    [Fact]
    public async Task Recover_Task_Generic_OnSuccess_SkipsCallback()
    {
        int invokeCount = 0;
        var r = await Task.FromResult(Result.Success(5)).Recover((e) => { invokeCount++; return Result.Success(5); });
        Assert.Equal(0, invokeCount);
        r.ShouldBeSuccess();
        Assert.Equal(5, r.Value);
    }

    [Fact]
    public async Task Recover_Task_Generic_OnFailure_ReturnsSuccess()
    {
        int invokeCount = 0;
        var r = await Task.FromResult(Result.Failure<int>(TestError)).Recover((e) => { invokeCount++; return Result.Success(5); });
        Assert.Equal(1, invokeCount);
        r.ShouldBeSuccess();
        Assert.Equal(5, r.Value);
    }

    [Fact]
    public async Task Recover_Task_Generic_OnFailure_ReturnsFailure()
    {
        int invokeCount = 0;
        var r = await Task.FromResult(Result.Failure<int>(TestError)).Recover((e) => { invokeCount++; return Result.Failure<int>(TestError2); });
        Assert.Equal(1, invokeCount);
        r.ShouldBeFailure();
        Assert.Same(TestError2, r.Error);
    }

    [Fact]
    public async Task Recover_Task_Generic_WithState_OnSuccess_SkipsCallback()
    {
        int invokeCount = 0;
        var r = await Task.FromResult(Result.Success(5)).Recover(99, (state, e) => { invokeCount++; return Result.Success(5); });
        Assert.Equal(0, invokeCount);
        r.ShouldBeSuccess();
        Assert.Equal(5, r.Value);
    }

    [Fact]
    public async Task Recover_Task_Generic_WithState_OnFailure_ReturnsSuccess()
    {
        int invokeCount = 0;
        var r = await Task.FromResult(Result.Failure<int>(TestError)).Recover(99, (state, e) => { invokeCount++; return Result.Success(5); });
        Assert.Equal(1, invokeCount);
        r.ShouldBeSuccess();
        Assert.Equal(5, r.Value);
    }

    [Fact]
    public async Task Recover_Task_Generic_WithState_OnFailure_ReturnsFailure()
    {
        int invokeCount = 0;
        var r = await Task.FromResult(Result.Failure<int>(TestError)).Recover(99, (state, e) => { invokeCount++; return Result.Failure<int>(TestError2); });
        Assert.Equal(1, invokeCount);
        r.ShouldBeFailure();
        Assert.Same(TestError2, r.Error);
    }

    [Fact]
    public async Task Recover_Task_Generic_TaskCallback_OnSuccess_SkipsCallback()
    {
        int invokeCount = 0;
        var r = await Task.FromResult(Result.Success(5)).Recover((e) => { invokeCount++; return Task.FromResult(Result.Success(5)); });
        Assert.Equal(0, invokeCount);
        r.ShouldBeSuccess();
        Assert.Equal(5, r.Value);
    }

    [Fact]
    public async Task Recover_Task_Generic_TaskCallback_OnFailure_ReturnsSuccess()
    {
        int invokeCount = 0;
        var r = await Task.FromResult(Result.Failure<int>(TestError)).Recover((e) => { invokeCount++; return Task.FromResult(Result.Success(5)); });
        Assert.Equal(1, invokeCount);
        r.ShouldBeSuccess();
        Assert.Equal(5, r.Value);
    }

    [Fact]
    public async Task Recover_Task_Generic_TaskCallback_OnFailure_ReturnsFailure()
    {
        int invokeCount = 0;
        var r = await Task.FromResult(Result.Failure<int>(TestError)).Recover((e) => { invokeCount++; return Task.FromResult(Result.Failure<int>(TestError2)); });
        Assert.Equal(1, invokeCount);
        r.ShouldBeFailure();
        Assert.Same(TestError2, r.Error);
    }

    [Fact]
    public async Task Recover_Task_Generic_WithState_TaskCallback_OnSuccess_SkipsCallback()
    {
        int invokeCount = 0;
        var r = await Task.FromResult(Result.Success(5)).Recover(99, (state, e) => { invokeCount++; return Task.FromResult(Result.Success(5)); });
        Assert.Equal(0, invokeCount);
        r.ShouldBeSuccess();
        Assert.Equal(5, r.Value);
    }

    [Fact]
    public async Task Recover_Task_Generic_WithState_TaskCallback_OnFailure_ReturnsSuccess()
    {
        int invokeCount = 0;
        var r = await Task.FromResult(Result.Failure<int>(TestError)).Recover(99, (state, e) => { invokeCount++; return Task.FromResult(Result.Success(5)); });
        Assert.Equal(1, invokeCount);
        r.ShouldBeSuccess();
        Assert.Equal(5, r.Value);
    }

    [Fact]
    public async Task Recover_Task_Generic_WithState_TaskCallback_OnFailure_ReturnsFailure()
    {
        int invokeCount = 0;
        var r = await Task.FromResult(Result.Failure<int>(TestError)).Recover(99, (state, e) => { invokeCount++; return Task.FromResult(Result.Failure<int>(TestError2)); });
        Assert.Equal(1, invokeCount);
        r.ShouldBeFailure();
        Assert.Same(TestError2, r.Error);
    }

    [Fact]
    public async Task Recover_ValueTask_NonGeneric_OnSuccess_SkipsCallback()
    {
        int invokeCount = 0;
        var r = await new ValueTask<Result>(Result.Success()).Recover((e) => { invokeCount++; return Result.Success(); });
        Assert.Equal(0, invokeCount);
        r.ShouldBeSuccess();
    }

    [Fact]
    public async Task Recover_ValueTask_NonGeneric_OnFailure_ReturnsSuccess()
    {
        int invokeCount = 0;
        var r = await new ValueTask<Result>(Result.Failure(TestError)).Recover((e) => { invokeCount++; return Result.Success(); });
        Assert.Equal(1, invokeCount);
        r.ShouldBeSuccess();
    }

    [Fact]
    public async Task Recover_ValueTask_NonGeneric_OnFailure_ReturnsFailure()
    {
        int invokeCount = 0;
        var r = await new ValueTask<Result>(Result.Failure(TestError)).Recover((e) => { invokeCount++; return Result.Failure(TestError2); });
        Assert.Equal(1, invokeCount);
        r.ShouldBeFailure();
        Assert.Same(TestError2, r.Error);
    }

    [Fact]
    public async Task Recover_ValueTask_NonGeneric_WithState_OnSuccess_SkipsCallback()
    {
        int invokeCount = 0;
        var r = await new ValueTask<Result>(Result.Success()).Recover(99, (state, e) => { invokeCount++; return Result.Success(); });
        Assert.Equal(0, invokeCount);
        r.ShouldBeSuccess();
    }

    [Fact]
    public async Task Recover_ValueTask_NonGeneric_WithState_OnFailure_ReturnsSuccess()
    {
        int invokeCount = 0;
        var r = await new ValueTask<Result>(Result.Failure(TestError)).Recover(99, (state, e) => { invokeCount++; return Result.Success(); });
        Assert.Equal(1, invokeCount);
        r.ShouldBeSuccess();
    }

    [Fact]
    public async Task Recover_ValueTask_NonGeneric_WithState_OnFailure_ReturnsFailure()
    {
        int invokeCount = 0;
        var r = await new ValueTask<Result>(Result.Failure(TestError)).Recover(99, (state, e) => { invokeCount++; return Result.Failure(TestError2); });
        Assert.Equal(1, invokeCount);
        r.ShouldBeFailure();
        Assert.Same(TestError2, r.Error);
    }

    [Fact]
    public async Task Recover_ValueTask_NonGeneric_TaskCallback_OnSuccess_SkipsCallback()
    {
        int invokeCount = 0;
        var r = await new ValueTask<Result>(Result.Success()).Recover((e) => { invokeCount++; return new ValueTask<Result>(Result.Success()); });
        Assert.Equal(0, invokeCount);
        r.ShouldBeSuccess();
    }

    [Fact]
    public async Task Recover_ValueTask_NonGeneric_TaskCallback_OnFailure_ReturnsSuccess()
    {
        int invokeCount = 0;
        var r = await new ValueTask<Result>(Result.Failure(TestError)).Recover((e) => { invokeCount++; return new ValueTask<Result>(Result.Success()); });
        Assert.Equal(1, invokeCount);
        r.ShouldBeSuccess();
    }

    [Fact]
    public async Task Recover_ValueTask_NonGeneric_TaskCallback_OnFailure_ReturnsFailure()
    {
        int invokeCount = 0;
        var r = await new ValueTask<Result>(Result.Failure(TestError)).Recover((e) => { invokeCount++; return new ValueTask<Result>(Result.Failure(TestError2)); });
        Assert.Equal(1, invokeCount);
        r.ShouldBeFailure();
        Assert.Same(TestError2, r.Error);
    }

    [Fact]
    public async Task Recover_ValueTask_Generic_OnSuccess_SkipsCallback()
    {
        int invokeCount = 0;
        var r = await new ValueTask<Result<int>>(Result.Success(5)).Recover((e) => { invokeCount++; return Result.Success(5); });
        Assert.Equal(0, invokeCount);
        r.ShouldBeSuccess();
        Assert.Equal(5, r.Value);
    }

    [Fact]
    public async Task Recover_ValueTask_Generic_OnFailure_ReturnsSuccess()
    {
        int invokeCount = 0;
        var r = await new ValueTask<Result<int>>(Result.Failure<int>(TestError)).Recover((e) => { invokeCount++; return Result.Success(5); });
        Assert.Equal(1, invokeCount);
        r.ShouldBeSuccess();
        Assert.Equal(5, r.Value);
    }

    [Fact]
    public async Task Recover_ValueTask_Generic_OnFailure_ReturnsFailure()
    {
        int invokeCount = 0;
        var r = await new ValueTask<Result<int>>(Result.Failure<int>(TestError)).Recover((e) => { invokeCount++; return Result.Failure<int>(TestError2); });
        Assert.Equal(1, invokeCount);
        r.ShouldBeFailure();
        Assert.Same(TestError2, r.Error);
    }

    [Fact]
    public async Task Recover_ValueTask_Generic_WithState_OnSuccess_SkipsCallback()
    {
        int invokeCount = 0;
        var r = await new ValueTask<Result<int>>(Result.Success(5)).Recover(99, (state, e) => { invokeCount++; return Result.Success(5); });
        Assert.Equal(0, invokeCount);
        r.ShouldBeSuccess();
        Assert.Equal(5, r.Value);
    }

    [Fact]
    public async Task Recover_ValueTask_Generic_WithState_OnFailure_ReturnsSuccess()
    {
        int invokeCount = 0;
        var r = await new ValueTask<Result<int>>(Result.Failure<int>(TestError)).Recover(99, (state, e) => { invokeCount++; return Result.Success(5); });
        Assert.Equal(1, invokeCount);
        r.ShouldBeSuccess();
        Assert.Equal(5, r.Value);
    }

    [Fact]
    public async Task Recover_ValueTask_Generic_WithState_OnFailure_ReturnsFailure()
    {
        int invokeCount = 0;
        var r = await new ValueTask<Result<int>>(Result.Failure<int>(TestError)).Recover(99, (state, e) => { invokeCount++; return Result.Failure<int>(TestError2); });
        Assert.Equal(1, invokeCount);
        r.ShouldBeFailure();
        Assert.Same(TestError2, r.Error);
    }

    [Fact]
    public async Task Recover_ValueTask_Generic_TaskCallback_OnSuccess_SkipsCallback()
    {
        int invokeCount = 0;
        var r = await new ValueTask<Result<int>>(Result.Success(5)).Recover((e) => { invokeCount++; return new ValueTask<Result<int>>(Result.Success(5)); });
        Assert.Equal(0, invokeCount);
        r.ShouldBeSuccess();
        Assert.Equal(5, r.Value);
    }

    [Fact]
    public async Task Recover_ValueTask_Generic_TaskCallback_OnFailure_ReturnsSuccess()
    {
        int invokeCount = 0;
        var r = await new ValueTask<Result<int>>(Result.Failure<int>(TestError)).Recover((e) => { invokeCount++; return new ValueTask<Result<int>>(Result.Success(5)); });
        Assert.Equal(1, invokeCount);
        r.ShouldBeSuccess();
        Assert.Equal(5, r.Value);
    }

    [Fact]
    public async Task Recover_ValueTask_Generic_TaskCallback_OnFailure_ReturnsFailure()
    {
        int invokeCount = 0;
        var r = await new ValueTask<Result<int>>(Result.Failure<int>(TestError)).Recover((e) => { invokeCount++; return new ValueTask<Result<int>>(Result.Failure<int>(TestError2)); });
        Assert.Equal(1, invokeCount);
        r.ShouldBeFailure();
        Assert.Same(TestError2, r.Error);
    }

    [Fact]
    public async Task Recover_ValueTask_Generic_WithState_TaskCallback_OnSuccess_SkipsCallback()
    {
        int invokeCount = 0;
        var r = await new ValueTask<Result<int>>(Result.Success(5)).Recover(99, (state, e) => { invokeCount++; return new ValueTask<Result<int>>(Result.Success(5)); });
        Assert.Equal(0, invokeCount);
        r.ShouldBeSuccess();
        Assert.Equal(5, r.Value);
    }

    [Fact]
    public async Task Recover_ValueTask_Generic_WithState_TaskCallback_OnFailure_ReturnsSuccess()
    {
        int invokeCount = 0;
        var r = await new ValueTask<Result<int>>(Result.Failure<int>(TestError)).Recover(99, (state, e) => { invokeCount++; return new ValueTask<Result<int>>(Result.Success(5)); });
        Assert.Equal(1, invokeCount);
        r.ShouldBeSuccess();
        Assert.Equal(5, r.Value);
    }

    [Fact]
    public async Task Recover_ValueTask_Generic_WithState_TaskCallback_OnFailure_ReturnsFailure()
    {
        int invokeCount = 0;
        var r = await new ValueTask<Result<int>>(Result.Failure<int>(TestError)).Recover(99, (state, e) => { invokeCount++; return new ValueTask<Result<int>>(Result.Failure<int>(TestError2)); });
        Assert.Equal(1, invokeCount);
        r.ShouldBeFailure();
        Assert.Same(TestError2, r.Error);
    }
}




