// Copyright © Erickson Lopez. MIT License.
using System;
using EricksonLopez.Result;
using Xunit;

namespace EricksonLopez.Result.Tests.Core;

public class ResultCoreEdgeCaseTests
{
    private static readonly Error TestError = Error.Failure("code", "msg");
    private const int TestState = 42;

    [Fact]
    public void Result_Match_EvaluatesBranchesCorrectly()
    {
        var success = Result.Success();
        var failure = Result.Failure(TestError);

        Assert.Equal(1, success.Match(() => 1, e => 0));
        Assert.Equal(0, failure.Match(() => 1, e => 0));
        Assert.Equal(42, success.Match(TestState, s => s, (s, e) => 0));
        Assert.Equal(0, failure.Match(TestState, s => s, (s, e) => 0));
    }

    [Fact]
    public void Result_Execute_ExecutesBranchActions()
    {
        var success = Result.Success();
        var failure = Result.Failure(TestError);

        bool successCalled = false;
        bool failureCalled = false;

        success.Execute(() => successCalled = true, e => { });
        failure.Execute(() => { }, e => failureCalled = true);
        Assert.True(successCalled);
        Assert.True(failureCalled);

        bool stateSuccessCalled = false;
        bool stateFailureCalled = false;
        success.Execute(TestState, s => stateSuccessCalled = (s == TestState), (s, e) => { });
        failure.Execute(TestState, s => { }, (s, e) => stateFailureCalled = (s == TestState));
        Assert.True(stateSuccessCalled);
        Assert.True(stateFailureCalled);
    }

    [Fact]
    public void Result_Tap_ExecutesSideEffects()
    {
        var success = Result.Success();
        var failure = Result.Failure(TestError);

        bool tapSuccessCalled = false;
        bool tapFailureCalled = false;

        success.TapOnSuccess(() => tapSuccessCalled = true);
        success.TapOnSuccess(TestState, s => tapSuccessCalled = (s == TestState));
        failure.TapOnFailure(e => tapFailureCalled = true);
        failure.TapOnFailure(TestState, (s, e) => tapFailureCalled = (s == TestState));

        Assert.True(tapSuccessCalled);
        Assert.True(tapFailureCalled);
    }

    [Fact]
    public void Result_MapError_TransformsErrorOnFailure()
    {
        var success = Result.Success();
        var failure = Result.Failure(TestError);

        var mappedFailure = failure.MapError(e => Error.Failure("new", "new"));
        Assert.Equal("new", mappedFailure.Error.Code);

        var mappedFailureState = failure.MapError(TestState, (s, e) => Error.Failure("new", "new"));
        Assert.Equal("new", mappedFailureState.Error.Code);

        Assert.True(success.MapError(e => Error.Failure("new", "new")).IsSuccess);
        Assert.True(success.MapError(TestState, (s, e) => Error.Failure("new", "new")).IsSuccess);
    }

    [Fact]
    public void Result_Inspect_InvokesInspectorCallback()
    {
        var success = Result.Success();
        bool inspectCalled = false;
        bool stateInspectCalled = false;

        success.Inspect(r => inspectCalled = r.IsSuccess);
        success.Inspect(TestState, (s, r) => stateInspectCalled = (s == TestState && r.IsSuccess));

        Assert.True(inspectCalled);
        Assert.True(stateInspectCalled);
    }

    [Fact]
    public void Result_Recover_RecoversFromFailure()
    {
        var success = Result.Success();
        var failure = Result.Failure(TestError);

        Assert.True(failure.Recover(e => Result.Success()).IsSuccess);
        Assert.True(failure.Recover(TestState, (s, e) => Result.Success()).IsSuccess);
        Assert.True(success.Recover(e => Result.Failure(Error.Failure("X", "X"))).IsSuccess);
        Assert.True(success.Recover(TestState, (s, e) => Result.Failure(Error.Failure("X", "X"))).IsSuccess);
    }

    [Fact]
    public void Result_Ensure_ValidatesPredicateAndState()
    {
        var success = Result.Success();
        var failure = Result.Failure(TestError);

        Assert.True(success.Ensure(() => true, Error.Failure("X", "X")).IsSuccess);
        Assert.False(success.Ensure(() => false, Error.Failure("X", "X")).IsSuccess);
        Assert.False(failure.Ensure(() => true, Error.Failure("X", "X")).IsSuccess);
        Assert.True(success.Ensure(TestState, s => s == TestState, Error.Failure("X", "X")).IsSuccess);
        Assert.False(success.Ensure(TestState, s => false, Error.Failure("X", "X")).IsSuccess);
        Assert.False(failure.Ensure(TestState, s => true, Error.Failure("X", "X")).IsSuccess);
    }

