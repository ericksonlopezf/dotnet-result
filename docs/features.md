# EricksonLopez.Result Feature Catalog & Specifications

> **Ecosystem:** `EricksonLopez.Result` | **Status:** Enterprise Production Ready | **Language:** English

---

## 1. Core Primitives (`EricksonLopez.Result`)

### 1.1 `Result` (Non-Generic)
- Value-type envelope (`readonly struct Result`) representing operation success or failure without return payload.
- Methods: `Result.Success()`, `Result.Failure(Error)`.
- Properties: `IsSuccess`, `IsFailure`, `IsUninitialized`, `Error`.

### 1.2 `Result<TValue>` (Generic)
- Value-type envelope (`readonly struct Result<TValue>`) encapsulating a value of type `TValue` or an `Error`.
- Methods: `Result<TValue>.Success(TValue)`, `Result<TValue>.Failure(Error)`.
- Implicit conversions from `TValue` (to `Result<TValue>.Success`) and `Error` (to `Result<TValue>.Failure`).
- Safe value unwrapping: `TryGetValue()`, `GetValueOrDefault()`, `Match()`, `Execute()`, `DiscardValue()`.

### 1.3 `Error` Domain Model
- Immutable, sealed model with rich multi-dimensional taxonomy:
  - `Code`: Unique string identifier (e.g. `"User.NotFound"`).
  - `Description`: Human-readable explanation.
  - `Type`: Semantic category (`Validation`, `Unauthorized`, `Forbidden`, `NotFound`, `Conflict`, `Unavailable`, `Failure`, `Unexpected`).
  - `Severity`: Impact level (`Info`, `Warning`, `Error`, `Critical`).
  - `Retryability`: `ErrorRetryability` enum (`Transient`, `Permanent`, `Undetermined`).
  - `DescriptionKey`: Optional localization key for client/i18n translation.
  - `TraceId`: Lazy ambient distributed trace correlation from `Activity.Current`.
  - `CorrelationId`: Optional business operation correlation ID.
  - `Metadata`: Immutable dictionary of arbitrary diagnostic metadata.
  - `InnerErrors`: `ImmutableArray<Error>` of child errors for composite failures.

### 1.4 Monadic Combinator Operators
- **`MapFailure`**: Maps the `Error` from a failure to a value of type `TOut`, or returns `successDefault` on success. Provides a `TState` overload to avoid closure allocation.
- **`Map`**: Transforms `Result<TIn>` to `Result<TOut>` synchronously or asynchronously (`Task` / `ValueTask`).
- **`MapError`**: Transforms `Error` into a specialized or sanitized error representation.
- **`Bind`**: Chains operations returning `Result<TOut>`, short-circuiting on failure.
- **`TapOnSuccess`**: Executes side effects on success (e.g., logging, metrics) without altering the value.
- **`TapOnFailure`**: Executes side effects on failure without altering the error.
- **`Ensure`**: Validates predicates against success values, converting to failure if predicate fails.
- **`Match`**: Resolves result to `TResult` via `Func<TValue, TResult>` (success) and `Func<Error, TResult>` (failure).
- **`Execute`**: Branching execution of action delegates without return value.
- **`Inspect`**: Unconditional inspection of result state via `Action<Result<T>>`.
- **`Recover`**: Intercepts a failure and provides an alternative fallback computation.
- **`ValidateAll`**: Evaluates spans of validation functions and combines failures using `ArrayPool<Error>`.
- **`ValidateAllAsync`**: Async variant of `ValidateAll` accepting `IReadOnlyList<Func<CancellationToken, Task<Result>>>` or `ValueTask<Result>` validators. Accumulates all failures and passes `CancellationToken` to each validator.
- **`TryAsyncValue`**: Exception-to-`Result` adapter returning `ValueTask<Result>` or `ValueTask<Result<T>>`. Prefer over the `Task`-based `TryAsync` variant in end-to-end `ValueTask` pipelines to avoid unnecessary state machine allocation.
- **`Combine`**: Combines multiple results into tuple results or aggregates failures.

### 1.5 Closure-Free `TState` Mechanics
- Monadic combinators provide overloads accepting `TState state` and `Func<TState, TValue, TResult>`.
- Eliminates heap allocation of compiler-generated closure display classes when passing external parameters to lambdas.

### 1.6 LINQ Query Comprehension
- Full LINQ query syntax support:
```csharp
var result = from user in GetUser(id)
             from order in GetOrder(user.OrderId)
             where order.IsActive
             select CalculateTotal(order);
```

---

