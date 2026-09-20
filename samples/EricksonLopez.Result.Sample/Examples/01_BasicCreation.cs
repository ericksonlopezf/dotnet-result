// Copyright © Erickson Lopez. MIT License.
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

        // A TValue can also implicitly be converted to a successful Result<TValue>.
        Result<string> implicitValueSuccess = "hello";
        Console.WriteLine($"Implicit Value Success converted successfully: {implicitValueSuccess.IsSuccess}");

        // 6. Error.Business — semantic alias for Error.Domain
        // ----------------------------------------------------------
        // Error.Business is an alias for Error.Domain, provided for teams that
        // prefer the term "business rule" over "domain error". Both produce an
        // Error with Type=Domain and Severity=Error.
        Console.WriteLine("\n--- 6. Error.Business — business rule violation factory ---");

        Error businessError = Error.Business(
            "Order.CannotShip",
            "The order cannot be shipped because it has not been confirmed.");

        Console.WriteLine($"Code:     {businessError.Code}");
        Console.WriteLine($"Type:     {businessError.Type}");   // Domain
        Console.WriteLine($"Severity: {businessError.Severity}");

        // Confirm it is equivalent to Error.Domain:
        Error domainError = Error.Domain("Order.CannotShip", "The order cannot be shipped because it has not been confirmed.");
        Console.WriteLine($"Business.Type == Domain.Type: {businessError.Type == domainError.Type}");

        // 7. Result<TValue> → Result implicit conversion operator
        // ----------------------------------------------------------
        // A Result<T> can be implicitly widened to a non-generic Result,
        // discarding the typed value while preserving the outcome state.
        // This is useful when a caller returns Result<T> but the caller's
        // signature requires a plain Result (e.g., command handlers, middleware).
        Console.WriteLine("\n--- 7. Result<TValue> → Result implicit widening ---");

        Result<int> typedSuccess  = Result.Success(99);
        Result<int> typedFailure  = Result.Failure<int>(myError);

        // Widening — no explicit cast needed:
        Result widenedSuccess = typedSuccess;   // preserves Success state
        Result widenedFailure = typedFailure;   // preserves Failure state + Error

        Console.WriteLine($"typedSuccess  (Result<int>) → Result: IsSuccess={widenedSuccess.IsSuccess}");
        Console.WriteLine($"typedFailure  (Result<int>) → Result: IsFailure={widenedFailure.IsFailure}, Code={widenedFailure.Error.Code}");

        // Same as calling .DiscardValue() explicitly:
        Result viaMethod = typedSuccess.DiscardValue();
        Console.WriteLine($"DiscardValue()  IsSuccess={viaMethod.IsSuccess}");
    }
}
