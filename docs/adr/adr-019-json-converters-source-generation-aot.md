# ADR-019: System.Text.Json Converters and Source Generator Architecture for Native AOT

- **Status**: Accepted
- **Date**: 2026-08-23
- **Authors**: Erickson Lopez

---

## Context

Targeting Native AOT compilation (`PublishAot=true`) in modern .NET requires zero dynamic reflection in JSON serialization paths. 

`System.Text.Json` custom polymorphic converter factories (such as `JsonConverterFactory`) rely on `Type.MakeGenericType()` at runtime, which triggers trim and AOT compiler warnings (`IL2026: RequiresUnreferencedCode`, `IL3050: RequiresDynamicCode`).

## Decision

1. **Replace polymorphic factory with explicit converters**:
   - `ResultJsonConverter`: Dedicated converter for non-generic `Result`.
   - `ResultOfTJsonConverter<T>`: Generic converter instantiated with explicit concrete type arguments.
   - `ErrorJsonConverter`: Dedicated converter for the rich `Error` model.
2. **Roslyn Source Generator**:
   - `EricksonLopez.Result.Serialization.Generators` analyzes consumer types and generates AOT-safe converter registrations and `JsonSerializerContext` bindings at compile time.
   - Diagnostic rule `RESULT_GEN_001` warns when `[JsonSerializable(typeof(Result))]` is declared without generic arguments, guiding developers to source-generated or explicit converter setup.

## Consequences

### Positive
- Fully certified for Native AOT with 0 trim warnings.
- Reflection-free serialization and deserialization in high-throughput environments.
- Compile-time verification of serializable Result payloads.

### Negative / Trade-Offs
- Consumers using custom generic types in Native AOT must register `ResultOfTJsonConverter<T>` explicitly or configure their source-generated `JsonSerializerContext`.
