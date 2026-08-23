// Copyright © Erickson Lopez. MIT License.
using System;
using EricksonLopez.Result;

namespace EricksonLopez.Result.Sample.Examples;

public static class Combining
{
    public static void Run()
    {
        Console.WriteLine("\n--- 06. COMBINING RESULTS ---");

        // Sometimes you have multiple independent operations that return Results,
        // and you want to combine them into a single Result containing a tuple of their values,
        // failing if ANY of them failed.

        Result<string> firstNameResult = Result.Success("John");
        Result<string> lastNameResult = Result.Success("Doe");
        Result<int> ageResult = Result.Success(30);

        // 1. Combine all successes
        Result<(string, string, int)> combinedSuccess = Result.Combine(
            firstNameResult,
            lastNameResult,
            ageResult);

        if (combinedSuccess.IsSuccess)
        {
            var (first, last, age) = combinedSuccess.GetValueOrDefault(("", "", 0));
            Console.WriteLine($"Combined Success: {first} {last}, Age {age}");
        }

        // 2. Combine with a failure
        Result<int> invalidAgeResult = Result.Failure<int>(Error.Create("Age.Invalid", "Age cannot be negative.").Build());

        Result<(string, string, int)> combinedFailure = Result.Combine(
            firstNameResult,
            lastNameResult,
            invalidAgeResult);

        Console.WriteLine($"Combined Failure is failure: {combinedFailure.IsFailure}");
        Console.WriteLine($"Combined Failure Error Code: {combinedFailure.Error?.Code}");
    }
}
