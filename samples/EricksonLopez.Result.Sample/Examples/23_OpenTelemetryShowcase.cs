// Copyright © Erickson Lopez. MIT License.
using System;
using System.Diagnostics;
using EricksonLopez.Result;
using EricksonLopez.Result.OpenTelemetry;

namespace EricksonLopez.Result.Sample.Examples;

public static class OpenTelemetryShowcase
{
    public static void Run()
    {
        Console.WriteLine("\n========================================================");
        Console.WriteLine(" 23. OPENTELEMETRY TRACING & BCL METRICS");
        Console.WriteLine("========================================================");

        // Configure a local ActivityListener to demonstrate span annotation
        using var listener = new ActivityListener
        {
            ShouldListenTo = source => source.Name == "EricksonLopez.Result.Sample" || source.Name == ResultActivityExtensions.ActivitySourceName,
            Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllDataAndRecorded,
            ActivityStarted = act => Console.WriteLine($"  [Span Started] {act.OperationName}"),
            ActivityStopped = act =>
            {
                Console.WriteLine($"  [Span Stopped] {act.OperationName} (Status: {act.Status})");
                foreach (var tag in act.Tags)
                {
                    Console.WriteLine($"    Tag: {tag.Key} = {tag.Value}");
                }
            }
        };
        ActivitySource.AddActivityListener(listener);

        var sampleSource = new ActivitySource("EricksonLopez.Result.Sample");

        // 1. Trace a Successful Operation
        Console.WriteLine("\n[1] Tracing a Successful Result:");
        using (var activity = sampleSource.StartActivity("ProcessPayment"))
        {
            Result<string> paymentResult = Result.Success("TXN-884920");
            paymentResult.TraceOutcome("ProcessPayment", activity);
            Console.WriteLine($"  Payment confirmed: {paymentResult.Value}");
        }

        // 2. Trace a Failed Operation with Error Diagnostics
        Console.WriteLine("\n[2] Tracing a Failed Result with Error Metadata:");
        using (var activity = sampleSource.StartActivity("AuthorizeOrder"))
        {
            Error orderError = Error.Create("Order.CreditLimitExceeded", "The customer's credit limit was exceeded.")
                .WithType(ErrorType.Validation)
                .WithSeverity(ErrorSeverity.Warning)
                .WithRetryability(ErrorRetryability.Permanent)
                .Build();

            Result orderResult = Result.Failure(orderError);
            orderResult.TraceOutcome("AuthorizeOrder", activity);
            Console.WriteLine($"  Order rejected: {orderResult.Error.Code}");
        }

        // 3. Static Metric Emission
        Console.WriteLine("\n[3] BCL Metrics emission (Static Mode):");
        ResultMetrics.StaticTrackSuccess("InvoiceGenerated");
        ResultMetrics.StaticTrackFailure("InvoiceGenerated", "Invoice.TaxServiceTimeout", "Unavailable");
        Console.WriteLine("  Recorded metric counters: StaticTrackSuccess and StaticTrackFailure emitted successfully.");
    }
}
