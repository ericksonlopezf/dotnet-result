// Copyright © Erickson Lopez. MIT License.
using System;
using System.Threading;
using System.Threading.Tasks;
using AwesomeAssertions;
using EricksonLopez.Result;
using EricksonLopez.Result.Testing;
using Xunit;

namespace EricksonLopez.Result.Tests.Core;

public class ResultExtensionsEnsureRecoverTests
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

    [Fact]
    public async Task Recover_Sync_Failure_CompletedTask_ReturnsRecoveredResult()
    {
        var task = Task.FromResult(Result.Failure<int>(Error.Failure("e", "m")));
        var result = await task.Recover(e => Result.Success(2));
        result.ShouldBeSuccess().Should().Be(2);
    }

    [Fact]
    public async Task Recover_Sync_Failure_IncompleteTask_ReturnsRecoveredResult()
    {
        var task = Task.Run(async () => { await Task.Yield(); return Result.Failure<int>(Error.Failure("e", "m")); });
        var result = await task.Recover(e => Result.Success(2));
        result.ShouldBeSuccess().Should().Be(2);
    }

    [Fact]
    public async Task Recover_WithState_Sync_Failure_CompletedTask_ReturnsRecoveredResult()
    {
        var task = Task.FromResult(Result.Failure<int>(Error.Failure("e", "m")));
        var result = await task.Recover(10, (s, e) => Result.Success(s));
        result.ShouldBeSuccess().Should().Be(10);
    }

    [Fact]
    public async Task Recover_WithState_Sync_Failure_IncompleteTask_ReturnsRecoveredResult()
    {
        var task = Task.Run(async () => { await Task.Yield(); return Result.Failure<int>(Error.Failure("e", "m")); });
        var result = await task.Recover(10, (s, e) => Result.Success(s));
        result.ShouldBeSuccess().Should().Be(10);
    }

    [Fact]
    public async Task Recover_Async_Failure_CompletedTask_ReturnsRecoveredResult()
    {
        var task = Task.FromResult(Result.Failure<int>(Error.Failure("e", "m")));
        var result = await task.Recover(async e => { await Task.Yield(); return Result.Success(2); });
        result.ShouldBeSuccess().Should().Be(2);
    }

    [Fact]
    public async Task Recover_Async_Success_CompletedTask_ReturnsOriginal()
    {
        var task = Task.FromResult(Result.Success(1));
        var result = await task.Recover(async e => { await Task.Yield(); return Result.Success(2); });
        result.ShouldBeSuccess().Should().Be(1);
    }

    [Fact]
    public async Task Recover_Async_Failure_IncompleteTask_ReturnsRecoveredResult()
    {
        var task = Task.Run(async () => { await Task.Yield(); return Result.Failure<int>(Error.Failure("e", "m")); });
        var result = await task.Recover(async e => { await Task.Yield(); return Result.Success(2); });
        result.ShouldBeSuccess().Should().Be(2);
    }

    [Fact]
    public async Task Recover_Async_Success_IncompleteTask_ReturnsOriginal()
    {
        var task = Task.Run(async () => { await Task.Yield(); return Result.Success(1); });
        var result = await task.Recover(async e => { await Task.Yield(); return Result.Success(2); });
        result.ShouldBeSuccess().Should().Be(1);
    }

    [Fact]
    public async Task Recover_WithState_Async_Failure_CompletedTask_ReturnsRecoveredResult()
    {
        var task = Task.FromResult(Result.Failure<int>(Error.Failure("e", "m")));
        var result = await task.Recover(10, async (s, e) => { await Task.Yield(); return Result.Success(s); });
        result.ShouldBeSuccess().Should().Be(10);
    }

    [Fact]
    public async Task Recover_WithState_Async_Success_CompletedTask_ReturnsOriginal()
    {
        var task = Task.FromResult(Result.Success(1));
        var result = await task.Recover(10, async (s, e) => { await Task.Yield(); return Result.Success(s); });
        result.ShouldBeSuccess().Should().Be(1);
    }

    [Fact]
    public async Task Recover_WithState_Async_Failure_IncompleteTask_ReturnsRecoveredResult()
    {
        var task = Task.Run(async () => { await Task.Yield(); return Result.Failure<int>(Error.Failure("e", "m")); });
        var result = await task.Recover(10, async (s, e) => { await Task.Yield(); return Result.Success(s); });
        result.ShouldBeSuccess().Should().Be(10);
    }

    [Fact]
    public async Task Recover_WithState_Async_Success_IncompleteTask_ReturnsOriginal()
    {
        var task = Task.Run(async () => { await Task.Yield(); return Result.Success(1); });
        var result = await task.Recover(10, async (s, e) => { await Task.Yield(); return Result.Success(s); });
        result.ShouldBeSuccess().Should().Be(1);
    }

    // MapError
    [Fact]
    public async Task MapError_Sync_Failure_CompletedTask_ReturnsMappedError()
    {
        var task = Task.FromResult(Result.Failure<int>(Error.Failure("e", "m")));
        var result = await task.MapError(e => Error.Failure("e2", "m2"));
        result.ShouldBeFailure().Code.Should().Be("e2");
    }

    [Fact]
    public async Task MapError_Sync_Failure_IncompleteTask_ReturnsMappedError()
    {
        var task = Task.Run(async () => { await Task.Yield(); return Result.Failure<int>(Error.Failure("e", "m")); });
        var result = await task.MapError(e => Error.Failure("e2", "m2"));
        result.ShouldBeFailure().Code.Should().Be("e2");
    }

    [Fact]
    public async Task MapError_WithState_Sync_Failure_CompletedTask_ReturnsMappedError()
    {
        var task = Task.FromResult(Result.Failure<int>(Error.Failure("e", "m")));
        var result = await task.MapError("e2", (s, e) => Error.Failure(s, "m2"));
        result.ShouldBeFailure().Code.Should().Be("e2");
    }

    [Fact]
    public async Task MapError_WithState_Sync_Failure_IncompleteTask_ReturnsMappedError()
    {
        var task = Task.Run(async () => { await Task.Yield(); return Result.Failure<int>(Error.Failure("e", "m")); });
        var result = await task.MapError("e2", (s, e) => Error.Failure(s, "m2"));
        result.ShouldBeFailure().Code.Should().Be("e2");
    }
}




