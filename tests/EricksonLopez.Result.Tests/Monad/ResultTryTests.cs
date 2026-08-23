// Copyright © Erickson Lopez. MIT License.
using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using AwesomeAssertions;
using EricksonLopez.Result;
using EricksonLopez.Result.Testing;
using Xunit;

#nullable enable
namespace EricksonLopez.Result.Tests.Monad;

public class ResultTryTests
{
#pragma warning disable CA2201, CS0162
    public static TheoryData<Exception> FatalExceptions => new()
    {
        new OutOfMemoryException(),
        new StackOverflowException(),
        new AccessViolationException()
    };

    [Theory]
    [MemberData(nameof(FatalExceptions))]
    public void Try_FatalException_Throws(Exception fatalEx)
    {
        Assert.Throws(fatalEx.GetType(), () => Result.Try(() => throw fatalEx, ex => Error.Failure("err", "desc")));
    }

    [Theory]
    [MemberData(nameof(FatalExceptions))]
    public void TryWithState_FatalException_Throws(Exception fatalEx)
    {
        Assert.Throws(fatalEx.GetType(), () => Result.Try("state", () => throw fatalEx, (s, ex) => Error.Failure("err", "desc")));
    }

    [Theory]
    [MemberData(nameof(FatalExceptions))]
    public async Task TryAsync_FatalException_Throws(Exception fatalEx)
    {
        await Assert.ThrowsAsync(fatalEx.GetType(), async () => await Result.TryAsync((Func<Task>)(async () => { throw fatalEx; await Task.Yield(); }), ex => Error.Failure("err", "desc")));
    }

    [Theory]
    [MemberData(nameof(FatalExceptions))]
    public async Task TryAsyncCancellation_FatalException_Throws(Exception fatalEx)
    {
        await Assert.ThrowsAsync(fatalEx.GetType(), async () => await Result.TryAsync((Func<CancellationToken, Task>)(async (c) => { throw fatalEx; await Task.Yield(); }), ex => Error.Failure("err", "desc"), default));
    }

    [Theory]
    [MemberData(nameof(FatalExceptions))]
    public void TryT_FatalException_Throws(Exception fatalEx)
    {
        Assert.Throws(fatalEx.GetType(), () => Result.Try<int>(() => throw fatalEx, ex => Error.Failure("err", "desc")));
    }

    [Theory]
    [MemberData(nameof(FatalExceptions))]
    public void TryTWithState_FatalException_Throws(Exception fatalEx)
    {
        Assert.Throws(fatalEx.GetType(), () => Result.Try<string, int>("state", () => throw fatalEx, (s, ex) => Error.Failure("err", "desc")));
    }

    [Theory]
    [MemberData(nameof(FatalExceptions))]
    public async Task TryAsyncT_FatalException_Throws(Exception fatalEx)
    {
        await Assert.ThrowsAsync(fatalEx.GetType(), async () => await Result.TryAsync<int>((Func<Task<int>>)(async () => { throw fatalEx; await Task.Yield(); return 1; }), ex => Error.Failure("err", "desc")));
    }

    [Theory]
    [MemberData(nameof(FatalExceptions))]
    public async Task TryAsyncTCancellation_FatalException_Throws(Exception fatalEx)
    {
        await Assert.ThrowsAsync(fatalEx.GetType(), async () => await Result.TryAsync<int>((Func<CancellationToken, Task<int>>)(async (c) => { throw fatalEx; await Task.Yield(); return 1; }), ex => Error.Failure("err", "desc"), default));
    }

    [Theory]
    [MemberData(nameof(FatalExceptions))]
    public async Task TryAsyncValue_FatalException_Throws(Exception fatalEx)
    {
        await Assert.ThrowsAsync(fatalEx.GetType(), async () => await Result.TryAsyncValue((Func<ValueTask>)(async () => { throw fatalEx; await Task.Yield(); }), ex => Error.Failure("err", "desc")));
    }

    [Theory]
    [MemberData(nameof(FatalExceptions))]
    public async Task TryAsyncValueCancellation_FatalException_Throws(Exception fatalEx)
    {
        await Assert.ThrowsAsync(fatalEx.GetType(), async () => await Result.TryAsyncValue((Func<CancellationToken, ValueTask>)(async (c) => { throw fatalEx; await Task.Yield(); }), ex => Error.Failure("err", "desc"), default));
    }

    [Theory]
    [MemberData(nameof(FatalExceptions))]
    public async Task TryAsyncValueT_FatalException_Throws(Exception fatalEx)
    {
        await Assert.ThrowsAsync(fatalEx.GetType(), async () => await Result.TryAsyncValue<int>((Func<ValueTask<int>>)(async () => { throw fatalEx; await Task.Yield(); return 1; }), ex => Error.Failure("err", "desc")));
    }

