# ADR-011: Roslyn Diagnostic Analyzers Package

- **Status**: Accepted
- **Date**: 2026-07-28
- **Authors**: Erickson Lopez

---

## Context

Several categories of developer mistakes with the Result pattern are only detectable at compile time, not at runtime:

1. Using `Result<T>` with large struct types (>64 bytes) causes excessive copying across pipeline operations since `Result<T>` is a `readonly struct` that copies by value.
2. Subclassing `Error` with additional fields without overriding `Equals`/`GetHashCode` silently breaks value equality semantics.
3. Discarding the return value of `ErrorBuilder.With*()` methods loses the mutated struct copy, since `ErrorBuilder` is a mutable struct.

## Decision

We created `EricksonLopez.Result.Analyzers` as a Roslyn diagnostic analyzer project targeting `netstandard2.0` with three analyzers:

| ID | Category | Severity | Rule |
|---|---|---|---|
| `RESULT001` | Performance | Warning | Large struct in `Result<T>` (estimated >64 bytes) |

| `RESULT003` | Usage | Warning | `ErrorBuilder.With*()` return value discarded |

The analyzers are bundled with the core `EricksonLopez.Result` package via:
```xml
<ProjectReference Include="..\EricksonLopez.Result.Analyzers\EricksonLopez.Result.Analyzers.csproj"
                  OutputItemType="Analyzer"
                  ReferenceOutputAssembly="false" />
```

The Analyzers project itself is `<IsPackable>false</IsPackable>` — it is not a standalone NuGet package.

## Consequences

### Positive
- Catches performance issues (large structs), design violations (missing equality overrides), and usage bugs (lost struct mutations) at compile time.
- Zero runtime overhead — analyzers run only during compilation.
- Automatically available to all consumers of `EricksonLopez.Result`.

### Negative / Trade-Offs
- Targets `netstandard2.0` with `ImplicitUsings=disable` to maximize Roslyn host compatibility, requiring explicit using directives.
- `TreatWarningsAsErrors` is set to `false` to avoid blocking the build for analyzer-internal warnings.
