using System;
using System.Reflection;
using AwesomeAssertions;
using Xunit;
using EricksonLopez.Result.Testing;
using EricksonLopez.Result.Testing.NUnit;

namespace EricksonLopez.Result.Tests.Testing.NUnit;

public class ResultNUnitAssertionTests : IDisposable
{
    private readonly Type _nunitExceptionType;
    
    public ResultNUnitAssertionTests()
    {
        // Reset state before each test to ensure isolation
        var method = typeof(ResultNUnitAssertionConfig).GetMethod("Reset", BindingFlags.NonPublic | BindingFlags.Static)!;
        method.Invoke(null, null);
        
        var assembly = typeof(ResultNUnitAssertionConfig).Assembly;
        _nunitExceptionType = assembly.GetType("EricksonLopez.Result.Testing.NUnit.ResultAssertionNUnitException")!;
    }

    public void Dispose()
    {
        // Restore default for other tests
        var method = typeof(ResultNUnitAssertionConfig).GetMethod("Reset", BindingFlags.NonPublic | BindingFlags.Static)!;
        method.Invoke(null, null);
        GC.SuppressFinalize(this);
    }

    [Fact]
    public void ResultAssertionNUnitException_DefaultConstructor_SetsDefaultMessage()
    {
        var exception = (Exception)Activator.CreateInstance(_nunitExceptionType)!;
        
        exception.Message.Should().Be("A Result assertion failed.");
        exception.InnerException.Should().BeNull();
    }

    [Fact]
    public void ResultAssertionNUnitException_MessageConstructor_SetsMessage()
    {
        var exception = (Exception)Activator.CreateInstance(_nunitExceptionType, "Test message")!;
        
        exception.Message.Should().Be("Test message");
        exception.InnerException.Should().BeNull();
    }

    [Fact]
    public void ResultAssertionNUnitException_InnerExceptionConstructor_PreservesMessageAndInner()
    {
        var inner = new ResultAssertionException("Inner message");
        var exception = (Exception)Activator.CreateInstance(_nunitExceptionType, inner)!;
        
        exception.Message.Should().Be("Inner message");
        exception.InnerException.Should().BeSameAs(inner);
    }

    [Fact]
    public void ResultAssertionNUnitException_InnerExceptionConstructorWithNull_SetsDefaultMessage()
    {
        var constructor = _nunitExceptionType.GetConstructor(new[] { typeof(ResultAssertionException) })!;
        var exception = (Exception)constructor.Invoke(new object?[] { null })!;
        
        exception.Message.Should().Be("A Result assertion failed.");
        exception.InnerException.Should().BeNull();
    }

    [Fact]
    public void ResultNUnitAssertionConfig_UseNUnitExceptions_SetsExceptionFactory()
    {
        ResultNUnitAssertionConfig.UseNUnitExceptions();
        
        var factory = ResultAssertionException.ExceptionFactory;
        
        var exception = factory("Factory test");
        exception.Should().BeOfType(_nunitExceptionType);
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
    }

    [Fact]
    public void ResultNUnitAssertionConfig_Reset_RestoresDefaultExceptionFactory()
    {
        ResultNUnitAssertionConfig.UseNUnitExceptions();
        
        var method = typeof(ResultNUnitAssertionConfig).GetMethod("Reset", BindingFlags.NonPublic | BindingFlags.Static)!;
        method.Invoke(null, null);
        
        var factory = ResultAssertionException.ExceptionFactory;
        
        var exception = factory("Factory test");
        exception.Should().BeOfType<ResultAssertionException>()
            .Which.Message.Should().Be("Factory test");
    }

    [Fact]
    public void ResultNUnitAssertionConfig_UseNUnitExceptions_AfterReset_Works()
    {
        ResultNUnitAssertionConfig.UseNUnitExceptions();
        var method = typeof(ResultNUnitAssertionConfig).GetMethod("Reset", BindingFlags.NonPublic | BindingFlags.Static)!;
        method.Invoke(null, null);
        
        ResultNUnitAssertionConfig.UseNUnitExceptions();
        var factory = ResultAssertionException.ExceptionFactory;
        var exception = factory("Factory test");
        exception.Should().BeOfType(_nunitExceptionType);
    }

    [Fact]
    public void ResultNUnitAssertionConfig_AutoConfigure_SetsExceptionFactory()
    {
        var methodReset = typeof(ResultNUnitAssertionConfig).GetMethod("Reset", BindingFlags.NonPublic | BindingFlags.Static)!;
        methodReset.Invoke(null, null);
        
        var method = typeof(ResultNUnitAssertionConfig).GetMethod("AutoConfigure", BindingFlags.NonPublic | BindingFlags.Static)!;
        method.Invoke(null, null);
        
        var factory = ResultAssertionException.ExceptionFactory;
        var exception = factory("Factory test");
        exception.Should().BeOfType(_nunitExceptionType);
    }
}
