using System.Threading.Tasks;
using EricksonLopez.Result.Testing;
using Xunit;

namespace EricksonLopez.Result.Tests.Testing;

public class ResultAssertionsMissingCoverageTests
{
    private static async ValueTask<Result> SlowValueFailure(Error e) { await Task.Yield(); return Result.Failure(e); }

    [Fact]
    public async Task Cover_ShouldHaveErrorTypeAsync_Slow()
    {
        var e = Error.Failure("code", "msg");
        var vt = SlowValueFailure(e);
        await vt.ShouldHaveErrorTypeAsync(ErrorType.Failure);
    }
}
public class ExtraTests
{
    private static async ValueTask<Result<T>> SlowValueFailure<T>(Error e) { await Task.Yield(); return Result.Failure<T>(e); }

    [Fact]
    public async Task Cover_ShouldBeRetryableAsync_T_Slow()
    {
        var e = Error.Create("C", "D").WithRetryability(ErrorRetryability.Transient).Build();
        var vt = SlowValueFailure<int>(e);
        await vt.ShouldBeRetryableAsync();
    }
}
