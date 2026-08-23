// Copyright © Erickson Lopez. MIT License.
using System;
using System.Threading;
using System.Threading.Tasks;
using EricksonLopez.Result;
using EricksonLopez.Result.MediatR;
using EricksonLopez.Result.Testing;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace EricksonLopez.Result.MediatR.Tests;

public class MediatRTests
{
    private record PingRequest(bool ThrowException) : IRequest<Result>;
    private record PingQuery(bool ThrowException) : IRequest<Result<string>>;

    private class PingRequestHandler : IRequestHandler<PingRequest, Result>
    {
        public Task<Result> Handle(PingRequest request, CancellationToken cancellationToken)
        {
            if (request.ThrowException) throw new InvalidOperationException("Ping failed!");
            return Task.FromResult(Result.Success());
        }
    }

    private class PingQueryHandler : IRequestHandler<PingQuery, Result<string>>
    {
        public Task<Result<string>> Handle(PingQuery request, CancellationToken cancellationToken)
        {
            if (request.ThrowException) throw new InvalidOperationException("Query failed!");
            return Task.FromResult(Result.Success("Pong"));
        }
    }

    [Fact]
    public async Task ResultExceptionBehavior_InterceptsException_ReturnsFailure()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(MediatRTests).Assembly));
        services.AddResultExceptionBehavior();

        var provider = services.BuildServiceProvider();
        var mediator = provider.GetRequiredService<IMediator>();

        var successResult = await mediator.Send(new PingRequest(false));
        successResult.ShouldBeSuccess();

        var failureResult = await mediator.Send(new PingRequest(true));
        failureResult.ShouldBeFailure()
                     .ShouldHaveErrorType(ErrorType.Unexpected);
        Assert.Equal("An unexpected handler error occurred.", failureResult.Error.Description);
    }

    [Fact]
    public async Task ResultExceptionBehavior_TypedResult_InterceptsException()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(MediatRTests).Assembly));
        services.AddResultExceptionBehavior();

        var provider = services.BuildServiceProvider();
        var mediator = provider.GetRequiredService<IMediator>();

        var success = await mediator.Send(new PingQuery(false));
        success.ShouldHaveValue("Pong");

        var failureResult = await mediator.Send(new PingQuery(true));
        failureResult.ShouldBeFailure()
                     .ShouldHaveErrorType(ErrorType.Unexpected);
    }
}




