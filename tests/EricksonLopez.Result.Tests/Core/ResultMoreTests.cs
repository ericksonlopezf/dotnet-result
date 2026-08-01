using System;
using System.Threading.Tasks;
using AwesomeAssertions;
using EricksonLopez.Result.Testing;
using Xunit;

namespace EricksonLopez.Result.Tests.Core;

public class ResultMoreTests
{
    [Fact]
    public void MatchWithState_Success_ReturnsOnSuccess()
    {
        var result = Result.Success();
        var match = result.Match(10, s => s * 2, (s, e) => 0);
        match.Should().Be(20);
    }

    [Fact]
    public void MatchWithState_Failure_ReturnsOnFailure()
    {
        var result = Result.Failure(Error.Failure("e", "m"));
        var match = result.Match(10, s => s * 2, (s, e) => s);
        match.Should().Be(10);
    }

    [Fact]
    public void SwitchWithState_Success_InvokesOnSuccess()
    {
        var result = Result.Success();
        bool success = false;
        result.Execute(10, s => success = true, (s, e) => { });
        success.Should().BeTrue();
    }

    [Fact]
    public void SwitchWithState_Failure_InvokesOnFailure()
    {
        var result = Result.Failure(Error.Failure("e", "m"));
        bool failure = false;
        result.Execute(10, s => { }, (s, e) => failure = true);
        failure.Should().BeTrue();
    }

    [Fact]
    public void MapFailure_Success_ReturnsSuccessDefault()
    {
        var result = Result.Success();
        var match = result.MapFailure(e => 0, 10);
        match.Should().Be(10);
    }

    [Fact]
    public void MapFailure_Failure_ReturnsOnFailure()
    {
        var result = Result.Failure(Error.Failure("e", "m"));
        var match = result.MapFailure(e => 0, 10);
        match.Should().Be(0);
    }
    
    [Fact]
    public void MapFailureWithState_Success_ReturnsSuccessDefault()
    {
        var result = Result.Success();
        var match = result.MapFailure(5, (s, e) => s * 2, 10);
        match.Should().Be(10);
    }

    [Fact]
    public void MapFailureWithState_Failure_ReturnsOnFailure()
    {
        var result = Result.Failure(Error.Failure("e", "m"));
        var match = result.MapFailure(5, (s, e) => s * 2, 10);
        match.Should().Be(10);
    }

    [Fact]
    public void Map_Success_ReturnsSuccessValue()
    {
        var result = Result.Success();
        var map = result.Map(() => 10);
        map.ShouldBeSuccess().Should().Be(10);
    }

    [Fact]
    public void Map_Failure_ReturnsFailure()
    {
        var result = Result.Failure(Error.Failure("e", "m"));
        var map = result.Map(() => 10);
        map.ShouldBeFailure().Code.Should().Be("e");
    }

    [Fact]
    public void MapWithState_Success_ReturnsSuccessValue()
    {
        var result = Result.Success();
        var map = result.Map(10, s => s * 2);
        map.ShouldBeSuccess().Should().Be(20);
    }

    [Fact]
    public void MapWithState_Failure_ReturnsFailure()
    {
        var result = Result.Failure(Error.Failure("e", "m"));
        var map = result.Map(10, s => s * 2);
        map.ShouldBeFailure().Code.Should().Be("e");
    }

    [Fact]
    public void BindWithState_Success_ReturnsBoundResult()
    {
        var result = Result.Success();
        var bind = result.Bind(10, s => Result.Success());
        bind.ShouldBeSuccess();
    }

    [Fact]
    public void BindWithState_Failure_ReturnsFailure()
    {
        var result = Result.Failure(Error.Failure("e", "m"));
        var bind = result.Bind(10, s => Result.Success());
        bind.ShouldBeFailure().Code.Should().Be("e");
    }

    [Fact]
    public void BindGeneric_Success_ReturnsBoundResult()
    {
        var result = Result.Success();
        var bind = result.Bind(() => Result.Success(10));
        bind.ShouldBeSuccess().Should().Be(10);
    }

    [Fact]
    public void BindGeneric_Failure_ReturnsFailure()
    {
        var result = Result.Failure(Error.Failure("e", "m"));
        var bind = result.Bind(() => Result.Success(10));
        bind.ShouldBeFailure().Code.Should().Be("e");
    }

