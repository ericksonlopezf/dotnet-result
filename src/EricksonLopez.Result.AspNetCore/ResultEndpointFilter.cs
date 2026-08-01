using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace EricksonLopez.Result.AspNetCore;

/// <summary>
/// Minimal API Endpoint Filter that automatically transforms returned <see cref="Result"/> and <see cref="Result{T}"/> instances into HTTP responses.
/// Completely reflection-free and NativeAOT compatible.
/// </summary>
/// <remarks>
/// <para>
/// <b>⚠ OpenAPI / Swagger metadata:</b> When using this filter with <see cref="Result{T}"/>,
/// the success value is returned as <c>object?</c> via <see cref="IResultOutcome.RawValue"/>.
/// This means OpenAPI/Swagger will show <c>object</c> instead of the concrete type <c>T</c>.
/// To fix this, explicitly declare the response type on your endpoint:
/// <code>
/// app.MapGet("/orders/{id}", Handler)
///    .AddResultEndpointFilter()
///    .Produces&lt;OrderDto&gt;(StatusCodes.Status200OK)
///    .ProducesProblem(StatusCodes.Status404NotFound);
/// </code>
/// </para>
/// <para>
/// Alternatively, call <see cref="ResultHttpExtensions.ToHttpResult{T}"/> directly from your
/// handler to get typed <c>Ok&lt;T&gt;</c> results with proper OpenAPI metadata without needing
/// <c>.Produces&lt;T&gt;()</c>.
/// </para>
/// </remarks>
public sealed class ResultEndpointFilter : IEndpointFilter
{
    private readonly ResultHttpOptions _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="ResultEndpointFilter"/> class using DI options.
    /// </summary>
    [Microsoft.Extensions.DependencyInjection.ActivatorUtilitiesConstructor]
    public ResultEndpointFilter(IOptions<ResultHttpOptions>? options = null)
        : this(options?.Value)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ResultEndpointFilter"/> class.
    /// </summary>
    /// <param name="options">Optional HTTP mapping options.</param>
    public ResultEndpointFilter(ResultHttpOptions? options)
    {
        _options = options ?? new ResultHttpOptions();
    }

    /// <inheritdoc/>
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var result = await next(context);

        if (result is Result nonGenericResult)
        {
            return nonGenericResult.ToHttpResult(_options);
        }

        if (result is IResultOutcome outcome)
        {
            if (outcome.IsFailure)
            {
                if (outcome.Error is { } error)
                {
                    return ResultHttpExtensions.CreateProblemDetails(error, _options);
                }

                // Defensive guard: IsFailure is true but Error is null — this is an invalid state
                // that cannot occur through public constructors (Result.Failure requires a non-null Error).
                // However, it could theoretically arise via reflection, internal API misuse, or future
                // refactoring. Returning the raw boxed struct silently would produce a 200 OK response
                // with the struct serialized as a JSON body — a catastrophic failure mode.
                // Throw explicitly to surface the bug rather than silently producing an incorrect response.
                throw new InvalidOperationException(
                    $"ResultEndpointFilter encountered a result in an invalid state: " +
                    $"{result.GetType().FullName} has IsFailure=true but Error is null. " +
                    "This indicates a corrupted Result instance. " +
                    "Ensure all Result instances are created via Result.Success() or Result.Failure(Error).");
            }

            if (outcome.IsSuccess)
            {
                // ⚠ BOXING NOTE (ALL requests — not just failure):
                // When Result<T> (readonly struct) is matched against "is IResultOutcome", the struct
                // is boxed to satisfy the managed interface constraint. This boxing occurs on EVERY
                // request that returns a Result<T>, regardless of success or failure.
                //
                // Additionally, on the success path with value type T:
                //   When IResultOutcome.RawValue is accessed — it returns object?, boxing T again.
                //
                // This means each request through the endpoint filter allocates at minimum:
                //   • 1 object on the heap (boxing of Result<T> struct via 'is IResultOutcome')
                //   • +1 object for value type T on the success path (boxing of T in RawValue)
                //
                // To avoid boxing entirely, call ToHttpResult<T>() directly from your handler:
                //   return result.ToHttpResult(_options);  // no boxing — typed Ok<T>
                //
                // Additionally, TypedResults.Ok(object?) returns Ok<object?>, not Ok<T>, which means
                // OpenAPI/Swagger shows "object" instead of the concrete T.
                // Use .Produces<T>() to declare the response type if using the filter.
                return TypedResults.Ok(outcome.RawValue);
            }

            // Result is Uninitialized (neither IsSuccess nor IsFailure). This occurs when a handler
            // returns default(Result<T>) without going through Result.Success() or Result.Failure().
            // Silently returning 200 OK with a null body would be a misleading success response.
            // Throw explicitly so the error surfaces as a 500 during development.
            // Note: outcome.IsUninitialized is now exposed by IResultOutcome, making this check explicit.
            throw new InvalidOperationException(
                $"ResultEndpointFilter encountered an uninitialized Result: " +
                $"{result.GetType().FullName} has state Uninitialized (IsUninitialized={outcome.IsUninitialized}). " +
                "This typically means a handler returned default(Result<T>) instead of a properly constructed result. " +
                "Ensure all handler return paths use Result.Success(), Result.Success<T>(value), or Result.Failure(error).");
        }

        return result;
    }
}
