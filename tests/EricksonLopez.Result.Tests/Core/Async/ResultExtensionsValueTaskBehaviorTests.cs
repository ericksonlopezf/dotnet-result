// Copyright © Erickson Lopez. MIT License.
#pragma warning disable CA2012 // ValueTask instances returned from method calls should be awaited directly
using System;
using System.Threading;
using System.Threading.Tasks;
using EricksonLopez.Result;
using EricksonLopez.Result.Testing;
using Xunit;

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
        r1.ShouldBeSuccess();

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
    public async Task Map_ValueTask_AsyncMapper_ExplicitValueTask_KillsNoCoverageMutants()
    {
        Func<int, ValueTask<string>> asyncMapper = async v => { await Task.Yield(); return $"mapped-{v}"; };
        Func<int, int, ValueTask<string>> asyncStateMapper = async (s, v) => { await Task.Yield(); return $"mapped-{s}-{v}"; };

        // Fast path - Failure: mapper should NOT be called, should return Failure with exact Error
        int fastFailInvoked = 0;
        var r1 = await F<int>().Map(async v => { fastFailInvoked++; await Task.Yield(); return "val"; });
        Assert.Equal(0, fastFailInvoked);
        r1.ShouldBeFailure();
        Assert.Same(SharedError, r1.Error);

        int fastFailStateInvoked = 0;
        var r2 = await F<int>().Map(99, async (s, v) => { fastFailStateInvoked++; await Task.Yield(); return "val"; });
        Assert.Equal(0, fastFailStateInvoked);
        r2.ShouldBeFailure();
        Assert.Same(SharedError, r2.Error);

        // Slow path - Failure: mapper should NOT be called, should return Failure with exact Error
        int slowFailInvoked = 0;
        var r3 = await IF<int>().Map(async v => { slowFailInvoked++; await Task.Yield(); return "val"; });
        Assert.Equal(0, slowFailInvoked);
        r3.ShouldBeFailure();
        Assert.Same(SharedError, r3.Error);

        int slowFailStateInvoked = 0;
        var r4 = await IF<int>().Map(99, async (s, v) => { slowFailStateInvoked++; await Task.Yield(); return "val"; });
        Assert.Equal(0, slowFailStateInvoked);
        r4.ShouldBeFailure();
        Assert.Same(SharedError, r4.Error);

        // Fast path - Success: mapper called, returns mapped value
        var r5 = await S(42).Map(asyncMapper);
        r5.ShouldBeSuccess();
        Assert.Equal("mapped-42", r5.Value);

        var r6 = await S(42).Map(10, asyncStateMapper);
        r6.ShouldBeSuccess();
        Assert.Equal("mapped-10-42", r6.Value);

        // Slow path - Success: mapper called, returns mapped value
        var r7 = await IS(42).Map(asyncMapper);
        r7.ShouldBeSuccess();
        Assert.Equal("mapped-42", r7.Value);

        var r8 = await IS(42).Map(10, asyncStateMapper);
        r8.ShouldBeSuccess();
        Assert.Equal("mapped-10-42", r8.Value);
    }

    [Fact]
    public async Task Recover_ValueTask_AsyncRecovery_Matrix_KillsAllMutants()
    {
        var recoveryError = Error.Failure("Recovery.Failed", "Recovery error");
        Func<Error, ValueTask<Result<int>>> recoverToSuccess = async e => { await Task.Yield(); return Result.Success(100); };
        Func<Error, ValueTask<Result<int>>> recoverToFailure = async e => { await Task.Yield(); return Result.Failure<int>(recoveryError); };
        Func<int, Error, ValueTask<Result<int>>> recoverStateToSuccess = async (s, e) => { await Task.Yield(); return Result.Success(s + 100); };
        Func<int, Error, ValueTask<Result<int>>> recoverStateToFailure = async (s, e) => { await Task.Yield(); return Result.Failure<int>(recoveryError); };

        // 1. Fast Path - Success: recovery should NOT be called, original value returned
        int fastSuccessInvoked = 0;
        var r1 = await S(42).Recover(async e => { fastSuccessInvoked++; await Task.Yield(); return Result.Success(999); });
        Assert.Equal(0, fastSuccessInvoked);
        r1.ShouldBeSuccess();
        Assert.Equal(42, r1.Value);

        int fastSuccessStateInvoked = 0;
        var r2 = await S(42).Recover(10, async (s, e) => { fastSuccessStateInvoked++; await Task.Yield(); return Result.Success(999); });
        Assert.Equal(0, fastSuccessStateInvoked);
        r2.ShouldBeSuccess();
        Assert.Equal(42, r2.Value);

        // 2. Fast Path - Failure -> Recovery Success
        int fastFailSuccessInvoked = 0;
        var r3 = await F<int>().Recover(async e => { fastFailSuccessInvoked++; return await recoverToSuccess(e); });
        Assert.Equal(1, fastFailSuccessInvoked);
        r3.ShouldBeSuccess();
        Assert.Equal(100, r3.Value);

        int fastFailStateSuccessInvoked = 0;
        var r4 = await F<int>().Recover(50, async (s, e) => { fastFailStateSuccessInvoked++; return await recoverStateToSuccess(s, e); });
        Assert.Equal(1, fastFailStateSuccessInvoked);
        r4.ShouldBeSuccess();
        Assert.Equal(150, r4.Value);

        // 3. Fast Path - Failure -> Recovery Failure
        int fastFailFailInvoked = 0;
        var r5 = await F<int>().Recover(async e => { fastFailFailInvoked++; return await recoverToFailure(e); });
        Assert.Equal(1, fastFailFailInvoked);
        r5.ShouldBeFailure();
        Assert.Same(recoveryError, r5.Error);

        int fastFailStateFailInvoked = 0;
        var r6 = await F<int>().Recover(50, async (s, e) => { fastFailStateFailInvoked++; return await recoverStateToFailure(s, e); });
        Assert.Equal(1, fastFailStateFailInvoked);
        r6.ShouldBeFailure();
        Assert.Same(recoveryError, r6.Error);

        // 4. Slow Path - Success: recovery should NOT be called, original value returned
        int slowSuccessInvoked = 0;
        var r7 = await IS(42).Recover(async e => { slowSuccessInvoked++; await Task.Yield(); return Result.Success(999); });
        Assert.Equal(0, slowSuccessInvoked);
        r7.ShouldBeSuccess();
        Assert.Equal(42, r7.Value);

        int slowSuccessStateInvoked = 0;
        var r8 = await IS(42).Recover(10, async (s, e) => { slowSuccessStateInvoked++; await Task.Yield(); return Result.Success(999); });
        Assert.Equal(0, slowSuccessStateInvoked);
        r8.ShouldBeSuccess();
        Assert.Equal(42, r8.Value);

        // 5. Slow Path - Failure -> Recovery Success
        int slowFailSuccessInvoked = 0;
        var r9 = await IF<int>().Recover(async e => { slowFailSuccessInvoked++; return await recoverToSuccess(e); });
        Assert.Equal(1, slowFailSuccessInvoked);
        r9.ShouldBeSuccess();
        Assert.Equal(100, r9.Value);

        int slowFailStateSuccessInvoked = 0;
        var r10 = await IF<int>().Recover(50, async (s, e) => { slowFailStateSuccessInvoked++; return await recoverStateToSuccess(s, e); });
        Assert.Equal(1, slowFailStateSuccessInvoked);
        r10.ShouldBeSuccess();
        Assert.Equal(150, r10.Value);

        // 6. Slow Path - Failure -> Recovery Failure
        int slowFailFailInvoked = 0;
        var r11 = await IF<int>().Recover(async e => { slowFailFailInvoked++; return await recoverToFailure(e); });
        Assert.Equal(1, slowFailFailInvoked);
        r11.ShouldBeFailure();
        Assert.Same(recoveryError, r11.Error);

        int slowFailStateFailInvoked = 0;
        var r12 = await IF<int>().Recover(50, async (s, e) => { slowFailStateFailInvoked++; return await recoverStateToFailure(s, e); });
        Assert.Equal(1, slowFailStateFailInvoked);
        r12.ShouldBeFailure();
        Assert.Same(recoveryError, r12.Error);
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
            res.ShouldBeSuccess();
            Assert.Equal(outVal, res.Value);
            Assert.Equal(1, invoked);
        }
        foreach (var t in new[] { f, fi })
        {
            int invoked = 0;
            var res = await act(t, v => { invoked++; return outVal; });
            res.ShouldBeFailure();
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
            res.ShouldBeSuccess();
            Assert.Equal(1, invoked);
        }
        foreach (var t in new[] { f, fi })
        {
            int invoked = 0;
            var res = await act(t, v => { invoked++; return outVal; });
            res.ShouldBeFailure();
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
            res.ShouldBeSuccess();
            Assert.Equal(1, invoked);
        }
        foreach (var t in new[] { f, fi })
        {
            int invoked = 0;
            var res = await act(t, () => { invoked++; return "A"; });
            res.ShouldBeFailure();
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
            res.ShouldBeSuccess();
            Assert.Equal(1, invoked);
        }
        foreach (var t in new[] { f, fi })
        {
            int invoked = 0;
            var res = await act(t, () => invoked++);
            res.ShouldBeFailure();
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
            res.ShouldBeSuccess();
            Assert.Equal(0, invoked);
        }
        foreach (var t in new[] { f, fi })
        {
            int invoked = 0;
            var res = await act(t, () => invoked++);
            res.ShouldBeFailure();
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




