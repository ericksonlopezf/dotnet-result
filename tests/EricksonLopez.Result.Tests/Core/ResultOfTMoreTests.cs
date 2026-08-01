using System;
using AwesomeAssertions;
using EricksonLopez.Result.Testing;
using Xunit;

namespace EricksonLopez.Result.Tests.Core;

public class ResultOfTMoreTests
{
    [Fact]
    public void MatchWithState_Success_ReturnsOnSuccess()
    {
        var result = Result.Success(10);
        var match = result.Match(5, (s, v) => s + v, (s, e) => 0);
        match.Should().Be(15);
    }

    [Fact]
    public void MatchWithState_Failure_ReturnsOnFailure()
    {
        var result = Result.Failure<int>(Error.Failure("e", "m"));
        var match = result.Match(5, (s, v) => s + v, (s, e) => s);
        match.Should().Be(5);
    }

    [Fact]
    public void SwitchWithState_Success_InvokesOnSuccess()
    {
        var result = Result.Success(10);
        bool success = false;
        result.Execute(5, (s, v) => success = true, (s, e) => { });
        success.Should().BeTrue();
    }

    [Fact]
    public void SwitchWithState_Failure_InvokesOnFailure()
    {
        var result = Result.Failure<int>(Error.Failure("e", "m"));
        bool failure = false;
        result.Execute(5, (s, v) => { }, (s, e) => failure = true);
        failure.Should().BeTrue();
    }

    [Fact]
    public void MapFailure_Success_ReturnsSuccessDefault()
    {
        var result = Result.Success(10);
        var match = result.MapFailure(e => 0, 100);
        match.Should().Be(100);
    }

    [Fact]
    public void MapFailure_Failure_ReturnsOnFailure()
    {
        var result = Result.Failure<int>(Error.Failure("e", "m"));
        var match = result.MapFailure(e => 0, 100);
        match.Should().Be(0);
    }
    
    [Fact]
    public void MapFailureWithState_Success_ReturnsSuccessDefault()
    {
        var result = Result.Success(10);
        var match = result.MapFailure(5, (s, e) => s * 2, 100);
        match.Should().Be(100);
    }

    [Fact]
    public void MapFailureWithState_Failure_ReturnsOnFailure()
    {
        var result = Result.Failure<int>(Error.Failure("e", "m"));
        var match = result.MapFailure(5, (s, e) => s * 2, 100);
        match.Should().Be(10);
    }

    [Fact]
    public void Map_Success_ReturnsSuccessValue()
    {
        var result = Result.Success(10);
        var map = result.Map(v => v * 2);
        map.ShouldBeSuccess().Should().Be(20);
    }

    [Fact]
    public void Map_Failure_ReturnsFailure()
    {
        var result = Result.Failure<int>(Error.Failure("e", "m"));
        var map = result.Map(v => v * 2);
        map.ShouldBeFailure().Code.Should().Be("e");
    }

    [Fact]
    public void MapWithState_Success_ReturnsSuccessValue()
    {
        var result = Result.Success(10);
        var map = result.Map(5, (s, v) => s * v);
        map.ShouldBeSuccess().Should().Be(50);
    }

    [Fact]
    public void MapWithState_Failure_ReturnsFailure()
    {
        var result = Result.Failure<int>(Error.Failure("e", "m"));
        var map = result.Map(5, (s, v) => s * v);
        map.ShouldBeFailure().Code.Should().Be("e");
    }

    [Fact]
    public void BindWithState_Success_ReturnsBoundResult()
    {
        var result = Result.Success(10);
        var bind = result.Bind(5, (s, v) => Result.Success(s + v));
        bind.ShouldBeSuccess().Should().Be(15);
    }

    [Fact]
    public void BindWithState_Failure_ReturnsFailure()
    {
        var result = Result.Failure<int>(Error.Failure("e", "m"));
        var bind = result.Bind(5, (s, v) => Result.Success(s + v));
        bind.ShouldBeFailure().Code.Should().Be("e");
    }

    [Fact]
    public void BindNonGeneric_Success_ReturnsBoundResult()
    {
        var result = Result.Success(10);
        var bind = result.Bind(v => Result.Success());
        bind.ShouldBeSuccess();
    }

