// Copyright © Erickson Lopez. MIT License.
using System;
using System.Threading;
using System.Threading.Tasks;
using EricksonLopez.Result;
using EricksonLopez.Result.MediatR;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace EricksonLopez.Result.Sample.Examples;

public record PingQuery(string Message) : IRequest<Result<string>>;
public record ThrowingCommand(int Id) : IRequest<Result>;

public sealed class PingQueryHandler : IRequestHandler<PingQuery, Result<string>>
{
    public Task<Result<string>> Handle(PingQuery request, CancellationToken cancellationToken)
    {
        if (request.Message == "throw")
        {
            throw new InvalidOperationException("Simulated unexpected database failure.");
        }

        return Task.FromResult(Result.Success($"Pong: {request.Message}"));
    }
}

public sealed class ThrowingCommandHandler : IRequestHandler<ThrowingCommand, Result>
{
    public Task<Result> Handle(ThrowingCommand request, CancellationToken cancellationToken)
    {
        if (request.Id < 0)
        {
            throw new TimeoutException("Downstream network timeout.");
        }

        return Task.FromResult(Result.Success());
    }
}

public static class MediatRShowcase
{
    public static async Task RunAsync()
    {
        Console.WriteLine("\n========================================================");
        Console.WriteLine(" 25. MEDIATR PIPELINE BEHAVIOR SHOWCASE");
        Console.WriteLine("========================================================");

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(MediatRShowcase).Assembly);
        });

        // Register official ResultExceptionBehavior
        services.AddResultExceptionBehavior(ex =>
            Error.Create($"Handler.{ex.GetType().Name}", ex.Message)
                 .WithType(ErrorType.Unexpected)
                 .WithSeverity(ErrorSeverity.Critical)
                 .Build());

        using var provider = services.BuildServiceProvider();
        var mediator = provider.GetRequiredService<IMediator>();

        // 1. Successful Query
        Console.WriteLine("\n[1] Normal Query execution:");
        var normalQuery = new PingQuery("Hello MediatR");
        Result<string> normalResult = await mediator.Send(normalQuery);
        Console.WriteLine($"  Normal query: IsSuccess={normalResult.IsSuccess}, Value='{normalResult.Value}'");

        // 2. Query throwing exception — converted to Result<T>.Failure
        Console.WriteLine("\n[2] Query throwing Exception (intercepted by ResultExceptionBehavior):");
        var failingQuery = new PingQuery("throw");
        Result<string> caughtQueryResult = await mediator.Send(failingQuery);
        Console.WriteLine($"  Caught Exception: IsFailure={caughtQueryResult.IsFailure}");
        Console.WriteLine($"  Error Code: [{caughtQueryResult.Error.Code}]");
        Console.WriteLine($"  Error Type: {caughtQueryResult.Error.Type}");
        Console.WriteLine($"  Error Description: {caughtQueryResult.Error.Description}");

        // 3. Command throwing exception — converted to non-generic Result.Failure
        Console.WriteLine("\n[3] Command throwing Exception (converted to non-generic Result):");
        var failingCmd = new ThrowingCommand(-1);
        Result caughtCmdResult = await mediator.Send(failingCmd);
        Console.WriteLine($"  Command failure intercepted: IsFailure={caughtCmdResult.IsFailure}");
        Console.WriteLine($"  Error Code: [{caughtCmdResult.Error.Code}]");
    }
}
