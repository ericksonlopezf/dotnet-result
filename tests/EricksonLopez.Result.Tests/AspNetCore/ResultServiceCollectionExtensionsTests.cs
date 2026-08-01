using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;
using EricksonLopez.Result.AspNetCore;

namespace EricksonLopez.Result.Tests.AspNetCore;

public class ResultServiceCollectionExtensionsTests
{
    [Fact]
    public void AddResultHttpOptions_WithConfigure_RegistersOptions()
    {
        var services = new ServiceCollection();
        services.AddResultHttpOptions(opts => opts.DefaultSuccessStatusCode = 201);
        
        var sp = services.BuildServiceProvider();
        var options = sp.GetRequiredService<IOptions<ResultHttpOptions>>().Value;
        
        Assert.Equal(201, options.DefaultSuccessStatusCode);
    }

    [Fact]
    public void AddResultHttpOptions_WithoutConfigure_RegistersDefaultOptions()
    {
        var services = new ServiceCollection();
        services.AddResultHttpOptions();
        
        var sp = services.BuildServiceProvider();
        var options = sp.GetRequiredService<IOptions<ResultHttpOptions>>().Value;
        
        Assert.NotNull(options);
        Assert.Equal(204, options.DefaultSuccessStatusCode);
    }
}
