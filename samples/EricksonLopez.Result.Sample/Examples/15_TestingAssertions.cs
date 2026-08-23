// Copyright © Erickson Lopez. MIT License.
using System;
using EricksonLopez.Result;
using EricksonLopez.Result.Testing;

namespace EricksonLopez.Result.Sample.Examples;

public static class TestingAssertionsExample
{
    public static void Run()
    {
        Console.WriteLine("\n========================================================");
        Console.WriteLine(" 15. FLUENT TESTING ASSERTIONS (EricksonLopez.Result.Testing)");
        Console.WriteLine("========================================================");

        // -------------------------------------------------------------
        // 1. Success Assertions
        // -------------------------------------------------------------
        Console.WriteLine("\n[1] Testing Success Outcomes:");

        Result<string> successResult = Result.Success("Order-12345");

        // Returns the underlying value for further fluent assertions
        string orderId = successResult.ShouldBeSuccess();
        Console.WriteLine($"Asserted Success returned value: {orderId}");

        Result nonGenericSuccess = Result.Success();
        nonGenericSuccess.ShouldBeSuccess();
        Console.WriteLine("Asserted non-generic Success successfully.");

        // -------------------------------------------------------------
        // 2. Failure Assertions and Error Verification
        // -------------------------------------------------------------
        Console.WriteLine("\n[2] Testing Failure Outcomes & Fluent Error Chaining:");

        Result<string> failureResult = Result.Failure<string>(
            Error.Create("Order.InvalidPayment", "Payment method expired.")
                .WithType(ErrorType.Validation)
                .WithSeverity(ErrorSeverity.Warning)
                .WithRetryability(ErrorRetryability.NotApplicable)
                .WithMetadata("OrderId", 12345)
                .Build()
        );

        // ShouldBeFailure returns the Error object for fluent chaining
        Error error = failureResult.ShouldBeFailure()
            .ShouldHaveErrorCode("Order.InvalidPayment")
            .ShouldHaveErrorType(ErrorType.Validation)
            .ShouldHaveSeverity(ErrorSeverity.Warning)
            .ShouldHaveRetryability(ErrorRetryability.NotApplicable)
            .ShouldHaveDescription("Payment method expired.")
            .ShouldHaveMetadata("OrderId", 12345);

        Console.WriteLine($"Chained assertions verified on Error: [{error.Code}] {error.Description}");

        // -------------------------------------------------------------
        // 3. Exception Behavior on Failure Mismatch
        // -------------------------------------------------------------
        Console.WriteLine("\n[3] Handling assertion failure exceptions:");

        try
        {
            // Asserting Failure on a Success result throws ResultAssertionException
            successResult.ShouldBeFailure();
        }
        catch (ResultAssertionException ex)
        {
            Console.WriteLine($"Expected assertion exception caught: {ex.Message}");
        }
    }
}
