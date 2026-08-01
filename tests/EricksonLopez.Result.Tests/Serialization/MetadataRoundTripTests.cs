using System;
using System.Collections.Generic;
using System.Text.Json;
using Xunit;
using EricksonLopez.Result.Serialization;

namespace EricksonLopez.Result.Tests.Serialization;

public class MetadataRoundTripTests
{
    private static readonly JsonSerializerOptions Options = new()
    {
        Converters = { new ErrorJsonConverter() }
    };

    [Fact]
    public void IntValue_RecoveredAsLong()
    {
        var error = Error.Create("C", "D").WithMetadata("key", 42).Build();
        var json = JsonSerializer.Serialize(error, Options);
        var deserialized = JsonSerializer.Deserialize<Error>(json, Options)!;

        // int is written as JSON number; on deserialization, TryGetInt64 succeeds → long
        Assert.IsType<long>(deserialized.Metadata["key"]);
        Assert.Equal(42L, deserialized.Metadata["key"]);
    }

    [Fact]
    public void LongValue_RecoveredAsLong()
    {
        var error = Error.Create("C", "D").WithMetadata("key", 9999999999L).Build();
        var json = JsonSerializer.Serialize(error, Options);
        var deserialized = JsonSerializer.Deserialize<Error>(json, Options)!;

        Assert.IsType<long>(deserialized.Metadata["key"]);
        Assert.Equal(9999999999L, deserialized.Metadata["key"]);
    }

    [Fact]
    public void DoubleValue_RecoveredAsDouble()
    {
        var error = Error.Create("C", "D").WithMetadata("key", 3.14).Build();
        var json = JsonSerializer.Serialize(error, Options);
        var deserialized = JsonSerializer.Deserialize<Error>(json, Options)!;

        Assert.IsType<double>(deserialized.Metadata["key"]);
        Assert.Equal(3.14, (double)deserialized.Metadata["key"], precision: 10);
    }

    [Fact]
    public void FloatValue_RecoveredAsDouble()
    {
        // float → JSON number → double (lossy: float precision differs from double)
        var error = Error.Create("C", "D").WithMetadata("key", 1.5f).Build();
        var json = JsonSerializer.Serialize(error, Options);
        var deserialized = JsonSerializer.Deserialize<Error>(json, Options)!;

        // 1.5f is exactly representable, so it round-trips as long (1) if integral, or double
        var value = deserialized.Metadata["key"];
        Assert.True(value is long or double, $"Expected long or double, got {value?.GetType().Name}");
    }

    [Fact]
    public void DecimalValue_RecoveredAsLongOrDouble()
    {
        // decimal → JSON number → long (if integral) or double
        var error = Error.Create("C", "D").WithMetadata("key", 99.99m).Build();
        var json = JsonSerializer.Serialize(error, Options);
        var deserialized = JsonSerializer.Deserialize<Error>(json, Options)!;

        var value = deserialized.Metadata["key"];
        Assert.True(value is long or double, $"Expected long or double, got {value?.GetType().Name}");
    }

    [Fact]
    public void BoolTrue_RecoveredAsBool()
    {
        var error = Error.Create("C", "D").WithMetadata("key", true).Build();
        var json = JsonSerializer.Serialize(error, Options);
        var deserialized = JsonSerializer.Deserialize<Error>(json, Options)!;

        Assert.IsType<bool>(deserialized.Metadata["key"]);
        Assert.Equal(true, deserialized.Metadata["key"]);
    }

    [Fact]
    public void BoolFalse_RecoveredAsBool()
    {
        var error = Error.Create("C", "D").WithMetadata("key", false).Build();
        var json = JsonSerializer.Serialize(error, Options);
        var deserialized = JsonSerializer.Deserialize<Error>(json, Options)!;

        Assert.IsType<bool>(deserialized.Metadata["key"]);
        Assert.Equal(false, deserialized.Metadata["key"]);
    }

    [Fact]
    public void StringValue_RecoveredAsString()
    {
        var error = Error.Create("C", "D").WithMetadata("key", "hello world").Build();
        var json = JsonSerializer.Serialize(error, Options);
        var deserialized = JsonSerializer.Deserialize<Error>(json, Options)!;

        Assert.IsType<string>(deserialized.Metadata["key"]);
        Assert.Equal("hello world", deserialized.Metadata["key"]);
    }

    [Fact]
    public void DateTimeValue_RecoveredAsString()
    {
        // DateTime → JSON string (ISO 8601 "O" format) → string on deserialize
        var dt = new DateTime(2026, 7, 29, 12, 0, 0, DateTimeKind.Utc);
        var error = Error.Create("C", "D").WithMetadata("key", dt).Build();
        var json = JsonSerializer.Serialize(error, Options);
        var deserialized = JsonSerializer.Deserialize<Error>(json, Options)!;

        Assert.IsType<string>(deserialized.Metadata["key"]);
        var recovered = (string)deserialized.Metadata["key"];
        Assert.Contains("2026", recovered);
    }

