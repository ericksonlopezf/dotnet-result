using System;
using Xunit;

namespace EricksonLopez.Result.Testing.Tests;

public class ResultAssertionExceptionAdditionalTests
{
    [Fact]
    public void Constructors_Work()
    {
        var ex1 = new ResultAssertionException();
        Assert.Equal("A Result assertion failed.", ex1.Message);

        var ex2 = new ResultAssertionException("Message");
        Assert.Equal("Message", ex2.Message);

        var inner = new FormatException("Inner");
        var ex3 = new ResultAssertionException("Message", inner);
        Assert.Equal("Message", ex3.Message);
        Assert.Equal(inner, ex3.InnerException);
    }

    [Fact]
    public void ExceptionFactory_SetsAndGets()
    {
        Func<string, Exception> oldFactory = ResultAssertionException.ExceptionFactory;
        try
        {
            Func<string, Exception> newFactory = msg => new InvalidOperationException(msg);
            ResultAssertionException.ExceptionFactory = newFactory;
            Assert.Equal(newFactory, ResultAssertionException.ExceptionFactory);

            var m = typeof(ResultAssertionException).GetMethod("Throw", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            var ex = Assert.Throws<System.Reflection.TargetInvocationException>(() => m!.Invoke(null, new object[] { "test" }));
            Assert.IsType<InvalidOperationException>(ex.InnerException);
        }
        finally
        {
            ResultAssertionException.ExceptionFactory = oldFactory;
        }
    }
}
