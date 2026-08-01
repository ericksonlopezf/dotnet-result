using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace EricksonLopez.Result.AspNetCore;

/// <summary>
/// Extension methods for configuring Result endpoints on an <see cref="IEndpointRouteBuilder"/>.
/// </summary>
public static class ResultEndpointRouteBuilderExtensions
{
    /// <summary>
    /// Adds the <see cref="ResultEndpointFilter"/> to the route handler.
    /// This filter automatically converts returned <see cref="Result"/> and <see cref="Result{T}"/> instances
    /// into appropriate HTTP responses, including RFC 9457 Problem Details for failures.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>OpenAPI / Swagger schema:</b> The filter returns <c>Ok&lt;object?&gt;</c> internally because it
    /// accesses the success value via the <c>IResultOutcome.RawValue</c> covariant property.
    /// OpenAPI tooling (Swashbuckle, NSwag, Scalar) cannot infer the concrete response schema automatically.
    /// <b>You must add <c>.Produces&lt;T&gt;(StatusCodes.Status200OK)</c> to your endpoint</b> to get an
    /// accurate Swagger schema. The Roslyn analyzer <c>RESULT008</c> (<c>EndpointFilterOpenApiAnalyzer</c>)
    /// warns at compile time if <c>.Produces&lt;T&gt;()</c> is missing.
    /// </para>
    /// <para>
    /// <b>Boxing tradeoff:</b> <see cref="Result{T}"/> is a struct. The filter detects it via
    /// <c>result is IResultOutcome</c>, which boxes the struct on every request (1 heap allocation).
    /// If <c>T</c> is also a value type, the success value is additionally boxed via <c>RawValue</c>
    /// (2 allocations total per request). This is acceptable for most endpoints. For high-throughput
    /// paths (&gt; 10k req/s) where zero allocation on the success path is required, call
    /// <c>result.ToHttpResult()</c> directly from the handler:
    /// <code>
    /// app.MapGet("/orders/{id}", async (Guid id, IOrderService svc) =>
    /// {
    ///     var result = await svc.GetOrderAsync(id);
    ///     return result.ToHttpResult(); // typed Ok&lt;OrderDto&gt;, zero boxing, full OpenAPI
    /// });
    /// </code>
    /// </para>
    /// <para>
    /// <b>Uninitialized result protection:</b> If the handler returns a <c>default(Result&lt;T&gt;)</c>
    /// (an uninitialized struct), the filter detects this via <c>IResultOutcome.IsUninitialized</c>
    /// and throws <see cref="InvalidOperationException"/> rather than producing a silent <c>200 OK</c>
    /// response with a <c>null</c> body.
    /// </para>
    /// </remarks>
    /// <param name="builder">The route handler builder to add the filter to.</param>
    /// <returns>The <paramref name="builder"/> for chaining.</returns>
    public static RouteHandlerBuilder AddResultEndpointFilter(this RouteHandlerBuilder builder)
    {
        builder.AddEndpointFilter<ResultEndpointFilter>();
        return builder;
    }

    /// <summary>
    /// Adds the <see cref="ResultEndpointFilter"/> to the route group.
    /// This filter automatically converts returned <see cref="Result"/> and <see cref="Result{T}"/> instances
    /// into appropriate HTTP responses, including RFC 9457 Problem Details for failures.
    /// </summary>
    /// <remarks>
    /// <para>
    /// When added at the route group level, all endpoints in the group have their <see cref="Result"/>
    /// and <see cref="Result{T}"/> return values automatically unwrapped. See the <see cref="AddResultEndpointFilter(RouteHandlerBuilder)"/>
    /// overload for important notes on OpenAPI schema requirements and boxing tradeoffs.
    /// </para>
    /// </remarks>
    /// <param name="builder">The route group builder.</param>
    /// <returns>A <see cref="RouteGroupBuilder"/> that can be used to further customize the endpoint.</returns>
    public static RouteGroupBuilder AddResultEndpointFilter(this RouteGroupBuilder builder)
    {
        builder.AddEndpointFilter<ResultEndpointFilter>();
        return builder;
    }

    /// <summary>
    /// Configures the endpoint to produce a strongly typed <typeparamref name="T"/> on success (default 200 OK)
    /// and a <see cref="Microsoft.AspNetCore.Mvc.ProblemDetails"/> on failure (400 Bad Request).
    /// </summary>
    /// <remarks>
    /// This method is an ergonomic wrapper around <c>.Produces&lt;T&gt;()</c> and <c>.Produces&lt;ProblemDetails&gt;()</c>
    /// to fix the OpenAPI degradation (schema <c>object</c>) introduced by <see cref="AddResultEndpointFilter(RouteHandlerBuilder)"/>.
    /// </remarks>
    /// <typeparam name="T">The success response type.</typeparam>
    /// <param name="builder">The route handler builder.</param>
    /// <param name="statusCode">The HTTP status code for success (defaults to 200 OK).</param>
    /// <returns>The <paramref name="builder"/> for chaining.</returns>
    public static RouteHandlerBuilder ProducesResult<T>(this RouteHandlerBuilder builder, int statusCode = StatusCodes.Status200OK)
    {
        return builder.Produces<T>(statusCode)
                      .Produces<Microsoft.AspNetCore.Mvc.ProblemDetails>(StatusCodes.Status400BadRequest);
    }
}