using System;
using System.Threading.Tasks;
using Xunit;

namespace EricksonLopez.Result.Tests.Core;

public class ResultOfTComprehensiveTests
{
    private static readonly Error TestError = Error.Failure("T1", "M1");

    [Fact]
    public void Match_State_Works()
    {
        var s = Result.Success(42);
        var f = Result.Failure<int>(TestError);

        Assert.Equal(420, s.Match(10, (state, v) => v * state, (state, err) => -1));
        Assert.Equal(-1, f.Match(10, (state, v) => v * state, (state, err) => -1));
    }

    [Fact]
    public void Switch_State_Works()
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
    public void Map_Works()
    {
        var s = Result.Success(42);
        var f = Result.Failure<int>(TestError);

        Assert.Equal(420, s.Map(10, (state, v) => v * state).Value);
        Assert.True(f.Map(10, (state, v) => v * state).IsFailure);
    }

    [Fact]
    public void Bind_Works()
    {
        var s = Result.Success(42);
        var f = Result.Failure<int>(TestError);

        Assert.True(s.Bind(10, (state, v) => Result.Success()).IsSuccess);
        Assert.True(s.Bind(10, (state, v) => Result.Success(v * state)).IsSuccess);
        Assert.Equal(420, s.Bind(10, (state, v) => Result.Success(v * state)).Value);

        Assert.True(f.Bind(10, (state, v) => Result.Success()).IsFailure);
        Assert.True(f.Bind(10, (state, v) => Result.Success(v * state)).IsFailure);
    }

    [Fact]
    public void Tap_Works()
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
    public void TapOnFailure_Works()
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
    public void Ensure_Works()
    {
        var s = Result.Success(42);
        var f = Result.Failure<int>(TestError);

        Assert.True(s.Ensure(10, (state, val) => val > state, TestError).IsSuccess);
        Assert.True(s.Ensure(100, (state, val) => val > state, TestError).IsFailure);

        Assert.True(f.Ensure(10, (state, val) => true, Error.Failure("T2", "M2")).IsFailure);
    }

    [Fact]
    public void Inspect_Works()
    {
        var s = Result.Success(42);
        int v = 0;
        s.Inspect(5, (state, r) => v = state * r.Value);
        Assert.Equal(210, v);
    }



    [Fact]
    public void Recover_Works()
    {
        var s = Result.Success(42);
        var f = Result.Failure<int>(TestError);

        Assert.True(s.Recover(10, (state, e) => Result.Failure<int>(e)).IsSuccess);
        Assert.True(f.Recover(10, (state, e) => Result.Success(state)).IsSuccess);
    }

    [Fact]
    public void Deconstruct_Works()
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
    public void TryGetError_Works()
    {
        Assert.False(Result.Success(42).TryGetError(out _));
        Assert.True(Result.Failure<int>(TestError).TryGetError(out var e) && e == TestError);
        
        Assert.False(Result.Success(42).TryGetError(out _, out var isUn));
        Assert.False(isUn);
    }

    [Fact]
    public void MapError_Works()
    {
        var s = Result.Success(42);
        var f = Result.Failure<int>(TestError);

        Assert.Equal("T2", f.MapError(10, (state, e) => Error.Failure("T2", "M2")).Error!.Code);
    }

    [Fact]
    public void Equality_Works()
    {
        var s1 = Result.Success(42);
        var s2 = Result.Success(42);
        var s3 = Result.Success(43);
        var f1 = Result.Failure<int>(TestError);
        var f2 = Result.Failure<int>(TestError);
        var f3 = Result.Failure<int>(Error.Failure("X", "Y"));

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
        Assert.True(implicitCast.IsFailure);
        
        Result<int> implicitCastVal = 42;
        Assert.True(implicitCastVal.IsSuccess);
    }
}
