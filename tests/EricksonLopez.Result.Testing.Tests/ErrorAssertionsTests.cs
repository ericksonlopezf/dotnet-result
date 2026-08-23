// Copyright © Erickson Lopez. MIT License.
using System;
using EricksonLopez.Result;
using EricksonLopez.Result.Testing;
using Xunit;

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
        var ex = Assert.Throws<ResultAssertionException>(() => error.ShouldHaveErrorCode("Code2"));
        Assert.Equal("Expected error code 'Code2', but got 'Code1'.", ex.Message);
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
        var ex = Assert.Throws<ResultAssertionException>(() => error.ShouldHaveErrorType(ErrorType.Validation));
        Assert.Equal("Expected ErrorType 'Validation', but got 'Failure'.", ex.Message);
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
        var ex = Assert.Throws<ResultAssertionException>(() => error.ShouldHaveSeverity(ErrorSeverity.Error));
        Assert.Equal("Expected ErrorSeverity 'Error', but got 'Critical'.", ex.Message);
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
        var ex = Assert.Throws<ResultAssertionException>(() => error.ShouldHaveRetryability(ErrorRetryability.Permanent));
        Assert.Equal("Expected ErrorRetryability 'Permanent', but got 'Transient'.", ex.Message);
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
        var ex = Assert.Throws<ResultAssertionException>(() => error.ShouldHaveDescription("C"));
        Assert.Equal("Expected error Description to be 'C', but got 'B'.", ex.Message);
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
        var ex1 = Assert.Throws<ResultAssertionException>(() => error.ShouldHaveMetadata("K", "V2"));
        Assert.Equal("Expected metadata key 'K' with value 'V2', but got 'V'.", ex1.Message);

        var ex2 = Assert.Throws<ResultAssertionException>(() => error.ShouldHaveMetadata("K2", "V"));
        Assert.Equal("Expected metadata key 'K2' with value 'V', but got ''.", ex2.Message);

        var ex3 = Assert.Throws<ResultAssertionException>(() => error.ShouldHaveMetadataKey("K2"));
        Assert.Equal("Expected metadata to contain key 'K2', but it was not found.", ex3.Message);
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
        var ex = Assert.Throws<ResultAssertionException>(() => error.ShouldHaveTraceId("T2"));
        Assert.Equal("Expected error TraceId to be 'T2', but got 'T1'.", ex.Message);
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
        var ex = Assert.Throws<ResultAssertionException>(() => error.ShouldHaveCorrelationId("C2"));
        Assert.Equal("Expected error CorrelationId to be 'C2', but got 'C1'.", ex.Message);
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
        var ex = Assert.Throws<ResultAssertionException>(() => error.ShouldBeRetryable());
        Assert.Equal("Expected error to be Transient retryable, but got 'Permanent'.", ex.Message);
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
        var ex = Assert.Throws<ResultAssertionException>(() => error.ShouldBePermanent());
        Assert.Equal("Expected error to be Permanent (not retryable), but got 'Transient'.", ex.Message);
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
        var ex = Assert.Throws<ResultAssertionException>(() => error.ShouldHaveInnerErrors(0));
        Assert.Equal("Expected 0 inner errors, but got 1.", ex.Message);
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
        var ex1 = Assert.Throws<ResultAssertionException>(() => error.ShouldContainInnerError("Z"));
        Assert.Equal("Expected at least one inner error with code 'Z', but none was found.", ex1.Message);

        var ex2 = Assert.Throws<ResultAssertionException>(() => Error.Failure("A", "B").ShouldContainInnerError("Z"));
        Assert.Equal("Expected at least one inner error with code 'Z', but error has no inner errors.", ex2.Message);
    }

    [Fact]
    public void ShouldHaveNoInnerErrors_Success()
    {
        var error = Error.Failure("A", "B");
        error.ShouldHaveNoInnerErrors();
    }

    [Fact]
    public void ShouldHaveNoInnerErrors_Throws()
    {
        var error = Error.Failure("A", "B", Error.Failure("X", "Y"));
        var ex = Assert.Throws<ResultAssertionException>(() => error.ShouldHaveNoInnerErrors());
        Assert.Equal("Expected no inner errors, but found 1.", ex.Message);
    }

    [Fact]
    public void ShouldHaveTraceId_Throws_WhenNull()
    {
        var error = Error.Failure("A", "B");
        var ex = Assert.Throws<ResultAssertionException>(() => error.ShouldHaveTraceId("T1"));
        Assert.Equal("Expected error TraceId to be 'T1', but got '<null>'.", ex.Message);
    }

    [Fact]
    public void ShouldHaveCorrelationId_Throws_WhenNull()
    {
        var error = Error.Failure("A", "B");
        var ex = Assert.Throws<ResultAssertionException>(() => error.ShouldHaveCorrelationId("C1"));
        Assert.Equal("Expected error CorrelationId to be 'C1', but got '<null>'.", ex.Message);
    }
}


