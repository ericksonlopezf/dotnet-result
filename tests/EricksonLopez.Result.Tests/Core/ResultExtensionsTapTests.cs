using System;
using System.Threading;
using System.Threading.Tasks;
using AwesomeAssertions;
using EricksonLopez.Result.Testing;
using Xunit;

namespace EricksonLopez.Result.Tests.Core;

public class ResultExtensionsTapTests
{

    [Fact]
    public async Task Tap_Sync_Success_CompletedTask_InvokesAction()
    {
        var task = Task.FromResult(Result.Success(1));
        bool invoked = false;
        var result = await task.TapOnSuccess(v => invoked = true);
        result.ShouldBeSuccess();
        invoked.Should().BeTrue();
    }

    [Fact]
    public async Task Tap_Sync_Success_IncompleteTask_InvokesAction()
    {
        var task = Task.Run(async () => { await Task.Yield(); return Result.Success(1); });
        bool invoked = false;
        var result = await task.TapOnSuccess(v => invoked = true);
        result.ShouldBeSuccess();
        invoked.Should().BeTrue();
    }

    [Fact]
    public async Task Tap_WithState_Sync_Success_CompletedTask_InvokesAction()
    {
        var task = Task.FromResult(Result.Success(1));
        bool invoked = false;
        var result = await task.TapOnSuccess(10, (s, v) => invoked = true);
        result.ShouldBeSuccess();
        invoked.Should().BeTrue();
    }
    
    [Fact]
    public async Task Tap_WithState_Sync_Success_IncompleteTask_InvokesAction()
    {
        var task = Task.Run(async () => { await Task.Yield(); return Result.Success(1); });
        bool invoked = false;
        var result = await task.TapOnSuccess(10, (s, v) => invoked = true);
        result.ShouldBeSuccess();
        invoked.Should().BeTrue();
    }

    [Fact]
    public async Task Tap_Async_Success_CompletedTask_InvokesAction()
    {
        var task = Task.FromResult(Result.Success(1));
        bool invoked = false;
        var result = await task.TapOnSuccess(async v => { await Task.Yield(); invoked = true; });
        result.ShouldBeSuccess();
        invoked.Should().BeTrue();
    }

    [Fact]
    public async Task Tap_Async_Failure_CompletedTask_DoesNotInvokeAction()
    {
        var task = Task.FromResult(Result.Failure<int>(Error.Failure("e", "m")));
        bool invoked = false;
        var result = await task.TapOnSuccess(async v => { await Task.Yield(); invoked = true; });
        result.ShouldBeFailure();
        invoked.Should().BeFalse();
    }

    [Fact]
    public async Task Tap_Async_Success_IncompleteTask_InvokesAction()
    {
        var task = Task.Run(async () => { await Task.Yield(); return Result.Success(1); });
        bool invoked = false;
        var result = await task.TapOnSuccess(async v => { await Task.Yield(); invoked = true; });
        result.ShouldBeSuccess();
        invoked.Should().BeTrue();
    }

    [Fact]
    public async Task Tap_Async_Failure_IncompleteTask_DoesNotInvokeAction()
    {
        var task = Task.Run(async () => { await Task.Yield(); return Result.Failure<int>(Error.Failure("e", "m")); });
        bool invoked = false;
        var result = await task.TapOnSuccess(async v => { await Task.Yield(); invoked = true; });
        result.ShouldBeFailure();
        invoked.Should().BeFalse();
    }


    [Fact]
    public async Task TapOnFailure_Sync_Failure_CompletedTask_InvokesAction()
    {
        var task = Task.FromResult(Result.Failure<int>(Error.Failure("e", "m")));
        bool invoked = false;
        var result = await task.TapOnFailure(e => invoked = true);
        result.ShouldBeFailure();
        invoked.Should().BeTrue();
    }

    [Fact]
    public async Task TapOnFailure_Sync_Failure_IncompleteTask_InvokesAction()
    {
        var task = Task.Run(async () => { await Task.Yield(); return Result.Failure<int>(Error.Failure("e", "m")); });
        bool invoked = false;
        var result = await task.TapOnFailure(e => invoked = true);
        result.ShouldBeFailure();
        invoked.Should().BeTrue();
    }

    [Fact]
    public async Task TapOnFailure_WithState_Sync_Failure_CompletedTask_InvokesAction()
    {
        var task = Task.FromResult(Result.Failure<int>(Error.Failure("e", "m")));
        bool invoked = false;
        var result = await task.TapOnFailure(10, (s, e) => invoked = true);
        result.ShouldBeFailure();
        invoked.Should().BeTrue();
    }
    
    [Fact]
    public async Task TapOnFailure_WithState_Sync_Failure_IncompleteTask_InvokesAction()
    {
        var task = Task.Run(async () => { await Task.Yield(); return Result.Failure<int>(Error.Failure("e", "m")); });
        bool invoked = false;
        var result = await task.TapOnFailure(10, (s, e) => invoked = true);
        result.ShouldBeFailure();
        invoked.Should().BeTrue();
    }

    [Fact]
    public async Task TapOnFailure_Async_Failure_CompletedTask_InvokesAction()
    {
        var task = Task.FromResult(Result.Failure<int>(Error.Failure("e", "m")));
        bool invoked = false;
        var result = await task.TapOnFailure(async e => { await Task.Yield(); invoked = true; });
        result.ShouldBeFailure();
        invoked.Should().BeTrue();
    }
    
