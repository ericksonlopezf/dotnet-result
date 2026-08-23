// Copyright © Erickson Lopez. MIT License.
using System;
using System.Threading;
using System.Threading.Tasks;
using EricksonLopez.Result;
using EricksonLopez.Result.Testing;
using Xunit;

namespace EricksonLopez.Result.Tests.Core;

public class ResultStateMatchTests : ResultExtensionsTestsBase
{
    [Fact]
    public void Match_WhenSuccess_ReturnsMappedState()
    {
        var s = Result.Success();
        var f = Result.Failure(TestError);

        Assert.Equal(10, s.Match(10, state => state, (state, err) => -1));
        Assert.Equal(-1, f.Match(10, state => state, (state, err) => -1));
    }

    [Fact]
    public void Switch_WhenSuccess_ExecutesSuccessAction()
    {
        var s = Result.Success();
        var f = Result.Failure(TestError);

        int val = 0;
        s.Execute(10, state => val = state, (state, err) => val = -1);
        Assert.Equal(10, val);

        f.Execute(10, state => val = state, (state, err) => val = -1);
        Assert.Equal(-1, val);
    }

    [Fact]
    public void Map_WhenSuccess_ReturnsMappedResult()
    {
        var s = Result.Success();
        var f = Result.Failure(TestError);

        Assert.Equal(10, s.Map(() => 10).Value);
        f.Map(() => 10).ShouldBeFailure();

        Assert.Equal(10, s.Map(10, state => state).Value);
        f.Map(10, state => state).ShouldBeFailure();
    }

    [Fact]
    public void Bind_WhenSuccess_ReturnsBoundResult()
    {
        var s = Result.Success();
        var f = Result.Failure(TestError);

        s.Bind(() => Result.Success()).ShouldBeSuccess();
        s.Bind(10, state => Result.Success()).ShouldBeSuccess();
        s.Bind(() => Result.Success(1)).ShouldBeSuccess();
        s.Bind(10, state => Result.Success(state)).ShouldBeSuccess();

        f.Bind(() => Result.Success()).ShouldBeFailure();
        f.Bind(10, state => Result.Success()).ShouldBeFailure();
        f.Bind(() => Result.Success(1)).ShouldBeFailure();
        f.Bind(10, state => Result.Success(state)).ShouldBeFailure();
    }

    [Fact]
    public void Tap_WhenSuccess_InvokesAction()
    {
        var s = Result.Success();
        var f = Result.Failure(TestError);

        int v = 0;
        s.TapOnSuccess(() => v = 1);
        Assert.Equal(1, v);

        f.TapOnSuccess(() => v = 2);
        Assert.Equal(1, v);

        s.TapOnSuccess(5, state => v = state);
        Assert.Equal(5, v);

        f.TapOnSuccess(6, state => v = state);
        Assert.Equal(5, v);
    }

    [Fact]
    public void TapOnFailure_WhenFailure_InvokesAction()
    {
        var s = Result.Success();
        var f = Result.Failure(TestError);

        int v = 0;
        s.TapOnFailure(e => v = 1);
        Assert.Equal(0, v);

        f.TapOnFailure(e => v = 2);
        Assert.Equal(2, v);

        s.TapOnFailure(5, (state, e) => v = state);
        Assert.Equal(2, v);

        f.TapOnFailure(6, (state, e) => v = state);
        Assert.Equal(6, v);
    }

    [Fact]
    public void Ensure_WhenPredicateFails_ReturnsFailure()
    {
        var s = Result.Success();
        var f = Result.Failure(TestError);

        s.Ensure(() => true, TestError).ShouldBeSuccess();
        s.Ensure(() => false, TestError).ShouldBeFailure();

        s.Ensure(10, state => true, TestError).ShouldBeSuccess();
        s.Ensure(10, state => false, TestError).ShouldBeFailure();

        f.Ensure(() => true, TestError2).ShouldBeFailure();
        f.Ensure(10, state => true, TestError2).ShouldBeFailure();
    }

