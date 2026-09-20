# Architecture & Design Overview

This document describes the architectural principles, structural models, telemetry integration, and execution pipelines of the `EricksonLopez.Result` ecosystem.

---

## 1. Clean Architecture & Result Flow

In Clean Architecture and Domain-Driven Design (DDD), domain rules, validation logic, and business invariants communicate expected failures as domain values rather than control-flow exceptions.

```mermaid
sequenceDiagram
    participant Client as API Client / Consumer
    participant API as ASP.NET Core Minimal API
    participant App as Application Service
    participant Domain as Domain Model
    participant Telemetry as OpenTelemetry Activity

    Client->>API: POST /orders
    API->>Telemetry: Start Activity ("CreateOrder")
    API->>App: Execute(CreateOrderCommand)
    App->>Domain: Order.Create(id, items)
    
    alt Invariant Passed (Success Path)
        Domain-->>App: Result<Order>.Success(order)
        App-->>API: Result<OrderDto>.Success(dto)
        API->>Telemetry: RecordResult(Success)
        API-->>Client: 200 OK (JSON OrderDto)
    else Domain Rule Failed (Expected Failure Path)
        Domain-->>App: Result<Order>.Failure(Error.Validation(...))
        App-->>API: Result<OrderDto>.Failure(Error)
        API->>Telemetry: RecordResult(Error status & tags)
        API-->>Client: 400 Bad Request (RFC 9457 ProblemDetails)
    end
```

---

## 2. Railway-Oriented Programming (ROP) Pipeline Flow

Monadic chaining allows operations to execute in sequence on the **Success Track**, while automatically short-circuiting to the **Failure Track** if any step evaluates to a failure.

```mermaid
flowchart TD
    Start[Input Data] --> Ensure{Ensure(predicate)}
    Ensure -- Pass --> Bind[Bind(Async Service Call)]
    Ensure -- Fail --> FailTrack[Return Result.Failure]
    
    Bind -- Success --> Map[Map(To DTO)]
    Bind -- Failure --> FailTrack
    
    Map --> Tap[Tap(Side Effects / Cache / Logging)]
    Tap --> Match{Match / ToHttpResult}
    
    Match -- Success --> OkRes[200 OK / Success Value]
    Match -- Failure --> ProbRes[RFC 9457 ProblemDetails]
    
    style FailTrack fill:#ff9999,stroke:#333,stroke-width:2px
    style OkRes fill:#99ccff,stroke:#333,stroke-width:2px
    style ProbRes fill:#ffcc99,stroke:#333,stroke-width:2px
```

---

## 3. Readonly Struct Memory & State Layout

Both `Result` and `Result<TValue>` are implemented as `readonly struct` value types with `[StructLayout(LayoutKind.Auto)]` to eliminate heap allocation overhead on the success path.

```mermaid
stateDiagram-v8
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

### Memory Footprint Comparison

| Component | Type | Heap Allocation |
|---|---|---|
| `Result` Envelope | `readonly struct` | **0 bytes** (allocated on stack or CPU register) |
| `Result<TValue>` Envelope | `readonly struct` | **0 bytes** (stack / register) |
| `Result<TValue, TError>` | `readonly struct` | **0 bytes** (strongly-typed compile-time error) |
| `Maybe<T>` Envelope | `readonly struct` | **0 bytes** (optional DDD entity wrapper) |
| `TValue` (Value Type e.g., `int`, `Guid`) | `struct` | **0 bytes** (stored inline within struct) |
| `TValue` (Reference Type e.g., `User`) | `class` | Allocated on heap when instantiated |
| `Error` | `sealed class` | Heap allocated on failure path only (0 allocation on success) |
| `ErrorBuilder` | `readonly struct` | **0 bytes** (stack-allocated builder for compound errors) |

---

## 4. ASP.NET Core HTTP Mapping Architecture

`EricksonLopez.Result.AspNetCore` integrates with ASP.NET Core via `.ToHttpResult()` and `ResultEndpointFilter`.

```mermaid
flowchart LR
    Res[Result / Result<T>] --> Filter[ResultEndpointFilter / ToHttpResult]
    Filter --> Check{IsSuccess?}
    Check -- Yes --> HTTP200[Results.Ok(value) / Results.NoContent()]
    Check -- No --> MapError[Map ErrorType to HTTP Code]
    
    MapError --> V[Validation -> 400 Bad Request]
    MapError --> U[Unauthorized -> 401 Unauthorized]
    MapError --> F[Forbidden -> 403 Forbidden]
    MapError --> NF[NotFound -> 404 Not Found]
    MapError --> C[Conflict -> 409 Conflict]
    MapError --> S[Unavailable -> 503 Service Unavailable]
    MapError --> E[Failure/Unexpected -> 500 Server Error]

    V & U & F & NF & C & S & E --> ProbDetails[RFC 9457 ProblemDetails Payload]
