// Copyright © Erickson Lopez. MIT License.
using System;
using EricksonLopez.Result;

namespace EricksonLopez.Result.Sample.Examples;

public static class SafeAccessAndDeconstruct
{
    public static void Run()
    {
        Console.WriteLine("\n========================================================");
        Console.WriteLine(" 17. SAFE ACCESS, TRY-PATTERN & DECONSTRUCT");
        Console.WriteLine("========================================================");

        // -------------------------------------------------------------
        // 1. GetValueOrDefault — safe fallback, never throws
        // -------------------------------------------------------------
        Console.WriteLine("\n[1] GetValueOrDefault — BCL *OrDefault convention:");

        Result<int> success = Result.Success(42);
        Result<int> failure = Result.Failure<int>(Error.NotFound("Item.Missing", "Item not found."));

        int successVal = success.GetValueOrDefault(0);
        int failureVal = failure.GetValueOrDefault(-1);

        Console.WriteLine($"  Success: {successVal}");    // 42
        Console.WriteLine($"  Failure: {failureVal}");    // -1

        // IMPORTANT: GetValueOrDefault NEVER THROWS — even on uninitialized results.
        // Per BCL *OrDefault convention, it returns the default for any non-success state.
        Result<string> uninitialized = default;
        string safeFromUninitialized = uninitialized.GetValueOrDefault("(default)");
        Console.WriteLine($"  Uninitialized (no throw): \"{safeFromUninitialized}\"");

        // -------------------------------------------------------------
        // 2. GetValueOrFallback — error-aware fallback with lazy computation
        // -------------------------------------------------------------
        // Different from GetValueOrDefault: the fallback is a Func that receives
        // the error, so you can produce a contextual fallback value.
        // THROWS on uninitialized results (unlike GetValueOrDefault).
        Console.WriteLine("\n[2] GetValueOrFallback — error-aware lazy fallback:");

        Result<string> orderResult = Result.Failure<string>(
            Error.NotFound("Order.NotFound", "Order #99 does not exist."));

        string fallback = orderResult.GetValueOrFallback(
            err => $"[Fallback due to {err.Code}]"
        );
        Console.WriteLine($"  Fallback value: \"{fallback}\"");

        // With state (allocation-free):
        string prefix = "[ERR]";
        string fallbackWithState = orderResult.GetValueOrFallback(
            state: prefix,
            fallback: (state, err) => $"{state} {err.Code}: {err.Description}"
        );
        Console.WriteLine($"  Fallback with state: \"{fallbackWithState}\"");

        // On success: returns the value directly
        Result<string> okResult = Result.Success("Order-12345");
        string fromSuccess = okResult.GetValueOrFallback(err => "fallback");
        Console.WriteLine($"  GetValueOrFallback on success: \"{fromSuccess}\"");

        // -------------------------------------------------------------
        // 3. TryGetValue — BCL Try-pattern for success
        // -------------------------------------------------------------
        // Returns false for failure AND uninitialized (no throw).
        Console.WriteLine("\n[3] TryGetValue — pattern matching style:");

        if (success.TryGetValue(out int val))
            Console.WriteLine($"  TryGetValue success: {val}");

        if (!failure.TryGetValue(out int _))
            Console.WriteLine("  TryGetValue failure: returned false (no value)");

        // Disambiguation overload: distinguishes Failure from Uninitialized
        Result<int> uninitResult = default;
        bool got = uninitResult.TryGetValue(out int maybeVal, out bool isUninitialized);
        Console.WriteLine($"  TryGetValue(uninitialized): got={got}, isUninitialized={isUninitialized}");

        // -------------------------------------------------------------
        // 4. TryGetError — BCL Try-pattern for failures
        // -------------------------------------------------------------
        Console.WriteLine("\n[4] TryGetError — pattern matching on failure:");

        if (failure.TryGetError(out Error? err))
            Console.WriteLine($"  TryGetError failure: [{err.Code}] {err.Description}");

        if (!success.TryGetError(out Error? _))
            Console.WriteLine("  TryGetError success: returned false (no error)");

        // Disambiguation overload
        bool hasErr = uninitResult.TryGetError(out Error? maybeErr, out bool isUninit);
        Console.WriteLine($"  TryGetError(uninitialized): hasErr={hasErr}, isUninitialized={isUninit}");

        // -------------------------------------------------------------
        // 5. Deconstruct — C# destructuring syntax
        // -------------------------------------------------------------
        // Allows idiomatic tuple-like deconstruction of Result<T>.
        Console.WriteLine("\n[5] Deconstruct — C# destructuring:");

        Result<string> nameResult = Result.Success("Erickson");
        var (isOk, value, error) = nameResult;
        Console.WriteLine($"  Deconstruct success: isOk={isOk}, value=\"{value}\"");

        Result<string> failedName = Result.Failure<string>(
            Error.Validation("Name.Required", "Name cannot be blank."));
        var (isFail, fValue, fError) = failedName;
        Console.WriteLine($"  Deconstruct failure: isOk={isFail}, error=[{fError?.Code}]");

        // 2-arg overload: isSuccess + error only (ignores value)
        Result<int> score = Result.Success(100);
        var (won, scoreError) = score;
        Console.WriteLine($"  Deconstruct (2 args): isSuccess={won}, error={scoreError?.Code ?? "null"}");

        // Idiomatic use in an if statement
        if (nameResult is { IsSuccess: true, Value: var name })
            Console.WriteLine($"  Pattern matching on struct: name=\"{name}\"");
    }
}
