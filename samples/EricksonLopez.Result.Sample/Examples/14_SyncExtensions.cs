// Copyright © Erickson Lopez. MIT License.
using System;
using EricksonLopez.Result;

namespace EricksonLopez.Result.Sample.Examples;

public static class SyncExtensions
{
    public static void Run()
    {
        Console.WriteLine("\n--- 14. SYNC EXTENSIONS (HIGH PERFORMANCE) ---");

        // The ResultSyncExtensions provide `ref` overloads for operations on Result<T>.
        // These are extremely useful in hot paths to avoid copying the struct when chaining operations.

        Result<int> initialResult = Result.Success(100);

        // 1. Map (ref)
        // Notice we pass `ref initialResult`. 
        Result<string> stringResult = initialResult.Map(val => $"Value is {val}");
        Console.WriteLine($"Ref Map: {stringResult.GetValueOrDefault("")}");

        // 2. Bind (ref)
        Result<string> boundResult = initialResult.Bind(val => Result.Success($"Bound {val}"));
        Console.WriteLine($"Ref Bind: {boundResult.GetValueOrDefault("")}");

        // 3. Ensure (ref)
        Result<int> ensuredResult = initialResult.Ensure(
            predicate: val => val > 50,
            error: Error.Validation("Value.TooSmall", "Value must be over 50.")
        );
        Console.WriteLine($"Ref Ensure (Success): {ensuredResult.IsSuccess}");

        // 4. Match (ref)
        string matchResult = initialResult.Match(
            onSuccess: val => $"Matched Value: {val}",
            onFailure: err => $"Failed: {err.Code}"
        );
        Console.WriteLine(matchResult);

        // 5. TryGetValue (ref)
        if (initialResult.TryGetValue(out int extracted))
        {
            Console.WriteLine($"Ref TryGetValue: {extracted}");
        }

        // 6. GetValueOrDefault (ref)
        int defaultResult = initialResult.GetValueOrDefault(-1);
        Console.WriteLine($"Ref GetValueOrDefault: {defaultResult}");
    }
}
