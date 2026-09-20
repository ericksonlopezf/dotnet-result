// Copyright © Erickson Lopez. MIT License.
using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using EricksonLopez.Result;
using EricksonLopez.Result.Serialization;

namespace EricksonLopez.Result.Sample.Examples;

public record ProductDto(int Id, string Sku, decimal Price);

public static class SerializationShowcase
{
    public static void Run()
    {
        Console.WriteLine("\n========================================================");
        Console.WriteLine(" 22. SYSTEM.TEXT.JSON SERIALIZATION & ROUND-TRIP");
        Console.WriteLine("========================================================");

        var options = new JsonSerializerOptions
        {
            WriteIndented = false,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        // Register official converters from EricksonLopez.Result.Serialization
        options.Converters.Add(new ResultJsonConverter());
        options.Converters.Add(new ErrorJsonConverter());
        options.Converters.Add(new ResultOfTJsonConverter<ProductDto>());

        // 1. Serialize and Deserialize Success Result<T>
        Console.WriteLine("\n[1] Round-trip Result<ProductDto> (Success):");
        var product = new ProductDto(42, "PRD-998", 149.99m);
        Result<ProductDto> originalSuccess = Result.Success(product);

        string jsonSuccess = JsonSerializer.Serialize(originalSuccess, options);
        Console.WriteLine($"  JSON: {jsonSuccess}");

        Result<ProductDto> deserializedSuccess = JsonSerializer.Deserialize<Result<ProductDto>>(jsonSuccess, options);
        Console.WriteLine($"  Deserialized IsSuccess={deserializedSuccess.IsSuccess}, Sku={deserializedSuccess.GetValueOrDefault(new ProductDto(0, "", 0)).Sku}");

        // 2. Serialize and Deserialize Failure Result<T> with rich Error
        Console.WriteLine("\n[2] Round-trip Result<ProductDto> (Failure with rich Error):");
        Error richError = Error.Create("Product.NotFound", "The requested product does not exist.")
            .WithType(ErrorType.NotFound)
            .WithSeverity(ErrorSeverity.Warning)
            .WithRetryability(ErrorRetryability.NotApplicable)
            .WithMetadata("RequestedId", 42)
            .Build();

        Result<ProductDto> originalFailure = Result.Failure<ProductDto>(richError);
        string jsonFailure = JsonSerializer.Serialize(originalFailure, options);
        Console.WriteLine($"  JSON: {jsonFailure}");

        Result<ProductDto> deserializedFailure = JsonSerializer.Deserialize<Result<ProductDto>>(jsonFailure, options);
        Console.WriteLine($"  Deserialized IsFailure={deserializedFailure.IsFailure}, Code={deserializedFailure.Error.Code}, Type={deserializedFailure.Error.Type}");

        // 3. Serialize and Deserialize non-generic Result
        Console.WriteLine("\n[3] Non-generic Result serialization:");
        Result opSuccess = Result.Success();
        string jsonOpSuccess = JsonSerializer.Serialize(opSuccess, options);
        Console.WriteLine($"  Result.Success JSON: {jsonOpSuccess}");

        Result opFailure = Result.Failure(Error.Unauthorized("Auth.ExpiredToken", "Security token has expired."));
        string jsonOpFailure = JsonSerializer.Serialize(opFailure, options);
        Console.WriteLine($"  Result.Failure JSON: {jsonOpFailure}");
    }
}
