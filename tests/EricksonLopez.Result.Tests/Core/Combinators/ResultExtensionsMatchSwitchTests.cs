// Copyright © Erickson Lopez. MIT License.
using System;
using System.Threading;
using System.Threading.Tasks;
using AwesomeAssertions;
using EricksonLopez.Result;
using EricksonLopez.Result.Testing;
using Xunit;

namespace EricksonLopez.Result.Tests.Core;

public class ResultExtensionsMatchExecuteTests
{

    [Fact]
    public async Task Match_Sync_Success_CompletedTask_ReturnsSuccessValue()
    {
        var task = Task.FromResult(Result.Success(1));
        var result = await task.Match(v => v * 2, e => 0);
        result.Should().Be(2);
    }

    [Fact]
    public async Task Match_Sync_Failure_CompletedTask_ReturnsFailureValue()
    {
        var task = Task.FromResult(Result.Failure<int>(Error.Failure("e", "m")));
        var result = await task.Match(v => v * 2, e => 0);
        result.Should().Be(0);
    }

    [Fact]
    public async Task Match_Sync_Success_IncompleteTask_ReturnsSuccessValue()
    {
        var task = Task.Run(async () => { await Task.Yield(); return Result.Success(1); });
        var result = await task.Match(v => v * 2, e => 0);
        result.Should().Be(2);
    }

    [Fact]
    public async Task Match_WithState_Sync_Success_CompletedTask_ReturnsSuccessValue()
    {
        var task = Task.FromResult(Result.Success(1));
        var result = await task.Match(10, (s, v) => v + s, (s, e) => s);
        result.Should().Be(11);
    }

    [Fact]
    public async Task Match_WithState_Sync_Success_IncompleteTask_ReturnsSuccessValue()
    {
        var task = Task.Run(async () => { await Task.Yield(); return Result.Success(1); });
        var result = await task.Match(10, (s, v) => v + s, (s, e) => s);
        result.Should().Be(11);
    }

    // Match <TOut> (non-generic result)
    [Fact]
    public async Task MatchNonGeneric_Sync_Success_CompletedTask_ReturnsSuccessValue()
    {
        var task = Task.FromResult(Result.Success());
        var result = await task.Match(() => 2, e => 0);
        result.Should().Be(2);
    }

    [Fact]
    public async Task MatchNonGeneric_Sync_Success_IncompleteTask_ReturnsSuccessValue()
    {
        var task = Task.Run(async () => { await Task.Yield(); return Result.Success(); });
        var result = await task.Match(() => 2, e => 0);
        result.Should().Be(2);
    }

    [Fact]
    public async Task MatchNonGeneric_WithState_Sync_Success_CompletedTask_ReturnsSuccessValue()
    {
        var task = Task.FromResult(Result.Success());
        var result = await task.Match(10, s => s + 2, (s, e) => 0);
        result.Should().Be(12);
    }

    [Fact]
    public async Task MatchNonGeneric_WithState_Sync_Success_IncompleteTask_ReturnsSuccessValue()
    {
        var task = Task.Run(async () => { await Task.Yield(); return Result.Success(); });
        var result = await task.Match(10, s => s + 2, (s, e) => 0);
        result.Should().Be(12);
    }

    // Switch <T>
    [Fact]
    public async Task Switch_Sync_Success_CompletedTask_InvokesSuccess()
    {
        var task = Task.FromResult(Result.Success(1));
        bool successInvoked = false, failureInvoked = false;
        await task.Execute(v => successInvoked = true, e => failureInvoked = true);
        successInvoked.Should().BeTrue();
        failureInvoked.Should().BeFalse();
    }

    [Fact]
    public async Task Switch_Sync_Failure_CompletedTask_InvokesFailure()
    {
        var task = Task.FromResult(Result.Failure<int>(Error.Failure("e", "m")));
        bool successInvoked = false, failureInvoked = false;
        await task.Execute(v => successInvoked = true, e => failureInvoked = true);
        successInvoked.Should().BeFalse();
        failureInvoked.Should().BeTrue();
    }

