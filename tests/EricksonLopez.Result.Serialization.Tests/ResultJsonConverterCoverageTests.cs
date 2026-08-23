// Copyright © Erickson Lopez. MIT License.
using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using AwesomeAssertions;
using EricksonLopez.Result;
using EricksonLopez.Result.Serialization;
using EricksonLopez.Result.Testing;
using Xunit;

#pragma warning disable CS0619 // Intentionally testing the reflection-based constructor (Obsolete error:true)
#pragma warning disable CS0618
namespace EricksonLopez.Result.Serialization.Tests;

public class ResultJsonConverterCoverageTests
{
    private static readonly JsonSerializerOptions Options = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true,
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
    public void Result_WhenSuccessAndFailure_SerializesCorrectly()
    {
        var successResult = Result.Success();
        var jsonSuccess = JsonSerializer.Serialize(successResult, Options);
        jsonSuccess.Should().Contain("\"isSuccess\":true");
        jsonSuccess.Should().Contain("\"isFailure\":false");

        var failureResult = Result.Failure(Error.NotFound("Not.Found", "Resource not found"));
        var jsonFailure = JsonSerializer.Serialize(failureResult, Options);
        jsonFailure.Should().Contain("\"isSuccess\":false");
        jsonFailure.Should().Contain("\"isFailure\":true");
        jsonFailure.Should().Contain("\"error\"");
        jsonFailure.Should().Contain("Not.Found");
    }

    [Fact]
    public void Result_WhenNullError_ProducesFallbackError()
    {
        var json = "{ \"isSuccess\": false, \"error\": null }";
        var result = JsonSerializer.Deserialize<Result>(json, Options)!;
        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("Serialization.Error");
    }

    [Fact]
    public void Result_WhenWithoutError_ProducesFallbackError()
    {
        var json = "{ \"isSuccess\": false }";
        var result = JsonSerializer.Deserialize<Result>(json, Options)!;
        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("Serialization.Error");
    }

    [Fact]
    public void Result_WhenNestedUnknownObjectAndIsFailure_ParsesCorrectly()
    {
        var json = "{ \"unknownNested\": { \"a\": [1, 2, 3], \"b\": { \"c\": \"d\" } }, \"isSuccess\": true, \"isFailure\": false, \"trailingUnknown\": \"test\" }";
        var result = JsonSerializer.Deserialize<Result>(json, Options)!;
        result.IsSuccess.Should().BeTrue();
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
    public void ResultOfT_WhenNullError_ProducesFallbackError()
    {
        var json = "{ \"isSuccess\": false, \"error\": null }";
        var result = JsonSerializer.Deserialize<Result<int>>(json, Options)!;
        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("Serialization.Error");
    }

    [Fact]
    public void ResultOfT_WhenWithoutError_ProducesFallbackError()
    {
        var json = "{ \"isSuccess\": false }";
        var result = JsonSerializer.Deserialize<Result<int>>(json, Options)!;
        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("Serialization.Error");
    }

    [Fact]
    public void ResultOfT_WhenNestedUnknownObjectAndIsFailure_ParsesCorrectly()
    {
        var json = "{ \"unknownNested\": { \"a\": [1, 2, 3], \"b\": { \"c\": \"d\" } }, \"isSuccess\": true, \"isFailure\": false, \"value\": 42, \"trailing\": 99 }";
        var result = JsonSerializer.Deserialize<Result<int>>(json, Options)!;
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(42);
    }

    [Fact]
    public void ResultOfTConstructor_WhenNullTypeInfo_ThrowsArgumentNullException()
    {
        Action action = () => _ = new ResultOfTJsonConverter<int>(null!);
        action.Should().Throw<ArgumentNullException>();
    }

    private sealed class SentinelAotType
    {
        public string Marker { get; set; } = string.Empty;
    }

    private sealed class SentinelConverter : JsonConverter<SentinelAotType>
    {
        public override SentinelAotType Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            => new SentinelAotType { Marker = "FROM_TYPE_INFO:" + reader.GetString() };

        public override void Write(Utf8JsonWriter writer, SentinelAotType value, JsonSerializerOptions options)
            => writer.WriteStringValue(value.Marker);
    }

    [Fact]
    public void ResultOfTTypeInfoConstructor_WhenUsed_SerializesAndDeserializes()
    {
        var typeInfoOptions = new JsonSerializerOptions
        {
            TypeInfoResolver = new System.Text.Json.Serialization.Metadata.DefaultJsonTypeInfoResolver()
        };
        var customTypeInfo = System.Text.Json.Serialization.Metadata.JsonMetadataServices.CreateValueInfo<SentinelAotType>(
            typeInfoOptions,
            new SentinelConverter());

        var options = new JsonSerializerOptions
        {
            Converters =
            {
                new ResultOfTJsonConverter<SentinelAotType>(customTypeInfo),
                new ErrorJsonConverter()
            }
        };

        var value = new SentinelAotType { Marker = "hello" };
        var success = Result.Success(value);
        var jsonSuccess = JsonSerializer.Serialize(success, options);
        jsonSuccess.Should().Contain("\"value\":\"hello\"");

        var deserializedSuccess = JsonSerializer.Deserialize<Result<SentinelAotType>>(jsonSuccess, options)!;
        deserializedSuccess.IsSuccess.Should().BeTrue();
        deserializedSuccess.Value.Marker.Should().Be("FROM_TYPE_INFO:hello");

        var failure = Result.Failure<SentinelAotType>(Error.Validation("V.1", "Validation error"));
        var jsonFailure = JsonSerializer.Serialize(failure, options);
        jsonFailure.Should().Contain("\"isFailure\":true");

        var deserializedFailure = JsonSerializer.Deserialize<Result<SentinelAotType>>(jsonFailure, options)!;
        deserializedFailure.IsFailure.Should().BeTrue();
        deserializedFailure.Error!.Code.Should().Be("V.1");
    }

    [Fact]
    public void Result_WhenCommentsPresent_ParsesCorrectly()
    {
        var json = "{ /* comment */ \"isSuccess\": true }"u8;
        var readerOptions = new JsonReaderOptions { CommentHandling = JsonCommentHandling.Skip };
        var reader = new Utf8JsonReader(json, readerOptions);
        reader.Read();

        var converter = new ResultJsonConverter();
        var result = converter.Read(ref reader, typeof(Result), Options);
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Result_WhenDeserializing_StopsAtEndObject()
    {
        var json = "[{\"isSuccess\": true}, {\"isSuccess\": false}]"u8;
        var reader = new Utf8JsonReader(json);
        reader.Read(); // StartArray
        reader.Read(); // StartObject (item 1)

        var converter = new ResultJsonConverter();
        var res1 = converter.Read(ref reader, typeof(Result), Options);
        res1.IsSuccess.Should().BeTrue();
        reader.TokenType.Should().Be(JsonTokenType.EndObject);
    }

    [Fact]
    public void ResultOfT_WhenDeserializing_StopsAtEndObject()
    {
        var json = "[{\"isSuccess\": true, \"value\": 10}, {\"isSuccess\": false}]"u8;
        var reader = new Utf8JsonReader(json);
        reader.Read(); // StartArray
        reader.Read(); // StartObject (item 1)

        var converter = new ResultOfTJsonConverter<int>();
        var res1 = converter.Read(ref reader, typeof(Result<int>), Options);
        res1.IsSuccess.Should().BeTrue();
        res1.Value.Should().Be(10);
        reader.TokenType.Should().Be(JsonTokenType.EndObject);
    }
}


