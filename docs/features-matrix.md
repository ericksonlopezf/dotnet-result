# EricksonLopez.Result Feature Matrix & Compatibility Guide

> **Ecosystem:** `EricksonLopez.Result` | **Status:** Enterprise Production Ready | **Language:** English

---

## 1. Feature Support by Framework Version

| Feature / Package | .NET 8.0 LTS | .NET 9.0 STS | .NET 10.0 Preview/LTS | Native AOT |
|---|:---:|:---:|:---:|:---:|
| `EricksonLopez.Result` (Core) | ✅ Full | ✅ Full | ✅ Full | ✅ Certified |
| `EricksonLopez.Result.Maybe` | ✅ Full | ✅ Full | ✅ Full | ✅ Certified |
| `EricksonLopez.Result.Generic` | ✅ Full | ✅ Full | ✅ Full | ✅ Certified |
| `EricksonLopez.Result.AspNetCore` | ✅ Full | ✅ Full | ✅ Full | ✅ Certified |
| `EricksonLopez.Result.OpenApi` | ✅ Full | ✅ Full | ✅ Full | ✅ Certified |
| `EricksonLopez.Result.OpenTelemetry` | ✅ Full | ✅ Full | ✅ Full | ✅ Certified |
| `EricksonLopez.Result.FluentValidation` | ✅ Full | ✅ Full | ✅ Full | ✅ Certified |
| `EricksonLopez.Result.MediatR` | ✅ Full | ✅ Full | ✅ Full | ⚠️ Non-AOT (Legacy, see ADR-018) |
| `EricksonLopez.Result.Serialization` | ✅ Full | ✅ Full | ✅ Full | ✅ Certified |
| `EricksonLopez.Result.Serialization.Generators` | `netstandard2.0` | `netstandard2.0` | `netstandard2.0` | ✅ Compiler Tool |
| `EricksonLopez.Result.DomainErrors.Generators` | `netstandard2.0` | `netstandard2.0` | `netstandard2.0` | ✅ Compiler Tool |
| `EricksonLopez.Result.EntityFrameworkCore` | ✅ Full | ✅ Full | ✅ Full | ✅ Certified |
| `EricksonLopez.Result.Polly` | ✅ Full | ✅ Full | ✅ Full | ✅ Certified |
| `EricksonLopez.Result.MassTransit` | ✅ Full | ✅ Full | ✅ Full | ⚠️ Non-AOT (see ADR-025) |
| `EricksonLopez.Result.Dapr` | ✅ Full | ✅ Full | ✅ Full | ✅ Certified |
| `EricksonLopez.Result.Grpc` | ✅ Full | ✅ Full | ✅ Full | ✅ Certified |
| `EricksonLopez.Result.Analyzers` | `netstandard2.0` | `netstandard2.0` | `netstandard2.0` | ✅ Compiler Tool |
| `EricksonLopez.Result.Testing` | ✅ Full | ✅ Full | ✅ Full | ❌ Test Only |
| `EricksonLopez.Result.Testing.XUnit` | ✅ Full | ✅ Full | ✅ Full | ❌ Test Only |
| `EricksonLopez.Result.Testing.NUnit` | ✅ Full | ✅ Full | ✅ Full | ❌ Test Only |

---

## 2. Monadic Operators Matrix

| Operator | Sync | Async (`Task`) | Async (`ValueTask`) | Closure-Free (`TState`) | LINQ Syntax |
|---|:---:|:---:|:---:|:---:|:---:|
| **`Map`** | ✅ Yes | ✅ Yes | ✅ Yes | ✅ Yes | `select` |
| **`MapError`** | ✅ Yes | ✅ Yes | ✅ Yes | ✅ Yes | — |
| **`Bind`** | ✅ Yes | ✅ Yes | ✅ Yes | ✅ Yes | `from ... in` |
| **`TapOnSuccess`** | ✅ Yes | ✅ Yes | ✅ Yes | ✅ Yes | — |
| **`TapOnFailure`** | ✅ Yes | ✅ Yes | ✅ Yes | ✅ Yes | — |
| **`Ensure`** | ✅ Yes | ✅ Yes | ✅ Yes | ✅ Yes | `where` |
| **`Match`** | ✅ Yes | ✅ Yes | ✅ Yes | ✅ Yes | — |
| **`Execute`** | ✅ Yes | ✅ Yes | ✅ Yes | ✅ Yes | — |
| **`Inspect`** | ✅ Yes | ✅ Yes | ✅ Yes | ✅ Yes | — |
| **`Recover`** | ✅ Yes | ✅ Yes | ✅ Yes | ✅ Yes | — |
| **`ValidateAll`** | ✅ `ReadOnlySpan` | ✅ `IReadOnlyList` | ✅ `ValueTask` | ✅ Yes | — |
| **`Combine`** | ✅ `ReadOnlySpan` | ✅ Tuples (2-5) | — | — | — |

