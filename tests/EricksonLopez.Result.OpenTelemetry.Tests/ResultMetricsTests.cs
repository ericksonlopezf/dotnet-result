using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using Xunit;
using EricksonLopez.Result.OpenTelemetry;
using System.Linq;

namespace EricksonLopez.Result.OpenTelemetry.Tests;


[Collection("Metrics")]
public class ResultMetricsTests : IDisposable
{
    private readonly MeterListener _meterListener;
    private readonly List<Measurement<long>> _measurements = new();

    public ResultMetricsTests()
    {
        _meterListener = new MeterListener();
        _meterListener.InstrumentPublished = (instrument, listener) =>
        {
            if (instrument.Meter.Name == ResultMetrics.MeterName)
            {
                listener.EnableMeasurementEvents(instrument);
            }
        };
        _meterListener.SetMeasurementEventCallback<long>((instrument, measurement, tags, state) =>
        {
            _measurements.Add(new Measurement<long>(measurement, tags));
        });
        _meterListener.Start();
    }

    public void Dispose()
    {
        _meterListener.Dispose();
        
        var method = typeof(ResultMetrics).GetMethod("ResetStaticMeterForTesting", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        method?.Invoke(null, null);

        var modeField = typeof(ResultMetrics).GetField("_initializationMode", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        modeField?.SetValue(null, 0);

        GC.SuppressFinalize(this);
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenMeterIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => new ResultMetrics(null!));
    }

    [Fact]
    public void TrackSuccess_ThrowsObjectDisposedException_WhenDisposed()
    {
        var meter = new Meter(ResultMetrics.MeterName, "1.0.0");
        var metrics = new ResultMetrics(meter);
        metrics.Dispose();

        Assert.Throws<ObjectDisposedException>(() => metrics.TrackSuccess("Op1"));
    }

    [Fact]
    public void TrackFailure_ThrowsObjectDisposedException_WhenDisposed()
    {
        var meter = new Meter(ResultMetrics.MeterName, "1.0.0");
        var metrics = new ResultMetrics(meter);
        metrics.Dispose();

        Assert.Throws<ObjectDisposedException>(() => metrics.TrackFailure("Op1", "Err", "Type"));
    }

    [Fact]
    public void TrackSuccess_RecordsMeasurement_WithCorrectTags()
    {
        var meter = new Meter(ResultMetrics.MeterName, "1.0.0");
        var metrics = new ResultMetrics(meter);

        metrics.TrackSuccess("TestOp");

        var measurement = Assert.Single(_measurements);
        Assert.Equal(1, measurement.Value);
        Assert.Contains(measurement.Tags.ToArray(), t => t.Key == "ericksonlopez.result.operation.name" && (string)t.Value! == "TestOp");
        Assert.Contains(measurement.Tags.ToArray(), t => t.Key == "ericksonlopez.result.outcome" && (string)t.Value! == "success");
    }

    [Fact]
    public void TrackFailure_RecordsMeasurement_WithCorrectTags()
    {
        var meter = new Meter(ResultMetrics.MeterName, "1.0.0");
        var metrics = new ResultMetrics(meter);

        metrics.TrackFailure("FailOp", "ErrCode", "validation");

        var measurement = Assert.Single(_measurements);
        Assert.Equal(1, measurement.Value);
        Assert.Contains(measurement.Tags.ToArray(), t => t.Key == "ericksonlopez.result.operation.name" && (string)t.Value! == "FailOp");
        Assert.Contains(measurement.Tags.ToArray(), t => t.Key == "ericksonlopez.result.outcome" && (string)t.Value! == "failure");
        Assert.Contains(measurement.Tags.ToArray(), t => t.Key == "error.type" && (string)t.Value! == "validation");
        Assert.Contains(measurement.Tags.ToArray(), t => t.Key == "ericksonlopez.result.error.code" && (string)t.Value! == "ErrCode");
    }

    [Fact]
    public void Static_TrackSuccess_RecordsMeasurement_WithCorrectTags()
    {
        _measurements.Clear();
#pragma warning disable CS0618 // Static mode is intentionally tested for regression coverage
        ResultMetrics.StaticTrackSuccess("StaticSuccessOp");
#pragma warning restore CS0618

        var measurement = _measurements.Last();
        Assert.Equal(1, measurement.Value);
        Assert.Contains(measurement.Tags.ToArray(), t => t.Key == "ericksonlopez.result.operation.name" && (string)t.Value! == "StaticSuccessOp");
        Assert.Contains(measurement.Tags.ToArray(), t => t.Key == "ericksonlopez.result.outcome" && (string)t.Value! == "success");
    }

    [Fact]
    public void Static_TrackFailure_RecordsMeasurement_WithCorrectTags()
    {
        _measurements.Clear();
#pragma warning disable CS0618 // Static mode is intentionally tested for regression coverage
        ResultMetrics.StaticTrackFailure("StaticFailOp", "CodeXYZ", "domain");
#pragma warning restore CS0618

        var measurement = _measurements.Last();
        Assert.Equal(1, measurement.Value);
        Assert.Contains(measurement.Tags.ToArray(), t => t.Key == "ericksonlopez.result.operation.name" && (string)t.Value! == "StaticFailOp");
        Assert.Contains(measurement.Tags.ToArray(), t => t.Key == "ericksonlopez.result.outcome" && (string)t.Value! == "failure");
        Assert.Contains(measurement.Tags.ToArray(), t => t.Key == "error.type" && (string)t.Value! == "domain");
        Assert.Contains(measurement.Tags.ToArray(), t => t.Key == "ericksonlopez.result.error.code" && (string)t.Value! == "CodeXYZ");
    }

    [Fact]
    public void Static_RecordSuccess_CallsStaticTrackSuccess()
    {
        _measurements.Clear();
#pragma warning disable CS0618
        ResultMetrics.RecordSuccess("StaticSuccessOp2");
#pragma warning restore CS0618
        var measurement = Assert.Single(_measurements);
        Assert.Equal(1, measurement.Value);
    }

    [Fact]
    public void Static_RecordFailure_CallsStaticTrackFailure()
    {
        _measurements.Clear();
#pragma warning disable CS0618
        ResultMetrics.RecordFailure("FailOp2", "Code", "Type");
#pragma warning restore CS0618
        var measurement = Assert.Single(_measurements);
        Assert.Equal(1, measurement.Value);
    }

    [Fact]
    public void ResetStaticMeterForTesting_ClearsState()
    {
        ResultMetrics.StaticTrackSuccess("Op");
        
        var method = typeof(ResultMetrics).GetMethod("ResetStaticMeterForTesting", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        method!.Invoke(null, null);
        
        var field = typeof(ResultMetrics).GetField("_staticOperationsCounter", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        var counter = field!.GetValue(null);
        Assert.Null(counter);
    }

    [Fact]
    public void MixingModes_ThrowsInvalidOperationException()
    {
        ResultMetrics.ResetStaticMeterForTesting();
        
        ResultMetrics.StaticTrackSuccess("Op");
        
        var meter = new System.Diagnostics.Metrics.Meter("TestMix");
        var ex1 = Assert.Throws<InvalidOperationException>(() => new ResultMetrics(meter, ownsMeter: false));
        Assert.Contains("static mode is already active", ex1.Message);

        ResultMetrics.ResetStaticMeterForTesting();

        var diMetrics = new ResultMetrics(meter, ownsMeter: false);
        diMetrics.Dispose(); // hits _ownedMeter?.Dispose() when null

        var ex2 = Assert.Throws<InvalidOperationException>(() => ResultMetrics.StaticTrackSuccess("Op"));
        Assert.Contains("DI mode", ex2.Message);
        
        ResultMetrics.ResetStaticMeterForTesting();
    }

    [Fact]
    public async System.Threading.Tasks.Task EnsureStaticInstruments_Concurrent_InitializesOnce()
    {
        ResultMetrics.ResetStaticMeterForTesting();
        var tasks = new System.Threading.Tasks.Task[10];
        for (int i = 0; i < 10; i++)
        {
            tasks[i] = System.Threading.Tasks.Task.Run(() => ResultMetrics.StaticTrackSuccess("ConcurrentOp"));
        }
        await System.Threading.Tasks.Task.WhenAll(tasks);
        ResultMetrics.ResetStaticMeterForTesting();
    }
}

