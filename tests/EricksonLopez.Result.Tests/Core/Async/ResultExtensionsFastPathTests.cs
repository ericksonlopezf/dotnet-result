// Copyright © Erickson Lopez. MIT License.
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AwesomeAssertions;
using EricksonLopez.Result;
using EricksonLopez.Result.Testing;
using Xunit;

namespace EricksonLopez.Result.Tests.Core.Async;

public class ResultExtensionsFastPathTests : ResultExtensionsTestsBase
{
    [Fact]
    public async Task Switch_Task_WhenSuccessFastPath_InvokesSuccessAction()
    {
        var task = Task.FromResult(Result.Success());
        bool called = false;

        await task.Execute(
            42,
            state => { called = state == 42; },
            (state, err) => { }
        );

        called.Should().BeTrue();
    }

    [Fact]
    public async Task Switch_Task_WhenFailureFastPath_InvokesFailureAction()
    {
        var task = Task.FromResult(Result.Failure(TestError));
        bool called = false;

        await task.Execute(
            42,
            state => { },
            (state, err) => { called = state == 42 && err == TestError; }
        );

        called.Should().BeTrue();
    }

    [Fact]
    public async Task TapOnFailure_ValueTask_WhenFailureFastPath_InvokesCallback()
    {
        var task = new ValueTask<Result>(Result.Failure(TestError));
        bool called = false;

        var result = await task.TapOnFailure(err =>
        {
            called = err == TestError;
            return new ValueTask();
        });

        called.Should().BeTrue();
        result.ShouldBeFailure().Should().BeSameAs(TestError);
    }

    [Fact]
    public async Task Bind_TaskOfResultT_WhenCompletedAsynchronously_ReturnsSuccess()
    {
        var tcs = new TaskCompletionSource<Result<int>>();
        var task = tcs.Task.Bind(x => Task.FromResult(Result.Success()));
        tcs.SetResult(Result.Success(1));

        var result = await task;

        result.ShouldBeSuccess();
    }

    [Fact]
    public async Task Ensure_TaskOfResultT_WhenPredicateReturnsTrue_ReturnsSuccess()
    {
        var tcs = new TaskCompletionSource<Result<int>>();
        var task = tcs.Task.Ensure(x => Task.FromResult(true), TestError);
        tcs.SetResult(Result.Success(1));

        var result = await task;

        result.ShouldBeSuccess().Should().Be(1);
    }

    [Fact]
    public async Task Ensure_TaskOfResultT_WithState_WhenPredicateReturnsTrue_ReturnsSuccess()
    {
        var tcs = new TaskCompletionSource<Result<int>>();
        var task = tcs.Task.Ensure("state", (s, x) => Task.FromResult(s == "state" && x == 1), TestError);
        tcs.SetResult(Result.Success(1));

        var result = await task;

        result.ShouldBeSuccess().Should().Be(1);
    }

    [Fact]
    public async Task Bind_TaskOfResult_WithState_WhenCompleted_ReturnsSuccessWithValue()
    {
        var tcs = new TaskCompletionSource<Result>();
        var task = tcs.Task.Bind("state", s => Result.Success(s.Length));
        tcs.SetResult(Result.Success());

        var result = await task;

        result.ShouldBeSuccess().Should().Be(5);
    }

    [Fact]
    public async Task Ensure_ValueTaskOfResultT_WhenAsyncPredicateReturnsTrue_ReturnsSuccess()
    {
        var tcs = new TaskCompletionSource<Result<int>>();
        var vt = new ValueTask<Result<int>>(tcs.Task);
        var task = vt.Ensure(x => new ValueTask<bool>(x == 1), TestError);
        tcs.SetResult(Result.Success(1));

        var result = await task;

        result.ShouldBeSuccess().Should().Be(1);
    }

    [Fact]
    public async Task Ensure_ValueTaskOfResultT_WithState_WhenAsyncPredicateReturnsTrue_ReturnsSuccess()
    {
        var tcs = new TaskCompletionSource<Result<int>>();
        var vt = new ValueTask<Result<int>>(tcs.Task);
        var task = vt.Ensure("state", (s, x) => new ValueTask<bool>(s == "state" && x == 1), TestError);
        tcs.SetResult(Result.Success(1));

        var result = await task;

        result.ShouldBeSuccess().Should().Be(1);
    }

    [Fact]
    public async Task Bind_ValueTaskOfResult_WithState_WhenCompleted_ReturnsSuccessWithValue()
    {
        var tcs = new TaskCompletionSource<Result>();
        var vt = new ValueTask<Result>(tcs.Task);
        var task = vt.Bind("state", s => Result.Success(s.Length));
        tcs.SetResult(Result.Success());

        var result = await task;

        result.ShouldBeSuccess().Should().Be(5);
    }

    [Fact]
    public async Task Ensure_ValueTaskOfResultT_WhenSourceIsFailureFastPath_PropagatesFailure()
    {
        var failedResult = Result.Failure<int>(TestError);
        var vt = new ValueTask<Result<int>>(failedResult);

        var e1 = await vt.Ensure(x => new ValueTask<bool>(true), TestError2);
        e1.ShouldBeFailure().Should().BeSameAs(TestError);

        var e2 = await vt.Ensure("state", (s, x) => new ValueTask<bool>(true), TestError2);
        e2.ShouldBeFailure().Should().BeSameAs(TestError);
    }

    [Fact]
    public async Task Bind_ValueTaskOfResult_WhenSourceIsFailureFastPath_PropagatesFailure()
    {
        var failedResultNonGen = Result.Failure(TestError);
        var vtNonGen = new ValueTask<Result>(failedResultNonGen);

        var b1 = await vtNonGen.Bind("state", s => Result.Success(1));
        b1.ShouldBeFailure().Should().BeSameAs(TestError);

        var b2 = await vtNonGen.Bind(() => new ValueTask<Result<int>>(Result.Success(1)));
        b2.ShouldBeFailure().Should().BeSameAs(TestError);
    }
}
