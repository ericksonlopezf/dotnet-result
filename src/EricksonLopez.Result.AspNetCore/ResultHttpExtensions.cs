// Copyright © Erickson Lopez. MIT License.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using EricksonLopez.Result;
using Microsoft.AspNetCore.Http;

namespace EricksonLopez.Result.AspNetCore;


/// <summary>
/// Provides HTTP integration extensions for Result types.
/// </summary>
public static class ResultHttpExtensions
{
    private static readonly ResultHttpOptions DefaultOptions = new();

    /// <summary>
    /// Converts a failed <see cref="Result"/> into an ASP.NET Core <see cref="Microsoft.AspNetCore.Http.IResult"/> containing an RFC 9457 ProblemDetails response.
    /// </summary>
    /// <param name="result">The failed result to convert, passed by readonly reference.</param>
    /// <param name="options">Optional HTTP options to customize status codes and problem details mapping, or <see langword="null"/> to use default options.</param>
    /// <returns>An <see cref="Microsoft.AspNetCore.Http.IResult"/> representing the ProblemDetails response.</returns>
    /// <exception cref="InvalidOperationException"><paramref name="result"/> is a successful result</exception>
    public static Microsoft.AspNetCore.Http.IResult ToProblemDetails(this in Result result, ResultHttpOptions? options = null)
    {
        if (result.IsSuccess)
        {
            throw new InvalidOperationException("Cannot create ProblemDetails from a successful result.");
        }

        return CreateProblemDetails(result.Error, options ?? DefaultOptions);
    }

    /// <summary>
    /// Converts a failed <see cref="Result{T}"/> into an ASP.NET Core <see cref="Microsoft.AspNetCore.Http.IResult"/> containing an RFC 9457 ProblemDetails response.
    /// </summary>
    /// <typeparam name="T">The value type of the result.</typeparam>
    /// <param name="result">The failed result to convert, passed by readonly reference.</param>
    /// <param name="options">Optional HTTP options to customize status codes and problem details mapping, or <see langword="null"/> to use default options.</param>
    /// <returns>An <see cref="Microsoft.AspNetCore.Http.IResult"/> representing the ProblemDetails response.</returns>
    /// <exception cref="InvalidOperationException"><paramref name="result"/> is a successful result</exception>
    public static Microsoft.AspNetCore.Http.IResult ToProblemDetails<T>(this in Result<T> result, ResultHttpOptions? options = null)
    {
        if (result.IsSuccess)
        {
            throw new InvalidOperationException("Cannot create ProblemDetails from a successful result.");
        }

        return CreateProblemDetails(result.Error, options ?? DefaultOptions);
    }

    /// <summary>
    /// Converts a successful Result to a status response (default 204 No Content), and a failed Result to a ProblemDetails.
    /// </summary>
    /// <param name="result">The result to convert, passed by readonly reference.</param>
    /// <param name="options">Optional HTTP options to customize status codes and problem details mapping, or <see langword="null"/> to use default options.</param>
    /// <returns>An <see cref="Microsoft.AspNetCore.Http.IResult"/> representing either the success status response or the failure ProblemDetails response.</returns>
    /// <remarks>
    /// The following success status codes map to <c>TypedResults.*</c> methods for full OpenAPI inference:
    /// 200 OK, 201 Created, 202 Accepted, 204 No Content. All other codes use
    /// <see cref="Microsoft.AspNetCore.Http.TypedResults.StatusCode"/> which is opaque to OpenAPI tooling.
    /// If you configure a non-standard success code and need OpenAPI to infer it, use
    /// <see cref="ToHttpResult{T}(in Result{T}, ResultHttpOptions?)"/> or return the
    /// <see cref="Microsoft.AspNetCore.Http.TypedResults"/> value directly from your endpoint handler.
    /// <para>
    /// <b>⚠️ RFC 9110 §15.3.2 note</b>: When <see cref="ResultHttpOptions.DefaultSuccessStatusCode"/> is
    /// <see cref="StatusCodes.Status201Created"/>, the generated response does NOT include a
    /// <c>Location</c> header. RFC 9110 §15.3.2 states that a 201 response SHOULD include a
    /// <c>Location</c> header with the URI of the newly created resource. If a <c>Location</c>
    /// header is required, return <c>TypedResults.Created(uri, value)</c> directly from your endpoint
    /// handler instead of using this method.
    /// </para>
    /// </remarks>
    public static Microsoft.AspNetCore.Http.IResult ToHttpResult(this in Result result, ResultHttpOptions? options = null)
    {
        var opt = options ?? DefaultOptions;
        return result.Match(
            onSuccess: () => opt.DefaultSuccessStatusCode switch
            {
                StatusCodes.Status200OK => Microsoft.AspNetCore.Http.TypedResults.Ok(),
                StatusCodes.Status201Created => Microsoft.AspNetCore.Http.TypedResults.Created((string?)null),
                StatusCodes.Status202Accepted => Microsoft.AspNetCore.Http.TypedResults.Accepted((string?)null),
                StatusCodes.Status204NoContent => Microsoft.AspNetCore.Http.TypedResults.NoContent(),
                _ => Microsoft.AspNetCore.Http.TypedResults.StatusCode(opt.DefaultSuccessStatusCode)
            },
            onFailure: error => CreateProblemDetails(error, opt)
        );
    }

