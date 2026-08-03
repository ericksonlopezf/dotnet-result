using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using EricksonLopez.Result;
using MediatR;

namespace EricksonLopez.Result.MediatR.Sample;

// Command returning a Result
public record ProcessDataCommand(int Id) : IRequest<Result<string>>;

// Handler returning a Result
public class ProcessDataCommandHandler : IRequestHandler<ProcessDataCommand, Result<string>>
{
    public Task<Result<string>> Handle(ProcessDataCommand request, CancellationToken cancellationToken)
    {
        if (request.Id < 0)
        {
            return Task.FromResult(Result.Failure<string>(
                Error.Create("Data.Invalid", "ID cannot be negative.").Build()
            ));
        }
        
        if (request.Id == 0)
        {
            // Throwing an exception. The MediatR behavior `ResultExceptionBehavior` will catch this
            // and turn it into a failed Result automatically!
            throw new InvalidOperationException("ID 0 causes a system fault.");
        }

        return Task.FromResult(Result.Success($"Processed {request.Id}"));
    }
}

public static class MediatRExample
{
    public static async Task RunAsync(IServiceProvider services)
    {
        Console.WriteLine("\n--- 11. MEDIATR ---");

        var mediator = services.GetRequiredService<IMediator>();

        // 1. Success case
        var successResult = await mediator.Send(new ProcessDataCommand(10));
        Console.WriteLine($"MediatR Success: {successResult.IsSuccess}, Value: {successResult.GetValueOrDefault("")}");

        // 2. Failure case (explicit failure in handler)
        var failureResult = await mediator.Send(new ProcessDataCommand(-5));
        Console.WriteLine($"MediatR Explicit Failure: {failureResult.IsFailure}, Error: {failureResult.Error?.Code}");

        // 3. Exception behavior case
        // This won't throw, but return a failed Result because of `AddResultExceptionBehavior` in Program.cs
        var exceptionResult = await mediator.Send(new ProcessDataCommand(0));
        Console.WriteLine($"MediatR Exception Caught as Result Failure: {exceptionResult.IsFailure}, Error: {exceptionResult.Error?.Code} - {exceptionResult.Error?.Description}");
    }
}

