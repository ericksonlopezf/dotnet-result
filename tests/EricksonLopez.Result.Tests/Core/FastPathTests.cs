using System;
using System.Threading.Tasks;
using Xunit;

namespace EricksonLopez.Result.Tests.Core;

public class FastPathTests
{
    private static readonly Error TestError = Error.Failure("E", "M");

    [Fact]
    public async Task Switch_Task_State_FastPath_Success()
    {
        var task = Task.FromResult(Result.Success());
        bool called = false;
        await task.Execute(
            42,
            (state) => { called = true; },
            (state, err) => { }
        );
        Assert.True(called);
    }
    
    [Fact]
    public async Task Switch_Task_State_FastPath_Failure()
    {
        var task = Task.FromResult(Result.Failure(TestError));
        bool called = false;
        await task.Execute(
            42,
            (state) => { },
            (state, err) => { called = true; }
        );
        Assert.True(called);
    }

    [Fact]
    public async Task TapOnFailure_ValueTask_FastPath_Failure()
    {
        var task = new ValueTask<Result>(Result.Failure(TestError));
        bool called = false;
        await task.TapOnFailure(err => { called = true; return new ValueTask(); });
        Assert.True(called);
    }

    [Fact]
    public void Error_TraceIdValue_Property_Coverage()
    {
        var error = Error.Failure("A", "B").WithTraceId("custom-trace");
        var builder = ErrorBuilder.FromError(error);
        Assert.NotNull(builder.Build().TraceId);
    }

    [Fact]
    public void ResultOfT_Uninitialized_GetHashCode_Coverage()
    {
        var result = default(Result<int>);
        var hash = result.GetHashCode();
        Assert.NotEqual(0, hash);
    }

    [Fact]
    public void ResultOfT_Failure_GetHashCode_Coverage()
    {
        var result = Result.Failure<int>(Error.Failure("A", "B"));
        var hash = result.GetHashCode();
        Assert.NotEqual(0, hash);
    }

    [Fact]
    public void Error_TraceIdValue_Property_Hit_When_NoOverride()
    {
        using var listener = new System.Diagnostics.ActivityListener
        {
            ShouldListenTo = _ => true,
            Sample = (ref System.Diagnostics.ActivityCreationOptions<System.Diagnostics.ActivityContext> _) => System.Diagnostics.ActivitySamplingResult.AllData
        };
        System.Diagnostics.ActivitySource.AddActivityListener(listener);
        var source = new System.Diagnostics.ActivitySource("Test");
        using var act = source.StartActivity("Test");
        
        var error = Error.Failure("A", "B"); // Captures Activity TraceId
        var builder = ErrorBuilder.FromError(error);
        Assert.NotNull(builder.Build().TraceId);
    }

    [Fact]
    public async Task Bind_TaskOfResultT_FuncT_TaskOfResult_Incomplete()
    {
        var tcs = new TaskCompletionSource<Result<int>>();
        var task = tcs.Task.Bind(x => Task.FromResult(Result.Success()));
        tcs.SetResult(Result.Success(1));
        await task;
    }

    [Fact]
    public async Task Ensure_TaskOfResultT_FuncT_TaskOfBool_Incomplete()
    {
        var tcs = new TaskCompletionSource<Result<int>>();
        var task = tcs.Task.Ensure(x => Task.FromResult(true), Error.Failure("A", "B"));
        tcs.SetResult(Result.Success(1));
        await task;
    }

    [Fact]
    public async Task Ensure_TaskOfResultT_FuncTStateT_TaskOfBool_Incomplete()
    {
        var tcs = new TaskCompletionSource<Result<int>>();
        var task = tcs.Task.Ensure("state", (s, x) => Task.FromResult(true), Error.Failure("A", "B"));
        tcs.SetResult(Result.Success(1));
        await task;
    }

    [Fact]
    public async Task Bind_TaskOfResult_FuncTState_ResultTNext_Incomplete()
    {
        var tcs = new TaskCompletionSource<Result>();
        var task = tcs.Task.Bind("state", s => Result.Success(1));
        tcs.SetResult(Result.Success());
        await task;
    }
    
    [Fact]
    public void Result_Combine_Span_FailureBranch()
    {
        var span = new ReadOnlySpan<Result<int>>(new[] { Result.Failure<int>(Error.Failure("A", "B")) });
        Result.Combine(span);
    }
    
