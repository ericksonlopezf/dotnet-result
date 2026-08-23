// Copyright © Erickson Lopez. MIT License.
using System;
using System.Threading;
using System.Threading.Tasks;
using AwesomeAssertions;
using EricksonLopez.Result;
using EricksonLopez.Result.Testing;
using Xunit;

namespace EricksonLopez.Result.Tests.Core;

public class ResultExtensionsEnsureTests
{
    [Fact]
    public async Task Ensure_Sync_Success_CompletedTask_ReturnsOriginalIfPredicateTrue()
    {
        var task = Task.FromResult(Result.Success(1));
        var result = await task.Ensure(v => v == 1, Error.Failure("e", "m"));
        result.ShouldBeSuccess().Should().Be(1);
    }

    [Fact]
    public async Task Ensure_Sync_Success_CompletedTask_ReturnsFailureIfPredicateFalse()
    {
        var task = Task.FromResult(Result.Success(1));
        var result = await task.Ensure(v => v != 1, Error.Failure("e", "m"));
        result.ShouldBeFailure().Code.Should().Be("e");
    }

    [Fact]
    public async Task Ensure_Sync_Success_IncompleteTask_ReturnsOriginalIfPredicateTrue()
    {
        var task = Task.Run(async () => { await Task.Yield(); return Result.Success(1); });
        var result = await task.Ensure(v => v == 1, Error.Failure("e", "m"));
        result.ShouldBeSuccess().Should().Be(1);
    }

    [Fact]
    public async Task Ensure_WithState_Sync_Success_CompletedTask_ReturnsOriginalIfPredicateTrue()
    {
        var task = Task.FromResult(Result.Success(1));
        var result = await task.Ensure(10, (s, v) => s == 10, Error.Failure("e", "m"));
        result.ShouldBeSuccess().Should().Be(1);
    }

    [Fact]
    public async Task Ensure_WithState_Sync_Success_IncompleteTask_ReturnsOriginalIfPredicateTrue()
    {
        var task = Task.Run(async () => { await Task.Yield(); return Result.Success(1); });
        var result = await task.Ensure(10, (s, v) => s == 10, Error.Failure("e", "m"));
        result.ShouldBeSuccess().Should().Be(1);
    }

    [Fact]
    public async Task Ensure_Async_Success_CompletedTask_ReturnsOriginalIfPredicateTrue()
    {
        var task = Task.FromResult(Result.Success(1));
        var result = await task.Ensure(async v => { await Task.Yield(); return v == 1; }, Error.Failure("e", "m"));
        result.ShouldBeSuccess().Should().Be(1);
    }

    [Fact]
    public async Task Ensure_Async_Failure_CompletedTask_ReturnsFailure()
    {
        var task = Task.FromResult(Result.Failure<int>(Error.Failure("err", "m")));
        var result = await task.Ensure(async v => { await Task.Yield(); return v == 1; }, Error.Failure("e", "m"));
        result.ShouldBeFailure().Code.Should().Be("err");
    }

    [Fact]
    public async Task Ensure_Async_Success_CompletedTask_ReturnsFailureIfPredicateFalse()
    {
        var task = Task.FromResult(Result.Success(1));
        var result = await task.Ensure(async v => { await Task.Yield(); return v != 1; }, Error.Failure("e", "m"));
        result.ShouldBeFailure().Code.Should().Be("e");
    }

    [Fact]
    public async Task Ensure_Async_Success_IncompleteTask_ReturnsOriginalIfPredicateTrue()
    {
        var task = Task.Run(async () => { await Task.Yield(); return Result.Success(1); });
        var result = await task.Ensure(async v => { await Task.Yield(); return v == 1; }, Error.Failure("e", "m"));
        result.ShouldBeSuccess().Should().Be(1);
    }

    [Fact]
    public async Task Ensure_Async_Failure_IncompleteTask_ReturnsFailure()
    {
        var task = Task.Run(async () => { await Task.Yield(); return Result.Failure<int>(Error.Failure("err", "m")); });
        var result = await task.Ensure(async v => { await Task.Yield(); return v == 1; }, Error.Failure("e", "m"));
        result.ShouldBeFailure().Code.Should().Be("err");
    }

    [Fact]
    public async Task Ensure_WithState_Async_Success_CompletedTask_ReturnsOriginalIfPredicateTrue()
    {
        var task = Task.FromResult(Result.Success(1));
        var result = await task.Ensure(10, async (s, v) => { await Task.Yield(); return s == 10; }, Error.Failure("e", "m"));
        result.ShouldBeSuccess().Should().Be(1);
    }

    [Fact]
    public async Task Ensure_WithState_Async_Failure_CompletedTask_ReturnsFailure()
    {
        var task = Task.FromResult(Result.Failure<int>(Error.Failure("err", "m")));
        var result = await task.Ensure(10, async (s, v) => { await Task.Yield(); return s == 10; }, Error.Failure("e", "m"));
        result.ShouldBeFailure().Code.Should().Be("err");
    }

    [Fact]
    public async Task Ensure_WithState_Async_Success_CompletedTask_ReturnsFailureIfPredicateFalse()
    {
        var task = Task.FromResult(Result.Success(1));
        var result = await task.Ensure(10, async (s, v) => { await Task.Yield(); return s != 10; }, Error.Failure("e", "m"));
        result.ShouldBeFailure().Code.Should().Be("e");
    }

    [Fact]
    public async Task Ensure_WithState_Async_Success_IncompleteTask_ReturnsOriginalIfPredicateTrue()
    {
        var task = Task.Run(async () => { await Task.Yield(); return Result.Success(1); });
        var result = await task.Ensure(10, async (s, v) => { await Task.Yield(); return s == 10; }, Error.Failure("e", "m"));
        result.ShouldBeSuccess().Should().Be(1);
    }

}




