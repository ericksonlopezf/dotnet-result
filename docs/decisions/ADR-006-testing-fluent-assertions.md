# ADR-006: Dedicated Unit Testing Fluent Assertions Package

- **Status**: Accepted
- **Date**: 2026-07-28
- **Authors**: Erickson Lopez

---

## Context

Testing `Result` outcomes in unit tests can become repetitive and verbose when relying solely on generic test framework assertions (e.g. `Assert.True(result.IsSuccess)`).

## Decision

We created `EricksonLopez.Result.Testing`, a lightweight, framework-agnostic fluent testing assertion library.

## Consequences

### Positive
- Declarative assertion syntax (`result.ShouldBeSuccess()`, `result.ShouldHaveError("Code")`).
- Framework-agnostic (works with xUnit, NUnit, MSTest).
- Detailed exception diagnostics via `ResultAssertionException`.
