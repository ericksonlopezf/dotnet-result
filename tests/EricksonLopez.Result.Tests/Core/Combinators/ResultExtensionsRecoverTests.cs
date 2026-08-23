// Copyright © Erickson Lopez. MIT License.
using System;
using System.Threading;
using System.Threading.Tasks;
using AwesomeAssertions;
using EricksonLopez.Result;
using EricksonLopez.Result.Testing;
using Xunit;

namespace EricksonLopez.Result.Tests.Core;

public class ResultExtensionsRecoverTests
{
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

}