```

---

## 5. OpenTelemetry Activity & Metrics Pipeline

`EricksonLopez.Result.OpenTelemetry` attaches diagnostic attributes directly to OpenTelemetry `ActivitySource` spans and updates runtime `Meter` statistics:

```mermaid
sequenceDiagram
    participant Code as Domain / Service Layer
    participant Activity as OpenTelemetry Activity
    participant Metrics as ResultMetrics (Meter)
    participant Exporter as OTLP Collector / Jaeger / Prometheus

    Code->>Activity: TraceOutcome(opName, activity)
    alt Failure Result
        Activity->>Activity: SetStatus(ActivityStatusCode.Error, error.Description)
        Activity->>Activity: SetTag("ericksonlopez.result.error.code", error.Code)
        Activity->>Activity: SetTag("error.type", error.Type)
        Activity->>Activity: SetTag("ericksonlopez.result.error.severity", error.Severity)
    else Success Result
        Activity->>Activity: SetStatus(ActivityStatusCode.Ok)
    end
    
    Code->>Metrics: TrackSuccess(opName) / TrackFailure(opName, error)
    Metrics->>Metrics: Counter.Add(1, tags)
    Metrics->>Exporter: Export Telemetry Data
```

---

## 6. System.Text.Json Serialization Architecture

`EricksonLopez.Result.Serialization` utilizes custom polymorphic converter factories and NativeAOT `JsonSerializerContext` generation:

```mermaid
flowchart TD
    Obj[Result / Result<T> / Error / Maybe<T>] --> Converter{Converter Context}
    Converter -- Explicit Converters --> Converters[ResultJsonConverter / ResultOfTJsonConverter<T>]
    Converter -- NativeAOT / Trimming --> Context[ResultJsonSerializerContext / SourceGen]
    
    Converters --> JSON[JSON String Payload]
    Context --> JSON
    
    JSON --> Deserializer[JsonSerializer.Deserialize]
    Deserializer --> Obj
```

---

## 7. Asynchronous State Machine Wrapper Pattern (ADR-008)

To maximize compatibility with asynchronous code coverage and instrumentation tools (Coverlet, OpenTelemetry automatic instrumentation) and prevent testing deadlocks when dealing with `ValueTask`, the library utilizes a state-machine splitting architectural pattern.

When the compiler generates an `IAsyncStateMachine` for an `async ValueTask` method, instrumentation tools sometimes insert breakpoints in branches that lock the test runner's thread context, especially in Release mode. 

To resolve this and achieve robust test coverage, asynchronous methods are separated into a public wrapper that evaluates synchronicity, and a private `async Task` method that executes the slow path:

```csharp
// 1. The Public Wrapper (No 'async' modifier, returns ValueTask)
public static ValueTask<Result> Map(this ValueTask<Result> task, Func<Result> next)
{
    // Fast path: if the ValueTask is already completed, process synchronously.
    // This avoids creating an IAsyncStateMachine allocation entirely.
    if (task.IsCompletedSuccessfully)
    {
        return new ValueTask<Result>(task.Result.Map(next));
    }
    
    // Slow path: delegate to the core Task method
    return new ValueTask<Result>(MapCore(task, next));
}

