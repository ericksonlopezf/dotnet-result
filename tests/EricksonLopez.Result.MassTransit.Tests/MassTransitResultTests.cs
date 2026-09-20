// Copyright © Erickson Lopez. MIT License.
using System.Threading.Tasks;
using AwesomeAssertions;
using MassTransit;
using MassTransit.Configuration;
using NSubstitute;
using Xunit;

namespace EricksonLopez.Result.MassTransit.Tests;

public class MassTransitResultTests
{
    [Fact]
    public void ResultFault_FromError_Maps_All_Properties()
    {
        var error = Error.Create("Order.Invalid", "Quantity must be positive")
            .WithType(ErrorType.Validation)
            .WithSeverity(ErrorSeverity.Warning)
            .WithRetryability(ErrorRetryability.Permanent)
            .WithDescriptionKey("orders.invalid_qty")
            .WithTraceId("trace-xyz")
            .WithCorrelationId("corr-xyz")
            .WithMetadata("Field", "Quantity")
            .Build();

        var fault = ResultFault.FromError(error);

        fault.Code.Should().Be("Order.Invalid");
        fault.Description.Should().Be("Quantity must be positive");
        fault.Type.Should().Be("Validation");
        fault.Severity.Should().Be("Warning");
        fault.Retryability.Should().Be("Permanent");
        fault.TraceId.Should().Be("trace-xyz");
        fault.CorrelationId.Should().Be("corr-xyz");
        fault.DescriptionKey.Should().Be("orders.invalid_qty");
        fault.Metadata["Field"].Should().Be("Quantity");
    }

    [Fact]
    public async Task ResultConsumeFilter_Passes_Context_Through_Next_Pipe()
    {
        var filter = new ResultConsumeFilter<TestMessage>();
        var context = Substitute.For<ConsumeContext<TestMessage>>();
        var next = Substitute.For<IPipe<ConsumeContext<TestMessage>>>();

        await filter.Send(context, next);

        await next.Received(1).Send(context);
    }

    [Fact]
    public void ResultConsumeFilter_Probe_Creates_Filter_Scope()
    {
        var filter = new ResultConsumeFilter<TestMessage>();
        var probeContext = Substitute.For<ProbeContext>();

        filter.Probe(probeContext);

        probeContext.Received(1).CreateFilterScope("resultConsumeFilter");
    }

    [Fact]
    public async Task ResultConsumeFilter_Invokes_OnFault_When_Exception_Thrown()
    {
        bool faultCalled = false;
        var filter = new ResultConsumeFilter<TestMessage>((ctx, err) =>
        {
            faultCalled = true;
            err.Code.Should().Be("Consumer.Fault");
            return Task.CompletedTask;
        });

        var context = Substitute.For<ConsumeContext<TestMessage>>();
        var next = Substitute.For<IPipe<ConsumeContext<TestMessage>>>();
        next.Send(context).Returns(Task.FromException(new InvalidOperationException("Handler exploded")));

        await Assert.ThrowsAsync<InvalidOperationException>(() => filter.Send(context, next));
        faultCalled.Should().BeTrue();
    }

    [Fact]
    public async Task ResultConsumeFilter_Throws_On_Null_Arguments()
    {
        var filter = new ResultConsumeFilter<TestMessage>();
        var context = Substitute.For<ConsumeContext<TestMessage>>();
        var next = Substitute.For<IPipe<ConsumeContext<TestMessage>>>();

        await Assert.ThrowsAsync<ArgumentNullException>(() => filter.Send(null!, next));
        await Assert.ThrowsAsync<ArgumentNullException>(() => filter.Send(context, null!));
        Assert.Throws<ArgumentNullException>(() => filter.Probe(null!));
    }

    [Fact]
    public void ResultFault_Default_Constructor_Sets_Defaults_And_Properties_Work()
    {
        var fault = new ResultFault
        {
            Code = "C",
            Description = "D",
            Type = "Failure",
            Severity = "Error",
            Retryability = "Transient",
            TraceId = "trace-123",
            CorrelationId = "corr-456",
            DescriptionKey = "key-1",
            Metadata = new System.Collections.Generic.Dictionary<string, object> { ["k"] = "v" }
        };

        fault.Code.Should().Be("C");
        fault.Description.Should().Be("D");
        fault.Type.Should().Be("Failure");
        fault.Severity.Should().Be("Error");
        fault.Retryability.Should().Be("Transient");
        fault.TraceId.Should().Be("trace-123");
        fault.CorrelationId.Should().Be("corr-456");
        fault.DescriptionKey.Should().Be("key-1");
        fault.Metadata["k"].Should().Be("v");
    }

    [Fact]
    public void ResultFault_Throws_When_Null_Error()
    {
        Assert.Throws<ArgumentNullException>(() => new ResultFault(null!));
        Assert.Throws<ArgumentNullException>(() => ResultFault.FromError(null!));
    }

    [Fact]
    public void UseResultFilter_Registers_Filter_On_Configurator()
    {
        var configurator = Substitute.For<IConsumePipeConfigurator>();

        configurator.UseResultFilter<TestMessage>();

        configurator.Received(1).AddPipeSpecification(Arg.Any<IPipeSpecification<ConsumeContext<TestMessage>>>());

        Assert.Throws<ArgumentNullException>(() => ResultConsumePipeConfiguratorExtensions.UseResultFilter<TestMessage>(null!));
    }

    [Fact]
    public void UseResultFilter_With_Custom_OnFault_Registers_Filter_On_Configurator()
    {
        var configurator = Substitute.For<IConsumePipeConfigurator>();
        System.Func<ConsumeContext<TestMessage>, Error, Task> onFault = (ctx, err) => Task.CompletedTask;

        configurator.UseResultFilter(onFault);

        configurator.Received(1).AddPipeSpecification(Arg.Any<IPipeSpecification<ConsumeContext<TestMessage>>>());
    }
}