    /// <summary>
    /// Converts a successful Result to a 200 OK with value, and a failed Result to a ProblemDetails.
    /// </summary>
    /// <typeparam name="T">The value type of the result.</typeparam>
    /// <param name="result">The result to convert, passed by readonly reference.</param>
    /// <param name="options">Optional HTTP options to customize status codes and problem details mapping, or <see langword="null"/> to use default options.</param>
    /// <returns>An <see cref="Microsoft.AspNetCore.Http.IResult"/> representing either the 200 OK response or the failure ProblemDetails response.</returns>
    public static Microsoft.AspNetCore.Http.IResult ToHttpResult<T>(this in Result<T> result, ResultHttpOptions? options = null)
    {
        var opt = options ?? DefaultOptions;
        return result.Match(
            onSuccess: value => Microsoft.AspNetCore.Http.TypedResults.Ok(value),
            onFailure: error => CreateProblemDetails(error, opt)
        );
    }

    internal static Microsoft.AspNetCore.Http.IResult CreateProblemDetails(in Error error, ResultHttpOptions options)
    {
        if (!options.GetFrozenStatusCodeMap().TryGetValue(error.Type, out var statusCode))
        {
            statusCode = StatusCodes.Status500InternalServerError;
        }

        var description = options.IncludeDescription
            ? error.Description
            : options.DefaultFallbackDescription;

        var dictionary = new Dictionary<string, object?>
        {
            { "errorCode", error.Code },
            { "severity", ErrorSeverityToString(error.Severity) },
            { "retryability", ErrorRetryabilityToString(error.Retryability) }
        };

        if (options.IncludeTraceId && error.TraceId is not null)
        {
            dictionary.Add("traceId", error.TraceId);
        }

        if (error.CorrelationId is not null)
        {
            dictionary.Add("correlationId", error.CorrelationId);
        }

        if (error.HasInnerErrors)
        {
            var innerErrors = new List<ErrorDetailDto>(error.InnerErrors.Length);
            foreach (var innerError in error.InnerErrors)
            {
                var innerDescription = options.IncludeDescription
                    ? innerError.Description
                    : options.DefaultFallbackDescription;
                innerErrors.Add(new ErrorDetailDto(
                    innerError.Code,
                    innerDescription,
                    ErrorTypeToString(innerError.Type),
                    ErrorSeverityToString(innerError.Severity),
                    ErrorRetryabilityToString(innerError.Retryability),
                    innerError.DescriptionKey,
                    options.IncludeTraceId ? innerError.TraceId : null));
            }
            dictionary.Add("errors", innerErrors);
        }

        if (error.HasMetadata)
        {
            foreach (var kvp in error.Metadata)
            {
                var key = kvp.Key;
                // Guard against overwriting built-in ProblemDetails extension keys.
                // These keys are already populated from Error properties (lines above):
                // "errorCode", "severity", "retryability", "errors", "traceId", "correlationId".
                // If user metadata contains the same key, prefix with "meta." to avoid collision.
                if (key is "errorCode" or "severity" or "retryability" or "errors" or "traceId" or "correlationId")
                {
                    key = $"meta.{key}";
                }
                dictionary[key] = SerializeMetadataValue(kvp.Value);
            }
        }

        // Compute the RFC 9110 status section for potential use in the type URI.
        var section = GetRfcSection(statusCode);

        // RFC 9457 §4.2.1: when TypeUriBase is exactly "about:blank" we must NOT append a
        // fragment (that would produce the invalid "about:blank#15.5.1"). For any other base
        // URI (e.g. an RFC 9110 section URL or an app-specific catalog URL) we append the
        // RFC 9110 status section as before.
        var typeUri = string.Equals(options.TypeUriBase, "about:blank", StringComparison.Ordinal)
            ? "about:blank"
            : $"{options.TypeUriBase}{section}";

        return Microsoft.AspNetCore.Http.TypedResults.Problem(
            statusCode: statusCode,
            title: GetTitle(error.Type, statusCode, options),
            type: typeUri,
            detail: description,
            extensions: dictionary);
    }

