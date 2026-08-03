#pragma warning disable CS8602
using System;
using Xunit;
using AwesomeAssertions;
using NSubstitute;
using EricksonLopez.Result.AspNetCore;
using Microsoft.AspNetCore.Http;

namespace EricksonLopez.Result.AspNetCore.Tests;

public class ResultHttpOptionsAdditionalTests
{
    [Fact]
    public void ConfigureStatusCode_AfterFreeze_Throws()
    {
        var options = new ResultHttpOptions();
        options.GetType().GetMethod("GetFrozenStatusCodeMap", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!.Invoke(options, null);
        Assert.Throws<InvalidOperationException>(() => options.ConfigureStatusCode(ErrorType.Validation, 200));
    }

    [Fact]
    public void ConfigureTitleOverride_AddsTitle()
    {
        var options = new ResultHttpOptions();
        options.ConfigureTitleOverride(ErrorType.Validation, "Bad Request Title");
        var map = options.TitleOverrides;
        Assert.Equal("Bad Request Title", map[ErrorType.Validation]);
    }

    [Fact]
    public void ConfigureTitleOverride_AfterFreeze_Throws()
    {
        var options = new ResultHttpOptions();
        options.GetType().GetMethod("GetFrozenStatusCodeMap", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!.Invoke(options, null);
        Assert.Throws<InvalidOperationException>(() => options.ConfigureTitleOverride(ErrorType.Validation, "Bad Request Title"));
    }



    [Fact]
    public void IncludeDescriptionInDevelopment_SetsCorrectly()
    {
        var opt = new ResultHttpOptions();
        var mockEnv = NSubstitute.Substitute.For<Microsoft.Extensions.Hosting.IHostEnvironment>();
        
        mockEnv.EnvironmentName.Returns("Development");
        opt.IncludeDescriptionInDevelopment(mockEnv);
        opt.IncludeDescription.Should().BeTrue();

        mockEnv.EnvironmentName.Returns("Production");
        opt.IncludeDescriptionInDevelopment(mockEnv);
        opt.IncludeDescription.Should().BeFalse();
    }

    [Fact]
    public void GetTitleOverride_PreFreeze_ReturnsFromMutableDict()
    {
        var opt = new ResultHttpOptions();
        opt.ConfigureTitleOverride(ErrorType.Failure, "Custom");
        
        // Reflection to call internal method
        var method = typeof(ResultHttpOptions).GetMethod("GetTitleOverride", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var result = method.Invoke(opt, new object[] { ErrorType.Failure });
        result.Should().Be("Custom");
        
        var resultNull = method.Invoke(opt, new object[] { ErrorType.Conflict });
        resultNull.Should().BeNull();
    }

    [Fact]
    public void IsFrozen_ReturnsInternalField()
    {
        var opt = new ResultHttpOptions();
        var prop = typeof(ResultHttpOptions).GetProperty("IsFrozen", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        var isFrozen = prop.GetValue(opt);
        isFrozen.Should().Be(false);
        
        var method = typeof(ResultHttpOptions).GetMethod("GetFrozenStatusCodeMap", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        method.Invoke(opt, null);
        
        isFrozen = prop.GetValue(opt);
        isFrozen.Should().Be(true);
    }


    [Fact]
    public void ToProblemDetails_CoversAllDescriptiveTitles()
    {
        var options = new ResultHttpOptions { TypeUriBase = "https://example.com/" };
        foreach (ErrorType type in Enum.GetValues<ErrorType>())
        {
            var result = Result.Failure(Error.Create("C", "D").WithType(type).Build());
            result.ToProblemDetails(options);
        }
        
        var options2 = new ResultHttpOptions { TypeUriBase = "https://example.com/" };
        var dict = new System.Collections.Generic.Dictionary<ErrorType, int> { { ErrorType.Failure, 500 } };
        typeof(ResultHttpOptions).GetField("_statusCodeMap", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!.SetValue(options2, dict);
        Result.Failure(Error.Failure("C", "D")).ToProblemDetails(options2);
    }

    [Fact]
    public void StatusCodeMap_ReturnsCachedWrapper()
    {
        var options = new ResultHttpOptions();
        var map1 = options.StatusCodeMap;
        var map2 = options.StatusCodeMap;
        Assert.Same(map1, map2);
    }

    [Fact]
    public void ConfigureStatusCode_ReleasesLock()
    {
        var options = new ResultHttpOptions();
        options.ConfigureStatusCode(ErrorType.Failure, 400);
        var t = new System.Threading.Thread(() => options.ConfigureStatusCode(ErrorType.Conflict, 409));
        t.Start();
        bool completed = t.Join(1000);
        Assert.True(completed);
    }

    [Fact]
    public void GetFrozenStatusCodeMap_SetsIsFrozen_Test()
    {
        var options = new ResultHttpOptions();
        options.GetType().GetMethod("GetFrozenStatusCodeMap", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!.Invoke(options, null);
        Assert.Throws<InvalidOperationException>(() => options.ConfigureStatusCode(ErrorType.Validation, 200));
    }



    [Fact]
    public void IncludeDescriptionInDevelopment_ThrowsIfNull()
    {
        var options = new ResultHttpOptions();
        Assert.Throws<ArgumentNullException>(() => options.IncludeDescriptionInDevelopment(null!));
    }
}


