// Copyright © Erickson Lopez. MIT License.
#pragma warning disable CA2012
using System;
using System.Threading;
using System.Threading.Tasks;
using EricksonLopez.Result;
using EricksonLopez.Result.Testing;
using Xunit;

namespace EricksonLopez.Result.Tests.Core;

public class ResultExtensionsSlowPathTests
{
    [Fact]
    public async Task Map_Task_SlowPath_ExecutesCorrectly()
    {
        var result = await 1.AsAsyncResult().Map(x => x + 1);
        result.ShouldBeSuccess();
        Assert.Equal(2, result.Value);
    }

    [Fact]
    public async Task Map_ValueTask_SlowPath_ExecutesCorrectly()
    {
        var result = await 1.AsAsyncValueTaskResult().Map(x => x + 1);
        result.ShouldBeSuccess();
        Assert.Equal(2, result.Value);
    }

    [Fact]
    public async Task Bind_Task_SlowPath_ExecutesCorrectly()
    {
        var result = await 1.AsAsyncResult().Bind(x => Result.Success(x + 1));
        result.ShouldBeSuccess();
        Assert.Equal(2, result.Value);
    }

    [Fact]
    public async Task Bind_ValueTask_SlowPath_ExecutesCorrectly()
    {
        var result = await 1.AsAsyncValueTaskResult().Bind(x => Result.Success(x + 1));
        result.ShouldBeSuccess();
        Assert.Equal(2, result.Value);
    }

    [Fact]
    public async Task Bind_Task_AsyncBind_SlowPath_ExecutesCorrectly()
    {
        var result = await 1.AsAsyncResult().Bind(x => (x + 1).AsAsyncResult());
        result.ShouldBeSuccess();
        Assert.Equal(2, result.Value);
    }

    [Fact]
    public async Task Bind_ValueTask_AsyncBind_SlowPath_ExecutesCorrectly()
    {
        var result = await 1.AsAsyncValueTaskResult().Bind(x => (x + 1).AsAsyncValueTaskResult());
        result.ShouldBeSuccess();
        Assert.Equal(2, result.Value);
    }

    [Fact]
    public async Task Ensure_Task_SlowPath_ExecutesCorrectly()
    {
        var result = await 1.AsAsyncResult().Ensure(x => x > 0, Error.Failure("E", "M"));
        result.ShouldBeSuccess();
    }

    [Fact]
    public async Task Ensure_ValueTask_SlowPath_ExecutesCorrectly()
    {
        var result = await 1.AsAsyncValueTaskResult().Ensure(x => x > 0, Error.Failure("E", "M"));
        result.ShouldBeSuccess();
    }

    [Fact]
    public async Task Tap_Task_SlowPath_ExecutesCorrectly()
    {
        bool called = false;
        var result = await 1.AsAsyncResult().TapOnSuccess(x => { called = true; });
        result.ShouldBeSuccess();
        Assert.True(called);
    }

    [Fact]
    public async Task Tap_ValueTask_SlowPath_ExecutesCorrectly()
    {
        bool called = false;
        var result = await 1.AsAsyncValueTaskResult().TapOnSuccess(x => { called = true; });
        result.ShouldBeSuccess();
        Assert.True(called);
    }

    [Fact]
    public async Task Recover_Task_SlowPath_ExecutesCorrectly()
    {
        var error = Error.Failure("E", "M");
        var result = await error.AsAsyncFailedResult<int>().Recover(e => Result.Success(2));
        result.ShouldBeSuccess();
        Assert.Equal(2, result.Value);
    }

    [Fact]
    public async Task Recover_ValueTask_SlowPath_ExecutesCorrectly()
    {
        var error = Error.Failure("E", "M");
        var result = await error.AsAsyncFailedValueTaskResult<int>().Recover(e => Result.Success(2));
        result.ShouldBeSuccess();
        Assert.Equal(2, result.Value);
    }

    [Fact]
    public async Task MapError_Task_SlowPath_ExecutesCorrectly()
    {
        var error = Error.Failure("E", "M");
        var result = await error.AsAsyncFailedResult<int>().MapError(e => Error.Failure("E2", "M2"));
        result.ShouldBeFailure();
        Assert.Equal("E2", result.Error.Code);
    }

    [Fact]
    public async Task MapError_ValueTask_SlowPath_ExecutesCorrectly()
    {
        var error = Error.Failure("E", "M");
        var result = await error.AsAsyncFailedValueTaskResult<int>().MapError(e => Error.Failure("E2", "M2"));
        result.ShouldBeFailure();
        Assert.Equal("E2", result.Error.Code);
    }
}




