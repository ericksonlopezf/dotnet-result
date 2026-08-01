using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace EricksonLopez.Result.Tests.Core;

public class MissingValueTaskSlowPathTests
{
    private static readonly Error TestError = Error.Failure("Test", "Test error");

    [Fact]
    public async Task Missing_ValueTask_Extensions_SlowPath()
    {
        Func<int, int, int> m3 = (s, v) => s;
        await 1.AsAsyncValueTaskResult().Map(1, m3);
        await TestError.AsAsyncFailedValueTaskResult<int>().Map(1, m3);
        
        Func<int, int, Result<int>> b3 = (s, v) => Result.Success(s);
        await 1.AsAsyncValueTaskResult().Bind(1, b3);
        await TestError.AsAsyncFailedValueTaskResult<int>().Bind(1, b3);

        Func<int, Result> b5 = v => Result.Success();
        await 1.AsAsyncValueTaskResult().Bind(b5);
        await TestError.AsAsyncFailedValueTaskResult<int>().Bind(b5);

        Func<int, int, Result> b7 = (s, v) => Result.Success();
        await 1.AsAsyncValueTaskResult().Bind(1, b7);
        await TestError.AsAsyncFailedValueTaskResult<int>().Bind(1, b7);

        Action<int, int> t3 = (s, v) => { };
        await 1.AsAsyncValueTaskResult().TapOnSuccess(1, t3);
        await TestError.AsAsyncFailedValueTaskResult<int>().TapOnSuccess(1, t3);
        
        Func<int, Error, Result<int>> r3 = (s, e) => Result.Success(s);
        await 1.AsAsyncValueTaskResult().Recover(1, r3);
        await TestError.AsAsyncFailedValueTaskResult<int>().Recover(1, r3);

        Func<int, int, int> ms1 = (s, v) => s;
        Func<int, Error, int> me1 = (s, e) => s;
        await 1.AsAsyncValueTaskResult().Match(1, ms1, me1);
        await TestError.AsAsyncFailedValueTaskResult<int>().Match(1, ms1, me1);

        Func<int, Error, Error> mapErr1 = (s, e) => e;
        await 1.AsAsyncValueTaskResult().MapError(1, mapErr1);
        await TestError.AsAsyncFailedValueTaskResult<int>().MapError(1, mapErr1);
        
        Action<int, Error> tapErr1 = (s, e) => { };
        await 1.AsAsyncValueTaskResult().TapOnFailure(1, tapErr1);
        await TestError.AsAsyncFailedValueTaskResult<int>().TapOnFailure(1, tapErr1);
        
        Func<int, int, bool> ens1 = (s, v) => true;
        await 1.AsAsyncValueTaskResult().Ensure(1, ens1, TestError);
        await TestError.AsAsyncFailedValueTaskResult<int>().Ensure(1, ens1, TestError);
        
        Action<int, Result<int>> ins1 = (s, r) => { };
        await 1.AsAsyncValueTaskResult().Inspect(1, ins1);
        await TestError.AsAsyncFailedValueTaskResult<int>().Inspect(1, ins1);
    }
}
