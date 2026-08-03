#pragma warning disable CS0619 // Tests intentionally exercise the reflection-based constructor (Obsolete error:true)
#pragma warning disable CS0618 // And the warning-level obsolete constructor
using System;
using System.Text.Json;
using EricksonLopez.Result;
using EricksonLopez.Result.Serialization;
using EricksonLopez.Result.Testing;
using Xunit;

namespace EricksonLopez.Result.Serialization.Tests;

public class SerializationTests
{
    // Uses direct converter registration — the recommended AOT-compatible approach.
    // See ResultJsonConverterFactoryTests for explicit factory coverage.
    private readonly JsonSerializerOptions _options = new()
    {
        Converters =
        {
            new ResultJsonConverter(),
            new ResultOfTJsonConverter<string>(),
            new ResultOfTJsonConverter<int>(),
            new ErrorJsonConverter()
        },
        WriteIndented = false
    };


    [Fact]
    public void Error_Serialization_Roundtrip()
    {
        var childError = Error.Validation("Field.Invalid", "Field must be positive");
        var error = Error.Unavailable("DB.Down", "Database connection timeout")
            .WithTraceId("trace-999")
            .WithCorrelationId("corr-111")
            .WithDescriptionKey("errors.db_down")
            .WithMetadata("retry_after_ms", 5000L);

        error = Error.Custom(
            error.Code,
            error.Description,
            error.Type,
            error.Severity,
            error.Retryability,
            error.DescriptionKey,
            error.TraceId,
            error.CorrelationId,
            [childError],
            error.Metadata);

        var json = JsonSerializer.Serialize(error, _options);
        var deserialized = JsonSerializer.Deserialize<Error>(json, _options);

        Assert.NotNull(deserialized);
        Assert.Equal(error.Code, deserialized.Code);
        Assert.Equal(error.Description, deserialized.Description);
        Assert.Equal(error.Type, deserialized.Type);
        Assert.Equal(error.Retryability, deserialized.Retryability);
        Assert.Equal("errors.db_down", deserialized.DescriptionKey);
        Assert.Equal("trace-999", deserialized.TraceId);
        Assert.Equal("corr-111", deserialized.CorrelationId);
        Assert.True(deserialized.HasInnerErrors);
        Assert.Single(deserialized.InnerErrors);
        Assert.Equal("Field.Invalid", deserialized.InnerErrors[0].Code);
        Assert.True(deserialized.HasMetadata);
        Assert.Equal(5000L, Convert.ToInt64(deserialized.Metadata["retry_after_ms"]));
    }

    [Fact]
    public void Result_Success_Serialization_Roundtrip()
    {
        var result = Result.Success();

        var json = JsonSerializer.Serialize(result, _options);
        Assert.Contains("\"isSuccess\":true", json);

        var deserialized = JsonSerializer.Deserialize<Result>(json, _options);
        deserialized.ShouldBeSuccess();
    }

    [Fact]
    public void ResultOfT_Success_Serialization_Roundtrip()
    {
        var result = Result.Success("Hello World");

        var json = JsonSerializer.Serialize(result, _options);
        Assert.Contains("\"value\":\"Hello World\"", json);

        var deserialized = JsonSerializer.Deserialize<Result<string>>(json, _options);
        Assert.Equal("Hello World", deserialized.ShouldBeSuccess());
    }

    [Fact]
    public void ResultOfT_Failure_Serialization_Roundtrip()
    {
        var result = Result.Failure<int>(Error.NotFound("User.404", "User not found"));

        var json = JsonSerializer.Serialize(result, _options);
        Assert.Contains("\"isFailure\":true", json);

        var deserialized = JsonSerializer.Deserialize<Result<int>>(json, _options);
        deserialized.ShouldHaveErrorCode("User.404");
    }
}

