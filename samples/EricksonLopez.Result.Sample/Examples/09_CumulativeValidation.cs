// Copyright © Erickson Lopez. MIT License.
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EricksonLopez.Result;

namespace EricksonLopez.Result.Sample.Examples;

public record RegisterUserDto(string Username, string Email, int Age);

public static class CumulativeValidation
{
    public static async Task RunAsync()
    {
        Console.WriteLine("\n========================================================");
        Console.WriteLine(" 09. CUMULATIVE VALIDATION (Result.ValidateAll)");
        Console.WriteLine("========================================================");

        var validUser = new RegisterUserDto("erickson", "dev@ericksonlopez.dev", 28);
        var invalidUser = new RegisterUserDto("", "not-an-email", 16);

        // -------------------------------------------------------------
        // 1. Synchronous Cumulative Validation with ReadOnlySpan
        // -------------------------------------------------------------
        Console.WriteLine("\n[1] Synchronous Result.ValidateAll with target instance:");

        Result<RegisterUserDto> validateOutcome = Result.ValidateAll(invalidUser,
            u => string.IsNullOrWhiteSpace(u.Username)
                ? Error.Validation("User.UsernameRequired", "Username cannot be blank.")
                : Result.Success(),
            u => !u.Email.Contains('@')
                ? Error.Validation("User.EmailInvalid", "Email format is invalid.")
                : Result.Success(),
            u => u.Age < 18
                ? Error.Validation("User.Underage", "User must be at least 18 years old.")
                : Result.Success()
        );

        Console.WriteLine($"Validation on invalid user: IsSuccess={validateOutcome.IsSuccess}, IsFailure={validateOutcome.IsFailure}");
        if (validateOutcome.IsFailure)
        {
            Console.WriteLine($"Main Error: [{validateOutcome.Error.Code}] {validateOutcome.Error.Description}");
            if (validateOutcome.Error.HasInnerErrors)
            {
                Console.WriteLine("Accumulated Inner Errors (ArrayPool-backed zero heap allocation):");
                foreach (var inner in validateOutcome.Error.InnerErrors)
                {
                    Console.WriteLine($"  -> [{inner.Code}] {inner.Description} (Type: {inner.Type}, Severity: {inner.Severity})");
                }
            }
        }

        // Validate valid user
        var validOutcome = Result.ValidateAll(validUser,
            u => string.IsNullOrWhiteSpace(u.Username) ? Error.Validation("User.UsernameRequired", "Username required") : Result.Success(),
            u => u.Age < 18 ? Error.Validation("User.Underage", "Underage") : Result.Success()
        );
        Console.WriteLine($"Validation on valid user: IsSuccess={validOutcome.IsSuccess}, Value={validOutcome.Value.Username}");

        // -------------------------------------------------------------
        // 2. Asynchronous Cumulative Validation (Task & ValueTask)
        // -------------------------------------------------------------
        Console.WriteLine("\n[2] Asynchronous Result.ValidateAllAsync (ValueTask / Task):");

        var asyncValidators = new List<Func<RegisterUserDto, CancellationToken, ValueTask<Result>>>
        {
            async (u, ct) =>
            {
                await Task.Yield();
                return string.IsNullOrWhiteSpace(u.Username)
                    ? Error.Validation("User.UsernameRequired", "Username is required.")
                    : Result.Success();
            },
            async (u, ct) =>
            {
                await Task.Yield();
                // Simulating database unique check
                bool emailExists = u.Email.Contains("exists", StringComparison.OrdinalIgnoreCase) || u.Email == "not-an-email";
                return emailExists
                    ? Error.Conflict("User.EmailExists", "Email address is already registered.")
                    : Result.Success();
            }
        };

        var asyncOutcome = await Result.ValidateAllAsync(invalidUser, asyncValidators);
        Console.WriteLine($"Async Validation: IsFailure={asyncOutcome.IsFailure}, Error={asyncOutcome.Error.Description}");
        if (asyncOutcome.Error.HasInnerErrors)
        {
            foreach (var err in asyncOutcome.Error.InnerErrors)
            {
                Console.WriteLine($"  -> [{err.Code}] {err.Description}");
            }
        }
    }
}
