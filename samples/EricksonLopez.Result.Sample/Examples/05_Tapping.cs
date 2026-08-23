// Copyright © Erickson Lopez. MIT License.
using System;
using EricksonLopez.Result;

namespace EricksonLopez.Result.Sample.Examples;

public static class Tapping
{
    public static void Run()
    {
        Console.WriteLine("\n--- 05. TAPPING (SIDE-EFFECTS IN PIPELINES) ---");

        // Tap allows you to inspect the value or execute a side-effect (like logging)
        // without altering the pipeline's Success/Failure state or its value.

        Result<int> successResult = Result.Success(42);
        Result<int> failureResult = Result.Failure<int>(Error.Create("Demo.Error", "Demonstrating Tap").Build());

        // 1. TapOnSuccess (Always executes, regardless of success or failure)
        Console.WriteLine("Executing TapOnSuccess on Success:");
        successResult.TapOnSuccess(val => Console.WriteLine($" [TapOnSuccess] Pipeline value is currently: {val}"));

        // Note: TapOnSuccess on a value Result only executes the action if it is a Success.
        Console.WriteLine("Executing TapOnSuccess on Failure (should not print value):");
        failureResult.TapOnSuccess(val => Console.WriteLine($" [TapOnSuccess] This won't print: {val}"));

        // 2. TapOnSuccess
        // Explicitly expresses the intent that this side-effect should only happen on success.
        Console.WriteLine("Executing TapOnSuccess on Success:");
        successResult.TapOnSuccess(val => Console.WriteLine($" [TapOnSuccess] Executed with: {val}"));

        // You can also use TapOnSuccess on non-value Results (Result)
        Result emptySuccess = Result.Success();
        Result emptyFailure = Result.Failure(Error.Create("Empty.Failure", "No value").Build());

        Console.WriteLine("Executing TapOnSuccess on non-value Success:");
        emptySuccess.TapOnSuccess(() => Console.WriteLine(" [TapOnSuccess] Non-value Result was successful."));

        Console.WriteLine("Executing TapOnSuccess on non-value Failure:");
        // For parameterless TapOnSuccess, it also only executes on Success conceptually in this library's design.
        emptyFailure.TapOnSuccess(() => Console.WriteLine(" [TapOnSuccess] This also won't print because it's a failure."));

        // 3. TapOnFailure
        // Explicitly expresses the intent that this side-effect should only happen on failure.
        Console.WriteLine("Executing TapOnFailure on Failure:");
        failureResult.TapOnFailure(err => Console.WriteLine($" [TapOnFailure] Executed because it failed with: {err.Code}"));

        Console.WriteLine("Executing TapOnFailure on Success (should not print):");
        successResult.TapOnFailure(err => Console.WriteLine($" [TapOnFailure] This won't print."));
    }
}
