using System;
using EricksonLopez.Result;

namespace EricksonLopez.Result.Sample.Examples;

public static class BasicCreation
{
    public static void Run()
    {
        Console.WriteLine("\n--- 01. BASIC CREATION ---");

        // 1. Success without a value
        Result successResult = Result.Success();
        Console.WriteLine($"IsSuccess: {successResult.IsSuccess}");

        // 2. Failure without a value
        Error myError = Error.Create("User.NotFound", "The requested user does not exist in the database.").Build();
        Result failureResult = Result.Failure(myError);
        Console.WriteLine($"IsFailure: {failureResult.IsFailure}, Error Code: {failureResult.Error.Code}");

        // 3. Success with a value (Result<T>)
        Result<int> valueSuccess = Result.Success(42);
        Console.WriteLine($"Value Success: {valueSuccess.IsSuccess}, Value (using Match): {valueSuccess.Match(v => v.ToString(System.Globalization.CultureInfo.InvariantCulture), err => err.Code)}");

        // 4. Failure with a value (Result<T>)
        Result<int> valueFailure = Result.Failure<int>(myError);
        Console.WriteLine($"Value Failure: {valueFailure.IsFailure}, Error: {valueFailure.Error.Description}");

        // 5. Implicit Conversions
        // An Error can implicitly be converted to a Failure Result.
        Result implicitFailure = myError;
        Result<string> implicitValueFailure = myError;

        Console.WriteLine($"Implicit Failure converted successfully: {implicitFailure.IsFailure}");
        Console.WriteLine($"Implicit Value Failure converted successfully: {implicitValueFailure.IsFailure}");
    }
}
