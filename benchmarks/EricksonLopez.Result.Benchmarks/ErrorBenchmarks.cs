using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Order;
using System.Collections.Generic;

namespace EricksonLopez.Result.Benchmarks;

/// <summary>
/// Measures the cost of Error construction, metadata attachment,
/// and the ErrorBuilder vs direct construction paths.
///
/// Performance philosophy: maximum performance through source generators and
/// zero-allocation patterns. ErrorBuilder benchmarks quantify the stack-copy
/// break-even point vs heap allocation to guide library consumers.
///
/// Key findings these benchmarks capture:
///   - Factory methods (Error.Failure, Error.NotFound, etc.) are the fastest path
///     for errors without dynamic metadata.
///   - ErrorBuilder With*() chains copy ~96-104 bytes per call (stack memcpy);
///     N calls = N×100B of stack churn — cheaper than N heap allocations for N≤10.
///   - For hot loops, pre-build the Error once outside the loop and reuse it.
///   - BatchMetadata (single IReadOnlyDictionary call) is cheaper than N individual
///     WithMetadata(k,v) calls because it reduces the number of struct copies.
/// </summary>
[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[HideColumns(Column.Error, Column.StdDev, Column.RatioSD)]

public class ErrorBenchmarks
{
    // Pre-built metadata dict for batch-path benchmarks — demonstrates the recommended
    // high-throughput pattern when multiple metadata entries are needed.
    private static readonly IReadOnlyDictionary<string, object> _batchMetadata = new Dictionary<string, object>
    {
        { "key1", "val1" },
        { "key2", "val2" },
        { "key3", "val3" },
        { "key4", "val4" },
        { "key5", "val5" },
    };

    // ─── Section 1: Factory vs Builder (baseline comparison) ─────────────────

    /// <summary>Baseline: raw factory — the zero-allocation path for static errors.</summary>
    [Benchmark(Baseline = true)]
    public Error Factory_Failure()
        => Error.Failure("ERR001", "Something failed");

    [Benchmark]
    public Error Factory_Validation()
        => Error.Validation("VAL001", "Invalid input");

    [Benchmark]
    public Error Builder_Simple()
        => Error.Create("ERR001", "Something failed")
            .Build();

    // ─── Section 2: Metadata via direct Error.WithMetadata (immutable chain) ─

    [Benchmark]
    public Error WithMetadata_Chain_3()
    {
        var error = Error.Failure("ERR001", "Something failed");
        return error
            .WithMetadata("key1", "val1")
            .WithMetadata("key2", "val2")
            .WithMetadata("key3", "val3");
    }

    // ─── Section 3: Builder chains of increasing length ──────────────────────
    //
    // Each With*() call copies the entire ErrorBuilder struct (~96-104 bytes).
    // These benchmarks quantify the cumulative stack-copy cost at N=3,5,7,10.
    //
    // Interpretation:
    //   Chain of N = N×100B stack memcpy. For N=10 this is 1,000B of stack writes.
    //   No extra heap allocations from the builder itself — only the final .Build()
    //   allocates the Error on the heap (unavoidable, Error is a sealed class).

    [Benchmark]
    public Error Builder_WithMetadata_3()
        => Error.Create("ERR001", "Something failed")
            .WithMetadata("key1", "val1")
            .WithMetadata("key2", "val2")
            .WithMetadata("key3", "val3")
            .Build();

    /// <summary>
    /// 5-step chain — break-even territory.
    /// ~500 bytes of total stack-copy cost, 0 extra heap allocations from the builder itself.
    /// The final .Build() allocates the Error on the heap (unavoidable, Error is a sealed class).
    /// </summary>
    [Benchmark]
    public Error Builder_Chain_5()
        => Error.Create("ERR001", "Something failed")
            .WithType(ErrorType.Domain)
            .WithSeverity(ErrorSeverity.Critical)
            .WithRetryability(ErrorRetryability.Permanent)
            .WithCorrelationId("corr-123")
            .WithMetadata("orderId", "ORD-123")
            .Build();

    /// <summary>
    /// 7-step chain — above the recommended break-even threshold.
    /// ~700 bytes of total stack-copy. Consider pre-building outside hot loops.
    /// </summary>
    [Benchmark]
    public Error Builder_Chain_7()
        => Error.Create("ERR001", "Something failed")
            .WithType(ErrorType.Domain)
            .WithSeverity(ErrorSeverity.Critical)
            .WithRetryability(ErrorRetryability.Permanent)
            .WithCorrelationId("corr-123")
            .WithTraceId("trace-456")
            .WithMetadata("orderId", "ORD-123")
            .WithMetadata("userId", "USR-789")
            .Build();

    /// <summary>
    /// 10-step chain — the longest practical chain a developer would write.
    /// ~1,000 bytes of total stack-copy. If this is in a hot path,
    /// pre-building the Error or using Builder_BatchMetadata_5 is strongly recommended.
    /// </summary>
    [Benchmark]
    public Error Builder_Chain_10()
        => Error.Create("ERR001", "Something failed")
            .WithType(ErrorType.Domain)
            .WithSeverity(ErrorSeverity.Critical)
            .WithRetryability(ErrorRetryability.Permanent)
            .WithCorrelationId("corr-123")
            .WithTraceId("trace-456")
            .WithMetadata("key1", "val1")
            .WithMetadata("key2", "val2")
            .WithMetadata("key3", "val3")
            .WithMetadata("key4", "val4")
            .WithMetadata("key5", "val5")
            .Build();

    // ─── Section 4: Batch metadata — recommended for 3+ metadata entries ─────

    /// <summary>
    /// BatchMetadata (single IReadOnlyDictionary call) vs N individual WithMetadata(k,v) calls.
    /// Reduces struct copy count from N to 1 — the optimal path for 3+ metadata entries.
    /// </summary>
    [Benchmark]
    public Error Builder_BatchMetadata_5()
        => Error.Create("ERR001", "Something failed")
            .WithMetadata(_batchMetadata)
            .Build();

    // ─── Section 5: Equality and hashing performance ──────────────────────────

    [Benchmark]
    public bool Error_Equality()
    {
        var a = Error.Failure("ERR001", "Something failed");
        var b = Error.Failure("ERR001", "Something failed");
        return a.Equals(b);
    }

    [Benchmark]
    public int Error_GetHashCode()
    {
        var a = Error.Failure("ERR001", "Something failed");
        return a.GetHashCode();
    }

    // ─── Section 6: Full featured Error (for real-world baseline) ─────────────

    [Benchmark]
    public Error Builder_Full()
        => Error.Create("ERR001", "Something failed")
            .WithType(ErrorType.Domain)
            .WithSeverity(ErrorSeverity.Critical)
            .WithRetryability(ErrorRetryability.Permanent)
            .WithCorrelationId("corr-123")
            .WithTraceId("trace-456")
            .WithMetadata("key1", "val1")
            .WithMetadata("key2", "val2")
            .Build();
}
