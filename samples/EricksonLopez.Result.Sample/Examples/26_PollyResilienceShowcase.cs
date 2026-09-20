// Copyright © Erickson Lopez. MIT License.
using System;
using System.Threading;
using System.Threading.Tasks;
using EricksonLopez.Result;
using EricksonLopez.Result.Polly;
using Polly;

namespace EricksonLopez.Result.Sample.Examples;

public static class PollyResilienceShowcase
{
    public static async Task RunAsync()
    {
        Console.WriteLine("\n========================================================");
        Console.WriteLine(" 26. POLLY V8 RESILIENCE PIPELINE SHOWCASE");
        Console.WriteLine("========================================================");

        // 1. Build a generic ResiliencePipeline for Result<string> with AddResultRetry
        Console.WriteLine("\n[1] Configure ResiliencePipelineBuilder with AddResultRetry:");
        var pipeline = new ResiliencePipelineBuilder<Result<string>>()
            .AddResultRetry(maxRetryAttempts: 3, delay: TimeSpan.FromMilliseconds(50))
            .Build();

        // Simulate an operation that fails twice with Transient error, then succeeds on attempt 3
        int attempts = 0;
        Result<string> retryOutcome = await pipeline.ExecuteResultAsync(async ct =>
        {
            attempts++;
            Console.WriteLine($"  [Attempt {attempts}] Executing remote HTTP call...");
            if (attempts < 3)
            {
                // Transient error: AddResultRetry handles this automatically!
                return Result.Failure<string>(
                    Error.Create("Gateway.Timeout", "Upstream payment gateway timeout.")
                         .WithType(ErrorType.Unavailable)
                         .WithRetryability(ErrorRetryability.Transient)
                         .Build());
            }

            return Result.Success("TXN-SUCCESS-998822");
        });

        Console.WriteLine($"  Result after retries: IsSuccess={retryOutcome.IsSuccess}, Value={retryOutcome.Value}");
        Console.WriteLine($"  Total attempts made: {attempts}");

        // 2. Non-generic pipeline with AddResultRetry
        Console.WriteLine("\n[2] Non-generic ResiliencePipeline with AddResultRetry:");
        var nonGenericRetryPipeline = new ResiliencePipelineBuilder<Result>()
            .AddResultRetry(maxRetryAttempts: 2, delay: TimeSpan.FromMilliseconds(20))
            .Build();

        int commandAttempts = 0;
        Result cmdOutcome = await nonGenericRetryPipeline.ExecuteAsync(async ct =>
        {
            commandAttempts++;
            if (commandAttempts == 1)
            {
                return Result.Failure(
                    Error.Create("Database.Deadlock", "Deadlock victim error.")
                         .WithType(ErrorType.Failure)
                         .WithRetryability(ErrorRetryability.Transient)
                         .Build());
            }

            return Result.Success();
        });

        Console.WriteLine($"  Command retried and succeeded: IsSuccess={cmdOutcome.IsSuccess}, Attempts={commandAttempts}");

        // Demonstration of ExecuteResultAsync on standard ResiliencePipeline
        var standardPipeline = new ResiliencePipelineBuilder().Build();
        Result standardOutcome = await standardPipeline.ExecuteResultAsync(async ct =>
        {
            return Result.Success();
        });
        Console.WriteLine($"  Standard ResiliencePipeline.ExecuteResultAsync: IsSuccess={standardOutcome.IsSuccess}");

        // 3. Permanent error — AddResultRetry does NOT retry Permanent errors
        Console.WriteLine("\n[3] Permanent error rejection without useless retries:");
        int permanentAttempts = 0;
        Result<string> permanentOutcome = await pipeline.ExecuteResultAsync(async ct =>
        {
            permanentAttempts++;
            return Result.Failure<string>(
                Error.Create("Auth.InvalidCredentials", "API Key is revoked.")
                     .WithType(ErrorType.Unauthorized)
                     .WithRetryability(ErrorRetryability.Permanent)
                     .Build());
        });

        Console.WriteLine($"  Permanent failure returned immediately: IsFailure={permanentOutcome.IsFailure}, Total Attempts={permanentAttempts}");

        // 4. Synchronous ExecuteResult and State-Passing
        Console.WriteLine("\n[4] Synchronous ExecuteResult with allocation-free state passing:");
        var syncPipeline = new ResiliencePipelineBuilder().Build();
        var syncResult = syncPipeline.ExecuteResult(
            state: "CachedResource",
            callback: static key => Result.Success($"Loaded {key} without allocation"));

        Console.WriteLine($"  Sync ExecuteResult: {syncResult.Value}");
    }
}
