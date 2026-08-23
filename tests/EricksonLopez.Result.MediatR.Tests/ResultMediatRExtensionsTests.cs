// Copyright © Erickson Lopez. MIT License.
using System;
using AwesomeAssertions;
using EricksonLopez.Result;
using EricksonLopez.Result.MediatR;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace EricksonLopez.Result.MediatR.Tests;

public class ResultMediatRExtensionsTests
{
    private sealed class DummyRequest : IRequest<Result> { }

    [Fact]
    public void AddResultExceptionBehavior_RegistersBehavior()
    {
        var services = new ServiceCollection();
        services.AddResultExceptionBehavior();
        var provider = services.BuildServiceProvider();

        var behaviors = provider.GetServices<IPipelineBehavior<DummyRequest, Result>>();
        behaviors.Should().ContainSingle(b => b is ResultExceptionBehavior<DummyRequest, Result>);

        var factory = provider.GetService<Func<Exception, Error>>();
        factory.Should().BeNull();
    }

    [Fact]
    public void AddResultExceptionBehavior_WithFactory_RegistersFactory()
    {
        var services = new ServiceCollection();
        Func<Exception, Error> factory = ex => Error.Failure("F", "M");
        services.AddResultExceptionBehavior(factory);
        var provider = services.BuildServiceProvider();

        var registeredFactory = provider.GetService<Func<Exception, Error>>();
        registeredFactory.Should().BeSameAs(factory);
    }
}
