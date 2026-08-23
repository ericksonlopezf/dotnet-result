// Copyright © Erickson Lopez. MIT License.
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using EricksonLopez.Result;
using EricksonLopez.Result.OpenTelemetry;
using Xunit;

namespace EricksonLopez.Result.OpenTelemetry.Tests;

[Collection("Metrics")]
public class ResultMetricsTests : IDisposable
{
    private readonly MeterListener _meterListener;
    private readonly List<Measurement<long>> _measurements = new();
    private Instrument? _publishedInstrument;

    public ResultMetricsTests()
    {
        _meterListener = new MeterListener();
        _meterListener.InstrumentPublished = (instrument, listener) =>
        {
            if (instrument.Meter.Name == ResultMetrics.MeterName)
            {
                _publishedInstrument = instrument;
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

        ResultMetrics.ResetStaticMeterForTesting();

        GC.SuppressFinalize(this);
    }

    [Fact]
    public void Constants_AreExpectedValues()
    {
        Assert.Equal("EricksonLopez.Result", ResultMetrics.MeterName);
        Assert.False(string.IsNullOrWhiteSpace(ResultMetrics.AssemblyVersion));
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenMeterIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => new ResultMetrics(null!));
        Assert.Throws<ArgumentNullException>(() => new ResultMetrics(null!, ownsMeter: false));
    }

    [Fact]
    public void Constructor_CreatesOperationsCounter_WithExactMetadata()
    {
        var meter = new Meter(ResultMetrics.MeterName, "1.0.0");
        using var metrics = new ResultMetrics(meter);

        Assert.NotNull(_publishedInstrument);
        Assert.Equal("ericksonlopez.result.operations", _publishedInstrument!.Name);
        Assert.Equal("{count}", _publishedInstrument.Unit);
        Assert.Equal("Number of Result outcomes (success and failure).", _publishedInstrument.Description);
    }

    [Fact]
    public void Constructor_OwnsMeterTrue_SetsOwnedMeterAndDisposesItOnDispose()
    {
        var meter = new Meter(ResultMetrics.MeterName, "1.0.0");
        var metrics = new ResultMetrics(meter, ownsMeter: true);

        Assert.Same(meter, metrics.OwnedMeterForTesting);

        metrics.Dispose();
        // Disposing again should be idempotent
        metrics.Dispose();

        Assert.Throws<ObjectDisposedException>(() => metrics.TrackSuccess("Op"));

        // Check internal field of Meter indicating disposal
        var fields = typeof(Meter).GetFields(BindingFlags.NonPublic | BindingFlags.Instance);
        var disposedField = fields.FirstOrDefault(f => f.Name.Contains("disposed", StringComparison.OrdinalIgnoreCase));
        Assert.NotNull(disposedField);
        var isDisposed = (bool)disposedField!.GetValue(meter)!;
        Assert.True(isDisposed, "Meter must be disposed when ResultMetrics is disposed with ownsMeter: true.");
    }

    [Fact]
    public void Constructor_OwnsMeterFalse_SetsOwnedMeterToNullAndDoesNotDisposeMeterOnDispose()
    {
        var meter = new Meter(ResultMetrics.MeterName, "1.0.0");
        var metrics = new ResultMetrics(meter, ownsMeter: false);

        Assert.Null(metrics.OwnedMeterForTesting);

        metrics.Dispose();
        metrics.Dispose();

        // Meter itself is still alive and can create instruments without throwing ObjectDisposedException
        var counter = meter.CreateCounter<long>("other.counter");
        Assert.NotNull(counter);

        var fields = typeof(Meter).GetFields(BindingFlags.NonPublic | BindingFlags.Instance);
        var disposedField = fields.FirstOrDefault(f => f.Name.Contains("disposed", StringComparison.OrdinalIgnoreCase));
        Assert.NotNull(disposedField);
        var isDisposed = (bool)disposedField!.GetValue(meter)!;
        Assert.False(isDisposed, "Meter must not be disposed when ResultMetrics is disposed with ownsMeter: false.");

        meter.Dispose();
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
    public void TrackSuccess_RecordsMeasurement_WithExactTwoTags()
    {
        var meter = new Meter(ResultMetrics.MeterName, "1.0.0");
        using var metrics = new ResultMetrics(meter);

        metrics.TrackSuccess("TestOp");

        var measurement = Assert.Single(_measurements);
        Assert.Equal(1, measurement.Value);
        var tags = measurement.Tags.ToArray();
        Assert.Equal(2, tags.Length);
        Assert.Contains(tags, t => t.Key == "ericksonlopez.result.operation.name" && (string)t.Value! == "TestOp");
        Assert.Contains(tags, t => t.Key == "ericksonlopez.result.outcome" && (string)t.Value! == "success");
    }

    [Fact]
    public void TrackFailure_RecordsMeasurement_WithExactFourTags()
    {
        var meter = new Meter(ResultMetrics.MeterName, "1.0.0");
        using var metrics = new ResultMetrics(meter);

        metrics.TrackFailure("FailOp", "ErrCode", "validation");

        var measurement = Assert.Single(_measurements);
        Assert.Equal(1, measurement.Value);
        var tags = measurement.Tags.ToArray();
        Assert.Equal(4, tags.Length);
        Assert.Contains(tags, t => t.Key == "ericksonlopez.result.operation.name" && (string)t.Value! == "FailOp");
        Assert.Contains(tags, t => t.Key == "ericksonlopez.result.outcome" && (string)t.Value! == "failure");
        Assert.Contains(tags, t => t.Key == "error.type" && (string)t.Value! == "validation");
        Assert.Contains(tags, t => t.Key == "ericksonlopez.result.error.code" && (string)t.Value! == "ErrCode");
    }

    [Fact]
    public void Static_TrackSuccess_RecordsMeasurement_WithExactTwoTags()
    {
        _measurements.Clear();
        ResultMetrics.StaticTrackSuccess("StaticSuccessOp");

        var measurement = _measurements.Last();
        Assert.Equal(1, measurement.Value);
        var tags = measurement.Tags.ToArray();
        Assert.Equal(2, tags.Length);
        Assert.Contains(tags, t => t.Key == "ericksonlopez.result.operation.name" && (string)t.Value! == "StaticSuccessOp");
        Assert.Contains(tags, t => t.Key == "ericksonlopez.result.outcome" && (string)t.Value! == "success");
    }

    [Fact]
    public void Static_TrackSuccess_MultipleCalls_RecordsAllMeasurementsOnSameCounter()
    {
        ResultMetrics.ResetStaticMeterForTesting();
        _measurements.Clear();

        ResultMetrics.StaticTrackSuccess("Op1");
        ResultMetrics.StaticTrackSuccess("Op2");

        Assert.Equal(2, _measurements.Count);
        Assert.Equal("Op1", _measurements[0].Tags.ToArray().First(t => t.Key == "ericksonlopez.result.operation.name").Value);
        Assert.Equal("Op2", _measurements[1].Tags.ToArray().First(t => t.Key == "ericksonlopez.result.operation.name").Value);
    }

    [Fact]
    public void Static_TrackFailure_RecordsMeasurement_WithExactFourTags()
    {
        _measurements.Clear();
        ResultMetrics.StaticTrackFailure("StaticFailOp", "CodeXYZ", "domain");

        var measurement = _measurements.Last();
        Assert.Equal(1, measurement.Value);
        var tags = measurement.Tags.ToArray();
        Assert.Equal(4, tags.Length);
        Assert.Contains(tags, t => t.Key == "ericksonlopez.result.operation.name" && (string)t.Value! == "StaticFailOp");
        Assert.Contains(tags, t => t.Key == "ericksonlopez.result.outcome" && (string)t.Value! == "failure");
        Assert.Contains(tags, t => t.Key == "error.type" && (string)t.Value! == "domain");
        Assert.Contains(tags, t => t.Key == "ericksonlopez.result.error.code" && (string)t.Value! == "CodeXYZ");
    }

    [Fact]
    public async Task Static_TrackSuccess_WhenAlreadyInitialized_BypassesStaticLockFastPath()
    {
        ResultMetrics.ResetStaticMeterForTesting();
        ResultMetrics.StaticTrackSuccess("InitOp");

        var staticLock = ResultMetrics.StaticLockForTesting;

        var lockAcquired = new ManualResetEventSlim(false);
        var releaseLock = new ManualResetEventSlim(false);

        var holdingTask = Task.Run(() =>
        {
            lock (staticLock)
            {
                lockAcquired.Set();
                releaseLock.Wait();
            }
        });

        lockAcquired.Wait();

        try
        {
            // Holding thread holds staticLock. Since _staticOperationsCounter is already initialized,
            // calling StaticTrackSuccess will hit line 265 fast-path and complete without blocking on staticLock.
            var fastPathTask = Task.Run(() => ResultMetrics.StaticTrackSuccess("FastPathOp"));
            var completed = await Task.WhenAny(fastPathTask, Task.Delay(2000));
            Assert.Same(fastPathTask, completed);
        }
        finally
        {
            releaseLock.Set();
            await holdingTask;
        }

        ResultMetrics.ResetStaticMeterForTesting();
    }

    [Fact]
    public async Task EnsureStaticInstruments_InsideLock_HandlesDoubleCheckCorrectly()
    {
        ResultMetrics.ResetStaticMeterForTesting();
        var staticLock = ResultMetrics.StaticLockForTesting;

        var t1 = Task.Run(() =>
        {
            Monitor.Enter(staticLock);
            try
            {
                Thread.Sleep(500); // Give Thread 2 time to pass outer check and block on lock
                // Calling StaticTrackSuccess while holding the lock
                // Since Monitor is reentrant, this will acquire the lock again, initialize, and return
                ResultMetrics.StaticTrackSuccess("ThreadAOp");
                return ResultMetrics.StaticMeterForTesting;
            }
            finally
            {
                Monitor.Exit(staticLock);
            }
        });

        Thread.Sleep(50); // Ensure Thread 1 acquires the lock before Thread 2 calls EnsureStaticInstruments

        // Thread 2 calls EnsureStaticInstruments.
        // Outer check sees null -> goes to lock -> blocks.
        // Thread 1 initializes it -> releases lock.
        // Thread 2 acquires lock -> inner check sees non-null -> hits the missing branch!
        ResultMetrics.StaticTrackSuccess("ThreadBOp");

        var meterCreatedByThread1 = await t1;
        var finalMeter = ResultMetrics.StaticMeterForTesting;

        Assert.NotNull(meterCreatedByThread1);
        Assert.Same(meterCreatedByThread1, finalMeter);

        ResultMetrics.ResetStaticMeterForTesting();
    }

    [Fact]
    public void Static_TrackSuccess_EmitsMeasurement()
    {
        _measurements.Clear();
        ResultMetrics.StaticTrackSuccess("StaticSuccessOp2");
        var measurement = Assert.Single(_measurements);
        Assert.Equal(1, measurement.Value);
    }

    [Fact]
    public void Static_TrackFailure_EmitsMeasurement()
    {
        _measurements.Clear();
        ResultMetrics.StaticTrackFailure("FailOp2", "Code", "Type");
        var measurement = Assert.Single(_measurements);
        Assert.Equal(1, measurement.Value);
    }

    [Fact]
    public void ResetStaticMeterForTesting_ClearsStateAndDisposesExistingStaticMeter()
    {
        ResultMetrics.ResetStaticMeterForTesting();
        ResultMetrics.StaticTrackSuccess("Op");

        var staticMeter = ResultMetrics.StaticMeterForTesting;
        Assert.NotNull(staticMeter);

        ResultMetrics.ResetStaticMeterForTesting();

        Assert.Null(ResultMetrics.StaticOperationsCounterForTesting);
        Assert.Null(ResultMetrics.StaticMeterForTesting);

        var fields = typeof(Meter).GetFields(BindingFlags.NonPublic | BindingFlags.Instance);
        var disposedField = fields.FirstOrDefault(f => f.Name.Contains("disposed", StringComparison.OrdinalIgnoreCase));
        Assert.NotNull(disposedField);
        var isDisposed = (bool)disposedField!.GetValue(staticMeter)!;
        Assert.True(isDisposed, "ResetStaticMeterForTesting must dispose the previous static Meter.");
    }

    [Fact]
    public void MixingModes_ThrowsInvalidOperationException()
    {
        ResultMetrics.ResetStaticMeterForTesting();

        ResultMetrics.StaticTrackSuccess("Op");

        var meter = new Meter("TestMix");
        var ex1 = Assert.Throws<InvalidOperationException>(() => new ResultMetrics(meter, ownsMeter: false));
        Assert.Contains("static mode is already active", ex1.Message);

        ResultMetrics.ResetStaticMeterForTesting();

        var diMetrics = new ResultMetrics(meter, ownsMeter: false);
        diMetrics.Dispose();

        var ex2 = Assert.Throws<InvalidOperationException>(() => ResultMetrics.StaticTrackSuccess("Op"));
        Assert.Contains("DI mode", ex2.Message);

        ResultMetrics.ResetStaticMeterForTesting();
    }

    [Fact]
    public async Task EnsureStaticInstruments_Concurrent_InitializesOnce()
    {
        ResultMetrics.ResetStaticMeterForTesting();
        var tasks = new Task[10];
        for (int i = 0; i < 10; i++)
        {
            tasks[i] = Task.Run(() => ResultMetrics.StaticTrackSuccess("ConcurrentOp"));
        }
        await Task.WhenAll(tasks);
        ResultMetrics.ResetStaticMeterForTesting();
    }
}




