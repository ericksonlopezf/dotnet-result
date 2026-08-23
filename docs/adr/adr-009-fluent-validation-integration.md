# ADR-009: FluentValidation Integration as Separate Package

- **Status**: Accepted
- **Date**: 2026-07-28
- **Authors**: Erickson Lopez

---

## Context

Applications frequently use FluentValidation for input validation. Converting `FluentValidation.ValidationResult` into `Result` failures manually is repetitive and error-prone, requiring developers to map validation failures, error codes, severity levels, and metadata consistently across the codebase.

## Decision

We created a dedicated companion package `EricksonLopez.Result.FluentValidation` that provides:

1. `ToResult()` / `ToResult<T>()` extension methods on `ValidationResult`
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
- Introduces a dependency on `FluentValidation 11.11.0`. Applications not using FluentValidation should not reference this package.
