// Copyright © Erickson Lopez. MIT License.
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Linq;
using EricksonLopez.Result;
using EricksonLopez.Result.OpenTelemetry;
using Xunit;

namespace EricksonLopez.Result.OpenTelemetry.Tests;

[Collection("Metrics")]
public class ResultActivityExtensionsTests : IDisposable
{
    private readonly ActivityListener _activityListener;
    private readonly List<Activity> _activities = new();
    private readonly MeterListener _meterListener;
    private readonly List<Measurement<long>> _measurements = new();

    public ResultActivityExtensionsTests()
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
    public void TraceOutcome_Result_Success_SetsActivityStatusToOk_AndRecordsMetrics()
    {
        var result = Result.Success();
        using var activity = CreateTestActivity();
        var metrics = new ResultMetrics(new Meter(ResultMetrics.MeterName, "1.0"));

        result.TraceOutcome("Op1", activity, metrics);

        Assert.Equal(ActivityStatusCode.Ok, activity.Status);
        Assert.Equal("Op1", activity.GetTagItem("ericksonlopez.result.operation.name"));
        Assert.Equal("success", activity.GetTagItem("ericksonlopez.result.outcome"));

        var measurement = Assert.Single(_measurements);
        Assert.Contains(measurement.Tags.ToArray(), t => t.Key == "ericksonlopez.result.outcome" && (string)t.Value! == "success");
    }