    private static string GetRfcSection(int statusCode) => statusCode switch
    {
        StatusCodes.Status400BadRequest => "15.5.1",
        StatusCodes.Status401Unauthorized => "15.5.2",
        StatusCodes.Status402PaymentRequired => "15.5.3",
        StatusCodes.Status403Forbidden => "15.5.4",
        StatusCodes.Status404NotFound => "15.5.5",
        StatusCodes.Status405MethodNotAllowed => "15.5.6",
        StatusCodes.Status406NotAcceptable => "15.5.7",
        407 => "15.5.8",
        StatusCodes.Status408RequestTimeout => "15.5.9",
        StatusCodes.Status409Conflict => "15.5.10",
        StatusCodes.Status410Gone => "15.5.11",
        StatusCodes.Status411LengthRequired => "15.5.12",
        StatusCodes.Status412PreconditionFailed => "15.5.13",
        StatusCodes.Status413RequestEntityTooLarge => "15.5.14",
        StatusCodes.Status414RequestUriTooLong => "15.5.15",
        StatusCodes.Status415UnsupportedMediaType => "15.5.16",
        416 => "15.5.17",
        StatusCodes.Status417ExpectationFailed => "15.5.18",
        // RFC 9110 does not define status 418. It originates from RFC 2324 (a humorous RFC).
        // Returning the generic client error section '15.5' instead of the non-existent '15.5.19'.
        StatusCodes.Status418ImATeapot => "15.5",
        StatusCodes.Status422UnprocessableEntity => "15.5.21",
        StatusCodes.Status426UpgradeRequired => "15.5.22",
        StatusCodes.Status428PreconditionRequired => "15.5.25",
        StatusCodes.Status429TooManyRequests => "15.5.26",
        StatusCodes.Status431RequestHeaderFieldsTooLarge => "15.5.28",
        StatusCodes.Status451UnavailableForLegalReasons => "15.5.30",
        StatusCodes.Status500InternalServerError => "15.6.1",
        StatusCodes.Status501NotImplemented => "15.6.2",
        StatusCodes.Status502BadGateway => "15.6.3",
        StatusCodes.Status503ServiceUnavailable => "15.6.4",
        StatusCodes.Status504GatewayTimeout => "15.6.5",
        StatusCodes.Status505HttpVersionNotsupported => "15.6.6",
        _ => (statusCode / 100) switch
        {
            5 => "15.6",
            4 => "15.5",
            _ => "15"
        }
    };

