using System.Diagnostics;
using System.Threading.Tasks;

namespace EricksonLopez.Result.OpenTelemetry;

/// <summary>
/// Async extension methods for recording <see cref="Result"/> and <see cref="Result{T}"/> outcomes
/// as OpenTelemetry activities and metrics in async pipelines.
/// </summary>
/// <remarks>
/// These overloads allow chaining <c>.TraceOutcome()</c> directly on <c>Task&lt;Result&gt;</c>
/// and <c>Task&lt;Result&lt;T&gt;&gt;</c> without breaking the fluent async pipeline by awaiting first.
/// Follows the same sync-path optimization pattern as the core <c>ResultExtensions</c>: when the
/// task is already completed synchronously, no async state machine is allocated.
/// </remarks>

public static class ResultActivityAsyncExtensions
{
    // ─── Task<Result> (non-generic) ───────────────────────────────────────────

    /// <inheritdoc cref="ResultActivityExtensions.TraceOutcome(in Result, string, Activity?, ResultMetrics?)"/>
    public static Task<Result> TraceOutcome(
        this Task<Result> resultTask,
        string operationName,
        Activity? targetActivity = null,
        ResultMetrics? metrics = null)
    {
        // Stryker disable all : Fast path optimization
        if (resultTask.IsCompletedSuccessfully)
            return Task.FromResult(resultTask.Result.TraceOutcome(operationName, targetActivity, metrics));
        return TraceOutcomeCore(resultTask, operationName, targetActivity, metrics);
        // Stryker restore all
        // Stryker disable all : Excluded from coverage
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage] static async Task<Result> TraceOutcomeCore(Task<Result> t, string op, Activity? act, ResultMetrics? m)
            => (await t.ConfigureAwait(false)).TraceOutcome(op, act, m);
        // Stryker restore all
    }

    /// <inheritdoc cref="ResultActivityExtensions.TraceOnFailure(in Result, string, Activity?, ResultMetrics?)"/>
    public static Task<Result> TraceOnFailure(
        this Task<Result> resultTask,
        string operationName,
        Activity? targetActivity = null,
        ResultMetrics? metrics = null)
    {
        // Stryker disable all : Fast path optimization
        if (resultTask.IsCompletedSuccessfully)
            return Task.FromResult(resultTask.Result.TraceOnFailure(operationName, targetActivity, metrics));
        return TraceOnFailureCore(resultTask, operationName, targetActivity, metrics);
        // Stryker restore all
        // Stryker disable all : Excluded from coverage
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage] static async Task<Result> TraceOnFailureCore(Task<Result> t, string op, Activity? act, ResultMetrics? m)
            => (await t.ConfigureAwait(false)).TraceOnFailure(op, act, m);
        // Stryker restore all
    }

    /// <inheritdoc cref="ResultActivityExtensions.TraceOnSuccess(in Result, string, Activity?, ResultMetrics?)"/>
    public static Task<Result> TraceOnSuccess(
        this Task<Result> resultTask,
        string operationName,
        Activity? targetActivity = null,
        ResultMetrics? metrics = null)
    {
        // Stryker disable all : Fast path optimization
        if (resultTask.IsCompletedSuccessfully)
            return Task.FromResult(resultTask.Result.TraceOnSuccess(operationName, targetActivity, metrics));
        return TraceOnSuccessCore(resultTask, operationName, targetActivity, metrics);
        // Stryker restore all
        // Stryker disable all : Excluded from coverage
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage] static async Task<Result> TraceOnSuccessCore(Task<Result> t, string op, Activity? act, ResultMetrics? m)
            => (await t.ConfigureAwait(false)).TraceOnSuccess(op, act, m);
        // Stryker restore all
    }

    // ─── Task<Result<T>> (generic) ────────────────────────────────────────────

    /// <inheritdoc cref="ResultActivityExtensions.TraceOutcome{T}(in Result{T}, string, Activity?, ResultMetrics?)"/>
    public static Task<Result<T>> TraceOutcome<T>(
        this Task<Result<T>> resultTask,
        string operationName,
        Activity? targetActivity = null,
        ResultMetrics? metrics = null)
    {
        // Stryker disable all : Fast path optimization
        if (resultTask.IsCompletedSuccessfully)
            return Task.FromResult(resultTask.Result.TraceOutcome(operationName, targetActivity, metrics));
        return TraceOutcomeCore(resultTask, operationName, targetActivity, metrics);
        // Stryker restore all
        static async Task<Result<T>> TraceOutcomeCore(Task<Result<T>> t, string op, Activity? act, ResultMetrics? m)
            => (await t.ConfigureAwait(false)).TraceOutcome(op, act, m);
    }

    /// <inheritdoc cref="ResultActivityExtensions.TraceOnFailure{T}(in Result{T}, string, Activity?, ResultMetrics?)"/>
    public static Task<Result<T>> TraceOnFailure<T>(
        this Task<Result<T>> resultTask,
        string operationName,
        Activity? targetActivity = null,
        ResultMetrics? metrics = null)
    {
        // Stryker disable all : Fast path optimization
        if (resultTask.IsCompletedSuccessfully)
            return Task.FromResult(resultTask.Result.TraceOnFailure(operationName, targetActivity, metrics));
        return TraceOnFailureCore(resultTask, operationName, targetActivity, metrics);
        // Stryker restore all
        static async Task<Result<T>> TraceOnFailureCore(Task<Result<T>> t, string op, Activity? act, ResultMetrics? m)
            => (await t.ConfigureAwait(false)).TraceOnFailure(op, act, m);
    }

    /// <inheritdoc cref="ResultActivityExtensions.TraceOnSuccess{T}(in Result{T}, string, Activity?, ResultMetrics?)"/>
    public static Task<Result<T>> TraceOnSuccess<T>(
        this Task<Result<T>> resultTask,
        string operationName,
        Activity? targetActivity = null,
        ResultMetrics? metrics = null)
    {
        // Stryker disable all : Fast path optimization
        if (resultTask.IsCompletedSuccessfully)
            return Task.FromResult(resultTask.Result.TraceOnSuccess(operationName, targetActivity, metrics));
        return TraceOnSuccessCore(resultTask, operationName, targetActivity, metrics);
        // Stryker restore all
        static async Task<Result<T>> TraceOnSuccessCore(Task<Result<T>> t, string op, Activity? act, ResultMetrics? m)
            => (await t.ConfigureAwait(false)).TraceOnSuccess(op, act, m);
    }

    // ─── ValueTask<Result> (non-generic) ─────────────────────────────────────

    /// <inheritdoc cref="ResultActivityExtensions.TraceOutcome(in Result, string, Activity?, ResultMetrics?)"/>
    public static ValueTask<Result> TraceOutcome(
        this ValueTask<Result> resultTask,
        string operationName,
        Activity? targetActivity = null,
        ResultMetrics? metrics = null)
    {
        // Stryker disable all : Fast path optimization
        if (resultTask.IsCompletedSuccessfully)
            return new ValueTask<Result>(resultTask.Result.TraceOutcome(operationName, targetActivity, metrics));
        return new ValueTask<Result>(TraceOutcomeCore(resultTask, operationName, targetActivity, metrics));
        // Stryker restore all
        // Stryker disable all : Excluded from coverage
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage] static async Task<Result> TraceOutcomeCore(ValueTask<Result> t, string op, Activity? act, ResultMetrics? m)
            => (await t.ConfigureAwait(false)).TraceOutcome(op, act, m);
        // Stryker restore all
    }

    /// <inheritdoc cref="ResultActivityExtensions.TraceOnFailure(in Result, string, Activity?, ResultMetrics?)"/>
    public static ValueTask<Result> TraceOnFailure(
        this ValueTask<Result> resultTask,
        string operationName,
        Activity? targetActivity = null,
        ResultMetrics? metrics = null)
    {
        // Stryker disable all : Fast path optimization
        if (resultTask.IsCompletedSuccessfully)
            return new ValueTask<Result>(resultTask.Result.TraceOnFailure(operationName, targetActivity, metrics));
        return new ValueTask<Result>(TraceOnFailureCore(resultTask, operationName, targetActivity, metrics));
        // Stryker restore all
        // Stryker disable all : Excluded from coverage
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage] static async Task<Result> TraceOnFailureCore(ValueTask<Result> t, string op, Activity? act, ResultMetrics? m)
            => (await t.ConfigureAwait(false)).TraceOnFailure(op, act, m);
        // Stryker restore all
    }

    /// <inheritdoc cref="ResultActivityExtensions.TraceOnSuccess(in Result, string, Activity?, ResultMetrics?)"/>
    public static ValueTask<Result> TraceOnSuccess(
        this ValueTask<Result> resultTask,
        string operationName,
        Activity? targetActivity = null,
        ResultMetrics? metrics = null)
    {
        // Stryker disable all : Fast path optimization
        if (resultTask.IsCompletedSuccessfully)
            return new ValueTask<Result>(resultTask.Result.TraceOnSuccess(operationName, targetActivity, metrics));
        return new ValueTask<Result>(TraceOnSuccessCore(resultTask, operationName, targetActivity, metrics));
        // Stryker restore all
        // Stryker disable all : Excluded from coverage
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage] static async Task<Result> TraceOnSuccessCore(ValueTask<Result> t, string op, Activity? act, ResultMetrics? m)
            => (await t.ConfigureAwait(false)).TraceOnSuccess(op, act, m);
        // Stryker restore all
    }

    // ─── ValueTask<Result<T>> (generic) ──────────────────────────────────────

    /// <inheritdoc cref="ResultActivityExtensions.TraceOutcome{T}(in Result{T}, string, Activity?, ResultMetrics?)"/>
    public static ValueTask<Result<T>> TraceOutcome<T>(
        this ValueTask<Result<T>> resultTask,
        string operationName,
        Activity? targetActivity = null,
        ResultMetrics? metrics = null)
    {
        // Stryker disable all : Fast path optimization
        if (resultTask.IsCompletedSuccessfully)
            return new ValueTask<Result<T>>(resultTask.Result.TraceOutcome(operationName, targetActivity, metrics));
        return new ValueTask<Result<T>>(TraceOutcomeCore(resultTask, operationName, targetActivity, metrics));
        // Stryker restore all
        static async Task<Result<T>> TraceOutcomeCore(ValueTask<Result<T>> t, string op, Activity? act, ResultMetrics? m)
            => (await t.ConfigureAwait(false)).TraceOutcome(op, act, m);
    }

    /// <inheritdoc cref="ResultActivityExtensions.TraceOnFailure{T}(in Result{T}, string, Activity?, ResultMetrics?)"/>
    public static ValueTask<Result<T>> TraceOnFailure<T>(
        this ValueTask<Result<T>> resultTask,
        string operationName,
        Activity? targetActivity = null,
        ResultMetrics? metrics = null)
    {
        // Stryker disable all : Fast path optimization
        if (resultTask.IsCompletedSuccessfully)
            return new ValueTask<Result<T>>(resultTask.Result.TraceOnFailure(operationName, targetActivity, metrics));
        return new ValueTask<Result<T>>(TraceOnFailureCore(resultTask, operationName, targetActivity, metrics));
        // Stryker restore all
        static async Task<Result<T>> TraceOnFailureCore(ValueTask<Result<T>> t, string op, Activity? act, ResultMetrics? m)
            => (await t.ConfigureAwait(false)).TraceOnFailure(op, act, m);
    }

    /// <inheritdoc cref="ResultActivityExtensions.TraceOnSuccess{T}(in Result{T}, string, Activity?, ResultMetrics?)"/>
    public static ValueTask<Result<T>> TraceOnSuccess<T>(
        this ValueTask<Result<T>> resultTask,
        string operationName,
        Activity? targetActivity = null,
        ResultMetrics? metrics = null)
    {
        // Stryker disable all : Fast path optimization
        if (resultTask.IsCompletedSuccessfully)
            return new ValueTask<Result<T>>(resultTask.Result.TraceOnSuccess(operationName, targetActivity, metrics));
        return new ValueTask<Result<T>>(TraceOnSuccessCore(resultTask, operationName, targetActivity, metrics));
        // Stryker restore all
        static async Task<Result<T>> TraceOnSuccessCore(ValueTask<Result<T>> t, string op, Activity? act, ResultMetrics? m)
            => (await t.ConfigureAwait(false)).TraceOnSuccess(op, act, m);
    }
}


