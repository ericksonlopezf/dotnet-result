// Copyright © Erickson Lopez. MIT License.
using System;
using AwesomeAssertions;
using EricksonLopez.Result;
using Xunit;

namespace EricksonLopez.Result.Tests.Core;

public class ErrorEnumStringsTests
{
    [Theory]
    [InlineData(ErrorType.Failure, "Failure")]
    [InlineData(ErrorType.Validation, "Validation")]
    [InlineData(ErrorType.NotFound, "NotFound")]
    [InlineData(ErrorType.Conflict, "Conflict")]
    [InlineData(ErrorType.Unauthorized, "Unauthorized")]
    [InlineData(ErrorType.Forbidden, "Forbidden")]
    [InlineData(ErrorType.Unavailable, "Unavailable")]
    [InlineData(ErrorType.Unexpected, "Unexpected")]
    [InlineData(ErrorType.Domain, "Domain")]
    [InlineData(ErrorType.Infrastructure, "Infrastructure")]
    [InlineData(ErrorType.Custom, "Custom")]
    [InlineData((ErrorType)255, "Failure")]
    public void ErrorTypeToString_ReturnsExpectedString(ErrorType type, string expected)
    {
        ErrorEnumStrings.ErrorTypeToString(type).Should().Be(expected);
    }

    [Theory]
    [InlineData(ErrorSeverity.Info, "Info")]
    [InlineData(ErrorSeverity.Warning, "Warning")]
    [InlineData(ErrorSeverity.Error, "Error")]
    [InlineData(ErrorSeverity.Critical, "Critical")]
    [InlineData((ErrorSeverity)255, "Error")]
    public void ErrorSeverityToString_ReturnsExpectedString(ErrorSeverity severity, string expected)
    {
        ErrorEnumStrings.ErrorSeverityToString(severity).Should().Be(expected);
    }

    [Theory]
    [InlineData(ErrorRetryability.NotApplicable, "NotApplicable")]
    [InlineData(ErrorRetryability.Transient, "Transient")]
    [InlineData(ErrorRetryability.Permanent, "Permanent")]
    [InlineData((ErrorRetryability)255, "NotApplicable")]
    public void ErrorRetryabilityToString_ReturnsExpectedString(ErrorRetryability retryability, string expected)
    {
        ErrorEnumStrings.ErrorRetryabilityToString(retryability).Should().Be(expected);
    }

    [Theory]
    [InlineData(ErrorType.Failure, "failure")]
    [InlineData(ErrorType.Validation, "validation")]
    [InlineData(ErrorType.NotFound, "not_found")]
    [InlineData(ErrorType.Conflict, "conflict")]
    [InlineData(ErrorType.Unauthorized, "unauthorized")]
    [InlineData(ErrorType.Forbidden, "forbidden")]
    [InlineData(ErrorType.Unavailable, "unavailable")]
    [InlineData(ErrorType.Unexpected, "unexpected")]
    [InlineData(ErrorType.Domain, "domain")]
    [InlineData(ErrorType.Infrastructure, "infrastructure")]
    [InlineData(ErrorType.Custom, "custom")]
    [InlineData((ErrorType)255, "_OTHER")]
    public void ErrorTypeToOTelString_ReturnsExpectedString(ErrorType type, string expected)
    {
        ErrorEnumStrings.ErrorTypeToOTelString(type).Should().Be(expected);
    }

    [Theory]
    [InlineData(ErrorSeverity.Info, "info")]
    [InlineData(ErrorSeverity.Warning, "warning")]
    [InlineData(ErrorSeverity.Error, "error")]
    [InlineData(ErrorSeverity.Critical, "critical")]
    [InlineData((ErrorSeverity)255, "error")]
    public void ErrorSeverityToOTelString_ReturnsExpectedString(ErrorSeverity severity, string expected)
    {
        ErrorEnumStrings.ErrorSeverityToOTelString(severity).Should().Be(expected);
    }
}



