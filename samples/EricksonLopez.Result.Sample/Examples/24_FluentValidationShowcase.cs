// Copyright © Erickson Lopez. MIT License.
using System;
using System.Threading.Tasks;
using EricksonLopez.Result;
using EricksonLopez.Result.FluentValidation;
using FluentValidation;

namespace EricksonLopez.Result.Sample.Examples;

public record RegisterCustomerCommand(string Name, string Email, int Age, string Password);

public sealed class RegisterCustomerCommandValidator : AbstractValidator<RegisterCustomerCommand>
{
    public RegisterCustomerCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithErrorCode("Customer.Name.Required")
            .WithMessage("Customer name is required.");

        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .WithErrorCode("Customer.Email.Invalid")
            .WithMessage("A valid email address is required.");

        RuleFor(x => x.Age)
            .GreaterThanOrEqualTo(18)
            .WithErrorCode("Customer.Age.Underage")
            .WithMessage("Customer must be at least 18 years old.");

        RuleFor(x => x.Password)
            .MinimumLength(8)
            .WithErrorCode("Customer.Password.TooShort")
            .WithMessage("Password must be at least 8 characters.");
    }
}

public static class FluentValidationShowcase
{
    public static async Task RunAsync()
    {
        Console.WriteLine("\n========================================================");
        Console.WriteLine(" 24. FLUENTVALIDATION INTEGRATION SHOWCASE");
        Console.WriteLine("========================================================");

        var validator = new RegisterCustomerCommandValidator();

        // 1. ValidateToResult (Non-generic validation)
        Console.WriteLine("\n[1] ValidateToResult — command validation:");
        var validCommand = new RegisterCustomerCommand("Alice Smith", "alice@example.com", 25, "SuperSecret123!");
        Result validResult = validator.ValidateToResult(validCommand);
        Console.WriteLine($"  Valid input IsSuccess: {validResult.IsSuccess}");

        var invalidCommand = new RegisterCustomerCommand("", "invalid-email", 16, "short");
        Result invalidResult = validator.ValidateToResult(invalidCommand);
        Console.WriteLine($"  Invalid input IsFailure: {invalidResult.IsFailure}");
        Console.WriteLine($"  Aggregate Code: [{invalidResult.Error.Code}] - {invalidResult.Error.Description}");
        Console.WriteLine($"  Inner Errors count: {invalidResult.Error.InnerErrors.Length}");
        foreach (var inner in invalidResult.Error.InnerErrors)
        {
            Console.WriteLine($"    -> [{inner.Code}]: {inner.Description}");
        }

        // 2. ValidateToResultWithValue (Typed validation wrapping the valid entity)
        Console.WriteLine("\n[2] ValidateToResultWithValue — wrapping valid instance:");
        Result<RegisterCustomerCommand> typedSuccess = validator.ValidateToResultWithValue(validCommand);
        Console.WriteLine($"  Success: IsSuccess={typedSuccess.IsSuccess}, Customer={typedSuccess.Value.Name}");

        Result<RegisterCustomerCommand> typedFailure = validator.ValidateToResultWithValue(invalidCommand);
        Console.WriteLine($"  Failure: IsFailure={typedFailure.IsFailure}, ErrorType={typedFailure.Error.Type}");

        // 3. EnsureValid into pipeline with sensitive property redaction check
        Console.WriteLine("\n[3] EnsureValid monadic pipeline integration:");
        Result<RegisterCustomerCommand> pipelineResult = Result.Success(invalidCommand)
            .EnsureValid(validator);

        Console.WriteLine($"  Pipeline rejected invalid command: IsFailure={pipelineResult.IsFailure}");
        // Notice password metadata is automatically redacted by the integration
        foreach (var inner in pipelineResult.Error.InnerErrors)
        {
            if (inner.Code == "Customer.Password.TooShort" && inner.TryGetMetadata<string>("attemptedValue", out var attemptedVal))
            {
                Console.WriteLine($"  Password attemptedValue is safely redacted: {attemptedVal}");
            }
        }

        // 4. Async validation overloads
        Console.WriteLine("\n[4] ValidateToResultAsync & EnsureValidAsync:");
        Result asyncValidation = await validator.ValidateToResultAsync(validCommand);
        Console.WriteLine($"  Async ValidateToResultAsync IsSuccess: {asyncValidation.IsSuccess}");

        Result<RegisterCustomerCommand> asyncPipeline = await Task.FromResult(Result.Success(validCommand))
            .EnsureValidAsync(validator);
        Console.WriteLine($"  Async EnsureValidAsync IsSuccess: {asyncPipeline.IsSuccess}");
    }
}
