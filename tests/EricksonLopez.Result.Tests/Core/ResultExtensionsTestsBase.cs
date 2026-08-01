using System.Threading;
using Xunit;

namespace EricksonLopez.Result.Tests.Core;

public class ResultExtensionsTestsBase
{

    protected static readonly Error TestError = Error.Failure("Test.Error", "Test error message");
    protected static readonly Error TestError2 = Error.Failure("Test.Error2", "Test error message 2");

    protected static CancellationToken CanceledToken
    {
        get
        {
            var cts = new CancellationTokenSource();
            cts.Cancel();
            return cts.Token;
        }
    }

    protected static void AssertFailureInvariant<T>(Result<T> result, Error expectedError)
    {
        Assert.True(result.IsFailure);
        Assert.False(result.IsSuccess);
        Assert.Same(expectedError, result.Error);
    }

    protected static void AssertFailureInvariant(Result result, Error expectedError)
    {
        Assert.True(result.IsFailure);
        Assert.False(result.IsSuccess);
        Assert.Same(expectedError, result.Error);
    }
}
