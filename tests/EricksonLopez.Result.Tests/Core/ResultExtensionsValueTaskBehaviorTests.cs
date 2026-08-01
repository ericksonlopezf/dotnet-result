#pragma warning disable CA2012 // ValueTask instances returned from method calls should be awaited directly

using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using EricksonLopez.Result.Testing;

namespace EricksonLopez.Result.Tests.Core;

public class ResultExtensionsValueTaskBehaviorTests
{
    private static readonly Error SharedError = Error.Failure("Shared.Code", "Shared message");
    
    private static ValueTask<Result> S() => new(Result.Success());
    private static ValueTask<Result> F() => new(Result.Failure(SharedError));
    private static ValueTask<Result<int>> S(int v) => new(Result.Success(v));
    private static ValueTask<Result<int>> F<T>() => new(Result.Failure<int>(SharedError));

    private static async ValueTask<Result> IS() { await Task.Yield(); return Result.Success(); }
    private static async ValueTask<Result> IF() { await Task.Yield(); return Result.Failure(SharedError); }
    private static async ValueTask<Result<int>> IS(int v) { await Task.Yield(); return Result.Success(v); }
    private static async ValueTask<Result<int>> IF<T>() { await Task.Yield(); return Result.Failure<int>(SharedError); }

    [Fact]
    public async Task Map_ValueTask_Behavior()
    {

        await AssertMap(S(1), IS(1), F<int>(), IF<int>(), (t, f) => t.Map(f), 1, "A");
        await AssertMap(S(1), IS(1), F<int>(), IF<int>(), (t, f) => t.Map(10, (s, v) => f(v)), 1, "A");
        

        await AssertMap(S(1), IS(1), F<int>(), IF<int>(), (t, f) => t.Map(v => new ValueTask<string>(f(v))), 1, "A");
        await AssertMap(S(1), IS(1), F<int>(), IF<int>(), (t, f) => t.Map(10, (s, v) => new ValueTask<string>(f(v))), 1, "A");
    }

    [Fact]
    public async Task Bind_ValueTask_Behavior()
    {

        await AssertBind(S(), IS(), F(), IF(), (t, f) => t.Bind(() => { f(); return Result.Success(); }));
        await AssertBind(S(), IS(), F(), IF(), (t, f) => t.Bind(() => { f(); return new ValueTask<Result>(Result.Success()); }));
        

        await AssertBind(S(1), IS(1), F<int>(), IF<int>(), (t, f) => t.Bind(v => Result.Success(f(v))), 1, "A");
        await AssertBind(S(1), IS(1), F<int>(), IF<int>(), (t, f) => t.Bind(v => new ValueTask<Result<string>>(Result.Success(f(v)))), 1, "A");
    }

    [Fact]
    public async Task Tap_ValueTask_Behavior()
    {
        await AssertTap(S(1), IS(1), F<int>(), IF<int>(), (t, a) => t.TapOnSuccess(v => a()));
        await AssertTap(S(1), IS(1), F<int>(), IF<int>(), (t, a) => t.TapOnSuccess(10, (s, v) => a()));
    }
    
    [Fact]
    public async Task TapOnFailure_ValueTask_Behavior()
    {
        await AssertTapOnFailure(S(1), IS(1), F<int>(), IF<int>(), (t, a) => t.TapOnFailure(e => a()));
        await AssertTapOnFailure(S(1), IS(1), F<int>(), IF<int>(), (t, a) => t.TapOnFailure(10, (s, e) => a()));
        
        await AssertTapOnFailure(S(1), IS(1), F<int>(), IF<int>(), (t, a) => t.TapOnFailure(e => { a(); return ValueTask.CompletedTask; }));
    }
    
    [Fact]
    public async Task Ensure_ValueTask_Behavior()
    {
        int invoked = 0;
        var r1 = await S(1).Ensure(v => { invoked++; return true; }, Error.Failure("A", "B"));
        Assert.Equal(1, invoked);
        Assert.True(r1.IsSuccess);
        
        invoked = 0;
        var r2 = await F<int>().Ensure(v => { invoked++; return true; }, Error.Failure("A", "B"));
        Assert.Equal(0, invoked);
        Assert.Same(SharedError, r2.Error);
    }
    
    [Fact]
    public async Task Recover_ValueTask_Behavior()
    {
        int invoked = 0;
        var r1 = await F<int>().Recover(e => { invoked++; return S(10); });
        Assert.Equal(1, invoked);
        Assert.Equal(10, r1.Value);
        
        invoked = 0;
        var r2 = await S(5).Recover(e => { invoked++; return S(10); });
        Assert.Equal(0, invoked);
        Assert.Equal(5, r2.Value);
    }

    [Fact]
    public async Task Switch_ValueTask_Behavior()
    {
        await AssertExecute(S(1), IS(1), F<int>(), IF<int>(), (t, a, b) => t.Execute(a, b));
        await AssertExecute(S(1), IS(1), F<int>(), IF<int>(), (t, a, b) => t.Execute(10, (s, v) => a(v), (s, e) => b(e)));
    }
    
    [Fact]
    public async Task Match_ValueTask_Behavior()
    {
        await AssertMatch(S(1), IS(1), F<int>(), IF<int>(), (t, a, b) => t.Match(a, b));
        await AssertMatch(S(1), IS(1), F<int>(), IF<int>(), (t, a, b) => t.Match(10, (s, v) => a(v), (s, e) => b(e)));
    }