    private static string GetTitle(ErrorType type, int statusCode, ResultHttpOptions options)
    {
        // GetTitleOverride() is thread-safe in both pre-freeze and post-freeze states:
        //   - Post-freeze: lock-free FrozenDictionary read via volatile field.
        //   - Pre-freeze: acquires _freezeLock to safely read the mutable dictionary.
        // Previously this method read options.TitleOverrides directly in the pre-freeze path
        // without a lock, creating a race condition with ConfigureTitleOverride() during startup.
        var overrideTitle = options.GetTitleOverride(type);
        if (overrideTitle is not null)
            return overrideTitle;

        // RFC 9457 §4.2.1: when type is "about:blank", the title MUST be the canonical HTTP
        // reason phrase for the status code (e.g., "Unprocessable Content" for 422, not a
        // library-specific string like "Domain Rule Violation"). Any other TypeUriBase may use
        // the descriptive domain titles below because they complement the problem-type URI.
        if (string.Equals(options.TypeUriBase, "about:blank", StringComparison.Ordinal))
            return GetCanonicalHttpTitle(statusCode);

        return GetDescriptiveTitle(type, statusCode);
    }

    /// <summary>
    /// Returns the canonical HTTP reason phrase for a given status code, conforming to
    /// RFC 9457 §4.2.1 which requires the canonical title when type is "about:blank".
    /// </summary>
    private static string GetCanonicalHttpTitle(int statusCode) => statusCode switch
    {
        StatusCodes.Status200OK => "OK",
        StatusCodes.Status201Created => "Created",
        StatusCodes.Status202Accepted => "Accepted",
        StatusCodes.Status204NoContent => "No Content",
        StatusCodes.Status400BadRequest => "Bad Request",
        StatusCodes.Status401Unauthorized => "Unauthorized",
        StatusCodes.Status402PaymentRequired => "Payment Required",
        StatusCodes.Status403Forbidden => "Forbidden",
        StatusCodes.Status404NotFound => "Not Found",
        StatusCodes.Status405MethodNotAllowed => "Method Not Allowed",
        StatusCodes.Status406NotAcceptable => "Not Acceptable",
        StatusCodes.Status408RequestTimeout => "Request Timeout",
        StatusCodes.Status409Conflict => "Conflict",
        StatusCodes.Status410Gone => "Gone",
        StatusCodes.Status412PreconditionFailed => "Precondition Failed",
        StatusCodes.Status413RequestEntityTooLarge => "Content Too Large",
        StatusCodes.Status415UnsupportedMediaType => "Unsupported Media Type",
        StatusCodes.Status422UnprocessableEntity => "Unprocessable Content",
        StatusCodes.Status429TooManyRequests => "Too Many Requests",
        StatusCodes.Status500InternalServerError => "Internal Server Error",
        StatusCodes.Status501NotImplemented => "Not Implemented",
        StatusCodes.Status502BadGateway => "Bad Gateway",
        StatusCodes.Status503ServiceUnavailable => "Service Unavailable",
        StatusCodes.Status504GatewayTimeout => "Gateway Timeout",
        _ => (statusCode / 100) switch
        {
            5 => "Internal Server Error",
            4 => "Client Error",
            _ => "Error"
        }
    };

    /// <summary>
    /// Returns the descriptive library-specific title for a given ErrorType, used when
    /// TypeUriBase is not "about:blank" (i.e., a problem-type URI is provided).
    /// These titles are more descriptive than HTTP reason phrases but must only be used
    /// with a non-blank type URI per RFC 9457 §4.2.1.
    /// </summary>
    private static string GetDescriptiveTitle(ErrorType type, int statusCode) => type switch
    {
        // ErrorType.Failure is a domain operation failure, not necessarily a server error.
        // Use "Internal Server Error" for 5xx and "Operation Failed" for overridden lower codes.
        ErrorType.Failure => statusCode >= 500 ? "Internal Server Error" : "Operation Failed",
        ErrorType.Validation => "Validation Error",
        ErrorType.NotFound => "Not Found",
        ErrorType.Conflict => "Conflict",
        ErrorType.Unauthorized => "Unauthorized",
        ErrorType.Forbidden => "Forbidden",
        ErrorType.Unavailable => "Service Unavailable",
        ErrorType.Unexpected => "Internal Server Error",
        ErrorType.Domain => "Domain Rule Violation",
        ErrorType.Infrastructure => "Infrastructure Error",
        ErrorType.Custom => "Application Error",
        _ => "Operation Failed"
    };

