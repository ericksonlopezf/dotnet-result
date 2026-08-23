// Copyright © Erickson Lopez. MIT License.
using System;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using AwesomeAssertions;
using EricksonLopez.Result;
using EricksonLopez.Result.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Xunit;

namespace EricksonLopez.Result.AspNetCore.Tests;

[Trait("Category", "Integration")]
public class ResultEndpointRouteBuilderExtensionsTests
{
    [Fact]
    public async Task AddResultEndpointFilter_WhenRouteHandlerBuilder_AddsFilter()
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        await using var app = builder.Build();

        var routeBuilder = app.MapGet("/test", () => Result.Failure(Error.Validation("V", "Desc")));
        var returnedBuilder = routeBuilder.AddResultEndpointFilter();
        returnedBuilder.Should().BeSameAs(routeBuilder);

        await app.StartAsync();
        var client = app.GetTestClient();
        var response = await client.GetAsync("/test");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task AddResultEndpointFilter_WhenRouteGroupBuilder_AddsFilter()
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        await using var app = builder.Build();

        var groupBuilder = app.MapGroup("/testgroup");
        var returnedBuilder = groupBuilder.AddResultEndpointFilter();
        returnedBuilder.Should().BeSameAs(groupBuilder);

        groupBuilder.MapGet("/test", () => Result.Failure(Error.Conflict("C", "Desc")));

        await app.StartAsync();
        var client = app.GetTestClient();
        var response = await client.GetAsync("/testgroup/test");

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task ProducesResult_WhenConfigured_AddsProducesMetadata()
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        await using var app = builder.Build();

        var routeBuilder1 = app.MapGet("/api/order", () => Result.Success("OrderData"))
            .AddResultEndpointFilter()
            .ProducesResult<string>();
        routeBuilder1.Should().NotBeNull();

        var routeBuilder2 = app.MapPost("/api/order", () => Result.Success("OrderCreated"))
            .AddResultEndpointFilter()
            .ProducesResult<string>(StatusCodes.Status201Created);
        routeBuilder2.Should().NotBeNull();

        await app.StartAsync();
        var client = app.GetTestClient();
        var res1 = await client.GetAsync("/api/order");
        res1.StatusCode.Should().Be(HttpStatusCode.OK);

        var res2 = await client.PostAsync("/api/order", null);
        res2.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}





