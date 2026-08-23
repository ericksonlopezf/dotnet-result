// Copyright © Erickson Lopez. MIT License.
using System;
using System.Linq;
using System.Text.Json;
using System.Threading;
using AwesomeAssertions;
using EricksonLopez.Result;
using EricksonLopez.Result.Serialization;
using EricksonLopez.Result.Testing;
using Xunit;

#pragma warning disable CS0619 // Tests intentionally exercise the reflection-based constructor (Obsolete error:true)
#pragma warning disable CS0618 // And the warning-level obsolete constructor
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
    public void Error_WhenSerialized_RoundTripsCorrectly()
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

        deserialized.Should().NotBeNull();
        deserialized.Code.Should().Be(error.Code);
        deserialized.Description.Should().Be(error.Description);
        deserialized.Type.Should().Be(error.Type);
        deserialized.Retryability.Should().Be(error.Retryability);
        deserialized.DescriptionKey.Should().Be("errors.db_down");
        deserialized.TraceId.Should().Be("trace-999");
        deserialized.CorrelationId.Should().Be("corr-111");
        deserialized.HasInnerErrors.Should().BeTrue();
        Assert.Single(deserialized.InnerErrors);
        deserialized.InnerErrors[0].Code.Should().Be("Field.Invalid");
        deserialized.HasMetadata.Should().BeTrue();
        Assert.Equal(5000L, Convert.ToInt64(deserialized.Metadata["retry_after_ms"]));
    }

    [Fact]
    public void ResultSuccess_WhenSerialized_RoundTripsCorrectly()
    {
        var result = Result.Success();

        var json = JsonSerializer.Serialize(result, _options);
        Assert.Contains("\"isSuccess\":true", json);

        var deserialized = JsonSerializer.Deserialize<Result>(json, _options);
        deserialized.ShouldBeSuccess();
    }

    [Fact]
    public void ResultOfTSuccess_WhenSerialized_RoundTripsCorrectly()
    {
        var result = Result.Success("Hello World");

        var json = JsonSerializer.Serialize(result, _options);
        Assert.Contains("\"value\":\"Hello World\"", json);

        var deserialized = JsonSerializer.Deserialize<Result<string>>(json, _options);
        Assert.Equal("Hello World", deserialized.ShouldBeSuccess());
    }

    [Fact]
    public void ResultOfTFailure_WhenSerialized_RoundTripsCorrectly()
    {
        var result = Result.Failure<int>(Error.NotFound("User.404", "User not found"));

        var json = JsonSerializer.Serialize(result, _options);
        Assert.Contains("\"isFailure\":true", json);

        var deserialized = JsonSerializer.Deserialize<Result<int>>(json, _options);
        deserialized.ShouldHaveErrorCode("User.404");
    }
}



