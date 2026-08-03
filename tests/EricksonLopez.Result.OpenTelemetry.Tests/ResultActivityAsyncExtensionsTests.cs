using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Linq;
using Xunit;
using EricksonLopez.Result.OpenTelemetry;

using System.Threading.Tasks;
namespace EricksonLopez.Result.OpenTelemetry.Tests;

[Collection("Metrics")]
public class ResultActivityAsyncExtensionsTests : IDisposable
{

    private static async Task<T> Deferred<T>(T value) { await Task.Yield(); return value; }
    private static ValueTask<T> DeferredValueTask<T>(T value) => new(DeferredValueTaskCore(value)); private static async Task<T> DeferredValueTaskCore<T>(T value) { await Task.Yield(); return value; }

    private readonly ActivityListener _activityListener;
    private readonly List<Activity> _activities = new();
    private readonly MeterListener _meterListener;
    private readonly List<Measurement<long>> _measurements = new();

    public ResultActivityAsyncExtensionsTests()
    {
        _activityListener = new ActivityListener
        {
            ShouldListenTo = source => source.Name == ResultActivityExtensions.ActivitySourceName,
            Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllData,
            ActivityStarted = activity => { },
            ActivityStopped = activity => _activities.Add(activity)
        };
        ActivitySource.AddActivityListener(_activityListener);

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
        _activityListener.Dispose();
        _meterListener.Dispose();
        GC.SuppressFinalize(this);
    }

    private static Activity CreateTestActivity()
    {
        var source = new ActivitySource(ResultActivityExtensions.ActivitySourceName);
        var activity = source.StartActivity("TestActivity");
        return activity!;
    }

    [Fact]
    public async Task Task_TraceOutcome_Result_Success_SetsActivityStatusToOk()
    {
        var result = Deferred(Result.Success());
        using var activity = CreateTestActivity();
        var metrics = new ResultMetrics(new Meter(ResultMetrics.MeterName, "1.0"));

        await result.TraceOutcome("Op1", activity, metrics);

        Assert.Equal(ActivityStatusCode.Ok, activity.Status);
        Assert.Equal("Op1", activity.GetTagItem("ericksonlopez.result.operation.name"));
        Assert.Equal("success", activity.GetTagItem("ericksonlopez.result.outcome"));

        var measurement = Assert.Single(_measurements);
        Assert.Contains(measurement.Tags.ToArray(), t => t.Key == "ericksonlopez.result.outcome" && (string)t.Value! == "success");
    }

[Fact]
    public async Task ValueTask_TraceOutcome_Result_Success_SetsActivityStatusToOk()
    {
        var result = DeferredValueTask(Result.Success());
        using var activity = CreateTestActivity();
        var metrics = new ResultMetrics(new Meter(ResultMetrics.MeterName, "1.0"));

        await result.TraceOutcome("Op1", activity, metrics);

        Assert.Equal(ActivityStatusCode.Ok, activity.Status);
        Assert.Equal("Op1", activity.GetTagItem("ericksonlopez.result.operation.name"));
        Assert.Equal("success", activity.GetTagItem("ericksonlopez.result.outcome"));

        var measurement = Assert.Single(_measurements);
        Assert.Contains(measurement.Tags.ToArray(), t => t.Key == "ericksonlopez.result.outcome" && (string)t.Value! == "success");
    }

    [Fact]
    public async Task Task_TraceOutcome_Result_Failure_SetsActivityStatusToError()
    {
        var result = Deferred(Result.Failure(Error.Failure("F", "Desc")));
        using var activity = CreateTestActivity();
        var metrics = new ResultMetrics(new Meter(ResultMetrics.MeterName, "1.0"));

        await result.TraceOutcome("Op2", activity, metrics);

        Assert.Equal(ActivityStatusCode.Error, activity.Status);
        Assert.Equal("Desc", activity.StatusDescription);
        Assert.Equal("failure", activity.GetTagItem("error.type"));
        Assert.Equal("F", activity.GetTagItem("ericksonlopez.result.error.code"));
        Assert.Equal("error", activity.GetTagItem("ericksonlopez.result.error.severity"));
        Assert.Equal("Op2", activity.GetTagItem("ericksonlopez.result.operation.name"));
        Assert.Equal("failure", activity.GetTagItem("ericksonlopez.result.outcome"));

        var measurement = Assert.Single(_measurements);
        Assert.Contains(measurement.Tags.ToArray(), t => t.Key == "ericksonlopez.result.outcome" && (string)t.Value! == "failure");
    }

[Fact]
    public async Task ValueTask_TraceOutcome_Result_Failure_SetsActivityStatusToError()
    {
        var result = DeferredValueTask(Result.Failure(Error.Failure("F", "Desc")));
        using var activity = CreateTestActivity();
        var metrics = new ResultMetrics(new Meter(ResultMetrics.MeterName, "1.0"));

        await result.TraceOutcome("Op2", activity, metrics);

        Assert.Equal(ActivityStatusCode.Error, activity.Status);
        Assert.Equal("Desc", activity.StatusDescription);
        Assert.Equal("failure", activity.GetTagItem("error.type"));
        Assert.Equal("F", activity.GetTagItem("ericksonlopez.result.error.code"));
        Assert.Equal("error", activity.GetTagItem("ericksonlopez.result.error.severity"));
        Assert.Equal("Op2", activity.GetTagItem("ericksonlopez.result.operation.name"));
        Assert.Equal("failure", activity.GetTagItem("ericksonlopez.result.outcome"));

        var measurement = Assert.Single(_measurements);
        Assert.Contains(measurement.Tags.ToArray(), t => t.Key == "ericksonlopez.result.outcome" && (string)t.Value! == "failure");
    }

