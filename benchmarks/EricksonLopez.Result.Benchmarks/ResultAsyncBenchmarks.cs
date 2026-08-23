// Copyright © Erickson Lopez. MIT License.
using System;
using System.Threading;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Order;
using EricksonLopez.Result;

namespace EricksonLopez.Result.Benchmarks;

/// <summary>
/// Measures the async pipeline overhead, validating the sync-path optimization
/// (IsCompletedSuccessfully fast path) avoids state machine allocation.
/// </summary>
[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[HideColumns(Column.Error, Column.StdDev, Column.RatioSD)]

public class ResultAsyncBenchmarks
{
    private static readonly Error BenchError = Error.Failure("Bench.Error", "fail");

    // ─── Sync-completed Task (hot path) ────────────────────────────────────

    [Benchmark(Baseline = true)]
    public Task<Result<int>> Map_SyncCompleted()
        => Task.FromResult(Result.Success(42)).Map(x => x * 2);

    [Benchmark]
    public Task<Result<int>> Map_SyncCompleted_TState()
        => Task.FromResult(Result.Success(42)).Map(2, static (mul, x) => x * mul);

    [Benchmark]
    public Task<Result<string>> Bind_SyncCompleted()
        => Task.FromResult(Result.Success(42)).Bind(x => Result.Success(x.ToString()));

    [Benchmark]
    public Task<Result<int>> Ensure_SyncCompleted_Passes()
        => Task.FromResult(Result.Success(42)).Ensure(x => x > 0, BenchError);

    [Benchmark]
    public Task<Result<int>> Ensure_SyncCompleted_Fails()
        => Task.FromResult(Result.Success(42)).Ensure(x => x > 100, BenchError);

    [Benchmark]
    public Task<Result<int>> Tap_SyncCompleted()
    {
        int sink = 0;
        return Task.FromResult(Result.Success(42)).TapOnSuccess(x => sink = x);
    }

    [Benchmark]
    public Task<Result<int>> Failure_Map_SyncCompleted()
        => Task.FromResult(Result.Failure<int>(BenchError)).Map(x => x * 2);

    // ─── Async-completed Task (slow path) ──────────────────────────────────

    [Benchmark]
    public Task<Result<int>> Map_AsyncCompleted()
        => SlowSuccessTask(42).Map(x => x * 2);

    [Benchmark]
    public Task<Result<int>> Failure_Map_AsyncCompleted()
        => SlowFailureTask<int>().Map(x => x * 2);

    private static async Task<Result<T>> SlowSuccessTask<T>(T value)
    {
        await Task.Yield();
        return Result.Success(value);
    }

    private static async Task<Result<T>> SlowFailureTask<T>()
    {
        await Task.Yield();
        return Result.Failure<T>(BenchError);
    }
}







