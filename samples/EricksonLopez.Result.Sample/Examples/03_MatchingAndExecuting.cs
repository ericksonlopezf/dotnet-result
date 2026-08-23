// Copyright © Erickson Lopez. MIT License.
using System;
using EricksonLopez.Result;

namespace EricksonLopez.Result.Sample.Examples;

public static class MatchingAndExecuting
{
    public static void Run()
    {
        Console.WriteLine("\n--- 03. MATCHING AND EXECUTING ---");

        Result<string> successResult = Result.Success("Hello from Result!");
        Result<string> failureResult = Result.Failure<string>(
            Error.Create("Auth.Denied", "Access Denied.").Build());

        // 1. Match (When you need a return value)
        // Resolves the Result into a single type depending on if it's Success or Failure.
        string successMessage = successResult.Match(
            onSuccess: value => $"Match resolved Success: {value}",
            onFailure: error => $"Match resolved Failure: {error.Code}"
        );
        Console.WriteLine(successMessage);

        string failureMessage = failureResult.Match(
            onSuccess: value => $"Match resolved Success: {value}",
            onFailure: error => $"Match resolved Failure: {error.Code}"
        );
        Console.WriteLine(failureMessage);

        // 2. Execute (When you just want to run side-effects/void actions)
        Console.Write("Executing Success Result: ");
        successResult.Execute(
            onSuccess: value => Console.WriteLine($"Printed value '{value}'"),
            onFailure: error => Console.WriteLine($"Printed error '{error.Code}'")
        );

        // 3. MatchError / MapFailure (When you only care about handling the error)
        // Transforms the error, but returns a default value if it was a success.
        string mapFailureDemo = failureResult.MapFailure(
            onFailure: err => $"Handled error: {err.Code}",
            successDefault: "No error occurred."
        );
        Console.WriteLine($"MapFailure result: {mapFailureDemo}");

        // 4. TryGetValue & GetValueOrDefault
        if (successResult.TryGetValue(out string? val))
        {
            Console.WriteLine($"TryGetValue found: {val}");
        }

        string fallbackValue = failureResult.GetValueOrDefault("FallbackString");
        Console.WriteLine($"GetValueOrDefault returned: {fallbackValue}");

        // 5. GetValueOrFallback (Lazy evaluation of the fallback)
        string computedFallback = failureResult.GetValueOrFallback(error => $"Computed based on: {error.Code}");
        Console.WriteLine($"GetValueOrFallback returned: {computedFallback}");

        // 6. FoldError (Similar to MapFailure but specifically folds the error into a return type)
        // Note: FoldError is obsolete in favor of MapFailure.
        // string foldedError = failureResult.FoldError(
        //     onFailure: error => $"Folded: {error.Description}",
        //     successDefault: "Success!"
        // );
        // Console.WriteLine($"FoldError returned: {foldedError}");

        // 7. DiscardValue & WithoutValue
        // When you have a Result<T> but only care about success/failure (Result)
        // Note: WithoutValue() is obsolete in favor of DiscardValue()
        // Result unitResult1 = successResult.WithoutValue();
        Result unitResult2 = successResult.DiscardValue();
        Console.WriteLine($"DiscardValue IsSuccess: {unitResult2.IsSuccess}");

        // 8. Merge
        // Useful for merging a valueless guard Result with a value-returning Result<T>
        Result guardResult = Result.Success();
        Result<string> nextResult = Result.Success("Merged Value");
        // Result.Merge takes them by `in` for high performance
        Result<string> mergedResult = Result.Merge(in guardResult, in nextResult);
        Console.WriteLine($"Merged Result Value: {mergedResult.GetValueOrDefault("")}");
    }
}