    [Fact]
    public async Task Task_TraceOnFailure_Result_Success_DoesNotRecord()
    {
        var result = Deferred(Result.Success());
        using var activity = CreateTestActivity();
        var metrics = new ResultMetrics(new Meter(ResultMetrics.MeterName, "1.0"));

        await result.TraceOnFailure("Op3", activity, metrics);

        Assert.Equal(ActivityStatusCode.Unset, activity.Status);
        Assert.Null(activity.GetTagItem("ericksonlopez.result.outcome"));
        Assert.Empty(_measurements);
    }

[Fact]
    public async Task ValueTask_TraceOnFailure_Result_Success_DoesNotRecord()
    {
        var result = DeferredValueTask(Result.Success());
        using var activity = CreateTestActivity();
        var metrics = new ResultMetrics(new Meter(ResultMetrics.MeterName, "1.0"));

        await result.TraceOnFailure("Op3", activity, metrics);

        Assert.Equal(ActivityStatusCode.Unset, activity.Status);
        Assert.Null(activity.GetTagItem("ericksonlopez.result.outcome"));
        Assert.Empty(_measurements);
    }

    [Fact]
    public async Task Task_TraceOnFailure_Result_Failure_RecordsFailure()
    {
        var result = Deferred(Result.Failure(Error.NotFound("N", "Desc")));
        using var activity = CreateTestActivity();

        await result.TraceOnFailure("Op4", activity); // testing with null metrics

        Assert.Equal(ActivityStatusCode.Error, activity.Status);
        Assert.Equal("failure", activity.GetTagItem("ericksonlopez.result.outcome"));
        Assert.Empty(_measurements); // null metrics means no metrics
    }

[Fact]
    public async Task ValueTask_TraceOnFailure_Result_Failure_RecordsFailure()
    {
        var result = DeferredValueTask(Result.Failure(Error.NotFound("N", "Desc")));
        using var activity = CreateTestActivity();

        await result.TraceOnFailure("Op4", activity); // testing with null metrics

        Assert.Equal(ActivityStatusCode.Error, activity.Status);
        Assert.Equal("failure", activity.GetTagItem("ericksonlopez.result.outcome"));
        Assert.Empty(_measurements); // null metrics means no metrics
    }

    [Fact]
    public async Task Task_TraceOnSuccess_Result_Failure_DoesNotRecord()
    {
        var result = Deferred(Result.Failure(Error.NotFound("N", "Desc")));
        using var activity = CreateTestActivity();
        
        await result.TraceOnSuccess("Op5", activity);

        Assert.Equal(ActivityStatusCode.Unset, activity.Status);
        Assert.Null(activity.GetTagItem("ericksonlopez.result.outcome"));
    }

[Fact]
    public async Task ValueTask_TraceOnSuccess_Result_Failure_DoesNotRecord()
    {
        var result = DeferredValueTask(Result.Failure(Error.NotFound("N", "Desc")));
        using var activity = CreateTestActivity();
        
        await result.TraceOnSuccess("Op5", activity);

        Assert.Equal(ActivityStatusCode.Unset, activity.Status);
        Assert.Null(activity.GetTagItem("ericksonlopez.result.outcome"));
    }

    [Fact]
    public async Task Task_TraceOnSuccess_Result_Success_RecordsSuccess()
    {
        var result = Deferred(Result.Success());
        using var activity = CreateTestActivity();
        
        await result.TraceOnSuccess("Op6", activity);

        Assert.Equal(ActivityStatusCode.Ok, activity.Status);
        Assert.Equal("success", activity.GetTagItem("ericksonlopez.result.outcome"));
    }

[Fact]
    public async Task ValueTask_TraceOnSuccess_Result_Success_RecordsSuccess()
    {
        var result = DeferredValueTask(Result.Success());
        using var activity = CreateTestActivity();
        
        await result.TraceOnSuccess("Op6", activity);

        Assert.Equal(ActivityStatusCode.Ok, activity.Status);
        Assert.Equal("success", activity.GetTagItem("ericksonlopez.result.outcome"));
    }

