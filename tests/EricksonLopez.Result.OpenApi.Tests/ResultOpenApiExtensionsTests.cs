// Copyright © Erickson Lopez. MIT License.
using System;
using System.Linq;
using System.Threading.Tasks;
using AwesomeAssertions;
using EricksonLopez.Result.OpenApi;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace EricksonLopez.Result.OpenApi.Tests;

public class ResultOpenApiExtensionsTests
{
    // ────────────────────────── ProducesResultProblemDetails ──────────────────────────

    [Fact]
    public void ProducesResultProblemDetails_WhenBuilderNull_ThrowsArgumentNullException()
    {
        RouteHandlerBuilder builder = null!;
        var act = () => builder.ProducesResultProblemDetails();
        act.Should().Throw<ArgumentNullException>().WithParameterName("builder");
    }

    [Fact]
    public async Task ProducesResultProblemDetails_WhenCalled_ReturnsSameBuilderForChaining()
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        await using var app = builder.Build();
        var routeBuilder = app.MapGet("/chain-test", () => Result.Success());

        var returnedBuilder = routeBuilder.ProducesResultProblemDetails();

        returnedBuilder.Should().BeSameAs(routeBuilder);
    }

    [Fact]
    public async Task ProducesResultProblemDetails_WhenCalled_AddsStandardProblemDetailsMetadata()
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        await using var app = builder.Build();

        var routeBuilder = app.MapGet("/problem-details", () => Result.Success());
        routeBuilder.ProducesResultProblemDetails();

        await app.StartAsync();

        var dataSource = app.Services.GetRequiredService<EndpointDataSource>();
        var endpoint = dataSource.Endpoints.OfType<RouteEndpoint>().First(e => e.RoutePattern.RawText == "/problem-details");
        var metadata = endpoint.Metadata.OfType<IProducesResponseTypeMetadata>().ToList();

        var statusCodes = metadata.Select(m => m.StatusCode).ToList();
        statusCodes.Should().Contain(StatusCodes.Status400BadRequest);
        statusCodes.Should().Contain(StatusCodes.Status404NotFound);
        statusCodes.Should().Contain(StatusCodes.Status409Conflict);
        statusCodes.Should().Contain(StatusCodes.Status500InternalServerError);

        foreach (var status in new[] { 400, 404, 409, 500 })
        {
            var entry = metadata.First(m => m.StatusCode == status);
            entry.ContentTypes.Should().Contain("application/problem+json");
        }

        await app.StopAsync();
    }

    // ────────────────────────── ProducesResult<TResponse> ──────────────────────────

    [Fact]
    public void ProducesResultT_WhenBuilderNull_ThrowsArgumentNullException()
    {
        RouteHandlerBuilder builder = null!;
        var act = () => builder.ProducesResult<string>();
        act.Should().Throw<ArgumentNullException>().WithParameterName("builder");
    }

    [Fact]
    public async Task ProducesResultT_WhenCalled_ReturnsSameBuilderForChaining()
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        await using var app = builder.Build();
        var routeBuilder = app.MapGet("/chain-test-t", () => Result.Success(42));

        var returnedBuilder = routeBuilder.ProducesResult<int>();

        returnedBuilder.Should().BeSameAs(routeBuilder);
    }

    [Fact]
    public async Task ProducesResultT_WhenCalledWithDefaultStatusCode_Produces200OkWithApplicationJsonAndType()
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        await using var app = builder.Build();

        var routeBuilder = app.MapGet("/generic-default", () => Result.Success(new TestPayload(1, "Widget")));
        routeBuilder.ProducesResult<TestPayload>();

        await app.StartAsync();

        var dataSource = app.Services.GetRequiredService<EndpointDataSource>();
        var endpoint = dataSource.Endpoints.OfType<RouteEndpoint>().First(e => e.RoutePattern.RawText == "/generic-default");
        var metadata = endpoint.Metadata.OfType<IProducesResponseTypeMetadata>().ToList();

        var successMetadata = metadata.FirstOrDefault(m => m.StatusCode == StatusCodes.Status200OK && m.Type == typeof(TestPayload));
        successMetadata.Should().NotBeNull();
        successMetadata!.Type.Should().Be<TestPayload>();
        successMetadata.ContentTypes.Should().Contain("application/json");

        // Verify ProblemDetails are also present
        metadata.Should().Contain(m => m.StatusCode == StatusCodes.Status400BadRequest);
        metadata.Should().Contain(m => m.StatusCode == StatusCodes.Status404NotFound);
        metadata.Should().Contain(m => m.StatusCode == StatusCodes.Status409Conflict);
        metadata.Should().Contain(m => m.StatusCode == StatusCodes.Status500InternalServerError);

        await app.StopAsync();
    }

    [Fact]
    public async Task ProducesResultT_WhenCalledWithCustomStatusCode_ProducesSpecifiedStatusCodeWithApplicationJsonAndType()
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        await using var app = builder.Build();

        var routeBuilder = app.MapGet("/generic-custom", () => Result.Success(new TestPayload(2, "Gadget")));
        routeBuilder.ProducesResult<TestPayload>(StatusCodes.Status201Created);

        await app.StartAsync();

        var dataSource = app.Services.GetRequiredService<EndpointDataSource>();
        var endpoint = dataSource.Endpoints.OfType<RouteEndpoint>().First(e => e.RoutePattern.RawText == "/generic-custom");
        var metadata = endpoint.Metadata.OfType<IProducesResponseTypeMetadata>().ToList();

        var successMetadata = metadata.FirstOrDefault(m => m.StatusCode == StatusCodes.Status201Created && m.Type == typeof(TestPayload));
        successMetadata.Should().NotBeNull();
        successMetadata!.Type.Should().Be<TestPayload>();
        successMetadata.ContentTypes.Should().Contain("application/json");

        await app.StopAsync();
    }

    // ────────────────────────── ProducesResult (Non-Generic) ──────────────────────────

    [Fact]
    public void ProducesResult_WhenBuilderNull_ThrowsArgumentNullException()
    {
        RouteHandlerBuilder builder = null!;
        var act = () => builder.ProducesResult();
        act.Should().Throw<ArgumentNullException>().WithParameterName("builder");
    }

    [Fact]
    public async Task ProducesResult_WhenCalled_ReturnsSameBuilderForChaining()
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        await using var app = builder.Build();
        var routeBuilder = app.MapGet("/chain-test-nongeneric", () => Result.Success());

        var returnedBuilder = routeBuilder.ProducesResult();

        returnedBuilder.Should().BeSameAs(routeBuilder);
    }

    [Fact]
    public async Task ProducesResult_WhenCalledWithDefaultStatusCode_Produces204NoContent()
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        await using var app = builder.Build();

        var routeBuilder = app.MapGet("/nongeneric-default", () => Result.Success());
        routeBuilder.ProducesResult();

        await app.StartAsync();

        var dataSource = app.Services.GetRequiredService<EndpointDataSource>();
        var endpoint = dataSource.Endpoints.OfType<RouteEndpoint>().First(e => e.RoutePattern.RawText == "/nongeneric-default");
        var metadata = endpoint.Metadata.OfType<IProducesResponseTypeMetadata>().ToList();

        var successMetadata = metadata.FirstOrDefault(m => m.StatusCode == StatusCodes.Status204NoContent);
        successMetadata.Should().NotBeNull();

        // Verify ProblemDetails are also present
        metadata.Should().Contain(m => m.StatusCode == StatusCodes.Status400BadRequest);
        metadata.Should().Contain(m => m.StatusCode == StatusCodes.Status404NotFound);
        metadata.Should().Contain(m => m.StatusCode == StatusCodes.Status409Conflict);
        metadata.Should().Contain(m => m.StatusCode == StatusCodes.Status500InternalServerError);

        await app.StopAsync();
    }

    [Fact]
    public async Task ProducesResult_WhenCalledWithCustomStatusCode_ProducesSpecifiedStatusCode()
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        await using var app = builder.Build();

        var routeBuilder = app.MapGet("/nongeneric-custom", () => Result.Success());
        routeBuilder.ProducesResult(StatusCodes.Status200OK);

        await app.StartAsync();

        var dataSource = app.Services.GetRequiredService<EndpointDataSource>();
        var endpoint = dataSource.Endpoints.OfType<RouteEndpoint>().First(e => e.RoutePattern.RawText == "/nongeneric-custom");
        var metadata = endpoint.Metadata.OfType<IProducesResponseTypeMetadata>().ToList();

        var successMetadata = metadata.FirstOrDefault(m => m.StatusCode == StatusCodes.Status200OK);
        successMetadata.Should().NotBeNull();

        await app.StopAsync();
    }

    private sealed record TestPayload(int Id, string Name);
}