    [Fact]
    public void BindNonGeneric_Failure_ReturnsFailure()
    {
        var result = Result.Failure<int>(Error.Failure("e", "m"));
        var bind = result.Bind(v => Result.Success());
        bind.ShouldBeFailure().Code.Should().Be("e");
    }

    [Fact]
    public void BindNonGenericWithState_Success_ReturnsBoundResult()
    {
        var result = Result.Success(10);
        var bind = result.Bind(5, (s, v) => Result.Success());
        bind.ShouldBeSuccess();
    }

    [Fact]
    public void BindNonGenericWithState_Failure_ReturnsFailure()
    {
        var result = Result.Failure<int>(Error.Failure("e", "m"));
        var bind = result.Bind(5, (s, v) => Result.Success());
        bind.ShouldBeFailure().Code.Should().Be("e");
    }

    [Fact]
    public void Tap_Success_InvokesAction()
    {
        var result = Result.Success(10);
        bool invoked = false;
        result.TapOnSuccess(v => invoked = true);
        invoked.Should().BeTrue();
    }

    [Fact]
    public void Tap_Failure_DoesNotInvokeAction()
    {
        var result = Result.Failure<int>(Error.Failure("e", "m"));
        bool invoked = false;
        result.TapOnSuccess(v => invoked = true);
        invoked.Should().BeFalse();
    }
    
    [Fact]
    public void TapWithState_Success_InvokesAction()
    {
        var result = Result.Success(10);
        bool invoked = false;
        result.TapOnSuccess(5, (s, v) => invoked = true);
        invoked.Should().BeTrue();
    }

    [Fact]
    public void TapWithState_Failure_DoesNotInvokeAction()
    {
        var result = Result.Failure<int>(Error.Failure("e", "m"));
        bool invoked = false;
        result.TapOnSuccess(5, (s, v) => invoked = true);
        invoked.Should().BeFalse();
    }

    [Fact]
    public void TapOnFailure_Failure_InvokesAction()
    {
        var result = Result.Failure<int>(Error.Failure("e", "m"));
        bool invoked = false;
        result.TapOnFailure(e => invoked = true);
        invoked.Should().BeTrue();
    }

    [Fact]
    public void TapOnFailure_Success_DoesNotInvokeAction()
    {
        var result = Result.Success(10);
        bool invoked = false;
        result.TapOnFailure(e => invoked = true);
        invoked.Should().BeFalse();
    }

    [Fact]
    public void TapErrorWithState_Failure_InvokesAction()
    {
        var result = Result.Failure<int>(Error.Failure("e", "m"));
        bool invoked = false;
        result.TapOnFailure(10, (s, e) => invoked = true);
        invoked.Should().BeTrue();
    }

    [Fact]
    public void TapErrorWithState_Success_DoesNotInvokeAction()
    {
        var result = Result.Success(10);
        bool invoked = false;
        result.TapOnFailure(10, (s, e) => invoked = true);
        invoked.Should().BeFalse();
    }

    [Fact]
    public void Ensure_Success_PredicateTrue_ReturnsOriginal()
    {
        var result = Result.Success(10);
        var ensure = result.Ensure(v => v == 10, Error.Failure("e", "m"));
        ensure.ShouldBeSuccess().Should().Be(10);
    }

    [Fact]
    public void Ensure_Success_PredicateFalse_ReturnsFailure()
    {
        var result = Result.Success(10);
        var ensure = result.Ensure(v => v != 10, Error.Failure("e", "m"));
        ensure.ShouldBeFailure().Code.Should().Be("e");
    }

    [Fact]
    public void Ensure_Failure_ReturnsFailure()
    {
        var result = Result.Failure<int>(Error.Failure("err", "m"));
        var ensure = result.Ensure(v => true, Error.Failure("e", "m"));
        ensure.ShouldBeFailure().Code.Should().Be("err");
    }
    
    [Fact]
    public void EnsureWithState_Success_PredicateTrue_ReturnsOriginal()
    {
        var result = Result.Success(10);
        var ensure = result.Ensure(10, (s, v) => s == v, Error.Failure("e", "m"));
        ensure.ShouldBeSuccess().Should().Be(10);
    }

