// Copyright © Erickson Lopez. MIT License.
using System;
using System.Threading.Tasks;
using MassTransit;

namespace EricksonLopez.Result.MassTransit;

/// <summary>
/// Provides extension methods for <see cref="IConsumePipeConfigurator"/> to configure
/// Result pattern pipeline filters and fault translation.
/// </summary>
public static class ResultConsumePipeConfiguratorExtensions
{
    /// <summary>
    /// Configures the consume pipe to execute the <see cref="ResultConsumeFilter{TMessage}"/> for the specified message type.
    /// </summary>
    /// <typeparam name="TMessage">The message type to filter.</typeparam>
    /// <param name="configurator">The pipe configurator instance.</param>
    /// <param name="onFault">An optional custom fault callback.</param>
    public static void UseResultFilter<TMessage>(
        this IConsumePipeConfigurator configurator,
        Func<ConsumeContext<TMessage>, Error, Task>? onFault = null)
        where TMessage : class
    {
        if (configurator is null) throw new ArgumentNullException(nameof(configurator));

        configurator.UseFilter(new ResultConsumeFilter<TMessage>(onFault));
    }
}