    [Fact]
    public async Task TapOnFailure_Async_Success_CompletedTask_DoesNotInvokeAction()
    {
        var task = Task.FromResult(Result.Success(1));
        bool invoked = false;
        var result = await task.TapOnFailure(async e => { await Task.Yield(); invoked = true; });
        result.ShouldBeSuccess();
        invoked.Should().BeFalse();
    }

    [Fact]
    public async Task TapOnFailure_Async_Failure_IncompleteTask_InvokesAction()
    {
        var task = Task.Run(async () => { await Task.Yield(); return Result.Failure<int>(Error.Failure("e", "m")); });
        bool invoked = false;
        var result = await task.TapOnFailure(async e => { await Task.Yield(); invoked = true; });
        result.ShouldBeFailure();
        invoked.Should().BeTrue();
    }

    [Fact]
    public async Task TapOnFailure_Async_Success_IncompleteTask_DoesNotInvokeAction()
    {
        var task = Task.Run(async () => { await Task.Yield(); return Result.Success(1); });
        bool invoked = false;
        var result = await task.TapOnFailure(async e => { await Task.Yield(); invoked = true; });
        result.ShouldBeSuccess();
        invoked.Should().BeFalse();
    }


    [Fact]
    public async Task Inspect_Sync_CompletedTask_InvokesAction()
    {
        var task = Task.FromResult(Result.Success(1));
        bool invoked = false;
        var result = await task.Inspect(r => invoked = true);
        result.ShouldBeSuccess();
        invoked.Should().BeTrue();
    }

    [Fact]
    public async Task Inspect_Sync_IncompleteTask_InvokesAction()
    {
        var task = Task.Run(async () => { await Task.Yield(); return Result.Success(1); });
        bool invoked = false;
        var result = await task.Inspect(r => invoked = true);
        result.ShouldBeSuccess();
        invoked.Should().BeTrue();
    }

    [Fact]
    public async Task Inspect_WithState_Sync_CompletedTask_InvokesAction()
    {
        var task = Task.FromResult(Result.Success(1));
        bool invoked = false;
        var result = await task.Inspect(10, (s, r) => invoked = true);
        result.ShouldBeSuccess();
        invoked.Should().BeTrue();
    }

    [Fact]
    public async Task Inspect_WithState_Sync_IncompleteTask_InvokesAction()
    {
        var task = Task.Run(async () => { await Task.Yield(); return Result.Success(1); });
        bool invoked = false;
        var result = await task.Inspect(10, (s, r) => invoked = true);
        result.ShouldBeSuccess();
        invoked.Should().BeTrue();
    }
    

    [Fact]
    public async Task TapNonGeneric_Sync_CompletedTask_InvokesAction()
    {
        var task = Task.FromResult(Result.Success());
        bool invoked = false;
        var result = await task.TapOnSuccess(() => invoked = true);
        result.ShouldBeSuccess();
        invoked.Should().BeTrue();
    }
    
    [Fact]
    public async Task TapNonGeneric_Sync_IncompleteTask_InvokesAction()
    {
        var task = Task.Run(async () => { await Task.Yield(); return Result.Success(); });
        bool invoked = false;
        var result = await task.TapOnSuccess(() => invoked = true);
        result.ShouldBeSuccess();
        invoked.Should().BeTrue();
    }

    [Fact]
    public async Task TapNonGeneric_WithState_Sync_CompletedTask_InvokesAction()
    {
        var task = Task.FromResult(Result.Success());
        bool invoked = false;
        var result = await task.TapOnSuccess(10, s => invoked = true);
        result.ShouldBeSuccess();
        invoked.Should().BeTrue();
    }

    [Fact]
    public async Task TapNonGeneric_WithState_Sync_IncompleteTask_InvokesAction()
    {
        var task = Task.Run(async () => { await Task.Yield(); return Result.Success(); });
        bool invoked = false;
        var result = await task.TapOnSuccess(10, s => invoked = true);
        result.ShouldBeSuccess();
        invoked.Should().BeTrue();
    }

    [Fact]
    public async Task TapNonGeneric_Async_CompletedTask_InvokesAction()
    {
        var task = Task.FromResult(Result.Success());
        bool invoked = false;
        var result = await task.TapOnSuccess(async () => { await Task.Yield(); invoked = true; });
        result.ShouldBeSuccess();
        invoked.Should().BeTrue();
    }

    [Fact]
    public async Task TapNonGeneric_Async_Failure_CompletedTask_DoesNotInvokeAction()
    {
        var task = Task.FromResult(Result.Failure(Error.Failure("e", "m")));
        bool invoked = false;
        var result = await task.TapOnSuccess(async () => { await Task.Yield(); invoked = true; });
        result.ShouldBeFailure();
        invoked.Should().BeFalse();
    }

    [Fact]
    public async Task TapNonGeneric_Async_IncompleteTask_InvokesAction()
    {
        var task = Task.Run(async () => { await Task.Yield(); return Result.Success(); });
        bool invoked = false;
        var result = await task.TapOnSuccess(async () => { await Task.Yield(); invoked = true; });
        result.ShouldBeSuccess();
        invoked.Should().BeTrue();
    }
}
