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
}



