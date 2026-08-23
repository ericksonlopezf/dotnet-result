# ADR-011: Roslyn Diagnostic Analyzers Package

- **Status**: Accepted
- **Date**: 2026-07-28
- **Authors**: Erickson Lopez

---

## Context

Several categories of developer mistakes with the Result pattern are only detectable at compile time, not at runtime:

1. Using `Result<T>` with struct types that cause the total struct size to exceed 32 bytes causes excessive copying across pipeline operations since `Result<T>` is a `readonly struct` that copies by value.
2. Discarding the return value of `ErrorBuilder.With*()` methods loses the mutated struct copy, since `ErrorBuilder` is an immutable `readonly struct`.
3. Capturing variables inside lambda expressions in hot-path Result pipelines allocates compiler display classes on the GC heap.
4. Using collections with `Error` without `ErrorEqualityComparer.Strict` silently deduplicates errors with distinct trace IDs or metadata.

## Decision

We created `EricksonLopez.Result.Analyzers` as a Roslyn diagnostic analyzer project targeting `netstandard2.0` with 11 diagnostic rules and code fixes:

| ID | Category | Severity | Rule |
|---|---|---|---|
| `RESULT001` | Performance | Warning | Large struct in `Result<T>` (struct size >32 bytes) |
| `RESULT003` | Usage | **Error** | `ErrorBuilder.With*()` return value discarded (CodeFix available) |
| `RESULT004` | Performance | Warning | Lambda captures outer variable in Result pipeline (CodeFix available) |
| `RESULT005` | Performance | Warning | `Error.WithMetadata()` chained 3+ times consecutively |
| `RESULT006` | Performance | Warning | `ErrorBuilder.WithInnerError()` chained 2+ times consecutively |
| `RESULT007` | Reliability | Warning | Missing `ErrorEqualityComparer.Strict` in collection deduplication |
| `RESULT008` | Usage | Warning | `ResultEndpointFilter` used without `.Produces<T>()` |
| `RESULT009` | Security | Warning | `IncludeDescription = true` set without environment guard |
| `RESULT010` | Security | Warning | `Exception.Message` used in `ResultExceptionBehavior` |
| `RESULT012` | Usage | Warning | Method returns `default(Result)` or `default(Result<T>)` |
| `RESULT_OTEL_001` | Observability | Info | `TraceOutcome()` called without `ResultMetrics` registered |

The analyzers are bundled with the core `EricksonLopez.Result` package and also published as a standalone NuGet package (`EricksonLopez.Result.Analyzers`).

## Consequences

### Positive
- Catches performance issues (large structs, closures), design violations, and usage bugs (lost struct mutations) at compile time.
- Zero runtime overhead — analyzers run only during compilation.
- Automatically available to all consumers of `EricksonLopez.Result`.

### Negative / Trade-Offs
- Targets `netstandard2.0` with `ImplicitUsings=disable` to maximize Roslyn host compatibility, requiring explicit using directives.
- Analyzer project sets `TreatWarningsAsErrors=false` internally for Roslyn-specific build rules.
