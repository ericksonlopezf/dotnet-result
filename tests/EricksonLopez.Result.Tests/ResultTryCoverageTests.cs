using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using AwesomeAssertions;

#pragma warning disable CA2012

namespace EricksonLopez.Result.Tests;

public class ResultTryCoverageTests
{
    [Fact]
    public void Try_State_Success_ReturnsSuccess()
    {
        var r = Result.Try("state", () => { }, (s, ex) => Error.Failure(s, s));
        r.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Try_State_Exception_ReturnsFailure()
    {
        var r = Result.Try("state", () => throw new InvalidOperationException("x"), (s, ex) => Error.Failure(s, ex.Message));
        r.IsFailure.Should().BeTrue();
        r.Error.Code.Should().Be("state");
        r.Error.Description.Should().Be("x");
    }

    [Fact]
    public void TryT_State_Success_ReturnsSuccess()
    {
        var r = Result.Try("state", () => 42, (s, ex) => Error.Failure(s, s));
        r.IsSuccess.Should().BeTrue();
        r.Value.Should().Be(42);
    }

    [Fact]
    public void TryT_State_Exception_ReturnsFailure()
    {
        var r = Result.Try("state", new Func<int>(() => throw new InvalidOperationException("x")), (s, ex) => Error.Failure(s, ex.Message));
        r.IsFailure.Should().BeTrue();
        r.Error.Code.Should().Be("state");
    }

    [Fact]
    public async Task TryAsync_Task_Success()
    {
        var r = await Result.TryAsync(() => Task.CompletedTask, ex => Error.Failure("x", "x"));
        r.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task TryAsync_Task_Exception()
    {
        var r = await Result.TryAsync(() => Task.FromException(new InvalidOperationException("x")), ex => Error.Failure("err", ex.Message));
        r.IsFailure.Should().BeTrue();
        r.Error.Description.Should().Be("x");
    }

    [Fact]
    public async Task TryAsync_TaskCancel_Exception()
    {
        var r = await Result.TryAsync(ct => Task.FromException(new InvalidOperationException("x")), ex => Error.Failure("err", ex.Message), CancellationToken.None);
        r.IsFailure.Should().BeTrue();
    }
    [Fact]
    public async Task TryAsyncValue_Success()
    {
        var r = await Result.TryAsyncValue(() => ValueTask.CompletedTask, ex => Error.Failure("err", "x"));
        r.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task TryAsyncValue_Exception()
    {
        var r = await Result.TryAsyncValue(() => ValueTask.FromException(new InvalidOperationException("x")), ex => Error.Failure("err", ex.Message));
        r.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task TryAsyncValue_Cancel_Success()
    {
        var r = await Result.TryAsyncValue(ct => ValueTask.CompletedTask, ex => Error.Failure("err", "x"), CancellationToken.None);
        r.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task TryAsyncValue_Cancel_Exception()
    {
        var r = await Result.TryAsyncValue(ct => ValueTask.FromException(new InvalidOperationException("x")), ex => Error.Failure("err", ex.Message), CancellationToken.None);
        r.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task TryAsyncValueT_Success()
    {
        var r = await Result.TryAsyncValue(() => ValueTask.FromResult(42), ex => Error.Failure("err", "x"));
        r.IsSuccess.Should().BeTrue();
        r.Value.Should().Be(42);
    }

    [Fact]
    public async Task TryAsyncValueT_Exception()
    {
        var r = await Result.TryAsyncValue<int>(() => ValueTask.FromException<int>(new InvalidOperationException("x")), ex => Error.Failure("err", ex.Message));
        r.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task TryAsyncValueT_Cancel_Success()
    {
        var r = await Result.TryAsyncValue(ct => ValueTask.FromResult(42), ex => Error.Failure("err", "x"), CancellationToken.None);
        r.IsSuccess.Should().BeTrue();
        r.Value.Should().Be(42);
    }

    [Fact]
    public async Task TryAsyncValueT_Cancel_Exception()
    {
        var r = await Result.TryAsyncValue<int>(ct => ValueTask.FromException<int>(new InvalidOperationException("x")), ex => Error.Failure("err", ex.Message), CancellationToken.None);
        r.IsFailure.Should().BeTrue();
    }
}