    [Fact]
    public void Result_Bind_ChainsPipelineResults()
    {
        var success = Result.Success();
        var failure = Result.Failure(TestError);

        Assert.True(success.Bind(() => Result.Success()).IsSuccess);
        Assert.False(success.Bind(() => Result.Failure(Error.Failure("X", "X"))).IsSuccess);
        Assert.False(failure.Bind(() => Result.Success()).IsSuccess);
        Assert.True(success.Bind(TestState, s => Result.Success()).IsSuccess);
        Assert.False(success.Bind(TestState, s => Result.Failure(Error.Failure("X", "X"))).IsSuccess);
        Assert.False(failure.Bind(TestState, s => Result.Success()).IsSuccess);

        Assert.True(success.Bind(() => Result.Success(1)).IsSuccess);
        Assert.False(success.Bind(() => Result.Failure<int>(Error.Failure("X", "X"))).IsSuccess);
        Assert.False(failure.Bind(() => Result.Success(1)).IsSuccess);
        Assert.True(success.Bind(TestState, s => Result.Success(1)).IsSuccess);
        Assert.False(success.Bind(TestState, s => Result.Failure<int>(Error.Failure("X", "X"))).IsSuccess);
        Assert.False(failure.Bind(TestState, s => Result.Success(1)).IsSuccess);
    }

    [Fact]
    public void Result_Map_TransformsSuccessValue()
    {
        var success = Result.Success();
        var failure = Result.Failure(TestError);

        Assert.True(success.Map(() => 1).IsSuccess);
        Assert.False(failure.Map(() => 1).IsSuccess);
        Assert.True(success.Map(TestState, s => s).IsSuccess);
        Assert.False(failure.Map(TestState, s => s).IsSuccess);
    }

    [Fact]
    public void ResultOfT_Match_EvaluatesBranchesCorrectly()
    {
        var success = Result.Success(100);
        var failure = Result.Failure<int>(TestError);

        Assert.Equal(100, success.Match(v => v, e => 0));
        Assert.Equal(0, failure.Match(v => v, e => 0));
        Assert.Equal(142, success.Match(TestState, (s, v) => s + v, (s, e) => 0));
        Assert.Equal(0, failure.Match(TestState, (s, v) => s + v, (s, e) => 0));
    }

    [Fact]
    public void ResultOfT_Execute_ExecutesBranchActions()
    {
        var success = Result.Success(100);
        var failure = Result.Failure<int>(TestError);

        bool successCalled = false;
        bool failureCalled = false;
        success.Execute(v => successCalled = (v == 100), e => { });
        failure.Execute(v => { }, e => failureCalled = true);
        Assert.True(successCalled);
        Assert.True(failureCalled);

        bool stateSuccessCalled = false;
        bool stateFailureCalled = false;
        success.Execute(TestState, (s, v) => stateSuccessCalled = (s == TestState && v == 100), (s, e) => { });
        failure.Execute(TestState, (s, v) => { }, (s, e) => stateFailureCalled = (s == TestState));
        Assert.True(stateSuccessCalled);
        Assert.True(stateFailureCalled);
    }

    [Fact]
    public void ResultOfT_Tap_ExecutesSideEffects()
    {
        var success = Result.Success(100);
        var failure = Result.Failure<int>(TestError);

        bool tapSuccessCalled = false;
        bool tapFailureCalled = false;

        success.TapOnSuccess(v => tapSuccessCalled = (v == 100));
        success.TapOnSuccess(TestState, (s, v) => tapSuccessCalled = (s == TestState && v == 100));
        failure.TapOnFailure(e => tapFailureCalled = true);
        failure.TapOnFailure(TestState, (s, e) => tapFailureCalled = (s == TestState));

        Assert.True(tapSuccessCalled);
        Assert.True(tapFailureCalled);
    }