    [Fact]
    public void Result_Combine_Enumerable_FailureBranch()
    {
        var list = new List<Result<int>> { Result.Failure<int>(Error.Failure("A", "B")) };
        Result.Combine(list.ToArray());
    }

    

    [Fact]
    public void ResultOfT_Failure_Null_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => Result<string>.Failure(null!));
    }
    
    [Fact]
    public void ResultOfT_Ensure_ErrorFactory_TrueAndFalse()
    {
        var success = Result.Success(42);
        
        var r1 = success.Ensure(x => true, () => Error.Failure("1", "1"));
        Assert.True(r1.IsSuccess);
        
        var r2 = success.Ensure(x => false, () => Error.Failure("1", "1"));
        Assert.True(r2.IsFailure);
        
        var r3 = success.Ensure("state", (s, x) => true, () => Error.Failure("1", "1"));
        Assert.True(r3.IsSuccess);
        
        var r4 = success.Ensure("state", (s, x) => false, () => Error.Failure("1", "1"));
        Assert.True(r4.IsFailure);
    }
    
    [Fact]
    public void ResultOfT_GetHashCode_NullValue()
    {
        var success = Result<string>.Success(null!);
        Assert.NotEqual(0, success.GetHashCode());
    }

    [Fact]
    public async Task Ensure_ValueTaskOfResultT_FuncT_ValueTaskOfBool_Incomplete()
    {
        var tcs = new TaskCompletionSource<Result<int>>();
        var vt = new ValueTask<Result<int>>(tcs.Task);
        var task = vt.Ensure(x => new ValueTask<bool>(true), Error.Failure("A", "B"));
        tcs.SetResult(Result.Success(1));
        await task;
    }

    [Fact]
    public async Task Ensure_ValueTaskOfResultT_FuncTStateT_ValueTaskOfBool_Incomplete()
    {
        var tcs = new TaskCompletionSource<Result<int>>();
        var vt = new ValueTask<Result<int>>(tcs.Task);
        var task = vt.Ensure("state", (s, x) => new ValueTask<bool>(true), Error.Failure("A", "B"));
        tcs.SetResult(Result.Success(1));
        await task;
    }

    [Fact]
    public async Task Bind_ValueTaskOfResult_FuncTState_ResultTNext_Incomplete()
    {
        var tcs = new TaskCompletionSource<Result>();
        var vt = new ValueTask<Result>(tcs.Task);
        var task = vt.Bind("state", s => Result.Success(1));
        tcs.SetResult(Result.Success());
        await task;
    }

    [Fact]
    public async Task ResultExtensions_ValueTaskFastPaths_AllFailureBranches()
    {
        var failedResult = Result.Failure<int>(Error.Failure("A", "B"));
        var vt = new ValueTask<Result<int>>(failedResult);
        var e1 = await vt.Ensure(x => new ValueTask<bool>(true), Error.Failure("C", "D"));
        Assert.True(e1.IsFailure);
        var e2 = await vt.Ensure("state", (s, x) => new ValueTask<bool>(true), Error.Failure("C", "D"));
        Assert.True(e2.IsFailure);
        var failedResultNonGen = Result.Failure(Error.Failure("A", "B"));
        var vtNonGen = new ValueTask<Result>(failedResultNonGen);
        var b1 = await vtNonGen.Bind("state", s => Result.Success(1));
        Assert.True(b1.IsFailure);
    }

    [Fact]
    public async Task ResultExtensions_ValueTaskFastPaths_AllFailureBranches_Part2()
    {
        var failedResult = Result.Failure<int>(Error.Failure("A", "B"));
        var vt = new ValueTask<Result<int>>(failedResult);
        var e1 = await vt.Ensure(x => new ValueTask<bool>(true), Error.Failure("C", "D"));
        Assert.True(e1.IsFailure);
        var e2 = await vt.Ensure("state", (s, x) => new ValueTask<bool>(true), Error.Failure("C", "D"));
        Assert.True(e2.IsFailure);
        var failedResultNonGen = Result.Failure(Error.Failure("A", "B"));
        var vtNonGen = new ValueTask<Result>(failedResultNonGen);
        var b1 = await vtNonGen.Bind(() => new ValueTask<Result<int>>(Result.Success(1)));
        Assert.True(b1.IsFailure);
    }
}
