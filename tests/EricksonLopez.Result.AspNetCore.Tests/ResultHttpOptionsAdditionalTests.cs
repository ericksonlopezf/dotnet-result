// Copyright © Erickson Lopez. MIT License.
#pragma warning disable CS8602
using System;
using System.Threading;
using AwesomeAssertions;
using EricksonLopez.Result;
using EricksonLopez.Result.AspNetCore;
using Microsoft.AspNetCore.Http;
using NSubstitute;
using Xunit;

namespace EricksonLopez.Result.AspNetCore.Tests;

public class ResultHttpOptionsAdditionalTests
{
    [Fact]
    public void ConfigureStatusCode_WhenAfterFreeze_Throws()
    {
        var options = new ResultHttpOptions();
        options.GetFrozenStatusCodeMap();
        Action action = () => options.ConfigureStatusCode(ErrorType.Validation, 200);
        action.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void ConfigureTitleOverride_WhenValidParameters_AddsTitle()
    {
        var options = new ResultHttpOptions();
        options.ConfigureTitleOverride(ErrorType.Validation, "Bad Request Title");
        var map = options.TitleOverrides;
        map[ErrorType.Validation].Should().Be("Bad Request Title");
    }

    [Fact]
    public void ConfigureTitleOverride_WhenAfterFreeze_Throws()
    {
        var options = new ResultHttpOptions();
        options.GetFrozenStatusCodeMap();
        Action action = () => options.ConfigureTitleOverride(ErrorType.Validation, "Bad Request Title");
        action.Should().Throw<InvalidOperationException>();
    }



    [Fact]
    public void IncludeDescriptionInDevelopment_WhenConfigured_SetsCorrectly()
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
    public void GetTitleOverride_WhenPreFreeze_ReturnsFromMutableDict()
    {
        var opt = new ResultHttpOptions();
        opt.ConfigureTitleOverride(ErrorType.Failure, "Custom");

        // Call internal method directly
        var result = opt.GetTitleOverride(ErrorType.Failure);
        result.Should().Be("Custom");

        var resultNull = opt.GetTitleOverride(ErrorType.Conflict);
        resultNull.Should().BeNull();
    }

    [Fact]
    public void IsFrozen_WhenQueried_ReturnsInternalField()
    {
        var opt = new ResultHttpOptions();

        var isFrozen = opt.IsFrozen;
        isFrozen.Should().Be(false);

        opt.GetFrozenStatusCodeMap();

        isFrozen = opt.IsFrozen;
        isFrozen.Should().Be(true);
    }


    [Fact]
    public void ToProblemDetails_WhenAllErrorTypes_CoversAllDescriptiveTitles()
    {
        var options = new ResultHttpOptions { TypeUriBase = "https://example.com/" };
        foreach (ErrorType type in Enum.GetValues<ErrorType>())
        {
            var result = Result.Failure(Error.Create("C", "D").WithType(type).Build());
            result.ToProblemDetails(options);
        }

        var options2 = new ResultHttpOptions { TypeUriBase = "https://example.com/" };
        options2.ConfigureStatusCode(ErrorType.Failure, 500);
        Result.Failure(Error.Failure("C", "D")).ToProblemDetails(options2);
    }

    [Fact]
    public void StatusCodeMap_WhenQueried_ReturnsCachedWrapper()
    {
        var options = new ResultHttpOptions();
        var map1 = options.StatusCodeMap;
        var map2 = options.StatusCodeMap;
        map2.Should().BeSameAs(map1);
    }

    [Fact]
    public void ConfigureStatusCode_WhenConcurrent_ReleasesLock()
    {
        var options = new ResultHttpOptions();
        options.ConfigureStatusCode(ErrorType.Failure, 400);
        var t = new Thread(() => options.ConfigureStatusCode(ErrorType.Conflict, 409));
        t.Start();
        bool completed = t.Join(1000);
        completed.Should().BeTrue();
    }

    [Fact]
    public void GetFrozenStatusCodeMap_WhenCalled_SetsIsFrozen()
    {
        var options = new ResultHttpOptions();
        options.GetFrozenStatusCodeMap();
        Action action = () => options.ConfigureStatusCode(ErrorType.Validation, 200);
        action.Should().Throw<InvalidOperationException>();
    }



    [Fact]
    public void IncludeDescriptionInDevelopment_WhenNullEnvironment_ThrowsArgumentNullException()
    {
        var options = new ResultHttpOptions();
        Action action = () => options.IncludeDescriptionInDevelopment(null!);
        action.Should().Throw<ArgumentNullException>();
    }
}





