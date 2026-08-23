// Copyright © Erickson Lopez. MIT License.
using System;
using System.Collections.Generic;
using System.Text.Json;
using AwesomeAssertions;
using EricksonLopez.Result;
using EricksonLopez.Result.Serialization;
using Xunit;

namespace EricksonLopez.Result.Serialization.Tests;

public class MetadataRoundTripTests
{
    private static readonly JsonSerializerOptions Options = new()
    {
        Converters = { new ErrorJsonConverter() }
    };

    [Fact]
    public void IntValue_WhenSerialized_RecoversAsLong()
    {
        var error = Error.Create("C", "D").WithMetadata("key", 42).Build();
        var json = JsonSerializer.Serialize(error, Options);
        var deserialized = JsonSerializer.Deserialize<Error>(json, Options)!;

        // int is written as JSON number; on deserialization, TryGetInt64 succeeds → long
        Assert.IsType<long>(deserialized.Metadata["key"]);
        deserialized.Metadata["key"].Should().Be(42L);
    }

    [Fact]
    public void LongValue_WhenSerialized_RecoversAsLong()
    {
        var error = Error.Create("C", "D").WithMetadata("key", 9999999999L).Build();
        var json = JsonSerializer.Serialize(error, Options);
        var deserialized = JsonSerializer.Deserialize<Error>(json, Options)!;

        Assert.IsType<long>(deserialized.Metadata["key"]);
        deserialized.Metadata["key"].Should().Be(9999999999L);
    }

    [Fact]
    public void DoubleValue_WhenSerialized_RecoversAsDouble()
    {
        var error = Error.Create("C", "D").WithMetadata("key", 3.14).Build();
        var json = JsonSerializer.Serialize(error, Options);
        var deserialized = JsonSerializer.Deserialize<Error>(json, Options)!;

        Assert.IsType<double>(deserialized.Metadata["key"]);
        Assert.Equal(3.14, (double)deserialized.Metadata["key"], precision: 10);
    }

    [Fact]
    public void FloatValue_WhenSerialized_RecoversAsDouble()
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
    public void DecimalValue_WhenSerialized_RecoversAsLongOrDouble()
    {
        // decimal → JSON number → long (if integral) or double
        var error = Error.Create("C", "D").WithMetadata("key", 99.99m).Build();
        var json = JsonSerializer.Serialize(error, Options);
        var deserialized = JsonSerializer.Deserialize<Error>(json, Options)!;

        var value = deserialized.Metadata["key"];
        Assert.True(value is long or double, $"Expected long or double, got {value?.GetType().Name}");
    }

    [Fact]
    public void BoolTrue_WhenSerialized_RecoversAsBool()
    {
        var error = Error.Create("C", "D").WithMetadata("key", true).Build();
        var json = JsonSerializer.Serialize(error, Options);
        var deserialized = JsonSerializer.Deserialize<Error>(json, Options)!;

        Assert.IsType<bool>(deserialized.Metadata["key"]);
        deserialized.Metadata["key"].Should().Be(true);
    }

    [Fact]
    public void BoolFalse_WhenSerialized_RecoversAsBool()
    {
        var error = Error.Create("C", "D").WithMetadata("key", false).Build();
        var json = JsonSerializer.Serialize(error, Options);
        var deserialized = JsonSerializer.Deserialize<Error>(json, Options)!;

        Assert.IsType<bool>(deserialized.Metadata["key"]);
        deserialized.Metadata["key"].Should().Be(false);
    }

    [Fact]
    public void StringValue_WhenSerialized_RecoversAsString()
    {
        var error = Error.Create("C", "D").WithMetadata("key", "hello world").Build();
        var json = JsonSerializer.Serialize(error, Options);
        var deserialized = JsonSerializer.Deserialize<Error>(json, Options)!;

        Assert.IsType<string>(deserialized.Metadata["key"]);
        deserialized.Metadata["key"].Should().Be("hello world");
    }

    [Fact]
    public void DateTimeValue_WhenSerialized_RecoversAsString()
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
    public void GuidValue_WhenSerialized_RecoversAsString()
    {
        // Guid → JSON string → string on deserialize
        var guid = Guid.Parse("12345678-1234-1234-1234-123456789012");
        var error = Error.Create("C", "D").WithMetadata("key", guid).Build();
        var json = JsonSerializer.Serialize(error, Options);
        var deserialized = JsonSerializer.Deserialize<Error>(json, Options)!;

        Assert.IsType<string>(deserialized.Metadata["key"]);
        deserialized.Metadata["key"].Should().Be(guid.ToString());
    }