    [Fact]
    public void ResultOfT_MapError_TransformsErrorOnFailure()
    {
        var success = Result.Success(100);
        var failure = Result.Failure<int>(TestError);

        Assert.Equal("new", failure.MapError(e => Error.Failure("new", "new")).Error.Code);
        Assert.Equal("new", failure.MapError(TestState, (s, e) => Error.Failure("new", "new")).Error.Code);
        Assert.True(success.MapError(e => Error.Failure("new", "new")).IsSuccess);
        Assert.True(success.MapError(TestState, (s, e) => Error.Failure("new", "new")).IsSuccess);
    }

    [Fact]
    public void ResultOfT_Inspect_InvokesInspectorCallback()
    {
        var success = Result.Success(100);
        bool inspectCalled = false;
        bool stateInspectCalled = false;

        success.Inspect(r => inspectCalled = (r.IsSuccess && r.Value == 100));
        success.Inspect(TestState, (s, r) => stateInspectCalled = (s == TestState && r.IsSuccess && r.Value == 100));

        Assert.True(inspectCalled);
        Assert.True(stateInspectCalled);
    }

    [Fact]
    public void ResultOfT_Recover_RecoversFromFailure()
    {
        var success = Result.Success(100);
        var failure = Result.Failure<int>(TestError);

        Assert.True(failure.Recover(e => Result.Success(1)).IsSuccess);
        Assert.True(failure.Recover(TestState, (s, e) => Result.Success(1)).IsSuccess);
        Assert.True(success.Recover(e => Result.Failure<int>(Error.Failure("X", "X"))).IsSuccess);
        Assert.True(success.Recover(TestState, (s, e) => Result.Failure<int>(Error.Failure("X", "X"))).IsSuccess);
    }

    [Fact]
    public void ResultOfT_Ensure_ValidatesPredicateAndState()
    {
        var success = Result.Success(100);
        var failure = Result.Failure<int>(TestError);

        Assert.True(success.Ensure(v => true, Error.Failure("X", "X")).IsSuccess);
        Assert.False(success.Ensure(v => false, Error.Failure("X", "X")).IsSuccess);
        Assert.False(failure.Ensure(v => true, Error.Failure("X", "X")).IsSuccess);
        Assert.True(success.Ensure(TestState, (s, v) => true, Error.Failure("X", "X")).IsSuccess);
        Assert.False(success.Ensure(TestState, (s, v) => false, Error.Failure("X", "X")).IsSuccess);
        Assert.False(failure.Ensure(TestState, (s, v) => true, Error.Failure("X", "X")).IsSuccess);
    }

    [Fact]
    public void ResultOfT_Bind_ChainsPipelineResults()
    {
        var success = Result.Success(100);
        var failure = Result.Failure<int>(TestError);

        Assert.True(success.Bind(v => Result.Success()).IsSuccess);
        Assert.False(success.Bind(v => Result.Failure(Error.Failure("X", "X"))).IsSuccess);
        Assert.False(failure.Bind(v => Result.Success()).IsSuccess);
        Assert.True(success.Bind(TestState, (s, v) => Result.Success()).IsSuccess);
        Assert.False(success.Bind(TestState, (s, v) => Result.Failure(Error.Failure("X", "X"))).IsSuccess);
        Assert.False(failure.Bind(TestState, (s, v) => Result.Success()).IsSuccess);

        Assert.True(success.Bind(v => Result.Success(1)).IsSuccess);
        Assert.False(success.Bind(v => Result.Failure<int>(Error.Failure("X", "X"))).IsSuccess);
        Assert.False(failure.Bind(v => Result.Success(1)).IsSuccess);
        Assert.True(success.Bind(TestState, (s, v) => Result.Success(1)).IsSuccess);
        Assert.False(success.Bind(TestState, (s, v) => Result.Failure<int>(Error.Failure("X", "X"))).IsSuccess);
        Assert.False(failure.Bind(TestState, (s, v) => Result.Success(1)).IsSuccess);
    }

    [Fact]
    public void ResultOfT_Map_TransformsSuccessValue()
    {
        var success = Result.Success(100);
        var failure = Result.Failure<int>(TestError);

        Assert.True(success.Map(v => v.ToString()).IsSuccess);
        Assert.False(failure.Map(v => v.ToString()).IsSuccess);
        Assert.True(success.Map(TestState, (s, v) => v.ToString()).IsSuccess);
        Assert.False(failure.Map(TestState, (s, v) => v.ToString()).IsSuccess);
    }
}