    [Fact]
    public async Task Switch_Sync_Success_IncompleteTask_InvokesSuccess()
    {
        var task = Task.Run(async () => { await Task.Yield(); return Result.Success(1); });
        bool successInvoked = false, failureInvoked = false;
        await task.Execute(v => successInvoked = true, e => failureInvoked = true);
        successInvoked.Should().BeTrue();
        failureInvoked.Should().BeFalse();
    }

    [Fact]
    public async Task Switch_WithState_Sync_Success_CompletedTask_InvokesSuccess()
    {
        var task = Task.FromResult(Result.Success(1));
        bool successInvoked = false, failureInvoked = false;
        await task.Execute(10, (s, v) => successInvoked = true, (s, e) => failureInvoked = true);
        successInvoked.Should().BeTrue();
        failureInvoked.Should().BeFalse();
    }

    [Fact]
    public async Task Switch_WithState_Sync_Success_IncompleteTask_InvokesSuccess()
    {
        var task = Task.Run(async () => { await Task.Yield(); return Result.Success(1); });
        bool successInvoked = false, failureInvoked = false;
        await task.Execute(10, (s, v) => successInvoked = true, (s, e) => failureInvoked = true);
        successInvoked.Should().BeTrue();
        failureInvoked.Should().BeFalse();
    }

    // Switch (non-generic)
    [Fact]
    public async Task SwitchNonGeneric_Sync_Success_CompletedTask_InvokesSuccess()
    {
        var task = Task.FromResult(Result.Success());
        bool successInvoked = false, failureInvoked = false;
        await task.Execute(() => successInvoked = true, e => failureInvoked = true);
        successInvoked.Should().BeTrue();
        failureInvoked.Should().BeFalse();
    }

    [Fact]
    public async Task SwitchNonGeneric_Sync_IncompleteTask_InvokesSuccess()
    {
        var task = Task.Run(async () => { await Task.Yield(); return Result.Success(); });
        bool successInvoked = false, failureInvoked = false;
        await task.Execute(() => successInvoked = true, e => failureInvoked = true);
        successInvoked.Should().BeTrue();
        failureInvoked.Should().BeFalse();
    }

    [Fact]
    public async Task SwitchNonGeneric_WithState_Sync_Success_CompletedTask_InvokesSuccess()
    {
        var task = Task.FromResult(Result.Success());
        bool successInvoked = false, failureInvoked = false;
        await task.Execute(10, s => successInvoked = true, (s, e) => failureInvoked = true);
        successInvoked.Should().BeTrue();
        failureInvoked.Should().BeFalse();
    }

    [Fact]
    public async Task SwitchNonGeneric_WithState_Sync_IncompleteTask_InvokesSuccess()
    {
        var task = Task.Run(async () => { await Task.Yield(); return Result.Success(); });
        bool successInvoked = false, failureInvoked = false;
        await task.Execute(10, s => successInvoked = true, (s, e) => failureInvoked = true);
        successInvoked.Should().BeTrue();
        failureInvoked.Should().BeFalse();
    }


    [Fact]
    public void ResultT_Switch_ExecutesCorrectBranch()
    {
        var success = Result.Success(42);
        var failure = Result.Failure<int>(Error.Unexpected("Err", "Msg"));

        int successVal = 0;
        success.Execute(v => successVal = v, _ => Assert.Fail("Should not call failure"));
        Assert.Equal(42, successVal);

        string? errorVal = null;
        failure.Execute(_ => Assert.Fail("Should not call success"), e => errorVal = e.Code);
        Assert.Equal("Err", errorVal);
    }

    [Fact]
    public void SwitchTState_Uninitialized_Throws()
    {
        Result result = default;
        Assert.Throws<InvalidOperationException>(() => result.Execute(1, _ => { }, (_, _) => { }));
    }
}




