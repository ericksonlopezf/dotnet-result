// Copyright © Erickson Lopez. MIT License.
using System;
using System.Threading.Tasks;
using EricksonLopez.Result;
using EricksonLopez.Result.Testing;
using Xunit;

namespace EricksonLopez.Result.Tests.Core;

public class ResultExtensionsGenericTaskSlowPathTests : ResultExtensionsTestsBase
{

    [Fact]
    public async Task Map_WhenExecutedOnSlowPath_ReturnsMappedResult()
    {
        Func<int, int> m1 = x => x * 2;
        Func<int, Task<int>> m2 = x => Task.FromResult(x * 2);
        Func<int, int, int> m3 = (s, x) => s + x;
        Func<int, int, Task<int>> m4 = (s, x) => Task.FromResult(s + x);

        var map1 = await 1.AsAsyncResult().Map(m1);
        map1.ShouldBeSuccess();
        Assert.Equal(2, map1.Value);

        var map1F = await TestError.AsAsyncFailedResult<int>().Map(m1);
        map1F.ShouldBeFailure();

        var map2 = await 1.AsAsyncResult().Map(m2);
        map2.ShouldBeSuccess();
        Assert.Equal(2, map2.Value);

        var map2F = await TestError.AsAsyncFailedResult<int>().Map(m2);
        map2F.ShouldBeFailure();

        var map3 = await 1.AsAsyncResult().Map(1, m3);
        map3.ShouldBeSuccess();
        Assert.Equal(2, map3.Value);

        var map3F = await TestError.AsAsyncFailedResult<int>().Map(1, m3);
        map3F.ShouldBeFailure();

        var map4 = await 1.AsAsyncResult().Map(1, m4);
        map4.ShouldBeSuccess();
        Assert.Equal(2, map4.Value);

        var map4F = await TestError.AsAsyncFailedResult<int>().Map(1, m4);
        map4F.ShouldBeFailure();
    }

    [Fact]
    public async Task Bind_WhenExecutedOnSlowPath_ReturnsBoundResult()
    {
        Func<int, Result<int>> b1 = x => Result.Success(x * 2);
        Func<int, Task<Result<int>>> b2 = x => Task.FromResult(Result.Success(x * 2));
        Func<int, int, Result<int>> b3 = (s, x) => Result.Success(s + x);

        var bind1 = await 1.AsAsyncResult().Bind(b1);
        bind1.ShouldBeSuccess();
        Assert.Equal(2, bind1.Value);

        var bind1F = await TestError.AsAsyncFailedResult<int>().Bind(b1);
        bind1F.ShouldBeFailure();

        var bind2 = await 1.AsAsyncResult().Bind(b2);
        bind2.ShouldBeSuccess();
        Assert.Equal(2, bind2.Value);

        var bind2F = await TestError.AsAsyncFailedResult<int>().Bind(b2);
        bind2F.ShouldBeFailure();

        var bind3 = await 1.AsAsyncResult().Bind(1, b3);
        bind3.ShouldBeSuccess();
        Assert.Equal(2, bind3.Value);

        var bind3F = await TestError.AsAsyncFailedResult<int>().Bind(1, b3);
        bind3F.ShouldBeFailure();
    }

    [Fact]
    public async Task Ensure_WhenExecutedOnSlowPath_ValidatesCondition()
    {
        Func<int, bool> e1 = x => x > 0;
        Func<int, Task<bool>> e2 = x => Task.FromResult(x > 0);
        Func<int, int, bool> e3 = (s, x) => x > s;
        Func<int, int, Task<bool>> e4 = (s, x) => Task.FromResult(x > s);

        var ens1 = await 1.AsAsyncResult().Ensure(e1, TestError);
        ens1.ShouldBeSuccess();

        var ens1F = await TestError.AsAsyncFailedResult<int>().Ensure(e1, TestError);
        ens1F.ShouldBeFailure();

        var ens2 = await 1.AsAsyncResult().Ensure(e2, TestError);
        ens2.ShouldBeSuccess();

        var ens2F = await TestError.AsAsyncFailedResult<int>().Ensure(e2, TestError);
        ens2F.ShouldBeFailure();

        var ens3 = await 1.AsAsyncResult().Ensure(0, e3, TestError);
        ens3.ShouldBeSuccess();

        var ens3F = await TestError.AsAsyncFailedResult<int>().Ensure(0, e3, TestError);
        ens3F.ShouldBeFailure();

        var ens4 = await 1.AsAsyncResult().Ensure(0, e4, TestError);
        ens4.ShouldBeSuccess();

        var ens4F = await TestError.AsAsyncFailedResult<int>().Ensure(0, e4, TestError);
        ens4F.ShouldBeFailure();
    }

