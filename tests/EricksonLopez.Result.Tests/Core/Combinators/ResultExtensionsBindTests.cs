// Copyright © Erickson Lopez. MIT License.
using System;
using System.Threading;
using System.Threading.Tasks;
using AwesomeAssertions;
using EricksonLopez.Result;
using EricksonLopez.Result.Testing;
using Xunit;

namespace EricksonLopez.Result.Tests.Core;

public class ResultExtensionsBindTests
{
    [Fact]
    public async Task Bind_Sync_Success_CompletedTask_ReturnsMappedValue()
    {
        var task = Task.FromResult(Result.Success(1));
        var result = await task.Bind(v => Result.Success(v * 2));
        result.ShouldBeSuccess().Should().Be(2);
    }

    [Fact]
    public async Task Bind_Sync_Success_IncompleteTask_ReturnsMappedValue()
    {
        var task = Task.Run(async () => { await Task.Yield(); return Result.Success(1); });
        var result = await task.Bind(v => Result.Success(v * 2));
        result.ShouldBeSuccess().Should().Be(2);
    }

    [Fact]
    public async Task Bind_WithState_Sync_Success_CompletedTask_ReturnsMappedValue()
    {
        var task = Task.FromResult(Result.Success(1));
        var result = await task.Bind(10, (state, v) => Result.Success(v + state));
        result.ShouldBeSuccess().Should().Be(11);
    }

    [Fact]
    public async Task Bind_WithState_Sync_Success_IncompleteTask_ReturnsMappedValue()
    {
        var task = Task.Run(async () => { await Task.Yield(); return Result.Success(1); });
        var result = await task.Bind(10, (state, v) => Result.Success(v + state));
        result.ShouldBeSuccess().Should().Be(11);
    }

    [Fact]
    public async Task Bind_Async_Success_CompletedTask_ReturnsMappedValue()
    {
        var task = Task.FromResult(Result.Success(1));
        var result = await task.Bind(async v => { await Task.Yield(); return Result.Success(v * 2); });
        result.ShouldBeSuccess().Should().Be(2);
    }

    [Fact]
    public async Task Bind_Async_Failure_CompletedTask_ReturnsFailure()
    {
        var task = Task.FromResult(Result.Failure<int>(Error.Failure("e", "m")));
        var result = await task.Bind(async v => { await Task.Yield(); return Result.Success(v * 2); });
        result.ShouldBeFailure().Code.Should().Be("e");
    }

    [Fact]
    public async Task Bind_Async_Success_IncompleteTask_ReturnsMappedValue()
    {
        var task = Task.Run(async () => { await Task.Yield(); return Result.Success(1); });
        var result = await task.Bind(async v => { await Task.Yield(); return Result.Success(v * 2); });
        result.ShouldBeSuccess().Should().Be(2);
    }

    [Fact]
    public async Task Bind_Async_Failure_IncompleteTask_ReturnsFailure()
    {
        var task = Task.Run(async () => { await Task.Yield(); return Result.Failure<int>(Error.Failure("e", "m")); });
        var result = await task.Bind(async v => { await Task.Yield(); return Result.Success(v * 2); });
        result.ShouldBeFailure().Code.Should().Be("e");
    }

    [Fact]
    public async Task Bind_NonGenericResult_Async_Success_CompletedTask_ReturnsSuccess()
    {
        var task = Task.FromResult(Result.Success(1));
        var result = await task.Bind(async v => { await Task.Yield(); return Result.Success(); });
        result.ShouldBeSuccess();
    }

    [Fact]
    public async Task Bind_NonGenericResult_Async_Failure_CompletedTask_ReturnsFailure()
    {
        var task = Task.FromResult(Result.Failure<int>(Error.Failure("e", "m")));
        var result = await task.Bind(async v => { await Task.Yield(); return Result.Success(); });
        result.ShouldBeFailure().Code.Should().Be("e");
    }

    [Fact]
    public async Task Bind_NonGenericResult_Async_Success_IncompleteTask_ReturnsSuccess()
    {
        var task = Task.Run(async () => { await Task.Yield(); return Result.Success(1); });
        var result = await task.Bind(async v => { await Task.Yield(); return Result.Success(); });
        result.ShouldBeSuccess();
    }

