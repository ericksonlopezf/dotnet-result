// Copyright © Erickson Lopez. MIT License.
using System;
using System.Threading;
using System.Threading.Tasks;
using AwesomeAssertions;
using EricksonLopez.Result;
using EricksonLopez.Result.Testing;
using Xunit;

namespace EricksonLopez.Result.Tests.Core;

public class ResultExtensionsBindBehaviorTests : ResultExtensionsTestsBase
{
    [Fact]
    public void Bind_NonGeneric_OnSuccess_ReturnsSuccess()
    {
        int invokeCount = 0;
        var r = Result.Success().Bind(() => { invokeCount++; return Result.Success(); });
        invokeCount.Should().Be(1);
        r.ShouldBeSuccess();
    }

    [Fact]
    public void Bind_NonGeneric_OnFailure_ShortCircuits()
    {
        int invokeCount = 0;
        var r = Result.Failure(TestError).Bind(() => { invokeCount++; return Result.Success(); });
        invokeCount.Should().Be(0);
        r.ShouldBeFailure().Should().BeSameAs(TestError);
    }

    [Fact]
    public void Bind_NonGeneric_OnSuccess_ReturnsFailure()
    {
        int invokeCount = 0;
        var r = Result.Success().Bind(() => { invokeCount++; return Result.Failure(TestError2); });
        invokeCount.Should().Be(1);
        r.ShouldBeFailure().Should().BeSameAs(TestError2);
    }

    [Fact]
    public void Bind_NonGeneric_WithState_OnSuccess_ReturnsSuccess()
    {
        int invokeCount = 0;
        var r = Result.Success().Bind(99, state => { invokeCount++; return Result.Success(); });
        invokeCount.Should().Be(1);
        r.ShouldBeSuccess();
    }

    [Fact]
    public void Bind_NonGeneric_WithState_OnFailure_ShortCircuits()
    {
        int invokeCount = 0;
        var r = Result.Failure(TestError).Bind(99, state => { invokeCount++; return Result.Success(); });
        invokeCount.Should().Be(0);
        r.ShouldBeFailure().Should().BeSameAs(TestError);
    }

    [Fact]
    public void Bind_NonGeneric_WithState_OnSuccess_ReturnsFailure()
    {
        int invokeCount = 0;
        var r = Result.Success().Bind(99, state => { invokeCount++; return Result.Failure(TestError2); });
        invokeCount.Should().Be(1);
        r.ShouldBeFailure().Should().BeSameAs(TestError2);
    }

    [Fact]
    public void Bind_Generic_OnSuccess_ReturnsSuccess()
    {
        int invokeCount = 0;
        var r = Result.Success(5).Bind(v => { invokeCount++; return Result.Success("test"); });
        invokeCount.Should().Be(1);
        r.ShouldBeSuccess().Should().Be("test");
    }

    [Fact]
    public void Bind_Generic_OnFailure_ShortCircuits()
    {
        int invokeCount = 0;
        var r = Result.Failure<int>(TestError).Bind(v => { invokeCount++; return Result.Success("test"); });
        invokeCount.Should().Be(0);
        r.ShouldBeFailure().Should().BeSameAs(TestError);
    }

    [Fact]
    public void Bind_Generic_OnSuccess_ReturnsFailure()
    {
        int invokeCount = 0;
        var r = Result.Success(5).Bind(v => { invokeCount++; return Result.Failure<string>(TestError2); });
        invokeCount.Should().Be(1);
        r.ShouldBeFailure().Should().BeSameAs(TestError2);
    }

    [Fact]
    public void Bind_Generic_WithState_OnSuccess_ReturnsSuccess()
    {
        int invokeCount = 0;
        var r = Result.Success(5).Bind(99, (state, v) => { invokeCount++; return Result.Success("test"); });
        invokeCount.Should().Be(1);
        r.ShouldBeSuccess().Should().Be("test");
    }

