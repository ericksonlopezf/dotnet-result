using System.Linq;
using Xunit;

namespace EricksonLopez.Result.Tests.Core;

public class ResultLinqExtensionsTests
{
    private static readonly Error TestError = Error.Failure("Test.Error", "Test error");
    private static readonly Error TestError2 = Error.Failure("Test.Error2", "Test error 2");

    [Fact]
    public void Select_Success()
    {

        var result = from x in Result.Success(10)
                     select x * 2;
        Assert.True(result.IsSuccess);
        Assert.Equal(20, result.Value);
    }

    [Fact]
    public void Select_Failure()
    {

        var result = from x in Result.Failure<int>(TestError)
                     select x * 2;
        Assert.True(result.IsFailure);
        Assert.Equal(TestError, result.Error);
    }

    [Fact]
    public void SelectMany_Success()
    {

        var result = from x in Result.Success(10)
                     from y in Result.Success(20)
                     select x + y;
        Assert.True(result.IsSuccess);
        Assert.Equal(30, result.Value);
    }

    [Fact]
    public void SelectMany_OuterFailure()
    {

        var result = from x in Result.Failure<int>(TestError)
                     from y in Result.Success(20)
                     select x + y;
        Assert.True(result.IsFailure);
        Assert.Equal(TestError, result.Error);
    }

    [Fact]
    public void SelectMany_InnerFailure()
    {

        var result = from x in Result.Success(10)
                     from y in Result.Failure<int>(TestError2)
                     select x + y;
        Assert.True(result.IsFailure);
        Assert.Equal(TestError2, result.Error);
    }
}
