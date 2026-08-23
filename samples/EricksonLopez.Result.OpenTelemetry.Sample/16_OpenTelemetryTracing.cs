// Copyright © Erickson Lopez. MIT License.
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using EricksonLopez.Result;
using EricksonLopez.Result.OpenTelemetry;

namespace EricksonLopez.Result.OpenTelemetry.Sample;

public static class OpenTelemetryTracing
{
    private static readonly ActivitySource ActivitySource = new("EricksonLopez.Result.Sample");

    public static async Task RunAsync(ResultMetrics metrics)
    {
        Console.WriteLine("\n--- 16. OPENTELEMETRY TRACING ---");

        // The library integrates natively with System.Diagnostics.Activity to attach
        // semantic conventions and events to spans based on Result success/failure.

        // 1. Manual Metric Tracking
        // You can use ResultMetrics (injected via DI) to manually track arbitrary outcomes
        metrics.TrackSuccess("Manual_Operation");
        metrics.TrackFailure("Manual_Operation", "Db.Timeout", "Infrastructure");

        // Note: ResultMetrics.StaticTrackSuccess("Static_Operation"); 
        // is available if you don't use DI, but it will throw an exception if 
        // services.AddResultMetrics() is already active to prevent double-counting.

        // 2. Tracing Sync Results
        using (var syncActivity = ActivitySource.StartActivity("SyncDatabaseCall"))
        {
            Result<int> dbResult = Result.Failure<int>(Error.Infrastructure("Db.Deadlock", "Deadlock detected"));

            // TraceOnFailure adds an Exception event and tags to the activity only if it fails
            dbResult.TraceOnFailure("SyncDatabaseCall", syncActivity, metrics);

            // TraceOutcome logs both Success and Failure depending on the state
            Result.Success().TraceOutcome("AnotherCall", syncActivity, metrics);
        }

        // 3. Tracing Async Results via pipelines
        using (var asyncActivity = ActivitySource.StartActivity("AsyncServiceCall"))
        {
            Task<Result<string>> FetchDataAsync() => Task.FromResult(Result.Success("Data Loaded"));

            // The async extensions allow chaining the tracing directly into the Task pipeline
            Result<string> asyncResult = await FetchDataAsync()
                .TraceOnSuccess("AsyncServiceCall", asyncActivity, metrics);

            Console.WriteLine($"Async Trace pipeline result: {asyncResult.GetValueOrDefault("")}");
        }

        Console.WriteLine("OpenTelemetry metrics and traces have been emitted (check your OTEL exporter/console if configured).");
    }
}