    private static async Task AssertMap<TIn, TOut>(
        ValueTask<Result<TIn>> s, ValueTask<Result<TIn>> si, ValueTask<Result<TIn>> f, ValueTask<Result<TIn>> fi,
        Func<ValueTask<Result<TIn>>, Func<TIn, TOut>, ValueTask<Result<TOut>>> act, TIn inVal, TOut outVal)
    {
        foreach (var t in new[] { s, si })
        {
            int invoked = 0;
            var res = await act(t, v => { invoked++; return outVal; });
            Assert.True(res.IsSuccess);
            Assert.Equal(outVal, res.Value);
            Assert.Equal(1, invoked);
        }
        foreach (var t in new[] { f, fi })
        {
            int invoked = 0;
            var res = await act(t, v => { invoked++; return outVal; });
            Assert.True(res.IsFailure);
            Assert.Equal(0, invoked);
            Assert.Same(SharedError, res.Error);
        }
    }
    
    private static async Task AssertBind<TIn, TOut>(
        ValueTask<Result<TIn>> s, ValueTask<Result<TIn>> si, ValueTask<Result<TIn>> f, ValueTask<Result<TIn>> fi,
        Func<ValueTask<Result<TIn>>, Func<TIn, TOut>, ValueTask<Result<TOut>>> act, TIn inVal, TOut outVal)
    {
        foreach (var t in new[] { s, si })
        {
            int invoked = 0;
            var res = await act(t, v => { invoked++; return outVal; });
            Assert.True(res.IsSuccess);
            Assert.Equal(1, invoked);
        }
        foreach (var t in new[] { f, fi })
        {
            int invoked = 0;
            var res = await act(t, v => { invoked++; return outVal; });
            Assert.True(res.IsFailure);
            Assert.Equal(0, invoked);
            Assert.Same(SharedError, res.Error);
        }
    }
    
    private static async Task AssertBind(
        ValueTask<Result> s, ValueTask<Result> si, ValueTask<Result> f, ValueTask<Result> fi,
        Func<ValueTask<Result>, Func<string>, ValueTask<Result>> act)
    {
        foreach (var t in new[] { s, si })
        {
            int invoked = 0;
            var res = await act(t, () => { invoked++; return "A"; });
            Assert.True(res.IsSuccess);
            Assert.Equal(1, invoked);
        }
        foreach (var t in new[] { f, fi })
        {
            int invoked = 0;
            var res = await act(t, () => { invoked++; return "A"; });
            Assert.True(res.IsFailure);
            Assert.Equal(0, invoked);
            Assert.Same(SharedError, res.Error);
        }
    }

    private static async Task AssertTap<TIn>(
        ValueTask<Result<TIn>> s, ValueTask<Result<TIn>> si, ValueTask<Result<TIn>> f, ValueTask<Result<TIn>> fi,
        Func<ValueTask<Result<TIn>>, Action, ValueTask<Result<TIn>>> act)
    {
        foreach (var t in new[] { s, si })
        {
            int invoked = 0;
            var res = await act(t, () => invoked++);
            Assert.True(res.IsSuccess);
            Assert.Equal(1, invoked);
        }
        foreach (var t in new[] { f, fi })
        {
            int invoked = 0;
            var res = await act(t, () => invoked++);
            Assert.True(res.IsFailure);
            Assert.Equal(0, invoked);
            Assert.Same(SharedError, res.Error);
        }
    }
    
    private static async Task AssertTapOnFailure<TIn>(
        ValueTask<Result<TIn>> s, ValueTask<Result<TIn>> si, ValueTask<Result<TIn>> f, ValueTask<Result<TIn>> fi,
        Func<ValueTask<Result<TIn>>, Action, ValueTask<Result<TIn>>> act)
    {
        foreach (var t in new[] { s, si })
        {
            int invoked = 0;
            var res = await act(t, () => invoked++);
            Assert.True(res.IsSuccess);
            Assert.Equal(0, invoked);
        }
        foreach (var t in new[] { f, fi })
        {
            int invoked = 0;
            var res = await act(t, () => invoked++);
            Assert.True(res.IsFailure);
            Assert.Equal(1, invoked);
            Assert.Same(SharedError, res.Error);
        }
    }
    
    private static async Task AssertExecute<TIn>(
        ValueTask<Result<TIn>> s, ValueTask<Result<TIn>> si, ValueTask<Result<TIn>> f, ValueTask<Result<TIn>> fi,
        Func<ValueTask<Result<TIn>>, Action<TIn>, Action<Error>, ValueTask> act)
    {
        foreach (var t in new[] { s, si })
        {
            int sInv = 0, fInv = 0;
            await act(t, v => sInv++, e => fInv++);
            Assert.Equal(1, sInv);
            Assert.Equal(0, fInv);
        }
        foreach (var t in new[] { f, fi })
        {
            int sInv = 0, fInv = 0;
            await act(t, v => sInv++, e => { fInv++; Assert.Same(SharedError, e); });
            Assert.Equal(0, sInv);
            Assert.Equal(1, fInv);
        }
    }
    
    private static async Task AssertMatch<TIn>(
        ValueTask<Result<TIn>> s, ValueTask<Result<TIn>> si, ValueTask<Result<TIn>> f, ValueTask<Result<TIn>> fi,
        Func<ValueTask<Result<TIn>>, Func<TIn, string>, Func<Error, string>, ValueTask<string>> act)
    {
        foreach (var t in new[] { s, si })
        {
            int sInv = 0, fInv = 0;
            var res = await act(t, v => { sInv++; return "A"; }, e => { fInv++; return "B"; });
            Assert.Equal("A", res);
            Assert.Equal(1, sInv);
            Assert.Equal(0, fInv);
        }
        foreach (var t in new[] { f, fi })
        {
            int sInv = 0, fInv = 0;
            var res = await act(t, v => { sInv++; return "A"; }, e => { fInv++; Assert.Same(SharedError, e); return "B"; });
            Assert.Equal("B", res);
            Assert.Equal(0, sInv);
            Assert.Equal(1, fInv);
        }
    }
}