    [Fact]
    public async Task TapOnSuccess_WhenExecutedOnSlowPath_ExecutesSideEffectOnlyOnSuccess()
    {
        bool tapSuccessCalled = false;
        Action<int> t1 = x => { tapSuccessCalled = true; };
        Func<int, Task> t2 = x => { tapSuccessCalled = true; return Task.CompletedTask; };
        Action<int, int> t3 = (s, x) => { tapSuccessCalled = true; };

        var tap1 = await 1.AsAsyncResult().TapOnSuccess(t1);
        tap1.ShouldBeSuccess();
        Assert.True(tapSuccessCalled);

        tapSuccessCalled = false;
        var tap1F = await TestError.AsAsyncFailedResult<int>().TapOnSuccess(t1);
        tap1F.ShouldBeFailure();
        Assert.False(tapSuccessCalled);

        var tap2 = await 1.AsAsyncResult().TapOnSuccess(t2);
        tap2.ShouldBeSuccess();

        var tap3 = await 1.AsAsyncResult().TapOnSuccess(1, t3);
        tap3.ShouldBeSuccess();
    }

    [Fact]
    public async Task Recover_WhenExecutedOnSlowPath_RecoversFromFailure()
    {
        Func<Error, Result<int>> r1 = e => Result.Success(42);
        Func<Error, Task<Result<int>>> r2 = e => Task.FromResult(Result.Success(42));
        Func<int, Error, Result<int>> r3 = (s, e) => Result.Success(s);
        Func<int, Error, Task<Result<int>>> r4 = (s, e) => Task.FromResult(Result.Success(s));

        var rec1 = await TestError.AsAsyncFailedResult<int>().Recover(r1);
        rec1.ShouldBeSuccess();
        Assert.Equal(42, rec1.Value);

        var rec1S = await 1.AsAsyncResult().Recover(r1);
        rec1S.ShouldBeSuccess();
        Assert.Equal(1, rec1S.Value);

        var rec2 = await TestError.AsAsyncFailedResult<int>().Recover(r2);
        rec2.ShouldBeSuccess();
        Assert.Equal(42, rec2.Value);

        var rec3 = await TestError.AsAsyncFailedResult<int>().Recover(10, r3);
        rec3.ShouldBeSuccess();
        Assert.Equal(10, rec3.Value);

        var rec4 = await TestError.AsAsyncFailedResult<int>().Recover(10, r4);
        rec4.ShouldBeSuccess();
        Assert.Equal(10, rec4.Value);
    }

    [Fact]
    public async Task Match_WhenExecutedOnSlowPath_EvaluatesAppropriateBranch()
    {
        Func<int, int> ms1 = x => x * 2;
        Func<Error, int> me1 = e => -1;
        Func<int, Task<int>> ms2 = x => Task.FromResult(x * 2);
        Func<Error, Task<int>> me2 = e => Task.FromResult(-1);

        var mch1 = await 1.AsAsyncResult().Match(ms1, me1);
        Assert.Equal(2, mch1);

        var mch1F = await TestError.AsAsyncFailedResult<int>().Match(ms1, me1);
        Assert.Equal(-1, mch1F);

        var mch2 = await (await 1.AsAsyncResult().Match(ms2, me2));
        Assert.Equal(2, mch2);

        var mch2F = await (await TestError.AsAsyncFailedResult<int>().Match(ms2, me2));
        Assert.Equal(-1, mch2F);
    }

    [Fact]
    public async Task Execute_WhenExecutedOnSlowPath_InvokesAppropriateAction()
    {
        int execSuccess = 0;
        Action<int> ss1 = x => { execSuccess = x; };
        Action<Error> se1 = e => { execSuccess = -1; };

        await 1.AsAsyncResult().Execute(ss1, se1);
        Assert.Equal(1, execSuccess);

        await TestError.AsAsyncFailedResult<int>().Execute(ss1, se1);
        Assert.Equal(-1, execSuccess);
    }

    [Fact]
    public async Task MapError_WhenExecutedOnSlowPath_TransformsError()
    {
        Func<Error, Error> me1_ = e => Error.Validation("V", "Mapped");
        Func<int, Error, Error> me3_ = (s, e) => Error.Validation("V", "MappedState");

        var mapErr1 = await TestError.AsAsyncFailedResult<int>().MapError(me1_);
        mapErr1.ShouldBeFailure();
        Assert.Equal("V", mapErr1.Error.Code);

        var mapErr1S = await 1.AsAsyncResult().MapError(me1_);
        mapErr1S.ShouldBeSuccess();

        var mapErr3 = await TestError.AsAsyncFailedResult<int>().MapError(1, me3_);
        mapErr3.ShouldBeFailure();
        Assert.Equal("V", mapErr3.Error.Code);
    }

    [Fact]
    public async Task TapOnFailure_WhenExecutedOnSlowPath_ExecutesSideEffectOnlyOnFailure()
    {
        bool tapFailureCalled = false;
        Action<Error> te1 = e => { tapFailureCalled = true; };
        Action<int, Error> te3 = (s, e) => { tapFailureCalled = true; };

        var tapF1 = await TestError.AsAsyncFailedResult<int>().TapOnFailure(te1);
        tapF1.ShouldBeFailure();
        Assert.True(tapFailureCalled);

        tapFailureCalled = false;
        var tapF1S = await 1.AsAsyncResult().TapOnFailure(te1);
        tapF1S.ShouldBeSuccess();
        Assert.False(tapFailureCalled);

        var tapF3 = await TestError.AsAsyncFailedResult<int>().TapOnFailure(1, te3);
        tapF3.ShouldBeFailure();
        Assert.True(tapFailureCalled);
    }
}