    [Fact]
    public async Task Bind_NonGenericResult_Sync_Success_CompletedTask_ReturnsSuccess()
    {
        var task = Task.FromResult(Result.Success(1));
        var result = await task.Bind(v => Result.Success());
        result.ShouldBeSuccess();
    }

    [Fact]
    public async Task Bind_NonGenericResult_Sync_Success_IncompleteTask_ReturnsSuccess()
    {
        var task = Task.Run(async () => { await Task.Yield(); return Result.Success(1); });
        var result = await task.Bind(v => Result.Success());
        result.ShouldBeSuccess();
    }

    [Fact]
    public async Task Bind_NonGenericResult_WithState_Sync_Success_CompletedTask_ReturnsSuccess()
    {
        var task = Task.FromResult(Result.Success(1));
        var result = await task.Bind(10, (state, v) => Result.Success());
        result.ShouldBeSuccess();
    }

    [Fact]
    public async Task Bind_NonGenericResult_WithState_Sync_Success_IncompleteTask_ReturnsSuccess()
    {
        var task = Task.Run(async () => { await Task.Yield(); return Result.Success(1); });
        var result = await task.Bind(10, (state, v) => Result.Success());
        result.ShouldBeSuccess();
    }

    [Fact]
    public async Task Bind_FromNonGeneric_Sync_Success_CompletedTask_ReturnsSuccess()
    {
        var task = Task.FromResult(Result.Success());
        var result = await task.Bind(() => Result.Success());
        result.ShouldBeSuccess();
    }

    [Fact]
    public async Task Bind_FromNonGeneric_Sync_Success_IncompleteTask_ReturnsSuccess()
    {
        var task = Task.Run(async () => { await Task.Yield(); return Result.Success(); });
        var result = await task.Bind(() => Result.Success());
        result.ShouldBeSuccess();
    }

    [Fact]
    public async Task Bind_FromNonGeneric_WithState_Sync_Success_CompletedTask_ReturnsSuccess()
    {
        var task = Task.FromResult(Result.Success());
        var result = await task.Bind(10, (state) => Result.Success());
        result.ShouldBeSuccess();
    }

    [Fact]
    public async Task Bind_FromNonGeneric_WithState_Sync_Success_IncompleteTask_ReturnsSuccess()
    {
        var task = Task.Run(async () => { await Task.Yield(); return Result.Success(); });
        var result = await task.Bind(10, (state) => Result.Success());
        result.ShouldBeSuccess();
    }

    [Fact]
    public async Task Bind_FromNonGeneric_Async_Success_CompletedTask_ReturnsSuccess()
    {
        var task = Task.FromResult(Result.Success());
        var result = await task.Bind(async () => { await Task.Yield(); return Result.Success(); });
        result.ShouldBeSuccess();
    }

    [Fact]
    public async Task Bind_FromNonGeneric_Async_Failure_CompletedTask_ReturnsFailure()
    {
        var task = Task.FromResult(Result.Failure(Error.Failure("e", "m")));
        var result = await task.Bind(async () => { await Task.Yield(); return Result.Success(); });
        result.ShouldBeFailure().Code.Should().Be("e");
    }

    [Fact]
    public async Task Bind_FromNonGeneric_Async_Success_IncompleteTask_ReturnsSuccess()
    {
        var task = Task.Run(async () => { await Task.Yield(); return Result.Success(); });
        var result = await task.Bind(async () => { await Task.Yield(); return Result.Success(); });
        result.ShouldBeSuccess();
    }

    [Fact]
    public async Task Bind_FromNonGeneric_ToGeneric_Sync_Success_CompletedTask_ReturnsMappedValue()
    {
        var task = Task.FromResult(Result.Success());
        var result = await task.Bind(() => Result.Success(2));
        result.ShouldBeSuccess().Should().Be(2);
    }

    [Fact]
    public async Task Bind_FromNonGeneric_ToGeneric_Sync_Success_IncompleteTask_ReturnsMappedValue()
    {
        var task = Task.Run(async () => { await Task.Yield(); return Result.Success(); });
        var result = await task.Bind(() => Result.Success(2));
        result.ShouldBeSuccess().Should().Be(2);
    }

    [Fact]
    public async Task Bind_FromNonGeneric_ToGeneric_WithState_Sync_Success_CompletedTask_ReturnsMappedValue()
    {
        var task = Task.FromResult(Result.Success());
        var result = await task.Bind(10, (state) => Result.Success(state));
        result.ShouldBeSuccess().Should().Be(10);
    }