    [Fact]
    public async Task Task_TraceOutcome_ResultT_Success_SetsActivityStatusToOk()
    {
        var result = Deferred(Result.Success(42));
        using var activity = CreateTestActivity();
        var metrics = new ResultMetrics(new Meter(ResultMetrics.MeterName, "1.0"));

        await result.TraceOutcome("Op7", activity, metrics);

        Assert.Equal(ActivityStatusCode.Ok, activity.Status);
        Assert.Equal("success", activity.GetTagItem("ericksonlopez.result.outcome"));
        var measurement = Assert.Single(_measurements);
        Assert.Contains(measurement.Tags.ToArray(), t => t.Key == "ericksonlopez.result.outcome" && (string)t.Value! == "success");
    }

[Fact]
    public async Task ValueTask_TraceOutcome_ResultT_Success_SetsActivityStatusToOk()
    {
        var result = DeferredValueTask(Result.Success(42));
        using var activity = CreateTestActivity();
        var metrics = new ResultMetrics(new Meter(ResultMetrics.MeterName, "1.0"));

        await result.TraceOutcome("Op7", activity, metrics);

        Assert.Equal(ActivityStatusCode.Ok, activity.Status);
        Assert.Equal("success", activity.GetTagItem("ericksonlopez.result.outcome"));
        var measurement = Assert.Single(_measurements);
        Assert.Contains(measurement.Tags.ToArray(), t => t.Key == "ericksonlopez.result.outcome" && (string)t.Value! == "success");
    }

    [Fact]
    public async Task Task_TraceOutcome_ResultT_Failure_SetsActivityStatusToError()
    {
        var result = Deferred(Result.Failure<int>(Error.Conflict("C", "Desc")));
        using var activity = CreateTestActivity();
        var metrics = new ResultMetrics(new Meter(ResultMetrics.MeterName, "1.0"));

        await result.TraceOutcome("Op8", activity, metrics);

        Assert.Equal(ActivityStatusCode.Error, activity.Status);
        Assert.Equal("conflict", activity.GetTagItem("error.type"));
        Assert.Equal("C", activity.GetTagItem("ericksonlopez.result.error.code"));
        var measurement = Assert.Single(_measurements);
        Assert.Contains(measurement.Tags.ToArray(), t => t.Key == "ericksonlopez.result.outcome" && (string)t.Value! == "failure");
    }

[Fact]
    public async Task ValueTask_TraceOutcome_ResultT_Failure_SetsActivityStatusToError()
    {
        var result = DeferredValueTask(Result.Failure<int>(Error.Conflict("C", "Desc")));
        using var activity = CreateTestActivity();
        var metrics = new ResultMetrics(new Meter(ResultMetrics.MeterName, "1.0"));

        await result.TraceOutcome("Op8", activity, metrics);

        Assert.Equal(ActivityStatusCode.Error, activity.Status);
        Assert.Equal("conflict", activity.GetTagItem("error.type"));
        Assert.Equal("C", activity.GetTagItem("ericksonlopez.result.error.code"));
        var measurement = Assert.Single(_measurements);
        Assert.Contains(measurement.Tags.ToArray(), t => t.Key == "ericksonlopez.result.outcome" && (string)t.Value! == "failure");
    }

    [Fact]
    public async Task Task_TraceOnFailure_ResultT_Success_DoesNotRecord()
    {
        var result = Deferred(Result.Success(10));
        using var activity = CreateTestActivity();

        await result.TraceOnFailure("Op9", activity);

        Assert.Equal(ActivityStatusCode.Unset, activity.Status);
    }

[Fact]
    public async Task ValueTask_TraceOnFailure_ResultT_Success_DoesNotRecord()
    {
        var result = DeferredValueTask(Result.Success(10));
        using var activity = CreateTestActivity();

        await result.TraceOnFailure("Op9", activity);

        Assert.Equal(ActivityStatusCode.Unset, activity.Status);
    }

    [Fact]
    public async Task Task_TraceOnFailure_ResultT_Failure_RecordsFailure()
    {
        var result = Deferred(Result.Failure<string>(Error.Infrastructure("I", "Desc")));
        using var activity = CreateTestActivity();

        await result.TraceOnFailure("Op10", activity);

        Assert.Equal(ActivityStatusCode.Error, activity.Status);
    }

[Fact]
    public async Task ValueTask_TraceOnFailure_ResultT_Failure_RecordsFailure()
    {
        var result = DeferredValueTask(Result.Failure<string>(Error.Infrastructure("I", "Desc")));
        using var activity = CreateTestActivity();

        await result.TraceOnFailure("Op10", activity);

        Assert.Equal(ActivityStatusCode.Error, activity.Status);
    }

    [Fact]
    public async Task Task_TraceOnSuccess_ResultT_Failure_DoesNotRecord()
    {
        var result = Deferred(Result.Failure<string>(Error.Unexpected("U", "Desc")));
        using var activity = CreateTestActivity();

        await result.TraceOnSuccess("Op11", activity);

        Assert.Equal(ActivityStatusCode.Unset, activity.Status);
    }

[Fact]
    public async Task ValueTask_TraceOnSuccess_ResultT_Failure_DoesNotRecord()
    {
        var result = DeferredValueTask(Result.Failure<string>(Error.Unexpected("U", "Desc")));
        using var activity = CreateTestActivity();

        await result.TraceOnSuccess("Op11", activity);

        Assert.Equal(ActivityStatusCode.Unset, activity.Status);
    }

    [Fact]
    public async Task Task_TraceOnSuccess_ResultT_Success_RecordsSuccess()
    {
        var result = Deferred(Result.Success(55));
        using var activity = CreateTestActivity();

        await result.TraceOnSuccess("Op12", activity);

        Assert.Equal(ActivityStatusCode.Ok, activity.Status);
    }

[Fact]
    public async Task ValueTask_TraceOnSuccess_ResultT_Success_RecordsSuccess()
    {
        var result = DeferredValueTask(Result.Success(55));
        using var activity = CreateTestActivity();

        await result.TraceOnSuccess("Op12", activity);

        Assert.Equal(ActivityStatusCode.Ok, activity.Status);
    }

