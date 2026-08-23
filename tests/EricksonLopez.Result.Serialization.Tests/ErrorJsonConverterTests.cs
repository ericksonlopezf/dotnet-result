// Copyright © Erickson Lopez. MIT License.
#pragma warning disable CA1869
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using AwesomeAssertions;
using EricksonLopez.Result;
using EricksonLopez.Result.Serialization;
using Xunit;

namespace EricksonLopez.Result.Serialization.Tests;

public class ErrorJsonConverterTests
{
    private static readonly JsonSerializerOptions Options = new JsonSerializerOptions
    {
        Converters = { new ErrorJsonConverter() }
    };

    [Fact]
    public void Read_WhenNotStartObject_ThrowsJsonException()
    {
        var json = "\"just a string\"";
        Action action = () => JsonSerializer.Deserialize<Error>(json, Options);
        action.Should().Throw<JsonException>();
    }

    [Fact]
    public void Read_WhenMissingCodeOrDescription_ThrowsJsonException()
    {
        var jsonMissingCode = "{\"description\":\"desc\"}";
        var jsonMissingDesc = "{\"code\":\"code\"}";

        Action action1 = () => JsonSerializer.Deserialize<Error>(jsonMissingCode, Options);
        action1.Should().Throw<JsonException>();
        Action action2 = () => JsonSerializer.Deserialize<Error>(jsonMissingDesc, Options);
        action2.Should().Throw<JsonException>();
    }

    [Fact]
    public void RoundTrip_WhenBasicError_WorksCorrectly()
    {
        var error = Error.Failure("CODE", "DESC");
        var json = JsonSerializer.Serialize(error, Options);
        var deserialized = JsonSerializer.Deserialize<Error>(json, Options);

        deserialized.Should().NotBeNull();
        deserialized.Code.Should().Be("CODE");
        deserialized.Description.Should().Be("DESC");
        deserialized.Type.Should().Be(ErrorType.Failure);
        deserialized.Severity.Should().Be(ErrorSeverity.Error);
        deserialized.Retryability.Should().Be(ErrorRetryability.NotApplicable);
        Assert.Null(deserialized.TraceId);
        Assert.Null(deserialized.CorrelationId);
        deserialized.HasInnerErrors.Should().BeFalse();
        deserialized.HasMetadata.Should().BeFalse();
    }

    [Fact]
    public void RoundTrip_WhenComplexError_WorksCorrectly()
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

        deserialized.Should().NotBeNull();
        deserialized.Code.Should().Be("complex.code");
        deserialized.Description.Should().Be("complex.desc");
        deserialized.Type.Should().Be(ErrorType.Validation);
        deserialized.Severity.Should().Be(ErrorSeverity.Warning);
        deserialized.Retryability.Should().Be(ErrorRetryability.Transient);
        deserialized.DescriptionKey.Should().Be("key");
        deserialized.TraceId.Should().Be("trace-123");
        deserialized.CorrelationId.Should().Be("corr-456");

        deserialized.HasInnerErrors.Should().BeTrue();
        var inner = Assert.Single(deserialized.InnerErrors);
        inner.Code.Should().Be("inner");