## 2. Option Monad (`EricksonLopez.Result.Maybe`)

- `readonly struct Maybe<T>` representing optional presence of domain values.
- Interop: `.ToResult(error)` converts `Maybe<T>` to `Result<T>`.
- Factories: `Maybe<T>.From(value)`, `Maybe<T>.None`.
- Combinators: `Map`, `Bind`, `Ensure`, `Match`, `Execute`, `GetValueOrDefault`.

---

## 3. Strongly-Typed Errors (`EricksonLopez.Result.Generic`)

- `readonly struct Result<TValue, TError>` for explicit domain error types.
- Ensures compile-time enforcement of error contracts across architecture boundaries.

---

## 4. ASP.NET Core & HTTP Integration (`EricksonLopez.Result.AspNetCore`)

- **RFC 9457 ProblemDetails**: Automatic conversion of `Error` to `ProblemDetails` response objects.
- **Status Code Mapping**:
  - `ErrorType.Validation` $\rightarrow$ `400 Bad Request`
  - `ErrorType.Unauthorized` $\rightarrow$ `401 Unauthorized`
  - `ErrorType.Forbidden` $\rightarrow$ `403 Forbidden`
  - `ErrorType.NotFound` $\rightarrow$ `404 Not Found`
  - `ErrorType.Conflict` $\rightarrow$ `409 Conflict`
  - `ErrorType.Unavailable` $\rightarrow$ `503 Service Unavailable`
  - `ErrorType.Failure` / `ErrorType.Unexpected` $\rightarrow$ `500 Internal Server Error`
- **Minimal APIs Support**: Extension method `.ToHttpResult()` returning `IResult`.
- **Endpoint Filters**: `ResultEndpointFilter` transparently unwraps `Result<T>` and maps failures to `ProblemDetails`.

---

## 5. Distributed Observability (`EricksonLopez.Result.OpenTelemetry`)

- Native BCL integration via `System.Diagnostics.ActivitySource` and `System.Diagnostics.Metrics`.
- Metrics:
  - `ericksonlopez.result.operations` (Counter, tagged by `operation.name`, `ericksonlopez.result.outcome`, `ericksonlopez.result.error.code`, `error.type`, `ericksonlopez.result.error.severity`)
- Tracing:
  - Automatically enriches active spans with `ericksonlopez.result.outcome`, `ericksonlopez.result.error.code`, `error.type`, `ericksonlopez.result.error.severity`.

---

## 6. Serialization & Source Generation (`EricksonLopez.Result.Serialization`)

- Custom `System.Text.Json` converters (`ResultJsonConverter`, `ResultOfTJsonConverter<T>`, `ErrorJsonConverter`) for Native AOT.
- Roslyn Source Generator (`EricksonLopez.Result.Serialization.Generators`) generating AOT-safe converter bindings at compile time.

---

## 7. Roslyn Analyzers & CodeFixes (`EricksonLopez.Result.Analyzers`)

- Build-time diagnostic rules preventing runtime bugs and performance degradation:
  - `RESULT001`: `Result<T>` struct size exceeds 32-byte threshold.
  - `RESULT003`: Discarded `ErrorBuilder` mutation return value (with CodeFix).
  - `RESULT004`: Closure allocation in monadic pipeline (with CodeFix).
  - `RESULT005`: `Error.WithMetadata()` chained 3+ times without batching.
  - `RESULT006`: `ErrorBuilder.WithInnerError()` chained 2+ times consecutively.
  - `RESULT007`: Missing `ErrorEqualityComparer.Strict` in collection/LINQ deduplication.
  - `RESULT008`: Endpoint filter used without `.Produces<T>()`.
  - `RESULT009`: `IncludeDescription = true` set without environment guard.
  - `RESULT010`: `Exception.Message` used in `ResultExceptionBehavior`.
  - `RESULT012`: Method returns `default(Result)` or `default(Result<T>)`.
  - `RESULT_OTEL_001`: `TraceOutcome` called without `ResultMetrics` registered.
  - `RESULT_GEN_001`: `[JsonSerializable(typeof(Result))]` on serializer context has no effect.

---

## 8. Fluent Testing Framework (`EricksonLopez.Result.Testing`)

- Fluent assertion extensions:
```csharp
User user = result.ShouldBeSuccess();
Error error = result.ShouldBeFailure().ShouldHaveErrorCode("User.NotFound");
```
- Dedicated test adapter packages: `EricksonLopez.Result.Testing.XUnit` and `EricksonLopez.Result.Testing.NUnit`.