    [Theory]
    [InlineData((int)ErrorType.Failure, "failure")]
    [InlineData((int)ErrorType.Validation, "validation")]
    [InlineData((int)ErrorType.NotFound, "not_found")]
    [InlineData((int)ErrorType.Conflict, "conflict")]
    [InlineData((int)ErrorType.Unauthorized, "unauthorized")]
    [InlineData((int)ErrorType.Forbidden, "forbidden")]
    [InlineData((int)ErrorType.Unavailable, "unavailable")]
    [InlineData((int)ErrorType.Unexpected, "unexpected")]
    [InlineData((int)ErrorType.Domain, "domain")]
    [InlineData((int)ErrorType.Infrastructure, "infrastructure")]
    [InlineData((int)ErrorType.Custom, "custom")]
    [InlineData(99, "_OTHER")]
    public async Task Task_TraceOutcome_Result_MapsErrorTypes(int typeValue, string expectedStr)
    {
        var result = Deferred(Result.Failure(Error.Custom("Code", "Desc", (ErrorType)typeValue)));
        using var activity = CreateTestActivity();

        await result.TraceOutcome("MapType", activity);

        Assert.Equal(expectedStr, activity.GetTagItem("error.type"));
    }

[Theory]
    [InlineData((int)ErrorType.Failure, "failure")]
    [InlineData((int)ErrorType.Validation, "validation")]
    [InlineData((int)ErrorType.NotFound, "not_found")]
    [InlineData((int)ErrorType.Conflict, "conflict")]
    [InlineData((int)ErrorType.Unauthorized, "unauthorized")]
    [InlineData((int)ErrorType.Forbidden, "forbidden")]
    [InlineData((int)ErrorType.Unavailable, "unavailable")]
    [InlineData((int)ErrorType.Unexpected, "unexpected")]
    [InlineData((int)ErrorType.Domain, "domain")]
    [InlineData((int)ErrorType.Infrastructure, "infrastructure")]
    [InlineData((int)ErrorType.Custom, "custom")]
    [InlineData(99, "_OTHER")]
    public async Task ValueTask_TraceOutcome_Result_MapsErrorTypes(int typeValue, string expectedStr)
    {
        var result = DeferredValueTask(Result.Failure(Error.Custom("Code", "Desc", (ErrorType)typeValue)));
        using var activity = CreateTestActivity();

        await result.TraceOutcome("MapType", activity);

        Assert.Equal(expectedStr, activity.GetTagItem("error.type"));
    }

    [Theory]
    [InlineData((int)ErrorSeverity.Info, "info")]
    [InlineData((int)ErrorSeverity.Warning, "warning")]
    [InlineData((int)ErrorSeverity.Error, "error")]
    [InlineData((int)ErrorSeverity.Critical, "critical")]
    [InlineData(99, "error")]
    public async Task Task_TraceOutcome_Result_MapsErrorSeverities(int severityValue, string expectedStr)
    {
        var result = Deferred(Result.Failure(Error.Create("Code", "Desc").WithType(ErrorType.Failure).WithSeverity((ErrorSeverity)severityValue).Build()));
        using var activity = CreateTestActivity();

        await result.TraceOutcome("MapSev", activity);

        Assert.Equal(expectedStr, activity.GetTagItem("ericksonlopez.result.error.severity"));
    }

[Theory]
    [InlineData((int)ErrorSeverity.Info, "info")]
    [InlineData((int)ErrorSeverity.Warning, "warning")]
    [InlineData((int)ErrorSeverity.Error, "error")]
    [InlineData((int)ErrorSeverity.Critical, "critical")]
    [InlineData(99, "error")]
    public async Task ValueTask_TraceOutcome_Result_MapsErrorSeverities(int severityValue, string expectedStr)
    {
        var result = DeferredValueTask(Result.Failure(Error.Create("Code", "Desc").WithType(ErrorType.Failure).WithSeverity((ErrorSeverity)severityValue).Build()));
        using var activity = CreateTestActivity();

        await result.TraceOutcome("MapSev", activity);

        Assert.Equal(expectedStr, activity.GetTagItem("ericksonlopez.result.error.severity"));
    }

