// Copyright © Erickson Lopez. MIT License.
using System;
using Microsoft.AspNetCore.Http;

namespace EricksonLopez.Result.Dapr;

/// <summary>
/// Provides extension methods for mapping <see cref="Result"/> and <see cref="Result{TValue}"/>
/// outcomes to Dapr Pub/Sub HTTP endpoint results based on <see cref="ErrorRetryability"/>.
/// </summary>
public static class DaprResultPubSubExtensions
{
    /// <summary>
    /// Translates a <see cref="Result"/> outcome into an HTTP <see cref="IResult"/> configured for Dapr Pub/Sub topic endpoints.
    /// </summary>
    /// <param name="result">The outcome to translate.</param>
    /// <returns>
    /// Returns HTTP 200 OK on success (ACK), HTTP 503 Service Unavailable for transient errors (triggers Dapr RETRY),
    /// or HTTP 422 Unprocessable Entity for permanent errors (triggers Dapr DROP / Dead-letter).
    /// </returns>
    public static IResult ToDaprTopicResult(this Result result)
    {
        if (result.IsSuccess)
        {
            return Results.Ok();
        }

        var error = result.Error;
        if (error.Retryability == ErrorRetryability.Transient)
        {
            return Results.StatusCode(StatusCodes.Status503ServiceUnavailable);
        }

        return Results.UnprocessableEntity(new DaprErrorResponse("DROP", error.Code, error.Description));
    }

    /// <summary>
    /// Translates a <see cref="Result{TValue}"/> outcome into an HTTP <see cref="IResult"/> configured for Dapr Pub/Sub topic endpoints.
    /// </summary>
    /// <typeparam name="T">The result value type.</typeparam>
    /// <param name="result">The outcome to translate.</param>
    /// <returns>
    /// Returns HTTP 200 OK on success with value (ACK), HTTP 503 Service Unavailable for transient errors (triggers Dapr RETRY),
    /// or HTTP 422 Unprocessable Entity for permanent errors (triggers Dapr DROP / Dead-letter).
    /// </returns>
    public static IResult ToDaprTopicResult<T>(this Result<T> result)
    {
        if (result.IsSuccess)
        {
            return Results.Ok(result.Value);
        }

        var error = result.Error;
        if (error.Retryability == ErrorRetryability.Transient)
        {
            return Results.StatusCode(StatusCodes.Status503ServiceUnavailable);
        }

        return Results.UnprocessableEntity(new DaprErrorResponse("DROP", error.Code, error.Description));
    }
}
