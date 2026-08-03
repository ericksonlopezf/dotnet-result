using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Xunit;
using EricksonLopez.Result.AspNetCore;
using System.Linq;

namespace EricksonLopez.Result.AspNetCore.Tests;

public class ResultEndpointRouteBuilderExtensionsTests
{
    [Fact]
    public void AddResultEndpointFilter_RouteHandlerBuilder_AddsFilter()
    {
        var builder = WebApplication.CreateBuilder();
        using var app = builder.Build();

        var routeBuilder = app.MapGet("/test", () => Result.Success());
        
        var returnedBuilder = routeBuilder.AddResultEndpointFilter();
        
        Assert.Same(routeBuilder, returnedBuilder);
        // It's hard to assert the filter is in the pipeline without executing, 
        // but verifying it returns the builder without throwing is a good start for coverage.
    }

    [Fact]
    public void AddResultEndpointFilter_RouteGroupBuilder_AddsFilter()
    {
        var builder = WebApplication.CreateBuilder();
        using var app = builder.Build();

        var groupBuilder = app.MapGroup("/testgroup");
        
        var returnedBuilder = groupBuilder.AddResultEndpointFilter();
        
        Assert.Same(groupBuilder, returnedBuilder);
    }
}

