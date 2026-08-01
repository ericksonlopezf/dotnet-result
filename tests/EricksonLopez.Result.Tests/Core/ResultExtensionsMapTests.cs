using System;
using System.Threading;
using System.Threading.Tasks;
using AwesomeAssertions;
using EricksonLopez.Result.Testing;
using Xunit;

namespace EricksonLopez.Result.Tests.Core;

public class ResultExtensionsMapTests
{
    [Fact]
    public async Task Map_Sync_Success_CompletedTask_ReturnsMappedValue()
    {
        var task = Task.FromResult(Result.Success(1));
        var result = await task.Map(v => v * 2);
        result.ShouldBeSuccess().Should().Be(2);
    }

    [Fact]
    public async Task Map_Sync_Success_IncompleteTask_ReturnsMappedValue()
    {
        var task = Task.Run(async () => { await Task.Yield(); return Result.Success(1); });
        var result = await task.Map(v => v * 2);
        result.ShouldBeSuccess().Should().Be(2);
    }

    [Fact]
    public async Task Map_Sync_Failure_CompletedTask_ReturnsFailure()
    {
        var task = Task.FromResult(Result.Failure<int>(Error.Failure("e", "m")));
        var result = await task.Map(v => v * 2);
        result.ShouldBeFailure().Code.Should().Be("e");
    }

    [Fact]
    public async Task Map_Sync_Failure_IncompleteTask_ReturnsFailure()
    {
        var task = Task.Run(async () => { await Task.Yield(); return Result.Failure<int>(Error.Failure("e", "m")); });
        var result = await task.Map(v => v * 2);
        result.ShouldBeFailure().Code.Should().Be("e");
    }

    [Fact]
    public async Task Map_WithState_Success_CompletedTask_ReturnsMappedValue()
    {
        var task = Task.FromResult(Result.Success(1));
        var result = await task.Map(10, (state, v) => v + state);
        result.ShouldBeSuccess().Should().Be(11);
    }

    [Fact]
    public async Task Map_WithState_Success_IncompleteTask_ReturnsMappedValue()
    {
        var task = Task.Run(async () => { await Task.Yield(); return Result.Success(1); });
        var result = await task.Map(10, (state, v) => v + state);
        result.ShouldBeSuccess().Should().Be(11);
    }

    [Fact]
    public async Task Map_WithState_Failure_IncompleteTask_ReturnsFailure()
    {
        var task = Task.Run(async () => { await Task.Yield(); return Result.Failure<int>(Error.Failure("e", "m")); });
        var result = await task.Map(10, (state, v) => v + state);
        result.ShouldBeFailure().Code.Should().Be("e");
    }

    [Fact]
    public async Task Map_Async_Success_CompletedTask_ReturnsMappedValue()
    {
        var task = Task.FromResult(Result.Success(1));
        var result = await task.Map(async v => { await Task.Yield(); return v * 2; });
        result.ShouldBeSuccess().Should().Be(2);
    }

    [Fact]
    public async Task Map_Async_Failure_CompletedTask_ReturnsFailure()
    {
        var task = Task.FromResult(Result.Failure<int>(Error.Failure("e", "m")));
        var result = await task.Map(async v => { await Task.Yield(); return v * 2; });
        result.ShouldBeFailure().Code.Should().Be("e");
    }

    [Fact]
    public async Task Map_Async_Success_IncompleteTask_ReturnsMappedValue()
    {
        var task = Task.Run(async () => { await Task.Yield(); return Result.Success(1); });
        var result = await task.Map(async v => { await Task.Yield(); return v * 2; });
        result.ShouldBeSuccess().Should().Be(2);
    }

    [Fact]
    public async Task Map_Async_Failure_IncompleteTask_ReturnsFailure()
    {
        var task = Task.Run(async () => { await Task.Yield(); return Result.Failure<int>(Error.Failure("e", "m")); });
        var result = await task.Map(async v => { await Task.Yield(); return v * 2; });
        result.ShouldBeFailure().Code.Should().Be("e");
    }

