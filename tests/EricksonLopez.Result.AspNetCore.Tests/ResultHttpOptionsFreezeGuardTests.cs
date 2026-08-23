// Copyright © Erickson Lopez. MIT License.
using System;
using System.Reflection;
using AwesomeAssertions;
using EricksonLopez.Result;
using EricksonLopez.Result.AspNetCore;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using NSubstitute;
using Xunit;

namespace EricksonLopez.Result.AspNetCore.Tests;

/// <summary>
/// Tests that verify the freeze guard introduced in ARB audit finding F2-04-A:
/// all scalar properties of <see cref="ResultHttpOptions"/> throw
/// <see cref="InvalidOperationException"/> when mutated after the first request
/// (i.e., after <c>GetFrozenStatusCodeMap()</c> has been called).
/// </summary>
public class ResultHttpOptionsFreezeGuardTests
{
    // Helper: triggers freeze by calling GetFrozenStatusCodeMap (internal method).
    private static ResultHttpOptions CreateFrozenOptions()
    {
        var options = new ResultHttpOptions();
        typeof(ResultHttpOptions)
            .GetMethod("GetFrozenStatusCodeMap", BindingFlags.NonPublic | BindingFlags.Instance)!
            .Invoke(options, null);
        return options;
    }

    // ─── Pre-freeze: all setters must accept values ────────────────────────────

    [Fact]
    public void DefaultSuccessStatusCode_WhenBeforeFreeze_Accepted()
    {
        var options = new ResultHttpOptions();
        options.DefaultSuccessStatusCode = StatusCodes.Status200OK;
        options.DefaultSuccessStatusCode.Should().Be(StatusCodes.Status200OK);
    }

    [Fact]
    public void IncludeTraceId_WhenBeforeFreeze_Accepted()
    {
        var options = new ResultHttpOptions();
        options.IncludeTraceId = true;
        options.IncludeTraceId.Should().BeTrue();
    }

    [Fact]
    public void IncludeDescription_WhenBeforeFreeze_Accepted()
    {
        var options = new ResultHttpOptions();
        options.IncludeDescription = true;
        options.IncludeDescription.Should().BeTrue();
    }

    [Fact]
    public void DefaultFallbackDescription_WhenBeforeFreeze_Accepted()
    {
        var options = new ResultHttpOptions();
        options.DefaultFallbackDescription = "Custom fallback";
        options.DefaultFallbackDescription.Should().Be("Custom fallback");
    }

    [Fact]
    public void TypeUriBase_WhenBeforeFreeze_Accepted()
    {
        var options = new ResultHttpOptions();
        options.TypeUriBase = "https://api.example.com/errors/";
        options.TypeUriBase.Should().Be("https://api.example.com/errors/");
    }

    // ─── Post-freeze: all setters must throw InvalidOperationException ─────────

    [Fact]
    public void DefaultSuccessStatusCode_WhenAfterFreeze_ThrowsInvalidOperationException()
    {
        // Arrange
        var options = CreateFrozenOptions();

        // Act & Assert
        // Regression: scalar properties were unprotected auto-properties (ARB F2-04-A).
        // Setting DefaultSuccessStatusCode after the first request would silently change
        // the success status code for ALL subsequent requests.
        Action action = () => options.DefaultSuccessStatusCode = 200;
        action.Should().Throw<InvalidOperationException>()
            .WithMessage("*DefaultSuccessStatusCode*")
            .WithMessage("*cannot be set after the first request*");
    }

    [Fact]
    public void IncludeTraceId_WhenAfterFreeze_ThrowsInvalidOperationException()
    {
        var options = CreateFrozenOptions();

        Action action = () => options.IncludeTraceId = true;
        action.Should().Throw<InvalidOperationException>()
            .WithMessage("*IncludeTraceId*")
            .WithMessage("*cannot be set after the first request*");
    }

    [Fact]
    public void IncludeDescription_WhenAfterFreeze_ThrowsInvalidOperationException()
    {
        var options = CreateFrozenOptions();

        // This is the highest-severity case: setting IncludeDescription = true after the first
        // request would expose error details to all subsequent API clients — an information
        // disclosure vulnerability (ARB finding F2-04-A / F6-02-A).
        Action action = () => options.IncludeDescription = true;
        action.Should().Throw<InvalidOperationException>()
            .WithMessage("*IncludeDescription*")
            .WithMessage("*cannot be set after the first request*");
    }

    [Fact]
    public void DefaultFallbackDescription_WhenAfterFreeze_ThrowsInvalidOperationException()
    {
        var options = CreateFrozenOptions();

        Action action = () => options.DefaultFallbackDescription = "Hacked";
        action.Should().Throw<InvalidOperationException>()
            .WithMessage("*DefaultFallbackDescription*")
            .WithMessage("*cannot be set after the first request*");
    }

    [Fact]
    public void TypeUriBase_WhenAfterFreeze_ThrowsInvalidOperationException()
    {
        var options = CreateFrozenOptions();

        Action action = () => options.TypeUriBase = "https://evil.example.com/";
        action.Should().Throw<InvalidOperationException>()
            .WithMessage("*TypeUriBase*")
            .WithMessage("*cannot be set after the first request*");
    }

    [Fact]
    public void IncludeDescriptionInDevelopment_WhenAfterFreeze_ThrowsInvalidOperationException()
    {
        var options = CreateFrozenOptions();
        var mockEnv = Substitute.For<IHostEnvironment>();
        mockEnv.EnvironmentName.Returns("Development");

        Action action = () => options.IncludeDescriptionInDevelopment(mockEnv);
        action.Should().Throw<InvalidOperationException>()
            .WithMessage("*IncludeDescriptionInDevelopment*")
            .WithMessage("*cannot be set after the first request*");
    }

    // ─── Post-freeze: getters must still work (read path is not frozen) ────────

    [Fact]
    public void AllScalarProperties_WhenAfterFreeze_GettersStillWork()
    {
        var options = new ResultHttpOptions
        {
            DefaultSuccessStatusCode = StatusCodes.Status201Created,
            IncludeTraceId = true,
            IncludeDescription = true,
            DefaultFallbackDescription = "Custom",
            TypeUriBase = "https://api.example.com/"
        };
        // Trigger freeze
        typeof(ResultHttpOptions)
            .GetMethod("GetFrozenStatusCodeMap", BindingFlags.NonPublic | BindingFlags.Instance)!
            .Invoke(options, null);

        // All getters must return the values set before freeze
        options.DefaultSuccessStatusCode.Should().Be(StatusCodes.Status201Created);
        options.IncludeTraceId.Should().BeTrue();
        options.IncludeDescription.Should().BeTrue();
        options.DefaultFallbackDescription.Should().Be("Custom");
        options.TypeUriBase.Should().Be("https://api.example.com/");
    }

    // ─── Default values ────────────────────────────────────────────────────────

    [Fact]
    public void DefaultValues_WhenQueried_AreCorrect()
    {
        var options = new ResultHttpOptions();

        options.DefaultSuccessStatusCode.Should().Be(StatusCodes.Status204NoContent);
        options.IncludeTraceId.Should().BeFalse();
        options.IncludeDescription.Should().BeFalse();
        options.DefaultFallbackDescription.Should().Be("An error occurred.");
        options.TypeUriBase.Should().Be("about:blank");
    }
}



