# ADR-009: FluentValidation Integration as Separate Package

## Status
Accepted

## Date
2026-07-28

- **Status**: Accepted
- **Date**: 2026-07-28
- **Authors**: Erickson Lopez

---

## Context

Applications frequently use FluentValidation for input validation. Converting `FluentValidation.ValidationResult` into `Result` failures manually is repetitive and error-prone, requiring developers to map validation failures, error codes, severity levels, and metadata consistently across the codebase.

## Decision

We created a dedicated companion package `EricksonLopez.Result.FluentValidation` that provides:

1. `ToValidationResult()` / `ToValidationResult<T>()` extension methods on `ValidationResult`
2. `Validate()` / `ValidateToResult()` extensions directly on `IValidator<T>`
3. `EnsureValid()` pipeline operator for composing validation within `Result<T>` chains
4. Async variants for all operations
5. Automatic mapping of FluentValidation `Severity` to `ErrorSeverity`
6. Structured metadata per failure (`propertyName`, `attemptedValue`, placeholder values)

## Consequences

### Positive
- Eliminates boilerplate FluentValidation → Result conversion code.
- Each `ValidationFailure` is mapped to a structured `Error` with `ErrorType.Validation`, preserving the full validation context as immutable metadata.
- `EnsureValid()` integrates validation seamlessly into monadic pipelines.
- AOT-compatible — no reflection used in the library.

### Negative / Trade-Offs
- Introduces a dependency on `FluentValidation` (updated to `12.1.1` in v2.0.0; originally `11.11.0`). Applications not using FluentValidation should not reference this package.

## Addendum (v2.0.0 Update)
- Method `ValidationResult.ToResult()` was renamed to `ToValidationResult()` in v2.0.0 to eliminate ambiguity with monadic conversion methods.
- The `FluentValidation` dependency was upgraded to `12.1.1` (BC-006).

## Addendum (v3.0.0 Update)
- In v3.0.0, `ValidationResult.ToValidationResult(bool aggregate = true)` defaults to cumulative multi-error aggregation (`aggregate = true`), bundling multiple validation failures into a single root `Error` containing structured `InnerErrors`. Callers requiring only the primary failure must pass `aggregate: false` explicitly (BC-U03).
