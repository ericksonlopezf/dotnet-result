using System;
using System.Text.Json;
using EricksonLopez.Result;

namespace EricksonLopez.Result.Serialization.Sample;

public static class SerializationExample
{
    public static void Run()
    {
        Console.WriteLine("\n--- 09. SERIALIZATION ---");

        var options = new JsonSerializerOptions
        {
            WriteIndented = true
        };
        options.Converters.Add(new EricksonLopez.Result.Serialization.ResultJsonConverter());
        options.Converters.Add(new EricksonLopez.Result.Serialization.ErrorJsonConverter());
#pragma warning disable CS0618
        options.Converters.Add(new EricksonLopez.Result.Serialization.ResultOfTJsonConverter<string>());
#pragma warning restore CS0618

        Result<string> myResult = Result.Failure<string>(
            Error.Create("User.Invalid", "User name is invalid.")
                .WithMetadata("Attempt", 1)
                .WithType(ErrorType.Validation)
                .Build()
        );

        string json = JsonSerializer.Serialize(myResult, options);
        Console.WriteLine("Serialized Result<string> (Failure):");
        Console.WriteLine(json);

        var deserialized = JsonSerializer.Deserialize<Result<string>>(json, options);
        Console.WriteLine($"Deserialized IsFailure: {deserialized.IsFailure}, Code: {deserialized.Error?.Code}");
    }
}