    [Fact]
    public void BindGenericWithState_Success_ReturnsBoundResult()
    {
        var result = Result.Success();
        var bind = result.Bind(10, s => Result.Success(s * 2));
        bind.ShouldBeSuccess().Should().Be(20);
    }

    [Fact]
    public void BindGenericWithState_Failure_ReturnsFailure()
    {
        var result = Result.Failure(Error.Failure("e", "m"));
        var bind = result.Bind(10, s => Result.Success(s * 2));
        bind.ShouldBeFailure().Code.Should().Be("e");
    }

    [Fact]
    public void Tap_Success_InvokesAction()
    {
        var result = Result.Success();
        bool invoked = false;
        result.TapOnSuccess(() => invoked = true);
        invoked.Should().BeTrue();
    }

    [Fact]
    public void Tap_Failure_DoesNotInvokeAction()
    {
        var result = Result.Failure(Error.Failure("e", "m"));
        bool invoked = false;
        result.TapOnSuccess(() => invoked = true);
        invoked.Should().BeFalse();
    }
    
    [Fact]
    public void TapWithState_Success_InvokesAction()
    {
        var result = Result.Success();
        bool invoked = false;
        result.TapOnSuccess(10, s => invoked = true);
        invoked.Should().BeTrue();
    }

    [Fact]
    public void TapWithState_Failure_DoesNotInvokeAction()
    {
        var result = Result.Failure(Error.Failure("e", "m"));
        bool invoked = false;
        result.TapOnSuccess(10, s => invoked = true);
        invoked.Should().BeFalse();
    }

    [Fact]
    public void TapOnFailure_Failure_InvokesAction()
    {
        var result = Result.Failure(Error.Failure("e", "m"));
        bool invoked = false;
        result.TapOnFailure(e => invoked = true);
        invoked.Should().BeTrue();
    }

    [Fact]
    public void TapOnFailure_Success_DoesNotInvokeAction()
    {
        var result = Result.Success();
        bool invoked = false;
        result.TapOnFailure(e => invoked = true);
        invoked.Should().BeFalse();
    }

    [Fact]
    public void TapErrorWithState_Failure_InvokesAction()
    {
        var result = Result.Failure(Error.Failure("e", "m"));
        bool invoked = false;
        result.TapOnFailure(10, (s, e) => invoked = true);
        invoked.Should().BeTrue();
    }

    [Fact]
    public void TapErrorWithState_Success_DoesNotInvokeAction()
    {
        var result = Result.Success();
        bool invoked = false;
        result.TapOnFailure(10, (s, e) => invoked = true);
        invoked.Should().BeFalse();
    }

    [Fact]
    public void Ensure_Success_PredicateTrue_ReturnsOriginal()
    {
        var result = Result.Success();
        var ensure = result.Ensure(() => true, Error.Failure("e", "m"));
        ensure.ShouldBeSuccess();
    }

    [Fact]
    public void Ensure_Success_PredicateFalse_ReturnsFailure()
    {
        var result = Result.Success();
        var ensure = result.Ensure(() => false, Error.Failure("e", "m"));
        ensure.ShouldBeFailure().Code.Should().Be("e");
    }

    [Fact]
    public void Ensure_Failure_ReturnsFailure()
    {
        var result = Result.Failure(Error.Failure("err", "m"));
        var ensure = result.Ensure(() => true, Error.Failure("e", "m"));
        ensure.ShouldBeFailure().Code.Should().Be("err");
    }
    
    [Fact]
    public void EnsureWithState_Success_PredicateTrue_ReturnsOriginal()
    {
        var result = Result.Success();
        var ensure = result.Ensure(10, s => s == 10, Error.Failure("e", "m"));
        ensure.ShouldBeSuccess();
    }

    [Fact]
    public void EnsureWithState_Success_PredicateFalse_ReturnsFailure()
    {
        var result = Result.Success();
        var ensure = result.Ensure(10, s => s != 10, Error.Failure("e", "m"));
        ensure.ShouldBeFailure().Code.Should().Be("e");
    }

    [Fact]
    public void EnsureWithState_Failure_ReturnsFailure()
    {
        var result = Result.Failure(Error.Failure("err", "m"));
        var ensure = result.Ensure(10, s => true, Error.Failure("e", "m"));
        ensure.ShouldBeFailure().Code.Should().Be("err");
    }

