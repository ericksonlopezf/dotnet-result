// Copyright © Erickson Lopez. MIT License.
using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace EricksonLopez.Result.OpenApi;

/// <summary>
/// OpenAPI extensions for annotating Minimal API endpoints that return <see cref="Result"/> or <see cref="Result{T}"/>.
/// </summary>
public static class ResultOpenApiExtensions
{
    /// <summary>
    /// Adds standard RFC 9457 ProblemDetails failure response metadata (400, 404, 409, 500) to the endpoint.
    /// </summary>
    /// <param name="builder">The route handler builder.</param>
    /// <returns>The builder for chaining.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="builder"/> is <see langword="null"/></exception>
    public static RouteHandlerBuilder ProducesResultProblemDetails(this RouteHandlerBuilder builder)
    {
        // Stryker disable once Statement
        ArgumentNullException.ThrowIfNull(builder);

        builder.ProducesProblem(StatusCodes.Status400BadRequest);
        builder.ProducesProblem(StatusCodes.Status404NotFound);
        builder.ProducesProblem(StatusCodes.Status409Conflict);
        builder.ProducesProblem(StatusCodes.Status500InternalServerError);

        return builder;
    }

    /// <summary>
    /// Adds success and RFC 9457 ProblemDetails failure metadata for a <see cref="Result{T}"/>.
    /// </summary>
    /// <typeparam name="TResponse">The success payload type.</typeparam>
    /// <param name="builder">The route handler builder.</param>
    /// <param name="statusCode">The success HTTP status code (default: 200 OK).</param>
    /// <returns>The route handler builder for chaining.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="builder"/> is <see langword="null"/></exception>
    public static RouteHandlerBuilder ProducesResult<TResponse>(
        this RouteHandlerBuilder builder,
        int statusCode = StatusCodes.Status200OK)
    {
        // Stryker disable once Statement
        ArgumentNullException.ThrowIfNull(builder);

        builder.Produces<TResponse>(statusCode);
        builder.ProducesResultProblemDetails();

        return builder;
    }

    /// <summary>
    /// Adds success and RFC 9457 ProblemDetails failure metadata for a non-generic <see cref="Result"/>.
    /// </summary>
    /// <param name="builder">The route handler builder.</param>
    /// <param name="statusCode">The success HTTP status code (default: 204 NoContent).</param>
    /// <returns>The route handler builder for chaining.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="builder"/> is <see langword="null"/></exception>
    public static RouteHandlerBuilder ProducesResult(
        this RouteHandlerBuilder builder,
        int statusCode = StatusCodes.Status204NoContent)
    {
        // Stryker disable once Statement
        ArgumentNullException.ThrowIfNull(builder);

        builder.Produces(statusCode);
        builder.ProducesResultProblemDetails();

        return builder;
    }
}
