# ADR-002: Extensible Error Class with Immutable Metadata

- **Status**: Accepted
- **Date**: 2026-07-28
- **Authors**: Erickson Lopez

---

## Context

Domain errors require structured data representation (`Code`, `Description`, `Type`, `Severity`, `Retryability`, `Metadata`) while retaining support for specialized domain subclasses without breaking serialization or HTTP mapping.

## Decision

We decided to model `Error` as an extensible `class` (unsealed) implementing `IEquatable<Error>`, equipped with static factory constructors (`Validation`, `NotFound`, `Conflict`, `Forbidden`, etc.) and fluent builder operators.

## Consequences

### Positive
- Unified, consistent error taxonomy (`ErrorType`, `ErrorSeverity`, `ErrorRetryability`).
- Allows subclassing for specialized domain errors (e.g. `PaymentError` with typed fields).
- Supports hierarchical inner errors (`InnerErrors`) for complex multi-field validation.

### Negative / Trade-Offs
- `Error` is a reference type, meaning failure paths allocate on the heap. This is acceptable since failures represent exceptional/non-happy paths in business workflows.
