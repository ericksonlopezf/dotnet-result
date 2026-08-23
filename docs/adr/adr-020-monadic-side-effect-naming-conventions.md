# ADR-020: Monadic Side-Effect Operator Naming Conventions (`TapOnSuccess`, `TapOnFailure`, `Execute`)

- **Status**: Accepted
- **Date**: 2026-08-23
- **Authors**: Erickson Lopez

---

## Context

In Railway-Oriented Programming and monadic design, side-effect operations (logging, metrics, notifications) are critical for observability and integration.

Early iterations of functional libraries used overloaded names like `Tap`, `TapError`, `OnSuccess`, `OnFailure`, `Switch`, or `Finally`. These created several issues:
1. Ambiguity in IntelliSense: `Tap` vs `TapError` caused developer confusion about whether failure states were bypassed.
2. Inconsistent async naming: `TapAsync`, `MapAsync`, `BindAsync` duplicated standard async method overload practices in .NET.
3. Obsolete methods like `Finally` implied `try-finally` cleanup semantics rather than pipeline inspection.

## Decision

Standardize all side-effect and pipeline execution methods into an explicit, self-documenting taxonomy:

1. **`TapOnSuccess`**: Executes side-effect actions exclusively when the result is in a success state. Returns the original result unchanged.
2. **`TapOnFailure`**: Executes side-effect actions exclusively when the result is in a failure state. Returns the original result unchanged.
3. **`Execute`**: Executes side-effect branching actions for both success (`Action<T>`) and failure (`Action<Error>`) states without returning a value (terminal consumer).
4. **`Inspect`**: Executes an unconditional inspection action receiving the `Result<T>` instance regardless of its outcome state.
5. **No `*Async` suffix for extensions**: Overloads taking async delegates (`Func<Task>`, `Func<ValueTask>`) retain standard names (`Map`, `Bind`, `TapOnSuccess`, `TapOnFailure`, `Recover`) following idiomatic modern C# extension design.

## Consequences

### Positive
- Maximum clarity in IntelliSense and code reviews: intent is immediately obvious.
- Eliminates silent execution of wrong branch side-effects.
- Uniform signatures across synchronous, `Task`, and `ValueTask` pipelines.

### Negative / Trade-Offs
- Slightly longer method names than generic `Tap`, but justified by safety and self-documentation.