    [Fact]
    public async Task Task_TraceOutcome_WithNullActivity_DoesNotThrowAndTracksMetrics()
    {
        var result = Deferred(Result.Success());
        var metrics = new ResultMetrics(new Meter(ResultMetrics.MeterName, "1.0"));
        
        // Temporarily clear Activity.Current
        var previous = Activity.Current;
        Activity.Current = null;
        try
        {
            await result.TraceOutcome("NullAct", null, metrics);
        }
        finally
        {
            Activity.Current = previous;
        }

        var measurement = Assert.Single(_measurements);
        Assert.Contains(measurement.Tags.ToArray(), t => t.Key == "ericksonlopez.result.outcome" && (string)t.Value! == "success");
    }

[Fact]
    public async Task ValueTask_TraceOutcome_WithNullActivity_DoesNotThrowAndTracksMetrics()
    {
        var result = DeferredValueTask(Result.Success());
        var metrics = new ResultMetrics(new Meter(ResultMetrics.MeterName, "1.0"));
        
        // Temporarily clear Activity.Current
        var previous = Activity.Current;
        Activity.Current = null;
        try
        {
            await result.TraceOutcome("NullAct", null, metrics);
        }
        finally
        {
            Activity.Current = previous;
        }

        var measurement = Assert.Single(_measurements);
        Assert.Contains(measurement.Tags.ToArray(), t => t.Key == "ericksonlopez.result.outcome" && (string)t.Value! == "success");
    }

[Fact]
    public async Task TaskFast_TraceOutcome_Result_Success_SetsActivityStatusToOk()
    {
        var result = Task.FromResult(Result.Success());
        using var activity = CreateTestActivity();
        var metrics = new ResultMetrics(new Meter(ResultMetrics.MeterName, "1.0"));

        await result.TraceOutcome("Op1", activity, metrics);

        Assert.Equal(ActivityStatusCode.Ok, activity.Status);
        Assert.Equal("Op1", activity.GetTagItem("ericksonlopez.result.operation.name"));
        Assert.Equal("success", activity.GetTagItem("ericksonlopez.result.outcome"));

        var measurement = Assert.Single(_measurements);
        Assert.Contains(measurement.Tags.ToArray(), t => t.Key == "ericksonlopez.result.outcome" && (string)t.Value! == "success");
    }

[Fact]
    public async Task ValueTaskFast_TraceOutcome_Result_Success_SetsActivityStatusToOk()
    {
        var result = ValueTask.FromResult(Result.Success());
        using var activity = CreateTestActivity();
        var metrics = new ResultMetrics(new Meter(ResultMetrics.MeterName, "1.0"));

        await result.TraceOutcome("Op1", activity, metrics);

        Assert.Equal(ActivityStatusCode.Ok, activity.Status);
        Assert.Equal("Op1", activity.GetTagItem("ericksonlopez.result.operation.name"));
        Assert.Equal("success", activity.GetTagItem("ericksonlopez.result.outcome"));

        var measurement = Assert.Single(_measurements);
        Assert.Contains(measurement.Tags.ToArray(), t => t.Key == "ericksonlopez.result.outcome" && (string)t.Value! == "success");
    }

    [Fact]
    public async Task TaskFast_TraceOutcome_Result_Failure_SetsActivityStatusToError()
    {
        var result = Task.FromResult(Result.Failure(Error.Failure("F", "Desc")));
        using var activity = CreateTestActivity();
        var metrics = new ResultMetrics(new Meter(ResultMetrics.MeterName, "1.0"));

        await result.TraceOutcome("Op2", activity, metrics);

        Assert.Equal(ActivityStatusCode.Error, activity.Status);
        Assert.Equal("Desc", activity.StatusDescription);
        Assert.Equal("failure", activity.GetTagItem("error.type"));
        Assert.Equal("F", activity.GetTagItem("ericksonlopez.result.error.code"));
        Assert.Equal("error", activity.GetTagItem("ericksonlopez.result.error.severity"));
        Assert.Equal("Op2", activity.GetTagItem("ericksonlopez.result.operation.name"));
        Assert.Equal("failure", activity.GetTagItem("ericksonlopez.result.outcome"));

        var measurement = Assert.Single(_measurements);
        Assert.Contains(measurement.Tags.ToArray(), t => t.Key == "ericksonlopez.result.outcome" && (string)t.Value! == "failure");
    }

[Fact]
    public async Task ValueTaskFast_TraceOutcome_Result_Failure_SetsActivityStatusToError()
    {
        var result = ValueTask.FromResult(Result.Failure(Error.Failure("F", "Desc")));
        using var activity = CreateTestActivity();
        var metrics = new ResultMetrics(new Meter(ResultMetrics.MeterName, "1.0"));

        await result.TraceOutcome("Op2", activity, metrics);

        Assert.Equal(ActivityStatusCode.Error, activity.Status);
        Assert.Equal("Desc", activity.StatusDescription);
        Assert.Equal("failure", activity.GetTagItem("error.type"));
        Assert.Equal("F", activity.GetTagItem("ericksonlopez.result.error.code"));
        Assert.Equal("error", activity.GetTagItem("ericksonlopez.result.error.severity"));
        Assert.Equal("Op2", activity.GetTagItem("ericksonlopez.result.operation.name"));
        Assert.Equal("failure", activity.GetTagItem("ericksonlopez.result.outcome"));

        var measurement = Assert.Single(_measurements);
        Assert.Contains(measurement.Tags.ToArray(), t => t.Key == "ericksonlopez.result.outcome" && (string)t.Value! == "failure");
    }