    [Fact]
    public void GuidValue_RecoveredAsString()
    {
        // Guid → JSON string → string on deserialize
        var guid = Guid.Parse("12345678-1234-1234-1234-123456789012");
        var error = Error.Create("C", "D").WithMetadata("key", guid).Build();
        var json = JsonSerializer.Serialize(error, Options);
        var deserialized = JsonSerializer.Deserialize<Error>(json, Options)!;

        Assert.IsType<string>(deserialized.Metadata["key"]);
        Assert.Equal(guid.ToString(), deserialized.Metadata["key"]);
    }

    [Fact]
    public void TimeSpanValue_RecoveredAsString()
    {
        // TimeSpan → JSON string (constant format "c") → string on deserialize
        var ts = TimeSpan.FromHours(2.5);
        var error = Error.Create("C", "D").WithMetadata("key", ts).Build();
        var json = JsonSerializer.Serialize(error, Options);
        var deserialized = JsonSerializer.Deserialize<Error>(json, Options)!;

        Assert.IsType<string>(deserialized.Metadata["key"]);
    }

    [Fact]
    public void NestedObject_RecoveredAsDictionary()
    {
        // Complex objects are now parsed recursively into dictionaries
        var json = @"{""code"":""C"", ""description"":""D"", ""metadata"": {""nested"": {""a"": 1, ""b"": ""two""}}}";
        var deserialized = JsonSerializer.Deserialize<Error>(json, Options)!;

        // Nested objects are recovered as Dictionary<string, object>
        var value = deserialized.Metadata["nested"];
        var dict = Assert.IsType<Dictionary<string, object?>>(value);
        Assert.Equal(1L, dict["a"]);
        Assert.Equal("two", dict["b"]);
    }

    [Fact]
    public void NullMetadataValue_HandledFromJson()
    {
        // When JSON metadata contains a null value, the key is intentionally omitted during deserialization.
        // ImmutableDictionary<string, object> cannot hold null values (the type parameter is 'object', not 'object?').
        // This is documented behavior: null metadata values are silently skipped.
        var json = @"{""code"":""C"", ""description"":""D"", ""metadata"": {""key"": null}}";
        var deserialized = JsonSerializer.Deserialize<Error>(json, Options)!;

        // Null values are not stored in metadata — the key is absent.
        Assert.False(deserialized.HasMetadata,
            "Null metadata values are intentionally omitted during deserialization.");
    }

    [Fact]
    public void ArrayMetadataValue_RecoveredAsList()
    {
        // JSON arrays in metadata are parsed recursively into lists
        var json = @"{""code"":""C"", ""description"":""D"", ""metadata"": {""items"": [1, 2, 3]}}";
        var deserialized = JsonSerializer.Deserialize<Error>(json, Options)!;

        var value = deserialized.Metadata["items"];
        var list = Assert.IsType<List<object?>>(value);
        Assert.Equal(3, list.Count);
        Assert.Equal(1L, list[0]);
        Assert.Equal(2L, list[1]);
        Assert.Equal(3L, list[2]);
    }

    [Fact]
    public void MultipleMetadataTypes_RoundTrip()
    {
        var error = Error.Create("C", "D")
            .WithMetadata("str", "hello")
            .WithMetadata("num", 42)
            .WithMetadata("dbl", 3.14)
            .WithMetadata("boolTrue", true)
            .WithMetadata("boolFalse", false)
            .Build();

        var json = JsonSerializer.Serialize(error, Options);
        var deserialized = JsonSerializer.Deserialize<Error>(json, Options)!;

        Assert.Equal("hello", deserialized.Metadata["str"]);
        Assert.Equal(42L, deserialized.Metadata["num"]);
        Assert.Equal(3.14, (double)deserialized.Metadata["dbl"], precision: 10);
        Assert.Equal(true, deserialized.Metadata["boolTrue"]);
        Assert.Equal(false, deserialized.Metadata["boolFalse"]);
    }

    [Fact]
    public void ShortValue_RecoveredAsLong()
    {
        var error = Error.Create("C", "D").WithMetadata("key", (short)123).Build();
        var json = JsonSerializer.Serialize(error, Options);
        var deserialized = JsonSerializer.Deserialize<Error>(json, Options)!;

        Assert.IsType<long>(deserialized.Metadata["key"]);
        Assert.Equal(123L, deserialized.Metadata["key"]);
    }

    [Fact]
    public void ByteValue_RecoveredAsLong()
    {
        var error = Error.Create("C", "D").WithMetadata("key", (byte)255).Build();
        var json = JsonSerializer.Serialize(error, Options);
        var deserialized = JsonSerializer.Deserialize<Error>(json, Options)!;

        Assert.IsType<long>(deserialized.Metadata["key"]);
        Assert.Equal(255L, deserialized.Metadata["key"]);
    }

    [Fact]
    public void DateTimeOffsetValue_RecoveredAsString()
    {
        var dto = new DateTimeOffset(2026, 7, 29, 12, 0, 0, TimeSpan.FromHours(-4));
        var error = Error.Create("C", "D").WithMetadata("key", dto).Build();
        var json = JsonSerializer.Serialize(error, Options);
        var deserialized = JsonSerializer.Deserialize<Error>(json, Options)!;

        Assert.IsType<string>(deserialized.Metadata["key"]);
        Assert.Contains("2026", (string)deserialized.Metadata["key"]);
    }
}
