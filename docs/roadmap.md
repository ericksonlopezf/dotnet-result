# Project Roadmap

This document outlines the architectural philosophy, shipped milestones, and future ecosystem expansion plans for `EricksonLopez.Result` and its ecosystem packages.

---

## 🎯 Performance Philosophy

This library's core design principle is **maximum performance through efficient alternatives to reflection, with source generators as the primary tool**.

Every design decision prioritizes:
1. **Source generators over reflection** — Version constants, JSON converters, and type registrations are emitted at compile time, not computed at runtime. This provides zero runtime overhead and unconditional NativeAOT/trimming compatibility.
2. **Zero-allocation patterns** — `TState` overloads, `in`-parameter extensions, `ArrayPool<T>`, and `ImmutableArray<T>` eliminate heap pressure in hot paths.
3. **Struct semantics** — `Result` and `Result<TValue>` are `readonly struct` types with `StructLayout.Auto` to minimize stack footprint and avoid heap allocation on the success path.
4. **Explicit performance documentation** — Every API surface with performance implications is documented in XML docs and benchmarks (`benchmarks/EricksonLopez.Result.Benchmarks`).

---

## 📌 Phase 1: Core Foundation & Ecosystem (v1.0.x — Shipped)

- ✅ **Core Struct Envelope** — Readonly struct implementation for `Result` and `Result<TValue>` with zero happy-path heap allocation.
- ✅ **Closure-Free `TState` Operators** — State-passing overloads for `Map`, `Bind`, `Tap`, `Match`, `Ensure`, `Recover`.
- ✅ **`in`-Parameter Sync Extensions** — `ResultSyncExtensions` provides `Map`, `Bind`, `Ensure`, `Match`, `TryGetValue`, and `GetValueOrDefault` as `in`-parameter extension methods, eliminating struct copies for value types.
- ✅ **Rich Error Taxonomy** — `ErrorType`, `ErrorSeverity`, `ErrorRetryability`, lazy `TraceId` capture, `CorrelationId`, localized keys (`DescriptionKey`), and immutable `Metadata`.
- ✅ **ASP.NET Core RFC 9457 Integration** — `ToHttpResult()`, `ResultEndpointFilter`, and `ResultHttpOptions`.
- ✅ **OpenTelemetry & Metrics** — Native `ActivitySource` tracing (`RecordResult`) and `System.Diagnostics.Metrics` counters (`ResultMetrics`).
- ✅ **System.Text.Json Serialization** — Custom converters and NativeAOT trim-safe `JsonSerializerContext`.
- ✅ **Fluent Unit Test Assertions** — `EricksonLopez.Result.Testing` library with declarative assertion syntax.
- ✅ **FluentValidation Integration** — `EricksonLopez.Result.FluentValidation` with `ToValidationResult()`, `EnsureValid()`, and severity mapping.
- ✅ **MediatR Pipeline Behavior** — `EricksonLopez.Result.MediatR` with `ResultExceptionBehavior<TRequest, TResponse>` and `AddResultExceptionBehavior()`.
- ✅ **Roslyn Analyzers** — `EricksonLopez.Result.Analyzers` bundled with Core: `RESULT001`–`RESULT012`, `RESULT_OTEL_001`.
- ✅ **Serialization Source Generator** — `EricksonLopez.Result.Serialization.Generators` for AOT-compatible `ResultOfTJsonConverter<T>` generation and compile-time version constants.

---

## 🚀 Phase 2: Tooling & Parity Deliveries (v2.0.0 — Shipped)

- ✅ **Cumulative Validation (`Result.ValidateAll`)** — Zero-allocation, span-based validator evaluating multiple rules and aggregating compound failures.
- ✅ **Option Type Package (`EricksonLopez.Result.Maybe`)** — High-performance struct-based `Maybe<T>` option type with monadic operators and seamless `Result` interop.
- ✅ **Generic Strongly-Typed Error (`EricksonLopez.Result.Generic`)** — `Result<TValue, TError>` for strict DDD domain models with compile-time error types.
- ✅ **OpenAPI Metadata Extensions (`EricksonLopez.Result.OpenApi`)** — `ProducesResult<T>()` and `ProducesResultProblemDetails()` for automated Minimal API OpenAPI schemas.
- ✅ **NUnit Testing Integration (`EricksonLopez.Result.Testing.NUnit`)** — Assertion failures surface as NUnit's `AssertionException`.
- ✅ **xUnit Testing Integration (`EricksonLopez.Result.Testing.XUnit`)** — Assertion failures surface as xUnit's `XunitException`.
- ✅ **Central Package Management (CPM)** — Solution-wide dependency pinning via `Directory.Packages.props`.

---

## ⚡ Phase 3: Monadic Parity & Wire-Format Modernization (v3.0.0 — Target Release)

- ✅ **`Result<TValue, TError>` Full Monadic Pipeline** — Parity combinators (`Ensure`, `Recover`, `Inspect`, `TapOnSuccess`, `TapOnFailure`, `DiscardValue`, `Match`, `Execute`), interface implementations (`IResultOutcome`, `IEquatable`), and explicit comparison operators.
- ✅ **Polymorphic & Wire-Format Serialization** — Standardized type discriminator properties and type safety across System.Text.Json serializers (`ErrorJsonConverter`, `ResultJsonConverter`).
- ✅ **FluentValidation Strict Multi-Error Aggregation** — Default cumulative failure validation (`aggregate = true`) and strict validation error propagation.
- ✅ **Roslyn Condition Safety Analyzer (`RESULT013`)** — Compile-time guard and code fix detecting and preventing unsafe implicit boolean conversions in conditional expressions.
- ✅ **ActivitySource Canonical Trace Naming** — Standardized OpenTelemetry trace activity names for `TraceOutcome`.

---

## 🌌 Phase 4: Ecosystem Expansion Deliveries (v3.0.0 — Shipped)

- ✅ **Source Generator for Domain Errors (`EricksonLopez.Result.DomainErrors.Generators`)** — Incremental Roslyn source generator producing strongly-typed static `Error` factory classes from `*.errors.json` schemas.
- ✅ **Entity Framework Core Integration (`EricksonLopez.Result.EntityFrameworkCore`)** — Persistence extension helpers mapping `DbUpdateConcurrencyException`, `DbUpdateException`, and timeouts to domain `Result<T>` envelopes.
- ✅ **Polly Resilience Integration (`EricksonLopez.Result.Polly`)** — Direct integration with Polly v8 `ResiliencePipeline`, evaluating `ErrorRetryability.Transient` without exception allocations.
- ✅ **MassTransit Pipeline Integration (`EricksonLopez.Result.MassTransit`)** — Structured `ResultFault` message contract and consume filter middleware for enterprise message bus architectures.

---

## 🔮 Phase 5: Distributed Horizons & Protocol Bridges (v3.0.0 — Shipped)

- ✅ **Dapr Distributed State & PubSub Extensions (`EricksonLopez.Result.Dapr`)**:
  - Direct integration bridging Dapr state store concurrency tags (ETags), bulk operations, and pub/sub message binding directly into Result envelopes.
- ✅ **gRPC Status Code Mapping & Interceptors (`EricksonLopez.Result.Grpc`)**:
  - Server interceptors (`ResultServerInterceptor`) and extensions translating domain `ErrorType` classifications into canonical gRPC status codes (`StatusCode.NotFound`, `StatusCode.PermissionDenied`, `StatusCode.InvalidArgument`, etc.) with rich metadata trailers.

---

> **Note:** This roadmap is a living document. Community input is encouraged — share your suggestions in [GitHub Discussions](https://github.com/ericksonlopezf/dotnet-result/discussions).
