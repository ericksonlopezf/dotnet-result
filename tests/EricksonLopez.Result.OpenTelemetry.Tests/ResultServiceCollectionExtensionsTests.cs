using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.Metrics;
using Xunit;
using EricksonLopez.Result.OpenTelemetry;
using System.Diagnostics.Metrics;

namespace EricksonLopez.Result.OpenTelemetry.Tests;

[Collection("Metrics")]
public class ResultServiceCollectionExtensionsTests : System.IDisposable
{
    public void Dispose()
    {
        var method = typeof(ResultMetrics).GetMethod("ResetStaticMeterForTesting", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        method?.Invoke(null, null);
        System.GC.SuppressFinalize(this);
    }
    private sealed class TestMeterFactory : IMeterFactory
    {
        public Meter CreatedMeter { get; private set; } = null!;
        public MeterOptions CapturedOptions { get; private set; } = null!;

        public Meter Create(MeterOptions options)
        {
            CapturedOptions = options;
            CreatedMeter = new Meter(options.Name, options.Version);
            return CreatedMeter;
        }

        public void Dispose()
        {
        }
    }
    [Fact]
    public void AddResultMetrics_RegistersResultMetricsAsSingleton()
    {
        var services = new ServiceCollection();
        var factory = new TestMeterFactory();
        services.AddSingleton<IMeterFactory>(factory);

        services.AddResultMetrics();
        var provider = services.BuildServiceProvider();
        var metrics1 = provider.GetRequiredService<ResultMetrics>();
        var metrics2 = provider.GetRequiredService<ResultMetrics>();

        Assert.NotNull(metrics1);
        Assert.Same(metrics1, metrics2);
        
        Assert.NotNull(factory.CapturedOptions);
        Assert.Equal(ResultMetrics.MeterName, factory.CapturedOptions.Name);
        Assert.Equal(ResultMetrics.AssemblyVersion, factory.CapturedOptions.Version);
    }
}

