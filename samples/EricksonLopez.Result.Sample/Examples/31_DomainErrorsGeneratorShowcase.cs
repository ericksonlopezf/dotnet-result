// Copyright © Erickson Lopez. MIT License.
using System;
using EricksonLopez.Result;
using EricksonLopez.Result.Sample.GeneratedErrors;

namespace EricksonLopez.Result.Sample.Examples;

public static class DomainErrorsGeneratorShowcase
{
    public static void Run()
    {
        Console.WriteLine("\n========================================================");
        Console.WriteLine(" 31. DOMAIN ERRORS INCREMENTAL SOURCE GENERATOR SHOWCASE");
        Console.WriteLine("========================================================");

        // 1. Generated Error with format parameter
        Console.WriteLine("\n[1] Invoking generated OrderErrors.NotFound(orderId):");
        Error notFoundError = OrderErrors.NotFound("ORD-98765");
        Console.WriteLine($"  Code:           {notFoundError.Code}");
        Console.WriteLine($"  Description:    {notFoundError.Description}");
        Console.WriteLine($"  Type:           {notFoundError.Type}");
        Console.WriteLine($"  Severity:       {notFoundError.Severity}");
        Console.WriteLine($"  Retryability:   {notFoundError.Retryability}");
        Console.WriteLine($"  DescriptionKey: {notFoundError.DescriptionKey}");

        // 2. Generated Error with transient retryability
        Console.WriteLine("\n[2] Invoking generated OrderErrors.PaymentFailed(orderId):");
        Error paymentError = OrderErrors.PaymentFailed("ORD-98765");
        Console.WriteLine($"  Code:           {paymentError.Code}");
        Console.WriteLine($"  Description:    {paymentError.Description}");
        Console.WriteLine($"  Type:           {paymentError.Type}");
        Console.WriteLine($"  Severity:       {paymentError.Severity}");
        Console.WriteLine($"  Retryability:   {paymentError.Retryability}");

        // 3. Creating a Result using generated Domain Errors
        Console.WriteLine("\n[3] Using generated Error in a Result<T> failure track:");
        Result<string> orderResult = Result.Failure<string>(notFoundError);
        Console.WriteLine($"  Result.IsFailure: {orderResult.IsFailure}");
        Console.WriteLine($"  Result.Error.Code: {orderResult.Error.Code}");
    }
}
