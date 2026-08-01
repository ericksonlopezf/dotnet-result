# ADR-001: Readonly Struct Result Implementation

- **Status**: Accepted
- **Date**: 2026-07-28
- **Authors**: Erickson Lopez

---

## Context

In high-performance .NET applications, using reference types (`class` or `record`) for the `Result` envelope forces a heap allocation for every method call returning a result. In high-throughput microservices or domain logic loops processing thousands of items per second, this creates garbage collection (GC) pressure.

## Decision

We decided to implement `Result` and `Result<TValue>` as **`readonly struct`** value types.

## Consequences

### Positive
- **Zero Heap Allocation for Envelope**: Returning `Result.Success()` or `Result.Success(value)` incurs zero heap allocation.
- **Cache-Friendly**: Value types are stored contiguously on stack frames or CPU registers.
- **Immutability Guaranteed**: The `readonly` modifier guarantees thread safety and prevents accidental mutation.

### Negative / Trade-Offs
- `default(Result)` results in an uninitialized struct. We added explicit `IsUninitialized` state checking and guard checks (`ThrowIfUninitialized()`) to prevent accessing invalid state.
