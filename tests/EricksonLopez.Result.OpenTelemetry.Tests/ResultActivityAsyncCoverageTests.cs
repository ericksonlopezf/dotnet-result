// Copyright © Erickson Lopez. MIT License.
using System;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Threading.Tasks;
using EricksonLopez.Result;
using EricksonLopez.Result.OpenTelemetry;
using Xunit;

namespace EricksonLopez.Result.OpenTelemetry.Tests;

[Collection("Metrics")]
public class ResultActivityAsyncCoverageTests
{
    private static (Task<T> task, Action<T> complete) CreatePendingTask<T>()
    {
        var tcs = new TaskCompletionSource<T>(TaskCreationOptions.RunContinuationsAsynchronously);
        return (tcs.Task, value => tcs.SetResult(value));
    }

    private static (ValueTask<T> task, Action<T> complete) CreatePendingValueTask<T>()
    {
        var tcs = new TaskCompletionSource<T>(TaskCreationOptions.RunContinuationsAsynchronously);
        return (new ValueTask<T>(tcs.Task), value => tcs.SetResult(value));
    }

    [Fact]
    public async Task Task_NonGeneric_SlowPath_AllMethods()
    {
        // TraceOutcome - Success
        {
            var (task, complete) = CreatePendingTask<Result>();
            var traceTask = task.TraceOutcome("Op");
            complete(Result.Success());
            var res = await traceTask;
            Assert.True(res.IsSuccess);
        }

        // TraceOutcome - Failure
        {
            var (task, complete) = CreatePendingTask<Result>();
            var traceTask = task.TraceOutcome("Op");
            complete(Result.Failure(Error.Failure("F", "Msg")));
            var res = await traceTask;
            Assert.True(res.IsFailure);
        }

        // TraceOnFailure - Success
        {
            var (task, complete) = CreatePendingTask<Result>();
            var traceTask = task.TraceOnFailure("Op");
            complete(Result.Success());
            var res = await traceTask;
            Assert.True(res.IsSuccess);
        }

        // TraceOnFailure - Failure
        {
            var (task, complete) = CreatePendingTask<Result>();
            var traceTask = task.TraceOnFailure("Op");
            complete(Result.Failure(Error.Failure("F", "Msg")));
            var res = await traceTask;
            Assert.True(res.IsFailure);
        }

        // TraceOnSuccess - Success
        {
            var (task, complete) = CreatePendingTask<Result>();
            var traceTask = task.TraceOnSuccess("Op");
            complete(Result.Success());
            var res = await traceTask;
            Assert.True(res.IsSuccess);
        }

        // TraceOnSuccess - Failure
        {
            var (task, complete) = CreatePendingTask<Result>();
            var traceTask = task.TraceOnSuccess("Op");
            complete(Result.Failure(Error.Failure("F", "Msg")));
            var res = await traceTask;
            Assert.True(res.IsFailure);
        }
    }

    [Fact]
    public async Task Task_Generic_SlowPath_AllMethods()
    {
        // TraceOutcome - Success
        {
            var (task, complete) = CreatePendingTask<Result<int>>();
            var traceTask = task.TraceOutcome("Op");
            complete(Result.Success(42));
            var res = await traceTask;
            Assert.True(res.IsSuccess);
            Assert.Equal(42, res.Value);
        }

        // TraceOutcome - Failure
        {
            var (task, complete) = CreatePendingTask<Result<int>>();
            var traceTask = task.TraceOutcome("Op");
            complete(Result.Failure<int>(Error.Failure("F", "Msg")));
            var res = await traceTask;
            Assert.True(res.IsFailure);
        }

        // TraceOnFailure - Success
        {
            var (task, complete) = CreatePendingTask<Result<int>>();
            var traceTask = task.TraceOnFailure("Op");
            complete(Result.Success(42));
            var res = await traceTask;
            Assert.True(res.IsSuccess);
        }

        // TraceOnFailure - Failure
        {
            var (task, complete) = CreatePendingTask<Result<int>>();
            var traceTask = task.TraceOnFailure("Op");
            complete(Result.Failure<int>(Error.Failure("F", "Msg")));
            var res = await traceTask;
            Assert.True(res.IsFailure);
        }

        // TraceOnSuccess - Success
        {
            var (task, complete) = CreatePendingTask<Result<int>>();
            var traceTask = task.TraceOnSuccess("Op");
            complete(Result.Success(42));
            var res = await traceTask;
            Assert.True(res.IsSuccess);
        }

        // TraceOnSuccess - Failure
        {
            var (task, complete) = CreatePendingTask<Result<int>>();
            var traceTask = task.TraceOnSuccess("Op");
            complete(Result.Failure<int>(Error.Failure("F", "Msg")));
            var res = await traceTask;
            Assert.True(res.IsFailure);
        }
    }

