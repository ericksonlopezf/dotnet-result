// Copyright © Erickson Lopez. MIT License.
using System;
using EricksonLopez.Result;
using EricksonLopez.Result.Generic;

namespace EricksonLopez.Result.Sample.Examples;

// Custom strongly-typed domain error hierarchy
public abstract record DomainError(string Code, string Message);
public record InsufficientFundsError(decimal CurrentBalance, decimal RequestedAmount) 
    : DomainError("Account.InsufficientFunds", $"Balance {CurrentBalance:C} is less than requested {RequestedAmount:C}");
public record AccountLockedError(string Reason) 
    : DomainError("Account.Locked", $"Account is locked: {Reason}");

public static class GenericResult
{
    public static void Run()
    {
        Console.WriteLine("\n========================================================");
        Console.WriteLine(" 11. STRONGLY-TYPED RESULT<TValue, TError>");
        Console.WriteLine("========================================================");

        // -------------------------------------------------------------
        // 1. Creating Strongly-Typed Results
        // -------------------------------------------------------------
        Console.WriteLine("\n[1] Strongly-typed Result creation:");

        Result<decimal, DomainError> Withdraw(decimal balance, decimal amount)
        {
            if (balance < amount)
                return Result<decimal, DomainError>.Failure(new InsufficientFundsError(balance, amount));

            return Result<decimal, DomainError>.Success(balance - amount);
        }

        var successOutcome = Withdraw(500m, 150m);
        var failureOutcome = Withdraw(100m, 300m);

        Console.WriteLine($"Withdraw Success: IsSuccess={successOutcome.IsSuccess}, New Balance={successOutcome.Value:C}");
        Console.WriteLine($"Withdraw Failure: IsFailure={failureOutcome.IsFailure}, ErrorType={failureOutcome.Error.GetType().Name}, Code={failureOutcome.Error.Code}");

        // -------------------------------------------------------------
        // 2. Monadic Mapping & Error Projection
        // -------------------------------------------------------------
        Console.WriteLine("\n[2] Mapping Value and Projecting Error:");

        Result<string, DomainError> formattedBalance = successOutcome.Map(b => $"Remaining: {b:C}");
        Console.WriteLine($"Mapped Value: {formattedBalance.Value}");

        // Transforming error type
        Result<decimal, string> simplifiedError = failureOutcome.MapError(err => $"Error [{err.Code}]: {err.Message}");
        Console.WriteLine($"Mapped Error: {simplifiedError.Error}");

        // -------------------------------------------------------------
        // 3. Pattern Matching
        // -------------------------------------------------------------
        Console.WriteLine("\n[3] Pattern Matching with typed errors:");

        string summary = failureOutcome.Match(
            onSuccess: b => $"Approved with balance {b:C}",
            onFailure: err => err switch
            {
                InsufficientFundsError f => $"Declined: needs {f.RequestedAmount:C} but only has {f.CurrentBalance:C}",
                AccountLockedError l => $"Security alert: {l.Reason}",
                _ => $"Generic error: {err.Message}"
            }
        );
        Console.WriteLine($"Matched Summary: {summary}");

        // -------------------------------------------------------------
        // 4. Interoperability with Standard Result<T>
        // -------------------------------------------------------------
        Console.WriteLine("\n[4] Converting to Standard EricksonLopez.Result.Result<T>:");

        EricksonLopez.Result.Result<decimal> standardResult = failureOutcome.ToResult(err =>
            Error.Create(err.Code, err.Message)
                .WithType(ErrorType.Domain)
                .WithSeverity(ErrorSeverity.Warning)
                .Build()
        );

        Console.WriteLine($"Standard Result: IsFailure={standardResult.IsFailure}, ErrorCode={standardResult.Error.Code}, Type={standardResult.Error.Type}");
    }
}
