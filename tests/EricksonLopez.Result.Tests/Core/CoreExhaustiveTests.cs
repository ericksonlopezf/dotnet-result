using System;
using System.Threading.Tasks;
using Xunit;

namespace EricksonLopez.Result.Tests.Core;

public class CoreExhaustiveTests
{
    [Fact]
    public void Result_Match_Switch_Tap_Recover_Ensure_MapError_Inspect_State_Overrides()
    {
        var success = Result.Success();
        var failure = Result.Failure(Error.Failure("code", "msg"));
        var state = 42;

        Assert.Equal(1, success.Match(() => 1, e => 0));
        Assert.Equal(0, failure.Match(() => 1, e => 0));
        Assert.Equal(42, success.Match(state, (s) => s, (s, e) => 0));
        Assert.Equal(0, failure.Match(state, (s) => s, (s, e) => 0));

        bool called = false; _ = called;
        success.Execute(() => called = true, e => { });
        failure.Execute(() => { }, e => called = true);
        success.Execute(state, (s) => called = true, (s, e) => { });
        failure.Execute(state, (s) => { }, (s, e) => called = true);

        success.TapOnSuccess(() => called = true);
        success.TapOnSuccess(state, s => called = true);
        failure.TapOnFailure(e => called = true);
        failure.TapOnFailure(state, (s, e) => called = true);

        Assert.Equal("new", failure.MapError(e => Error.Failure("new", "new")).Error.Code);
        Assert.Equal("new", failure.MapError(state, (s, e) => Error.Failure("new", "new")).Error.Code);
        Assert.True(success.MapError(e => Error.Failure("new", "new")).IsSuccess);
        Assert.True(success.MapError(state, (s, e) => Error.Failure("new", "new")).IsSuccess);

        success.Inspect(r => called = true);
        success.Inspect(state, (s, r) => called = true);

        Assert.True(failure.Recover(e => Result.Success()).IsSuccess);
        Assert.True(failure.Recover(state, (s, e) => Result.Success()).IsSuccess);
        Assert.True(success.Recover(e => Result.Failure(Error.Failure("X", "X"))).IsSuccess);
        Assert.True(success.Recover(state, (s, e) => Result.Failure(Error.Failure("X", "X"))).IsSuccess);

        Assert.True(success.Ensure(() => true, Error.Failure("X", "X")).IsSuccess);
        Assert.False(success.Ensure(() => false, Error.Failure("X", "X")).IsSuccess);
        Assert.False(failure.Ensure(() => true, Error.Failure("X", "X")).IsSuccess);
        Assert.True(success.Ensure(state, s => true, Error.Failure("X", "X")).IsSuccess);
        Assert.False(success.Ensure(state, s => false, Error.Failure("X", "X")).IsSuccess);
        Assert.False(failure.Ensure(state, s => true, Error.Failure("X", "X")).IsSuccess);

        Assert.True(success.Bind(() => Result.Success()).IsSuccess);
        Assert.False(success.Bind(() => Result.Failure(Error.Failure("X", "X"))).IsSuccess);
        Assert.False(failure.Bind(() => Result.Success()).IsSuccess);
        Assert.True(success.Bind(state, s => Result.Success()).IsSuccess);
        Assert.False(success.Bind(state, s => Result.Failure(Error.Failure("X", "X"))).IsSuccess);
        Assert.False(failure.Bind(state, s => Result.Success()).IsSuccess);

        Assert.True(success.Bind(() => Result.Success(1)).IsSuccess);
        Assert.False(success.Bind(() => Result.Failure<int>(Error.Failure("X", "X"))).IsSuccess);
        Assert.False(failure.Bind(() => Result.Success(1)).IsSuccess);
        Assert.True(success.Bind(state, s => Result.Success(1)).IsSuccess);
        Assert.False(success.Bind(state, s => Result.Failure<int>(Error.Failure("X", "X"))).IsSuccess);
        Assert.False(failure.Bind(state, s => Result.Success(1)).IsSuccess);
        
        Assert.True(success.Map(() => 1).IsSuccess);
        Assert.False(failure.Map(() => 1).IsSuccess);
        Assert.True(success.Map(state, s => 1).IsSuccess);
        Assert.False(failure.Map(state, s => 1).IsSuccess);
    }

