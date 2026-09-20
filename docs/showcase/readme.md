# Progressive Showcase & Executable Reference Guide

Welcome to the official progressive showcase and architectural reference for the **`EricksonLopez.Result`** ecosystem.

This showcase represents the **living, executable documentation** of the public API surface. Every example here is guaranteed to compile, execute without warnings, and reflect the real, uninvented APIs available in the library.

---

## Pedagogical Structure & Complexity Levels

The showcase is organized across **11 progressive levels of complexity**, transitioning from foundational philosophy to high-throughput enterprise architectures:

| Level | Topic | Dimension Covered | Executable C# Samples |
|---|---|---|---|
| **[Level 00 — Conceptual](level-00-conceptual.md)** | Architectural Philosophy | Railway-Oriented Programming, Zero-Alloc guarantees, tradeoffs, and peer benchmarks | Architectural Docs & Benchmarks |
| **[Level 01 — Quick Start](level-01-quickstart.md)** | Core Primitives & Quick Start | Installation, minimal setup, `Result`, `Result<T>`, unwrapping safely | `01_BasicCreation.cs`, `03_MatchingAndExecuting.cs` |
| **[Level 02 — Complete Configuration](level-02-complete-configuration.md)** | Full Diagnostics & Builders | `ErrorBuilder`, `ErrorType`, `ErrorSeverity`, `ErrorRetryability`, metadata, correlation | `02_ErrorsAndBuilders.cs`, `12_ErrorEqualityAndMutation.cs` |
| **[Level 03 — Real-World Use Cases](level-03-real-world-usecases.md)** | Domain Business Scenarios | User registration, payment processing, inventory reservation, order checkout | `04_MappingAndChaining.cs`, `11_GenericResult.cs` |
| **[Level 04 — Advanced Integration](level-04-advanced-integration.md)** | Functional Composition | Monadic binding (`Bind`), `Ensure`, `Combine`, `Merge`, `ValidateAll`, LINQ queries | `06_Combining.cs`, `09_CumulativeValidation.cs`, `13_LinqIntegration.cs`, `19_CombineAndMergeAdvanced.cs` |
| **[Level 05 — Processing](level-05-processing.md)** | Asynchronous & Background Work | `Task<Result>`, `ValueTask<Result>`, `TryAsync`, cooperative `CancellationToken`, batching | `07_AsyncOperations.cs`, `15_AdvancedAsyncOperations.cs`, `20_WellKnownErrorsAndTry.cs` |
| **[Level 06 — Error Handling](level-06-error-handling.md)** | Resilient Error Recovery | `Recover`, `MapError`, `MapFailure`, transient vs permanent classification, distributed retry | `17_RecoverAndErrorTransformation.cs`, `20_WellKnownErrorsAndTry.cs` |
| **[Level 07 — Scalability](level-07-scalability.md)** | Extreme High-Throughput | Zero-allocation `TState` state passing, `in Result<T>` ref semantics, NativeAOT trimming | `08_StatePassingAllocFree.cs`, `14_SyncExtensions.cs` |
| **[Level 08 — Customization](level-08-customization.md)** | Extensibility & Customization | `IResultOutcome`, `ErrorEqualityComparer.Default`/`Strict`, custom error types | `12_ErrorEqualityAndMutation.cs`, `21_AdvancedApiCoverage.cs`, `34_ComprehensiveApiCoverageShowcase.cs` |
| **[Level 09 — Extensions](level-09-extensions.md)** | Official Ecosystem Integrations | ASP.NET Core, FluentValidation, MediatR, OpenTelemetry, System.Text.Json, Polly, EF Core, MassTransit, Testing Adapters, DomainErrors Generator, Dapr, gRPC | `22_SerializationShowcase.cs` to `34_ComprehensiveApiCoverageShowcase.cs`, Dedicated `samples/*` |
| **[Level 10 — Enterprise Architecture](level-10-enterprise-architecture.md)** | Enterprise Architecture | Clean Architecture, DDD boundaries, CQRS handlers, RFC 9457 ProblemDetails, W3C tracing, gRPC & Dapr | Complete Ecosystem Solutions |

---

## Running the Showcase Locally

To execute the official reference showcase console application, run:

```bash
dotnet run --project samples/EricksonLopez.Result.Sample/EricksonLopez.Result.Sample.csproj
```

All 34 progressive reference modules execute sequentially, validating success and failure tracks, alloc-free pipelines, serialization round-trips, resilience retries, database queries, OpenTelemetry span enrichments, Dapr state store operations, gRPC status code mappings, and compile-time generated domain error catalogs.