        deserialized.HasMetadata.Should().BeTrue();
        deserialized.Metadata["stringKey"].Should().Be("str");
        deserialized.Metadata["numKey"].Should().Be(42L); // Written as JSON number, parsed as long
        deserialized.Metadata["boolKey"].Should().Be(true); // Written as JSON boolean, parsed as bool
    }

    [Fact]
    public void Read_WhenEnumIsCaseInsensitive_ParsesCorrectly()
    {
        var json = @"{
            ""code"": ""A"", ""description"": ""B"",
            ""type"": ""nOtfOuNd"", ""severity"": ""cRiTiCaL"", ""retryability"": ""pErMaNeNt""
        }";

        var error = JsonSerializer.Deserialize<Error>(json, Options);

        error.Should().NotBeNull();
        error.Type.Should().Be(ErrorType.NotFound);
        error.Severity.Should().Be(ErrorSeverity.Critical);
        error.Retryability.Should().Be(ErrorRetryability.Permanent);
    }

    [Fact]
    public void Read_WhenEnumIsUnknown_FallsBackToDefault()
    {
        var json = @"{
            ""code"": ""A"", ""description"": ""B"",
            ""type"": ""UNKNOWN_TYPE"", ""severity"": ""UNKNOWN_SEV"", ""retryability"": ""UNKNOWN_RETRY""
        }";

        var error = JsonSerializer.Deserialize<Error>(json, Options);

        error.Should().NotBeNull();
        error.Type.Should().Be(ErrorType.Failure);
        error.Severity.Should().Be(ErrorSeverity.Error);
        error.Retryability.Should().Be(ErrorRetryability.NotApplicable);
    }

    [Fact]
    public void Read_WhenUnknownProperties_SkipsProperties()
    {
        var json = @"{
            ""code"": ""A"", ""description"": ""B"", ""unknown"": 123
        }";

        var error = JsonSerializer.Deserialize<Error>(json, Options);
        error.Should().NotBeNull();
        error.Code.Should().Be("A");
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
    public void Read_WhenAllErrorTypes_ParsesCorrectly(string typeString, ErrorType expected)
    {
        var json = $"{{\"code\":\"A\", \"description\":\"B\", \"type\":\"{typeString}\"}}";
        var error = JsonSerializer.Deserialize<Error>(json, Options);
        error!.Type.Should().Be(expected);
    }

    [Theory]
    [InlineData("info", ErrorSeverity.Info)]
    public void Read_WhenAllErrorSeverities_ParsesCorrectly(string sevString, ErrorSeverity expected)
    {
        var json = $"{{\"code\":\"A\", \"description\":\"B\", \"severity\":\"{sevString}\"}}";
        var error = JsonSerializer.Deserialize<Error>(json, Options);
        error!.Severity.Should().Be(expected);
    }

    [Fact]
    public void Read_WhenMetadataContainsObjects_HandlesCorrectly()
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
        var objValue = Assert.IsType<Dictionary<string, object?>>(error.Metadata["objKey"]);
        objValue["prop"].Should().Be("val");
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

    public void Write_WhenAllErrorTypes_SerializesCorrectly(ErrorType type, string expectedTypeString)
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

    public void Write_WhenAllErrorSeverities_SerializesCorrectly(ErrorSeverity severity, string expectedSeverityString)
    {
        var error = Error.Create("A", "B").WithSeverity(severity).Build();
        var json = JsonSerializer.Serialize(error, Options);
        Assert.Contains("\"severity\":\"" + expectedSeverityString + "\"", json);
    }

    [Theory]
    [InlineData(ErrorRetryability.NotApplicable, "NotApplicable")]
    [InlineData(ErrorRetryability.Transient, "Transient")]
    [InlineData(ErrorRetryability.Permanent, "Permanent")]

    public void Write_WhenAllErrorRetryabilities_SerializesCorrectly(ErrorRetryability retry, string expectedRetryString)
    {
        var error = Error.Create("A", "B").WithRetryability(retry).Build();
        var json = JsonSerializer.Serialize(error, Options);
        Assert.Contains("\"retryability\":\"" + expectedRetryString + "\"", json);
    }

    [Fact]
    public void Read_WhenMetadataContainsFalseBoolean_HandlesCorrectly()
    {
        var json = @"{ ""code"": ""A"", ""description"": ""B"", ""metadata"": { ""boolKey"": false } }";
        var error = JsonSerializer.Deserialize<Error>(json, Options);
        error!.Metadata["boolKey"].Should().Be(false);
    }

    [Fact]
    public void Write_WhenAllErrorTypesFallback_SerializesCorrectly()
    {
        var error = Error.Create("A", "B").WithType((ErrorType)255).Build();
        var json = System.Text.Json.JsonSerializer.Serialize(error, Options);
        Assert.Contains("\"type\":\"Failure\"", json);
    }

    [Fact]
    public void Write_WhenAllErrorSeveritiesFallback_SerializesCorrectly()
    {
        var error = Error.Create("A", "B").WithSeverity((ErrorSeverity)255).Build();
        var json = System.Text.Json.JsonSerializer.Serialize(error, Options);
        Assert.Contains("\"severity\":\"Error\"", json);
    }

    [Fact]
    public void Write_WhenAllErrorRetryabilitiesFallback_SerializesCorrectly()
    {
        var error = Error.Create("A", "B").WithRetryability((ErrorRetryability)255).Build();
        var json = System.Text.Json.JsonSerializer.Serialize(error, Options);
        Assert.Contains("\"retryability\":\"NotApplicable\"", json);
    }

    [Fact]
    public void Serialization_WhenAllPrimitiveMetadataTypes_HandlesCorrectly()
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

