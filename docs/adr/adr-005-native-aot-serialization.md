# ADR-005: NativeAOT & Trimming Serialization Strategy

- **Status**: Accepted
- **Date**: 2026-07-28
- **Authors**: Erickson Lopez

---

## Context

Modern .NET 8 / 10 deployments increasingly target NativeAOT (Ahead-Of-Time compilation) and trimming to achieve sub-millisecond startup times and tiny container images. Traditional reflection-based JSON serializers fail under NativeAOT compilation.

## Decision

We designed `EricksonLopez.Result.Serialization` with NativeAOT-safe `System.Text.Json` custom converters (`ResultJsonConverter`, `ResultOfTJsonConverter<T>`, `ErrorJsonConverter`) and Roslyn Source Generators (`EricksonLopez.Result.Serialization.Generators`) for compile-time converter registration.

## Consequences

### Positive
- Zero reflection warnings when compiling with NativeAOT (`PublishAot=true`).
- Pre-compiled JSON metadata contexts ensure fast, trim-safe payload formatting.

### Negative / Trade-Offs
- NativeAOT scenarios require consumer code to register custom generic `Result<T>` types via source-generated contexts or explicit `ResultOfTJsonConverter<T>` instances.