    [Fact]
    public void Bind_Generic_WithState_OnFailure_ShortCircuits()
    {
        int invokeCount = 0;
        var r = Result.Failure<int>(TestError).Bind(99, (state, v) => { invokeCount++; return Result.Success("test"); });
        invokeCount.Should().Be(0);
        r.ShouldBeFailure().Should().BeSameAs(TestError);
    }

    [Fact]
    public void Bind_Generic_WithState_OnSuccess_ReturnsFailure()
    {
        int invokeCount = 0;
        var r = Result.Success(5).Bind(99, (state, v) => { invokeCount++; return Result.Failure<string>(TestError2); });
        invokeCount.Should().Be(1);
        r.ShouldBeFailure().Should().BeSameAs(TestError2);
    }

    [Fact]
    public async Task Bind_Task_NonGeneric_OnSuccess_ReturnsSuccess()
    {
        int invokeCount = 0;
        var r = await Task.FromResult(Result.Success()).Bind(() => { invokeCount++; return Result.Success(); });
        invokeCount.Should().Be(1);
        r.ShouldBeSuccess();
    }

    [Fact]
    public async Task Bind_Task_NonGeneric_OnFailure_ShortCircuits()
    {
        int invokeCount = 0;
        var r = await Task.FromResult(Result.Failure(TestError)).Bind(() => { invokeCount++; return Result.Success(); });
        invokeCount.Should().Be(0);
        r.ShouldBeFailure().Should().BeSameAs(TestError);
    }

    [Fact]
    public async Task Bind_Task_NonGeneric_OnSuccess_ReturnsFailure()
    {
        int invokeCount = 0;
        var r = await Task.FromResult(Result.Success()).Bind(() => { invokeCount++; return Result.Failure(TestError2); });
        invokeCount.Should().Be(1);
        r.ShouldBeFailure().Should().BeSameAs(TestError2);
    }

    [Fact]
    public async Task Bind_Task_NonGeneric_WithState_OnSuccess_ReturnsSuccess()
    {
        int invokeCount = 0;
        var r = await Task.FromResult(Result.Success()).Bind(99, state => { invokeCount++; return Result.Success(); });
        invokeCount.Should().Be(1);
        r.ShouldBeSuccess();
    }

    [Fact]
    public async Task Bind_Task_NonGeneric_WithState_OnFailure_ShortCircuits()
    {
        int invokeCount = 0;
        var r = await Task.FromResult(Result.Failure(TestError)).Bind(99, state => { invokeCount++; return Result.Success(); });
        invokeCount.Should().Be(0);
        r.ShouldBeFailure().Should().BeSameAs(TestError);
    }

    [Fact]
    public async Task Bind_Task_NonGeneric_WithState_OnSuccess_ReturnsFailure()
    {
        int invokeCount = 0;
        var r = await Task.FromResult(Result.Success()).Bind(99, state => { invokeCount++; return Result.Failure(TestError2); });
        invokeCount.Should().Be(1);
        r.ShouldBeFailure().Should().BeSameAs(TestError2);
    }

    [Fact]
    public async Task Bind_Task_NonGeneric_TaskCallback_OnSuccess_ReturnsSuccess()
    {
        int invokeCount = 0;
        var r = await Task.FromResult(Result.Success()).Bind(() => { invokeCount++; return Task.FromResult(Result.Success()); });
        invokeCount.Should().Be(1);
        r.ShouldBeSuccess();
    }

    [Fact]
    public async Task Bind_Task_NonGeneric_TaskCallback_OnFailure_ShortCircuits()
    {
        int invokeCount = 0;
        var r = await Task.FromResult(Result.Failure(TestError)).Bind(() => { invokeCount++; return Task.FromResult(Result.Success()); });
        invokeCount.Should().Be(0);
        r.ShouldBeFailure().Should().BeSameAs(TestError);
    }

