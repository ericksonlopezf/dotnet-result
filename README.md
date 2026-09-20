# EricksonLopez.Result

High-performance, struct-based, enterprise-grade Result Pattern and Railway-Oriented Programming ecosystem for modern .NET.

[![CI](https://img.shields.io/github/actions/workflow/status/ericksonlopezf/dotnet-result/ci.yml?branch=main&style=for-the-badge&logo=githubactions&logoColor=white&label=CI)](https://github.com/ericksonlopezf/dotnet-result/actions)
[![Coverage](https://img.shields.io/codecov/c/github/ericksonlopezf/dotnet-result?style=for-the-badge&logo=codecov&logoColor=white)](https://codecov.io/gh/ericksonlopezf/dotnet-result)
[![Quality Gate](https://img.shields.io/sonar/quality_gate/ericksonlopezf_dotnet-result?server=https%3A%2F%2Fsonarcloud.io&style=for-the-badge&logo=sonarcloud&logoColor=white)](https://sonarcloud.io/summary/new_code?id=ericksonlopezf_dotnet-result)
[![Mutation Score](https://img.shields.io/badge/Mutation_Score-%E2%89%A599%25-brightgreen?style=for-the-badge&logo=stryker&logoColor=white)](https://github.com/ericksonlopezf/dotnet-result/blob/main/docs/mutation-score.md)
[![NuGet](https://img.shields.io/nuget/v/EricksonLopez.Result?style=for-the-badge&logo=nuget&logoColor=white&color=512BD4)](https://www.nuget.org/packages/EricksonLopez.Result)
[![NuGet Downloads](https://img.shields.io/nuget/dt/EricksonLopez.Result?style=for-the-badge&logo=nuget&logoColor=white&color=004880)](https://www.nuget.org/packages/EricksonLopez.Result)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg?style=for-the-badge)](https://github.com/ericksonlopezf/dotnet-result/blob/main/LICENSE)
[![.NET](https://img.shields.io/badge/.NET_8_%7C_9_%7C_10-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)](https://dotnet.microsoft.com)
[![NativeAOT](https://img.shields.io/badge/NativeAOT-Compatible-brightgreen?style=for-the-badge)](https://learn.microsoft.com/en-us/dotnet/core/deploying/native-aot)

---

**EricksonLopez.Result** is a high-performance, struct-based, enterprise-grade **Result Pattern** and Railway-Oriented Programming ecosystem for modern .NET (`.NET 8`, `.NET 9`, `.NET 10`). Designed for mission-critical, high-throughput systems, it eliminates the CPU latency and heap allocation overhead of exception-driven control flow while providing a rich domain error taxonomy, RFC 9457 HTTP ProblemDetails mapping, distributed OpenTelemetry tracing, NativeAOT compliance, Roslyn compile-time analyzers, and fluent unit testing assertions.

---

## Table of Contents

- [What Problem It Solves](#-what-problem-it-solves)
- [Key Features](#-key-features)
- [Ecosystem](#-ecosystem)
- [Documentation](#-documentation)
  - [Interactive Showcase (Levels 00 to 10)](#-step-by-step-interactive-showcase-levels-00-to-10)
  - [Technical Reference & Architecture Guides](#-technical-reference--architecture-guides)
- [Installation](#-installation)
- [Quick Start](#-quick-start)
  - [1. Core Result & Domain Errors](#1-core-result--domain-errors)
  - [2. Monadic Pipeline (Railway-Oriented)](#2-monadic-pipeline-railway-oriented)
  - [3. Zero-Allocation TState Pattern](#3-zero-allocation-tstate-pattern)
  - [4. Pattern Matching & Safe Unwrapping](#4-pattern-matching--safe-unwrapping)
  - [5. Compound Validation](#5-compound-validation)
  - [6. LINQ Query Syntax](#6-linq-query-syntax)
  - [7. Well-Known Error Sentinels](#7-well-known-error-sentinels)
- [Core Use Cases](#-core-use-cases)
  - [Use Case 1: Clean Architecture Application Services / CQRS](#use-case-1-clean-architecture-application-services--cqrs)
  - [Use Case 2: Multi-Step Domain Workflow with Short-Circuiting](#use-case-2-multi-step-domain-workflow-with-short-circuiting)
  - [Use Case 3: Compound Form & Entity Validation](#use-case-3-compound-form--entity-validation)
  - [Use Case 4: Repository Querying with Maybe\<T\>](#use-case-4-repository-querying-with-maybet)
  - [Use Case 5: Compile-Time Strongly-Typed Domain Error Hierarchies](#use-case-5-compile-time-strongly-typed-domain-error-hierarchies)
  - [Use Case 6: Async Compound Validation with ValidateAllAsync](#use-case-6-async-compound-validation-with-validateallasync)
- [Configuration & Integrations](#-configuration--integrations)
  - [ASP.NET Core & RFC 9457 ProblemDetails](#aspnet-core--rfc-9457-problemdetails)
  - [OpenAPI Metadata Extensions](#openapi-metadata-extensions)
  - [OpenTelemetry Tracing & Metrics](#opentelemetry-tracing--metrics)
  - [JSON Serialization & NativeAOT](#json-serialization--nativeaot)
  - [FluentValidation Integration](#fluentvalidation-integration)
  - [MediatR Pipeline Behavior](#mediatr-pipeline-behavior)
  - [Roslyn Diagnostic Analyzers](#roslyn-diagnostic-analyzers)
- [Testing & Quality](#-testing--quality)
  - [Fluent Assertions API](#fluent-assertions-api)
  - [Test Framework Adapters](#test-framework-adapters)
  - [ValueTask & Async Deadlock Avoidance](#valuetask--async-deadlock-avoidance)
  - [Mutation Testing & Quality Gates](#mutation-testing--quality-gates)
- [Performance Benchmarks](#-performance-benchmarks)
  - [Result Construction Benchmark](#result-construction-benchmark)
  - [Pipeline Operations Benchmark](#pipeline-operations-benchmark)
- [Compatibility & Technical Matrix](#-compatibility--technical-matrix)
  - [Framework & NativeAOT Support](#framework--nativeaot-support)
  - [HTTP Status Code Mapping Matrix](#http-status-code-mapping-matrix)
- [Architecture & Design Principles](#-architecture--design-principles)
  - [Railway-Oriented Programming (ROP) Flow](#railway-oriented-programming-rop-flow)
  - [Struct Memory Layout & State Lifecycle](#struct-memory-layout--state-lifecycle)
- [Best Practices & Anti-Patterns](#-best-practices--anti-patterns)
  - [Recommended vs Avoid](#recommended-vs-avoid)
- [Troubleshooting & Common Pitfalls](#-troubleshooting--common-pitfalls)
- [Part of the EricksonLopez Ecosystem](#-part-of-the-ericksonlopez-ecosystem)
- [Contributing](#-contributing)
- [License](#-license)

---

## 🎯 What Problem It Solves

In enterprise .NET applications, managing business rule violations, input validation failures, and asynchronous workflows using traditional patterns introduces critical architectural and performance drawbacks:

1. **The Hidden Cost of Exceptions for Control Flow:**
   Throwing exceptions for expected domain outcomes (e.g., `UserNotFoundException`, `ValidationException`) causes severe latency penalties due to stack trace captures, thread context switches, and GC heap allocations. It also creates hidden control flow pathways that bypass type signatures.

2. **Primitive Obsession & Loss of Context:**
   Returning primitive flags (`bool`, `null`, or tuple pairs) strips away domain diagnostics: error codes, severity levels, retryability hints, localized message keys, and distributed tracing IDs.

3. **Allocation Overhead in Existing Result Libraries:**
   Many third-party Result libraries rely on heap-allocated `class` wrappers, box value types during pipeline operations, allocate compiler closure objects on every lambda execution, or fail when compiled under NativeAOT and trimming.

### How `EricksonLopez.Result` Solves This

- **Zero-Allocation Execution:** Core `Result` and `Result<TValue>` are `readonly struct` value types — generating **0 bytes of heap allocation** on happy paths.
- **Closure-Free Monadic Pipelines:** Every combinator (`Map`, `Bind`, `Ensure`, `Tap`, `Match`, `Recover`) includes `TState` overloads to completely eliminate lambda closure allocations.
- **Rich Domain Error Taxonomy:** Sealed `Error` class encapsulates `ErrorType`, `ErrorSeverity`, `ErrorRetryability`, lazy zero-allocation ambient `TraceId`, and localized resource keys (`DescriptionKey`).
- **Seamless Framework Integrations:** Native mapping to ASP.NET Core RFC 9457 ProblemDetails, OpenTelemetry `ActivitySource` tracing, System.Text.Json source generation, and Roslyn compile-time analyzers.

---

## ⚡ Key Features

- 🚀 **Zero-Allocation Envelope**: Core `Result` and `Result<TValue>` are `readonly struct` value types — zero heap allocation for success results.
- ⚡ **Closure-Free `TState` Pipeline**: All monadic operators (`Map`, `Bind`, `TapOnSuccess`, `TapOnFailure`, `Match`, `Execute`, `Ensure`, `Recover`) offer `TState` overloads to completely eliminate lambda closure allocations in hot execution paths.
- 🔒 **Rich Enterprise Error Taxonomy**: Sealed `Error` class featuring `ErrorType`, `ErrorSeverity`, `ErrorRetryability`, lazy zero-alloc `TraceId` (ambient `Activity`), `CorrelationId`, localized keys (`DescriptionKey` - see [i18n guide](https://github.com/ericksonlopezf/dotnet-result/blob/main/docs/internationalization.md)), and immutable `Metadata`.
- 🧩 **Span-Based `Result.Combine` & `Result.ValidateAll`**: Aggregates multiple validation failures or typed tuple results using `ArrayPool<Error>` to eliminate temporary array allocations.
- 🌐 **ASP.NET Core RFC 9457 Integration**: Automatic mapping of `Result` to HTTP responses (`200 OK`, `400 Bad Request`, `401 Unauthorized`, `403 Forbidden`, `404 Not Found`, `409 Conflict`, `503 Unavailable`, `500 Server Error`).
- 📊 **First-Class OpenTelemetry**: BCL-only ambient trace capture (`Error.TraceId`) and optional `ActivitySource` metrics counters via `EricksonLopez.Result.OpenTelemetry`.
- ⚡ **NativeAOT & Trimming Safe**: Designed with zero reflection in hot paths, featuring source-generated `JsonSerializerContext` definitions (`IsAotCompatible=true`).
- 🛡️ **Bundled Roslyn Diagnostic Analyzers**: 13+ compile-time diagnostic rules (`RESULT001`, `RESULT003`–`RESULT010`, `RESULT012`, `RESULT013`, `RESULT_OTEL_001` bundled with core; `RESULT_GEN_001` with `EricksonLopez.Result.Serialization`) preventing performance degradation, uninitialized structs, and sensitive data leakage.
- 🧪 **Fluent Test Assertions**: Declarative assertion API for unit testing with standard test frameworks (xUnit, NUnit, MSTest). Fully supports asynchronous `ValueTask` validation avoiding testing framework deadlocks.

---

## 📦 Ecosystem

| Package | Version | Description |
|---|---|---|
| [`EricksonLopez.Result`](https://www.nuget.org/packages/EricksonLopez.Result) | [![NuGet](https://img.shields.io/nuget/v/EricksonLopez.Result?style=flat-square)](https://www.nuget.org/packages/EricksonLopez.Result) | Core struct-based `Result`, `Error` domain model, monadic pipeline, cumulative `ValidateAll`, LINQ support, and bundled Roslyn analyzers |
| [`EricksonLopez.Result.Generic`](https://www.nuget.org/packages/EricksonLopez.Result.Generic) | [![NuGet](https://img.shields.io/nuget/v/EricksonLopez.Result.Generic?style=flat-square)](https://www.nuget.org/packages/EricksonLopez.Result.Generic) | Strongly-typed `Result<TValue, TError>` with compile-time error types for strict domain model pipelines |
| [`EricksonLopez.Result.Maybe`](https://www.nuget.org/packages/EricksonLopez.Result.Maybe) | [![NuGet](https://img.shields.io/nuget/v/EricksonLopez.Result.Maybe?style=flat-square)](https://www.nuget.org/packages/EricksonLopez.Result.Maybe) | Struct-based `Maybe<T>` option type for DDD repositories and query layers with seamless `Result` interop |
| [`EricksonLopez.Result.AspNetCore`](https://www.nuget.org/packages/EricksonLopez.Result.AspNetCore) | [![NuGet](https://img.shields.io/nuget/v/EricksonLopez.Result.AspNetCore?style=flat-square)](https://www.nuget.org/packages/EricksonLopez.Result.AspNetCore) | ASP.NET Core Minimal APIs filter & RFC 9457 ProblemDetails HTTP response mapper |
| [`EricksonLopez.Result.OpenApi`](https://www.nuget.org/packages/EricksonLopez.Result.OpenApi) | [![NuGet](https://img.shields.io/nuget/v/EricksonLopez.Result.OpenApi?style=flat-square)](https://www.nuget.org/packages/EricksonLopez.Result.OpenApi) | Minimal API OpenAPI metadata extensions (`ProducesResult<T>()`) for automated schema documentation |
| [`EricksonLopez.Result.OpenTelemetry`](https://www.nuget.org/packages/EricksonLopez.Result.OpenTelemetry) | [![NuGet](https://img.shields.io/nuget/v/EricksonLopez.Result.OpenTelemetry?style=flat-square)](https://www.nuget.org/packages/EricksonLopez.Result.OpenTelemetry) | OpenTelemetry `ActivitySource` tracing integration and `System.Diagnostics.Metrics` counters (BCL-only) |
| [`EricksonLopez.Result.Serialization`](https://www.nuget.org/packages/EricksonLopez.Result.Serialization) | [![NuGet](https://img.shields.io/nuget/v/EricksonLopez.Result.Serialization?style=flat-square)](https://www.nuget.org/packages/EricksonLopez.Result.Serialization) | `System.Text.Json` custom converters and NativeAOT trim-safe `JsonSerializerContext` |
| [`EricksonLopez.Result.Serialization.Generators`](https://www.nuget.org/packages/EricksonLopez.Result.Serialization.Generators) | [![NuGet](https://img.shields.io/nuget/v/EricksonLopez.Result.Serialization.Generators?style=flat-square)](https://www.nuget.org/packages/EricksonLopez.Result.Serialization.Generators) | Roslyn source generator for AOT-compatible `Result<T>` JSON serialization |
| [`EricksonLopez.Result.DomainErrors.Generators`](https://www.nuget.org/packages/EricksonLopez.Result.DomainErrors.Generators) | [![NuGet](https://img.shields.io/nuget/v/EricksonLopez.Result.DomainErrors.Generators?style=flat-square)](https://www.nuget.org/packages/EricksonLopez.Result.DomainErrors.Generators) | Roslyn incremental source generator for strongly-typed domain error classes from `*.errors.json` |
| [`EricksonLopez.Result.FluentValidation`](https://www.nuget.org/packages/EricksonLopez.Result.FluentValidation) | [![NuGet](https://img.shields.io/nuget/v/EricksonLopez.Result.FluentValidation?style=flat-square)](https://www.nuget.org/packages/EricksonLopez.Result.FluentValidation) | FluentValidation integration — converts `ValidationResult` to structured `Result` failures |
| [`EricksonLopez.Result.MediatR`](https://www.nuget.org/packages/EricksonLopez.Result.MediatR) | [![NuGet](https://img.shields.io/nuget/v/EricksonLopez.Result.MediatR?style=flat-square)](https://www.nuget.org/packages/EricksonLopez.Result.MediatR) | MediatR pipeline behavior — catches unhandled exceptions and wraps them as `Result` failures |
| [`EricksonLopez.Result.EntityFrameworkCore`](https://www.nuget.org/packages/EricksonLopez.Result.EntityFrameworkCore) | [![NuGet](https://img.shields.io/nuget/v/EricksonLopez.Result.EntityFrameworkCore?style=flat-square)](https://www.nuget.org/packages/EricksonLopez.Result.EntityFrameworkCore) | Entity Framework Core integration — exception-safe `SaveChangesAsyncToResult`, query extensions, and concurrency mapping |
| [`EricksonLopez.Result.Polly`](https://www.nuget.org/packages/EricksonLopez.Result.Polly) | [![NuGet](https://img.shields.io/nuget/v/EricksonLopez.Result.Polly?style=flat-square)](https://www.nuget.org/packages/EricksonLopez.Result.Polly) | Polly v8 resilience pipelines with Result-aware retry strategies (`AddResultRetry`) and predicate filtering |
| [`EricksonLopez.Result.MassTransit`](https://www.nuget.org/packages/EricksonLopez.Result.MassTransit) | [![NuGet](https://img.shields.io/nuget/v/EricksonLopez.Result.MassTransit?style=flat-square)](https://www.nuget.org/packages/EricksonLopez.Result.MassTransit) | MassTransit consumer filters — wraps consumer faults and responses into structured Result contracts |
| [`EricksonLopez.Result.Dapr`](https://www.nuget.org/packages/EricksonLopez.Result.Dapr) | [![NuGet](https://img.shields.io/nuget/v/EricksonLopez.Result.Dapr?style=flat-square)](https://www.nuget.org/packages/EricksonLopez.Result.Dapr) | Dapr distributed state & pub/sub bindings — maps state store ETags, bulk operations, and pub/sub events to Result envelopes |
| [`EricksonLopez.Result.Grpc`](https://www.nuget.org/packages/EricksonLopez.Result.Grpc) | [![NuGet](https://img.shields.io/nuget/v/EricksonLopez.Result.Grpc?style=flat-square)](https://www.nuget.org/packages/EricksonLopez.Result.Grpc) | gRPC server interceptor & client extensions — maps domain `Error` failures to canonical `RpcException` with metadata trailers |
| [`EricksonLopez.Result.Testing`](https://www.nuget.org/packages/EricksonLopez.Result.Testing) | [![NuGet](https://img.shields.io/nuget/v/EricksonLopez.Result.Testing?style=flat-square)](https://www.nuget.org/packages/EricksonLopez.Result.Testing) | Framework-agnostic fluent testing assertion library (`ShouldBeSuccess()`, `ShouldHaveError()`) |
| [`EricksonLopez.Result.Testing.XUnit`](https://www.nuget.org/packages/EricksonLopez.Result.Testing.XUnit) | [![NuGet](https://img.shields.io/nuget/v/EricksonLopez.Result.Testing.XUnit?style=flat-square)](https://www.nuget.org/packages/EricksonLopez.Result.Testing.XUnit) | xUnit-specific test helpers — assertion failures surface as `XunitException` |
| [`EricksonLopez.Result.Testing.NUnit`](https://www.nuget.org/packages/EricksonLopez.Result.Testing.NUnit) | [![NuGet](https://img.shields.io/nuget/v/EricksonLopez.Result.Testing.NUnit?style=flat-square)](https://www.nuget.org/packages/EricksonLopez.Result.Testing.NUnit) | NUnit-specific test helpers — assertion failures surface as `AssertionException` |
| [`EricksonLopez.Result.Analyzers`](https://www.nuget.org/packages/EricksonLopez.Result.Analyzers) | [![NuGet](https://img.shields.io/nuget/v/EricksonLopez.Result.Analyzers?style=flat-square)](https://www.nuget.org/packages/EricksonLopez.Result.Analyzers) | Roslyn analyzers & code fixes (RESULT001–RESULT013) — bundled with Core, also available standalone |

---

## 📚 Documentation

> 🌐 **Official Documentation Hub:** [https://github.com/ericksonlopezf/dotnet-result/tree/main/docs](https://github.com/ericksonlopezf/dotnet-result/tree/main/docs) | **Web Portal:** [https://ericksonlopez.dev/result](https://ericksonlopez.dev/result)

### 🎓 Step-by-Step Interactive Showcase (Levels 00 to 10)

| Level | Topic | Description |
|---|---|---|
| [**Level 00**](https://github.com/ericksonlopezf/dotnet-result/blob/main/docs/showcase/level-00-conceptual.md) | **Architecture & Philosophy** | Railway-Oriented Programming (ROP), zero-allocation guarantees, and competitive peer benchmarks |
| [**Level 01**](https://github.com/ericksonlopezf/dotnet-result/blob/main/docs/showcase/level-01-quickstart.md) | **Getting Started & Primitives** | Basic `Result` and `Result<T>` creation, DI configuration, and safe value unwrapping |
| [**Level 02**](https://github.com/ericksonlopezf/dotnet-result/blob/main/docs/showcase/level-02-complete-configuration.md) | **Domain Modeling & ErrorBuilder** | Rich error taxonomy, severity, retryability, metadata, and copy-on-write `ErrorBuilder` |
| [**Level 03**](https://github.com/ericksonlopezf/dotnet-result/blob/main/docs/showcase/level-03-real-world-usecases.md) | **Real-World Use Cases** | User registration, payment processing, inventory reservation, and strongly-typed `Result<T, E>` |
| [**Level 04**](https://github.com/ericksonlopezf/dotnet-result/blob/main/docs/showcase/level-04-advanced-integration.md) | **Functional Composition & LINQ** | Monadic chaining, heterogeneous `Result.Combine`, guards with `Merge`, `ValidateAll`, and LINQ |
| [**Level 05**](https://github.com/ericksonlopezf/dotnet-result/blob/main/docs/showcase/level-05-processing.md) | **Asynchronous & Background Work** | `Task<Result>`, `ValueTask<Result>`, `TryAsync`, cooperative `CancellationToken`, and batching |
| [**Level 06**](https://github.com/ericksonlopezf/dotnet-result/blob/main/docs/showcase/level-06-error-handling.md) | **Resilient Error Recovery** | `Recover`, `MapError`, `MapFailure`, transient vs permanent classification, and dead-letter queues |
| [**Level 07**](https://github.com/ericksonlopezf/dotnet-result/blob/main/docs/showcase/level-07-scalability.md) | **Extreme High-Throughput** | Zero-allocation `TState` state passing, `in Result<T>` ref semantics, and Native AOT certification |
| [**Level 08**](https://github.com/ericksonlopezf/dotnet-result/blob/main/docs/showcase/level-08-customization.md) | **Customization & Polymorphism** | `IResultOutcome` interface, `ErrorEqualityComparer.Default`/`Strict`, and custom error factories |
| [**Level 09**](https://github.com/ericksonlopezf/dotnet-result/blob/main/docs/showcase/level-09-extensions.md) | **Official Ecosystem Integrations** | ASP.NET Core, OpenAPI, FluentValidation, MediatR, EF Core, Polly, MassTransit, OpenTelemetry, Dapr, and gRPC |
| [**Level 10**](https://github.com/ericksonlopezf/dotnet-result/blob/main/docs/showcase/level-10-enterprise-architecture.md) | **Enterprise Architecture** | Clean Architecture, DDD aggregates, CQRS handlers, RFC 9457 ProblemDetails, and W3C tracing |

### 📖 Technical Reference & Architecture Guides

- [**Interactive Showcase Hub**](https://github.com/ericksonlopezf/dotnet-result/blob/main/docs/showcase/readme.md) — Master progressive showcase guide indexing all 11 levels and 34 runnable sample modules.
- [**API Reference (Microsoft Learn Style)**](https://github.com/ericksonlopezf/dotnet-result/blob/main/docs/api-reference.md) — Exhaustive method signatures, parameters, returns, and complexity specifications.
- [**Functional Map & Component Architecture**](https://github.com/ericksonlopezf/dotnet-result/blob/main/docs/functional-map.md) — Layer transitions, sequence diagrams, state machines, and Mermaid architecture maps.
- [**Frequently Asked Questions (FAQ)**](https://github.com/ericksonlopezf/dotnet-result/blob/main/docs/faq.md) — Architectural rationale, zero-allocation mechanics, and common questions.
- [**Troubleshooting & Diagnostics**](https://github.com/ericksonlopezf/dotnet-result/blob/main/docs/troubleshooting.md) — Remediation steps for Roslyn analyzers RESULT001–RESULT013, runtime gotchas, and tracing setup.
- [**Cookbook & Recipes**](https://github.com/ericksonlopezf/dotnet-result/blob/main/docs/cookbook.md) — 26 copy-pasteable enterprise recipes for ASP.NET Core, EF Core, MassTransit, Polly, OpenTelemetry, Dapr, gRPC, testing, and LINQ syntax.
- [**Architecture & Invariants**](https://github.com/ericksonlopezf/dotnet-result/blob/main/docs/architecture.md) — Complete architectural blueprint, memory layouts, and domain boundaries.
- [**Architectural Decision Records (ADRs)**](https://github.com/ericksonlopezf/dotnet-result/tree/main/docs/adr) — 27 ADRs (ADR-001–ADR-027) plus 2 rejected proposals documenting design rationale.
- [**Technical Audit**](https://github.com/ericksonlopezf/dotnet-result/blob/main/docs/audit.md) — Comprehensive technical audit, guarantees, and verification.
- [**Competitive Audit**](https://github.com/ericksonlopezf/dotnet-result/blob/main/docs/competitive-audit.md) — In-depth market comparison vs CSharpFunctionalExtensions, FluentResults, ErrorOr, OneOf, etc.
- [**Feature Catalog & Specs**](https://github.com/ericksonlopezf/dotnet-result/blob/main/docs/features.md) — Exhaustive specification of all core types, monads, and extensions.
- [**Features & Compatibility Matrix**](https://github.com/ericksonlopezf/dotnet-result/blob/main/docs/features-matrix.md) — Target framework matrix, diagnostics, and HTTP status codes.
- [**Testing & Quality Audit**](https://github.com/ericksonlopezf/dotnet-result/blob/main/docs/quality-audit.md) — Verification topology, fast-path/slow-path testing, and mutation metrics.
- [**Best Practices Guide**](https://github.com/ericksonlopezf/dotnet-result/blob/main/docs/best-practices.md) — Recommended production patterns for microservices and domain logic.
- [**Anti-Patterns Guide**](https://github.com/ericksonlopezf/dotnet-result/blob/main/docs/anti-patterns.md) — Unsafe patterns, state bugs, and pitfalls to avoid.
- [**Roslyn Analyzers Reference**](https://github.com/ericksonlopezf/dotnet-result/blob/main/docs/analyzers.md) — Detailed specifications and remediation steps for diagnostic rules RESULT001–RESULT013.
- [**Internationalization (i18n)**](https://github.com/ericksonlopezf/dotnet-result/blob/main/docs/internationalization.md) — Multi-language localized error messages using `DescriptionKey`.
- [**Migration Guide**](https://github.com/ericksonlopezf/dotnet-result/blob/main/docs/migration-guide.md) — Step-by-step guide for migrating from raw exceptions, FluentResults, or ErrorOr.
- [**Package Reference**](https://github.com/ericksonlopezf/dotnet-result/blob/main/docs/package-reference.md) — Full dependency graph and per-package metadata for all 20 NuGet packages.
- [**CI/CD & Build Pipeline**](https://github.com/ericksonlopezf/dotnet-result/blob/main/docs/cicd.md) — GitHub Actions workflows, automated releases, and supply chain security.

---

## 📥 Installation

Install the required packages using the .NET CLI:

### 1. Core Package (Required)

```bash
# Core package includes struct-based Result, Error taxonomy, and bundled Roslyn Analyzers
dotnet add package EricksonLopez.Result
```

### 2. Optional Framework & Integration Packages

```bash
# Strongly-typed error pipeline (Result<TValue, TError>)
dotnet add package EricksonLopez.Result.Generic

# Option monad for domain queries (Maybe<T>)
dotnet add package EricksonLopez.Result.Maybe

# ASP.NET Core RFC 9457 ProblemDetails & Minimal API Filters
dotnet add package EricksonLopez.Result.AspNetCore

# Minimal API OpenAPI metadata extensions (ProducesResult<T>)
dotnet add package EricksonLopez.Result.OpenApi

# OpenTelemetry Activity tracing & BCL Metrics counters
dotnet add package EricksonLopez.Result.OpenTelemetry

# System.Text.Json converters & NativeAOT trimming support
dotnet add package EricksonLopez.Result.Serialization

# Source generator for domain error classes (*.errors.json)
dotnet add package EricksonLopez.Result.DomainErrors.Generators

# FluentValidation validation result mapping
dotnet add package EricksonLopez.Result.FluentValidation

# MediatR pipeline exception-handling behavior
dotnet add package EricksonLopez.Result.MediatR

# Entity Framework Core resilient operations & query extensions
dotnet add package EricksonLopez.Result.EntityFrameworkCore

# Polly v8 resilience pipeline integration with Result retries
dotnet add package EricksonLopez.Result.Polly

# MassTransit ResultFault contract & consume filter
dotnet add package EricksonLopez.Result.MassTransit

# Dapr distributed state store & pub/sub bindings
dotnet add package EricksonLopez.Result.Dapr

# gRPC server interceptor & status code mapping
dotnet add package EricksonLopez.Result.Grpc
```

### 3. Testing & Assertion Packages

```bash
# Framework-agnostic fluent testing assertions
dotnet add package EricksonLopez.Result.Testing

# xUnit-specific assertion helpers (surfaces XunitException)
dotnet add package EricksonLopez.Result.Testing.XUnit

# NUnit-specific assertion helpers (surfaces AssertionException)
dotnet add package EricksonLopez.Result.Testing.NUnit
```

---

## 🚀 Quick Start

### 1. Core Result & Domain Errors

Define rich domain errors using semantic factory methods and return `Result<T>` instead of throwing exceptions:

```csharp
using EricksonLopez.Result;

public static class UserErrors
{
    public static Error NotFound(Guid id) =>
        Error.NotFound("User.NotFound", $"User with ID '{id}' was not found.");

    public static readonly Error InvalidEmail =
        Error.Validation("User.InvalidEmail", "The email format is invalid.");

    public static readonly Error Suspended =
        Error.Forbidden("User.Suspended", "User account has been suspended.")
             .WithRetryability(ErrorRetryability.Permanent);
}

public class UserService
{
    private readonly IUserRepository _repository;

    public UserService(IUserRepository repository) => _repository = repository;

    public Result<User> GetUser(Guid id)
    {
        var user = _repository.Find(id);
        
        return user is null 
            ? UserErrors.NotFound(id) 
            : user; // Implicit conversion to Result<User>.Success(user)
    }
}
```

#### Built-In Semantic Error Factory Methods

The sealed `Error` model provides semantic factory methods tailored for domain-driven design, mapping error types and default severities directly:

| Factory Method | `ErrorType` | Default `ErrorSeverity` | Semantic Role |
|---|---|---|---|
| `Error.Validation(code, desc)` | `Validation` | `Warning` | Input format, boundary validation, or contract violation |
| `Error.NotFound(code, desc)` | `NotFound` | `Warning` | Target resource or aggregate does not exist |
| `Error.Conflict(code, desc)` | `Conflict` | `Warning` | Concurrency state conflict or unique key violation |
| `Error.Unauthorized(code, desc)` | `Unauthorized` | `Error` | Missing or expired authentication credentials |
| `Error.Forbidden(code, desc)` | `Forbidden` | `Error` | Permission denied for authenticated user |
| `Error.Domain(code, desc)` | `Domain` | `Error` | Domain invariant or business rule violation |
| `Error.Business(code, desc)` | `Domain` | `Error` | Business rule violation (semantic alias for `Error.Domain`) |
| `Error.Unavailable(code, desc)` | `Unavailable` | `Error` (Transient) | Transient downstream service or network outage |
| `Error.Infrastructure(code, desc)` | `Infrastructure` | `Error` (Transient) | Database connectivity, disk I/O, or message queue failure |
| `Error.Failure(code, desc)` | `Failure` | `Error` | General unclassified domain or operation failure |
| `Error.Unexpected(code, desc)` | `Unexpected` | `Critical` | Unhandled critical system exception or runtime fault |

### 2. Monadic Pipeline (Railway-Oriented)

Chain synchronous and asynchronous operations effortlessly with automatic failure short-circuiting:

```csharp
public async Task<Result<OrderDto>> ProcessOrderAsync(
    Guid userId, 
    CreateOrderCommand command, 
    CancellationToken cancellationToken)
{
    return await _userService.GetUserAsync(userId, cancellationToken)
        .Ensure(u => u.IsActive, UserErrors.Suspended, cancellationToken)
        .Bind(u => _orderService.CreateOrderAsync(u, command, cancellationToken))
        .TapOnSuccess(order => _logger.LogInformation("Order {Id} created", order.Id), cancellationToken)
        .TapOnFailure(error => _logger.LogWarning("Order creation failed: {Code}", error.Code), cancellationToken)
        .Map(order => new OrderDto(order.Id, order.TotalAmount), cancellationToken);
}
```

### 3. Zero-Allocation TState Pattern

Standard lambdas capturing local variables allocate compiler display closures on the heap. High-throughput paths can eliminate closure allocations by passing `TState` alongside `static` lambdas:

```csharp
var minPrice = 50.0m;
var maxPrice = 500.0m;

// ❌ Allocates: Captures 'minPrice' and 'maxPrice' in a heap closure object
var result = GetProduct(id)
    .Ensure(p => p.Price >= minPrice && p.Price <= maxPrice, 
            Error.Validation("Price.OutOfRange", "Price out of bounds"));

// ✅ Zero Closure Allocation: State passed via tuple and evaluated in static lambda
var state = (minPrice, maxPrice);
var result = GetProduct(id)
    .Ensure(state, 
            static (s, p) => p.Price >= s.minPrice && p.Price <= s.maxPrice, 
            Error.Validation("Price.OutOfRange", "Price out of bounds"));
```

> [!TIP]
> `TState` overloads are available for all monadic operators: `Map`, `Bind`, `TapOnSuccess`, `TapOnFailure`, `Match`, `Execute`, `Ensure`, and `Recover`.

> [!TIP]
> **Avoid struct copies in synchronous pipelines:** For `Result<T>` where `T` is a large value type (≥ 16 bytes, e.g., `decimal`, `Guid`, large tuples), use the `in`-parameter overloads from `ResultSyncExtensions`. These carry identical names (`Map`, `Bind`, `Ensure`, `Match`, `TryGetValue`, `GetValueOrDefault`) but accept the struct by readonly `in` reference, eliminating the copy on every pipeline call. Call them as normal extension methods — the compiler selects the `in` overload automatically when the receiver is a local variable:
> ```csharp
> Result<decimal> result = GetPrice(productId);
> // Compiler selects ResultSyncExtensions.Map(in result, mapper) — zero struct copy
> var formatted = result.Map(price => price.ToString("C2"));
> ```

### 4. Pattern Matching & Safe Unwrapping

Safely consume results using functional matching, idiomatic `TryGetValue`, or tuple destructuring:

```csharp
// 1. Functional Pattern Matching
string message = result.Match(
    dto => $"Success: Order {dto.Id} processed with total ${dto.TotalAmount}",
    error => $"Error [{error.Code}]: {error.Description}"
);

// 2. Idiomatic TryGetValue Pattern
if (result.TryGetValue(out var orderDto))
{
    Console.WriteLine($"Order ID: {orderDto.Id}");
}

// 3. Tuple Destructuring
var (isSuccess, value, error) = result;
if (isSuccess)
{
    Console.WriteLine($"Processed: {value.Id}");
}
else
{
    Console.WriteLine($"Failed: {error.Description}");
}
```

### 5. Compound Validation

Aggregate multiple validation errors concurrently without throwing or allocating unnecessary intermediate collections:

```csharp
// Declarative rule evaluation with Result.ValidateAll
public Result<Order> ValidateOrder(Order order)
{
    return Result.ValidateAll(
        order,
        static o => o.Items.Count > 0 
            ? Result.Success() 
            : Error.Validation("Order.NoItems", "Order must contain at least one item."),
        static o => o.TotalAmount > 0 
            ? Result.Success() 
            : Error.Validation("Order.InvalidAmount", "Total amount must be greater than zero.")
    );
}

// Imperative accumulation with ErrorBuilder (Note: While ErrorBuilder struct is stack-allocated, its InnerErrors collection uses heap allocation)
public Result<Customer> CreateCustomer(CreateCustomerCommand command)
{
    var builder = ErrorBuilder.Validation("Customer.InvalidPayload", "Customer validation failed.");

    if (string.IsNullOrWhiteSpace(command.Name))
        builder = builder.WithInnerError(Error.Validation("Customer.NameRequired", "Name is required."));

    if (command.Age < 18)
        builder = builder.WithInnerError(Error.Validation("Customer.Underage", "Customer must be at least 18 years old."));

    if (builder.HasInnerErrors)
        return builder.Build();

    return new Customer(command.Name, command.Age);
}
```

### 6. LINQ Query Syntax

`ResultLinqExtensions` enables standard LINQ `from`/`where`/`select` query syntax on `Result<T>` pipelines, forwarding failures automatically:

```csharp
// LINQ select: maps the success value (equivalent to .Map())
Result<string> formatted =
    from order in GetOrder(id)
    select order.Total.ToString("C2");

// LINQ where: filters with a predicate; returns Validation error if false
Result<Order> active =
    from order in GetOrder(id)
    where order.IsActive
    select order;

// LINQ comprehension: monadic chaining via SelectMany (equivalent to .Bind())
Result<InvoiceDto> invoice =
    from order in GetOrder(id)
    from customer in GetCustomer(order.CustomerId)
    select new InvoiceDto(order.Id, customer.Name, order.Total);
```

> [!TIP]
> `ResultLinqExtensions` are available automatically — no extra import needed. The `where` clause produces a `Validation` error with code `"Result.FilteredOut"` when the predicate returns `false`. Use `.Ensure(predicate, error)` for custom error messages.

### 7. Well-Known Error Sentinels

`WellKnownErrors` provides static sentinel `Error` constants used internally by the framework:

| Constant | Code | Usage |
|---|---|---|
| `WellKnownErrors.UninitializedError` | `"Result.Uninitialized"` | Returned by `TryGetError` on uninitialized `default(Result)` |
| `WellKnownErrors.CombinedFailuresCode` | `"Result.CombinedErrors"` | Used by `Result.Combine`/`ValidateAll` when aggregating multiple failures |

> [!NOTE]
> `WellKnownErrors.UninitializedError` is an `Error` instance with `ErrorType.Unexpected`, `ErrorSeverity.Critical`, and `ErrorRetryability.NotApplicable`. It is created via `Error.CreateSentinel()` which bypasses `Activity.Current` capture at static initialization time, ensuring `TraceId` is always `null`. `CombinedFailuresCode` is a `const string` — not an `Error` instance.

---

## 💡 Core Use Cases

### Use Case 1: Clean Architecture Application Services / CQRS

Encapsulate application business workflows where command and query handlers return domain outcomes with explicit status codes:

```csharp
public class ConfirmPaymentCommandHandler : IRequestHandler<ConfirmPaymentCommand, Result<PaymentConfirmationDto>>
{
    private readonly IPaymentGateway _gateway;
    private readonly IOrderRepository _orders;

    public ConfirmPaymentCommandHandler(IPaymentGateway gateway, IOrderRepository orders)
    {
        _gateway = gateway;
        _orders = orders;
    }

    public async Task<Result<PaymentConfirmationDto>> Handle(
        ConfirmPaymentCommand command, 
        CancellationToken cancellationToken)
    {
        return await _orders.GetByIdAsync(command.OrderId, cancellationToken)
            .Ensure(o => o.Status == OrderStatus.PendingPayment, 
                    Error.Conflict("Order.InvalidState", "Order is not pending payment."))
            .Bind(o => _gateway.ChargeAsync(o, command.PaymentMethod, cancellationToken))
            .Map(receipt => new PaymentConfirmationDto(receipt.TransactionId, receipt.AmountPaid));
    }
}
```

### Use Case 2: Multi-Step Domain Workflow with Short-Circuiting

Coordinate multiple operations where failure in any intermediate step immediately aborts downstream execution without extra indentation:

```csharp
public async Task<Result<SubscriptionDto>> ActivateSubscriptionAsync(
    Guid accountId, 
    string planCode, 
    CancellationToken cancellationToken)
{
    return await _accountService.GetAccountAsync(accountId, cancellationToken)
        .Ensure(a => a.IsVerified, Error.Forbidden("Account.Unverified", "Account email must be verified."), cancellationToken)
        .Bind(a => _billingService.ValidatePaymentMethodAsync(a.Id, cancellationToken), cancellationToken)
        .Bind(_ => _planCatalog.GetPlanByCodeAsync(planCode, cancellationToken), cancellationToken)
        .Bind(plan => _subscriptionService.ProvisionAsync(accountId, plan, cancellationToken), cancellationToken)
        .TapOnSuccess(sub => _eventBus.PublishAsync(new SubscriptionActivatedEvent(sub.Id), cancellationToken), cancellationToken)
        .Map(sub => new SubscriptionDto(sub.Id, sub.ExpiresAt), cancellationToken);
}
```

### Use Case 3: Compound Form & Entity Validation

Collect all field validation issues simultaneously and return a single aggregated ProblemDetails response:

```csharp
public Result<UserProfile> UpdateProfile(UpdateProfileCommand command)
{
    return Result.ValidateAll(
        command,
        static c => !string.IsNullOrWhiteSpace(c.DisplayName) 
            ? Result.Success() 
            : Error.Validation("Profile.DisplayNameRequired", "Display name is required."),
        static c => c.BirthDate < DateOnly.FromDateTime(DateTime.UtcNow) 
            ? Result.Success() 
            : Error.Validation("Profile.InvalidBirthDate", "Birth date must be in the past."),
        static c => c.Bio?.Length <= 500 
            ? Result.Success() 
            : Error.Validation("Profile.BioTooLong", "Bio cannot exceed 500 characters.")
    ).Map(c => new UserProfile(c.DisplayName, c.BirthDate, c.Bio));
}
```

### Use Case 4: Repository Querying with `Maybe<T>`

Express optional domain entities without relying on null references or nullable annotations:

```csharp
using EricksonLopez.Result.Maybe;

public class CustomerRepository : ICustomerRepository
{
    public async Task<Maybe<Customer>> FindByTaxIdAsync(string taxId, CancellationToken ct)
    {
        Customer? entity = await _dbContext.Customers.FirstOrDefaultAsync(c => c.TaxId == taxId, ct);
        return Maybe.From(entity);
    }
}

// Seamless conversion to Result when absence constitutes a domain failure:
Maybe<Customer> maybeCustomer = await _customerRepository.FindByTaxIdAsync("TAX-12345", ct);
Result<Customer> result = maybeCustomer.ToResult(Error.NotFound("Customer.NotFound", "Customer not found."));
```

### Use Case 5: Compile-Time Strongly-Typed Domain Error Hierarchies

Enforce exhaustive compile-time error handling with `Result<TValue, TError>`:

```csharp
using EricksonLopez.Result.Generic;

public abstract record OrderProcessingError;
public sealed record InventoryUnavailable(string Sku) : OrderProcessingError;
public sealed record CardDeclined(string Reason) : OrderProcessingError;

public Result<Order, OrderProcessingError> PlaceOrder(OrderRequest request)
{
    if (!_inventory.HasStock(request.Sku))
        return new InventoryUnavailable(request.Sku);

    if (!_payment.Charge(request.Payment))
        return new CardDeclined("Insufficient balance");

    return new Order(request.Sku, request.Quantity);
}
```

---

### Use Case 6: Async Compound Validation with `ValidateAllAsync`

When validation rules require async I/O (e.g., database lookups, external API calls), use `Result.ValidateAllAsync` to evaluate all rules sequentially and aggregate all failures into a single compound error:

```csharp
public async Task<Result<Order>> CreateOrderAsync(CreateOrderCommand command, CancellationToken ct)
{
    // Evaluates ALL validators — does not short-circuit on first failure.
    // Passes cancellationToken to each validator individually.
    Result validationResult = await Result.ValidateAllAsync(
        new List<Func<CancellationToken, Task<Result>>>
        {
            async token => await _customerRepo.ExistsAsync(command.CustomerId, token)
                ? Result.Success()
                : Error.NotFound("Customer.NotFound", $"Customer '{command.CustomerId}' not found."),

            async token => await _inventoryService.HasStockAsync(command.Sku, command.Quantity, token)
                ? Result.Success()
                : Error.Conflict("Order.InsufficientStock", $"Insufficient stock for SKU '{command.Sku}'."),

            async token => await _paymentService.ValidateMethodAsync(command.PaymentMethodId, token)
                ? Result.Success()
                : Error.Validation("Payment.InvalidMethod", "Payment method is invalid or expired.")
        },
        cancellationToken: ct);

    if (validationResult.IsFailure)
        return validationResult.Error; // compound error with all InnerErrors populated

    return await _orderService.CreateAsync(command, ct);
}
```

> **`ValueTask` overload**: Use `ValidateAllAsync(IReadOnlyList<Func<CancellationToken, ValueTask<Result>>>, CancellationToken)` when validators return `ValueTask<Result>` to avoid unnecessary task boxing overhead.
>
> **Value-preserving overloads**: `ValidateAllAsync<T>(T value, validators, ct)` validates the value against all rules and returns `Task<Result<T>>` (or `ValueTask<Result<T>>`), preserving the value if all validators pass.

---

## 🔌 Configuration & Integrations

### ASP.NET Core & RFC 9457 ProblemDetails

`EricksonLopez.Result.AspNetCore` maps `Result` and `Result<T>` directly to HTTP responses according to RFC 9457 (ProblemDetails).

```csharp
using EricksonLopez.Result.AspNetCore;
using EricksonLopez.Result.OpenApi;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// Direct ToHttpResult() (Zero boxing, full OpenAPI metadata)
app.MapGet("/users/{id:guid}", (Guid id, UserService userService) =>
{
    return userService.GetUser(id).ToHttpResult();
});

// Automatic Minimal API Endpoint Filter unwrapping Result responses
app.MapPost("/orders", (CreateOrderCommand command, OrderService orderService) =>
{
    return orderService.CreateOrder(command); // Returns Result<OrderDto>
})
.AddResultEndpointFilter()
.ProducesResult<OrderDto>(StatusCodes.Status200OK);

app.Run();
```

#### Security Configuration for Problem Details

Configure environment guards to avoid leaking internal exception messages in production:

```csharp
builder.Services.Configure<ResultHttpOptions>(options =>
{
    // Descriptions are included only in Development environments (Enforced by RESULT009)
    options.IncludeDescription = builder.Environment.IsDevelopment();
    options.DefaultInstanceUri = "/errors";
});
```

### OpenAPI Metadata Extensions

`EricksonLopez.Result.OpenApi` provides metadata extensions to document all possible HTTP status codes and response schemas:

```csharp
app.MapPost("/api/v1/payments", (PaymentRequest request, IPaymentService svc) => svc.Pay(request))
   .AddResultEndpointFilter()
   .ProducesResult<PaymentReceiptDto>(StatusCodes.Status200OK)
   .ProducesProblemDetails(StatusCodes.Status400BadRequest)
   .ProducesProblemDetails(StatusCodes.Status401Unauthorized)
   .ProducesProblemDetails(StatusCodes.Status409Conflict)
   .ProducesProblemDetails(StatusCodes.Status500InternalServerError);
```

### OpenTelemetry Tracing & Metrics

`EricksonLopez.Result.OpenTelemetry` automatically enriches active OpenTelemetry `Activity` spans and tracks real-time BCL metrics counters:

```csharp
using EricksonLopez.Result.OpenTelemetry;

public async Task<Result<PaymentReceipt>> ExecutePaymentAsync(PaymentRequest request)
{
    using var activity = MyActivitySource.StartActivity("ExecutePayment");
    
    var result = await _paymentGateway.ProcessAsync(request);

    // Records error status, error code, type, severity, and metadata tags on the Activity span
    result.TraceOutcome("ExecutePayment", activity);
    
    // Increment metrics counter (BCL System.Diagnostics.Metrics) based on outcome
    if (result.IsSuccess)
    {
        ResultMetrics.StaticTrackSuccess("ExecutePayment");
    }
    else
    {
        ResultMetrics.StaticTrackFailure("ExecutePayment", result.Error.Code, result.Error.Type.ToString());
    }

    return result;
}
```

### JSON Serialization & NativeAOT

`EricksonLopez.Result.Serialization` provides trim-safe `System.Text.Json` custom converters for `Result`, `Result<T>`, `Maybe<T>`, and `Error`.

```csharp
using System.Text.Json;
using EricksonLopez.Result.Serialization;

var options = new JsonSerializerOptions();
options.Converters.Add(new ResultJsonConverter());
options.Converters.Add(new ResultOfTJsonConverter<OrderDto>());
options.Converters.Add(new ErrorJsonConverter());

// NativeAOT JSON Source Generation setup:
[JsonSerializable(typeof(Result<OrderDto>))]
[JsonSerializable(typeof(Error))]
internal partial class AppJsonSerializerContext : JsonSerializerContext { }
```

### FluentValidation Integration

`EricksonLopez.Result.FluentValidation` maps `FluentValidation.Results.ValidationResult` directly into structured `Result` failures:

```csharp
using EricksonLopez.Result.FluentValidation;
using FluentValidation;

public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Age).GreaterThanOrEqualTo(18);
    }
}

// Execution:
ValidationResult validationResult = await validator.ValidateAsync(command, ct);
Result result = validationResult.ToValidationResult();
```

### MediatR Pipeline Behavior

`EricksonLopez.Result.MediatR` provides a pipeline behavior that intercepts unhandled exceptions in MediatR command and query handlers, wrapping them as structured `Result` failures:

```csharp
using EricksonLopez.Result.MediatR;

builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));
builder.Services.AddResultExceptionBehavior();
```

### Roslyn Diagnostic Analyzers

The `EricksonLopez.Result.Analyzers` package is bundled with the core `EricksonLopez.Result` package and enforces correctness and performance invariants at compile time:

| Diagnostic ID | Severity | Category | Description | CodeFix |
|---|:---:|---|---|:---:|
| `RESULT001` | ⚠️ Warning | Performance | `Result<T>` struct size exceeds 32 bytes — recommends using a class | ❌ |
| `RESULT003` | 🛑 **Error** | Usage | `ErrorBuilder.With*()` return value is discarded (mutation lost) | ✅ Assign Return |
| `RESULT004` | ⚠️ Warning | Performance | Lambda captures outer variable in pipeline (closure allocation) | ✅ Use `TState` |
| `RESULT005` | ⚠️ Warning | Performance | `Error.WithMetadata()` chained 3+ times consecutively without batching | ❌ |
| `RESULT006` | ⚠️ Warning | Performance | `ErrorBuilder.WithInnerError()` chained 2+ times consecutively | ❌ |
| `RESULT007` | ⚠️ Warning | Reliability | `HashSet<Error>` or LINQ deduplication used without `ErrorEqualityComparer.Strict` | ❌ |
| `RESULT008` | ⚠️ Warning | Usage | `AddResultEndpointFilter()` used without explicit `.Produces<T>()` metadata | ❌ |
| `RESULT009` | ⚠️ Warning | Security | `ResultHttpOptions.IncludeDescription = true` set without environment guard | ❌ |
| `RESULT010` | ⚠️ Warning | Security | `Exception.Message` passed directly to `ResultExceptionBehavior` error factory | ❌ |
| `RESULT012` | ⚠️ Warning | Usage | Method returns `default(Result)` or `default(Result<T>)` | ❌ |
| `RESULT013` | ⚠️ Warning | Usage | Avoid implicit bool conversion of Result in condition contexts | ✅ Check `.IsSuccess` |
| `RESULT_OTEL_001` | ℹ️ Info | Observability | `TraceOutcome()` called without `ResultMetrics` registered | ❌ |
| `RESULT_GEN_001` ¹ | ⚠️ Warning | Usage | `[JsonSerializable(typeof(Result))]` has no effect for converter generation | ❌ |

> ¹ **`RESULT_GEN_001`** is emitted by the Roslyn source generator in `EricksonLopez.Result.Serialization.Generators`, not by `EricksonLopez.Result.Analyzers`. To receive this diagnostic, install `EricksonLopez.Result.Serialization` (which bundles the generator). Users who only install the core package will not see this warning.

---

## 🧪 Testing & Quality

### Fluent Assertions API

`EricksonLopez.Result.Testing` simplifies unit test assertions with a declarative, chainable API:

```csharp
using EricksonLopez.Result.Testing;
using Xunit;

public class UserServiceTests
{
    [Fact]
    public void GetUser_ShouldReturnSuccess_WhenUserExists()
    {
        Result<User> result = _userService.GetUser(existingId);

        result.ShouldBeSuccess()
              .Value.Name.ShouldBe("Erickson");
    }

    [Fact]
    public async Task GetUserAsync_ShouldReturnFailure_WhenNotFound()
    {
        Result<User> result = await _userService.GetUserAsync(nonExistingId);

        result.ShouldBeFailure()
              .ShouldHaveErrorCode("User.NotFound")
              .ShouldHaveErrorType(ErrorType.NotFound);
    }
}
```

### Test Framework Adapters

When testing with specific frameworks, test failure assertions surface using native exception types:

```bash
# xUnit: Failures throw Xunit.Sdk.XunitException
dotnet add package EricksonLopez.Result.Testing.XUnit

# NUnit: Failures throw NUnit.Framework.AssertionException
dotnet add package EricksonLopez.Result.Testing.NUnit
```

### ValueTask & Async Deadlock Avoidance

The testing suite avoids standard `Task.Result` or `ValueTask.GetAwaiter().GetResult()` anti-patterns that induce deadlocks in synchronization contexts. All assertions fully support asynchronous evaluation:

```csharp
await resultTask.ShouldBeSuccessAsync();
await resultTask.ShouldBeFailureAsync();
```

### Mutation Testing & Quality Gates

The test suite enforces rigorous quality guarantees verified across CI/CD:

- **100.00% Line Coverage** and **100.00% Method Coverage** across all core modules.
- **Stryker.NET Mutation Testing** with **≥98% Global Mutation Score** (certified **100.00%** on non-excluded mutants on Core, `break: 95`). See [ADR-013](https://github.com/ericksonlopezf/dotnet-result/blob/main/docs/adr/adr-013-mutation-testing-equivalent-mutants.md) and [Mutation Score Breakdown](https://github.com/ericksonlopezf/dotnet-result/blob/main/docs/mutation-score.md).
- Zero surviving mutants across all monadic combinators, validation aggregators, and struct state machines.

---

## ⚡ Performance Benchmarks

### Result Construction Benchmark

> **Environment:** .NET 10.0.10, X64 RyuJIT AVX-512, BenchmarkDotNet v0.15.8

| Method | Mean | Allocated |
|---|---:|---:|
| `Result.Success()` | 0.000 ns | **0 B** |
| `Result.Success("value")` | 0.000 ns | **0 B** |
| `Result.Success(42)` | 0.003 ns | **0 B** |
| Implicit `TValue → Result<T>` | 0.006 ns | **0 B** |
| Implicit `Error → Result<T>` | 0.757 ns | **0 B** |
| `Result.Failure(error)` (non-generic) | 0.757 ns | **0 B** |
| `Result.Failure(error)` (generic `int`) | 0.782 ns | **0 B** |
| `Result.Success(Guid.NewGuid())` | 35.437 ns | **0 B** |

> ⚠️ `Success(Guid.NewGuid())` latency comes from `Guid.NewGuid()` itself (OS entropy), not from the `Result<T>` wrapper. The wrapper always allocates **0 B** regardless of value type.

### Pipeline Operations Benchmark

> ⚠️ The pipeline numbers below are indicative pre-release estimates. Formal committed BenchmarkDotNet measurements for `ResultPipelineBenchmarks` will be published in `benchmarks/results/` upon final hardware calibration.

| Method | Estimated Mean | Allocated |
|---|---:|---:|
| `TapOnSuccess` (success, lambda) | ~0.16 ns | **0 B** |
| `Map` (success, lambda) | ~0.77 ns | **0 B** |
| `Ensure` (success, passes) | ~0.94 ns | **0 B** |
| `Bind` (success, lambda) | ~2.12 ns | **0 B** |
| Full pipeline (3-stage, `TState`) | ~7.17 ns | 32 B |

---

## 🌐 Compatibility & Technical Matrix

### Framework & NativeAOT Support

| Package | .NET 8.0 LTS | .NET 9.0 STS | .NET 10.0 | NativeAOT | Trimmable | Notes |
|---|:---:|:---:|:---:|:---:|:---:|---|
| `EricksonLopez.Result` | ✅ | ✅ | ✅ | ✅ Certified | ✅ Certified | Zero reflection in core types |
| `EricksonLopez.Result.Generic` | ✅ | ✅ | ✅ | ✅ Certified | ✅ Certified | Struct layout with zero reflection |
| `EricksonLopez.Result.Maybe` | ✅ | ✅ | ✅ | ✅ Certified | ✅ Certified | Struct layout with zero reflection |
| `EricksonLopez.Result.AspNetCore` | ✅ | ✅ | ✅ | ✅ Certified | ✅ Certified | STJ source generator compatible |
| `EricksonLopez.Result.OpenApi` | ✅ | ✅ | ✅ | ✅ Certified | ✅ Certified | Minimal API OpenAPI metadata |
| `EricksonLopez.Result.OpenTelemetry` | ✅ | ✅ | ✅ | ✅ Certified | ✅ Certified | Native BCL Activity & Metrics |
| `EricksonLopez.Result.Serialization` | ✅ | ✅ | ✅ | ⚠️ Partial | ⚠️ Partial | Use explicit `ResultOfTJsonConverter<T>` for AOT |
| `EricksonLopez.Result.Serialization.Generators` | `netstandard2.0` | `netstandard2.0` | `netstandard2.0` | ✅ Tool | ✅ Tool | Roslyn Source Generator (compile time) |
| `EricksonLopez.Result.FluentValidation` | ✅ | ✅ | ✅ | ✅ Certified | ✅ Certified | No reflection in mapping layer |
| `EricksonLopez.Result.MediatR` | ✅ | ✅ | ✅ | ❌ No | ❌ No | MediatR uses dynamic reflection ([ADR-018](https://github.com/ericksonlopezf/dotnet-result/blob/main/docs/adr/adr-018-result-mediatr-non-aot-governance-and-deprecation-roadmap.md)) |
| `EricksonLopez.Result.Testing` | ✅ | ✅ | ✅ | ❌ Test Only | ❌ Test Only | Test assertions library |
| `EricksonLopez.Result.Testing.XUnit` | ✅ | ✅ | ✅ | ❌ Test Only | ❌ Test Only | xUnit test runner adapter |
| `EricksonLopez.Result.Testing.NUnit` | ✅ | ✅ | ✅ | ❌ Test Only | ❌ Test Only | NUnit test runner adapter |
| `EricksonLopez.Result.Analyzers` | `netstandard2.0` | `netstandard2.0` | `netstandard2.0` | ✅ Tool | ✅ Tool | Roslyn Analyzer (runs inside compiler) |

### HTTP Status Code Mapping Matrix

| `ErrorType` | HTTP Status Code | RFC 9457 Title | Typical Domain Scenario |
|---|:---:|---|---|
| `Validation` | **400 Bad Request** | Bad Request | Input validation failure, invariant violation |
| `Unauthorized` | **401 Unauthorized** | Unauthorized | Missing or expired authentication token |
| `Forbidden` | **403 Forbidden** | Forbidden | Insufficient permissions for requested resource |
| `NotFound` | **404 Not Found** | Not Found | Entity or aggregate root does not exist |
| `Conflict` | **409 Conflict** | Conflict | Concurrency conflict, duplicate unique key |
| `Domain` | **422 Unprocessable Entity** | Domain Rule Violation | Business rule violation or domain entity invariant failure |
| `Unavailable` | **503 Service Unavailable** | Service Unavailable | Downstream dependency or circuit breaker tripped |
| `Infrastructure` | **500 Internal Server Error** | Infrastructure Error | Database, file system, or network I/O connectivity failure |
| `Failure` | **500 Internal Server Error** | Operation Failed / Internal Server Error | General unclassified domain or operation failure |
| `Unexpected` | **500 Internal Server Error** | Internal Server Error | Unhandled critical system exception or runtime fault |
| `Custom` | **500 Internal Server Error** | Application Error | Application-defined error type (status code configurable) |

---

> 🛡️ **Target Framework & Lifecycle Policy**: First-class multi-targeting across `.NET 10` (Modern LTS), `.NET 9` (STS), and `.NET 8` (Enterprise LTS) — along with `.NET Standard 2.0` for Roslyn analyzers and source generators — is actively maintained. Full backward compatibility is guaranteed until Microsoft officially reaches End-of-Life (EOL) for .NET 8 and .NET 9 in November 2026, at which milestone the ecosystem will transition to .NET 10 and .NET 11.

## 🏛️ Architecture & Design Principles

### Railway-Oriented Programming (ROP) Flow

```mermaid
flowchart TD
    Start[Input Data] --> Ensure{Ensure(predicate)}
    Ensure -- Pass --> Bind[Bind(Async Service Call)]
    Ensure -- Fail --> FailTrack[Return Result.Failure]
    
    Bind -- Success --> Map[Map(To DTO)]
    Bind -- Failure --> FailTrack
    
    Map --> Tap[Tap(Side Effects / Logging)]
    Tap --> Match{Match / ToHttpResult}
    
    Match -- Ok --> OkRes[200 OK / 204 NoContent]
    Match -- Error --> ProbRes[ProblemDetails RFC 9457]
    
    style FailTrack fill:#ff9999,stroke:#333,stroke-width:2px
    style OkRes fill:#99ccff,stroke:#333,stroke-width:2px
    style ProbRes fill:#ffcc99,stroke:#333,stroke-width:2px
```

### Struct Memory Layout & State Lifecycle

`Result` and `Result<TValue>` use a 3-state discriminant (`Uninitialized = 0`, `Success = 1`, `Failure = 2`) guaranteeing zero default initialization bugs:

```mermaid
stateDiagram-v2
    [*] --> Uninitialized: default(Result) / default(Result<T>)
    Uninitialized --> Success: Result.Success() / Result.Success(value)
    Uninitialized --> Failure: Result.Failure(error)
    
    state Success {
        IsSuccess: true
        IsFailure: false
        Value: Valid Instance
        Error: Throws InvalidOperationException
    }
    
    state Failure {
        IsSuccess: false
        IsFailure: true
        Value: Throws InvalidOperationException
        Error: Valid Error Instance
    }

    state Uninitialized {
        IsUninitialized: true
        Value: Throws InvalidOperationException
        Error: Returns WellKnownErrors.UninitializedError (sentinel, does NOT throw)
    }
```

---

## 🛡️ Best Practices & Anti-Patterns

### Recommended vs Avoid

| Scenario | ❌ Avoid | ✅ Recommended |
|---|---|---|
| **Domain Failures** | Throwing `NotFoundException` or `ValidationException` | Returning `Result<T>` with semantic `Error.NotFound()` |
| **High-Throughput Pipelines** | Lambda capturing outer variables (`.Ensure(x => x > limit)`) | Passing `TState` via static lambda (`.Ensure(limit, static (l, x) => x > l)`) |
| **Unwrapping Values** | Accessing `.Value` directly without checking `IsSuccess` | Using `.Match()`, `.TryGetValue()`, or tuple destructuring |
| **Uninitialized Structs** | Returning `default(Result)` or `default(Result<T>)` | Returning `Result.Success(...)` or `Result.Failure(...)` (Enforced by `RESULT012`) |
| **Error Collections** | `new HashSet<Error>()` with default equality | `new HashSet<Error>(ErrorEqualityComparer.Strict)` (Enforced by `RESULT007`) |
| **Minimal APIs** | Using `AddResultEndpointFilter()` without `.Produces<T>()` | Calling `.ToHttpResult()` or pairing with `.ProducesResult<T>()` (Enforced by `RESULT008`) |
| **Compound Errors** | Chaining `.WithMetadata()` 3+ times consecutively | Using `ErrorBuilder` or passing a dictionary (Enforced by `RESULT005`) |

---

## ⚠️ Troubleshooting & Common Pitfalls

> [!CAUTION]
> Review the following common pitfalls that can produce unexpected runtime behavior:

### 1. `default(Result)` evaluates silently as `false` in boolean context
`Result` and `Result<T>` are structs. An uninitialized struct (`default`) has `ResultState.Uninitialized`. Both `IsSuccess` and `IsFailure` evaluate to `false`. Always check `res.IsUninitialized` or avoid returning default structs (enforced by Roslyn analyzer `RESULT012`).

### 2. `AddResultEndpointFilter()` requires `.Produces<T>()` or `.ProducesResult<T>()`
`AddResultEndpointFilter()` unwraps results dynamically via `IResultOutcome`. To ensure Swagger/OpenAPI generators infer the proper response DTO schema instead of `object`, always chain `.ProducesResult<T>()` (enforced by `RESULT008`).

### 3. `HashSet<Error>` deduplicates errors with identical semantic fields
By default, `Error.Equals()` compares only the 5 domain fields (`Code`, `Description`, `Type`, `Severity`, `Retryability`). If collections must distinguish errors by `TraceId`, `CorrelationId`, or `Metadata`, explicitly provide `ErrorEqualityComparer.Strict` (enforced by `RESULT007`).

### 4. `ErrorBuilder` mutation discarded when return value is not assigned
`ErrorBuilder` is a stack-allocated `readonly struct`. Calling `.WithInnerError()` or `.WithMetadata()` returns a mutated copy. Discarding the return value loses the added data (enforced as a compile error by `RESULT003`).

---

## 🌐 Part of the EricksonLopez Ecosystem

`EricksonLopez.Result` is a foundational component of the **EricksonLopez** open-source library ecosystem:

- 🧱 [**EricksonLopez.SharedKernel**](https://github.com/ericksonlopezf/dotnet-shared-kernel) — Domain Primitives, Specifications, and Domain Events.
- ⚡ **EricksonLopez.Result** — High-Performance Struct-Based Result Pattern & Telemetry.
- 🔍 [**EricksonLopez.Specification**](https://github.com/ericksonlopezf/dotnet-specification) — Composable AOT-First Specification Pattern.

---

## 🤝 Contributing

We welcome contributions, bug reports, documentation improvements, and feature suggestions!

### Development Setup

1. **Prerequisites:** [.NET 8.0 / 9.0 / 10.0 SDK](https://dotnet.microsoft.com/download), Git, and an IDE (Rider, Visual Studio 2022+, or VS Code).
2. **Build Solution:**
   ```bash
   dotnet build EricksonLopez.Result.slnx
   ```
3. **Run All Tests:**
   ```bash
   dotnet test EricksonLopez.Result.slnx --configuration Release
   ```
4. **Run Mutation Tests:**
   ```bash
   # Core Result library
   cd tests/EricksonLopez.Result.Tests && dotnet stryker

   # Specific subsystems (from repository root using subsystem config):
   dotnet stryker --config-file stryker-aspnetcore-config.json
   dotnet stryker --config-file stryker-mediatr-config.json
   dotnet stryker --config-file stryker-fluentvalidation-config.json
   ```
   > See repository root for all 18 `stryker-*-config.json` configuration profiles and [`docs/mutation-score.md`](https://github.com/ericksonlopezf/dotnet-result/blob/main/docs/mutation-score.md).

Please read our [Contributing Guide](https://github.com/ericksonlopezf/dotnet-result/blob/main/CONTRIBUTING.md) and [Code of Conduct](https://github.com/ericksonlopezf/dotnet-result/blob/main/CODE_OF_CONDUCT.md) before submitting pull requests.

---

## 📄 License

Distributed under the [MIT License](https://github.com/ericksonlopezf/dotnet-result/blob/main/LICENSE). Copyright © 2026 Erickson Lopez.