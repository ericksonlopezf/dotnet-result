// Copyright © Erickson Lopez. MIT License.
using System;
using System.Collections.Generic;
using System.Text.Json;
using AwesomeAssertions;
using EricksonLopez.Result;
using Xunit;

namespace EricksonLopez.Result.AspNetCore.Tests;

public class ErrorDetailDtoTests
{
    [Fact]
    public void Constructor_WhenValidParameters_SetsProperties()
    {
        var dto = new ErrorDetailDto("CODE", "Desc", "Type", "Severity", "Retry", "Key", "TraceId");
        dto.Code.Should().Be("CODE");
        dto.Description.Should().Be("Desc");
        dto.Type.Should().Be("Type");
        dto.Severity.Should().Be("Severity");
        dto.Retryability.Should().Be("Retry");
        dto.DescriptionKey.Should().Be("Key");
        dto.TraceId.Should().Be("TraceId");
    }

    [Fact]
    public void GeneratedMethods_WhenCompared_ReturnExpectedResults()
    {
        var dto1 = new ErrorDetailDto("A", "B", "C", "D", "E", "F", "G");
        var dto2 = new ErrorDetailDto("A", "B", "C", "D", "E", "F", "G");
        var dto3 = new ErrorDetailDto("Z", "B", "C", "D", "E", "F", "G");

        dto1.Should().Be(dto2);
        dto1.Should().NotBe(dto3);

        (dto1 == dto2).Should().BeTrue();
        (dto1 == dto3).Should().BeFalse();
        (dto1 != dto3).Should().BeTrue();
        (dto1 != dto2).Should().BeFalse();

        dto1.Equals((object)dto2).Should().BeTrue();
        dto1.Equals((object)dto3).Should().BeFalse();
        dto1.Equals(new object()).Should().BeFalse();

        dto1.GetHashCode().Should().Be(dto2.GetHashCode());
        dto1.GetHashCode().Should().NotBe(dto3.GetHashCode());

        var str = dto1.ToString();
        str.Should().Contain("ErrorDetailDto");
        str.Should().Contain("Code = A");

        dto1.Deconstruct(out var code, out var desc, out var type, out var sev, out var retry, out var dk, out var tid);
        code.Should().Be("A");
        desc.Should().Be("B");
        type.Should().Be("C");
        sev.Should().Be("D");
        retry.Should().Be("E");
        dk.Should().Be("F");
        tid.Should().Be("G");
    }

    [Fact]
    public void JsonSerializer_WhenUsingSourceGenerator_SerializesSuccessfully()
    {
        var dto = new ErrorDetailDto("CODE", "Desc", "Validation", "Error", "NonRetryable", "KEY", "TRACE123");
        var json = JsonSerializer.Serialize(dto, AspNetCoreJsonSerializerContext.Default.ErrorDetailDto);
        json.Should().Contain("\"Code\":\"CODE\"");

        var deserialized = JsonSerializer.Deserialize(json, AspNetCoreJsonSerializerContext.Default.ErrorDetailDto);
        deserialized.Code.Should().Be("CODE");
        deserialized.Description.Should().Be("Desc");
        deserialized.Type.Should().Be("Validation");
        deserialized.Severity.Should().Be("Error");
        deserialized.Retryability.Should().Be("NonRetryable");
        deserialized.DescriptionKey.Should().Be("KEY");
        deserialized.TraceId.Should().Be("TRACE123");

        var list = new List<ErrorDetailDto> { dto };
        var listJson = JsonSerializer.Serialize(list, AspNetCoreJsonSerializerContext.Default.ListErrorDetailDto);
        listJson.Should().Contain("\"Code\":\"CODE\"");

        var listDeserialized = JsonSerializer.Deserialize(listJson, AspNetCoreJsonSerializerContext.Default.ListErrorDetailDto);
        listDeserialized.Should().NotBeNull();
        listDeserialized.Should().HaveCount(1);
        listDeserialized![0].Code.Should().Be("CODE");

        var ctx = new AspNetCoreJsonSerializerContext(new JsonSerializerOptions());
        ctx.GetTypeInfo(typeof(ErrorDetailDto)).Should().NotBeNull();
        ctx.GetTypeInfo(typeof(List<ErrorDetailDto>)).Should().NotBeNull();
        ctx.GetTypeInfo(typeof(string)).Should().NotBeNull();
        ctx.GetTypeInfo(typeof(int)).Should().BeNull();
    }
}