// 2. The Private Core Method (Generates the IAsyncStateMachine)
private static async Task<Result> MapCore(ValueTask<Result> task, Func<Result> next)
{
    var result = await task.ConfigureAwait(false);
    return result.Map(next);
}
```

---

## 8. Project Dependency Graph

The following diagram shows the internal project references across all **20 ecosystem packages**:

```mermaid
graph TD
    Core["EricksonLopez.Result<br/>(net8.0; net9.0; net10.0)"]
    Analyzers["EricksonLopez.Result.Analyzers<br/>(netstandard2.0)"]
    Generic["EricksonLopez.Result.Generic<br/>(net8.0; net9.0; net10.0)"]
    Maybe["EricksonLopez.Result.Maybe<br/>(net8.0; net9.0; net10.0)"]
    AspNetCore["EricksonLopez.Result.AspNetCore<br/>(net8.0; net9.0; net10.0)"]
    OpenApi["EricksonLopez.Result.OpenApi<br/>(net8.0; net9.0; net10.0)"]
    OTel["EricksonLopez.Result.OpenTelemetry<br/>(net8.0; net9.0; net10.0)"]
    Serialization["EricksonLopez.Result.Serialization<br/>(net8.0; net9.0; net10.0)"]
    Generators["EricksonLopez.Result.Serialization.Generators<br/>(netstandard2.0)"]
    DomainErrors["EricksonLopez.Result.DomainErrors.Generators<br/>(netstandard2.0)"]
    EFCore["EricksonLopez.Result.EntityFrameworkCore<br/>(net8.0; net9.0; net10.0)"]
    Polly["EricksonLopez.Result.Polly<br/>(net8.0; net9.0; net10.0)"]
    MassTransit["EricksonLopez.Result.MassTransit<br/>(net8.0; net9.0; net10.0)"]
    Dapr["EricksonLopez.Result.Dapr<br/>(net8.0; net9.0; net10.0)"]
    Grpc["EricksonLopez.Result.Grpc<br/>(net8.0; net9.0; net10.0)"]
    Testing["EricksonLopez.Result.Testing<br/>(net8.0; net9.0; net10.0)"]
    TestingXUnit["EricksonLopez.Result.Testing.XUnit<br/>(net8.0; net9.0; net10.0)"]
    TestingNUnit["EricksonLopez.Result.Testing.NUnit<br/>(net8.0; net9.0; net10.0)"]
    FluentVal["EricksonLopez.Result.FluentValidation<br/>(net8.0; net9.0; net10.0)"]
    MediatR["EricksonLopez.Result.MediatR<br/>(net8.0; net9.0; net10.0)"]

    Core -->|Bundled Analyzer| Analyzers
    Generic --> Core
    Maybe --> Core
    AspNetCore --> Core
    OpenApi --> Core
    OTel --> Core
    OTel --> Generators
    Serialization --> Core
    Serialization --> Generators
    EFCore --> Core
    EFCore --> Maybe
    Polly --> Core
    MassTransit --> Core
    Dapr --> Core
    Grpc --> Core
    Testing --> Core
    TestingXUnit --> Testing
    TestingNUnit --> Testing
    FluentVal --> Core
    MediatR --> Core

    style Core fill:#512BD4,stroke:#333,color:#fff
    style Analyzers fill:#E8DAEF,stroke:#333
    style Generators fill:#E8DAEF,stroke:#333
    style DomainErrors fill:#E8DAEF,stroke:#333
    style Testing fill:#D5F5E3,stroke:#333
    style TestingXUnit fill:#D5F5E3,stroke:#333
    style TestingNUnit fill:#D5F5E3,stroke:#333
