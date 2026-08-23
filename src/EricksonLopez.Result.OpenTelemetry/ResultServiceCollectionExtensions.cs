// Copyright © Erickson Lopez. MIT License.
using System;
using System.Diagnostics.Metrics;
using EricksonLopez.Result;
using Microsoft.Extensions.DependencyInjection;

namespace EricksonLopez.Result.OpenTelemetry;

/// <summary>
/// Extension methods for configuring Result OpenTelemetry integrations in an <see cref="IServiceCollection"/>.
/// </summary>
public static class ResultServiceCollectionExtensions
{
    /// <summary>
    /// Adds <see cref="ResultMetrics"/> as a singleton to the service collection, integrating it with the
    /// application's <see cref="IMeterFactory"/>.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The original service collection for chaining.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="services"/> is <see langword="null"/></exception>
    /// <remarks>
    /// The <see cref="Meter"/> is created via <see cref="IMeterFactory"/> and is disposed automatically
    /// when the host disposes the DI container. <see cref="ResultMetrics.Dispose"/> is a no-op for the
    /// meter itself when registered this way (ownsMeter=false). Do NOT also call the static
    /// <see cref="ResultMetrics.StaticTrackSuccess"/> / <see cref="ResultMetrics.StaticTrackFailure"/>
    /// methods in the same application — that would cause double-counting via two separate meters.
    /// </remarks>
    public static IServiceCollection AddResultMetrics(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddSingleton(sp =>
        {
            var meterFactory = sp.GetRequiredService<IMeterFactory>();
            // IMeterFactory creates and owns the meter. Passing ownsMeter: false (explicit) ensures
            // ResultMetrics.Dispose() does NOT dispose the meter — the factory disposes it when
            // the DI container is torn down (e.g., on application shutdown).
            var meter = meterFactory.Create(ResultMetrics.MeterName, ResultMetrics.AssemblyVersion);
            return new ResultMetrics(meter, ownsMeter: false);
        });
        return services;
    }
}