    // ─── Stable enum string serialization — delegates to shared ErrorEnumStrings ──

    private static string ErrorTypeToString(ErrorType type)
        => ErrorEnumStrings.ErrorTypeToString(type);

    private static string ErrorSeverityToString(ErrorSeverity severity)
        => ErrorEnumStrings.ErrorSeverityToString(severity);

    private static string ErrorRetryabilityToString(ErrorRetryability retryability)
        => ErrorEnumStrings.ErrorRetryabilityToString(retryability);

    /// <summary>
    /// Serializes a metadata value for inclusion in ProblemDetails extensions.
    /// Primitives (string, bool, numeric types, Guid, DateTime, DateTimeOffset, TimeSpan) are passed through directly.
    /// Collections implementing <see cref="System.Collections.IEnumerable"/> are converted to <c>List&lt;object?&gt;</c>
    /// for proper JSON serialization. Other types fall back to <see cref="object.ToString"/>.
    /// </summary>
    /// <remarks>
    /// This method is AOT-safe: it does not use <c>JsonSerializer</c> or reflection-based serialization.
    /// ASP.NET Core's built-in JSON serializer handles the returned primitives and lists correctly.
    /// <para>
    /// <b>⚠️ Depth limit:</b> Recursion is capped at 5 levels. Circular or deeply nested object graphs
    /// in metadata are safely truncated using <c>"[Depth Limit Exceeded]"</c> at the depth limit. Metadata values
    /// should be primitives or flat collections for predictable serialization behavior.
    /// </para>
    /// </remarks>
    private static object? SerializeMetadataValue(object? value) => SerializeMetadataValue(value, 0);

    private const int MaxMetadataSerializationDepth = 5;

    private static object? SerializeMetadataValue(object? value, int depth)
    {
        if (value is null) return null;

        // Truncate at max depth to prevent StackOverflowException from circular or deeply nested graphs.
        if (depth >= MaxMetadataSerializationDepth)
            return "[Depth Limit Exceeded]";

        // Primitives and well-known types: pass through directly — ASP.NET Core's
        // JSON serializer will handle them correctly.
        if (value is string or bool
            or int or long or short or byte
            or uint or ulong or ushort or sbyte
            or float or double or decimal
            or Guid or DateTime or DateTimeOffset or TimeSpan)
        {
            return value;
        }

        // Dictionaries: convert to Dictionary<string, object?> so ASP.NET Core serializes them
        // as JSON objects ({"key": value}) instead of arrays of KeyValuePair.
        // This must come BEFORE the generic IEnumerable check because IDictionary : IEnumerable.
        if (value is System.Collections.IDictionary dictionary)
        {
            var dict = new Dictionary<string, object?>(dictionary.Count);
            foreach (System.Collections.DictionaryEntry entry in dictionary)
            {
                dict[entry.Key?.ToString() ?? string.Empty] = SerializeMetadataValue(entry.Value, depth + 1);
            }
            return dict;
        }

        // Collections: convert to List<object?> so ASP.NET Core serializes them as JSON arrays
        // instead of producing the type name.
        if (value is System.Collections.IEnumerable enumerable and not string)
        {
            var list = new List<object?>();
            foreach (var item in enumerable)
            {
                list.Add(SerializeMetadataValue(item, depth + 1));
            }
            return list;
        }

        // IFormattable types (e.g., custom numeric/date types): use invariant formatting.
        if (value is IFormattable formattable)
        {
            return formattable.ToString(null, System.Globalization.CultureInfo.InvariantCulture);
        }

        // Fallback: ToString() is better than nothing for truly unknown types.
        return value.ToString();
    }
}



