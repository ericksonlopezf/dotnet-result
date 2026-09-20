// Copyright © Erickson Lopez. MIT License.
using System;
using System.Threading.Tasks;
using MassTransit;

namespace EricksonLopez.Result.MassTransit;

/// <summary>
/// Represents a MassTransit consume filter that coordinates domain <see cref="Result"/> outcomes,
/// publishing structured <see cref="ResultFault"/> messages when consumers return or signal domain failures.
/// </summary>
/// <typeparam name="TMessage">The consumed message type.</typeparam>
public sealed class ResultConsumeFilter<TMessage> : IFilter<ConsumeContext<TMessage>>
    where TMessage : class
{
    private readonly Func<ConsumeContext<TMessage>, Error, Task>? _onFault;

    /// <summary>
    /// Initializes a new instance of the <see cref="ResultConsumeFilter{TMessage}"/> class.
    /// </summary>
    /// <param name="onFault">An optional custom fault handler delegate.</param>
    public ResultConsumeFilter(Func<ConsumeContext<TMessage>, Error, Task>? onFault = null)
    {
        _onFault = onFault;
    }

    /// <inheritdoc/>
    public async Task Send(ConsumeContext<TMessage> context, IPipe<ConsumeContext<TMessage>> next)
    {
        if (context is null) throw new ArgumentNullException(nameof(context));
        if (next is null) throw new ArgumentNullException(nameof(next));

        try
        {
            await next.Send(context).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            if (_onFault is not null)
            {
                var error = Error.Failure("Consumer.Fault", ex.Message);
                await _onFault(context, error).ConfigureAwait(false);
            }
            throw;
        }
    }

    /// <inheritdoc/>
    public void Probe(ProbeContext context)
    {
        if (context is null) throw new ArgumentNullException(nameof(context));
        context.CreateFilterScope("resultConsumeFilter");
    }
}