    [Fact]
    public void EnsureWithState_Success_PredicateFalse_ReturnsFailure()
    {
        var result = Result.Success(10);
        var ensure = result.Ensure(10, (s, v) => s != v, Error.Failure("e", "m"));
        ensure.ShouldBeFailure().Code.Should().Be("e");
    }

    [Fact]
    public void EnsureWithState_Failure_ReturnsFailure()
    {
        var result = Result.Failure<int>(Error.Failure("err", "m"));
        var ensure = result.Ensure(10, (s, v) => true, Error.Failure("e", "m"));
        ensure.ShouldBeFailure().Code.Should().Be("err");
    }

    [Fact]
    public void EnsureWithFactory_Success_PredicateFalse_ReturnsFailure()
    {
        var result = Result.Success(10);
        var ensure = result.Ensure(v => false, () => Error.Failure("e", "m"));
        ensure.ShouldBeFailure().Code.Should().Be("e");
    }
    
    [Fact]
    public void EnsureWithStateFactory_Success_PredicateFalse_ReturnsFailure()
    {
        var result = Result.Success(10);
        var ensure = result.Ensure(10, (s, v) => false, () => Error.Failure("e", "m"));
        ensure.ShouldBeFailure().Code.Should().Be("e");
    }

    [Fact]
    public void Inspect_Success_InvokesAction()
    {
        var result = Result.Success(10);
        bool invoked = false;
        result.Inspect(r => invoked = true);
        invoked.Should().BeTrue();
    }

    [Fact]
    public void InspectWithState_Success_InvokesAction()
    {
        var result = Result.Success(10);
        bool invoked = false;
        result.Inspect(10, (s, r) => invoked = true);
        invoked.Should().BeTrue();
    }

    [Fact]
    public void Recover_Failure_ReturnsRecoveredResult()
    {
        var result = Result.Failure<int>(Error.Failure("e", "m"));
        var recover = result.Recover(e => Result.Success(10));
        recover.ShouldBeSuccess().Should().Be(10);
    }

    [Fact]
    public void Recover_Success_ReturnsOriginal()
    {
        var result = Result.Success(10);
        var recover = result.Recover(e => Result.Failure<int>(Error.Failure("e", "m")));
        recover.ShouldBeSuccess().Should().Be(10);
    }

    [Fact]
    public void RecoverWithState_Failure_ReturnsRecoveredResult()
    {
        var result = Result.Failure<int>(Error.Failure("e", "m"));
        var recover = result.Recover(10, (s, e) => Result.Success(s));
        recover.ShouldBeSuccess().Should().Be(10);
    }

    [Fact]
    public void MapError_Failure_ReturnsMappedError()
    {
        var result = Result.Failure<int>(Error.Failure("e", "m"));
        var mapped = result.MapError(e => Error.Failure("e2", "m2"));
        mapped.ShouldBeFailure().Code.Should().Be("e2");
    }
    
    [Fact]
    public void MapError_Success_ReturnsOriginal()
    {
        var result = Result.Success(10);
        var mapped = result.MapError(e => Error.Failure("e2", "m2"));
        mapped.ShouldBeSuccess();
    }
    
    [Fact]
    public void MapErrorWithState_Failure_ReturnsMappedError()
    {
        var result = Result.Failure<int>(Error.Failure("e", "m"));
        var mapped = result.MapError(10, (s, e) => Error.Failure("e2", "m2"));
        mapped.ShouldBeFailure().Code.Should().Be("e2");
    }

    [Fact]
    public void MapErrorWithState_Success_ReturnsOriginal()
    {
        var result = Result.Success(10);
        var mapped = result.MapError(10, (s, e) => Error.Failure("e2", "m2"));
        mapped.ShouldBeSuccess();
    }

    [Fact]
    public void TryGetValue_Success_ReturnsTrueAndValue()
    {
        var result = Result.Success(10);
        var hasValue = result.TryGetValue(out var value);
        hasValue.Should().BeTrue();
        value.Should().Be(10);
    }

    [Fact]
    public void TryGetValue_Failure_ReturnsFalseAndDefault()
    {
        var result = Result.Failure<int>(Error.Failure("e", "m"));
        var hasValue = result.TryGetValue(out var value);
        hasValue.Should().BeFalse();
        value.Should().Be(0);
    }