    [Fact]
    public async Task ValueTask_NonGeneric_SlowPath_AllMethods()
    {
        // TraceOutcome - Success
        {
            var (task, complete) = CreatePendingValueTask<Result>();
            var traceTask = task.TraceOutcome("Op");
            complete(Result.Success());
            var res = await traceTask;
            Assert.True(res.IsSuccess);
        }

        // TraceOutcome - Failure
        {
            var (task, complete) = CreatePendingValueTask<Result>();
            var traceTask = task.TraceOutcome("Op");
            complete(Result.Failure(Error.Failure("F", "Msg")));
            var res = await traceTask;
            Assert.True(res.IsFailure);
        }

        // TraceOnFailure - Success
        {
            var (task, complete) = CreatePendingValueTask<Result>();
            var traceTask = task.TraceOnFailure("Op");
            complete(Result.Success());
            var res = await traceTask;
            Assert.True(res.IsSuccess);
        }

        // TraceOnFailure - Failure
        {
            var (task, complete) = CreatePendingValueTask<Result>();
            var traceTask = task.TraceOnFailure("Op");
            complete(Result.Failure(Error.Failure("F", "Msg")));
            var res = await traceTask;
            Assert.True(res.IsFailure);
        }

        // TraceOnSuccess - Success
        {
            var (task, complete) = CreatePendingValueTask<Result>();
            var traceTask = task.TraceOnSuccess("Op");
            complete(Result.Success());
            var res = await traceTask;
            Assert.True(res.IsSuccess);
        }

        // TraceOnSuccess - Failure
        {
            var (task, complete) = CreatePendingValueTask<Result>();
            var traceTask = task.TraceOnSuccess("Op");
            complete(Result.Failure(Error.Failure("F", "Msg")));
            var res = await traceTask;
            Assert.True(res.IsFailure);
        }
    }

    [Fact]
    public async Task ValueTask_Generic_SlowPath_AllMethods()
    {
        // TraceOutcome - Success
        {
            var (task, complete) = CreatePendingValueTask<Result<string>>();
            var traceTask = task.TraceOutcome("Op");
            complete(Result.Success("hello"));
            var res = await traceTask;
            Assert.True(res.IsSuccess);
            Assert.Equal("hello", res.Value);
        }

        // TraceOutcome - Failure
        {
            var (task, complete) = CreatePendingValueTask<Result<string>>();
            var traceTask = task.TraceOutcome("Op");
            complete(Result.Failure<string>(Error.Failure("F", "Msg")));
            var res = await traceTask;
            Assert.True(res.IsFailure);
        }

        // TraceOnFailure - Success
        {
            var (task, complete) = CreatePendingValueTask<Result<string>>();
            var traceTask = task.TraceOnFailure("Op");
            complete(Result.Success("hello"));
            var res = await traceTask;
            Assert.True(res.IsSuccess);
        }

        // TraceOnFailure - Failure
        {
            var (task, complete) = CreatePendingValueTask<Result<string>>();
            var traceTask = task.TraceOnFailure("Op");
            complete(Result.Failure<string>(Error.Failure("F", "Msg")));
            var res = await traceTask;
            Assert.True(res.IsFailure);
        }

        // TraceOnSuccess - Success
        {
            var (task, complete) = CreatePendingValueTask<Result<string>>();
            var traceTask = task.TraceOnSuccess("Op");
            complete(Result.Success("hello"));
            var res = await traceTask;
            Assert.True(res.IsSuccess);
        }

        // TraceOnSuccess - Failure
        {
            var (task, complete) = CreatePendingValueTask<Result<string>>();
            var traceTask = task.TraceOnSuccess("Op");
            complete(Result.Failure<string>(Error.Failure("F", "Msg")));
            var res = await traceTask;
            Assert.True(res.IsFailure);
        }
    }
}
