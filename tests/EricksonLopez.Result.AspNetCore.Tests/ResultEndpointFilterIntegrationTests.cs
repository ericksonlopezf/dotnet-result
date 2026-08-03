using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Xunit;
using EricksonLopez.Result.AspNetCore;

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
public sealed class ResultEndpointFilterIntegrationTests : IAsyncLifetime
{
    // Skip integration tests on net8.0 — TestServer PipeWriter does not implement UnflushedBytes
    // in that version, causing STJ async serialization to throw on JSON responses.
    private static readonly bool ShouldSkip =
        Environment.Version.Major < 9;
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
    public async Task Filter_ResultSuccess_Returns204NoContent()
    {
        if (ShouldSkip) return; // net8.0 TestServer PipeWriter incompatibility

        var response = await _client!.GetAsync("/result/success");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Empty(body);
    }

    [Fact]
    public async Task Filter_ResultFailure_NotFound_Returns404ProblemDetails()
    {
        if (ShouldSkip) return; // net8.0 TestServer PipeWriter incompatibility

        var response = await _client!.GetAsync("/result/failure-notfound");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

        var problem = await response.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>();
        Assert.Equal(404, problem.GetProperty("status").GetInt32());
        Assert.Equal("Order not found.", problem.GetProperty("detail").GetString());
    }

    [Fact]
    public async Task Filter_ResultFailure_Validation_Returns400ProblemDetails()
    {
        if (ShouldSkip) return; // net8.0 TestServer PipeWriter incompatibility

        var response = await _client!.GetAsync("/result/failure-validation");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

        var problem = await response.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>();
        Assert.Equal(400, problem.GetProperty("status").GetInt32());
    }

    [Fact]
    public async Task Filter_ResultTSuccess_Returns200OkWithBody()
    {
        if (ShouldSkip) return; // net8.0 TestServer PipeWriter incompatibility

        var response = await _client!.GetAsync("/result-t/success");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>();
        Assert.Equal(42, body.GetProperty("id").GetInt32());
        Assert.Equal("Widget", body.GetProperty("name").GetString());
    }

    [Fact]
    public async Task Filter_ResultTFailure_Unexpected_Returns500ProblemDetails()
    {
        if (ShouldSkip) return; // net8.0 TestServer PipeWriter incompatibility

        var response = await _client!.GetAsync("/result-t/failure-unexpected");

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

        var problem = await response.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>();
        Assert.Equal(500, problem.GetProperty("status").GetInt32());
    }

    [Fact]
    public async Task Filter_NonResultReturn_PassesThroughUnchanged()
    {
        if (ShouldSkip) return; // net8.0 TestServer PipeWriter incompatibility

        var response = await _client!.GetAsync("/non-result");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        // TestServer returns the plain string without JSON quotes since Minimal API
        // maps string returns to text/plain rather than application/json
        Assert.True(body is "\"plain string\"" or "plain string",
            $"Unexpected body: {body}");
    }

    [Fact]
    public async Task Filter_MultipleRequests_AllReturnConsistentStatusCodes()
    {
        if (ShouldSkip) return; // net8.0 TestServer PipeWriter incompatibility

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

        Assert.Equal(HttpStatusCode.NoContent, responses[0].StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, responses[1].StatusCode);
        Assert.Equal(HttpStatusCode.OK, responses[2].StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, responses[3].StatusCode);
        Assert.Equal(HttpStatusCode.InternalServerError, responses[4].StatusCode);
    }

    [Fact]
    public async Task Filter_ResultSuccess_Group_Returns200Ok()
    {
        if (ShouldSkip) return; // net8.0 TestServer PipeWriter incompatibility
        var response = await _client!.GetAsync("/group/success");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}