    [Fact]
    public void EnsureWithFactory_Success_PredicateFalse_ReturnsFailure()
    {
        var result = Result.Success();
        var ensure = result.Ensure(() => false, () => Error.Failure("e", "m"));
        ensure.ShouldBeFailure().Code.Should().Be("e");
    }
    
    [Fact]
    public void EnsureWithStateFactory_Success_PredicateFalse_ReturnsFailure()
    {
        var result = Result.Success();
        var ensure = result.Ensure(10, s => false, () => Error.Failure("e", "m"));
        ensure.ShouldBeFailure().Code.Should().Be("e");
    }

    [Fact]
    public void Inspect_Success_InvokesAction()
    {
        var result = Result.Success();
        bool invoked = false;
        result.Inspect(r => invoked = true);
        invoked.Should().BeTrue();
    }

    [Fact]
    public void InspectWithState_Success_InvokesAction()
    {
        var result = Result.Success();
        bool invoked = false;
        result.Inspect(10, (s, r) => invoked = true);
        invoked.Should().BeTrue();
    }

    [Fact]
    public void Recover_Failure_ReturnsRecoveredResult()
    {
        var result = Result.Failure(Error.Failure("e", "m"));
        var recover = result.Recover(e => Result.Success());
        recover.ShouldBeSuccess();
    }

    [Fact]
    public void Recover_Success_ReturnsOriginal()
    {
        var result = Result.Success();
        var recover = result.Recover(e => Result.Failure(Error.Failure("e", "m")));
        recover.ShouldBeSuccess();
    }

    [Fact]
    public void RecoverWithState_Failure_ReturnsRecoveredResult()
    {
        var result = Result.Failure(Error.Failure("e", "m"));
        var recover = result.Recover(10, (s, e) => Result.Success());
        recover.ShouldBeSuccess();
    }

    [Fact]
    public void Deconstruct_Success_ReturnsTrueAndNull()
    {
        var result = Result.Success();
        var (isSuccess, error) = result;
        isSuccess.Should().BeTrue();
        error.Should().BeNull();
    }

    [Fact]
    public void Deconstruct_Failure_ReturnsFalseAndError()
    {
        var result = Result.Failure(Error.Failure("e", "m"));
        var (isSuccess, error) = result;
        isSuccess.Should().BeFalse();
        error!.Code.Should().Be("e");
    }
    
    [Fact]
    public void MapError_Failure_ReturnsMappedError()
    {
        var result = Result.Failure(Error.Failure("e", "m"));
        var mapped = result.MapError(e => Error.Failure("e2", "m2"));
        mapped.ShouldBeFailure().Code.Should().Be("e2");
    }
    
    [Fact]
    public void MapError_Success_ReturnsOriginal()
    {
        var result = Result.Success();
        var mapped = result.MapError(e => Error.Failure("e2", "m2"));
        mapped.ShouldBeSuccess();
    }
    
    [Fact]
    public void MapErrorWithState_Failure_ReturnsMappedError()
    {
        var result = Result.Failure(Error.Failure("e", "m"));
        var mapped = result.MapError(10, (s, e) => Error.Failure("e2", "m2"));
        mapped.ShouldBeFailure().Code.Should().Be("e2");
    }

    [Fact]
    public void MapErrorWithState_Success_ReturnsOriginal()
    {
        var result = Result.Success();
        var mapped = result.MapError(10, (s, e) => Error.Failure("e2", "m2"));
        mapped.ShouldBeSuccess();
    }

    [Fact]
    public void Equals_SameResult_ReturnsTrue()
    {
        var r1 = Result.Success();
        var r2 = Result.Success();
        r1.Equals(r2).Should().BeTrue();
        r1.Equals((object)r2).Should().BeTrue();
        (r1 == r2).Should().BeTrue();
        (r1 != r2).Should().BeFalse();
    }

    [Fact]
    public void Equals_DifferentResult_ReturnsFalse()
    {
        var r1 = Result.Success();
        var r2 = Result.Failure(Error.Failure("e", "m"));
        r1.Equals(r2).Should().BeFalse();
        r1.Equals((object)r2).Should().BeFalse();
        (r1 == r2).Should().BeFalse();
        (r1 != r2).Should().BeTrue();
    }

    [Fact]
    public void Equals_NullObject_ReturnsFalse()
    {
        var r1 = Result.Success();
        r1.Equals((object?)null).Should().BeFalse();
    }
}