    [Fact]
    public void TraceOutcome_Result_Failure_SetsActivityStatusToError_AndRecordsMetrics()
    {
        var result = Result.Failure(Error.Failure("F", "Desc"));
        using var activity = CreateTestActivity();
        var metrics = new ResultMetrics(new Meter(ResultMetrics.MeterName, "1.0"));

        result.TraceOutcome("Op2", activity, metrics);

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
    public void TraceOutcome_Result_Success_WithNullMetrics_DoesNotRecordMetrics()
    {
        var result = Result.Success();
        using var activity = CreateTestActivity();

        result.TraceOutcome("Op1", activity); // metrics = null

        Assert.Equal(ActivityStatusCode.Ok, activity.Status);
        Assert.Equal("Op1", activity.GetTagItem("ericksonlopez.result.operation.name"));
        Assert.Equal("success", activity.GetTagItem("ericksonlopez.result.outcome"));

        Assert.Empty(_measurements);
    }

    [Fact]
    public void TraceOutcome_Result_Failure_WithNullMetrics_DoesNotRecordMetrics()
    {
        var result = Result.Failure(Error.Failure("F", "Desc"));
        using var activity = CreateTestActivity();

        result.TraceOutcome("Op2", activity); // metrics = null

        Assert.Equal(ActivityStatusCode.Error, activity.Status);
        Assert.Equal("failure", activity.GetTagItem("error.type"));
        Assert.Empty(_measurements);
    }

    [Fact]
    public void TraceOnFailure_Result_Success_DoesNotRecord()
    {
        var result = Result.Success();
        using var activity = CreateTestActivity();
        var metrics = new ResultMetrics(new Meter(ResultMetrics.MeterName, "1.0"));

        result.TraceOnFailure("Op3", activity, metrics);

        Assert.Equal(ActivityStatusCode.Unset, activity.Status);
        Assert.Null(activity.GetTagItem("ericksonlopez.result.outcome"));
        Assert.Empty(_measurements);
    }

    [Fact]
    public void TraceOnFailure_Result_Failure_RecordsFailure()
    {
        var result = Result.Failure(Error.NotFound("N", "Desc"));
        using var activity = CreateTestActivity();
        var metrics = new ResultMetrics(new Meter(ResultMetrics.MeterName, "1.0"));

        result.TraceOnFailure("Op4", activity, metrics);

        Assert.Equal(ActivityStatusCode.Error, activity.Status);
        Assert.Equal("failure", activity.GetTagItem("ericksonlopez.result.outcome"));
        Assert.Equal("not_found", activity.GetTagItem("error.type"));
        Assert.Equal("N", activity.GetTagItem("ericksonlopez.result.error.code"));
        Assert.Equal("warning", activity.GetTagItem("ericksonlopez.result.error.severity"));
        Assert.Equal("Op4", activity.GetTagItem("ericksonlopez.result.operation.name"));

        var measurement = Assert.Single(_measurements);
        Assert.Contains(measurement.Tags.ToArray(), t => t.Key == "ericksonlopez.result.outcome" && (string)t.Value! == "failure");
    }

    [Fact]
    public void TraceOnFailure_Result_Failure_WithNullMetrics_DoesNotRecordMetrics()
    {
        var result = Result.Failure(Error.NotFound("N", "Desc"));
        using var activity = CreateTestActivity();

        result.TraceOnFailure("Op4", activity); // null metrics

        Assert.Equal(ActivityStatusCode.Error, activity.Status);
        Assert.Equal("failure", activity.GetTagItem("ericksonlopez.result.outcome"));
        Assert.Empty(_measurements);
    }

    [Fact]
    public void TraceOnSuccess_Result_Failure_DoesNotRecord()
    {
        var result = Result.Failure(Error.NotFound("N", "Desc"));
        using var activity = CreateTestActivity();
        var metrics = new ResultMetrics(new Meter(ResultMetrics.MeterName, "1.0"));

        result.TraceOnSuccess("Op5", activity, metrics);

        Assert.Equal(ActivityStatusCode.Unset, activity.Status);
        Assert.Null(activity.GetTagItem("ericksonlopez.result.outcome"));
        Assert.Empty(_measurements);
    }

    [Fact]
    public void TraceOnSuccess_Result_Success_RecordsSuccess()
    {
        var result = Result.Success();
        using var activity = CreateTestActivity();
        var metrics = new ResultMetrics(new Meter(ResultMetrics.MeterName, "1.0"));

        result.TraceOnSuccess("Op6", activity, metrics);

        Assert.Equal(ActivityStatusCode.Ok, activity.Status);
        Assert.Equal("success", activity.GetTagItem("ericksonlopez.result.outcome"));
        Assert.Equal("Op6", activity.GetTagItem("ericksonlopez.result.operation.name"));

        var measurement = Assert.Single(_measurements);
        Assert.Contains(measurement.Tags.ToArray(), t => t.Key == "ericksonlopez.result.outcome" && (string)t.Value! == "success");
    }

    [Fact]
    public void TraceOnSuccess_Result_Success_WithNullMetrics_DoesNotRecordMetrics()
    {
        var result = Result.Success();
        using var activity = CreateTestActivity();

        result.TraceOnSuccess("Op6", activity);

        Assert.Equal(ActivityStatusCode.Ok, activity.Status);
        Assert.Equal("success", activity.GetTagItem("ericksonlopez.result.outcome"));
        Assert.Empty(_measurements);
    }


    // ---------------------- Result<T> tests ------------------------

    [Fact]
    public void TraceOutcome_ResultT_Success_SetsActivityStatusToOk_AndRecordsMetrics()
    {
        var result = Result.Success(42);
        using var activity = CreateTestActivity();
        var metrics = new ResultMetrics(new Meter(ResultMetrics.MeterName, "1.0"));

        result.TraceOutcome("Op7", activity, metrics);

        Assert.Equal(ActivityStatusCode.Ok, activity.Status);
        Assert.Equal("Op7", activity.GetTagItem("ericksonlopez.result.operation.name"));
        Assert.Equal("success", activity.GetTagItem("ericksonlopez.result.outcome"));

        var measurement = Assert.Single(_measurements);
        Assert.Contains(measurement.Tags.ToArray(), t => t.Key == "ericksonlopez.result.outcome" && (string)t.Value! == "success");
    }

    [Fact]
    public void TraceOutcome_ResultT_Failure_SetsActivityStatusToError_AndRecordsMetrics()
    {
        var result = Result.Failure<int>(Error.Conflict("C", "Desc"));
        using var activity = CreateTestActivity();
        var metrics = new ResultMetrics(new Meter(ResultMetrics.MeterName, "1.0"));

        result.TraceOutcome("Op8", activity, metrics);

        Assert.Equal(ActivityStatusCode.Error, activity.Status);
        Assert.Equal("Desc", activity.StatusDescription);
        Assert.Equal("conflict", activity.GetTagItem("error.type"));
        Assert.Equal("C", activity.GetTagItem("ericksonlopez.result.error.code"));
        Assert.Equal("warning", activity.GetTagItem("ericksonlopez.result.error.severity"));
        Assert.Equal("Op8", activity.GetTagItem("ericksonlopez.result.operation.name"));
        Assert.Equal("failure", activity.GetTagItem("ericksonlopez.result.outcome"));

        var measurement = Assert.Single(_measurements);
        Assert.Contains(measurement.Tags.ToArray(), t => t.Key == "ericksonlopez.result.outcome" && (string)t.Value! == "failure");
    }

    [Fact]
    public void TraceOutcome_ResultT_Success_WithNullMetrics_DoesNotRecordMetrics()
    {
        var result = Result.Success(42);
        using var activity = CreateTestActivity();

        result.TraceOutcome("Op7", activity); // metrics = null

        Assert.Equal(ActivityStatusCode.Ok, activity.Status);
        Assert.Equal("Op7", activity.GetTagItem("ericksonlopez.result.operation.name"));
        Assert.Equal("success", activity.GetTagItem("ericksonlopez.result.outcome"));

        Assert.Empty(_measurements);
    }

    [Fact]
    public void TraceOutcome_ResultT_Failure_WithNullMetrics_DoesNotRecordMetrics()
    {
        var result = Result.Failure<int>(Error.Conflict("C", "Desc"));
        using var activity = CreateTestActivity();

        result.TraceOutcome("Op8", activity); // metrics = null

        Assert.Equal(ActivityStatusCode.Error, activity.Status);
        Assert.Equal("conflict", activity.GetTagItem("error.type"));
        Assert.Empty(_measurements);
    }

    [Fact]
    public void TraceOnFailure_ResultT_Success_DoesNotRecord()
    {
        var result = Result.Success(10);
        using var activity = CreateTestActivity();
        var metrics = new ResultMetrics(new Meter(ResultMetrics.MeterName, "1.0"));

        result.TraceOnFailure("Op9", activity, metrics);

        Assert.Equal(ActivityStatusCode.Unset, activity.Status);
        Assert.Empty(_measurements);
    }

    [Fact]
    public void TraceOnFailure_ResultT_Failure_RecordsFailure()
    {
        var result = Result.Failure<string>(Error.Infrastructure("I", "Desc"));
        using var activity = CreateTestActivity();
        var metrics = new ResultMetrics(new Meter(ResultMetrics.MeterName, "1.0"));

        result.TraceOnFailure("Op10", activity, metrics);

        Assert.Equal(ActivityStatusCode.Error, activity.Status);
        Assert.Equal("infrastructure", activity.GetTagItem("error.type"));
        Assert.Equal("I", activity.GetTagItem("ericksonlopez.result.error.code"));
        Assert.Equal("failure", activity.GetTagItem("ericksonlopez.result.outcome"));
        Assert.Equal("error", activity.GetTagItem("ericksonlopez.result.error.severity"));
        Assert.Equal("Op10", activity.GetTagItem("ericksonlopez.result.operation.name"));

        var measurement = Assert.Single(_measurements);
        Assert.Contains(measurement.Tags.ToArray(), t => t.Key == "ericksonlopez.result.outcome" && (string)t.Value! == "failure");
    }

    [Fact]
    public void TraceOnFailure_ResultT_Failure_WithNullMetrics_DoesNotRecordMetrics()
    {
        var result = Result.Failure<string>(Error.Infrastructure("I", "Desc"));
        using var activity = CreateTestActivity();

        result.TraceOnFailure("Op10", activity);

        Assert.Equal(ActivityStatusCode.Error, activity.Status);
        Assert.Empty(_measurements);
    }

    [Fact]
    public void TraceOnSuccess_ResultT_Failure_DoesNotRecord()
    {
        var result = Result.Failure<string>(Error.Unexpected("U", "Desc"));
        using var activity = CreateTestActivity();
        var metrics = new ResultMetrics(new Meter(ResultMetrics.MeterName, "1.0"));

        result.TraceOnSuccess("Op11", activity, metrics);

        Assert.Equal(ActivityStatusCode.Unset, activity.Status);
        Assert.Empty(_measurements);
    }

    [Fact]
    public void ResultSource_IsProperlyInitialized()
    {
        Assert.NotNull(ResultActivityExtensions.ResultSource);
        Assert.Equal(ResultActivityExtensions.ActivitySourceName, ResultActivityExtensions.ResultSource.Name);
        Assert.Equal(ResultMetrics.AssemblyVersion, ResultActivityExtensions.ResultSource.Version);
    }

    [Fact]
    public void TraceOnSuccess_ResultT_Success_RecordsSuccess()
    {
        var result = Result.Success(55);
        using var activity = CreateTestActivity();
        var metrics = new ResultMetrics(new Meter(ResultMetrics.MeterName, "1.0"));

        result.TraceOnSuccess("Op12", activity, metrics);

        Assert.Equal(ActivityStatusCode.Ok, activity.Status);
        Assert.Equal("Op12", activity.GetTagItem("ericksonlopez.result.operation.name"));
        Assert.Equal("success", activity.GetTagItem("ericksonlopez.result.outcome"));

        var measurement = Assert.Single(_measurements);
        Assert.Contains(measurement.Tags.ToArray(), t => t.Key == "ericksonlopez.result.outcome" && (string)t.Value! == "success");
    }

    [Fact]
    public void TraceOnSuccess_ResultT_Success_WithNullMetrics_DoesNotRecordMetrics()
    {
        var result = Result.Success(55);
        using var activity = CreateTestActivity();

        result.TraceOnSuccess("Op12", activity);

        Assert.Equal(ActivityStatusCode.Ok, activity.Status);
        Assert.Equal("Op12", activity.GetTagItem("ericksonlopez.result.operation.name"));
        Assert.Equal("success", activity.GetTagItem("ericksonlopez.result.outcome"));
        Assert.Empty(_measurements);
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
    public void TraceOutcome_Result_MapsErrorTypes(int typeValue, string expectedStr)
    {
        var result = Result.Failure(Error.Custom("Code", "Desc", (ErrorType)typeValue));
        using var activity = CreateTestActivity();

        result.TraceOutcome("MapType", activity);

        Assert.Equal(expectedStr, activity.GetTagItem("error.type"));
    }

    [Theory]
    [InlineData((int)ErrorSeverity.Info, "info")]
    [InlineData((int)ErrorSeverity.Warning, "warning")]
    [InlineData((int)ErrorSeverity.Error, "error")]
    [InlineData((int)ErrorSeverity.Critical, "critical")]
    [InlineData(99, "error")]
    public void TraceOutcome_Result_MapsErrorSeverities(int severityValue, string expectedStr)
    {
        var result = Result.Failure(Error.Create("Code", "Desc").WithType(ErrorType.Failure).WithSeverity((ErrorSeverity)severityValue).Build());
        using var activity = CreateTestActivity();

        result.TraceOutcome("MapSev", activity);

        Assert.Equal(expectedStr, activity.GetTagItem("ericksonlopez.result.error.severity"));
    }

    [Fact]
    public void TraceOutcome_WithNullActivity_DoesNotThrowAndTracksMetrics()
    {
        var result = Result.Success();
        var metrics = new ResultMetrics(new Meter(ResultMetrics.MeterName, "1.0"));

        var previous = Activity.Current;
        Activity.Current = null;
        try
        {
            result.TraceOutcome("NullAct", null, metrics);
        }
        finally
        {
            Activity.Current = previous;
        }

        var measurement = Assert.Single(_measurements);
        Assert.Contains(measurement.Tags.ToArray(), t => t.Key == "ericksonlopez.result.outcome" && (string)t.Value! == "success");
    }

    [Fact]
    public void TraceOutcome_Failure_WithNullActivity_DoesNotThrowAndTracksMetrics()
    {
        var result = Result.Failure(Error.Unexpected("X", "X"));
        var metrics = new ResultMetrics(new Meter(ResultMetrics.MeterName, "1.0"));
        var previous = Activity.Current;
        Activity.Current = null;
        try
        {
            result.TraceOutcome("NullAct2", null, metrics);
            result.TraceOnFailure("NullAct3", null, metrics);
            Result.Success().TraceOnSuccess("NullAct4", null, metrics);

            var resultT = Result.Failure<int>(Error.Unexpected("X", "X"));
            resultT.TraceOutcome("NullAct5", null, metrics);
            resultT.TraceOnFailure("NullAct6", null, metrics);
            Result.Success(1).TraceOnSuccess("NullAct7", null, metrics);
        }
        finally
        {
            Activity.Current = previous;
        }

        Assert.NotEmpty(_measurements);
    }

    [Fact]
    public void TraceOutcome_Prioritizes_TargetActivity_Over_CurrentActivity()
    {
        var result = Result.Failure(Error.Unexpected("X", "X"));
        using var currentAct = CreateTestActivity();
        using var targetAct = CreateTestActivity();

        var previous = Activity.Current;
        Activity.Current = currentAct;
        try
        {
            result.TraceOutcome("Priority1", targetAct);
            result.TraceOnFailure("Priority2", targetAct);

            var resultT = Result.Failure<int>(Error.Unexpected("X", "X"));
            resultT.TraceOutcome("Priority4", targetAct);
            resultT.TraceOnFailure("Priority5", targetAct);
        }
        finally
        {
            Activity.Current = previous;
        }

        // targetAct should have all the tags, currentAct should have none
        Assert.Equal(ActivityStatusCode.Error, targetAct.Status);
        Assert.Equal(ActivityStatusCode.Unset, currentAct.Status);
        Assert.Null(currentAct.GetTagItem("ericksonlopez.result.outcome"));
        Assert.NotNull(targetAct.GetTagItem("ericksonlopez.result.outcome"));
    }

    [Fact]
    public void TraceOutcome_Prioritizes_TargetActivity_Over_CurrentActivity_For_Success()
    {
        using var currentAct = CreateTestActivity();
        using var targetAct = CreateTestActivity();

        var previous = Activity.Current;
        Activity.Current = currentAct;
        try
        {
            Result.Success().TraceOnSuccess("Priority3", targetAct);
            Result.Success(1).TraceOnSuccess("Priority6", targetAct);
        }
        finally
        {
            Activity.Current = previous;
        }

        Assert.Equal(ActivityStatusCode.Ok, targetAct.Status);
        Assert.Equal(ActivityStatusCode.Unset, currentAct.Status);
    }

    [Fact]
    public void TraceOutcome_Result_Falls_Back()
    {
        using var currentAct = CreateTestActivity();
        var previous = Activity.Current;
        Activity.Current = currentAct;
        try { Result.Failure(Error.Unexpected("X", "X")).TraceOutcome("Fallback", targetActivity: null); }
        finally { Activity.Current = previous; }
        Assert.NotEqual(ActivityStatusCode.Unset, currentAct.Status);
    }

    [Fact]
    public void TraceOnFailure_Result_Falls_Back()
    {
        using var currentAct = CreateTestActivity();
        var previous = Activity.Current;
        Activity.Current = currentAct;
        try { Result.Failure(Error.Unexpected("X", "X")).TraceOnFailure("Fallback", targetActivity: null); }
        finally { Activity.Current = previous; }
        Assert.NotEqual(ActivityStatusCode.Unset, currentAct.Status);
    }

    [Fact]
    public void TraceOnSuccess_Result_Falls_Back()
    {
        using var currentAct = CreateTestActivity();
        var previous = Activity.Current;
        Activity.Current = currentAct;
        try { Result.Success().TraceOnSuccess("Fallback", targetActivity: null); }
        finally { Activity.Current = previous; }
        Assert.NotEqual(ActivityStatusCode.Unset, currentAct.Status);
    }

    [Fact]
    public void TraceOutcome_ResultT_Falls_Back()
    {
        using var currentAct = CreateTestActivity();
        var previous = Activity.Current;
        Activity.Current = currentAct;
        try { Result.Failure<int>(Error.Unexpected("X", "X")).TraceOutcome("Fallback", targetActivity: null); }
        finally { Activity.Current = previous; }
        Assert.NotEqual(ActivityStatusCode.Unset, currentAct.Status);
    }

    [Fact]
    public void TraceOnFailure_ResultT_Falls_Back()
    {
        using var currentAct = CreateTestActivity();
        var previous = Activity.Current;
        Activity.Current = currentAct;
        try { Result.Failure<int>(Error.Unexpected("X", "X")).TraceOnFailure("Fallback", targetActivity: null); }
        finally { Activity.Current = previous; }
        Assert.NotEqual(ActivityStatusCode.Unset, currentAct.Status);
    }

    [Fact]
    public void TraceOnSuccess_ResultT_Falls_Back()
    {
        using var currentAct = CreateTestActivity();
        var previous = Activity.Current;
        Activity.Current = currentAct;
        try { Result.Success(1).TraceOnSuccess("Fallback", targetActivity: null); }
        finally { Activity.Current = previous; }
        Assert.NotEqual(ActivityStatusCode.Unset, currentAct.Status);
    }
}



