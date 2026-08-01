using System.Text.Json;
using AwesomeAssertions;
using Xunit;

namespace EricksonLopez.Result.AspNetCore.Tests;

public class ErrorDetailDtoTests
{
    [Fact]
    public void Constructor_SetsProperties()
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
    public void GeneratedMethods_WorkCorrectly()
    {
        var dto1 = new ErrorDetailDto("A", "B", "C", "D", "E", "F", "G");
        var dto2 = new ErrorDetailDto("A", "B", "C", "D", "E", "F", "G");
        var dto3 = new ErrorDetailDto("Z", "B", "C", "D", "E", "F", "G");

        Assert.Equal(dto1, dto2);
        Assert.NotEqual(dto1, dto3);
        
        Assert.True(dto1 == dto2);
        Assert.False(dto1 == dto3);
        Assert.True(dto1 != dto3);
        Assert.False(dto1 != dto2);

        Assert.True(dto1.Equals((object)dto2));
        Assert.False(dto1.Equals((object)dto3));
        Assert.False(dto1.Equals(new object()));
        
        Assert.Equal(dto1.GetHashCode(), dto2.GetHashCode());
        Assert.NotEqual(dto1.GetHashCode(), dto3.GetHashCode());

        var str = dto1.ToString();
        Assert.Contains("ErrorDetailDto", str);
        Assert.Contains("Code = A", str);
        
        dto1.Deconstruct(out var code, out var desc, out var type, out var sev, out var retry, out var dk, out var tid);
        Assert.Equal("A", code);
        Assert.Equal("B", desc);
        Assert.Equal("C", type);
        Assert.Equal("D", sev);
        Assert.Equal("E", retry);
        Assert.Equal("F", dk);
        Assert.Equal("G", tid);
    }
}