---

## 3. Error Taxonomy, HTTP & gRPC Status Code Mapping Matrix

| Error Type (`ErrorType`) | Default Severity | HTTP Status Code (RFC 9457) | gRPC Status Code | Typical Domain Scenario |
|---|---|:---:|:---:|---|
| `Validation` | `Warning` | **400 Bad Request** | `InvalidArgument` | Input data invariant violations, FluentValidation failures |
| `Unauthorized` | `Warning` | **401 Unauthorized** | `Unauthenticated` | Missing or invalid authentication tokens |
| `Forbidden` | `Warning` | **403 Forbidden** | `PermissionDenied` | Authenticated user lacks permission for the resource |
| `NotFound` | `Info` | **404 Not Found** | `NotFound` | Requested entity or aggregate root does not exist |
| `Conflict` | `Warning` | **409 Conflict** | `AlreadyExists` / `Aborted` | Optimistic concurrency conflict, unique constraint violation |
| `Unavailable` | `Error` | **503 Service Unavailable** | `Unavailable` | Circuit breaker open, downstream service unavailable |
| `Failure` | `Error` | **500 Internal Server Error** | `Internal` | Business rule invariant violation or unhandled domain failure |
| `Unexpected` | `Error` | **500 Internal Server Error** | `Internal` | Unexpected exception caught during pipeline execution |

---

## 4. Diagnostics & Roslyn Analyzers Matrix

| Diagnostic ID | Severity | Default | Category | Description | CodeFix Available |
|---|---|:---:|---|---|:---:|
| `RESULT001` | Warning | Enabled | Performance | `Result<T>` struct size exceeds 32-byte threshold | ❌ |
| `RESULT003` | **Error** | Enabled | Usage | `ErrorBuilder.With*()` return value is discarded | ✅ Assign Return |
| `RESULT004` | Warning | Enabled | Performance | Closure allocation detected in Result pipeline | ✅ Use `TState` |
| `RESULT005` | Warning | Enabled | Performance | `Error.WithMetadata()` chained 3+ times consecutively | ❌ |
| `RESULT006` | Warning | Enabled | Performance | `ErrorBuilder.WithInnerError()` chained 2+ times consecutively | ❌ |
| `RESULT007` | Warning | Enabled | Reliability | Missing `ErrorEqualityComparer.Strict` in collection deduplication | ✅ Use Comparer |
| `RESULT008` | Warning | Enabled | Usage | `ResultEndpointFilter` used without `.Produces<T>()` | ❌ |
| `RESULT009` | Warning | Enabled | Security | `IncludeDescription = true` set without environment guard | ❌ |
| `RESULT010` | Warning | Enabled | Security | `Exception.Message` used in `ResultExceptionBehavior` | ❌ |
| `RESULT012` | Warning | Enabled | Usage | Method returns `default(Result)` or `default(Result<T>)` | ✅ Return Result |
| `RESULT013` | Warning | Enabled | Usage | Avoid implicit boolean conversion of `Result` or `Result<T>` | ✅ Use `.IsSuccess` / `.IsFailure` |
| `RESULT_OTEL_001` | Info | Enabled | Observability | `TraceOutcome()` called without `ResultMetrics` registered | ❌ |
| `RESULT_GEN_001` | Warning | Enabled | Usage | `[JsonSerializable(typeof(Result))]` has no effect for converter generation | ❌ |