    [Fact]
    public void TimeSpanValue_WhenSerialized_RecoversAsString()
    {
        // TimeSpan → JSON string (constant format "c") → string on deserialize
        var ts = TimeSpan.FromHours(2.5);
        var error = Error.Create("C", "D").WithMetadata("key", ts).Build();
        var json = JsonSerializer.Serialize(error, Options);
        var deserialized = JsonSerializer.Deserialize<Error>(json, Options)!;

        Assert.IsType<string>(deserialized.Metadata["key"]);
    }

    [Fact]
    public void NestedObject_WhenSerialized_RecoversAsDictionary()
    {
        // Complex objects are now parsed recursively into dictionaries
        var json = @"{""code"":""C"", ""description"":""D"", ""metadata"": {""nested"": {""a"": 1, ""b"": ""two""}}}";
        var deserialized = JsonSerializer.Deserialize<Error>(json, Options)!;

        // Nested objects are recovered as Dictionary<string, object>
        var value = deserialized.Metadata["nested"];
        var dict = Assert.IsType<Dictionary<string, object?>>(value);
        dict["a"].Should().Be(1L);
        dict["b"].Should().Be("two");
    }

    [Fact]
    public void NullMetadataValue_WhenSerialized_RecoversAsNull()
    {
        // When JSON metadata contains a null value, the key is intentionally omitted during deserialization.
        // ImmutableDictionary<string, object> cannot hold null values (the type parameter is 'object', not 'object?').
        // This is documented behavior: null metadata values are silently skipped.
        var json = @"{""code"":""C"", ""description"":""D"", ""metadata"": {""key"": null}}";
        var deserialized = JsonSerializer.Deserialize<Error>(json, Options)!;

        deserialized.HasMetadata.Should().BeFalse("Null metadata values are intentionally omitted during deserialization.");
    }

    [Fact]
    public void ArrayMetadataValue_WhenSerialized_RecoversAsList()
    {
        // JSON arrays in metadata are parsed recursively into lists
        var json = @"{""code"":""C"", ""description"":""D"", ""metadata"": {""items"": [1, 2, 3]}}";
        var deserialized = JsonSerializer.Deserialize<Error>(json, Options)!;

        var value = deserialized.Metadata["items"];
        var list = Assert.IsType<List<object?>>(value);
        list.Count.Should().Be(3);
        list[0].Should().Be(1L);
        list[1].Should().Be(2L);
        list[2].Should().Be(3L);
    }

    [Fact]
    public void MultipleMetadataTypes_WhenSerialized_RoundTripsCorrectly()
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

        deserialized.Metadata["str"].Should().Be("hello");
        deserialized.Metadata["num"].Should().Be(42L);
        Assert.Equal(3.14, (double)deserialized.Metadata["dbl"], precision: 10);
        deserialized.Metadata["boolTrue"].Should().Be(true);
        deserialized.Metadata["boolFalse"].Should().Be(false);
    }

    [Fact]
    public void ShortValue_WhenSerialized_RecoversAsLong()
    {
        var error = Error.Create("C", "D").WithMetadata("key", (short)123).Build();
        var json = JsonSerializer.Serialize(error, Options);
        var deserialized = JsonSerializer.Deserialize<Error>(json, Options)!;

        Assert.IsType<long>(deserialized.Metadata["key"]);
        deserialized.Metadata["key"].Should().Be(123L);
    }

    [Fact]
    public void ByteValue_WhenSerialized_RecoversAsLong()
    {
        var error = Error.Create("C", "D").WithMetadata("key", (byte)255).Build();
        var json = JsonSerializer.Serialize(error, Options);
        var deserialized = JsonSerializer.Deserialize<Error>(json, Options)!;

        Assert.IsType<long>(deserialized.Metadata["key"]);
        deserialized.Metadata["key"].Should().Be(255L);
    }

    [Fact]
    public void DateTimeOffsetValue_WhenSerialized_RecoversAsString()
    {
        var dto = new DateTimeOffset(2026, 7, 29, 12, 0, 0, TimeSpan.FromHours(-4));
        var error = Error.Create("C", "D").WithMetadata("key", dto).Build();
        var json = JsonSerializer.Serialize(error, Options);
        var deserialized = JsonSerializer.Deserialize<Error>(json, Options)!;

        Assert.IsType<string>(deserialized.Metadata["key"]);
        Assert.Contains("2026", (string)deserialized.Metadata["key"]);
    }
}