    [Fact]
    public async Task Bind_FromNonGeneric_ToGeneric_WithState_Sync_Success_IncompleteTask_ReturnsMappedValue()
    {
        var task = Task.Run(async () => { await Task.Yield(); return Result.Success(); });
        var result = await task.Bind(10, (state) => Result.Success(state));
        result.ShouldBeSuccess().Should().Be(10);
    }

    [Fact]
    public async Task Bind_FromNonGeneric_ToGeneric_Async_Success_CompletedTask_ReturnsMappedValue()
    {
        var task = Task.FromResult(Result.Success());
        var result = await task.Bind(async () => { await Task.Yield(); return Result.Success(2); });
        result.ShouldBeSuccess().Should().Be(2);
    }

    [Fact]
    public async Task Bind_FromNonGeneric_ToGeneric_Async_Failure_CompletedTask_ReturnsFailure()
    {
        var task = Task.FromResult(Result.Failure(Error.Failure("e", "m")));
        var result = await task.Bind(async () => { await Task.Yield(); return Result.Success(2); });
        result.ShouldBeFailure().Code.Should().Be("e");
    }

    [Fact]
    public async Task Bind_FromNonGeneric_ToGeneric_Async_Success_IncompleteTask_ReturnsMappedValue()
    {
        var task = Task.Run(async () => { await Task.Yield(); return Result.Success(); });
        var result = await task.Bind(async () => { await Task.Yield(); return Result.Success(2); });
        result.ShouldBeSuccess().Should().Be(2);
    }

    [Fact]
    public async Task Bind_Ct_Async_Success_CompletedTask_ReturnsSuccess()
    {
        var task = Task.FromResult(Result.Success());
        var result = await task.Bind(async ct => { await Task.Yield(); return Result.Success(); });
        result.ShouldBeSuccess();
    }

    [Fact]
    public async Task Bind_Ct_Async_Failure_CompletedTask_ReturnsFailure()
    {
        var task = Task.FromResult(Result.Failure(Error.Failure("e", "m")));
        var result = await task.Bind(async ct => { await Task.Yield(); return Result.Success(); });
        result.ShouldBeFailure().Code.Should().Be("e");
    }

    [Fact]
    public async Task Bind_Ct_Async_Success_IncompleteTask_ReturnsSuccess()
    {
        var task = Task.Run(async () => { await Task.Yield(); return Result.Success(); });
        var result = await task.Bind(async ct => { await Task.Yield(); return Result.Success(); });
        result.ShouldBeSuccess();
    }

    [Fact]
    public async Task Bind_Ct_Async_Failure_IncompleteTask_ReturnsFailure()
    {
        var task = Task.Run(async () => { await Task.Yield(); return Result.Failure(Error.Failure("e", "m")); });
        var result = await task.Bind(async ct => { await Task.Yield(); return Result.Success(); });
        result.ShouldBeFailure().Code.Should().Be("e");
    }

    [Fact]
    public async Task Bind_ToGeneric_Ct_Async_Success_CompletedTask_ReturnsSuccess()
    {
        var task = Task.FromResult(Result.Success());
        var result = await task.Bind(async ct => { await Task.Yield(); return Result.Success(1); });
        result.ShouldBeSuccess().Should().Be(1);
    }

    [Fact]
    public async Task Bind_ToGeneric_Ct_Async_Failure_CompletedTask_ReturnsFailure()
    {
        var task = Task.FromResult(Result.Failure(Error.Failure("e", "m")));
        var result = await task.Bind(async ct => { await Task.Yield(); return Result.Success(1); });
        result.ShouldBeFailure().Code.Should().Be("e");
    }

    [Fact]
    public async Task Bind_ToGeneric_Ct_Async_Success_IncompleteTask_ReturnsSuccess()
    {
        var task = Task.Run(async () => { await Task.Yield(); return Result.Success(); });
        var result = await task.Bind(async ct => { await Task.Yield(); return Result.Success(1); });
        result.ShouldBeSuccess().Should().Be(1);
    }

    [Fact]
    public async Task Bind_Sync_Cancellation_Propagates()
    {
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();
        var tcs = new TaskCompletionSource<Result<int>>();
        var task = tcs.Task;
        await Assert.ThrowsAsync<OperationCanceledException>(async () => await task.Bind(v => Result.Success(v), cts.Token));
    }
}




