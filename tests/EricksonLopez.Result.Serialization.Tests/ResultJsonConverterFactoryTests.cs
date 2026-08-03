#pragma warning disable CS0619 // Intentionally testing the reflection-based constructor (Obsolete error:true)
#pragma warning disable CS0618
using System;
using System.Text.Json;
using Xunit;
using EricksonLopez.Result.Serialization;

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
    public void Factory_CanConvert_ReturnsTrueForResult()
    {
        // Verify converter correctly identifies its supported type
        var converter = new ResultJsonConverter();
        Assert.True(converter.CanConvert(typeof(Result)));
        Assert.False(converter.CanConvert(typeof(Result<int>)));
    }

    [Fact]
    public void Factory_CanConvert_ReturnsTrueForResultOfT()
    {
        var intConverter = new ResultOfTJsonConverter<int>();
        Assert.True(intConverter.CanConvert(typeof(Result<int>)));
        Assert.False(intConverter.CanConvert(typeof(Result)));

        var strConverter = new ResultOfTJsonConverter<string>();
        Assert.True(strConverter.CanConvert(typeof(Result<string>)));
    }

    [Fact]
    public void Factory_CanConvert_ReturnsFalseForOtherTypes()
    {
        var converter = new ResultJsonConverter();
        Assert.False(converter.CanConvert(typeof(Error)));
        Assert.False(converter.CanConvert(typeof(int)));
        Assert.False(converter.CanConvert(typeof(string)));
    }


    [Fact]
    public void Result_Success_RoundTrips()
    {
        var result = Result.Success();
        var json = JsonSerializer.Serialize(result, Options);
        var deserialized = JsonSerializer.Deserialize<Result>(json, Options);

        Assert.True(deserialized.IsSuccess);
    }

    [Fact]
    public void Result_Failure_RoundTrips()
    {
        var error = Error.Failure("A", "B");
        var result = Result.Failure(error);
        var json = JsonSerializer.Serialize(result, Options);
        var deserialized = JsonSerializer.Deserialize<Result>(json, Options);

        Assert.True(deserialized.IsFailure);
        Assert.Equal("A", deserialized.Error.Code);
        Assert.Equal("B", deserialized.Error.Description);
    }

    [Fact]
    public void Result_Read_MissingError_OnFailure_ReturnsSerializationError()
    {
        // JSON that says it's a failure but lacks the "error" property
        var json = "{\"isSuccess\":false,\"isFailure\":true}";
        var deserialized = JsonSerializer.Deserialize<Result>(json, Options);

        Assert.True(deserialized.IsFailure);
        Assert.Equal("Serialization.Error", deserialized.Error.Code);
        Assert.Contains("Invalid Result JSON structure: failure without error.", deserialized.Error.Description);
    }

    [Fact]
    public void Result_Read_NullErrorElement_ReturnsSerializationError()
    {
        // JSON that says it's a failure and has null error
        var json = "{\"isSuccess\":false,\"isFailure\":true,\"error\":null}";
        var deserialized = JsonSerializer.Deserialize<Result>(json, Options);

        Assert.True(deserialized.IsFailure);
        Assert.Equal("Serialization.Error", deserialized.Error.Code);
        Assert.Contains("Invalid Result JSON structure: failure without error.", deserialized.Error.Description);
    }


    [Fact]
    public void ResultOfT_Success_RoundTrips()
    {
        var result = Result<int>.Success(42);
        var json = JsonSerializer.Serialize(result, Options);
        var deserialized = JsonSerializer.Deserialize<Result<int>>(json, Options);

        Assert.True(deserialized.IsSuccess);
        Assert.Equal(42, deserialized.Value);
    }

    [Fact]
    public void ResultOfT_Success_ComplexType_RoundTrips()
    {
        var localOptions = new JsonSerializerOptions
        {
            Converters =
            {
                new ResultJsonConverter(),
                new ResultOfTJsonConverter<TestDto>(),
                new ErrorJsonConverter()
            }
        };
        var result = Result<TestDto>.Success(new TestDto { Name = "Test" });
        var json = JsonSerializer.Serialize(result, localOptions);
        var deserialized = JsonSerializer.Deserialize<Result<TestDto>>(json, localOptions);

        Assert.True(deserialized.IsSuccess);
        Assert.Equal("Test", deserialized.Value.Name);
    }

    [Fact]
    public void ResultOfT_Failure_RoundTrips()
    {
        var error = Error.Failure("A", "B");
        var result = Result<string>.Failure(error);
        var json = JsonSerializer.Serialize(result, Options);
        var deserialized = JsonSerializer.Deserialize<Result<string>>(json, Options);

        Assert.True(deserialized.IsFailure);
        Assert.Equal("A", deserialized.Error.Code);
    }

    [Fact]
    public void ResultOfT_Read_MissingError_OnFailure_ReturnsSerializationError()
    {
        // JSON that says it's a failure but lacks the "error" property
        var json = "{\"isSuccess\":false,\"isFailure\":true}";
        var deserialized = JsonSerializer.Deserialize<Result<int>>(json, Options);

        Assert.True(deserialized.IsFailure);
        Assert.Equal("Serialization.Error", deserialized.Error.Code);
        Assert.Contains("Invalid Result<T> JSON structure: failure without error.", deserialized.Error.Description);
    }

    [Fact]
    public void ResultOfT_Read_NullErrorElement_ReturnsSerializationError()
    {
        // JSON that says it's a failure and has null error
        var json = "{\"isSuccess\":false,\"isFailure\":true,\"error\":null}";
        var deserialized = JsonSerializer.Deserialize<Result<int>>(json, Options);

        Assert.True(deserialized.IsFailure);
        Assert.Equal("Serialization.Error", deserialized.Error.Code);
        Assert.Contains("Invalid Result<T> JSON structure: failure without error.", deserialized.Error.Description);
    }

    private class TestDto
    {
        public string Name { get; set; } = string.Empty;
    }
}

