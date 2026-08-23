// Copyright © Erickson Lopez. MIT License.
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Order;
using EricksonLopez.Result;

namespace EricksonLopez.Result.Benchmarks;

/// <summary>
/// Measures the cost of pipeline operations (Map, Bind, Match, Switch, Tap, Ensure).
/// Compares standard lambda overloads vs TState zero-closure overloads.
/// </summary>
[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[HideColumns(Column.Error, Column.StdDev, Column.RatioSD)]

public class ResultPipelineBenchmarks
{
    private Result<int> _successResult;
    private Result<int> _failureResult;
    private int _capturedMultiplier;

    [GlobalSetup]
    public void Setup()
    {
        _successResult = Result.Success(42);
        _failureResult = Result.Failure<int>(Error.Failure("Bench.Error", "fail"));
        _capturedMultiplier = 10;
    }

    // ─── Map ───────────────────────────────────────────────────────────────

    [Benchmark(Baseline = true)]
    public Result<int> Map_Success_Lambda()
        => _successResult.Map(x => x * _capturedMultiplier);

    [Benchmark]
    public Result<int> Map_Success_TState()
        => _successResult.Map(_capturedMultiplier, static (mul, x) => x * mul);

    [Benchmark]
    public Result<int> Map_Failure_Lambda()
        => _failureResult.Map(x => x * _capturedMultiplier);

    [Benchmark]
    public Result<int> Map_Failure_TState()
        => _failureResult.Map(_capturedMultiplier, static (mul, x) => x * mul);

    // ─── Bind ──────────────────────────────────────────────────────────────

    [Benchmark]
    public Result<string> Bind_Success_Lambda()
        => _successResult.Bind(x => Result.Success(x.ToString()));

    [Benchmark]
    public Result<string> Bind_Success_TState()
        => _successResult.Bind(0, static (_, x) => Result.Success(x.ToString()));

    [Benchmark]
    public Result<string> Bind_Failure_Lambda()
        => _failureResult.Bind(x => Result.Success(x.ToString()));

    // ─── Match ─────────────────────────────────────────────────────────────

    [Benchmark]
    public string Match_Success_Lambda()
        => _successResult.Match(
            x => $"ok:{x}",
            e => $"err:{e.Code}");

    [Benchmark]
    public string Match_Success_TState()
        => _successResult.Match(
            "prefix",
            static (p, x) => $"{p}:{x}",
            static (p, e) => $"{p}:{e.Code}");

    // ─── Ensure ────────────────────────────────────────────────────────────

    private static readonly Error ThresholdError = Error.Validation("Bench.Threshold", "Below threshold");

    [Benchmark]
    public Result<int> Ensure_Success_Passes_Lambda()
        => _successResult.Ensure(x => x > 0, ThresholdError);

    [Benchmark]
    public Result<int> Ensure_Success_Passes_TState()
        => _successResult.Ensure(0, static (threshold, x) => x > threshold, ThresholdError);

    [Benchmark]
    public Result<int> Ensure_Success_Fails()
        => _successResult.Ensure(x => x > 100, ThresholdError);

    // ─── Tap ───────────────────────────────────────────────────────────────

    private int _sideEffect;

    [Benchmark]
    public Result<int> Tap_Success_Lambda()
        => _successResult.TapOnSuccess(x => _sideEffect = x);

    [Benchmark]
    public Result<int> Tap_Success_TState()
        => _successResult.TapOnSuccess(this, static (self, x) => self._sideEffect = x);

    // ─── Full Pipeline ─────────────────────────────────────────────────────

    [Benchmark]
    public Result<string> FullPipeline_Lambda()
        => _successResult
            .Map(x => x * _capturedMultiplier)
            .Ensure(x => x > 0, ThresholdError)
            .Map(x => x.ToString())
            .TapOnSuccess(x => _sideEffect = x.Length);

    [Benchmark]
    public Result<string> FullPipeline_TState()
        => _successResult
            .Map(_capturedMultiplier, static (mul, x) => x * mul)
            .Ensure(0, static (threshold, x) => x > threshold, ThresholdError)
            .Map(0, static (_, x) => x.ToString())
            .TapOnSuccess(this, static (self, x) => self._sideEffect = x.Length);
}




