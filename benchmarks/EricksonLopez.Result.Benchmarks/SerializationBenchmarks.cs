using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Order;
using System.Text.Json;
using EricksonLopez.Result.Serialization;

// Benchmarks intentionally use the reflection-based constructor to measure its performance baseline.
// CS0618/CS0619: Suppressed because this benchmark explicitly measures the reflection path as a baseline
// to compare against the AOT-safe JsonTypeInfo<T> overload.
#pragma warning disable CS0618, CS0619

namespace EricksonLopez.Result.Benchmarks;

/// <summary>
/// Benchmarks for Error JSON serialization, specifically measuring the cost of
/// metadata serialization with the type-aware WriteMetadataValue approach.
/// </summary>
[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[HideColumns(Column.Error, Column.StdDev, Column.RatioSD)]
[SimpleJob(RuntimeMoniker.Net80)]
[SimpleJob(RuntimeMoniker.Net10_0)]
public class SerializationBenchmarks
{
    private Error _errorNoMetadata = null!;
    private Error _errorWithStringMetadata = null!;
    private Error _errorWithMixedMetadata = null!;
    private Error _errorWithInnerErrors = null!;
    private Result _successResult;
    private Result _failureResult;
    private Result<int> _successResultOfT;
    private Result<int> _failureResultOfT;

    private JsonSerializerOptions _options = null!;
    private byte[] _serializedError = null!;
    private byte[] _serializedResult = null!;

    [GlobalSetup]
    public void Setup()
    {
        _errorNoMetadata = Error.Failure("Bench.NoMeta", "No metadata error");

        _errorWithStringMetadata = Error.Create("Bench.StringMeta", "String metadata error")
            .WithMetadata("key1", "value1")
            .WithMetadata("key2", "value2")
            .WithMetadata("key3", "value3")
            .Build();

        _errorWithMixedMetadata = Error.Create("Bench.MixedMeta", "Mixed metadata error")
            .WithMetadata("count", 42)
            .WithMetadata("ratio", 3.14)
            .WithMetadata("active", true)
            .WithMetadata("name", "test")
            .WithMetadata("id", System.Guid.Empty)
            .Build();

        _errorWithInnerErrors = Error.Create("Bench.Inner", "Error with inner errors")
            .WithInnerError(Error.Validation("Inner.1", "First inner"))
            .WithInnerError(Error.Validation("Inner.2", "Second inner"))
            .Build();

        _successResult = Result.Success();
        _failureResult = Result.Failure(_errorWithMixedMetadata);
        _successResultOfT = Result.Success(42);
        _failureResultOfT = Result.Failure<int>(_errorWithMixedMetadata);

        _options = new JsonSerializerOptions();
        _options.Converters.Add(new ErrorJsonConverter());
        _options.Converters.Add(new ResultJsonConverter());
        _options.Converters.Add(new ResultOfTJsonConverter<int>());

        _serializedError = JsonSerializer.SerializeToUtf8Bytes(_errorWithMixedMetadata, _options);
        _serializedResult = JsonSerializer.SerializeToUtf8Bytes(_failureResult, _options);
    }

    // ─── Error Serialization ──────────────────────────────────────────────────

    [Benchmark(Baseline = true)]
    public byte[] Serialize_Error_NoMetadata()
        => JsonSerializer.SerializeToUtf8Bytes(_errorNoMetadata, _options);

    [Benchmark]
    public byte[] Serialize_Error_StringMetadata()
        => JsonSerializer.SerializeToUtf8Bytes(_errorWithStringMetadata, _options);

    [Benchmark]
    public byte[] Serialize_Error_MixedMetadata()
        => JsonSerializer.SerializeToUtf8Bytes(_errorWithMixedMetadata, _options);

    [Benchmark]
    public byte[] Serialize_Error_InnerErrors()
        => JsonSerializer.SerializeToUtf8Bytes(_errorWithInnerErrors, _options);

    // ─── Error Deserialization ─────────────────────────────────────────────────

    [Benchmark]
    public Error? Deserialize_Error_MixedMetadata()
        => JsonSerializer.Deserialize<Error>(_serializedError, _options);

    // ─── Result Serialization ─────────────────────────────────────────────────

    [Benchmark]
    public byte[] Serialize_Result_Success()
        => JsonSerializer.SerializeToUtf8Bytes(_successResult, _options);

    [Benchmark]
    public byte[] Serialize_Result_Failure()
        => JsonSerializer.SerializeToUtf8Bytes(_failureResult, _options);

    [Benchmark]
    public byte[] Serialize_ResultOfT_Success()
        => JsonSerializer.SerializeToUtf8Bytes(_successResultOfT, _options);

    [Benchmark]
    public byte[] Serialize_ResultOfT_Failure()
        => JsonSerializer.SerializeToUtf8Bytes(_failureResultOfT, _options);

    // ─── Round-Trip ───────────────────────────────────────────────────────────

    [Benchmark]
    public Result? RoundTrip_Result()
    {
        var bytes = JsonSerializer.SerializeToUtf8Bytes(_failureResult, _options);
        return JsonSerializer.Deserialize<Result>(bytes, _options);
    }
}