    [Fact]
    public async Task Map_WithState_Async_Success_CompletedTask_ReturnsMappedValue()
    {
        var task = Task.FromResult(Result.Success(1));
        var result = await task.Map(10, async (state, v) => { await Task.Yield(); return v + state; });
        result.ShouldBeSuccess().Should().Be(11);
    }

    [Fact]
    public async Task Map_WithState_Async_Failure_CompletedTask_ReturnsFailure()
    {
        var task = Task.FromResult(Result.Failure<int>(Error.Failure("e", "m")));
        var result = await task.Map(10, async (state, v) => { await Task.Yield(); return v + state; });
        result.ShouldBeFailure().Code.Should().Be("e");
    }

    [Fact]
    public async Task Map_WithState_Async_Success_IncompleteTask_ReturnsMappedValue()
    {
        var task = Task.Run(async () => { await Task.Yield(); return Result.Success(1); });
        var result = await task.Map(10, async (state, v) => { await Task.Yield(); return v + state; });
        result.ShouldBeSuccess().Should().Be(11);
    }

    [Fact]
    public async Task Map_WithState_Async_Failure_IncompleteTask_ReturnsFailure()
    {
        var task = Task.Run(async () => { await Task.Yield(); return Result.Failure<int>(Error.Failure("e", "m")); });
        var result = await task.Map(10, async (state, v) => { await Task.Yield(); return v + state; });
        result.ShouldBeFailure().Code.Should().Be("e");
    }
    
    [Fact]
    public async Task Map_Result_Success_CompletedTask_ReturnsMappedValue()
    {
        var task = Task.FromResult(Result.Success());
        var result = await task.Map(async ct => { await Task.Yield(); return 2; });
        result.ShouldBeSuccess().Should().Be(2);
    }

    [Fact]
    public async Task Map_Result_Failure_CompletedTask_ReturnsFailure()
    {
        var task = Task.FromResult(Result.Failure(Error.Failure("e", "m")));
        var result = await task.Map(async ct => { await Task.Yield(); return 2; });
        result.ShouldBeFailure().Code.Should().Be("e");
    }
    
    [Fact]
    public async Task Map_Result_Success_IncompleteTask_ReturnsMappedValue()
    {
        var task = Task.Run(async () => { await Task.Yield(); return Result.Success(); });
        var result = await task.Map(async ct => { await Task.Yield(); return 2; });
        result.ShouldBeSuccess().Should().Be(2);
    }

    [Fact]
    public async Task Map_Result_Failure_IncompleteTask_ReturnsFailure()
    {
        var task = Task.Run(async () => { await Task.Yield(); return Result.Failure(Error.Failure("e", "m")); });
        var result = await task.Map(async ct => { await Task.Yield(); return 2; });
        result.ShouldBeFailure().Code.Should().Be("e");
    }
    
    [Fact]
    public async Task Map_Sync_CancellationToken_Propagates()
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel();
        var tcs = new TaskCompletionSource<Result<int>>();
        var task = tcs.Task;
        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () => await task.Map(v => v, cts.Token));
    }
    
    [Fact]
    public async Task Map_WithState_CancellationToken_Propagates()
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel();
        var tcs = new TaskCompletionSource<Result<int>>();
        var task = tcs.Task;
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => task.Map(1, (s,v) => v, cts.Token));
    }
    
    [Fact]
    public async Task Map_Async_CancellationToken_Propagates()
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel();
        var task = Task.FromResult(Result.Success(1));
        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () => await task.Map(async v => v, cts.Token));
    }

    [Fact]
    public async Task Map_WithState_Async_CancellationToken_Propagates()
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel();
        var task = Task.FromResult(Result.Success(1));
        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () => await task.Map(1, async (s,v) => v, cts.Token));
    }
    
    [Fact]
    public async Task Map_Result_Async_CancellationToken_Propagates()
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel();
        var task = Task.Run(async () => { await Task.Yield(); return Result.Success(); });
        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () => await task.Map(async ct => 1, cts.Token));
    }
}
