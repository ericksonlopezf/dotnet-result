using System;
using Xunit;
using EricksonLopez.Result.Testing;

namespace EricksonLopez.Result.Tests.Testing;

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
}
