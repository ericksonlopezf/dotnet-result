// Copyright © Erickson Lopez. MIT License.
using System;
using System.Threading;
using System.Threading.Tasks;
using EricksonLopez.Result;
using EricksonLopez.Result.FluentValidation;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace EricksonLopez.Result.FluentValidation.Sample;

public record CreateUserRequest(string Email, int Age);

public class CreateUserRequestValidator : AbstractValidator<CreateUserRequest>
{
    public CreateUserRequestValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Age).GreaterThanOrEqualTo(18);
    }
}

public static class FluentValidationExample
{
    public static async Task RunAsync(IServiceProvider services)
    {
        Console.WriteLine("\n--- 10. FLUENT VALIDATION ---");

        var validator = services.GetRequiredService<IValidator<CreateUserRequest>>();

        var validRequest = new CreateUserRequest("test@domain.com", 25);
        var invalidRequest = new CreateUserRequest("not-an-email", 15);

        // EricksonLopez.Result.FluentValidation adds ValidateToResult and ValidateToResultWithValue
        Result validResult = await validator.ValidateToResultAsync(validRequest);
        Console.WriteLine($"Valid Request IsSuccess: {validResult.IsSuccess}");

        Result invalidResult = await validator.ValidateToResultAsync(invalidRequest);
        Console.WriteLine($"Invalid Request IsFailure: {invalidResult.IsFailure}");

        if (invalidResult.IsFailure)
        {
            Console.WriteLine($"Error Code: {invalidResult.Error.Code}");
            Console.WriteLine($"Error Description: {invalidResult.Error.Description}");
            if (invalidResult.Error.InnerErrors != null)
            {
                Console.WriteLine("Inner Validation Errors:");
                foreach (var err in invalidResult.Error.InnerErrors)
                {
                    Console.WriteLine($" - {err.Code}: {err.Description}");
                }
            }
        }

        // 3. EnsureValid (Pipeline chaining)
        Result<CreateUserRequest> requestResult = Result.Success(validRequest);
        Result<CreateUserRequest> ensured = requestResult.EnsureValid(validator);
        Console.WriteLine($"EnsureValid (Pipeline) on valid request: {ensured.IsSuccess}");

        // 4. Reverse Mapping: ToValidationResult
        // If you have a FluentValidation.Results.ValidationResult and want to map it back to a Result:
        var rawFluentResult = await validator.ValidateAsync(invalidRequest);
        Result mappedFromRaw = rawFluentResult.ToValidationResult();
        Console.WriteLine($"ToValidationResult from raw validation: IsFailure = {mappedFromRaw.IsFailure}");
    }
}




