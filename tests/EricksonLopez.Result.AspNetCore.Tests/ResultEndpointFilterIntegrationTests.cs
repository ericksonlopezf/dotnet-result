// Copyright © Erickson Lopez. MIT License.
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using AwesomeAssertions;
using EricksonLopez.Result;
using EricksonLopez.Result.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace EricksonLopez.Result.AspNetCore.Tests;

/// <summary>
/// End-to-end integration tests for <see cref="ResultEndpointFilter"/> using a real
/// <see cref="TestServer"/> pipeline. These tests validate that the filter correctly intercepts
/// <see cref="Result"/> and <see cref="Result{T}"/> return values through the full ASP.NET Core
/// Minimal API request/response pipeline.
/// </summary>
/// <remarks>
/// These tests run on net10.0 only. The net8.0 TestServer's <c>ResponseBodyPipeWriter</c> does not
/// implement <c>PipeWriter.UnflushedBytes</c>, which causes <c>System.Text.Json</c> async serialization
/// to throw when writing ProblemDetails or JSON responses. This limitation is fixed in net9.0+.
/// </remarks>
[Trait("Category", "Integration")]
public sealed class ResultEndpointFilterIntegrationTests : IAsyncLifetime
{
    private IHost? _host;
    private HttpClient? _client;

    public async Task InitializeAsync()
    {
        _host = await BuildAndStartTestHostAsync();
        _client = _host.GetTestClient();
    }

    public async Task DisposeAsync()
    {
        _client?.Dispose();
        if (_host is not null)
        {
            await _host.StopAsync();
            _host.Dispose();
        }
    }

    private static async Task<IHost> BuildAndStartTestHostAsync()
    {
        var host = new HostBuilder()
            .ConfigureWebHost(webBuilder =>
            {
                webBuilder.UseTestServer();
                webBuilder.UseEnvironment("Testing");

                webBuilder.ConfigureServices(services =>
                {
                    services.AddRouting();
                    services.AddResultHttpOptions(options =>
                    {
                        options.IncludeDescription = true; // Full detail for integration tests
                    });
                });

                webBuilder.Configure(app =>
                {
                    app.UseRouting();
                    app.UseEndpoints(endpoints =>
                    {
                        // Endpoint 1: Result (non-generic) — success returns 204 No Content
                        endpoints.MapGet("/result/success", () => Result.Success())
                            .AddResultEndpointFilter();

                        // Endpoint 2: Result (non-generic) — failure returns 404 Problem
                        endpoints.MapGet("/result/failure-notfound", () =>
                            Result.Failure(Error.NotFound("Order.NotFound", "Order not found.")))
                            .AddResultEndpointFilter();

                        // Endpoint 3: Result (non-generic) — validation failure returns 400
                        endpoints.MapGet("/result/failure-validation", () =>
                            Result.Failure(Error.Validation("Order.Invalid", "Order is invalid.")))
                            .AddResultEndpointFilter();

                        // Endpoint 4: Result<T> — success returns 200 OK with body
                        endpoints.MapGet("/result-t/success", () => Result.Success(new { Id = 42, Name = "Widget" }))
                            .AddResultEndpointFilter();

                        // Endpoint 5: Result<T> — failure returns 500 for unexpected errors
                        endpoints.MapGet("/result-t/failure-unexpected", () =>
                            Result.Failure<string>(Error.Unexpected("System.Error", "An unexpected error occurred.")))
                            .AddResultEndpointFilter();

                        // Endpoint 6: Non-Result return — filter must pass through unchanged
                        endpoints.MapGet("/non-result", () => "plain string")
                            .AddResultEndpointFilter();

                        var group = endpoints.MapGroup("/group").AddResultEndpointFilter();
                        group.MapGet("/success", () => Result.Success(new { Id = 100 }));
                    });
                });
            })
            .Build();

        await host.StartAsync();
        return host;
    }

    // ─────────────────────────── Tests ──────────────────────────────────────────

    [Fact]
    public async Task Filter_WhenResultSuccess_Returns204NoContent()
    {


        var response = await _client!.GetAsync("/result/success");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        var body = await response.Content.ReadAsStringAsync();
        body.Should().BeEmpty();
    }

    [Fact]
    public async Task Filter_WhenResultFailureNotFound_Returns404ProblemDetails()
    {

        var response = await _client!.GetAsync("/result/failure-notfound");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/problem+json");

        var problem = await response.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>();
        problem.GetProperty("status").GetInt32().Should().Be(404);
        problem.GetProperty("detail").GetString().Should().Be("Order not found.");
    }

    [Fact]
    public async Task Filter_WhenResultFailureValidation_Returns400ProblemDetails()
    {

        var response = await _client!.GetAsync("/result/failure-validation");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/problem+json");

        var problem = await response.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>();
        problem.GetProperty("status").GetInt32().Should().Be(400);
    }

    [Fact]
    public async Task Filter_WhenResultTSuccess_Returns200OkWithBody()
    {

        var response = await _client!.GetAsync("/result-t/success");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>();
        body.GetProperty("id").GetInt32().Should().Be(42);
        body.GetProperty("name").GetString().Should().Be("Widget");
    }

    [Fact]
    public async Task Filter_WhenResultTFailureUnexpected_Returns500ProblemDetails()
    {

        var response = await _client!.GetAsync("/result-t/failure-unexpected");

        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/problem+json");

        var problem = await response.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>();
        problem.GetProperty("status").GetInt32().Should().Be(500);
    }

    [Fact]
    public async Task Filter_WhenNonResultReturn_PassesThroughUnchanged()
    {

        var response = await _client!.GetAsync("/non-result");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadAsStringAsync();
        // TestServer returns the plain string without JSON quotes since Minimal API
        // maps string returns to text/plain rather than application/json
        (body is "\"plain string\"" or "plain string").Should().BeTrue($"Unexpected body: {body}");
    }

    [Fact]
    public async Task Filter_WhenMultipleRequests_AllReturnConsistentStatusCodes()
    {

        // Validates thread safety across concurrent requests — important for the
        // FrozenDictionary freeze pattern in ResultHttpOptions (see ResultHttpOptions.GetFrozenStatusCodeMap)
        var tasks = new[]
        {
            _client!.GetAsync("/result/success"),
            _client!.GetAsync("/result/failure-notfound"),
            _client!.GetAsync("/result-t/success"),
            _client!.GetAsync("/result/failure-validation"),
            _client!.GetAsync("/result-t/failure-unexpected"),
        };

        var responses = await Task.WhenAll(tasks);

        responses[0].StatusCode.Should().Be(HttpStatusCode.NoContent);
        responses[1].StatusCode.Should().Be(HttpStatusCode.NotFound);
        responses[2].StatusCode.Should().Be(HttpStatusCode.OK);
        responses[3].StatusCode.Should().Be(HttpStatusCode.BadRequest);
        responses[4].StatusCode.Should().Be(HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task Filter_WhenGroupResultSuccess_Returns200Ok()
    {
        var response = await _client!.GetAsync("/group/success");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}






