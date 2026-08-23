// Copyright © Erickson Lopez. MIT License.
using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using EricksonLopez.Result;
using EricksonLopez.Result.Serialization;

namespace EricksonLopez.Result.Serialization.Sample;

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(UserDto))]
[JsonSerializable(typeof(Result<UserDto>))]
[JsonSerializable(typeof(Error))]
internal sealed partial class SampleJsonContext : JsonSerializerContext
{
}

public record UserDto(int Id, string Name, string Email);

public static class SerializationExample
{
    public static void Run()
    {
        Console.WriteLine("\n========================================================");
        Console.WriteLine(" 09. SYSTEM.TEXT.JSON SERIALIZATION & NATIVE AOT");
        Console.WriteLine("========================================================");

        // -------------------------------------------------------------
        // 1. Standard Reflection-Based Serialization
        // -------------------------------------------------------------
        Console.WriteLine("\n[1] Standard System.Text.Json Custom Converters:");

        var options = new JsonSerializerOptions
        {
            WriteIndented = true
        };
        options.Converters.Add(new ResultJsonConverter());
        options.Converters.Add(new ErrorJsonConverter());
#pragma warning disable CS0618 // Type or member is obsolete
        options.Converters.Add(new ResultOfTJsonConverter<UserDto>());
#pragma warning restore CS0618

        var user = new UserDto(101, "Alice Smith", "alice@example.com");
        Result<UserDto> successResult = Result.Success(user);
        Result<UserDto> failureResult = Result.Failure<UserDto>(
            Error.Create("User.InvalidEmail", "The email format is invalid.")
                .WithType(ErrorType.Validation)
                .WithSeverity(ErrorSeverity.Warning)
                .WithCorrelationId(Guid.NewGuid().ToString())
                .WithMetadata("ProvidedEmail", "invalid-email")
                .Build()
        );

        string successJson = JsonSerializer.Serialize(successResult, options);
        Console.WriteLine("Serialized Success Result<UserDto>:");
        Console.WriteLine(successJson);

        string failureJson = JsonSerializer.Serialize(failureResult, options);
        Console.WriteLine("Serialized Failure Result<UserDto>:");
        Console.WriteLine(failureJson);

        var deserializedSuccess = JsonSerializer.Deserialize<Result<UserDto>>(successJson, options);
        Console.WriteLine($"Deserialized Success: IsSuccess={deserializedSuccess.IsSuccess}, Value={deserializedSuccess.Value.Name}");

        var deserializedFailure = JsonSerializer.Deserialize<Result<UserDto>>(failureJson, options);
        Console.WriteLine($"Deserialized Failure: IsFailure={deserializedFailure.IsFailure}, ErrorCode={deserializedFailure.Error.Code}");

        // -------------------------------------------------------------
        // 2. NativeAOT & Trim-Safe Serialization with JsonTypeInfo<T>
        // -------------------------------------------------------------
        Console.WriteLine("\n[2] NativeAOT Trim-Safe Serialization with JsonTypeInfo<T>:");

        var aotOptions = new JsonSerializerOptions { WriteIndented = true };
        aotOptions.Converters.Add(new ResultJsonConverter());
        aotOptions.Converters.Add(new ErrorJsonConverter());
        // Using the AOT-safe constructor accepting JsonTypeInfo<T>
        aotOptions.Converters.Add(new ResultOfTJsonConverter<UserDto>(SampleJsonContext.Default.UserDto));

        string aotJson = JsonSerializer.Serialize(successResult, aotOptions);
        Console.WriteLine("AOT-Serialized Result<UserDto>:");
        Console.WriteLine(aotJson);

        var aotDeserialized = JsonSerializer.Deserialize<Result<UserDto>>(aotJson, aotOptions);
        Console.WriteLine($"AOT-Deserialized Result: IsSuccess={aotDeserialized.IsSuccess}, Value={aotDeserialized.Value.Email}");

        // -------------------------------------------------------------
        // 3. Standalone Non-Generic Result Serialization
        // -------------------------------------------------------------
        Console.WriteLine("\n[3] Non-Generic Result Serialization:");
        Result nonGenericFailure = Result.Failure(Error.Unauthorized("Auth.ExpiredToken", "Your session token has expired."));
        string nonGenericJson = JsonSerializer.Serialize(nonGenericFailure, options);
        Console.WriteLine("Serialized Result (Failure):");
        Console.WriteLine(nonGenericJson);

        var deserializedNonGeneric = JsonSerializer.Deserialize<Result>(nonGenericJson, options);
        Console.WriteLine($"Deserialized Non-Generic: IsFailure={deserializedNonGeneric.IsFailure}, Code={deserializedNonGeneric.Error.Code}");
    }
}