    [Fact]
    public async Task TaskFast_TraceOnFailure_Result_Success_DoesNotRecord()
    {
        var result = Task.FromResult(Result.Success());
        using var activity = CreateTestActivity();
        var metrics = new ResultMetrics(new Meter(ResultMetrics.MeterName, "1.0"));

        await result.TraceOnFailure("Op3", activity, metrics);

        Assert.Equal(ActivityStatusCode.Unset, activity.Status);
        Assert.Null(activity.GetTagItem("ericksonlopez.result.outcome"));
        Assert.Empty(_measurements);
    }

[Fact]
    public async Task ValueTaskFast_TraceOnFailure_Result_Success_DoesNotRecord()
    {
        var result = ValueTask.FromResult(Result.Success());
        using var activity = CreateTestActivity();
        var metrics = new ResultMetrics(new Meter(ResultMetrics.MeterName, "1.0"));

        await result.TraceOnFailure("Op3", activity, metrics);

        Assert.Equal(ActivityStatusCode.Unset, activity.Status);
        Assert.Null(activity.GetTagItem("ericksonlopez.result.outcome"));
        Assert.Empty(_measurements);
    }

    [Fact]
    public async Task TaskFast_TraceOnFailure_Result_Failure_RecordsFailure()
    {
        var result = Task.FromResult(Result.Failure(Error.NotFound("N", "Desc")));
        using var activity = CreateTestActivity();

        await result.TraceOnFailure("Op4", activity); // testing with null metrics

        Assert.Equal(ActivityStatusCode.Error, activity.Status);
        Assert.Equal("failure", activity.GetTagItem("ericksonlopez.result.outcome"));
        Assert.Empty(_measurements); // null metrics means no metrics
    }

[Fact]
    public async Task ValueTaskFast_TraceOnFailure_Result_Failure_RecordsFailure()
    {
        var result = ValueTask.FromResult(Result.Failure(Error.NotFound("N", "Desc")));
        using var activity = CreateTestActivity();

        await result.TraceOnFailure("Op4", activity); // testing with null metrics

        Assert.Equal(ActivityStatusCode.Error, activity.Status);
        Assert.Equal("failure", activity.GetTagItem("ericksonlopez.result.outcome"));
        Assert.Empty(_measurements); // null metrics means no metrics
    }

    [Fact]
    public async Task TaskFast_TraceOnSuccess_Result_Failure_DoesNotRecord()
    {
        var result = Task.FromResult(Result.Failure(Error.NotFound("N", "Desc")));
        using var activity = CreateTestActivity();
        
        await result.TraceOnSuccess("Op5", activity);

        Assert.Equal(ActivityStatusCode.Unset, activity.Status);
        Assert.Null(activity.GetTagItem("ericksonlopez.result.outcome"));
    }

[Fact]
    public async Task ValueTaskFast_TraceOnSuccess_Result_Failure_DoesNotRecord()
    {
        var result = ValueTask.FromResult(Result.Failure(Error.NotFound("N", "Desc")));
        using var activity = CreateTestActivity();
        
        await result.TraceOnSuccess("Op5", activity);

        Assert.Equal(ActivityStatusCode.Unset, activity.Status);
        Assert.Null(activity.GetTagItem("ericksonlopez.result.outcome"));
    }

    [Fact]
    public async Task TaskFast_TraceOnSuccess_Result_Success_RecordsSuccess()
    {
        var result = Task.FromResult(Result.Success());
        using var activity = CreateTestActivity();
        
        await result.TraceOnSuccess("Op6", activity);

        Assert.Equal(ActivityStatusCode.Ok, activity.Status);
        Assert.Equal("success", activity.GetTagItem("ericksonlopez.result.outcome"));
    }

[Fact]
    public async Task ValueTaskFast_TraceOnSuccess_Result_Success_RecordsSuccess()
    {
        var result = ValueTask.FromResult(Result.Success());
        using var activity = CreateTestActivity();
        
        await result.TraceOnSuccess("Op6", activity);

        Assert.Equal(ActivityStatusCode.Ok, activity.Status);
        Assert.Equal("success", activity.GetTagItem("ericksonlopez.result.outcome"));
    }

    [Fact]
    public async Task TaskFast_TraceOutcome_ResultT_Success_SetsActivityStatusToOk()
    {
        var result = Task.FromResult(Result.Success(42));
        using var activity = CreateTestActivity();
        var metrics = new ResultMetrics(new Meter(ResultMetrics.MeterName, "1.0"));

        await result.TraceOutcome("Op7", activity, metrics);

        Assert.Equal(ActivityStatusCode.Ok, activity.Status);
        Assert.Equal("success", activity.GetTagItem("ericksonlopez.result.outcome"));
        var measurement = Assert.Single(_measurements);
        Assert.Contains(measurement.Tags.ToArray(), t => t.Key == "ericksonlopez.result.outcome" && (string)t.Value! == "success");
    }

[Fact]
    public async Task ValueTaskFast_TraceOutcome_ResultT_Success_SetsActivityStatusToOk()
    {
        var result = ValueTask.FromResult(Result.Success(42));
        using var activity = CreateTestActivity();
        var metrics = new ResultMetrics(new Meter(ResultMetrics.MeterName, "1.0"));

        await result.TraceOutcome("Op7", activity, metrics);

        Assert.Equal(ActivityStatusCode.Ok, activity.Status);
        Assert.Equal("success", activity.GetTagItem("ericksonlopez.result.outcome"));
        var measurement = Assert.Single(_measurements);
        Assert.Contains(measurement.Tags.ToArray(), t => t.Key == "ericksonlopez.result.outcome" && (string)t.Value! == "success");
    }

