// Copyright © Erickson Lopez. MIT License.
using System;
using AwesomeAssertions;
using EricksonLopez.Result;
using EricksonLopez.Result.AspNetCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;

namespace EricksonLopez.Result.AspNetCore.Tests;

public class ResultServiceCollectionExtensionsTests
{
    [Fact]
    public void AddResultHttpOptions_WhenWithConfigure_RegistersOptions()
    {
        var services = new ServiceCollection();
        services.AddResultHttpOptions(opts => opts.DefaultSuccessStatusCode = 201);

        var sp = services.BuildServiceProvider();
        var options = sp.GetRequiredService<IOptions<ResultHttpOptions>>().Value;

        options.DefaultSuccessStatusCode.Should().Be(201);
    }

    [Fact]
    public void AddResultHttpOptions_WhenWithoutConfigure_RegistersDefaultOptions()
    {
        var services = new ServiceCollection();
        services.AddResultHttpOptions();

        var sp = services.BuildServiceProvider();
        var options = sp.GetRequiredService<IOptions<ResultHttpOptions>>().Value;

        options.Should().NotBeNull();
        options.DefaultSuccessStatusCode.Should().Be(204);
    }
}