    [Theory]
    [MemberData(nameof(FatalExceptions))]
    public async Task TryAsyncValueTCancellation_FatalException_Throws(Exception fatalEx)
    {
        await Assert.ThrowsAsync(fatalEx.GetType(), async () => await Result.TryAsyncValue<int>((Func<CancellationToken, ValueTask<int>>)(async (c) => { throw fatalEx; await Task.Yield(); return 1; }), ex => Error.Failure("err", "desc"), default));
    }

    [Theory]
    [MemberData(nameof(FatalExceptions))]
    public async Task TryAsyncValueTWithState_FatalException_Throws(Exception fatalEx)
    {
        await Assert.ThrowsAsync(fatalEx.GetType(), async () => await Result.TryAsyncValue<string, int>("state", (Func<ValueTask<int>>)(async () => { throw fatalEx; await Task.Yield(); return 1; }), (s, ex) => Error.Failure("err", "desc")));
    }

    [Theory]
    [MemberData(nameof(FatalExceptions))]
    public async Task TryAsyncValueTWithStateCancellation_FatalException_Throws(Exception fatalEx)
    {
        await Assert.ThrowsAsync(fatalEx.GetType(), async () => await Result.TryAsyncValue<string, int>("state", (Func<CancellationToken, ValueTask<int>>)(async (c) => { throw fatalEx; await Task.Yield(); return 1; }), (s, ex) => Error.Failure("err", "desc"), default));
    }
#pragma warning restore CA2201, CS0162

    [Fact]
    public void TryGetError_WithState_Failure_ReturnsError()
    {
        var result = Result.Failure(Error.Failure("err", "msg"));
        var b = result.TryGetError(out var error, out var isUninitialized);
        Assert.True(b);
        Assert.False(isUninitialized);
        Assert.NotNull(error);
        Assert.Equal("err", error!.Code);
    }

    [Fact]
    public void Try_OperationCanceledException_WhenMapped_ReturnsFailureResult()
    {
        var result = Result.Try(
            () => throw new OperationCanceledException("Operation was canceled."),
            ex => Error.Unavailable("CANCELED", ex.Message));

        result.ShouldBeFailure();
        Assert.Equal("CANCELED", result.Error.Code);
    }

    [Fact]
    public void Try_OperationCanceledException_WhenRethrownInHandler_PropagatesException()
    {
        Assert.Throws<OperationCanceledException>(() =>
            Result.Try(
                () => throw new OperationCanceledException("Canceled"),
                ex => ex is OperationCanceledException ? throw ex : Error.Failure("ERR", ex.Message)));
    }

    [Fact]
    public async Task TryAsync_OperationCanceledException_WhenMapped_ReturnsFailureResult()
    {
        var result = await Result.TryAsync(
            async () =>
            {
                await Task.Yield();
                throw new OperationCanceledException("Async canceled.");
            },
            ex => Error.Unavailable("CANCELED", ex.Message));

        result.ShouldBeFailure();
        Assert.Equal("CANCELED", result.Error.Code);
    }

    [Fact]
    public async Task TryAsync_OperationCanceledException_WhenRethrownInHandler_PropagatesException()
    {
        await Assert.ThrowsAsync<OperationCanceledException>(async () =>
            await Result.TryAsync(
                async () =>
                {
                    await Task.Yield();
                    throw new OperationCanceledException("Async canceled");
                },
                ex => ex is OperationCanceledException ? throw ex : Error.Failure("ERR", ex.Message)));
    }

    [Fact]
    public async Task TryAsyncT_OperationCanceledException_WhenMapped_ReturnsFailureResult()
    {
        var result = await Result.TryAsync<int>(
            async () =>
            {
                await Task.Yield();
                throw new OperationCanceledException("Async T canceled.");
            },
            ex => Error.Unavailable("CANCELED", ex.Message));

        result.ShouldBeFailure();
        Assert.Equal("CANCELED", result.Error.Code);
    }

    [Fact]
    public async Task TryAsyncT_OperationCanceledException_WhenRethrownInHandler_PropagatesException()
    {
        await Assert.ThrowsAsync<OperationCanceledException>(async () =>
            await Result.TryAsync<int>(
                async () =>
                {
                    await Task.Yield();
                    throw new OperationCanceledException("Async T canceled");
                },
                ex => ex is OperationCanceledException ? throw ex : Error.Failure("ERR", ex.Message)));
    }

