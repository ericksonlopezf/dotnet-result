#pragma warning disable CS0619 // Intentionally testing the reflection-based constructor (Obsolete error:true)
#pragma warning disable CS0618
using System.Text.Json;
using Xunit;
using EricksonLopez.Result;
using EricksonLopez.Result.Serialization;

namespace EricksonLopez.Result.Tests.Serialization;

// Uses direct converter registration — the recommended AOT-compatible approach.
// Factory-specific tests live in ResultJsonConverterFactoryTests.
public class ResultJsonConverterCoverageTests
{
    private static readonly JsonSerializerOptions Options = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true,
        Converters =
        {
            new ResultJsonConverter(),
            new ResultOfTJsonConverter<int>(),
            new ErrorJsonConverter()
        }
    };

    [Fact]
    public void Result_Throws_WhenNotStartObject()
    {
        var json = "\"Invalid\"";
        var ex = Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<Result>(json, Options));
        Assert.Contains("Expected StartObject", ex.Message);
    }

    [Fact]
    public void Result_Throws_WhenMissingIsSuccess()
    {
        var json = "{ \"other\": true }";
        var ex = Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<Result>(json, Options));
        Assert.Contains("Missing required property 'isSuccess'", ex.Message);
    }

    [Fact]
    public void ResultOfT_Throws_WhenNotStartObject()
    {
        var json = "\"Invalid\"";
        var ex = Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<Result<int>>(json, Options));
        Assert.Contains("Expected StartObject", ex.Message);
    }

    [Fact]
    public void ResultOfT_Throws_WhenMissingIsSuccess()
    {
        var json = "{ \"other\": true }";
        var ex = Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<Result<int>>(json, Options));
        Assert.Contains("Missing required property 'isSuccess'", ex.Message);
    }

    [Fact]
    public void ResultOfT_Throws_WhenSuccessButMissingValue()
    {
        var json = "{ \"isSuccess\": true }";
        var ex = Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<Result<int>>(json, Options));
        Assert.Contains("Missing required property 'value'", ex.Message);
    }
    [Fact]
    public void Result_IsFailure_WithoutIsSuccess_UsesIsFailure()
    {
        var json = "{ \"isFailure\": true, \"error\": { \"code\": \"C\", \"description\": \"D\" } }";
        var result = JsonSerializer.Deserialize<Result>(json, Options)!;
        Assert.True(result.IsFailure);
        Assert.Equal("C", result.Error!.Code);
    }

    [Fact]
    public void ResultOfT_IsFailure_WithoutIsSuccess_UsesIsFailure()
    {
        var json = "{ \"isFailure\": true, \"error\": { \"code\": \"C\", \"description\": \"D\" } }";
        var result = JsonSerializer.Deserialize<Result<int>>(json, Options)!;
        Assert.True(result.IsFailure);
        Assert.Equal("C", result.Error!.Code);
    }

    [Fact]
    public void Error_Metadata_WithTrue_ParsesTrueValue()
    {
        var json = "{ \"code\": \"C\", \"description\": \"D\", \"metadata\": { \"k1\": true, \"k2\": false } }";
        var error = JsonSerializer.Deserialize<Error>(json, Options)!;
        Assert.True((bool)error.Metadata["k1"]);
        Assert.False((bool)error.Metadata["k2"]);
    }
}
