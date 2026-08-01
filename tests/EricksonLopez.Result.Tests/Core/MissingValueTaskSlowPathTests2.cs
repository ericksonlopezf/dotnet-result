using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace EricksonLopez.Result.Tests.Core;

public class MissingValueTaskSlowPathTests2
{
    private static readonly Error TestError = Error.Failure("Test", "Test error");

    [Fact]
    public async Task Missing_ValueTask_NonGeneric_Extensions_SlowPath()
    {

        Func<int, Result> b3 = s => Result.Success();
        await Result.Success().AsAsyncValueTaskResult().Bind(1, b3);
        await Result.Failure(TestError).AsAsyncValueTaskResult().Bind(1, b3);
        
        Action<int> t3 = s => { };
        await Result.Success().AsAsyncValueTaskResult().TapOnSuccess(1, t3);
        await Result.Failure(TestError).AsAsyncValueTaskResult().TapOnSuccess(1, t3);
        
        Func<int, Error, Result> r3 = (s, e) => Result.Success();
        await Result.Success().AsAsyncValueTaskResult().Recover(1, r3);
        await Result.Failure(TestError).AsAsyncValueTaskResult().Recover(1, r3);
        
        Func<int, int> ms1 = s => s;
        Func<int, Error, int> me1 = (s, e) => s;
        await Result.Success().AsAsyncValueTaskResult().Match(1, ms1, me1);
        await Result.Failure(TestError).AsAsyncValueTaskResult().Match(1, ms1, me1);

        Func<int, Error, Error> mapErr1 = (s, e) => e;
        await Result.Success().AsAsyncValueTaskResult().MapError(1, mapErr1);
        await Result.Failure(TestError).AsAsyncValueTaskResult().MapError(1, mapErr1);
        
        Action<int, Error> tapErr1 = (s, e) => { };
        await Result.Success().AsAsyncValueTaskResult().TapOnFailure(1, tapErr1);
        await Result.Failure(TestError).AsAsyncValueTaskResult().TapOnFailure(1, tapErr1);

        Func<int, bool> ens1 = s => true;
        await Result.Success().AsAsyncValueTaskResult().Ensure(1, ens1, TestError);
        await Result.Failure(TestError).AsAsyncValueTaskResult().Ensure(1, ens1, TestError);

        Action<int, Result> ins1 = (s, r) => { };
        await Result.Success().AsAsyncValueTaskResult().Inspect(1, ins1);
        await Result.Failure(TestError).AsAsyncValueTaskResult().Inspect(1, ins1);
    }
}
