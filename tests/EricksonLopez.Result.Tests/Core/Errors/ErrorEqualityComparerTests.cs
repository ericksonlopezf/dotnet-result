// Copyright © Erickson Lopez. MIT License.
using System;
using System.Collections.Generic;
using AwesomeAssertions;
using EricksonLopez.Result;
using Xunit;

namespace EricksonLopez.Result.Tests.Core;

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

    [Fact]
    public void Strict_GetHashCode_VariesByEveryProperty()
    {
        var baseError = Error.Create("C", "D")
            .WithType(ErrorType.Domain)
            .WithSeverity(ErrorSeverity.Warning)
            .WithRetryability(ErrorRetryability.Transient)
            .WithDescriptionKey("key")
            .WithTraceId("trace")
            .WithCorrelationId("corr")
            .WithInnerError(Error.Failure("I1", "D1"))
            .WithMetadata("k1", "v1")
            .Build();

        var cmp = ErrorEqualityComparer.Strict;
        var baseHash = cmp.GetHashCode(baseError);

        // Code
        cmp.GetHashCode(baseError.ToBuilder().Build()).Should().Be(baseHash);
        cmp.GetHashCode(Error.Create("C2", "D").WithType(ErrorType.Domain).WithSeverity(ErrorSeverity.Warning).WithRetryability(ErrorRetryability.Transient).WithDescriptionKey("key").WithTraceId("trace").WithCorrelationId("corr").WithInnerError(Error.Failure("I1", "D1")).WithMetadata("k1", "v1").Build()).Should().NotBe(baseHash);

        // Description
        cmp.GetHashCode(Error.Create("C", "D2").WithType(ErrorType.Domain).WithSeverity(ErrorSeverity.Warning).WithRetryability(ErrorRetryability.Transient).WithDescriptionKey("key").WithTraceId("trace").WithCorrelationId("corr").WithInnerError(Error.Failure("I1", "D1")).WithMetadata("k1", "v1").Build()).Should().NotBe(baseHash);

        // Type
        cmp.GetHashCode(baseError.ToBuilder().WithType(ErrorType.Validation).Build()).Should().NotBe(baseHash);

        // Severity
        cmp.GetHashCode(baseError.ToBuilder().WithSeverity(ErrorSeverity.Critical).Build()).Should().NotBe(baseHash);

        // Retryability
        cmp.GetHashCode(baseError.ToBuilder().WithRetryability(ErrorRetryability.Permanent).Build()).Should().NotBe(baseHash);

        // DescriptionKey
        cmp.GetHashCode(baseError.WithDescriptionKey("key2")).Should().NotBe(baseHash);

        // TraceId
        cmp.GetHashCode(baseError.WithTraceId("trace2")).Should().NotBe(baseHash);

        // CorrelationId
        cmp.GetHashCode(baseError.WithCorrelationId("corr2")).Should().NotBe(baseHash);

        // InnerErrors - different code
        cmp.GetHashCode(baseError.ToBuilder().WithInnerError(Error.Failure("I2", "D1")).Build()).Should().NotBe(baseHash);

        // InnerErrors - different type
        cmp.GetHashCode(Error.Create("C", "D").WithType(ErrorType.Domain).WithSeverity(ErrorSeverity.Warning).WithRetryability(ErrorRetryability.Transient).WithDescriptionKey("key").WithTraceId("trace").WithCorrelationId("corr").WithInnerError(Error.Validation("I1", "D1")).WithMetadata("k1", "v1").Build()).Should().NotBe(baseHash);

        // Metadata - different key
        cmp.GetHashCode(Error.Create("C", "D").WithType(ErrorType.Domain).WithSeverity(ErrorSeverity.Warning).WithRetryability(ErrorRetryability.Transient).WithDescriptionKey("key").WithTraceId("trace").WithCorrelationId("corr").WithInnerError(Error.Failure("I1", "D1")).WithMetadata("k2", "v1").Build()).Should().NotBe(baseHash);

        // Metadata - different value
        cmp.GetHashCode(baseError.WithMetadata("k1", "v2")).Should().NotBe(baseHash);

        // Metadata - null value
        var errNullMeta = baseError.WithMetadata("k1", null!);
        cmp.GetHashCode(errNullMeta).Should().NotBe(0);
    }

    [Fact]
    public void Strict_GetHashCode_IsDeterministicAndMatchesExpected()
    {
        var err = Error.Create("CODE", "DESC")
            .WithType(ErrorType.Domain)
            .WithSeverity(ErrorSeverity.Warning)
            .WithRetryability(ErrorRetryability.Transient)
            .WithDescriptionKey("key")
            .WithTraceId("trace")
            .WithCorrelationId("corr")
            .WithInnerError(Error.Failure("I1", "D1"))
            .WithMetadata("k1", "v1")
            .Build();

        var expectedHash = new HashCode();
        expectedHash.Add(err.Code, System.StringComparer.Ordinal);
        expectedHash.Add(err.Description, System.StringComparer.Ordinal);
        expectedHash.Add<ErrorType>(err.Type);
        expectedHash.Add<ErrorSeverity>(err.Severity);
        expectedHash.Add<ErrorRetryability>(err.Retryability);
        expectedHash.Add<string?>(err.DescriptionKey, System.StringComparer.Ordinal);
        expectedHash.Add<string?>(err.TraceId, System.StringComparer.Ordinal);
        expectedHash.Add<string?>(err.CorrelationId, System.StringComparer.Ordinal);
        expectedHash.Add<int>(err.InnerErrors.Length);
        foreach (var inner in err.InnerErrors)
        {
            expectedHash.Add(inner.Code, System.StringComparer.Ordinal);
            expectedHash.Add<ErrorType>(inner.Type);
        }
        expectedHash.Add<int>(err.Metadata.Count);
        foreach (var kvp in err.Metadata)
        {
            expectedHash.Add(kvp.Key, System.StringComparer.Ordinal);
            int valHash = kvp.Value is null ? 0 : kvp.Value.GetHashCode();
            expectedHash.Add<int>(valHash);
        }

        var actualHash = ErrorEqualityComparer.Strict.GetHashCode(err);
        actualHash.Should().Be(expectedHash.ToHashCode());
    }
}
