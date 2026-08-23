# ADR-002: Sealed Error Class with Immutable Metadata and Composition-Based Extensibility

- **Status**: Accepted (Updated 2026-08-18 — Parity Audit Review)
- **Date**: 2026-07-28 (last updated: 2026-08-18)
- **Authors**: Erickson Lopez

---

## Context

Domain errors require rich, structured data representation (`Code`, `Description`, `Type`, `Severity`, `Retryability`, `Metadata`, `InnerErrors`, `TraceId`, `CorrelationId`, `DescriptionKey`). In initial design discussions, subclassing `Error` was considered to allow domain-specific error types.

However, in .NET value-like types, non-sealed classes that implement `IEquatable<T>` introduce severe equality pitfalls: derived classes adding domain fields without overriding `Equals` and `GetHashCode` silently break deduplication and lookup in `HashSet<Error>`, `Dictionary<Error, TValue>`, LINQ `.Distinct()`, and `Result.Equals`.

## Decision

We decided to model `Error` as a **`sealed class`** implementing `IEquatable<Error>`, equipped with static factory constructors (`Failure`, `Validation`, `NotFound`, `Conflict`, `Forbidden`, `Unexpected`), fluent struct-based `ErrorBuilder`, and composition-based metadata attachment (`WithMetadata`).

Extensibility is achieved entirely through **composition** rather than inheritance:
- Arbitrary key-value payloads via `WithMetadata(string, object)` / `ImmutableDictionary<string, object>`.
- Compound hierarchical errors via `InnerErrors` (`ImmutableArray<Error>`).
- Operational semantics via `ErrorType`, `ErrorSeverity`, and `ErrorRetryability`.
- Distributed tracing and localization via `TraceId`, `CorrelationId`, and `DescriptionKey`.

## Consequences

### Positive
- **Guaranteed Equality Semantics**: `IEquatable<Error>` comparisons and hash codes are strictly consistent and safe across all hash-based collections (`HashSet<Error>`, `Dictionary<Error, T>`).
- **Rich Error Taxonomy**: Comprehensive 10-dimensional semantic error model without class hierarchy explosion.
- **Zero-Alloc Construction via Struct Builder**: `ErrorBuilder` is a mutable struct that builds immutable `Error` instances without intermediary heap allocations.
- **Analyzers Protection**: Roslyn analyzer `RESULT007` protects callers when using `Error` in hash sets and dictionaries.

### Negative / Trade-Offs
- Subclassing is prevented by design. Callers with domain-specific requirements must use `WithMetadata` or dedicated domain factory classes (`MyDomainErrors.OrderNotFound(...)`), which is the recommended pattern in modern .NET DDD architectures.
- `Error` is a reference type; failure outcomes allocate on the heap. This is intentional as failures represent exceptional/non-happy paths in business workflows.
