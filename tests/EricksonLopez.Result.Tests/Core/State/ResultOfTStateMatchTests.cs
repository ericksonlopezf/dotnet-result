// Copyright © Erickson Lopez. MIT License.
using System;
using System.Threading;
using System.Threading.Tasks;
using EricksonLopez.Result;
using EricksonLopez.Result.Testing;
using Xunit;

namespace EricksonLopez.Result.Tests.Core;

public class ResultOfTStateMatchTests : ResultExtensionsTestsBase
{
    [Fact]
    public void Match_WhenSuccess_ReturnsMappedState()
    {
        var s = Result.Success(42);
        var f = Result.Failure<int>(TestError);

        Assert.Equal(420, s.Match(10, (state, v) => v * state, (state, err) => -1));
        Assert.Equal(-1, f.Match(10, (state, v) => v * state, (state, err) => -1));
    }

    [Fact]
    public void Switch_WhenSuccess_ExecutesSuccessAction()
    {
        var s = Result.Success(42);
        var f = Result.Failure<int>(TestError);

        int val = 0;
        s.Execute(10, (state, v) => val = v * state, (state, err) => val = -1);
        Assert.Equal(420, val);

        f.Execute(10, (state, v) => val = v * state, (state, err) => val = -1);
        Assert.Equal(-1, val);
    }

    [Fact]
    public void Map_WhenSuccess_ReturnsMappedResult()
    {
        var s = Result.Success(42);
        var f = Result.Failure<int>(TestError);

        Assert.Equal(420, s.Map(10, (state, v) => v * state).Value);
        f.Map(10, (state, v) => v * state).ShouldBeFailure();
    }

    [Fact]
    public void Bind_WhenSuccess_ReturnsBoundResult()
    {
        var s = Result.Success(42);
        var f = Result.Failure<int>(TestError);

        s.Bind(10, (state, v) => Result.Success()).ShouldBeSuccess();
        s.Bind(10, (state, v) => Result.Success(v * state)).ShouldBeSuccess();
        Assert.Equal(420, s.Bind(10, (state, v) => Result.Success(v * state)).Value);

        f.Bind(10, (state, v) => Result.Success()).ShouldBeFailure();
        f.Bind(10, (state, v) => Result.Success(v * state)).ShouldBeFailure();
    }

    [Fact]
    public void Tap_WhenSuccess_InvokesAction()
    {
        var s = Result.Success(42);
        var f = Result.Failure<int>(TestError);

        int v = 0;
        s.TapOnSuccess(5, (state, val) => v = state * val);
        Assert.Equal(210, v);

        f.TapOnSuccess(6, (state, val) => v = state * val);
        Assert.Equal(210, v);
    }

    [Fact]
    public void TapOnFailure_WhenFailure_InvokesAction()
    {
        var s = Result.Success(42);
        var f = Result.Failure<int>(TestError);

        int v = 0;
        s.TapOnFailure(5, (state, e) => v = state);
        Assert.Equal(0, v);

        f.TapOnFailure(6, (state, e) => v = state);
        Assert.Equal(6, v);
    }

    [Fact]
    public void Ensure_WhenPredicateFails_ReturnsFailure()
    {
        var s = Result.Success(42);
        var f = Result.Failure<int>(TestError);

        s.Ensure(10, (state, val) => val > state, TestError).ShouldBeSuccess();
        s.Ensure(100, (state, val) => val > state, TestError).ShouldBeFailure();

        f.Ensure(10, (state, val) => true, TestError2).ShouldBeFailure();
    }

    [Fact]
    public void Inspect_WhenInvoked_ExecutesAction()
    {
        var s = Result.Success(42);
        int v = 0;
        s.Inspect(5, (state, r) => v = state * r.Value);
        Assert.Equal(210, v);
    }

    [Fact]
    public void Recover_WhenFailure_ReturnsSuccess()
    {
        var s = Result.Success(42);
        var f = Result.Failure<int>(TestError);

        s.Recover(10, (state, e) => Result.Failure<int>(e)).ShouldBeSuccess();
        f.Recover(10, (state, e) => Result.Success(state)).ShouldBeSuccess();
    }

    [Fact]
    public void Deconstruct_WhenDeconstructed_ExtractsState()
    {
        var (isSuccess, val, err) = Result.Success(42);
        Assert.True(isSuccess);
        Assert.Equal(42, val);
        Assert.Null(err);

        var (isSuccess2, val2, err2) = Result.Failure<int>(TestError);
        Assert.False(isSuccess2);
        Assert.Equal(0, val2);
        Assert.Equal(TestError, err2);
    }

    [Fact]
    public void TryGetError_WhenFailure_ExtractsError()
    {
        Assert.False(Result.Success(42).TryGetError(out _));
        Assert.True(Result.Failure<int>(TestError).TryGetError(out var e) && e == TestError);

        Assert.False(Result.Success(42).TryGetError(out _, out var isUn));
        Assert.False(isUn);
    }

    [Fact]
    public void MapError_WhenFailure_TransformsError()
    {
        var s = Result.Success(42);
        var f = Result.Failure<int>(TestError);

        Assert.Equal(TestError2.Code, f.MapError(10, (state, e) => TestError2).Error!.Code);
    }

    [Fact]
    public void Equality_WhenCompared_ReturnsConsistentEquivalence()
    {
        var s1 = Result.Success(42);
        var s2 = Result.Success(42);
        var s3 = Result.Success(43);
        var f1 = Result.Failure<int>(TestError);
        var f2 = Result.Failure<int>(TestError);
        var f3 = Result.Failure<int>(TestError2);

        Assert.True(s1.Equals(s2));
        Assert.True(s1.Equals((object)s2));
        Assert.False(s1.Equals(s3));
        Assert.False(s1.Equals(f1));
        Assert.False(s1.Equals(new object()));

        Assert.True(f1.Equals(f2));
        Assert.False(f1.Equals(f3));

        Assert.True(s1 == s2);
        Assert.False(s1 != s2);
        Assert.True(s1.GetHashCode() == s2.GetHashCode());

        Result<int> implicitCast = TestError;
        implicitCast.ShouldBeFailure();

        Result<int> implicitCastVal = 42;
        implicitCastVal.ShouldBeSuccess();
    }
}



