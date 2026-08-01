using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace EricksonLopez.Result.AspNetCore;

/// <summary>
/// Extension methods for registering Result HTTP integration services in an <see cref="IServiceCollection"/>.
/// </summary>
public static class ResultServiceCollectionExtensions
{
    /// <summary>
    /// Registers <see cref="ResultHttpOptions"/> with the ASP.NET Core options system, optionally
    /// configuring the options using the provided <paramref name="configure"/> delegate.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <param name="configure">
    /// An optional delegate to configure <see cref="ResultHttpOptions"/> during application startup.
    /// All calls to <see cref="ResultHttpOptions.ConfigureStatusCode"/> must be made before the first request
    /// is processed. After the first request, the options are frozen.
    /// </param>
    /// <returns>The original <see cref="IServiceCollection"/> for chaining.</returns>
    /// <example>
    /// <code>
    /// builder.Services.AddResultHttpOptions(options =>
    /// {
    ///     options.ConfigureStatusCode(ErrorType.Failure, StatusCodes.Status422UnprocessableEntity);
    ///     options.DefaultSuccessStatusCode = StatusCodes.Status200OK;
    /// });
    ///
    /// // Then add the filter to your endpoint or group:
    /// app.MapPost("/cmd", Handler).AddResultEndpointFilter();
    /// </code>
    /// </example>
    public static IServiceCollection AddResultHttpOptions(
        this IServiceCollection services,
        Action<ResultHttpOptions>? configure = null)
    {
        if (configure is not null)
        {
            services.Configure<ResultHttpOptions>(configure);
        }
        else
        {
            // Register with default options so IOptions<ResultHttpOptions> resolves without explicit configuration
            services.AddOptions<ResultHttpOptions>();
        }

        return services;
    }
}
