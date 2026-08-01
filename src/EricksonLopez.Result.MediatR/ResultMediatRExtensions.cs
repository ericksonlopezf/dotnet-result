using System;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace EricksonLopez.Result.MediatR;

/// <summary>
/// Extension methods for registering Result-aware MediatR pipeline behaviors.
/// </summary>
public static class ResultMediatRExtensions
{
    /// <summary>
    /// Adds the <see cref="ResultExceptionBehavior{TRequest, TResponse}"/> pipeline behavior
    /// that catches unhandled exceptions and converts them to Result failures.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="errorFactory">
    /// Optional factory to create <see cref="Error"/> from an <see cref="Exception"/>.
    /// When null, uses <c>Error.Unexpected()</c> with the exception type and message.
    /// </param>
    /// <returns>The service collection for chaining.</returns>
    /// <example>
    /// <code>
    /// builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));
    /// builder.Services.AddResultExceptionBehavior();
    /// </code>
    /// </example>
    public static IServiceCollection AddResultExceptionBehavior(
        this IServiceCollection services,
        Func<Exception, Error>? errorFactory = null)
    {
        if (errorFactory is not null)
        {
            services.AddSingleton(errorFactory);
        }

        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ResultExceptionBehavior<,>));
        return services;
    }
}
