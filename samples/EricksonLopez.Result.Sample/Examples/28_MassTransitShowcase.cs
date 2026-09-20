// Copyright © Erickson Lopez. MIT License.
using System;
using System.Threading.Tasks;
using EricksonLopez.Result;
using EricksonLopez.Result.MassTransit;
using MassTransit;

namespace EricksonLopez.Result.Sample.Examples;

public record ProcessOrderMessage(Guid OrderId, decimal Amount);

public static class MassTransitShowcase
{
    public static void Run()
    {
        Console.WriteLine("\n========================================================");
        Console.WriteLine(" 28. MASSTRANSIT ECOSYSTEM INTEGRATION SHOWCASE");
        Console.WriteLine("========================================================");

        // 1. Map Error to ResultFault message contract
        Console.WriteLine("\n[1] ResultFault message contract generation from Error:");
        Error domainError = Error.Create("Payment.CardDeclined", "The card was declined by issuing bank.")
            .WithType(ErrorType.Failure)
            .WithSeverity(ErrorSeverity.Warning)
            .WithRetryability(ErrorRetryability.Permanent)
            .WithCorrelationId("corr-9988-abc")
            .WithTraceId("trace-5544-xyz")
            .WithMetadata("DeclineCode", "INSUFFICIENT_FUNDS")
            .WithMetadata("CardLast4", "4242")
            .Build();

        ResultFault faultMessage = ResultFault.FromError(domainError);

        Console.WriteLine($"  ResultFault Code:          {faultMessage.Code}");
        Console.WriteLine($"  ResultFault Description:   {faultMessage.Description}");
        Console.WriteLine($"  ResultFault Type:          {faultMessage.Type}");
        Console.WriteLine($"  ResultFault Severity:      {faultMessage.Severity}");
        Console.WriteLine($"  ResultFault Retryability:  {faultMessage.Retryability}");
        Console.WriteLine($"  ResultFault CorrelationId: {faultMessage.CorrelationId}");
        Console.WriteLine($"  ResultFault TraceId:       {faultMessage.TraceId}");
        Console.WriteLine($"  Metadata 'DeclineCode':    {faultMessage.Metadata["DeclineCode"]}");

        // 2. Demonstrating ResultConsumeFilter instantiation and pipe configuration
        Console.WriteLine("\n[2] ResultConsumeFilter & UseResultFilter extension:");
        var filter = new ResultConsumeFilter<ProcessOrderMessage>(
            onFault: (context, err) =>
            {
                Console.WriteLine($"  [Filter Fault Handler] Intercepted error [{err.Code}] on message for Order {context.Message.OrderId}");
                return Task.CompletedTask;
            });

        Console.WriteLine($"  Filter created successfully: {filter.GetType().Name}<{nameof(ProcessOrderMessage)}>");
    }
}
