using System;
using System.Threading;
using System.Threading.Tasks;
using EricksonLopez.Result;

namespace EricksonLopez.Result.Sample.Examples;

public static class AdvancedAsyncOperations
{
    public static async Task RunAsync()
    {
        Console.WriteLine("\n--- 15. ADVANCED ASYNC OPERATIONS (ValueTask & Cancellation) ---");

        var cts = new CancellationTokenSource();
        var token = cts.Token;

        // 1. ValueTask overloads
        // The library provides extensions for both Task<Result<T>> and ValueTask<Result<T>>
        ValueTask<Result<int>> GetValueTaskAsync() => new ValueTask<Result<int>>(Result.Success(42));
        
        Result<string> vtResult = await GetValueTaskAsync()
            .Map(val => $"ValueTask Mapped to: {val}", token)
            .TapOnSuccess(val => Console.WriteLine($"ValueTask Success Tap: {val}"), token)
            .TapOnFailure(err => Console.WriteLine($"ValueTask Failure Tap: {err.Code}"), token);
            
        Console.WriteLine($"ValueTask Chain Result: {vtResult.GetValueOrDefault("")}");

        // 2. Cancellation Tokens
        // All async extensions (Bind, Map, Ensure, Recover, etc.) accept CancellationToken
        Task<Result<int>> GetTaskAsync() => Task.FromResult(Result.Failure<int>(Error.Unexpected("Async.Fail", "Failed")));

        Result<int> recoveredTask = await GetTaskAsync()
            .Recover(err => 
            {
                Console.WriteLine($"Recovering from: {err.Code}");
                return 99; // Recover to a success state
            }, token)
            .Ensure(val => val > 0, Error.Validation("Val.Negative", "Must be positive"), token)
            .MapError(err => Error.Validation("Mapped.Error", "Mapped from previous"), token);
            
        Console.WriteLine($"Recovered Task Result IsSuccess: {recoveredTask.IsSuccess}, Value: {recoveredTask.GetValueOrDefault(-1)}");

        // 3. Inspect, Finally and Execute (Async side-effects)
        await GetTaskAsync()
            .Inspect(val => Console.WriteLine($"Inspected value: {val}"), token); // Only runs on success
            // .Finally is obsolete in favor of Inspect, but is shown commented out:
            // .Finally(_ => Console.WriteLine("Finally block ran."), token);

        await GetValueTaskAsync()
            .Execute(
                onSuccess: val => Console.WriteLine($"Execute Async Success: {val}"),
                onFailure: err => Console.WriteLine($"Execute Async Failure: {err.Code}"),
                token
            );
            
        // 4. MapFailure Async
        // Transforms a failed Result<T> task into a valid TOut directly (bypassing Result)
        string fallbackString = await GetTaskAsync()
            .MapFailure(
                onFailure: err => $"Handled async failure: {err.Code}",
                successDefault: "Success Default",
                token
            );
        Console.WriteLine($"MapFailure Async returned: {fallbackString}");
    }
}