    [Fact]
    public async Task Bind_Task_NonGeneric_TaskCallback_OnSuccess_ReturnsFailure()
    {
        int invokeCount = 0;
        var r = await Task.FromResult(Result.Success()).Bind(() => { invokeCount++; return Task.FromResult(Result.Failure(TestError2)); });
        invokeCount.Should().Be(1);
        r.ShouldBeFailure().Should().BeSameAs(TestError2);
    }

    [Fact]
    public async Task Bind_Task_Generic_OnSuccess_ReturnsSuccess()
    {
        int invokeCount = 0;
        var r = await Task.FromResult(Result.Success(5)).Bind(v => { invokeCount++; return Result.Success("test"); });
        invokeCount.Should().Be(1);
        r.ShouldBeSuccess().Should().Be("test");
    }

    [Fact]
    public async Task Bind_Task_Generic_OnFailure_ShortCircuits()
    {
        int invokeCount = 0;
        var r = await Task.FromResult(Result.Failure<int>(TestError)).Bind(v => { invokeCount++; return Result.Success("test"); });
        invokeCount.Should().Be(0);
        r.ShouldBeFailure().Should().BeSameAs(TestError);
    }

    [Fact]
    public async Task Bind_Task_Generic_OnSuccess_ReturnsFailure()
    {
        int invokeCount = 0;
        var r = await Task.FromResult(Result.Success(5)).Bind(v => { invokeCount++; return Result.Failure<string>(TestError2); });
        invokeCount.Should().Be(1);
        r.ShouldBeFailure().Should().BeSameAs(TestError2);
    }

    [Fact]
    public async Task Bind_Task_Generic_WithState_OnSuccess_ReturnsSuccess()
    {
        int invokeCount = 0;
        var r = await Task.FromResult(Result.Success(5)).Bind(99, (state, v) => { invokeCount++; return Result.Success("test"); });
        invokeCount.Should().Be(1);
        r.ShouldBeSuccess().Should().Be("test");
    }

    [Fact]
    public async Task Bind_Task_Generic_WithState_OnFailure_ShortCircuits()
    {
        int invokeCount = 0;
        var r = await Task.FromResult(Result.Failure<int>(TestError)).Bind(99, (state, v) => { invokeCount++; return Result.Success("test"); });
        invokeCount.Should().Be(0);
        r.ShouldBeFailure().Should().BeSameAs(TestError);
    }

    [Fact]
    public async Task Bind_Task_Generic_WithState_OnSuccess_ReturnsFailure()
    {
        int invokeCount = 0;
        var r = await Task.FromResult(Result.Success(5)).Bind(99, (state, v) => { invokeCount++; return Result.Failure<string>(TestError2); });
        invokeCount.Should().Be(1);
        r.ShouldBeFailure().Should().BeSameAs(TestError2);
    }

    [Fact]
    public async Task Bind_Task_Generic_TaskCallback_OnSuccess_ReturnsSuccess()
    {
        int invokeCount = 0;
        var r = await Task.FromResult(Result.Success(5)).Bind(v => { invokeCount++; return Task.FromResult(Result.Success("test")); });
        invokeCount.Should().Be(1);
        r.ShouldBeSuccess().Should().Be("test");
    }

    [Fact]
    public async Task Bind_Task_Generic_TaskCallback_OnFailure_ShortCircuits()
    {
        int invokeCount = 0;
        var r = await Task.FromResult(Result.Failure<int>(TestError)).Bind(v => { invokeCount++; return Task.FromResult(Result.Success("test")); });
        invokeCount.Should().Be(0);
        r.ShouldBeFailure().Should().BeSameAs(TestError);
    }

    [Fact]
    public async Task Bind_Task_Generic_TaskCallback_OnSuccess_ReturnsFailure()
    {
        int invokeCount = 0;
        var r = await Task.FromResult(Result.Success(5)).Bind(v => { invokeCount++; return Task.FromResult(Result.Failure<string>(TestError2)); });
        invokeCount.Should().Be(1);
        r.ShouldBeFailure().Should().BeSameAs(TestError2);
    }

