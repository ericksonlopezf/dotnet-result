# Project Roadmap

This document outlines the vision, current milestone deliverables, and future feature plans for `EricksonLopez.Result` and its ecosystem packages.

---

## 🎯 Performance Philosophy

This library's core design principle is **maximum performance through efficient alternatives to reflection, with source generators as the primary tool**.

Every design decision prioritizes:
1. **Source generators over reflection** — Version constants, JSON converters, and type registrations are emitted at compile time, not computed at runtime. This provides zero runtime overhead and unconditional NativeAOT/trimming compatibility.
2. **Zero-allocation patterns** — `TState` overloads, `in`-parameter extensions, `ArrayPool<T>`, and `ImmutableArray<T>` eliminate heap pressure in hot paths.
3. **Struct semantics** — `Result` and `Result<TValue>` are `readonly struct` types with `StructLayout.Auto` to minimize stack footprint and avoid heap allocation on the success path.
4. **Explicit performance documentation** — Every API surface with performance implications is documented in XML docs and benchmarks (`benchmarks/EricksonLopez.Result.Benchmarks`).

---

## 📌 Phase 1: Core Foundation & Ecosystem (v1.0.x)

- ✅ **Core Struct Envelope** — Readonly struct implementation for `Result` and `Result<TValue>` with zero happy-path heap allocation.
- ✅ **Closure-Free `TState` Operators** — State-passing overloads for `Map`, `Bind`, `Tap`, `Match`, `Switch`, `Ensure`, `Recover`.
- ✅ **`in`-Parameter Sync Extensions** — `ResultSyncExtensions` provides `Map`, `Bind`, `Ensure`, `Match`, `TryGetValue`, and `GetValueOrDefault` as `in`-parameter extension methods, eliminating struct copies for value types.
- ✅ **Rich Error Taxonomy** — `ErrorType`, `ErrorSeverity`, `ErrorRetryability`, lazy `TraceId` capture, `CorrelationId`, localized keys (`DescriptionKey`), and immutable `Metadata`.
- ✅ **ASP.NET Core RFC 9457 Integration** — `ToHttpResult()`, `ResultEndpointFilter`, and `ResultHttpOptions`.
- ✅ **OpenTelemetry & Metrics** — Native `ActivitySource` tracing (`RecordResult`) and `System.Diagnostics.Metrics` counters (`ResultMetrics`).
- ✅ **System.Text.Json Serialization** — Custom converters and NativeAOT trim-safe `JsonSerializerContext`.
- ✅ **Fluent Unit Test Assertions** — `EricksonLopez.Result.Testing` library with declarative assertion syntax.
- ✅ **FluentValidation Integration** — `EricksonLopez.Result.FluentValidation` with `ToResult()`, `EnsureValid()`, and severity mapping.
- ✅ **MediatR Pipeline Behavior** — `EricksonLopez.Result.MediatR` with `ResultExceptionBehavior<TRequest, TResponse>` and `AddResultExceptionBehavior()`.
- ✅ **Roslyn Analyzers** — `EricksonLopez.Result.Analyzers` bundled with Core: `RESULT001`–`RESULT012`, `RESULT_OTEL_001`.
- ✅ **Serialization Source Generator** — `EricksonLopez.Result.Serialization.Generators` for AOT-compatible `ResultOfTJsonConverter<T>` generation and compile-time version constants.

---

## 🚀 Phase 2: Tooling & Parity Enhancements (v1.0.x Deliveries)

- ✅ **Cumulative Validation (`Result.ValidateAll`)** — Zero-allocation, span-based validator evaluating multiple rules and aggregating compound failures.
- ✅ **Option Type Package (`EricksonLopez.Result.Maybe`)** — High-performance struct-based `Maybe<T>` option type with monadic operators and seamless `Result` interop.
- ✅ **Generic Strongly-Typed Error (`EricksonLopez.Result.Generic`)** — `Result<TValue, TError>` for strict DDD domain models with compile-time error types.
- ✅ **OpenAPI Metadata Extensions (`EricksonLopez.Result.OpenApi`)** — `ProducesResult<T>()` and `ProducesResultProblemDetails()` for automated Minimal API OpenAPI schemas.
- ✅ **NUnit Testing Integration (`EricksonLopez.Result.Testing.NUnit`)** — Assertion failures surface as NUnit's `AssertionException`.
- ✅ **xUnit Testing Integration (`EricksonLopez.Result.Testing.XUnit`)** — Assertion failures surface as xUnit's `XunitException`.
- 📋 **Additional Roslyn Analyzers**:
  - Diagnostic for unhandled `Result` return values (must be assigned or matched).
  - Diagnostic for unsafe `.Value` or `.Error` property accesses without guard checks.
- 📋 **Source Generator for Domain Errors**:
  - Code generator producing strongly-typed `Error` factory classes from JSON or YAML definitions.
- 📋 **Entity Framework Core Integration (`EricksonLopez.Result.EntityFrameworkCore`)**:
  - Extension helpers for converting EF Core operations directly into `Result<T>` with automatic exception handling for concurrency/database errors.

---

## 🌌 Phase 3: Ecosystem Expansion (v2.0.x)

- 📋 **Polly Resilience Integration**:
  - Extension methods bridging `Polly` resilience pipelines directly with `Result` retryability policies.
- 📋 **MassTransit Pipeline Integration**:
  - Pre-built pipeline behaviors for converting MassTransit consumer results into structured telemetry and ProblemDetails.

---

> **Note:** This roadmap is a living document. Community input is encouraged — share your suggestions in [GitHub Discussions](https://github.com/ericksonlopezf/dotnet-result/discussions).
