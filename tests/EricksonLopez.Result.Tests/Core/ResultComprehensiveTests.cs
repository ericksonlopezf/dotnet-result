using System;
using System.Threading.Tasks;
using Xunit;

namespace EricksonLopez.Result.Tests.Core;

public class ResultComprehensiveTests
{
    private static readonly Error TestError = Error.Failure("T1", "M1");

    [Fact]
    public void Match_State_Works()
    {
        var s = Result.Success();
        var f = Result.Failure(TestError);

        Assert.Equal(10, s.Match(10, state => state, (state, err) => -1));
        Assert.Equal(-1, f.Match(10, state => state, (state, err) => -1));
    }

    [Fact]
    public void Switch_State_Works()
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
    public void Map_Works()
    {
        var s = Result.Success();
        var f = Result.Failure(TestError);

        Assert.Equal(10, s.Map(() => 10).Value);
        Assert.True(f.Map(() => 10).IsFailure);

        Assert.Equal(10, s.Map(10, state => state).Value);
        Assert.True(f.Map(10, state => state).IsFailure);
    }

    [Fact]
    public void Bind_Works()
    {
        var s = Result.Success();
        var f = Result.Failure(TestError);

        Assert.True(s.Bind(() => Result.Success()).IsSuccess);
        Assert.True(s.Bind(10, state => Result.Success()).IsSuccess);
        Assert.True(s.Bind(() => Result.Success(1)).IsSuccess);
        Assert.True(s.Bind(10, state => Result.Success(state)).IsSuccess);

        Assert.True(f.Bind(() => Result.Success()).IsFailure);
        Assert.True(f.Bind(10, state => Result.Success()).IsFailure);
        Assert.True(f.Bind(() => Result.Success(1)).IsFailure);
        Assert.True(f.Bind(10, state => Result.Success(state)).IsFailure);
    }

    [Fact]
    public void Tap_Works()
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
    public void TapOnFailure_Works()
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
    public void Ensure_Works()
    {
        var s = Result.Success();
        var f = Result.Failure(TestError);

        Assert.True(s.Ensure(() => true, TestError).IsSuccess);
        Assert.True(s.Ensure(() => false, TestError).IsFailure);

        Assert.True(s.Ensure(10, state => true, TestError).IsSuccess);
        Assert.True(s.Ensure(10, state => false, TestError).IsFailure);

        Assert.True(f.Ensure(() => true, Error.Failure("T2", "M2")).IsFailure);
        Assert.True(f.Ensure(10, state => true, Error.Failure("T2", "M2")).IsFailure);
    }

    [Fact]
    public void Inspect_Works()
    {
        var s = Result.Success();
        int v = 0;
        s.Inspect(r => v = 1);
        Assert.Equal(1, v);

        s.Inspect(5, (state, r) => v = state);
        Assert.Equal(5, v);
    }



    [Fact]
    public void Recover_Works()
    {
        var s = Result.Success();
        var f = Result.Failure(TestError);

        Assert.True(s.Recover(e => Result.Failure(e)).IsSuccess);
        Assert.True(f.Recover(e => Result.Success()).IsSuccess);
        Assert.True(f.Recover(10, (state, e) => Result.Success()).IsSuccess);
    }

    [Fact]
    public void Deconstruct_Works()
    {

        var (isSuccess, err) = Result.Success();
        Assert.True(isSuccess);
        Assert.Null(err);

        var (isSuccess2, err2) = Result.Failure(TestError);
        Assert.False(isSuccess2);
        Assert.Equal(TestError, err2);
    }

    [Fact]
    public void Try_Works()
    {
        Assert.True(Result.Try(() => { }, e => TestError).IsSuccess);
        Assert.True(Result.Try(() => throw new InvalidOperationException(), e => TestError).IsFailure);
        
        Assert.True(Result.Try(() => 1, e => TestError).IsSuccess);
        Assert.True(Result.Try<int>(() => throw new InvalidOperationException(), e => TestError).IsFailure);
    }

    [Fact]
    public async Task TryAsync_Works()
    {
        Assert.True((await Result.TryAsync((Func<Task>)(async () => { await Task.Yield(); }), e => TestError)).IsSuccess);
        Assert.True((await Result.TryAsync((Func<Task>)(async () => { await Task.Yield(); throw new InvalidOperationException(); }), e => TestError)).IsFailure);
        
        Assert.True((await Result.TryAsync((Func<Task<int>>)(async () => { await Task.Yield(); return 1; }), e => TestError)).IsSuccess);
        Assert.True((await Result.TryAsync<int>((Func<Task<int>>)(async () => { await Task.Yield(); throw new InvalidOperationException(); }), e => TestError)).IsFailure);
    }

    [Fact]
    public void TryGetError_Works()
    {
        Assert.False(Result.Success().TryGetError(out _));
        Assert.True(Result.Failure(TestError).TryGetError(out var e) && e == TestError);
        
        Assert.False(Result.Success().TryGetError(out _, out var isUn));
        Assert.False(isUn);
    }

    [Fact]
    public void MapError_Works()
    {
        var s = Result.Success();
        var f = Result.Failure(TestError);

        Assert.True(s.MapError(e => Error.Failure("T2", "M2")).IsSuccess);
        Assert.Equal("T2", f.MapError(e => Error.Failure("T2", "M2")).Error!.Code);
        Assert.Equal("T2", f.MapError(10, (state, e) => Error.Failure("T2", "M2")).Error!.Code);
    }

    [Fact]
    public void Equality_Works()
    {
        var s1 = Result.Success();
        var s2 = Result.Success();
        var f1 = Result.Failure(TestError);
        var f2 = Result.Failure(TestError);
        var f3 = Result.Failure(Error.Failure("X", "Y"));

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
        Assert.True(implicitCast.IsFailure);
    }
}
