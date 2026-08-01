using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace EricksonLopez.Result.Tests.Core;

public class MissingSlowPathTests
{
    private static readonly Error TestError = Error.Failure("Test", "Test error");

    [Fact]
    public async Task Missing_Task_Extensions_SlowPath()
    {
        Func<Error, int> me1 = e => 0;
        Func<int, Error, int> me2 = (s, e) => 0;

        await Result.Failure(TestError).AsAsyncResult().MapFailure(me1, 1);
        await Result.Success().AsAsyncResult().MapFailure(me1, 1);
        await Result.Failure(TestError).AsAsyncResult().MapFailure(1, me2, 1);
        await Result.Success().AsAsyncResult().MapFailure(1, me2, 1);

        await TestError.AsAsyncFailedResult<int>().MapFailure(me1, 1);
        await 1.AsAsyncResult().MapFailure(me1, 1);
        await TestError.AsAsyncFailedResult<int>().MapFailure(1, me2, 1);
        await 1.AsAsyncResult().MapFailure(1, me2, 1);
    }
}
