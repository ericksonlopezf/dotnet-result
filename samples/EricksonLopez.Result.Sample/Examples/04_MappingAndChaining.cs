using System;
using EricksonLopez.Result;

namespace EricksonLopez.Result.Sample.Examples;

public static class MappingAndChaining
{
    public static void Run()
    {
        Console.WriteLine("\n--- 04. MAPPING AND CHAINING (PIPELINES) ---");

        Result<string> initialResult = Result.Success("100");
        
        Error parsingError = Error.Create("Parse.Failed", "Could not parse to integer.").Build();

        // 1. Map (Transform the internal value if Success)
        // Changes Result<string> to Result<int>
        Result<int> mappedResult = initialResult.Map(str => int.Parse(str, System.Globalization.CultureInfo.InvariantCulture));
        Console.WriteLine($"Map: String '100' mapped to int. Result: {mappedResult.GetValueOrDefault(0)}");

        // Map on a failure simply propagates the failure without executing the map function.
        Result<string> failedInitial = Result.Failure<string>(parsingError);
        Result<int> skippedMap = failedInitial.Map(str => int.Parse(str, System.Globalization.CultureInfo.InvariantCulture));
        Console.WriteLine($"Map on failure propagates error: {skippedMap.IsFailure} ({skippedMap.Error?.Code})");

        // 2. Bind (Chain operations that also return a Result)
        // Similar to Map, but the function returns a Result, flattening Result<Result<T>> to Result<T>.
        Result<int> ValidateAndParse(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return Result.Failure<int>(Error.Create("Input.Empty", "Input is empty.").Build());
                
            if (int.TryParse(input, out int parsed))
                return Result.Success(parsed);
                
            return Result.Failure<int>(parsingError);
        }

        Result<int> boundSuccess = initialResult.Bind(str => ValidateAndParse(str));
        Console.WriteLine($"Bind on success returned: {boundSuccess.IsSuccess}");

        Result<int> boundFailure = Result.Success("NotANumber").Bind(str => ValidateAndParse(str));
        Console.WriteLine($"Bind resulting in inner failure: {boundFailure.IsFailure} ({boundFailure.Error?.Code})");

        // 3. Ensure (Validates a condition, failing if not met)
        Error negativeError = Error.Create("Math.Negative", "Value cannot be negative.").Build();
        
        Result<int> validatedResult = mappedResult
            .Ensure(val => val >= 0, negativeError);
            
        Console.WriteLine($"Ensure (positive condition met): {validatedResult.IsSuccess}");

        Result<int> failedValidation = Result.Success(-50)
            .Ensure(val => val >= 0, negativeError);
            
        Console.WriteLine($"Ensure (positive condition failed): {failedValidation.IsFailure} ({failedValidation.Error?.Code})");
    }
}
