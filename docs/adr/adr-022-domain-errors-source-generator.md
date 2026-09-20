# ADR-022: Domain Errors Incremental Source Generator

## Status
Accepted

## Date
2026-09-08

## Context
Defining domain error factory methods manually in large domain models is repetitive and can lead to discrepancies between error codes, severity levels, localization keys, and parameter formats. While runtime reflection or configuration loading is common in enterprise systems, it violates the ecosystem's Native AOT and zero-allocation invariants.

## Decision
We implemented an incremental Roslyn source generator in `EricksonLopez.Result.DomainErrors.Generators`:
1. It monitors `AdditionalFiles` matching `*.errors.json`.
2. It parses schema files with zero third-party dependencies using a dedicated, high-efficiency JSON parser (`DomainErrorJsonParser`).
3. It emits compile-time `public static partial class` definitions with parameterized factory methods constructing immutable `Error` instances via `Error.Create(...)`.
4. It fully integrates with Native AOT, generates zero runtime overhead, and supports localization keys via `DescriptionKey`.

## Consequences
### Positive
- Strongly-typed, compile-time verified domain error factories.
- IDE auto-completion and documentation for all domain errors.
- Unconditional Native AOT compatibility with zero reflection.

### Negative / Trade-offs
- Consumers must configure `AdditionalFiles` in their project file to include `*.errors.json`.
