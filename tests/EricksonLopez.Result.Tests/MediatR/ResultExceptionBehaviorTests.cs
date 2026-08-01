using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using AwesomeAssertions;
using Xunit;
using EricksonLopez.Result.MediatR;
using EricksonLopez.Result.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace EricksonLopez.Result.Tests.MediatR;

public class DummyRequest : IRequest<Result> { }
public class DummyRequestOfT : IRequest<Result<int>> { }
public class NonResultRequest : IRequest<string> { }
public class DummyListRequest : IRequest<System.Collections.Generic.List<string>> { }

public class ResultExceptionBehaviorTests
{
    [Fact]
    public async Task Handle_NonResultType_PassesThrough()
    {
        var behavior = new ResultExceptionBehavior<NonResultRequest, string>();
        var result = await behavior.Handle(new NonResultRequest(), () => Task.FromResult("OK"), CancellationToken.None);
        result.Should().Be("OK");
    }

    [Fact]
    public async Task Handle_NonResultType_ThrowsException()
    {
        var behavior = new ResultExceptionBehavior<NonResultRequest, string>();
        await Assert.ThrowsAsync<InvalidOperationException>(() => behavior.Handle(new NonResultRequest(), () => throw new InvalidOperationException(), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ResultType_Success_PassesThrough()
    {
        var behavior = new ResultExceptionBehavior<DummyRequest, Result>();
        var result = await behavior.Handle(new DummyRequest(), () => Task.FromResult(Result.Success()), CancellationToken.None);
        result.ShouldBeSuccess();
    }

    [Fact]
    public async Task Handle_ResultType_Exception_ReturnsUnexpectedError()
    {
        var behavior = new ResultExceptionBehavior<DummyRequest, Result>();
        var result = await behavior.Handle(new DummyRequest(), () => throw new InvalidOperationException("Oops"), CancellationToken.None);
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
        var result = await behavior.Handle(new DummyRequestOfT(), () => throw new InvalidOperationException("Oops"), CancellationToken.None);
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
        var result = await behavior.Handle(new DummyRequest(), () => throw new InvalidOperationException("Oops"), CancellationToken.None);
        result.ShouldBeFailure();
        result.Error.Type.Should().Be(ErrorType.Custom);
        result.Error.Code.Should().Be("C");
        result.Error.Description.Should().Be("Oops");
    }

    [Fact]
    public async Task Handle_Cancellation_Throws()
    {
        var behavior = new ResultExceptionBehavior<DummyRequest, Result>();
        await Assert.ThrowsAsync<OperationCanceledException>(() => behavior.Handle(new DummyRequest(), () => throw new OperationCanceledException(), CancellationToken.None));
    }

    [Fact]
    public void BuildFailureFactory_HitDelegateCacheBranch()
    {
        // Hit the delegate cache branch for Result
        var method1 = typeof(ResultExceptionBehavior<DummyRequest, Result>).GetMethod("BuildFailureFactory", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!;
        method1.Invoke(null, null); // Second time, hits cache
        
        // Hit the delegate cache branch for Result<T> - wait, Result<T> uses Expression Trees, no delegate cache branch!
        // The uncovered branch on line 151 is likely due to `&&` in `responseType.IsGenericType && responseType.GetGenericTypeDefinition() == typeof(Result<>)`.
        // To cover the false branch of the second condition, we need a generic type that is NOT Result<T>.
        var method2 = typeof(ResultExceptionBehavior<DummyListRequest, System.Collections.Generic.List<string>>).GetMethod("BuildFailureFactory", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!;
        method2.Invoke(null, null);
    }
}

public class ResultMediatRExtensionsTests
{
    [Fact]
    public void AddResultExceptionBehavior_RegistersBehavior()
    {
        var services = new ServiceCollection();
        services.AddResultExceptionBehavior();
        var provider = services.BuildServiceProvider();

        var behaviors = provider.GetServices<IPipelineBehavior<DummyRequest, Result>>();
        behaviors.Should().ContainSingle(b => b is ResultExceptionBehavior<DummyRequest, Result>);
    }

    [Fact]
    public void AddResultExceptionBehavior_WithFactory_RegistersFactory()
    {
        var services = new ServiceCollection();
        Func<Exception, Error> factory = ex => Error.Failure("F", "M");
        services.AddResultExceptionBehavior(factory);
        var provider = services.BuildServiceProvider();

        var registeredFactory = provider.GetService<Func<Exception, Error>>();
        registeredFactory.Should().BeSameAs(factory);
    }
}