    [Fact]
    public void ResultOfT_Match_Switch_Tap_Recover_Ensure_MapError_Inspect_State_Overrides()
    {
        var success = Result.Success(100);
        var failure = Result.Failure<int>(Error.Failure("code", "msg"));
        var state = 42;

        Assert.Equal(100, success.Match(v => v, e => 0));
        Assert.Equal(0, failure.Match(v => v, e => 0));
        Assert.Equal(142, success.Match(state, (s, v) => s + v, (s, e) => 0));
        Assert.Equal(0, failure.Match(state, (s, v) => s + v, (s, e) => 0));

        bool called = false; _ = called;
        success.Execute(v => called = true, e => { });
        failure.Execute(v => { }, e => called = true);
        success.Execute(state, (s, v) => called = true, (s, e) => { });
        failure.Execute(state, (s, v) => { }, (s, e) => called = true);

        success.TapOnSuccess(v => called = true);
        success.TapOnSuccess(state, (s, v) => called = true);
        failure.TapOnFailure(e => called = true);
        failure.TapOnFailure(state, (s, e) => called = true);

        Assert.Equal("new", failure.MapError(e => Error.Failure("new", "new")).Error.Code);
        Assert.Equal("new", failure.MapError(state, (s, e) => Error.Failure("new", "new")).Error.Code);
        Assert.True(success.MapError(e => Error.Failure("new", "new")).IsSuccess);
        Assert.True(success.MapError(state, (s, e) => Error.Failure("new", "new")).IsSuccess);

        success.Inspect(r => called = true);
        success.Inspect(state, (s, r) => called = true);

        Assert.True(failure.Recover(e => Result.Success(1)).IsSuccess);
        Assert.True(failure.Recover(state, (s, e) => Result.Success(1)).IsSuccess);
        Assert.True(success.Recover(e => Result.Failure<int>(Error.Failure("X", "X"))).IsSuccess);
        Assert.True(success.Recover(state, (s, e) => Result.Failure<int>(Error.Failure("X", "X"))).IsSuccess);

        Assert.True(success.Ensure(v => true, Error.Failure("X", "X")).IsSuccess);
        Assert.False(success.Ensure(v => false, Error.Failure("X", "X")).IsSuccess);
        Assert.False(failure.Ensure(v => true, Error.Failure("X", "X")).IsSuccess);
        Assert.True(success.Ensure(state, (s, v) => true, Error.Failure("X", "X")).IsSuccess);
        Assert.False(success.Ensure(state, (s, v) => false, Error.Failure("X", "X")).IsSuccess);
        Assert.False(failure.Ensure(state, (s, v) => true, Error.Failure("X", "X")).IsSuccess);

        Assert.True(success.Bind(v => Result.Success()).IsSuccess);
        Assert.False(success.Bind(v => Result.Failure(Error.Failure("X", "X"))).IsSuccess);
        Assert.False(failure.Bind(v => Result.Success()).IsSuccess);
        Assert.True(success.Bind(state, (s, v) => Result.Success()).IsSuccess);
        Assert.False(success.Bind(state, (s, v) => Result.Failure(Error.Failure("X", "X"))).IsSuccess);
        Assert.False(failure.Bind(state, (s, v) => Result.Success()).IsSuccess);

        Assert.True(success.Bind(v => Result.Success(1)).IsSuccess);
        Assert.False(success.Bind(v => Result.Failure<int>(Error.Failure("X", "X"))).IsSuccess);
        Assert.False(failure.Bind(v => Result.Success(1)).IsSuccess);
        Assert.True(success.Bind(state, (s, v) => Result.Success(1)).IsSuccess);
        Assert.False(success.Bind(state, (s, v) => Result.Failure<int>(Error.Failure("X", "X"))).IsSuccess);
        Assert.False(failure.Bind(state, (s, v) => Result.Success(1)).IsSuccess);
        
        Assert.True(success.Map(v => v.ToString()).IsSuccess);
        Assert.False(failure.Map(v => v.ToString()).IsSuccess);
        Assert.True(success.Map(state, (s, v) => v.ToString()).IsSuccess);
        Assert.False(failure.Map(state, (s, v) => v.ToString()).IsSuccess);
    }
}

