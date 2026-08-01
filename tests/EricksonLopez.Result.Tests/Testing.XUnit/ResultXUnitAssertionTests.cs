using System;
using System.Reflection;
using AwesomeAssertions;
using Xunit;
using EricksonLopez.Result.Testing;
using EricksonLopez.Result.Testing.XUnit;

namespace EricksonLopez.Result.Tests.Testing.XUnit;

public class ResultXUnitAssertionTests : IDisposable
{
    private readonly Type _xunitExceptionType;
    
    public ResultXUnitAssertionTests()
    {
        // Reset state before each test to ensure isolation
        var method = typeof(ResultXUnitAssertionConfig).GetMethod("Reset", BindingFlags.NonPublic | BindingFlags.Static)!;
        method.Invoke(null, null);
        
        // Find the exception type dynamically to avoid CS0433 ambiguous type errors
        // since xunit.v3.assert is excluded from compile assets.
        var assembly = typeof(ResultXUnitAssertionConfig).Assembly;
        _xunitExceptionType = assembly.GetType("EricksonLopez.Result.Testing.XUnit.ResultAssertionXUnitException")!;
    }

    public void Dispose()
    {
        // Restore default for other tests
        var method = typeof(ResultXUnitAssertionConfig).GetMethod("Reset", BindingFlags.NonPublic | BindingFlags.Static)!;
        method.Invoke(null, null);
        GC.SuppressFinalize(this);
    }

    [Fact]
    public void ResultAssertionXUnitException_DefaultConstructor_SetsDefaultMessage()
    {
        var exception = (Exception)Activator.CreateInstance(_xunitExceptionType)!;
        
        exception.Message.Should().Be("A Result assertion failed.");
        exception.InnerException.Should().BeNull();
    }

    [Fact]
    public void ResultAssertionXUnitException_MessageConstructor_SetsMessage()
    {
        var exception = (Exception)Activator.CreateInstance(_xunitExceptionType, "Test message")!;
        
        exception.Message.Should().Be("Test message");
        exception.InnerException.Should().BeNull();
    }

    [Fact]
    public void ResultAssertionXUnitException_InnerExceptionConstructor_PreservesMessageAndInner()
    {
        var inner = new ResultAssertionException("Inner message");
        var exception = (Exception)Activator.CreateInstance(_xunitExceptionType, inner)!;
        
        exception.Message.Should().Be("Inner message");
        exception.InnerException.Should().BeSameAs(inner);
    }
    
    [Fact]
    public void ResultAssertionXUnitException_InnerExceptionConstructorWithNull_SetsDefaultMessage()
    {
        var constructor = _xunitExceptionType.GetConstructor(new[] { typeof(ResultAssertionException) })!;
        var exception = (Exception)constructor.Invoke(new object?[] { null })!;
        
        exception.Message.Should().Be("A Result assertion failed.");
        exception.InnerException.Should().BeNull();
    }

    [Fact]
    public void ResultXUnitAssertionConfig_UseXUnitExceptions_SetsExceptionFactory()
    {
        ResultXUnitAssertionConfig.UseXUnitExceptions();
        
        var factory = ResultAssertionException.ExceptionFactory;
        
        var exception = factory("Factory test");
        exception.Should().BeOfType(_xunitExceptionType);
        exception.Message.Should().Be("Factory test");
    }

    [Fact]
    public void ResultXUnitAssertionConfig_UseXUnitExceptions_IsIdempotent()
    {
        ResultXUnitAssertionConfig.UseXUnitExceptions();
        var factory1 = ResultAssertionException.ExceptionFactory;
        
        ResultXUnitAssertionConfig.UseXUnitExceptions();
        var factory2 = ResultAssertionException.ExceptionFactory;
        
        // Assert no exception was thrown and state was maintained
        factory1.Should().NotBeNull();
        factory2.Should().NotBeNull();
    }

    [Fact]
    public void ResultXUnitAssertionConfig_Reset_RestoresDefaultExceptionFactory()
    {
        ResultXUnitAssertionConfig.UseXUnitExceptions();
        
        var method = typeof(ResultXUnitAssertionConfig).GetMethod("Reset", BindingFlags.NonPublic | BindingFlags.Static)!;
        method.Invoke(null, null);
        
        var factory = ResultAssertionException.ExceptionFactory;
        
        var exception = factory("Factory test");
        exception.Should().BeOfType<ResultAssertionException>()
            .Which.Message.Should().Be("Factory test");
    }

    [Fact]
    public void ResultXUnitAssertionConfig_UseXUnitExceptions_AfterReset_Works()
    {
        ResultXUnitAssertionConfig.UseXUnitExceptions();
        var method = typeof(ResultXUnitAssertionConfig).GetMethod("Reset", BindingFlags.NonPublic | BindingFlags.Static)!;
        method.Invoke(null, null);
        
        ResultXUnitAssertionConfig.UseXUnitExceptions();
        var factory = ResultAssertionException.ExceptionFactory;
        var exception = factory("Factory test");
        exception.Should().BeOfType(_xunitExceptionType);
    }

    [Fact]
    public void ResultXUnitAssertionConfig_AutoConfigure_SetsExceptionFactory()
    {
        var methodReset = typeof(ResultXUnitAssertionConfig).GetMethod("Reset", BindingFlags.NonPublic | BindingFlags.Static)!;
        methodReset.Invoke(null, null);
        
        var method = typeof(ResultXUnitAssertionConfig).GetMethod("AutoConfigure", BindingFlags.NonPublic | BindingFlags.Static)!;
        method.Invoke(null, null);
        
        var factory = ResultAssertionException.ExceptionFactory;
        var exception = factory("Factory test");
        exception.Should().BeOfType(_xunitExceptionType);
    }
}