```

---

## 9. Ecosystem Extensions Architecture

### 9.1 Entity Framework Core Adapter (`EricksonLopez.Result.EntityFrameworkCore`)
Provides exception-safe persistence pipeline operations mapping EF Core concurrency conflicts and transient timeouts into domain `Result<T>` values (ADR-023):
- `SaveChangesAsyncToResult`: Traps `DbUpdateConcurrencyException` $\rightarrow$ `Error.Conflict`, `DbUpdateException` $\rightarrow$ `Error.Failure`, `TimeoutException` $\rightarrow$ `Error.Unavailable(Transient)`. Preserves cooperative `OperationCanceledException`.
- Query extensions on `IQueryable<T>` (`FirstOrDefaultToResultAsync`, `SingleOrDefaultToResultAsync`, `ToListToResultAsync`) return clean result envelopes without throwing on missing records.

```mermaid
flowchart LR
    EFQuery[IQueryable Operation] --> Exec{Execute}
    Exec -- Row Found --> Success[Result.Success(Entity)]
    Exec -- Missing --> NotFound[Result.Failure(NotFoundError)]
    Exec -- Concurrency Conflict --> Conflict[Result.Failure(ConflictError)]
    Exec -- Timeout --> Timeout[Result.Failure(UnavailableError)]
```

### 9.2 Polly v8 Resilience Pipeline (`EricksonLopez.Result.Polly`)
Integrates directly with Polly v8's zero-allocation `ResiliencePipeline` architecture (ADR-024):
- Exception-free retry evaluation: Evaluates `result.IsFailure && result.Error.Retryability == ErrorRetryability.Transient`. Permanent errors abort immediately without useless retry delays.
- `TState` overloads on `ExecuteResult` and `ExecuteResultAsync` eliminate delegate closure allocations in high-throughput network calls.

### 9.3 MassTransit Distributed Message Faults (`EricksonLopez.Result.MassTransit`)
Coordinates asynchronous messaging boundaries and event-driven architectures (ADR-025):
- `ResultFault`: Immutable, transport-safe contract preserving `Code`, `Description`, `Type`, `Severity`, `Retryability`, `CorrelationId`, `TraceId`, and `Metadata` across broker networks (RabbitMQ, Azure Service Bus, Amazon SQS).
- `ResultConsumeFilter<TMessage>`: Intercepts consumer results, publishes faults, and prevents poison-message retry storms on permanent failures.

### 9.4 Dapr Distributed State & Pub/Sub Integration (`EricksonLopez.Result.Dapr`)
Integrates with Dapr distributed application runtime sidecars (ADR-026):
- **State Management**: `GetStateWithResultAsync`, `SaveStateWithResultAsync`, and `DeleteStateWithResultAsync` map ETag concurrency conflicts directly to `Error.Conflict`, missing keys to `Error.NotFound`, and sidecar connectivity failures to `Error.Unavailable(Transient)`.
- **Pub/Sub Subscriptions**: `ToDaprTopicResult()` translates domain `Result` outcomes into canonical `TopicEventResponse`: `Success` on happy path, `Drop` on permanent non-retryable domain rejections (`Validation`, `Forbidden`), and `Retry` on transient errors (`Unavailable`).

```mermaid
flowchart LR
    DaprEvt[TopicEvent Event] --> Handle{Process Handler}
    Handle -- Success --> TS[TopicEventResponse.Success]
    Handle -- Transient Failure --> TR[TopicEventResponse.Retry]
    Handle -- Permanent Failure --> TD[TopicEventResponse.Drop]
```

### 9.5 gRPC Server Interceptor & Status Code Mapping (`EricksonLopez.Result.Grpc`)
Bridges gRPC RPC boundaries and HTTP/2 streaming protocols (ADR-027):
- **Server Interceptor**: `ResultServerInterceptor` intercepts unary RPC service methods, automatically translating domain `ErrorType` failures into canonical gRPC `StatusCode` values with rich diagnostic trailer metadata (`x-error-code`, `x-error-type`, `x-error-severity`, `x-trace-id`, `x-correlation-id`).
- **Client Extensions**: `ToResult()` / `ToResultAsync()` unpacks client-side `RpcException` responses back into strongly-typed `Result<T>` envelopes.

```mermaid
flowchart LR
    DomainErr[Domain Error] --> Interceptor[ResultServerInterceptor]
    Interceptor --> MapStatus{Map ErrorType}
    MapStatus --> V[Validation -> InvalidArgument]
    MapStatus --> NF[NotFound -> NotFound]
    MapStatus --> C[Conflict -> AlreadyExists/Aborted]
    MapStatus --> U[Unauthorized -> Unauthenticated]
    MapStatus --> F[Forbidden -> PermissionDenied]
    MapStatus --> S[Unavailable -> Unavailable]
    MapStatus --> E[Failure/Unexpected -> Internal]
    V & NF & C & U & F & S & E --> RpcEx[RpcException + Trailers]