    [Fact]
    public void GetValueOrDefault_Success_ReturnsValue()
    {
        var result = Result.Success(10);
        result.GetValueOrDefault(5).Should().Be(10);
    }

    [Fact]
    public void GetValueOrDefault_Failure_ReturnsDefault()
    {
        var result = Result.Failure<int>(Error.Failure("e", "m"));
        result.GetValueOrDefault(5).Should().Be(5);
    }

    [Fact]
    public void GetValueOrFallback_Success_ReturnsValue()
    {
        var result = Result.Success(10);
        result.GetValueOrFallback(e => 5).Should().Be(10);
    }

    [Fact]
    public void GetValueOrFallback_Failure_ReturnsFallback()
    {
        var result = Result.Failure<int>(Error.Failure("e", "m"));
        result.GetValueOrFallback(e => 5).Should().Be(5);
    }
    
    [Fact]
    public void GetValueOrFallbackWithState_Success_ReturnsValue()
    {
        var result = Result.Success(10);
        result.GetValueOrFallback(5, (s, e) => s).Should().Be(10);
    }

    [Fact]
    public void GetValueOrFallbackWithState_Failure_ReturnsFallback()
    {
        var result = Result.Failure<int>(Error.Failure("e", "m"));
        result.GetValueOrFallback(5, (s, e) => s).Should().Be(5);
    }

    [Fact]
    public void DiscardValue_Success_ReturnsSuccess()
    {
        var result = Result.Success(10);
        var discard = result.DiscardValue();
        discard.ShouldBeSuccess();
    }

    [Fact]
    public void DiscardValue_Failure_ReturnsFailure()
    {
        var result = Result.Failure<int>(Error.Failure("e", "m"));
        var discard = result.DiscardValue();
        discard.ShouldBeFailure().Code.Should().Be("e");
    }

    [Fact]
    public void Deconstruct3_Success_ReturnsTrueValueAndNull()
    {
        var result = Result.Success(10);
        var (isSuccess, value, error) = result;
        isSuccess.Should().BeTrue();
        value.Should().Be(10);
        error.Should().BeNull();
    }

    [Fact]
    public void Deconstruct3_Failure_ReturnsFalseDefaultAndError()
    {
        var result = Result.Failure<int>(Error.Failure("e", "m"));
        var (isSuccess, value, error) = result;
        isSuccess.Should().BeFalse();
        value.Should().Be(0);
        error!.Code.Should().Be("e");
    }

    [Fact]
    public void Deconstruct2_Success_ReturnsTrueAndNull()
    {
        var result = Result.Success(10);
        var (isSuccess, error) = result;
        isSuccess.Should().BeTrue();
        error.Should().BeNull();
    }

    [Fact]
    public void Deconstruct2_Failure_ReturnsFalseAndError()
    {
        var result = Result.Failure<int>(Error.Failure("e", "m"));
        var (isSuccess, error) = result;
        isSuccess.Should().BeFalse();
        error!.Code.Should().Be("e");
    }

    [Fact]
    public void Equals_SameResult_ReturnsTrue()
    {
        var r1 = Result.Success(10);
        var r2 = Result.Success(10);
        r1.Equals(r2).Should().BeTrue();
        r1.Equals((object)r2).Should().BeTrue();
        (r1 == r2).Should().BeTrue();
        (r1 != r2).Should().BeFalse();
    }

    [Fact]
    public void Equals_DifferentResult_ReturnsFalse()
    {
        var r1 = Result.Success(10);
        var r2 = Result.Failure<int>(Error.Failure("e", "m"));
        var r3 = Result.Success(5);
        r1.Equals(r2).Should().BeFalse();
        r1.Equals(r3).Should().BeFalse();
        r1.Equals((object)r2).Should().BeFalse();
        (r1 == r2).Should().BeFalse();
        (r1 != r2).Should().BeTrue();
    }

    [Fact]
    public void Equals_NullObject_ReturnsFalse()
    {
        var r1 = Result.Success(10);
        r1.Equals((object?)null).Should().BeFalse();
    }
}
