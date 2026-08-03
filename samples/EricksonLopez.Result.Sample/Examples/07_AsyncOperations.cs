using System;
using System.Threading;
using System.Threading.Tasks;
using EricksonLopez.Result;

namespace EricksonLopez.Result.Sample.Examples;

public static class AsyncOperations
{
    public static async Task RunAsync()
    {
        Console.WriteLine("\n--- 07. ASYNC OPERATIONS ---");

        // 1. TryAsync
        // Safely executes a Task-returning function, catching any exceptions 
        // and converting them into an Error via the provided error handler.

        Error errorHandler(Exception ex) => Error.Create("Async.Failed", $"Exception caught: {ex.Message}").Build();

        Result<string> asyncResult = await Result.TryAsync(
            async (CancellationToken ct) => 
            {
                await Task.Delay(10, ct);
                return "Async Data Loaded";
            },
            errorHandler
        );
        Console.WriteLine($"TryAsync success: {asyncResult.IsSuccess}, Value: {asyncResult.GetValueOrDefault("")}");

        // Exception example
        Result<string> asyncException = await Result.TryAsync(
            async (CancellationToken ct) => 
            {
                await Task.Delay(10, ct);
                throw new InvalidOperationException("Network disconnected");
#pragma warning disable CS0162
                return "Won't reach";
#pragma warning restore CS0162
            },
            errorHandler
        );
        Console.WriteLine($"TryAsync failure caught exception: {asyncException.IsFailure}, Error: {asyncException.Error?.Description}");

        // 2. Async Extensions (Bind, Map on Tasks)
        // If you have a Task<Result<T>>, you can call Map/Bind directly on it without awaiting first.
        Task<Result<string>> FetchUserAsync() => Task.FromResult(Result.Success("User123"));
        
        // Let's bind it with an async operation
        Task<Result<string>> ValidateUserAsync(string user, CancellationToken ct) => 
            Task.FromResult(Result.Success($"{user} is valid"));

        Result<string> chainedAsync = await FetchUserAsync()
            .Bind(user => ValidateUserAsync(user, default));

        Console.WriteLine($"Async Bind result: {chainedAsync.GetValueOrDefault("")}");
    }
}
