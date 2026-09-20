// Copyright © Erickson Lopez. MIT License.
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EricksonLopez.Result;
using EricksonLopez.Result.AspNetCore;
using EricksonLopez.Result.OpenApi;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace EricksonLopez.Result.Sample.Examples;

public record OrderSummary(Guid Id, string CustomerName, decimal Total);

public static class AspNetCoreAndOpenApiShowcase
{
    public static void Run()
    {
        Console.WriteLine("\n========================================================");
        Console.WriteLine(" 29. ASP.NET CORE & OPENAPI INTEGRATION SHOWCASE");
        Console.WriteLine("========================================================");

        // 1. ResultHttpOptions configuration
        Console.WriteLine("\n[1] ResultHttpOptions — custom status code and title configuration:");
        var options = new ResultHttpOptions()
            .ConfigureStatusCode(ErrorType.Validation, StatusCodes.Status400BadRequest)
            .ConfigureStatusCode(ErrorType.Conflict, StatusCodes.Status409Conflict)
            .ConfigureStatusCode(ErrorType.NotFound, StatusCodes.Status404NotFound)
            .ConfigureTitleOverride(ErrorType.Validation, "Domain Input Validation Failed");

        options.DefaultSuccessStatusCode = StatusCodes.Status200OK;
        options.IncludeTraceId = true;
        options.IncludeDescription = true;

        Console.WriteLine($"  Validation status code: {options.StatusCodeMap[ErrorType.Validation]}");
        Console.WriteLine($"  Conflict status code:   {options.StatusCodeMap[ErrorType.Conflict]}");
        Console.WriteLine($"  Validation title:       {options.TitleOverrides[ErrorType.Validation]}");

        // 2. ToHttpResult — mapping Success and Failure directly to IResult
        Console.WriteLine("\n[2] ToHttpResult — direct mapping to Microsoft.AspNetCore.Http.IResult:");
        var order = new OrderSummary(Guid.NewGuid(), "Jane Doe", 299.95m);
        Result<OrderSummary> successResult = Result.Success(order);

        IResult httpSuccess = successResult.ToHttpResult(options);
        Console.WriteLine($"  Success IResult created: {httpSuccess.GetType().Name}");

        Result<OrderSummary> failureResult = Result.Failure<OrderSummary>(
            Error.Create("Order.NotFound", "Order with specified ID does not exist.")
                 .WithType(ErrorType.NotFound)
                 .WithTraceId("trace-w3c-778899")
                 .Build());

        IResult httpFailure = failureResult.ToHttpResult(options);
        Console.WriteLine($"  Failure IResult (ProblemDetails) created: {httpFailure.GetType().Name}");

        // 3. ToProblemDetails — direct ProblemDetails extraction from Failure
        Console.WriteLine("\n[3] ToProblemDetails — dedicated ProblemDetails generator:");
        IResult problemResult = failureResult.ToProblemDetails(options);
        Console.WriteLine($"  Direct ProblemDetails outcome: {problemResult.GetType().Name}");

        // 4. DI Registration: AddResultHttpOptions
        Console.WriteLine("\n[4] AddResultHttpOptions in IServiceCollection:");
        var services = new ServiceCollection();
        services.AddResultHttpOptions(opts =>
        {
            opts.DefaultSuccessStatusCode = StatusCodes.Status201Created;
            opts.IncludeTraceId = true;
        });

        using var provider = services.BuildServiceProvider();
        Console.WriteLine("  AddResultHttpOptions registered and resolved successfully.");

        // 5. Minimal APIs OpenAPI RouteHandlerBuilder extension methods
        Console.WriteLine("\n[5] OpenAPI / Minimal API route builder extensions:");
        var builder = WebApplication.CreateBuilder();
        var app = builder.Build();

        RouteHandlerBuilder route = app.MapGet("/api/orders/{id}", () => Result.Success(order));
        
        // Fluent invocation of official extensions
        ResultOpenApiExtensions.ProducesResult<OrderSummary>(route, StatusCodes.Status200OK);
        route.ProducesResultProblemDetails();
        ResultEndpointRouteBuilderExtensions.AddResultEndpointFilter(route);

        Console.WriteLine("  Configured endpoint with ProducesResult<T>, ProducesResultProblemDetails, and AddResultEndpointFilter.");
    }
}
