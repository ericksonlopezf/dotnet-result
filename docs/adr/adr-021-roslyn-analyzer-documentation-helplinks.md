# ADR-021: Roslyn Diagnostic Analyzer Documentation and HelpLinkUri Strategy

- **Status**: Accepted
- **Date**: 2026-08-23
- **Authors**: Erickson Lopez

---

## Context

The `EricksonLopez.Result.Analyzers` package provides build-time safety and performance governance across 11 diagnostic rules (`RESULT001` through `RESULT012` and `RESULT_OTEL_001`).

Each analyzer rule defines a `DiagnosticDescriptor` containing an external documentation URI (`helpLinkUri`) pointing to detailed explanations, rationale, and resolution examples:
- `https://github.com/ericksonlopezf/dotnet-result/blob/main/docs/analyzers.md`
- `https://github.com/ericksonlopezf/dotnet-result/blob/main/docs/error-builder.md`
- `https://github.com/ericksonlopezf/dotnet-result/blob/main/docs/performance.md`
- `https://github.com/ericksonlopezf/dotnet-result/blob/main/docs/serialization.md`

If these documentation files are missing from the repository, developers clicking compiler error links in IDEs (Visual Studio, Rider, VS Code) receive 404 HTTP errors.

## Decision

1. **Establish Dedicated Markdown Documentation**:
   - `docs/analyzers.md`: General analyzer catalog and rules `RESULT007`, `RESULT008`, `RESULT009`, `RESULT010`, `RESULT012`, `RESULT_OTEL_001`.
   - `docs/error-builder.md`: Detailed guide for `ErrorBuilder` immutability, chaining rules (`RESULT003`, `RESULT005`, `RESULT006`), and code fixes.
   - `docs/performance.md`: Detailed reference for struct sizing (`RESULT001` with 32-byte threshold) and zero-allocation closure elimination (`RESULT004` with `TState` overloads).
   - `docs/serialization.md`: Native AOT serialization guide and `RESULT_GEN_001` diagnostics.
2. **Synchronize HelpLink URLs**:
   - All `DiagnosticDescriptor` instances across all analyzer classes must target valid, verified Markdown files within the repository.

## Consequences

### Positive
- Zero broken help links for any analyzer diagnostic surfaced during compilation.
- Rich contextual developer guidance in all IDEs with code fixes and mitigation patterns.
- High documentation cohesion and discoverability.
