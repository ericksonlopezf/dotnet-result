// Copyright © Erickson Lopez. MIT License.
using System;
using System.Reflection;
using AwesomeAssertions;
using EricksonLopez.Result;
using EricksonLopez.Result.Testing;
using EricksonLopez.Result.Testing.NUnit;
using Xunit;

namespace EricksonLopez.Result.Testing.NUnit.Tests;

public class ResultNUnitAssertionTests : IDisposable
{
    public ResultNUnitAssertionTests()
    {
        // Reset state before each test to ensure isolation (via InternalsVisibleTo)
        ResultNUnitAssertionConfig.Reset();
    }

    public void Dispose()
    {
        // Restore default for other tests
        ResultNUnitAssertionConfig.Reset();
        GC.SuppressFinalize(this);
    }

    [Fact]
    public void ResultAssertionNUnitException_DefaultConstructor_SetsDefaultMessage()
    {
        var exception = new ResultAssertionNUnitException();

        exception.Message.Should().Be("A Result assertion failed.");
        exception.InnerException.Should().BeNull();
    }

    [Fact]
    public void ResultAssertionNUnitException_MessageConstructor_SetsMessage()
    {
        var exception = new ResultAssertionNUnitException("Test message");

        exception.Message.Should().Be("Test message");
        exception.InnerException.Should().BeNull();
    }

    [Fact]
    public void ResultAssertionNUnitException_InnerExceptionConstructor_PreservesMessageAndInner()
    {
        var inner = new ResultAssertionException("Inner message");
        var exception = new ResultAssertionNUnitException(inner);

        exception.Message.Should().Be("Inner message");
        exception.InnerException.Should().BeSameAs(inner);
    }

    [Fact]
    public void ResultAssertionNUnitException_InnerExceptionConstructorWithNull_SetsDefaultMessage()
    {
        var exception = new ResultAssertionNUnitException((ResultAssertionException)null!);

        exception.Message.Should().Be("A Result assertion failed.");
        exception.InnerException.Should().BeNull();
    }

    [Fact]
    public void ResultNUnitAssertionConfig_UseNUnitExceptions_SetsExceptionFactory()
    {
        ResultNUnitAssertionConfig.UseNUnitExceptions();

        var factory = ResultAssertionException.ExceptionFactory;

        var exception = factory("Factory test");
        exception.Should().BeOfType<ResultAssertionNUnitException>();
        exception.Message.Should().Be("Factory test");
    }

    [Fact]
    public void ResultNUnitAssertionConfig_UseNUnitExceptions_IsIdempotent()
    {
        ResultNUnitAssertionConfig.UseNUnitExceptions();
        var factory1 = ResultAssertionException.ExceptionFactory;

        ResultNUnitAssertionConfig.UseNUnitExceptions();
        var factory2 = ResultAssertionException.ExceptionFactory;

        factory1.Should().NotBeNull();
        factory2.Should().NotBeNull();

        // Custom sentinel delegate to verify second call returns early when already configured
        Func<string, Exception> customSentinel = static _ => new InvalidOperationException("sentinel");
        ResultAssertionException.ExceptionFactory = customSentinel;

        ResultNUnitAssertionConfig.UseNUnitExceptions();
        Assert.Same(customSentinel, ResultAssertionException.ExceptionFactory);
    }

    [Fact]
    public void ResultNUnitAssertionConfig_Reset_RestoresDefaultExceptionFactory()
    {
        ResultNUnitAssertionConfig.UseNUnitExceptions();

        ResultNUnitAssertionConfig.Reset();

        var factory = ResultAssertionException.ExceptionFactory;

        var exception = factory("Factory test");
        exception.Should().BeOfType<ResultAssertionException>()
            .Which.Message.Should().Be("Factory test");
    }

    [Fact]
    public void ResultNUnitAssertionConfig_UseNUnitExceptions_AfterReset_Works()
    {
        ResultNUnitAssertionConfig.UseNUnitExceptions();
        ResultNUnitAssertionConfig.Reset();

        ResultNUnitAssertionConfig.UseNUnitExceptions();
        var factory = ResultAssertionException.ExceptionFactory;
        var exception = factory("Factory test");
        exception.Should().BeOfType<ResultAssertionNUnitException>();
    }

    [Fact]
    public void ResultNUnitAssertionConfig_AutoConfigure_SetsExceptionFactory()
    {
        ResultNUnitAssertionConfig.Reset();
        ResultNUnitAssertionConfig.AutoConfigure();

        var factory = ResultAssertionException.ExceptionFactory;
        var exception = factory("Factory test");
        exception.Should().BeOfType<ResultAssertionNUnitException>();
    }
}