    [Fact]
    public void Inspect_WhenInvoked_ExecutesAction()
    {
        var s = Result.Success();
        int v = 0;
        s.Inspect(r => v = 1);
        Assert.Equal(1, v);

        s.Inspect(5, (state, r) => v = state);
        Assert.Equal(5, v);
    }

    [Fact]
    public void Recover_WhenFailure_ReturnsSuccess()
    {
        var s = Result.Success();
        var f = Result.Failure(TestError);

        s.Recover(e => Result.Failure(e)).ShouldBeSuccess();
        f.Recover(e => Result.Success()).ShouldBeSuccess();
        f.Recover(10, (state, e) => Result.Success()).ShouldBeSuccess();
    }

    [Fact]
    public void Deconstruct_WhenDeconstructed_ExtractsState()
    {
        var (isSuccess, err) = Result.Success();
        Assert.True(isSuccess);
        Assert.Null(err);

        var (isSuccess2, err2) = Result.Failure(TestError);
        Assert.False(isSuccess2);
        Assert.Equal(TestError, err2);
    }

    [Fact]
    public void Try_WhenEvaluated_CapturesOutcome()
    {
        Result.Try(() => { }, e => TestError).ShouldBeSuccess();
        Result.Try(() => throw new InvalidOperationException(), e => TestError).ShouldBeFailure();

        Result.Try(() => 1, e => TestError).ShouldBeSuccess();
        Result.Try<int>(() => throw new InvalidOperationException(), e => TestError).ShouldBeFailure();
    }

    [Fact]
    public async Task TryAsync_WhenEvaluated_CapturesOutcome()
    {
        (await Result.TryAsync((Func<Task>)(async () => { await Task.Yield(); }), e => TestError)).ShouldBeSuccess();
        (await Result.TryAsync((Func<Task>)(async () => { await Task.Yield(); throw new InvalidOperationException(); }), e => TestError)).ShouldBeFailure();

        (await Result.TryAsync((Func<Task<int>>)(async () => { await Task.Yield(); return 1; }), e => TestError)).ShouldBeSuccess();
        (await Result.TryAsync<int>((Func<Task<int>>)(async () => { await Task.Yield(); throw new InvalidOperationException(); }), e => TestError)).ShouldBeFailure();
    }

    [Fact]
    public void TryGetError_WhenFailure_ExtractsError()
    {
        Assert.False(Result.Success().TryGetError(out _));
        Assert.True(Result.Failure(TestError).TryGetError(out var e) && e == TestError);

        Assert.False(Result.Success().TryGetError(out _, out var isUn));
        Assert.False(isUn);
    }

    [Fact]
    public void MapError_WhenFailure_TransformsError()
    {
        var s = Result.Success();
        var f = Result.Failure(TestError);

        s.MapError(e => TestError2).ShouldBeSuccess();
        Assert.Equal(TestError2.Code, f.MapError(e => TestError2).Error!.Code);
        Assert.Equal(TestError2.Code, f.MapError(10, (state, e) => TestError2).Error!.Code);
    }

    [Fact]
    public void Equality_WhenCompared_ReturnsConsistentEquivalence()
    {
        var s1 = Result.Success();
        var s2 = Result.Success();
        var f1 = Result.Failure(TestError);
        var f2 = Result.Failure(TestError);
        var f3 = Result.Failure(TestError2);

        Assert.True(s1.Equals(s2));
        Assert.True(s1.Equals((object)s2));
        Assert.False(s1.Equals(f1));
        Assert.False(s1.Equals(new object()));

        Assert.True(f1.Equals(f2));
        Assert.False(f1.Equals(f3));

        Assert.True(s1 == s2);
        Assert.False(s1 != s2);
        Assert.True(s1.GetHashCode() == s2.GetHashCode());

        Result implicitCast = TestError;
        implicitCast.ShouldBeFailure();
    }
}




