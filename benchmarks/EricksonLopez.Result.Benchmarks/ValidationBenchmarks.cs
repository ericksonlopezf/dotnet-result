// Copyright © Erickson Lopez. MIT License.
using System;
using System.Linq;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Order;
using EricksonLopez.Result;

namespace EricksonLopez.Result.Benchmarks;

/// <summary>
/// Measures the performance of cumulative validation with Result.ValidateAll.
/// Validates zero-allocation on happy path and pooling efficiency.
/// </summary>
[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[HideColumns(Column.Error, Column.StdDev, Column.RatioSD)]
public class ValidationBenchmarks
{
    private Func<string, Result>[] _allSuccessValidators = null!;
    private Func<string, Result>[] _oneFailureValidators = null!;
    private Func<string, Result>[] _multipleFailureValidators = null!;

    [Params(4, 10)]
    public int FieldCount { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _allSuccessValidators = Enumerable.Range(0, FieldCount)
            .Select<int, Func<string, Result>>(_ => _ => Result.Success())
            .ToArray();

        _oneFailureValidators = Enumerable.Range(0, FieldCount)
            .Select<int, Func<string, Result>>(i => s => i == 0 ? Error.Validation("V", $"Error {i}") : Result.Success())
            .ToArray();

        _multipleFailureValidators = Enumerable.Range(0, FieldCount)
            .Select<int, Func<string, Result>>(i => s => i % 2 == 0 ? Error.Validation("V", $"Error {i}") : Result.Success())
            .ToArray();
    }

    [Benchmark(Baseline = true)]
    public Result<string> ValidateAll_AllSuccess()
        => Result.ValidateAll("test-payload", _allSuccessValidators);

    [Benchmark]
    public Result<string> ValidateAll_OneFailure()
        => Result.ValidateAll("test-payload", _oneFailureValidators);

    [Benchmark]
    public Result<string> ValidateAll_MultipleFailures()
        => Result.ValidateAll("test-payload", _multipleFailureValidators);
}
