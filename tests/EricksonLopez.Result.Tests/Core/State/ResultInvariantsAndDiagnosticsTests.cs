// Copyright © Erickson Lopez. MIT License.
using System;
using System.Reflection;
using AwesomeAssertions;
using EricksonLopez.Result;
using EricksonLopez.Result.Testing;
using Xunit;

namespace EricksonLopez.Result.Tests.Core.State;

public class ResultInvariantsAndDiagnosticsTests
{
    private static readonly Error TestError = Error.Failure("E", "Test error message");

    [Fact]
    public void Error_WhenResultIsSuccess_ThrowsInvalidOperationException()
    {
        var r = Result.Success();
        var act = () => _ = r.Error;
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Execute_WhenResultIsFailure_InvokesFailureCallbackOnly()
    {
        var r = Result.Failure(TestError);
        bool successCalled = false;
        bool failureCalled = false;

        r.Execute(() => successCalled = true, e => failureCalled = e == TestError);

        successCalled.Should().BeFalse();
        failureCalled.Should().BeTrue();
    }

    [Fact]
    public void IResultOutcome_Properties_ExposeExpectedState()
    {
        IResultOutcome rSuccess = Result.Success();
        rSuccess.Error.Should().BeNull();
        rSuccess.RawValue.Should().BeNull();
        rSuccess.IsSuccess.Should().BeTrue();

        IResultOutcome rFailure = Result.Failure(TestError);
        rFailure.Error.Should().BeSameAs(TestError);
        rFailure.RawValue.Should().BeNull();
        rFailure.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void GetDebuggerDisplay_WhenCalledOnDifferentStates_ReturnsFormattedStrings()
    {
        var method = typeof(Result).GetMethod("GetDebuggerDisplay", BindingFlags.NonPublic | BindingFlags.Instance);
        method.Should().NotBeNull();

        var success = Result.Success();
        method!.Invoke(success, null).Should().Be("Success");

        var failure = Result.Failure(TestError);
        method.Invoke(failure, null).Should().Be("Failure (E)");

        var uninit = default(Result);
        method.Invoke(uninit, null).Should().Be("Uninitialized");
    }

    [Fact]
    public void Value_WhenResultOfTIsFailure_ThrowsInvalidOperationException()
    {
        var r = Result.Failure<int>(TestError);
        var act = () => _ = r.Value;
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Error_WhenResultOfTIsSuccess_ThrowsInvalidOperationException()
    {
        var r = Result.Success(42);
        var act = () => _ = r.Error;
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Execute_WhenResultOfTIsFailure_InvokesFailureCallbackOnly()
    {
        var r = Result.Failure<int>(TestError);
        bool successCalled = false;
        bool failureCalled = false;

        r.Execute(v => successCalled = true, e => failureCalled = e == TestError);

        successCalled.Should().BeFalse();
        failureCalled.Should().BeTrue();
    }

    [Fact]
    public void TryGetValue_WhenCalledOnSuccessAndFailure_ReturnsExpectedOutParameters()
    {
        var rSuccess = Result.Success(42);
        rSuccess.TryGetValue(out var val1).Should().BeTrue();
        val1.Should().Be(42);

        var rFailure = Result.Failure<int>(TestError);
        rFailure.TryGetValue(out var val2).Should().BeFalse();
        val2.Should().Be(default);
    }

    [Fact]
    public void GetValueOrDefault_WhenCalledOnSuccessAndFailure_ReturnsExpectedValues()
    {
        var rSuccess = Result.Success(42);
        rSuccess.GetValueOrDefault(100).Should().Be(42);

        var rFailure = Result.Failure<int>(TestError);
        rFailure.GetValueOrDefault(100).Should().Be(100);
    }

    [Fact]
    public void Recover_WhenResultOfTIsSuccess_ReturnsOriginalValue()
    {
        var r = Result.Success(42);
        var r2 = r.Recover(e => Result.Success(100));
        r2.ShouldBeSuccess().Should().Be(42);
    }

    [Fact]
    public void DiscardValue_WhenCalled_ConvertsToNonGenericResultMaintainingState()
    {
        var rSuccess = Result.Success(42);
        var dr = rSuccess.DiscardValue();
        dr.ShouldBeSuccess();

        var rFailure = Result.Failure<int>(TestError);
        var dr2 = rFailure.DiscardValue();
        dr2.ShouldBeFailure().Should().BeSameAs(TestError);
    }

    [Fact]
    public void Deconstruct_WhenCalledOnSuccessAndFailure_DeconstructsCorrectly()
    {
        var rSuccess = Result.Success(42);
        rSuccess.Deconstruct(out bool s1, out Error? e1);
        s1.Should().BeTrue();
        e1.Should().BeNull();

        var rFailure = Result.Failure<int>(TestError);
        rFailure.Deconstruct(out bool s2, out Error? e2);
        s2.Should().BeFalse();
        e2.Should().BeSameAs(TestError);
    }

    [Fact]
    public void EqualsAndGetHashCode_WhenUninitialized_BehavesDeterministically()
    {
        var u1 = default(Result<int>);
        var u2 = default(Result<int>);
        var s = Result.Success(42);

        u1.Equals(u2).Should().BeTrue();
        u1.Equals(s).Should().BeFalse();
        u1.GetHashCode().Should().Be(u2.GetHashCode());
        u1.GetHashCode().Should().NotBe(0);
    }

    [Fact]
    public void GetHashCode_WhenResultOfTIsFailure_ProducesNonZeroHash()
    {
        var result = Result.Failure<int>(TestError);
        result.GetHashCode().Should().NotBe(0);
    }

    [Fact]
    public void GetHashCode_WhenResultOfTSuccessWithNullValue_ProducesNonZeroHash()
    {
        var success = Result<string>.Success(null!);
        success.GetHashCode().Should().NotBe(0);
    }

    [Fact]
    public void Failure_WhenResultOfTWithNullError_ThrowsArgumentNullException()
    {
        var act = () => Result<string>.Failure(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Ensure_WhenUsingErrorFactory_ReturnsExpectedResult()
    {
        var success = Result.Success(42);

        var r1 = success.Ensure(x => true, () => Error.Failure("1", "1"));
        r1.ShouldBeSuccess().Should().Be(42);

        var r2 = success.Ensure(x => false, () => Error.Failure("1", "1"));
        r2.ShouldBeFailure().Code.Should().Be("1");

        var r3 = success.Ensure("state", (s, x) => true, () => Error.Failure("1", "1"));
        r3.ShouldBeSuccess().Should().Be(42);

        var r4 = success.Ensure("state", (s, x) => false, () => Error.Failure("1", "1"));
        r4.ShouldBeFailure().Code.Should().Be("1");
    }

    [Fact]
    public void GetDebuggerDisplay_WhenCalledOnResultOfT_ReturnsFormattedStrings()
    {
        var method = typeof(Result<int>).GetMethod("GetDebuggerDisplay", BindingFlags.NonPublic | BindingFlags.Instance);
        method.Should().NotBeNull();

        var success = Result.Success(42);
        method!.Invoke(success, null).Should().Be("Success (42)");

        var failure = Result.Failure<int>(TestError);
        method.Invoke(failure, null).Should().Be("Failure (E)");

        var uninit = default(Result<int>);
        method.Invoke(uninit, null).Should().Be("Uninitialized");
    }

    [Fact]
    public void DefensiveBranch_WhenStateIsCorrupted_HandledDefensively()
    {
        var type = typeof(Result);
        object boxed = new Result();
        type.GetField("_state", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(boxed, (byte)2);
        var badResult = (Result)boxed;

        badResult.TryGetError(out _).Should().BeTrue();
        badResult.TryGetError(out _, out _).Should().BeTrue();

        var getDebuggerDisplay2 = type.GetMethod("GetDebuggerDisplay", BindingFlags.Instance | BindingFlags.NonPublic);
        var disp1 = getDebuggerDisplay2!.Invoke(badResult, null);
        disp1.Should().Be("Failure ()");

        object boxedT = new Result<int>();
        var typeT = typeof(Result<int>);
        typeT.GetField("_state", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(boxedT, (byte)2);
        var badResultT = (Result<int>)boxedT;

        badResultT.GetHashCode().Should().NotBe(0);
        var getDebuggerDisplayT = typeT.GetMethod("GetDebuggerDisplay", BindingFlags.Instance | BindingFlags.NonPublic);
        var dispT = getDebuggerDisplayT!.Invoke(badResultT, null);
        dispT.Should().Be("Failure ()");
    }
}