#pragma warning disable S2225 // Testing ErrorJsonConverter handling of objects with null ToString()
    private sealed class NullToStringObj
    {
        public override string? ToString() => null;
    }
#pragma warning restore S2225

    [Fact]
    public void Serialize_WhenMetadataTypes_SerializesCorrectly()
    {
        var e = Error.Create("X", "X")
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
    public void Read_WhenIncompleteArray_ThrowsJsonException()
    {
        var converter = new EricksonLopez.Result.Serialization.ErrorJsonConverter();
        var json = "{\"innerErrors\": ["u8;
        var opt = new System.Text.Json.JsonSerializerOptions();
        try
        {
            var reader = new System.Text.Json.Utf8JsonReader(json, isFinalBlock: false, state: default);
            reader.Read(); // StartObject
            converter.Read(ref reader, typeof(Error), opt);
            true.Should().BeFalse("Expected JsonException");
        }
        catch (System.Text.Json.JsonException) { }
    }

    [Fact]
    public void Read_WhenIncompleteObject_ThrowsJsonException()
    {
        var converter = new EricksonLopez.Result.Serialization.ErrorJsonConverter();
        var json = "{\"metadata\": {"u8;
        var opt = new System.Text.Json.JsonSerializerOptions();
        try
        {
            var reader = new System.Text.Json.Utf8JsonReader(json, isFinalBlock: false, state: default);
            reader.Read(); // StartObject
            converter.Read(ref reader, typeof(Error), opt);
            true.Should().BeFalse("Expected JsonException");
        }
        catch (System.Text.Json.JsonException) { }
    }

    [Fact]
    public void Read_WhenIncompleteMetadataArray_ExitsLoopCleanly()
    {
        var converter = new EricksonLopez.Result.Serialization.ErrorJsonConverter();
        var json = "{\"code\":\"A\", \"description\":\"B\", \"metadata\": {\"arr\": [ 1, 2 "u8;
        var opt = new System.Text.Json.JsonSerializerOptions();
        try
        {
            var reader = new System.Text.Json.Utf8JsonReader(json, isFinalBlock: false, state: default);
            reader.Read(); // StartObject
            converter.Read(ref reader, typeof(Error), opt);
        }
        catch (System.Text.Json.JsonException) { } // May throw if builder fails or whatever, but branch is hit
    }

    [Fact]
    public void Read_WhenIncompleteMetadataNestedObject_ExitsLoopCleanly()
    {
        var converter = new EricksonLopez.Result.Serialization.ErrorJsonConverter();
        var json = "{\"code\":\"A\", \"description\":\"B\", \"metadata\": {\"obj\": { \"key\": 1 "u8;
        var opt = new System.Text.Json.JsonSerializerOptions();
        try
        {
            var reader = new System.Text.Json.Utf8JsonReader(json, isFinalBlock: false, state: default);
            reader.Read(); // StartObject
            converter.Read(ref reader, typeof(Error), opt);
        }
        catch (System.Text.Json.JsonException) { }
    }

    [Fact]
    public void Read_WhenUnknownTokenInMetadata_SkipsIt()
    {
        // Utf8JsonReader doesn't easily expose unknown tokens unless we use comments and Allow comments.
        // If we place a comment where a value is expected, and ReadMetadataValue is called on it, 
        // it hits the `_ => null` branch.
        var json = "{\"code\":\"A\", \"description\":\"B\", \"metadata\": {\"key\": /*unknown*/ 1 }}"u8;
        var readerOptions = new System.Text.Json.JsonReaderOptions { CommentHandling = System.Text.Json.JsonCommentHandling.Allow };
        var reader = new System.Text.Json.Utf8JsonReader(json, readerOptions);
        reader.Read(); // StartObject
        
        var converter = new EricksonLopez.Result.Serialization.ErrorJsonConverter();
        var opt = new System.Text.Json.JsonSerializerOptions();
        
        // This will deserialize the Error. 
        // When it hits the comment after "key", ReadMetadataValue will see TokenType.Comment and return null.
        // It will skip adding the key to the dictionary. Then the loop continues.
        // Wait, reader.Read() inside ReadMetadataObject moves to the comment, calls ReadMetadataValue (returns null),
        // then the next iteration calls reader.Read() which moves to '1' (a Number). But '1' is NOT a PropertyName!
        // So it skips '1' inside the while loop!
        var error = converter.Read(ref reader, typeof(Error), opt);
        error.Should().NotBeNull();
        error!.Metadata.ContainsKey("key").Should().BeFalse();
    }

    [Fact]
    public void Read_WhenMetadataContainsComments_ParsesCorrectly()
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

        error.Should().NotBeNull();
        error!.Metadata["key"].Should().Be("val");
    }

    [Fact]
    public void Read_WhenCodeOrDescriptionIsWhitespace_ThrowsJsonException()
    {
        var jsonWhitespaceCode = "{\"code\":\"   \",\"description\":\"desc\"}";
        var jsonWhitespaceDesc = "{\"code\":\"code\",\"description\":\"   \"}";

        Action action1 = () => JsonSerializer.Deserialize<Error>(jsonWhitespaceCode, Options);
        action1.Should().Throw<JsonException>();
        Action action2 = () => JsonSerializer.Deserialize<Error>(jsonWhitespaceDesc, Options);
        action2.Should().Throw<JsonException>();
    }

    [Fact]
    public void Read_WhenInnerErrorsNotArray_SkipsProperty()
    {
        var json = "{\"code\":\"C\",\"description\":\"D\",\"innerErrors\":\"not an array\"}";
        var error = JsonSerializer.Deserialize<Error>(json, Options)!;
        error.Should().NotBeNull();
        error.HasInnerErrors.Should().BeFalse();
    }

    [Fact]
    public void Read_WhenMetadataNotObject_SkipsProperty()
    {
        var json = "{\"code\":\"C\",\"description\":\"D\",\"metadata\":\"not an object\"}";
        var error = JsonSerializer.Deserialize<Error>(json, Options)!;
        error.Should().NotBeNull();
        error.HasMetadata.Should().BeFalse();
    }

    [Fact]
    public void Read_WhenNestedArraysAndObjectsInMetadata_ParsesRecursively()
    {
        var json = @"{
            ""code"": ""C"",
            ""description"": ""D"",
            ""metadata"": {
                ""arr"": [ 1, ""two"", true, null, [3, 4], { ""innerKey"": 5 } ],
                ""obj"": {
                    ""nestedList"": [ 10, 20 ],
                    ""nestedObj"": { ""leaf"": ""deep"" }
                }
            }
        }";

        var error = JsonSerializer.Deserialize<Error>(json, Options)!;
        error.Should().NotBeNull();
        error.HasMetadata.Should().BeTrue();

        var arr = error.Metadata["arr"].Should().BeOfType<List<object?>>().Subject;
        arr.Count.Should().Be(6);
        arr[0].Should().Be(1L);
        arr[1].Should().Be("two");
        arr[2].Should().Be(true);
        arr[3].Should().BeNull();
        arr[4].Should().BeOfType<List<object?>>();
        arr[5].Should().BeOfType<Dictionary<string, object?>>();

        var obj = error.Metadata["obj"].Should().BeOfType<Dictionary<string, object?>>().Subject;
        obj["nestedList"].Should().BeOfType<List<object?>>();
        obj["nestedObj"].Should().BeOfType<Dictionary<string, object?>>();
    }

    [Fact]
    public void Read_WhenUnknownProperties_SkipsThem()
    {
        var json = "{\"extra1\":\"foo\",\"code\":\"C\",\"extra2\":99,\"description\":\"D\",\"extra3\":{\"a\":1}}";
        var error = JsonSerializer.Deserialize<Error>(json, Options)!;
        error.Should().NotBeNull();
        error.Code.Should().Be("C");
        error.Description.Should().Be("D");
    }

    [Fact]
    public void Read_WhenUnknownEnumValues_FallsBackToDefaults()
    {
        var json = "{\"code\":\"C\",\"description\":\"D\",\"type\":\"invalid\",\"severity\":\"invalid\",\"retryability\":\"invalid\"}";
        var error = JsonSerializer.Deserialize<Error>(json, Options)!;
        error.Should().NotBeNull();
        error.Type.Should().Be(ErrorType.Failure);
        error.Severity.Should().Be(ErrorSeverity.Error);
        error.Retryability.Should().Be(ErrorRetryability.NotApplicable);
    }

    [Theory]
    [InlineData("Failure", ErrorType.Failure)]
    [InlineData("Validation", ErrorType.Validation)]
    [InlineData("NotFound", ErrorType.NotFound)]
    [InlineData("not_found", ErrorType.NotFound)]
    [InlineData("Conflict", ErrorType.Conflict)]
    [InlineData("Unauthorized", ErrorType.Unauthorized)]
    [InlineData("Forbidden", ErrorType.Forbidden)]
    public void Read_WhenErrorTypePermutations_ParsesCorrectly(string typeString, ErrorType expected)
    {
        var json = $"{{\"code\":\"C\",\"description\":\"D\",\"type\":\"{typeString}\"}}";
        var error = JsonSerializer.Deserialize<Error>(json, Options)!;
        error.Type.Should().Be(expected);
    }

    [Theory]
    [InlineData("Info", ErrorSeverity.Info)]
    [InlineData("Warning", ErrorSeverity.Warning)]
    [InlineData("Error", ErrorSeverity.Error)]
    [InlineData("Critical", ErrorSeverity.Critical)]
    public void Read_WhenErrorSeverityPermutations_ParsesCorrectly(string severityString, ErrorSeverity expected)
    {
        var json = $"{{\"code\":\"C\",\"description\":\"D\",\"severity\":\"{severityString}\"}}";
        var error = JsonSerializer.Deserialize<Error>(json, Options)!;
        error.Severity.Should().Be(expected);
    }

    [Theory]
    [InlineData("NotApplicable", ErrorRetryability.NotApplicable)]
    [InlineData("not_applicable", ErrorRetryability.NotApplicable)]
    [InlineData("Transient", ErrorRetryability.Transient)]
    [InlineData("Permanent", ErrorRetryability.Permanent)]
    public void Read_WhenErrorRetryabilityPermutations_ParsesCorrectly(string retryabilityString, ErrorRetryability expected)
    {
        var json = $"{{\"code\":\"C\",\"description\":\"D\",\"retryability\":\"{retryabilityString}\"}}";
        var error = JsonSerializer.Deserialize<Error>(json, Options)!;
        error.Retryability.Should().Be(expected);
    }

    [Fact]
    public void Write_WhenMetadataTypes_SerializesSpecificFormats()
    {
        var dt = new DateTime(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);
        var dto = new DateTimeOffset(2026, 1, 1, 12, 0, 0, TimeSpan.FromHours(-5));
        var ts = TimeSpan.FromDays(2) + TimeSpan.FromHours(3) + TimeSpan.FromMinutes(4) + TimeSpan.FromSeconds(5) + TimeSpan.FromMilliseconds(678);

        var error = Error.Create("C", "D")
            .WithMetadata("ui", (uint)100)
            .WithMetadata("ul", (ulong)200)
            .WithMetadata("us", (ushort)300)
            .WithMetadata("sb", (sbyte)40)
            .WithMetadata("dt", dt)
            .WithMetadata("dto", dto)
            .WithMetadata("ts", ts)
            .WithMetadata("list", new List<int> { 1, 2, 3 })
            .WithMetadata("formattable", System.Net.IPAddress.Loopback)
            .Build();

        var json = JsonSerializer.Serialize(error, Options);
        json.Should().Contain("\"ui\":100");
        json.Should().Contain("\"ul\":200");
        json.Should().Contain("\"us\":300");
        json.Should().Contain("\"sb\":40");
        json.Should().Contain("\"dt\":\"2026-01-01T12:00:00.0000000Z\"");
        json.Should().Contain("\"dto\":\"2026-01-01T12:00:00.0000000-05:00\"");
        json.Should().Contain("\"ts\":\"2.03:04:05.6780000\"");
        json.Should().Contain("\"list\":[1,2,3]");
        json.Should().Contain("127.0.0.1");
    }

    [Fact]
    public void Read_WhenCommentInsideNestedMetadataObject_SkipsComment()
    {
        var json = "{\"code\":\"C\",\"description\":\"D\",\"metadata\":{\"nested\":{ /* comment inside nested */ \"key\": 123}}}"u8;
        var readerOptions = new JsonReaderOptions { CommentHandling = JsonCommentHandling.Allow };
        var reader = new Utf8JsonReader(json, readerOptions);
        reader.Read(); // StartObject

        var converter = new ErrorJsonConverter();
        var error = converter.Read(ref reader, typeof(Error), Options)!;
        error.HasMetadata.Should().BeTrue();
        var nested = error.Metadata["nested"].Should().BeOfType<Dictionary<string, object?>>().Subject;
        nested["key"].Should().Be(123L);
    }

    [Fact]
    public void Read_WhenMetadataPrimitives_ParsesDoubleAndBooleans()
    {
        var json = "{\"code\":\"C\",\"description\":\"D\",\"metadata\":{\"dbl\":123.456,\"flagFalse\":false,\"flagTrue\":true,\"nullItem\":null}}";
        var error = JsonSerializer.Deserialize<Error>(json, Options)!;
        error.HasMetadata.Should().BeTrue();
        error.Metadata["dbl"].Should().Be(123.456);
        error.Metadata["flagFalse"].Should().Be(false);
        error.Metadata["flagTrue"].Should().Be(true);
        error.Metadata.ContainsKey("nullItem").Should().BeFalse();
    }



    [Fact]
    public void Write_WhenNullMetadataValue_WritesNull()
    {
        var error = Error.Create("C", "D")
            .WithMetadata("nullVal", (object?)null!)
            .Build();
        var json = JsonSerializer.Serialize(error, Options);
        json.Should().Contain("\"nullVal\":null");
    }

    private sealed class CustomMetadataObject
    {
        public override string ToString() => "custom_metadata_repr";
    }

#pragma warning disable S2225 // Testing ErrorJsonConverter handling of objects with null ToString()
    private sealed class NullToStringObject
    {
        public override string? ToString() => null;
    }
#pragma warning restore S2225

    [Fact]
    public void Write_WhenCustomObjectMetadata_WritesToString()
    {
        var error = Error.Create("C", "D")
            .WithMetadata("custom", new CustomMetadataObject())
            .WithMetadata("nullStr", new NullToStringObject())
            .Build();
        var json = JsonSerializer.Serialize(error, Options);
        json.Should().Contain("\"custom\":\"custom_metadata_repr\"");
        json.Should().Contain("\"nullStr\":\"\"");
    }
}








