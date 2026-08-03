using System;
using Xunit;
using EricksonLopez.Result.Testing;

namespace EricksonLopez.Result.Testing.Tests;

public class ErrorAssertionsTests
{
    [Fact]
    public void ShouldHaveErrorCode_Success()
    {
        var error = Error.Failure("Code1", "Desc");
        error.ShouldHaveErrorCode("Code1");
    }

    [Fact]
    public void ShouldHaveErrorCode_Throws()
    {
        var error = Error.Failure("Code1", "Desc");
        Assert.Throws<ResultAssertionException>(() => error.ShouldHaveErrorCode("Code2"));
    }

    [Fact]
    public void ShouldHaveErrorType_Success()
    {
        var error = Error.Failure("Code1", "Desc");
        error.ShouldHaveErrorType(ErrorType.Failure);
    }

    [Fact]
    public void ShouldHaveErrorType_Throws()
    {
        var error = Error.Failure("Code1", "Desc");
        Assert.Throws<ResultAssertionException>(() => error.ShouldHaveErrorType(ErrorType.Validation));
    }

    [Fact]
    public void ShouldHaveSeverity_Success()
    {
        var error = Error.Create("A", "B").WithSeverity(ErrorSeverity.Critical).Build();
        error.ShouldHaveSeverity(ErrorSeverity.Critical);
    }

    [Fact]
    public void ShouldHaveSeverity_Throws()
    {
        var error = Error.Create("A", "B").WithSeverity(ErrorSeverity.Critical).Build();
        Assert.Throws<ResultAssertionException>(() => error.ShouldHaveSeverity(ErrorSeverity.Error));
    }

    [Fact]
    public void ShouldHaveRetryability_Success()
    {
        var error = Error.Create("A", "B").WithRetryability(ErrorRetryability.Transient).Build();
        error.ShouldHaveRetryability(ErrorRetryability.Transient);
    }

    [Fact]
    public void ShouldHaveRetryability_Throws()
    {
        var error = Error.Create("A", "B").WithRetryability(ErrorRetryability.Transient).Build();
        Assert.Throws<ResultAssertionException>(() => error.ShouldHaveRetryability(ErrorRetryability.Permanent));
    }

    [Fact]
    public void ShouldHaveDescription_Success()
    {
        var error = Error.Failure("A", "B");
        error.ShouldHaveDescription("B");
    }

    [Fact]
    public void ShouldHaveDescription_Throws()
    {
        var error = Error.Failure("A", "B");
        Assert.Throws<ResultAssertionException>(() => error.ShouldHaveDescription("C"));
    }

    [Fact]
    public void ShouldHaveMetadata_Success()
    {
        var error = Error.Create("A", "B").WithMetadata("K", "V").Build();
        error.ShouldHaveMetadata("K", "V");
        error.ShouldHaveMetadataKey("K");
    }

    [Fact]
    public void ShouldHaveMetadata_Throws()
    {
        var error = Error.Create("A", "B").WithMetadata("K", "V").Build();
        Assert.Throws<ResultAssertionException>(() => error.ShouldHaveMetadata("K", "V2"));
        Assert.Throws<ResultAssertionException>(() => error.ShouldHaveMetadata("K2", "V"));
        Assert.Throws<ResultAssertionException>(() => error.ShouldHaveMetadataKey("K2"));
    }

    [Fact]
    public void ShouldHaveTraceId_Success()
    {
        var error = Error.Create("A", "B").WithTraceId("T1").Build();
        error.ShouldHaveTraceId("T1");
    }

    [Fact]
    public void ShouldHaveTraceId_Throws()
    {
        var error = Error.Create("A", "B").WithTraceId("T1").Build();
        Assert.Throws<ResultAssertionException>(() => error.ShouldHaveTraceId("T2"));
    }

    [Fact]
    public void ShouldHaveCorrelationId_Success()
    {
        var error = Error.Create("A", "B").WithCorrelationId("C1").Build();
        error.ShouldHaveCorrelationId("C1");
    }

    [Fact]
    public void ShouldHaveCorrelationId_Throws()
    {
        var error = Error.Create("A", "B").WithCorrelationId("C1").Build();
        Assert.Throws<ResultAssertionException>(() => error.ShouldHaveCorrelationId("C2"));
    }

    [Fact]
    public void ShouldBeRetryable_Success()
    {
        var error = Error.Create("A", "B").WithRetryability(ErrorRetryability.Transient).Build();
        error.ShouldBeRetryable();
    }

    [Fact]
    public void ShouldBeRetryable_Throws()
    {
        var error = Error.Create("A", "B").WithRetryability(ErrorRetryability.Permanent).Build();
        Assert.Throws<ResultAssertionException>(() => error.ShouldBeRetryable());
    }

    [Fact]
    public void ShouldBePermanent_Success()
    {
        var error = Error.Create("A", "B").WithRetryability(ErrorRetryability.Permanent).Build();
        error.ShouldBePermanent();
    }

    [Fact]
    public void ShouldBePermanent_Throws()
    {
        var error = Error.Create("A", "B").WithRetryability(ErrorRetryability.Transient).Build();
        Assert.Throws<ResultAssertionException>(() => error.ShouldBePermanent());
    }

    [Fact]
    public void ShouldHaveInnerErrors_Success()
    {
        var error = Error.Failure("A", "B", Error.Failure("X", "Y"));
        error.ShouldHaveInnerErrors(1);
    }

    [Fact]
    public void ShouldHaveInnerErrors_Throws()
    {
        var error = Error.Failure("A", "B", Error.Failure("X", "Y"));
        Assert.Throws<ResultAssertionException>(() => error.ShouldHaveInnerErrors(0));
    }

    [Fact]
    public void ShouldContainInnerError_Success()
    {
        var error = Error.Failure("A", "B", Error.Failure("X", "Y"));
        error.ShouldContainInnerError("X");
    }

    [Fact]
    public void ShouldContainInnerError_Throws()
    {
        var error = Error.Failure("A", "B", Error.Failure("X", "Y"));
        Assert.Throws<ResultAssertionException>(() => error.ShouldContainInnerError("Z"));
        Assert.Throws<ResultAssertionException>(() => Error.Failure("A", "B").ShouldContainInnerError("Z"));
    }
}

