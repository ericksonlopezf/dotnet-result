// Copyright © Erickson Lopez. MIT License.
using System;
using EricksonLopez.Result;
using EricksonLopez.Result.Testing;
using Xunit;

namespace EricksonLopez.Result.Testing.Tests;

public class ResultAssertionExceptionTests
{
    [Fact]
    public void Constructor_Default_SetsStandardMessage()
    {

        var exception = new ResultAssertionException();

        Assert.Equal("A Result assertion failed.", exception.Message);
    }

    [Fact]
    public void Constructor_WithMessage_SetsMessageProperty()
    {
        const string expectedMessage = "Custom error message.";

        var exception = new ResultAssertionException(expectedMessage);

        Assert.Equal(expectedMessage, exception.Message);
    }

    [Fact]
    public void Constructor_WithMessageAndInnerException_SetsMessageAndInnerProperties()
    {
        const string expectedMessage = "Custom error message with inner.";
        var innerException = new InvalidOperationException("Inner error.");

        var exception = new ResultAssertionException(expectedMessage, innerException);

        Assert.Equal(expectedMessage, exception.Message);
        Assert.Same(innerException, exception.InnerException);
    }

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

    [Fact]
    public void ExceptionFactory_Throws_OnNull()
    {
        Assert.Throws<ArgumentNullException>(() => ResultAssertionException.ExceptionFactory = null!);
    }
}