    [Fact]
    public void Try_State_Success_ReturnsSuccess()
    {
        var r = Result.Try("state", () => { }, (s, ex) => Error.Failure(s, s));
        r.ShouldBeSuccess();
    }

    [Fact]
    public void Try_State_Exception_ReturnsFailure()
    {
        var r = Result.Try("state", () => throw new InvalidOperationException("x"), (s, ex) => Error.Failure(s, ex.Message));
        var err = r.ShouldBeFailure();
        err.Code.Should().Be("state");
        err.Description.Should().Be("x");
    }

    [Fact]
    public void TryT_State_Success_ReturnsSuccess()
    {
        var r = Result.Try("state", () => 42, (s, ex) => Error.Failure(s, s));
        var val = r.ShouldBeSuccess();
        val.Should().Be(42);
    }

    [Fact]
    public void TryT_State_Exception_ReturnsFailure()
    {
        var r = Result.Try("state", new Func<int>(() => throw new InvalidOperationException("x")), (s, ex) => Error.Failure(s, ex.Message));
        var err = r.ShouldBeFailure();
        err.Code.Should().Be("state");
        err.Description.Should().Be("x");
    }

    [Fact]
    public async Task TryAsync_Task_Success()
    {
        var r = await Result.TryAsync(() => Task.CompletedTask, ex => Error.Failure("x", "x"));
        r.ShouldBeSuccess();
    }

    [Fact]
    public async Task TryAsync_Task_Exception()
    {
        var r = await Result.TryAsync(() => Task.FromException(new InvalidOperationException("x")), ex => Error.Failure("err", ex.Message));
        var err = r.ShouldBeFailure();
        err.Code.Should().Be("err");
        err.Description.Should().Be("x");
    }

    [Fact]
    public async Task TryAsync_TaskCancel_Exception()
    {
        var r = await Result.TryAsync(ct => Task.FromException(new InvalidOperationException("x")), ex => Error.Failure("err", ex.Message), CancellationToken.None);
        var err = r.ShouldBeFailure();
        err.Code.Should().Be("err");
        err.Description.Should().Be("x");
    }

    [Fact]
    public async Task TryAsyncValue_Success()
    {
        var r = await Result.TryAsyncValue(() => ValueTask.CompletedTask, ex => Error.Failure("err", "x"));
        r.ShouldBeSuccess();
    }

    [Fact]
    public async Task TryAsyncValue_Exception()
    {
        var r = await Result.TryAsyncValue(() => ValueTask.FromException(new InvalidOperationException("x")), ex => Error.Failure("err", ex.Message));
        var err = r.ShouldBeFailure();
        err.Code.Should().Be("err");
        err.Description.Should().Be("x");
    }

    [Fact]
    public async Task TryAsyncValue_Cancel_Success()
    {
        var r = await Result.TryAsyncValue(ct => ValueTask.CompletedTask, ex => Error.Failure("err", "x"), CancellationToken.None);
        r.ShouldBeSuccess();
    }

    [Fact]
    public async Task TryAsyncValue_Cancel_Exception()
    {
        var r = await Result.TryAsyncValue(ct => ValueTask.FromException(new InvalidOperationException("x")), ex => Error.Failure("err", ex.Message), CancellationToken.None);
        var err = r.ShouldBeFailure();
        err.Code.Should().Be("err");
        err.Description.Should().Be("x");
    }

    [Fact]
    public async Task TryAsyncValueT_Success()
    {
        var r = await Result.TryAsyncValue(() => ValueTask.FromResult(42), ex => Error.Failure("err", "x"));
        var val = r.ShouldBeSuccess();
        val.Should().Be(42);
    }

    [Fact]
    public async Task TryAsyncValueT_Exception()
    {
        var r = await Result.TryAsyncValue<int>(() => ValueTask.FromException<int>(new InvalidOperationException("x")), ex => Error.Failure("err", ex.Message));
        var err = r.ShouldBeFailure();
        err.Code.Should().Be("err");
        err.Description.Should().Be("x");
    }

    [Fact]
    public async Task TryAsyncValueT_Cancel_Success()
    {
        var r = await Result.TryAsyncValue(ct => ValueTask.FromResult(42), ex => Error.Failure("err", "x"), CancellationToken.None);
        var val = r.ShouldBeSuccess();
        val.Should().Be(42);
    }

    [Fact]
    public async Task TryAsyncValueT_Cancel_Exception()
    {
        var r = await Result.TryAsyncValue<int>(ct => ValueTask.FromException<int>(new InvalidOperationException("x")), ex => Error.Failure("err", ex.Message), CancellationToken.None);
        var err = r.ShouldBeFailure();
        err.Code.Should().Be("err");
        err.Description.Should().Be("x");
    }


}




