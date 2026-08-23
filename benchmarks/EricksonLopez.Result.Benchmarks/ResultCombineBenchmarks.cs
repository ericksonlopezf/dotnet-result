// Copyright © Erickson Lopez. MIT License.
using System;
using System.Linq;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Order;
using EricksonLopez.Result;

namespace EricksonLopez.Result.Benchmarks;

/// <summary>
/// Measures the cost of Result.Combine with varying error counts.
/// Validates the ArrayPool optimization and zero-alloc happy path.
/// </summary>
[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[HideColumns(Column.Error, Column.StdDev, Column.RatioSD)]

public class ResultCombineBenchmarks
{
    private Result[] _allSuccess = null!;
    private Result[] _oneFailure = null!;
    private Result[] _halfFailures = null!;
    private Result[] _allFailures = null!;

    [Params(4, 16, 64)]
    public int Count { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _allSuccess = Enumerable.Range(0, Count).Select(_ => Result.Success()).ToArray();

        _oneFailure = Enumerable.Range(0, Count)
            .Select(i => i == 0 ? Result.Failure(Error.Failure("E", $"Error {i}")) : Result.Success())
            .ToArray();

        _halfFailures = Enumerable.Range(0, Count)
            .Select(i => i % 2 == 0 ? Result.Failure(Error.Failure("E", $"Error {i}")) : Result.Success())
            .ToArray();

        _allFailures = Enumerable.Range(0, Count)
            .Select(i => Result.Failure(Error.Failure("E", $"Error {i}")))
            .ToArray();
    }

    [Benchmark(Baseline = true)]
    public Result Combine_AllSuccess()
        => Result.Combine(_allSuccess);

    [Benchmark]
    public Result Combine_OneFailure()
        => Result.Combine(_oneFailure);

    [Benchmark]
    public Result Combine_HalfFailures()
        => Result.Combine(_halfFailures);

    [Benchmark]
    public Result Combine_AllFailures()
        => Result.Combine(_allFailures);
}





