// Copyright © Erickson Lopez. MIT License.
using System;
using AwesomeAssertions;
using EricksonLopez.Result;
using EricksonLopez.Result.Testing;
using Xunit;

namespace EricksonLopez.Result.Tests.Core;

public class ResultAdditionalTests
{
    [Fact]
    public void DefaultResult_IsUninitialized_PropertiesReturnExpected()
    {
        Result r = default;
        r.IsUninitialized.Should().BeTrue();
        Assert.False(r.IsSuccess);
        Assert.False(r.IsFailure);

        // operator true/false
        bool isTrue = r ? true : false;
        isTrue.Should().BeFalse();

        if (r)
        {
            Assert.Fail("operator true on default returned true");
        }
    }

    [Fact]
    public void DefaultResultT_IsUninitialized_PropertiesReturnExpected()
    {
        Result<int> r = default;
        r.IsUninitialized.Should().BeTrue();
        Assert.False(r.IsSuccess);
        Assert.False(r.IsFailure);
    }

    [Fact]
    public void DefaultResult_ErrorProperty_ReturnsUninitializedError()
    {
        Result r = default;
        r.Error.Should().Be(WellKnownErrors.UninitializedError);
    }

    [Fact]
    public void Match_WhenUninitialized_ThrowsInvalidOperationException()
    {
        Result r = default;
        Action act = () => _ = r.Match(() => 1, e => 2);
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Match_WithState_WhenUninitialized_ThrowsInvalidOperationException()
    {
        Result r = default;
        Action act = () => _ = r.Match(0, (s) => 1, (s, e) => 2);
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Execute_WhenUninitialized_ThrowsInvalidOperationException()
    {
        Result r = default;
        Action act = () => r.Execute(() => { }, e => { });
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Execute_WithState_WhenUninitialized_ThrowsInvalidOperationException()
    {
        Result r = default;
        Action act = () => r.Execute(0, (s) => { }, (s, e) => { });
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void MapFailure_WhenUninitialized_ThrowsInvalidOperationException()
    {
        Result r = default;
        Action act = () => _ = r.MapFailure(e => 1, 0);
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void MapFailure_WithState_WhenUninitialized_ThrowsInvalidOperationException()
    {
        Result r = default;
        Action act = () => _ = r.MapFailure(0, (s, e) => 1, 0);
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void DefaultResultT_ErrorProperty_ReturnsUninitializedError()
    {
        Result<int> r = default;
        r.Error.Should().Be(WellKnownErrors.UninitializedError);
    }

    [Fact]
    public void Value_WhenUninitialized_ThrowsInvalidOperationException()
    {
        Result<int> r = default;
        Action act = () => _ = r.Value;
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Match_ResultT_WhenUninitialized_ThrowsInvalidOperationException()
    {
        Result<int> r = default;
        Action act = () => _ = r.Match(v => 1, e => 2);
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Match_ResultT_WithState_WhenUninitialized_ThrowsInvalidOperationException()
    {
        Result<int> r = default;
        Action act = () => _ = r.Match(0, (s, v) => 1, (s, e) => 2);
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Execute_ResultT_WhenUninitialized_ThrowsInvalidOperationException()
    {
        Result<int> r = default;
        Action act = () => r.Execute(v => { }, e => { });
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Execute_ResultT_WithState_WhenUninitialized_ThrowsInvalidOperationException()
    {
        Result<int> r = default;
        Action act = () => r.Execute(0, (s, v) => { }, (s, e) => { });
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void MapFailure_ResultT_WhenUninitialized_ThrowsInvalidOperationException()
    {
        Result<int> r = default;
        Action act = () => r.MapFailure(e => 1, 0);
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void MapFailure_ResultT_WithState_WhenUninitialized_ThrowsInvalidOperationException()
    {
        Result<int> r = default;
        Action act = () => r.MapFailure(0, (s, e) => 1, 0);
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void ImplicitOperator_FromError_ReturnsFailedResult()
    {
        Error e = Error.Failure("A", "B");
        Result r = e;
        Assert.False(r.IsSuccess);
        r.Error.Should().Be(e);

        Result<int> rt = e;
        rt.ShouldBeFailure();
        rt.Error.Should().Be(e);
    }

    [Fact]
    public void SuccessResult_ErrorProperty_ThrowsInvalidOperationException()
    {
        Result r = Result.Success();
        Action act = () => _ = r.Error;
        act.Should().Throw<InvalidOperationException>();

        Result<int> rt = Result.Success(42);
        Action act2 = () => _ = rt.Error;
        act2.Should().Throw<InvalidOperationException>();
    }
}