```

---

## 10. Roslyn Analyzers & Source Generators Architecture

The repository includes compiler tooling projects targeting `netstandard2.0`:

### `EricksonLopez.Result.Analyzers`

Bundled directly into `EricksonLopez.Result` (as `OutputItemType="Analyzer"`).

| Diagnostic ID | Category | Severity | Description | Code Fix Available |
|---|---|---|---|:---:|
| `RESULT001` | Performance | Warning | Large value type (>32 bytes) used as `Result<T>` — excessive copying overhead. | No |
| `RESULT003` | Usage | **Error** | `ErrorBuilder.With*()` return value discarded — mutated struct copy is lost. | `ErrorBuilderDiscardedReturnCodeFix` |
| `RESULT004` | Performance | Warning | Lambda expression captures local variables in Result pipeline (closure allocation). | `ClosureCaptureCodeFix` |
| `RESULT005` | Performance | Warning | `Error.WithMetadata()` / `ErrorBuilder.WithMetadata()` chained consecutively 3+ times. | No |
| `RESULT006` | Performance | Warning | `ErrorBuilder.WithInnerError()` chained consecutively 2+ times without batching. | No |
| `RESULT007` | Reliability | Warning | `HashSet<Error>`, `Distinct()`, `GroupBy()`, or `ToHashSet()` used on `Error` without `ErrorEqualityComparer.Strict`. | `HashSetErrorEqualityCodeFix` |
| `RESULT008` | Usage | Warning | Endpoint returning `Result<T>` uses `AddResultEndpointFilter()` without `.Produces<T>()`. | No |
| `RESULT009` | Security | Warning | `IncludeDescription = true` set without environment guard — potential information disclosure. | No |
| `RESULT010` | Security | Warning | `ResultExceptionBehavior` default error factory may expose internal exception type names. | No |
| `RESULT012` | Usage | Warning | Method returning `default(Result)` or `default(Result<T>)` — uninitialized state bug. | `DefaultResultReturnCodeFix` |
| `RESULT013` | Usage | Warning | Avoid implicit bool conversion of `Result` in condition contexts. | `BoolOperatorUsageCodeFix` |
| `RESULT_OTEL_001` | Observability | Info | `TraceOutcome()` called without `ResultMetrics` registered. | No |

### `EricksonLopez.Result.Serialization.Generators`

Incremental Roslyn Source Generator that produces:
- AOT-safe `ResultOfTJsonConverter<T>` registrations at compile time.
- Compile-time assembly version constants for the OpenTelemetry package (`ResultMetricsVersionGenerator`).
- Diagnostic `RESULT_GEN_001` (Warning) when `[JsonSerializable(typeof(Result))]` is used on non-generic `Result`.

### `EricksonLopez.Result.DomainErrors.Generators`

Incremental Roslyn Source Generator that monitors `*.errors.json` additional files (ADR-022):
- Emits compile-time `public static partial class` definitions with strongly-typed error factory methods.
- Certified 100% Native AOT compatible with zero runtime reflection overhead.

---

## 11. Known Limitations & Mitigations

### 1. `ResultEndpointFilter` Boxing & OpenAPI Type Metadata
When using `ResultEndpointFilter`, the filter receives `IResultOutcome`, which boxes the struct result on each request and emits `Ok<object?>` internally.
- **Mitigation for OpenAPI:** Call `.ProducesResult<T>()` on the endpoint builder.
- **Mitigation for Zero-Allocation:** Call `.ToHttpResult()` directly inside the endpoint handler.

### 2. Metadata Serialization Lossiness
`Error.Metadata` serializes numeric primitives into native JSON numbers. Deserialization restores numbers as `long` or `double`.
- **Mitigation:** Use typed DTO objects or `ErrorEqualityComparer.Default` for semantic equivalence rather than strict type-identity assertions after JSON round-tripping.
