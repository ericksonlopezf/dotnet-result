// Copyright © Erickson Lopez. MIT License.
using System;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Reflection;
using EricksonLopez.Result;
using EricksonLopez.Result.OpenTelemetry;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.Metrics;
using Xunit;

namespace EricksonLopez.Result.OpenTelemetry.Tests;

[Collection("Metrics")]
public class ResultServiceCollectionExtensionsTests : IDisposable
{
    public void Dispose()
    {
        ResultMetrics.ResetStaticMeterForTesting();
        GC.SuppressFinalize(this);
    }

    private sealed class TestMeterFactory : IMeterFactory
    {
        public Meter CreatedMeter { get; private set; } = null!;
        public MeterOptions CapturedOptions { get; private set; } = null!;
        public bool IsDisposed { get; private set; }

        public Meter Create(MeterOptions options)
        {
            CapturedOptions = options;
            CreatedMeter = new Meter(options.Name, options.Version);
            return CreatedMeter;
        }

        public void Dispose()
        {
            IsDisposed = true;
            CreatedMeter?.Dispose();
        }
    }

    [Fact]
    public void AddResultMetrics_ThrowsArgumentNullException_WhenServicesIsNull()
    {
        var ex = Assert.Throws<ArgumentNullException>(() => ((IServiceCollection)null!).AddResultMetrics());
        Assert.Equal("services", ex.ParamName);
        Assert.Contains(nameof(ResultServiceCollectionExtensions.AddResultMetrics), ex.StackTrace);
        Assert.DoesNotContain("AddSingleton", ex.StackTrace);
    }

    [Fact]
    public void AddResultMetrics_RegistersResultMetricsAsSingleton_WithOwnsMeterFalse()
    {
        var services = new ServiceCollection();
        var factory = new TestMeterFactory();
        services.AddSingleton<IMeterFactory>(factory);

        var returnedServices = services.AddResultMetrics();
        Assert.Same(services, returnedServices);

        using var provider = services.BuildServiceProvider();
        var metrics1 = provider.GetRequiredService<ResultMetrics>();
        var metrics2 = provider.GetRequiredService<ResultMetrics>();

        Assert.NotNull(metrics1);
        Assert.Same(metrics1, metrics2);

        Assert.NotNull(factory.CapturedOptions);
        Assert.Equal(ResultMetrics.MeterName, factory.CapturedOptions.Name);
        Assert.Equal(ResultMetrics.AssemblyVersion, factory.CapturedOptions.Version);

        // Verify ownsMeter is false by checking _ownedMeter field is null
        var ownedMeterField = typeof(ResultMetrics).GetField("_ownedMeter", BindingFlags.NonPublic | BindingFlags.Instance)!;
        Assert.Null(ownedMeterField.GetValue(metrics1));

        // Verify metrics instance does NOT dispose the factory's meter when metrics is disposed (ownsMeter = false)
        metrics1.Dispose();
        Assert.False(factory.IsDisposed);

        var fields = typeof(Meter).GetFields(BindingFlags.NonPublic | BindingFlags.Instance);
        var disposedField = fields.FirstOrDefault(f => f.Name.Contains("disposed", StringComparison.OrdinalIgnoreCase));
        Assert.NotNull(disposedField);
        var isDisposed = (bool)disposedField!.GetValue(factory.CreatedMeter)!;
        Assert.False(isDisposed, "AddResultMetrics must create ResultMetrics with ownsMeter = false so the DI factory manages meter lifetime.");
    }

    [Fact]
    public void AddResultMetrics_ThrowsInvalidOperationException_WhenMeterFactoryNotRegistered()
    {
        var services = new ServiceCollection();
        services.AddResultMetrics();

        using var provider = services.BuildServiceProvider();
        Assert.Throws<InvalidOperationException>(() => provider.GetRequiredService<ResultMetrics>());
    }
}