    [Fact]
    public async Task Bind_ValueTask_NonGeneric_OnSuccess_ReturnsSuccess()
    {
        int invokeCount = 0;
        var r = await new ValueTask<Result>(Result.Success()).Bind(() => { invokeCount++; return Result.Success(); });
        invokeCount.Should().Be(1);
        r.ShouldBeSuccess();
    }

    [Fact]
    public async Task Bind_ValueTask_NonGeneric_OnFailure_ShortCircuits()
    {
        int invokeCount = 0;
        var r = await new ValueTask<Result>(Result.Failure(TestError)).Bind(() => { invokeCount++; return Result.Success(); });
        invokeCount.Should().Be(0);
        r.ShouldBeFailure().Should().BeSameAs(TestError);
    }

    [Fact]
    public async Task Bind_ValueTask_NonGeneric_OnSuccess_ReturnsFailure()
    {
        int invokeCount = 0;
        var r = await new ValueTask<Result>(Result.Success()).Bind(() => { invokeCount++; return Result.Failure(TestError2); });
        invokeCount.Should().Be(1);
        r.ShouldBeFailure().Should().BeSameAs(TestError2);
    }

    [Fact]
    public async Task Bind_ValueTask_NonGeneric_WithState_OnSuccess_ReturnsSuccess()
    {
        int invokeCount = 0;
        var r = await new ValueTask<Result>(Result.Success()).Bind(99, state => { invokeCount++; return Result.Success(); });
        invokeCount.Should().Be(1);
        r.ShouldBeSuccess();
    }

    [Fact]
    public async Task Bind_ValueTask_NonGeneric_WithState_OnFailure_ShortCircuits()
    {
        int invokeCount = 0;
        var r = await new ValueTask<Result>(Result.Failure(TestError)).Bind(99, state => { invokeCount++; return Result.Success(); });
        invokeCount.Should().Be(0);
        r.ShouldBeFailure().Should().BeSameAs(TestError);
    }

    [Fact]
    public async Task Bind_ValueTask_NonGeneric_WithState_OnSuccess_ReturnsFailure()
    {
        int invokeCount = 0;
        var r = await new ValueTask<Result>(Result.Success()).Bind(99, state => { invokeCount++; return Result.Failure(TestError2); });
        invokeCount.Should().Be(1);
        r.ShouldBeFailure().Should().BeSameAs(TestError2);
    }

    [Fact]
    public async Task Bind_ValueTask_NonGeneric_TaskCallback_OnSuccess_ReturnsSuccess()
    {
        int invokeCount = 0;
        var r = await new ValueTask<Result>(Result.Success()).Bind(() => { invokeCount++; return new ValueTask<Result>(Result.Success()); });
        invokeCount.Should().Be(1);
        r.ShouldBeSuccess();
    }

    [Fact]
    public async Task Bind_ValueTask_NonGeneric_TaskCallback_OnFailure_ShortCircuits()
    {
        int invokeCount = 0;
        var r = await new ValueTask<Result>(Result.Failure(TestError)).Bind(() => { invokeCount++; return new ValueTask<Result>(Result.Success()); });
        invokeCount.Should().Be(0);
        r.ShouldBeFailure().Should().BeSameAs(TestError);
    }

    [Fact]
    public async Task Bind_ValueTask_NonGeneric_TaskCallback_OnSuccess_ReturnsFailure()
    {
        int invokeCount = 0;
        var r = await new ValueTask<Result>(Result.Success()).Bind(() => { invokeCount++; return new ValueTask<Result>(Result.Failure(TestError2)); });
        invokeCount.Should().Be(1);
        r.ShouldBeFailure().Should().BeSameAs(TestError2);
    }

    [Fact]
    public async Task Bind_ValueTask_Generic_OnSuccess_ReturnsSuccess()
    {
        int invokeCount = 0;
        var r = await new ValueTask<Result<int>>(Result.Success(5)).Bind(v => { invokeCount++; return Result.Success("test"); });
        invokeCount.Should().Be(1);
        r.ShouldBeSuccess().Should().Be("test");
    }

