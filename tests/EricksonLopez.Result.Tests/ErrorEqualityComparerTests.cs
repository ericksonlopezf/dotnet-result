using System;
using System.Collections.Generic;
using AwesomeAssertions;
using Xunit;

namespace EricksonLopez.Result.Tests;

public class ErrorEqualityComparerTests
{
    [Fact]
    public void Default_Equals_ReturnsTrue_WhenReferencesAreSame()
    {
        var error = Error.Failure("code", "description");
        var comparer = ErrorEqualityComparer.Default;
        comparer.Equals(error, error).Should().BeTrue();
    }

    [Fact]
    public void Default_Equals_ReturnsFalse_WhenXIsNull()
    {
        var y = Error.Failure("code", "description");
        var comparer = ErrorEqualityComparer.Default;
        comparer.Equals(null, y).Should().BeFalse();
    }

    [Fact]
    public void Default_Equals_ReturnsFalse_WhenYIsNull()
    {
        var x = Error.Failure("code", "description");
        var comparer = ErrorEqualityComparer.Default;
        comparer.Equals(x, null).Should().BeFalse();
    }

    [Fact]
    public void Default_Equals_ReturnsTrue_WhenPropertiesMatch()
    {
        var x = Error.Failure("code", "description");
        var y = Error.Failure("code", "description");
        var comparer = ErrorEqualityComparer.Default;
        comparer.Equals(x, y).Should().BeTrue();
    }

    [Fact]
    public void Default_GetHashCode_ReturnsExpectedValue()
    {
        var error = Error.Failure("code", "description");
        var comparer = ErrorEqualityComparer.Default;
        comparer.GetHashCode(error).Should().Be(error.GetHashCode());
    }

    [Fact]
    public void Strict_Equals_ReturnsTrue_WhenReferencesAreSame()
    {
        var error = Error.Failure("code", "description");
        var comparer = ErrorEqualityComparer.Strict;
        comparer.Equals(error, error).Should().BeTrue();
    }

    [Fact]
    public void Strict_Equals_ReturnsFalse_WhenXIsNull()
    {
        var y = Error.Failure("code", "description");
        var comparer = ErrorEqualityComparer.Strict;
        comparer.Equals(null, y).Should().BeFalse();
    }

    [Fact]
    public void Strict_Equals_ReturnsFalse_WhenYIsNull()
    {
        var x = Error.Failure("code", "description");
        var comparer = ErrorEqualityComparer.Strict;
        comparer.Equals(x, null).Should().BeFalse();
    }

    [Fact]
    public void Strict_Equals_ReturnsTrue_WhenStrictPropertiesMatch()
    {
        var x = Error.Failure("code", "description");
        var y = Error.Failure("code", "description");
        var comparer = ErrorEqualityComparer.Strict;
        comparer.Equals(x, y).Should().BeTrue();
    }

    [Fact]
    public void Strict_GetHashCode_VariesByInnerErrorsAndMetadata()
    {
        var error1 = Error.Failure("code", "description");
        var error2 = Error.Failure("code", "description").WithMetadata("key", "val");
        
        var comparer = ErrorEqualityComparer.Strict;
        var hash1 = comparer.GetHashCode(error1);
        var hash2 = comparer.GetHashCode(error2);

        hash1.Should().NotBe(hash2);
    }

    [Fact]
    public void Default_Equals_ReturnsTrue_WhenBothNull()
    {
        var comparer = ErrorEqualityComparer.Default;
        comparer.Equals(null, null).Should().BeTrue();
    }

    [Fact]
    public void Default_Equals_ReturnsFalse_WhenPropertiesDiffer()
    {
        var x = Error.Failure("code1", "description");
        var y = Error.Failure("code2", "description");
        var comparer = ErrorEqualityComparer.Default;
        comparer.Equals(x, y).Should().BeFalse();
    }

    [Fact]
    public void Strict_Equals_ReturnsTrue_WhenBothNull()
    {
        var comparer = ErrorEqualityComparer.Strict;
        comparer.Equals(null, null).Should().BeTrue();
    }

    [Fact]
    public void Strict_Equals_ReturnsFalse_WhenTraceIdDiffers()
    {
        var x = Error.Failure("code", "description").WithTraceId("trace1");
        var y = Error.Failure("code", "description").WithTraceId("trace2");
        var comparer = ErrorEqualityComparer.Strict;
        comparer.Equals(x, y).Should().BeFalse();
    }

    [Fact]
    public void Strict_Equals_ReturnsFalse_WhenInnerErrorsDiffer()
    {
        var inner1 = Error.Failure("inner1", "msg");
        var inner2 = Error.Failure("inner2", "msg");
        
        var x = Error.Failure("code", "description").ToBuilder().WithInnerError(inner1).Build();
        var y = Error.Failure("code", "description").ToBuilder().WithInnerError(inner2).Build();
        
        var comparer = ErrorEqualityComparer.Strict;
        comparer.Equals(x, y).Should().BeFalse();
    }
    
    [Fact]
    public void Strict_Equals_ReturnsFalse_WhenMetadataDiffers()
    {
        var x = Error.Failure("code", "description").WithMetadata("key", "val1");
        var y = Error.Failure("code", "description").WithMetadata("key", "val2");
        
        var comparer = ErrorEqualityComparer.Strict;
        comparer.Equals(x, y).Should().BeFalse();
    }

    [Fact]
    public void Strict_GetHashCode_WithInnerErrors_ComputesCorrectly()
    {
        var inner = Error.Failure("inner", "msg");
        var error = Error.Failure("code", "description").ToBuilder().WithInnerError(inner).Build();
        
        var comparer = ErrorEqualityComparer.Strict;
        var hash = comparer.GetHashCode(error);
        
        hash.Should().NotBe(0);
    }

    [Fact]
    public void Default_HashSet_DeduplicatesOnShallowEquality()
    {
        var x = Error.Failure("code", "description").WithTraceId("trace1");
        var y = Error.Failure("code", "description").WithTraceId("trace2");
        
        var set = new HashSet<Error>(ErrorEqualityComparer.Default) { x, y };
        
        set.Count.Should().Be(1);
    }

    [Fact]
    public void Strict_HashSet_DoesNotDeduplicateOnTraceIdDifference()
    {
        var x = Error.Failure("code", "description").WithTraceId("trace1");
        var y = Error.Failure("code", "description").WithTraceId("trace2");
        
        var set = new HashSet<Error>(ErrorEqualityComparer.Strict) { x, y };
        
        set.Count.Should().Be(2);
    }
}