    [Fact]
    public async Task TaskFast_TraceOutcome_ResultT_Failure_SetsActivityStatusToError()
    {
        var result = Task.FromResult(Result.Failure<int>(Error.Conflict("C", "Desc")));
        using var activity = CreateTestActivity();
        var metrics = new ResultMetrics(new Meter(ResultMetrics.MeterName, "1.0"));

        await result.TraceOutcome("Op8", activity, metrics);

        Assert.Equal(ActivityStatusCode.Error, activity.Status);
        Assert.Equal("conflict", activity.GetTagItem("error.type"));
        Assert.Equal("C", activity.GetTagItem("ericksonlopez.result.error.code"));
        var measurement = Assert.Single(_measurements);
        Assert.Contains(measurement.Tags.ToArray(), t => t.Key == "ericksonlopez.result.outcome" && (string)t.Value! == "failure");
    }

[Fact]
    public async Task ValueTaskFast_TraceOutcome_ResultT_Failure_SetsActivityStatusToError()
    {
        var result = ValueTask.FromResult(Result.Failure<int>(Error.Conflict("C", "Desc")));
        using var activity = CreateTestActivity();
        var metrics = new ResultMetrics(new Meter(ResultMetrics.MeterName, "1.0"));

        await result.TraceOutcome("Op8", activity, metrics);

        Assert.Equal(ActivityStatusCode.Error, activity.Status);
        Assert.Equal("conflict", activity.GetTagItem("error.type"));
        Assert.Equal("C", activity.GetTagItem("ericksonlopez.result.error.code"));
        var measurement = Assert.Single(_measurements);
        Assert.Contains(measurement.Tags.ToArray(), t => t.Key == "ericksonlopez.result.outcome" && (string)t.Value! == "failure");
    }

    [Fact]
    public async Task TaskFast_TraceOnFailure_ResultT_Success_DoesNotRecord()
    {
        var result = Task.FromResult(Result.Success(10));
        using var activity = CreateTestActivity();

        await result.TraceOnFailure("Op9", activity);

        Assert.Equal(ActivityStatusCode.Unset, activity.Status);
    }

[Fact]
    public async Task ValueTaskFast_TraceOnFailure_ResultT_Success_DoesNotRecord()
    {
        var result = ValueTask.FromResult(Result.Success(10));
        using var activity = CreateTestActivity();

        await result.TraceOnFailure("Op9", activity);

        Assert.Equal(ActivityStatusCode.Unset, activity.Status);
    }

    [Fact]
    public async Task TaskFast_TraceOnFailure_ResultT_Failure_RecordsFailure()
    {
        var result = Task.FromResult(Result.Failure<string>(Error.Infrastructure("I", "Desc")));
        using var activity = CreateTestActivity();

        await result.TraceOnFailure("Op10", activity);

        Assert.Equal(ActivityStatusCode.Error, activity.Status);
    }

[Fact]
    public async Task ValueTaskFast_TraceOnFailure_ResultT_Failure_RecordsFailure()
    {
        var result = ValueTask.FromResult(Result.Failure<string>(Error.Infrastructure("I", "Desc")));
        using var activity = CreateTestActivity();

        await result.TraceOnFailure("Op10", activity);

        Assert.Equal(ActivityStatusCode.Error, activity.Status);
    }

    [Fact]
    public async Task TaskFast_TraceOnSuccess_ResultT_Failure_DoesNotRecord()
    {
        var result = Task.FromResult(Result.Failure<string>(Error.Unexpected("U", "Desc")));
        using var activity = CreateTestActivity();

        await result.TraceOnSuccess("Op11", activity);

        Assert.Equal(ActivityStatusCode.Unset, activity.Status);
    }

[Fact]
    public async Task ValueTaskFast_TraceOnSuccess_ResultT_Failure_DoesNotRecord()
    {
        var result = ValueTask.FromResult(Result.Failure<string>(Error.Unexpected("U", "Desc")));
        using var activity = CreateTestActivity();

        await result.TraceOnSuccess("Op11", activity);

        Assert.Equal(ActivityStatusCode.Unset, activity.Status);
    }

    [Fact]
    public async Task TaskFast_TraceOnSuccess_ResultT_Success_RecordsSuccess()
    {
        var result = Task.FromResult(Result.Success(55));
        using var activity = CreateTestActivity();

        await result.TraceOnSuccess("Op12", activity);

        Assert.Equal(ActivityStatusCode.Ok, activity.Status);
    }

[Fact]
    public async Task ValueTaskFast_TraceOnSuccess_ResultT_Success_RecordsSuccess()
    {
        var result = ValueTask.FromResult(Result.Success(55));
        using var activity = CreateTestActivity();

        await result.TraceOnSuccess("Op12", activity);

        Assert.Equal(ActivityStatusCode.Ok, activity.Status);
    }

