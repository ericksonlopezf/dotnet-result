// Copyright © Erickson Lopez. MIT License.
using System;
using System.Text.RegularExpressions;
using Grpc.Core;

namespace EricksonLopez.Result.Grpc;

/// <summary>
/// Provides mapping logic between <see cref="Error"/>, <see cref="ErrorType"/>,
/// and gRPC <see cref="StatusCode"/> and <see cref="RpcException"/>.
/// </summary>
public static class GrpcErrorMapper
{
    private static readonly Regex ValidHeaderKeyRegex = new("^[a-z0-9_.-]+$", RegexOptions.Compiled, TimeSpan.FromSeconds(1));

    /// <summary>
    /// Maps a domain <see cref="ErrorType"/> to its canonical gRPC <see cref="StatusCode"/>.
    /// </summary>
    /// <param name="errorType">The error type to convert.</param>
    /// <returns>The corresponding gRPC status code.</returns>
    public static StatusCode ToStatusCode(this ErrorType errorType) => errorType switch
    {
        ErrorType.Validation => StatusCode.InvalidArgument,
        ErrorType.NotFound => StatusCode.NotFound,
        ErrorType.Conflict => StatusCode.AlreadyExists,
        ErrorType.Unauthorized => StatusCode.Unauthenticated,
        ErrorType.Forbidden => StatusCode.PermissionDenied,
        ErrorType.Unavailable => StatusCode.Unavailable,
        ErrorType.Infrastructure => StatusCode.DataLoss,
        _ => StatusCode.Internal
    };

    /// <summary>
    /// Converts an <see cref="Error"/> into an <see cref="RpcException"/> with structured metadata trailers.
    /// </summary>
    /// <param name="error">The error to convert.</param>
    /// <returns>A populated <see cref="RpcException"/>.</returns>
    public static RpcException ToRpcException(this Error error)
    {
        if (error is null)
        {
            throw new ArgumentNullException(nameof(error));
        }

        var statusCode = error.Type.ToStatusCode();
        var trailers = new Metadata
        {
            { "error-code", error.Code },
            { "error-type", error.Type.ToString() },
            { "error-severity", error.Severity.ToString() },
            { "error-retryability", error.Retryability.ToString() }
        };

        if (!string.IsNullOrEmpty(error.TraceId))
        {
            trailers.Add("error-traceid", error.TraceId);
        }

        if (!string.IsNullOrEmpty(error.CorrelationId))
        {
            trailers.Add("error-correlationid", error.CorrelationId);
        }
        // Stryker disable once Conditional : Equivalent mutation (error.Metadata is { Count: > 0 } to >= 0)
        if (error.Metadata is { Count: > 0 })
        {
            foreach (var kvp in error.Metadata)
            {
                string key = $"error-meta-{kvp.Key.ToLowerInvariant()}";
                if (ValidHeaderKeyRegex.IsMatch(key) && kvp.Value is not null)
                {
                    trailers.Add(key, kvp.Value.ToString() ?? string.Empty);
                }
            }
        }

        var status = new Status(statusCode, error.Description);
        return new RpcException(status, trailers, error.Description);
    }

    /// <summary>
    /// Reconstitutes an <see cref="Error"/> from an <see cref="RpcException"/> inspecting its status and trailers.
    /// </summary>
    /// <param name="rpcException">The gRPC exception to map.</param>
    /// <returns>A domain <see cref="Error"/> instance.</returns>
    public static Error ToError(this RpcException rpcException)
    {
        if (rpcException is null)
        {
            throw new ArgumentNullException(nameof(rpcException));
        }

        string code = rpcException.Trailers?.GetValue("error-code") ?? rpcException.Status.StatusCode.ToString();
        string description = rpcException.Status.Detail;
        var errorType = rpcException.Status.StatusCode switch
        {
            StatusCode.InvalidArgument => ErrorType.Validation,
            StatusCode.NotFound => ErrorType.NotFound,
            StatusCode.AlreadyExists => ErrorType.Conflict,
            StatusCode.Aborted => ErrorType.Conflict,
            StatusCode.Unauthenticated => ErrorType.Unauthorized,
            StatusCode.PermissionDenied => ErrorType.Forbidden,
            StatusCode.Unavailable => ErrorType.Unavailable,
            StatusCode.DataLoss => ErrorType.Infrastructure,
            _ => ErrorType.Unexpected
        };

        var builder = Error.Create(code, description)
                           .WithType(errorType);

        if (rpcException.Status.StatusCode == StatusCode.Unavailable)
        {
            builder = builder.WithRetryability(ErrorRetryability.Transient);
        }

        if (rpcException.Trailers is not null)
        {
            string? traceId = rpcException.Trailers.GetValue("error-traceid");
            if (!string.IsNullOrEmpty(traceId))
            {
                builder = builder.WithMetadata("traceId", traceId);
            }

            foreach (var entry in rpcException.Trailers)
            {
                if (entry.Key.StartsWith("error-meta-", StringComparison.OrdinalIgnoreCase))
                {
                    string metaKey = entry.Key.Substring("error-meta-".Length);
                    builder = builder.WithMetadata(metaKey, entry.Value);
                }
            }
        }

        return builder.Build();
    }
}
