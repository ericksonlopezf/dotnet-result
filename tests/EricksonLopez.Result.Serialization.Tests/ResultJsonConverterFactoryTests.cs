// Copyright © Erickson Lopez. MIT License.
#pragma warning disable CS0619 // Intentionally testing the reflection-based constructor (Obsolete error:true)
#pragma warning disable CS0618
using System;
using System.Text.Json;
using AwesomeAssertions;
using EricksonLopez.Result;
using EricksonLopez.Result.Serialization;
using EricksonLopez.Result.Testing;
using Xunit;

namespace EricksonLopez.Result.Serialization.Tests;

public class ResultJsonConverterFactoryTests
{
    private static readonly JsonSerializerOptions Options = new JsonSerializerOptions
    {
        Converters =
        {
            new ResultJsonConverter(),
            new ResultOfTJsonConverter<int>(),
            new ResultOfTJsonConverter<string>(),
            new ErrorJsonConverter()
        }
    };

    [Fact]
    public void Result_WhenNotStartObject_ThrowsJsonException()
    {
        var json = "\"Invalid\"";
        Action action = () => JsonSerializer.Deserialize<Result>(json, Options);
        var ex = action.Should().Throw<JsonException>().Which;
        Assert.Contains("Expected StartObject", ex.Message);
    }

    [Fact]
    public void Result_WhenMissingIsSuccess_ThrowsJsonException()
    {
        var json = "{ \"other\": true }";
        Action action = () => JsonSerializer.Deserialize<Result>(json, Options);
        var ex = action.Should().Throw<JsonException>().Which;
        Assert.Contains("Missing required property 'isSuccess'", ex.Message);
    }

    [Fact]
    public void ResultOfT_WhenNotStartObject_ThrowsJsonException()
    {
        var json = "\"Invalid\"";
        Action action = () => JsonSerializer.Deserialize<Result<int>>(json, Options);
        var ex = action.Should().Throw<JsonException>().Which;
        Assert.Contains("Expected StartObject", ex.Message);
    }

    [Fact]
    public void ResultOfT_WhenMissingIsSuccess_ThrowsJsonException()
    {
        var json = "{ \"other\": true }";
        Action action = () => JsonSerializer.Deserialize<Result<int>>(json, Options);
        var ex = action.Should().Throw<JsonException>().Which;
        Assert.Contains("Missing required property 'isSuccess'", ex.Message);
    }

    [Fact]
    public void ResultOfT_WhenSuccessButMissingValue_ThrowsJsonException()
    {
        var json = "{ \"isSuccess\": true }";
        Action action = () => JsonSerializer.Deserialize<Result<int>>(json, Options);
        var ex = action.Should().Throw<JsonException>().Which;
        Assert.Contains("Missing required property 'value'", ex.Message);
    }
    [Fact]
    public void Result_WhenIsFailureWithoutIsSuccess_UsesIsFailure()
    {
        var json = "{ \"isFailure\": true, \"error\": { \"code\": \"C\", \"description\": \"D\" } }";
        var result = JsonSerializer.Deserialize<Result>(json, Options)!;
        result.ShouldBeFailure();
        result.Error!.Code.Should().Be("C");
    }

    [Fact]
    public void ResultOfT_WhenIsFailureWithoutIsSuccess_UsesIsFailure()
    {
        var json = "{ \"isFailure\": true, \"error\": { \"code\": \"C\", \"description\": \"D\" } }";
        var result = JsonSerializer.Deserialize<Result<int>>(json, Options)!;
        result.ShouldBeFailure();
        result.Error!.Code.Should().Be("C");
    }

    [Fact]
    public void ErrorMetadata_WhenTrue_ParsesTrueValue()
    {
        var json = "{ \"code\": \"C\", \"description\": \"D\", \"metadata\": { \"k1\": true, \"k2\": false } }";
        var error = JsonSerializer.Deserialize<Error>(json, Options)!;
        Assert.True((bool)error.Metadata["k1"]);
        Assert.False((bool)error.Metadata["k2"]);
    }

}



