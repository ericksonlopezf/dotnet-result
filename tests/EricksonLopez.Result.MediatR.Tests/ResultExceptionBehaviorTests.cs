// Copyright © Erickson Lopez. MIT License.
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AwesomeAssertions;
using EricksonLopez.Result;
using EricksonLopez.Result.MediatR;
using EricksonLopez.Result.Testing;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace EricksonLopez.Result.MediatR.Tests;

public class ResultExceptionBehaviorTests
{
    private sealed class DummyRequest : IRequest<Result> { }
    private sealed class DummyRequestOfT : IRequest<Result<int>> { }
    private sealed class NonResultRequest : IRequest<string> { }
    private sealed class DummyListRequest : IRequest<List<string>> { }

    [Fact]
    public async Task Handle_NonResultType_PassesThrough()
    {
        var behavior = new ResultExceptionBehavior<NonResultRequest, string>();
        var result = await behavior.Handle(new NonResultRequest(), _ => Task.FromResult("OK"), CancellationToken.None);
        result.Should().Be("OK");
    }

    [Fact]
    public async Task Handle_NonResultType_ThrowsException()
    {
        var behavior = new ResultExceptionBehavior<NonResultRequest, string>();
        await Assert.ThrowsAsync<InvalidOperationException>(() => behavior.Handle(new NonResultRequest(), _ => throw new InvalidOperationException(), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ResultType_Success_PassesThrough()
    {
        var behavior = new ResultExceptionBehavior<DummyRequest, Result>();
        var result = await behavior.Handle(new DummyRequest(), _ => Task.FromResult(Result.Success()), CancellationToken.None);
        result.ShouldBeSuccess();
    }

    [Fact]
    public async Task Handle_ResultType_Exception_ReturnsUnexpectedError()
    {
        var behavior = new ResultExceptionBehavior<DummyRequest, Result>();
        var result = await behavior.Handle(new DummyRequest(), _ => throw new InvalidOperationException("Oops"), CancellationToken.None);
        result.ShouldBeFailure();
        result.Error.Type.Should().Be(ErrorType.Unexpected);
        result.Error.Code.Should().Be("Handler.InvalidOperationException");
        // Description is intentionally generic — exception.Message is NOT surfaced by default
        // to prevent sensitive data leakage in ProblemDetails responses.
        result.Error.Description.Should().Be("An unexpected handler error occurred.");
    }

    [Fact]
    public async Task Handle_ResultOfTType_Exception_ReturnsUnexpectedError()
    {
        var behavior = new ResultExceptionBehavior<DummyRequestOfT, Result<int>>();
        var result = await behavior.Handle(new DummyRequestOfT(), _ => throw new InvalidOperationException("Oops"), CancellationToken.None);
        result.ShouldBeFailure();
        result.Error.Type.Should().Be(ErrorType.Unexpected);
        result.Error.Code.Should().Be("Handler.InvalidOperationException");
        // Description is intentionally generic — exception.Message is NOT surfaced by default
        // to prevent sensitive data leakage in ProblemDetails responses.
        result.Error.Description.Should().Be("An unexpected handler error occurred.");
    }

    [Fact]
    public async Task Handle_ResultType_Exception_UsesCustomErrorFactory()
    {
        var behavior = new ResultExceptionBehavior<DummyRequest, Result>(ex => Error.Custom("C", ex.Message, ErrorType.Custom));
        var result = await behavior.Handle(new DummyRequest(), _ => throw new InvalidOperationException("Oops"), CancellationToken.None);
        result.ShouldBeFailure();
        result.Error.Type.Should().Be(ErrorType.Custom);
        result.Error.Code.Should().Be("C");
        result.Error.Description.Should().Be("Oops");
    }

    [Fact]
    public async Task Handle_ResultType_Exception_CustomErrorFactoryReturnsNull_UsesFallback()
    {
        var behavior = new ResultExceptionBehavior<DummyRequest, Result>(ex => null!);
        var result = await behavior.Handle(new DummyRequest(), _ => throw new ArgumentException("Bad arg"), CancellationToken.None);
        result.ShouldBeFailure();
        result.Error.Type.Should().Be(ErrorType.Unexpected);
        result.Error.Code.Should().Be("Handler.ArgumentException");
        result.Error.Description.Should().Be("An unexpected handler error occurred.");
    }

    [Fact]
    public async Task Handle_ResultOfTType_Exception_UsesCustomErrorFactory()
    {
        var behavior = new ResultExceptionBehavior<DummyRequestOfT, Result<int>>(ex => Error.Conflict("CONFLICT_CODE", ex.Message));
        var result = await behavior.Handle(new DummyRequestOfT(), _ => throw new InvalidOperationException("Item exists"), CancellationToken.None);
        result.ShouldBeFailure();
        result.Error.Type.Should().Be(ErrorType.Conflict);
        result.Error.Code.Should().Be("CONFLICT_CODE");
        result.Error.Description.Should().Be("Item exists");
    }

    [Fact]
    public async Task Handle_NonGenericResult_Exception_ReturnsFailure()
    {
        var behavior = new ResultExceptionBehavior<DummyRequest, Result>();
        var result = await behavior.Handle(new DummyRequest(), _ => throw new InvalidOperationException("Test non-generic exception"), CancellationToken.None);
        result.IsFailure.Should().BeTrue();
        result.Error.Description.Should().Be("An unexpected handler error occurred.");
    }

    [Fact]
    public async Task Handle_NonGenericResult_WithCustomFactory_ReturnsCustomError()
    {
        Func<Exception, Error> customFactory = ex => Error.NotFound("CUSTOM_NOT_FOUND", ex.Message);
        var behavior = new ResultExceptionBehavior<DummyRequest, Result>(customFactory);
        var result = await behavior.Handle(new DummyRequest(), _ => throw new InvalidOperationException("Test custom non-generic factory"), CancellationToken.None);
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("CUSTOM_NOT_FOUND");
    }

    [Fact]
    public void CallAllResultAssertionsMethods()
    {
        // Calling BuildFailureFactory directly via reflection for non-Result and generic types
        var method = typeof(ResultExceptionBehavior<NonResultRequest, string>).GetMethod("BuildFailureFactory", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!;
        method.Invoke(null, null);

        var method2 = typeof(ResultExceptionBehavior<DummyListRequest, List<string>>).GetMethod("BuildFailureFactory", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!;
        method2.Invoke(null, null);
    }
}
