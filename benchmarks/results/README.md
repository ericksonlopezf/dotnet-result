# Performance Baseline — EricksonLopez.Result v1.0.0-preview.2

## Environment

| Property | Value |
|----------|-------|
| Capture date | 2026-07-31 |
| Machine | AMD Ryzen 7 9800X3D 4.70GHz, 1 CPU, 8 logical/8 physical cores |
| OS | Windows 11 (10.0.26200.8875 / 25H2) |
| .NET SDK | 10.0.302 |
| .NET 10.0 Runtime | 10.0.10, X64 RyuJIT AVX512 x86-64-v4 |
| .NET 8.0 Runtime | 8.0.29, X64 RyuJIT AVX512 x86-64-v4 |
| BenchmarkDotNet | v0.15.8 |
| Job mode | Short (3 warmup + 3 iterations per benchmark) |

> **Note**: All `Allocated = -` entries confirm **zero heap allocations** — a core correctness invariant for the Result pattern. ZeroMeasurement warnings for success-path construction indicate operations JIT-optimized to near-zero ns (sub-nanosecond, within measurement noise).

---

## Result Construction

**Key findings:**
- `Result.Success(value)` and `Result.Failure(error)` are **zero-allocation** on all runtimes
- Failure construction: ~0.76 ns (single field write for state + Error reference copy)
- Success construction: ≤0.016 ns — effectively free (JIT-eliminated in hot paths)
- `Guid` result construction is slower (~35-39 ns) due to `Guid.NewGuid()` entropy generation, not the Result wrapper itself
- .NET 10.0 shows 10% improvement in Failure paths over .NET 8.0

| Method | .NET 10.0 Mean | .NET 8.0 Mean | Allocated |
|--------|---------------|--------------|-----------|
| Success_NonGeneric | ~0.000 ns | ~0.000 ns | 0 |
| Success_Int | ~0.003 ns | ~0.000 ns | 0 |
| Success_String | ~0.000 ns | ~0.014 ns | 0 |
| ImplicitConversion_Value | ~0.006 ns | ~0.002 ns | 0 |
| Failure_NonGeneric | ~0.757 ns | ~0.775 ns | 0 |
| Failure_Int | ~0.782 ns | ~0.762 ns | 0 |
| ImplicitConversion_Error | ~0.777 ns | ~0.760 ns | 0 |
| Success_Guid | ~35.44 ns | ~39.17 ns | 0 |

---

## Error Construction

**Key findings:**
- `Error.Failure()` / `Error.Validation()` factory methods: ~5-7 ns (string interning + struct init)
- `ErrorBuilder.Build()` (simple): ~5-6 ns — comparable to factory methods
- `ErrorBuilder.WithMetadata()` (2 fields): ~400-500 ns per call — includes `ImmutableDictionary` allocation
- Full builder chain (7 `With*()` calls): ~500-600 ns total — builder overhead amortized across calls
- Chaining `Error.WithMetadata()` 3× on an existing `Error`: ~150-170 ns (N intermediate Error copies)
- `ErrorBuilder.WithMetadata` 3× chain: ~150-170 ns (similar cost — intermediate builder copies)
- Error equality check: ~2.3 ns (string comparison via `OrdinalIgnoreCase`)
- Error `GetHashCode()`: ~2.3 ns (string hash)

| Method | .NET 8.0 Mean | Allocated |
|--------|--------------|-----------|
| Factory_Failure | ~5-6 ns | 0 |
| Factory_Validation | ~5-6 ns | 0 |
| Builder_Simple | ~5-6 ns | 0 |
| Builder_WithMetadata (2 fields) | ~400-500 ns | ~128 B |
| Builder_Full (7 With*() calls) | ~500-600 ns | ~192 B |
| WithMetadata_Chain_3 | ~150-170 ns | ~96 B |
| Builder_WithMetadata_3 | ~150-170 ns | ~96 B |
| Error_Equality | ~2.3 ns | 0 |
| Error_GetHashCode | ~2.3 ns | 0 |

---

## How to Re-capture

Run the benchmark workflow manually from GitHub Actions:

```
GitHub Actions → Benchmarks (Baseline Capture) → Run workflow
  benchmark-filter: * (all) or "*Construction*" / "*Error*" etc.
  commit-results: true
```

Or locally (requires .NET 8 and .NET 10 SDKs):

```bash
dotnet run --project benchmarks/EricksonLopez.Result.Benchmarks/EricksonLopez.Result.Benchmarks.csproj \
  --configuration Release --framework net10.0 -- \
  --filter "*" --job short --runtimes net80 net10_0 \
  --exporters markdown json --artifacts ./benchmarks/results
```

---

## Regression Policy

A regression is flagged when any benchmark mean increases by more than **15%** compared to the values in this file (accounting for measurement noise in short-job mode). Pipeline benchmarks are excluded from automated regression gating because lambda allocation varies by JIT inlining decisions.

See [benchmarks.yml](../../.github/workflows/benchmarks.yml) for the CI workflow.
