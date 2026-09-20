// Copyright © Erickson Lopez. MIT License.
using System;
using AwesomeAssertions;
using EricksonLopez.Result;
using Xunit;

namespace EricksonLopez.Result.Tests.Core;

public class WellKnownErrorsTests
{
    [Fact]
    public void CombinedFailuresCode_HasExpectedValue()
    {
        WellKnownErrors.CombinedFailuresCode.Should().Be("Result.CombinedErrors");
    }

    [Fact]
    public void UninitializedError_HasExpectedProperties()
    {
        var err = WellKnownErrors.UninitializedError;

        err.Should().NotBeNull();
        err.Code.Should().Be("Result.Uninitialized");
        err.Description.Should().Be("Cannot access an uninitialized default Result.");
        err.Type.Should().Be(ErrorType.Unexpected);
        err.Severity.Should().Be(ErrorSeverity.Critical);
        err.Retryability.Should().Be(ErrorRetryability.NotApplicable);
        err.TraceId.Should().BeNull();
        err.CorrelationId.Should().BeNull();
        err.HasInnerErrors.Should().BeFalse();
        err.HasMetadata.Should().BeFalse();
    }

    [Fact]
    public void Error_None_HasExpectedProperties()
    {
        Assert.Equal("Error.None", Error.None.Code);
        Assert.Equal("No error.", Error.None.Description);
        Assert.Equal(ErrorType.Failure, Error.None.Type);
        Assert.Equal(ErrorSeverity.Info, Error.None.Severity);
        Assert.Equal(ErrorRetryability.NotApplicable, Error.None.Retryability);
    }

    [Fact]
    public void Error_NullValue_HasExpectedProperties()
    {
        Assert.Equal("Error.NullValue", Error.NullValue.Code);
        Assert.Equal("A null value was provided.", Error.NullValue.Description);
        Assert.Equal(ErrorType.Failure, Error.NullValue.Type);
        Assert.Equal(ErrorSeverity.Error, Error.NullValue.Severity);
        Assert.Equal(ErrorRetryability.NotApplicable, Error.NullValue.Retryability);
    }
}