    [Fact]
    public async Task Bind_ValueTask_Generic_OnFailure_ShortCircuits()
    {
        int invokeCount = 0;
        var r = await new ValueTask<Result<int>>(Result.Failure<int>(TestError)).Bind(v => { invokeCount++; return Result.Success("test"); });
        invokeCount.Should().Be(0);
        r.ShouldBeFailure().Should().BeSameAs(TestError);
    }

    [Fact]
    public async Task Bind_ValueTask_Generic_OnSuccess_ReturnsFailure()
    {
        int invokeCount = 0;
        var r = await new ValueTask<Result<int>>(Result.Success(5)).Bind(v => { invokeCount++; return Result.Failure<string>(TestError2); });
        invokeCount.Should().Be(1);
        r.ShouldBeFailure().Should().BeSameAs(TestError2);
    }

    [Fact]
    public async Task Bind_ValueTask_Generic_WithState_OnSuccess_ReturnsSuccess()
    {
        int invokeCount = 0;
        var r = await new ValueTask<Result<int>>(Result.Success(5)).Bind(99, (state, v) => { invokeCount++; return Result.Success("test"); });
        invokeCount.Should().Be(1);
        r.ShouldBeSuccess().Should().Be("test");
    }

    [Fact]
    public async Task Bind_ValueTask_Generic_WithState_OnFailure_ShortCircuits()
    {
        int invokeCount = 0;
        var r = await new ValueTask<Result<int>>(Result.Failure<int>(TestError)).Bind(99, (state, v) => { invokeCount++; return Result.Success("test"); });
        invokeCount.Should().Be(0);
        r.ShouldBeFailure().Should().BeSameAs(TestError);
    }

    [Fact]
    public async Task Bind_ValueTask_Generic_WithState_OnSuccess_ReturnsFailure()
    {
        int invokeCount = 0;
        var r = await new ValueTask<Result<int>>(Result.Success(5)).Bind(99, (state, v) => { invokeCount++; return Result.Failure<string>(TestError2); });
        invokeCount.Should().Be(1);
        r.ShouldBeFailure().Should().BeSameAs(TestError2);
    }

    [Fact]
    public async Task Bind_ValueTask_Generic_TaskCallback_OnSuccess_ReturnsSuccess()
    {
        int invokeCount = 0;
        var r = await new ValueTask<Result<int>>(Result.Success(5)).Bind(v => { invokeCount++; return new ValueTask<Result<string>>(Result.Success("test")); });
        invokeCount.Should().Be(1);
        r.ShouldBeSuccess().Should().Be("test");
    }

    [Fact]
    public async Task Bind_ValueTask_Generic_TaskCallback_OnFailure_ShortCircuits()
    {
        int invokeCount = 0;
        var r = await new ValueTask<Result<int>>(Result.Failure<int>(TestError)).Bind(v => { invokeCount++; return new ValueTask<Result<string>>(Result.Success("test")); });
        invokeCount.Should().Be(0);
        r.ShouldBeFailure().Should().BeSameAs(TestError);
    }

    [Fact]
    public async Task Bind_ValueTask_Generic_TaskCallback_OnSuccess_ReturnsFailure()
    {
        int invokeCount = 0;
        var r = await new ValueTask<Result<int>>(Result.Success(5)).Bind(v => { invokeCount++; return new ValueTask<Result<string>>(Result.Failure<string>(TestError2)); });
        invokeCount.Should().Be(1);
        r.ShouldBeFailure().Should().BeSameAs(TestError2);
    }

    [Fact]
    public void NonGenericResult_Bind_ChainsPipeline()
    {
        var result = Result.Success()
            .Bind(() => Result.Success(100));

        result.ShouldBeSuccess().Should().Be(100);
    }

    [Fact]
    public void BindTState_Uninitialized_Throws()
    {
        Result result = default;
        Assert.Throws<InvalidOperationException>(() => result.Bind(1, _ => Result.Success()));
    }
}
