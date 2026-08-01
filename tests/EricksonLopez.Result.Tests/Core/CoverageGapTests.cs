using System;
using Xunit;
using AwesomeAssertions;

namespace EricksonLopez.Result.Tests.Core;

public class CoverageGapTests
{
    [Fact]
    public void Error_Properties_ShouldBeCovered()
    {
        var error = Error.Failure("CodeX", "DescriptionX").WithDescriptionKey("KeyX");
        
        error.Code.Should().Be("CodeX");
        error.Description.Should().Be("DescriptionX");
        error.DescriptionKey.Should().Be("KeyX");
    }

    [Fact]
    public void Result_Ensure_WithLazyError_OnFailure_ShouldReturnOriginalFailure()
    {
        var originalError = Error.Failure("Original", "Original error");
        var result = Result.Failure(originalError);

        var ensureResult1 = result.Ensure(() => true, () => Error.Failure("New", "New error"));
        ensureResult1.Should().Be(result);

        var ensureResult2 = result.Ensure(1, (s) => true, () => Error.Failure("New", "New error"));
        ensureResult2.Should().Be(result);
    }

    [Fact]
    public void ResultOfT_Ensure_WithLazyError_OnFailure_ShouldReturnOriginalFailure()
    {
        var originalError = Error.Failure("Original", "Original error");
        var result = Result.Failure<int>(originalError);

        var ensureResult1 = result.Ensure((v) => true, () => Error.Failure("New", "New error"));
        ensureResult1.Should().Be(result);

        var ensureResult2 = result.Ensure(1, (s, v) => true, () => Error.Failure("New", "New error"));
        ensureResult2.Should().Be(result);
    }

    [Fact]
    public void StrictErrorComparer_GetHashCode_WithInnerErrors_ShouldBeCovered()
    {
        var innerError = Error.Failure("Inner", "Inner");
        var error = Error.Failure("Outer", "Outer", innerError);
        
        var comparer = ErrorEqualityComparer.Strict;
        var hash = comparer.GetHashCode(error);
        hash.Should().NotBe(0);
    }
}
