// Copyright © Erickson Lopez. MIT License.
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using EricksonLopez.Result;
using EricksonLopez.Result.AspNetCore;
using EricksonLopez.Result.OpenTelemetry;
using EricksonLopez.Result.Serialization;
using Xunit;

namespace EricksonLopez.Result.Tests.Adversarial;

public class AdversarialSecurityAndCorrectnessTests
{
    [Fact]
    public void OpenTelemetry_TraceOutcome_WhenUninitialized_DoesNotRecordSuccess()
    {
        var uninitialized = default(Result);
        using var activity = new Activity("TestActivity").Start();

        // Adversarial check: An uninitialized Result must NOT be marked as Ok/Success in telemetry
        uninitialized.TraceOutcome("TestOp", activity);

        // Before fix: activity.Status was ActivityStatusCode.Ok and tag outcome was "success"!
        // Expected: Should be Error or Unset, never Ok / success!
        Assert.NotEqual(ActivityStatusCode.Ok, activity.Status);
        Assert.NotEqual("success", activity.GetTagItem("ericksonlopez.result.outcome")?.ToString());
    }

    [Fact]
    public void OpenTelemetry_TraceOutcome_Generic_WhenUninitialized_DoesNotRecordSuccess()
    {
        var uninitializedT = default(Result<int>);
        using var activity = new Activity("TestActivityT").Start();

        uninitializedT.TraceOutcome("TestOpT", activity);

        Assert.NotEqual(ActivityStatusCode.Ok, activity.Status);
        Assert.NotEqual("success", activity.GetTagItem("ericksonlopez.result.outcome")?.ToString());
    }

    [Fact]
    public async Task AsyncExtensions_WhenCancellationTokenAlreadyCanceled_FastPathThrowsOperationCanceledException()
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel(); // Pre-canceled token

        var completedTask = Task.FromResult(Result.Success(42));

        // Adversarial check: even if task is completed successfully, a canceled token must be respected!
        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
        {
            await completedTask.Map(x => x * 2, cancellationToken: cts.Token);
        });
    }

    [Fact]
    public async Task ValueTaskExtensions_WhenCancellationTokenAlreadyCanceled_FastPathThrowsOperationCanceledException()
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel(); // Pre-canceled token

        var completedValueTask = new ValueTask<Result<int>>(Result.Success(42));

        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
        {
            await completedValueTask.Map(x => x * 2, cancellationToken: cts.Token);
        });
    }

    [Fact]
    public void ErrorJsonConverter_DeeplyNestedPayload_ThrowsJsonExceptionRatherThanStackOverflow()
    {
        // Build JSON with 50 nested innerErrors to test recursion limiter
        var sb = new System.Text.StringBuilder();
        for (int i = 0; i < 50; i++)
        {
            sb.Append("{\"code\":\"ERR\",\"description\":\"desc\",\"innerErrors\":[");
        }
        sb.Append("{\"code\":\"LEAF\",\"description\":\"leaf\"}");
        for (int i = 0; i < 50; i++)
        {
            sb.Append("]}");
        }

        var json = sb.ToString();
        var options = new JsonSerializerOptions();
        options.Converters.Add(new ErrorJsonConverter());

        // Must throw JsonException and protect the process stack
        Assert.Throws<JsonException>(() =>
        {
            JsonSerializer.Deserialize<Error>(json, options);
        });
    }

    [Fact]
    public void Serialization_UninitializedResult_PreservesIdentityOrThrows_DoesNotMutateToFailure()
    {
        var uninitialized = default(Result);
        var options = new JsonSerializerOptions();
        options.Converters.Add(new ResultJsonConverter());

        var json = JsonSerializer.Serialize(uninitialized, options);
        var deserialized = JsonSerializer.Deserialize<Result>(json, options);

        // Before fix: deserialized.IsFailure was TRUE with Error "Serialization.Error"!
        // After fix: It should NOT be a failure with a fake synthetic error!
        Assert.False(deserialized.IsFailure, "Uninitialized Result was falsely deserialized as a Failure!");
        Assert.True(deserialized.IsUninitialized, "Deserialized Result should be Uninitialized!");
    }

    [Fact]
    public void Serialization_UninitializedResultOfT_PreservesIdentityOrThrows_DoesNotMutateToFailure()
    {
        var uninitialized = default(Result<string>);
        var options = new JsonSerializerOptions();
        options.Converters.Add(new ResultOfTJsonConverter<string>());

        var json = JsonSerializer.Serialize(uninitialized, options);
        var deserialized = JsonSerializer.Deserialize<Result<string>>(json, options);

        Assert.False(deserialized.IsFailure, "Uninitialized Result<T> was falsely deserialized as a Failure!");
        Assert.True(deserialized.IsUninitialized, "Deserialized Result<T> should be Uninitialized!");
    }
}
