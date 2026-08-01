#pragma warning disable CA2012
using System;
using System.Threading.Tasks;
using Xunit;

namespace EricksonLopez.Result.Tests.Core;

public class SlowPathExhaustiveTests
{
    private static readonly Error TestError = Error.Failure("E", "M");

    [Fact]
    public async Task Task_Generic_Extensions_SlowPath()
    {
        Func<int, int> m1 = x => x;
        Func<int, Task<int>> m2 = x => Task.FromResult(x);
        Func<int, int, int> m3 = (s, x) => x;
        Func<int, int, Task<int>> m4 = (s, x) => Task.FromResult(x);
        
        await 1.AsAsyncResult().Map(m1);
        await TestError.AsAsyncFailedResult<int>().Map(m1);
        await 1.AsAsyncResult().Map(m2);
        await TestError.AsAsyncFailedResult<int>().Map(m2);
        await 1.AsAsyncResult().Map(1, m3);
        await TestError.AsAsyncFailedResult<int>().Map(1, m3);
        await 1.AsAsyncResult().Map(1, m4);
        await TestError.AsAsyncFailedResult<int>().Map(1, m4);
        
        Func<int, Result<int>> b1 = x => Result.Success(x);
        Func<int, Task<Result<int>>> b2 = x => Task.FromResult(Result.Success(x));
        Func<int, int, Result<int>> b3 = (s, x) => Result.Success(x);
        Func<int, int, Task<Result<int>>> b4 = (s, x) => Task.FromResult(Result.Success(x));

        await 1.AsAsyncResult().Bind(b1);
        await TestError.AsAsyncFailedResult<int>().Bind(b1);
        await 1.AsAsyncResult().Bind(b2);
        await TestError.AsAsyncFailedResult<int>().Bind(b2);
        await 1.AsAsyncResult().Bind(1, b3);
        await TestError.AsAsyncFailedResult<int>().Bind(1, b3);
        
        Func<int, bool> e1 = x => true;
        Func<int, Task<bool>> e2 = x => Task.FromResult(true);
        Func<int, int, bool> e3 = (s, x) => true;
        Func<int, int, Task<bool>> e4 = (s, x) => Task.FromResult(true);

        await 1.AsAsyncResult().Ensure(e1, TestError);
        await TestError.AsAsyncFailedResult<int>().Ensure(e1, TestError);
        await 1.AsAsyncResult().Ensure(e2, TestError);
        await TestError.AsAsyncFailedResult<int>().Ensure(e2, TestError);
        await 1.AsAsyncResult().Ensure(1, e3, TestError);
        await TestError.AsAsyncFailedResult<int>().Ensure(1, e3, TestError);
        await 1.AsAsyncResult().Ensure(1, e4, TestError);
        await TestError.AsAsyncFailedResult<int>().Ensure(1, e4, TestError);
        
        Action<int> t1 = x => { };
        Func<int, Task> t2 = x => Task.CompletedTask;
        Action<int, int> t3 = (s, x) => { };
        Func<int, int, Task> t4 = (s, x) => Task.CompletedTask;

        await 1.AsAsyncResult().TapOnSuccess(t1);
        await TestError.AsAsyncFailedResult<int>().TapOnSuccess(t1);
        await 1.AsAsyncResult().TapOnSuccess(t2);
        await TestError.AsAsyncFailedResult<int>().TapOnSuccess(t2);
        await 1.AsAsyncResult().TapOnSuccess(1, t3);
        await TestError.AsAsyncFailedResult<int>().TapOnSuccess(1, t3);

        
        Func<Error, Result<int>> r1 = e => Result.Success(1);
        Func<Error, Task<Result<int>>> r2 = e => Task.FromResult(Result.Success(1));
        Func<int, Error, Result<int>> r3 = (s, e) => Result.Success(1);
        Func<int, Error, Task<Result<int>>> r4 = (s, e) => Task.FromResult(Result.Success(1));

        await TestError.AsAsyncFailedResult<int>().Recover(r1);
        await 1.AsAsyncResult().Recover(r1);
        await TestError.AsAsyncFailedResult<int>().Recover(r2);
        await 1.AsAsyncResult().Recover(r2);
        await TestError.AsAsyncFailedResult<int>().Recover(1, r3);
        await 1.AsAsyncResult().Recover(1, r3);
        await TestError.AsAsyncFailedResult<int>().Recover(1, r4);
        await 1.AsAsyncResult().Recover(1, r4);

        Func<int, int> ms1 = x => x;
        Func<Error, int> me1 = e => 0;
        Func<int, Task<int>> ms2 = x => Task.FromResult(x);
        Func<Error, Task<int>> me2 = e => Task.FromResult(0);
        
        await 1.AsAsyncResult().Match(ms1, me1);
        await TestError.AsAsyncFailedResult<int>().Match(ms1, me1);
        await 1.AsAsyncResult().Match(ms2, me2);
        await TestError.AsAsyncFailedResult<int>().Match(ms2, me2);
        
        Action<int> ss1 = x => { };
        Action<Error> se1 = e => { };
        Func<int, Task> ss2 = x => Task.CompletedTask;
        Func<Error, Task> se2 = e => Task.CompletedTask;

        await 1.AsAsyncResult().Execute(ss1, se1);
        await TestError.AsAsyncFailedResult<int>().Execute(ss1, se1);
        
        Func<Error, Error> me1_ = e => TestError;
        Func<int, Error, Error> me3_ = (s, e) => TestError;

        await TestError.AsAsyncFailedResult<int>().MapError(me1_);
        await 1.AsAsyncResult().MapError(me1_);
        await TestError.AsAsyncFailedResult<int>().MapError(1, me3_);
        await 1.AsAsyncResult().MapError(1, me3_);

        Action<Error> te1 = e => { };
        Action<int, Error> te3 = (s, e) => { };

        await TestError.AsAsyncFailedResult<int>().TapOnFailure(te1);
        await 1.AsAsyncResult().TapOnFailure(te1);
        await TestError.AsAsyncFailedResult<int>().TapOnFailure(1, te3);
        await 1.AsAsyncResult().TapOnFailure(1, te3);
    
}
}
