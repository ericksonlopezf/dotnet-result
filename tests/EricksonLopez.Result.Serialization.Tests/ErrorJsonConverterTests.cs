#pragma warning disable CA1869
using System;
using System.Text.Json;
using AwesomeAssertions;
using Xunit;
using EricksonLopez.Result.Serialization;

namespace EricksonLopez.Result.Serialization.Tests;

public class ErrorJsonConverterTests
{
    private static readonly JsonSerializerOptions Options = new JsonSerializerOptions
    {
        Converters = { new ErrorJsonConverter() }
    };

    [Fact]
    public void Read_ThrowsJsonException_WhenNotStartObject()
    {
        var json = "\"just a string\"";
        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<Error>(json, Options));
    }

    [Fact]
    public void Read_ThrowsJsonException_WhenMissingCodeOrDescription()
    {
        var jsonMissingCode = "{\"description\":\"desc\"}";
        var jsonMissingDesc = "{\"code\":\"code\"}";

        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<Error>(jsonMissingCode, Options));
        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<Error>(jsonMissingDesc, Options));
    }

    [Fact]
    public void RoundTrip_BasicError_WorksCorrectly()
    {
        var error = Error.Failure("CODE", "DESC");
        var json = JsonSerializer.Serialize(error, Options);
        var deserialized = JsonSerializer.Deserialize<Error>(json, Options);

        Assert.NotNull(deserialized);
        Assert.Equal("CODE", deserialized.Code);
        Assert.Equal("DESC", deserialized.Description);
        Assert.Equal(ErrorType.Failure, deserialized.Type);
        Assert.Equal(ErrorSeverity.Error, deserialized.Severity);
        Assert.Equal(ErrorRetryability.NotApplicable, deserialized.Retryability);
        Assert.Null(deserialized.TraceId);
        Assert.Null(deserialized.CorrelationId);
        Assert.False(deserialized.HasInnerErrors);
        Assert.False(deserialized.HasMetadata);
    }

    [Fact]
    public void RoundTrip_ComplexError_WorksCorrectly()
    {
        var error = Error.Create("complex.code", "complex.desc")
            .WithType(ErrorType.Validation)
            .WithSeverity(ErrorSeverity.Warning)
            .WithRetryability(ErrorRetryability.Transient)
            .WithDescriptionKey("key")
            .WithTraceId("trace-123")
            .WithCorrelationId("corr-456")
            .WithInnerError(Error.Validation("inner", "inner desc"))
            .WithMetadata("stringKey", "str")
            .WithMetadata("numKey", 42)
            .WithMetadata("boolKey", true)
            .Build();

        var json = JsonSerializer.Serialize(error, Options);
        var deserialized = JsonSerializer.Deserialize<Error>(json, Options);

        Assert.NotNull(deserialized);
        Assert.Equal("complex.code", deserialized.Code);
        Assert.Equal("complex.desc", deserialized.Description);
        Assert.Equal(ErrorType.Validation, deserialized.Type);
        Assert.Equal(ErrorSeverity.Warning, deserialized.Severity);
        Assert.Equal(ErrorRetryability.Transient, deserialized.Retryability);
        Assert.Equal("key", deserialized.DescriptionKey);
        Assert.Equal("trace-123", deserialized.TraceId);
        Assert.Equal("corr-456", deserialized.CorrelationId);
        
        Assert.True(deserialized.HasInnerErrors);
        var inner = Assert.Single(deserialized.InnerErrors);
        Assert.Equal("inner", inner.Code);

        Assert.True(deserialized.HasMetadata);
        Assert.Equal("str", deserialized.Metadata["stringKey"]);
        Assert.Equal(42L, deserialized.Metadata["numKey"]); // Written as JSON number, parsed as long
        Assert.Equal(true, deserialized.Metadata["boolKey"]); // Written as JSON boolean, parsed as bool
    }

    [Fact]
    public void Read_ParsesEnums_CaseInsensitive()
    {
        var json = @"{
            ""code"": ""A"", ""description"": ""B"",
            ""type"": ""nOtfOuNd"", ""severity"": ""cRiTiCaL"", ""retryability"": ""pErMaNeNt""
        }";
        
        var error = JsonSerializer.Deserialize<Error>(json, Options);
        
        Assert.NotNull(error);
        Assert.Equal(ErrorType.NotFound, error.Type);
        Assert.Equal(ErrorSeverity.Critical, error.Severity);
        Assert.Equal(ErrorRetryability.Permanent, error.Retryability);
    }

    [Fact]
    public void Read_ParsesEnums_FallbackToDefaultWhenUnknown()
    {
        var json = @"{
            ""code"": ""A"", ""description"": ""B"",
            ""type"": ""UNKNOWN_TYPE"", ""severity"": ""UNKNOWN_SEV"", ""retryability"": ""UNKNOWN_RETRY""
        }";
        
        var error = JsonSerializer.Deserialize<Error>(json, Options);
        
        Assert.NotNull(error);
        Assert.Equal(ErrorType.Failure, error.Type);
        Assert.Equal(ErrorSeverity.Error, error.Severity);
        Assert.Equal(ErrorRetryability.NotApplicable, error.Retryability);
    }

    [Fact]
    public void Read_SkipsUnknownProperties()
    {
        var json = @"{
            ""code"": ""A"", ""description"": ""B"", ""unknown"": 123
        }";
        
        var error = JsonSerializer.Deserialize<Error>(json, Options);
        Assert.NotNull(error);
        Assert.Equal("A", error.Code);
    }

    [Theory]
    [InlineData("conflict", ErrorType.Conflict)]
    [InlineData("unauthorized", ErrorType.Unauthorized)]
    [InlineData("forbidden", ErrorType.Forbidden)]
    [InlineData("unavailable", ErrorType.Unavailable)]
    [InlineData("unexpected", ErrorType.Unexpected)]
    [InlineData("domain", ErrorType.Domain)]
    [InlineData("infrastructure", ErrorType.Infrastructure)]
    [InlineData("custom", ErrorType.Custom)]
    public void Read_ParsesAllErrorTypes(string typeString, ErrorType expected)
    {
        var json = $"{{\"code\":\"A\", \"description\":\"B\", \"type\":\"{typeString}\"}}";
        var error = JsonSerializer.Deserialize<Error>(json, Options);
        Assert.Equal(expected, error!.Type);
    }

    [Theory]
    [InlineData("info", ErrorSeverity.Info)]
    public void Read_ParsesAllErrorSeverities(string sevString, ErrorSeverity expected)
    {
        var json = $"{{\"code\":\"A\", \"description\":\"B\", \"severity\":\"{sevString}\"}}";
        var error = JsonSerializer.Deserialize<Error>(json, Options);
        Assert.Equal(expected, error!.Severity);
    }
    
    [Fact]
    public void Read_ParsesMetadata_HandlesObjects()
    {
        var json = @"{
            ""code"": ""A"", ""description"": ""B"",
            ""metadata"": {
                ""objKey"": { ""prop"": ""val"" },
                ""floatKey"": 3.14
            }
        }";
        var error = JsonSerializer.Deserialize<Error>(json, Options);
        Assert.Equal(3.14, (double)error!.Metadata["floatKey"], 2);
        // JSON objects in metadata are deserialized as Dictionary<string, object?>, not JsonElement.
        var objValue = Assert.IsType<System.Collections.Generic.Dictionary<string, object?>>(error.Metadata["objKey"]);
        Assert.Equal("val", objValue["prop"]);
    }

    [Theory]
    [InlineData(ErrorType.NotFound, "NotFound")]
    [InlineData(ErrorType.Conflict, "Conflict")]
    [InlineData(ErrorType.Unauthorized, "Unauthorized")]
    [InlineData(ErrorType.Forbidden, "Forbidden")]
    [InlineData(ErrorType.Unavailable, "Unavailable")]
    [InlineData(ErrorType.Unexpected, "Unexpected")]
    [InlineData(ErrorType.Domain, "Domain")]
    [InlineData(ErrorType.Infrastructure, "Infrastructure")]
    [InlineData(ErrorType.Custom, "Custom")]
    [InlineData(ErrorType.Failure, "Failure")]
    [InlineData(ErrorType.Validation, "Validation")]
    
    public void Write_SerializesAllErrorTypes(ErrorType type, string expectedTypeString)
    {
        var error = Error.Create("A", "B").WithType(type).Build();
        var json = JsonSerializer.Serialize(error, Options);
        Assert.Contains("\"type\":\"" + expectedTypeString + "\"", json);
    }

    [Theory]
    [InlineData(ErrorSeverity.Info, "Info")]
    [InlineData(ErrorSeverity.Warning, "Warning")]
    [InlineData(ErrorSeverity.Error, "Error")]
    [InlineData(ErrorSeverity.Critical, "Critical")]
    
    public void Write_SerializesAllErrorSeverities(ErrorSeverity severity, string expectedSeverityString)
    {
        var error = Error.Create("A", "B").WithSeverity(severity).Build();
        var json = JsonSerializer.Serialize(error, Options);
        Assert.Contains("\"severity\":\"" + expectedSeverityString + "\"", json);
    }

    [Theory]
    [InlineData(ErrorRetryability.NotApplicable, "NotApplicable")]
    [InlineData(ErrorRetryability.Transient, "Transient")]
    [InlineData(ErrorRetryability.Permanent, "Permanent")]
    
    public void Write_SerializesAllErrorRetryabilities(ErrorRetryability retry, string expectedRetryString)
    {
        var error = Error.Create("A", "B").WithRetryability(retry).Build();
        var json = JsonSerializer.Serialize(error, Options);
        Assert.Contains("\"retryability\":\"" + expectedRetryString + "\"", json);
    }

    [Fact]
    public void Read_ParsesMetadata_HandlesFalseBoolean()
    {
        var json = @"{ ""code"": ""A"", ""description"": ""B"", ""metadata"": { ""boolKey"": false } }";
        var error = JsonSerializer.Deserialize<Error>(json, Options);
        Assert.Equal(false, error!.Metadata["boolKey"]);
    }

    [Fact]
    public void Write_SerializesAllErrorTypes_Fallback()
    {
        var error = Error.Create("A", "B").WithType((ErrorType)255).Build();
        var json = System.Text.Json.JsonSerializer.Serialize(error, Options);
        Assert.Contains("\"type\":\"Failure\"", json);
    }
    
    [Fact]
    public void Write_SerializesAllErrorSeverities_Fallback()
    {
        var error = Error.Create("A", "B").WithSeverity((ErrorSeverity)255).Build();
        var json = System.Text.Json.JsonSerializer.Serialize(error, Options);
        Assert.Contains("\"severity\":\"Error\"", json);
    }
    
    [Fact]
    public void Write_SerializesAllErrorRetryabilities_Fallback()
    {
        var error = Error.Create("A", "B").WithRetryability((ErrorRetryability)255).Build();
        var json = System.Text.Json.JsonSerializer.Serialize(error, Options);
        Assert.Contains("\"retryability\":\"NotApplicable\"", json);
    }

    [Fact]
    public void Serialization_HandlesAllPrimitiveMetadataTypes()
    {
        var meta = new Dictionary<string, object>
        {
            ["f"] = 1.5f,
            ["m"] = 1.5m,
            ["sh"] = (short)1,
            ["by"] = (byte)2,
            ["ui"] = 3u,
            ["ul"] = 4ul,
            ["us"] = (ushort)5,
            ["sb"] = (sbyte)6,
            ["arr"] = new[] { 1, 2 },
            ["formattable"] = new TestFormattable(),
            ["obj"] = new TestObject()
        };
        var e = Error.Failure("A", "B").WithMetadata(meta);
        var opt = new System.Text.Json.JsonSerializerOptions(); opt.Converters.Add(new EricksonLopez.Result.Serialization.ErrorJsonConverter());
        var json = System.Text.Json.JsonSerializer.Serialize(e, opt);
        json.Should().Contain("\"f\":1.5");
        json.Should().Contain("\"sb\":6");
    }

    private class TestFormattable : IFormattable
    {
        public string ToString(string? format, IFormatProvider? formatProvider) => "formattable";
    }

    private class TestObject
    {
        public override string ToString() => "test_obj";
    }




    private static readonly int[] _arr = new[] { 1, 2 };
    
    private class NullToStringObj {
        public override string? ToString() => null;
    }

    [Fact]
    public void Serialize_MetadataTypes()
    {
        var e = Error.Create("X","X")
            .WithMetadata("n", (object?)null!)
            .WithMetadata("ui", (uint)1)
            .WithMetadata("ul", (ulong)1)
            .WithMetadata("us", (ushort)1)
            .WithMetadata("sb", (sbyte)1)
            .WithMetadata("arr", _arr)
            .WithMetadata("obj", new object())
            .WithMetadata("nullstr", new NullToStringObj())
            .Build();
            
        var opt = new System.Text.Json.JsonSerializerOptions(); opt.Converters.Add(new EricksonLopez.Result.Serialization.ErrorJsonConverter());
        System.Text.Json.JsonSerializer.Serialize(e, opt);
    }

    [Fact]
    public void ErrorJsonConverter_IncompleteArray_Throws()
    {
        var converter = new EricksonLopez.Result.Serialization.ErrorJsonConverter();
        var json = "{\"innerErrors\": ["u8;
        var opt = new System.Text.Json.JsonSerializerOptions();
        try
        {
            var reader = new System.Text.Json.Utf8JsonReader(json);
            reader.Read(); // StartObject
            converter.Read(ref reader, typeof(Error), opt);
            Assert.Fail("Expected JsonException");
        }
        catch (System.Text.Json.JsonException) { }
    }

    [Fact]
    public void ErrorJsonConverter_IncompleteObject_Throws()
    {
        var converter = new EricksonLopez.Result.Serialization.ErrorJsonConverter();
        var json = "{\"metadata\": {"u8;
        var opt = new System.Text.Json.JsonSerializerOptions();
        try
        {
            var reader = new System.Text.Json.Utf8JsonReader(json);
            reader.Read(); // StartObject
            converter.Read(ref reader, typeof(Error), opt);
            Assert.Fail("Expected JsonException");
        }
        catch (System.Text.Json.JsonException) { }
    }

    [Fact]
    public void Read_ParsesMetadata_WithComments()
    {
        var json = @"{ ""code"": ""A"", ""description"": ""B"", ""metadata"": { /* comment */ ""key"": ""val"" } }"u8;
        var readerOptions = new System.Text.Json.JsonReaderOptions
        {
            CommentHandling = System.Text.Json.JsonCommentHandling.Allow
        };
        var reader = new System.Text.Json.Utf8JsonReader(json, readerOptions);
        reader.Read(); // StartObject
        
        var converter = new ErrorJsonConverter();
        var options = new JsonSerializerOptions();
        var error = converter.Read(ref reader, typeof(Error), options);
        
        Assert.NotNull(error);
        Assert.Equal("val", error!.Metadata["key"]);
    }
}