    [Theory]
    [InlineData((int)ErrorType.Failure, "failure")]
    [InlineData((int)ErrorType.Validation, "validation")]
    [InlineData((int)ErrorType.NotFound, "not_found")]
    [InlineData((int)ErrorType.Conflict, "conflict")]
    [InlineData((int)ErrorType.Unauthorized, "unauthorized")]
    [InlineData((int)ErrorType.Forbidden, "forbidden")]
    [InlineData((int)ErrorType.Unavailable, "unavailable")]
    [InlineData((int)ErrorType.Unexpected, "unexpected")]
    [InlineData((int)ErrorType.Domain, "domain")]
    [InlineData((int)ErrorType.Infrastructure, "infrastructure")]
    [InlineData((int)ErrorType.Custom, "custom")]
    [InlineData(99, "_OTHER")]
    public async Task TaskFast_TraceOutcome_Result_MapsErrorTypes(int typeValue, string expectedStr)
    {
        var result = Task.FromResult(Result.Failure(Error.Custom("Code", "Desc", (ErrorType)typeValue)));
        using var activity = CreateTestActivity();

        await result.TraceOutcome("MapType", activity);

        Assert.Equal(expectedStr, activity.GetTagItem("error.type"));
    }

[Theory]
    [InlineData((int)ErrorType.Failure, "failure")]
    [InlineData((int)ErrorType.Validation, "validation")]
    [InlineData((int)ErrorType.NotFound, "not_found")]
    [InlineData((int)ErrorType.Conflict, "conflict")]
    [InlineData((int)ErrorType.Unauthorized, "unauthorized")]
    [InlineData((int)ErrorType.Forbidden, "forbidden")]
    [InlineData((int)ErrorType.Unavailable, "unavailable")]
    [InlineData((int)ErrorType.Unexpected, "unexpected")]
    [InlineData((int)ErrorType.Domain, "domain")]
    [InlineData((int)ErrorType.Infrastructure, "infrastructure")]
    [InlineData((int)ErrorType.Custom, "custom")]
    [InlineData(99, "_OTHER")]
    public async Task ValueTaskFast_TraceOutcome_Result_MapsErrorTypes(int typeValue, string expectedStr)
    {
        var result = ValueTask.FromResult(Result.Failure(Error.Custom("Code", "Desc", (ErrorType)typeValue)));
        using var activity = CreateTestActivity();

        await result.TraceOutcome("MapType", activity);

        Assert.Equal(expectedStr, activity.GetTagItem("error.type"));
    }

    [Theory]
    [InlineData((int)ErrorSeverity.Info, "info")]
    [InlineData((int)ErrorSeverity.Warning, "warning")]
    [InlineData((int)ErrorSeverity.Error, "error")]
    [InlineData((int)ErrorSeverity.Critical, "critical")]
    [InlineData(99, "error")]
    public async Task TaskFast_TraceOutcome_Result_MapsErrorSeverities(int severityValue, string expectedStr)
    {
        var result = Task.FromResult(Result.Failure(Error.Create("Code", "Desc").WithType(ErrorType.Failure).WithSeverity((ErrorSeverity)severityValue).Build()));
        using var activity = CreateTestActivity();

        await result.TraceOutcome("MapSev", activity);

        Assert.Equal(expectedStr, activity.GetTagItem("ericksonlopez.result.error.severity"));
    }

[Theory]
    [InlineData((int)ErrorSeverity.Info, "info")]
    [InlineData((int)ErrorSeverity.Warning, "warning")]
    [InlineData((int)ErrorSeverity.Error, "error")]
    [InlineData((int)ErrorSeverity.Critical, "critical")]
    [InlineData(99, "error")]
    public async Task ValueTaskFast_TraceOutcome_Result_MapsErrorSeverities(int severityValue, string expectedStr)
    {
        var result = ValueTask.FromResult(Result.Failure(Error.Create("Code", "Desc").WithType(ErrorType.Failure).WithSeverity((ErrorSeverity)severityValue).Build()));
        using var activity = CreateTestActivity();

        await result.TraceOutcome("MapSev", activity);

        Assert.Equal(expectedStr, activity.GetTagItem("ericksonlopez.result.error.severity"));
    }

    [Fact]
    public async Task TaskFast_TraceOutcome_WithNullActivity_DoesNotThrowAndTracksMetrics()
    {
        var result = Task.FromResult(Result.Success());
        var metrics = new ResultMetrics(new Meter(ResultMetrics.MeterName, "1.0"));
        
        // Temporarily clear Activity.Current
        var previous = Activity.Current;
        Activity.Current = null;
        try
        {
            await result.TraceOutcome("NullAct", null, metrics);
        }
        finally
        {
            Activity.Current = previous;
        }

        var measurement = Assert.Single(_measurements);
        Assert.Contains(measurement.Tags.ToArray(), t => t.Key == "ericksonlopez.result.outcome" && (string)t.Value! == "success");
    }

[Fact]
    public async Task ValueTaskFast_TraceOutcome_WithNullActivity_DoesNotThrowAndTracksMetrics()
    {
        var result = ValueTask.FromResult(Result.Success());
        var metrics = new ResultMetrics(new Meter(ResultMetrics.MeterName, "1.0"));
        
        // Temporarily clear Activity.Current
        var previous = Activity.Current;
        Activity.Current = null;
        try
        {
            await result.TraceOutcome("NullAct", null, metrics);
        }
        finally
        {
            Activity.Current = previous;
        }

        var measurement = Assert.Single(_measurements);
        Assert.Contains(measurement.Tags.ToArray(), t => t.Key == "ericksonlopez.result.outcome" && (string)t.Value! == "success");
    }
}



