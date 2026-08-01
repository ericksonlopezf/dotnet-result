using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Order;

namespace EricksonLopez.Result.Benchmarks;

/// <summary>
/// Measures the cost of constructing Result and Result{T} instances.
/// Validates that struct-based construction is allocation-free.
/// </summary>
[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[HideColumns(Column.Error, Column.StdDev, Column.RatioSD)]
[SimpleJob(RuntimeMoniker.Net80)]
[SimpleJob(RuntimeMoniker.Net10_0)]
public class ResultConstructionBenchmarks
{
    private static readonly Error CachedError = Error.Failure("Bench.Error", "Benchmark error");

    [Benchmark(Baseline = true)]
    public Result Success_NonGeneric()
        => Result.Success();

    [Benchmark]
    public Result Failure_NonGeneric()
        => Result.Failure(CachedError);

    [Benchmark]
    public Result<int> Success_Int()
        => Result.Success(42);

    [Benchmark]
    public Result<string> Success_String()
        => Result.Success("hello");

    [Benchmark]
    public Result<int> Failure_Int()
        => Result.Failure<int>(CachedError);

    [Benchmark]
    public Result<Guid> Success_Guid()
        => Result.Success(Guid.NewGuid());

    [Benchmark]
    public Result<int> ImplicitConversion_Value()
        => 42;

    [Benchmark]
    public Result<int> ImplicitConversion_Error()
        => CachedError;
}


