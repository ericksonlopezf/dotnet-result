# Frequently Asked Questions (FAQ)

> **Ecosystem:** `EricksonLopez.Result` | **Audience:** Architects, Developers, DevOps Engineers | **Language:** English

---

### Q1: Why are `Result` and `Result<T>` implemented as `readonly struct` instead of `class`?
**A:** As readonly value-type structs, they are allocated directly on the stack or passed inline inside CPU registers. On the success path (*happy path*), an operation generates **0 bytes of GC heap allocation**. This eliminates garbage collection pauses, reduces cache misses, and maximizes throughput under heavy enterprise workloads.

---

### Q2: What happens if I access `.Value` on a failed or uninitialized `Result<T>`?
**A:** `EricksonLopez.Result` intentionally throws an `InvalidOperationException` with an actionable error message to prevent silent consumption of null, default, or invalid states. The recommended pattern is to consume results through safe functional combinators like `.Match()`, `.TryGetValue()`, `.GetValueOrDefault()`, or guarded pattern matching via `if (result.IsSuccess)`.

---

### Q3: How do I handle uninitialized default state `default(Result)`?
**A:** Like any C# struct, `default(Result)` has all internal fields set to zero in memory. `Result` and `Result<T>` expose an explicit `IsUninitialized` property. Both `IsSuccess` and `IsFailure` return `false` when a result is uninitialized. The bundled Roslyn analyzer **`RESULT012`** (`DefaultResultReturnAnalyzer`) detects and prevents returning `default(Result)` or `default(Result<T>)` from methods at compile time.

---

### Q4: Why is `Error` a `class` and not a `struct`?
**A:** An `Error` instance is instantiated exclusively when an operation fails. In healthy production systems, failure paths represent a small fraction of overall throughput. As a reference-type class, `Error` provides rich domain modeling, extensible metadata dictionaries, inner error hierarchies (`InnerErrors`), localized resource keys (`DescriptionKey`), and W3C distributed tracing context without inflating the memory footprint of the `Result` struct on the CPU stack.

---

### Q5: What is the difference between `Result.Combine` and `Result.ValidateAll`?
**A:**
- **`Result.Combine`**: Composes 2 or more heterogeneous operations and merges their results into a strongly-typed tuple (e.g., `(User, Account)`). If any operation fails, it aggregates the errors into a combined failure result.
- **`Result.ValidateAll`**: Validates multiple validation rules against a single entity or request, accumulating all validation errors using an internal `ArrayPool<Error>` buffer to minimize temporary heap allocations before returning the aggregated failure.

---

### Q6: Why should I use `TState` overloads instead of standard lambdas?
**A:** Standard lambda expressions that capture outer variables allocate compiler-generated closure instances (*display classes*) on the heap on every invocation. The `TState` combinator overloads allow you to pass external state explicitly to static lambdas (`static (val, state) => ...`), achieving **zero heap allocations (0 bytes)** in hot execution loops. Roslyn analyzer **`RESULT004`** warns whenever an avoidable closure capture is detected.

---

### Q7: Is the library fully compatible with NativeAOT and Trimming in .NET 8, 9, and 10?
**A:** Yes. 100% of the core `EricksonLopez.Result` codebase and its primary extension packages (`AspNetCore`, `OpenApi`, `OpenTelemetry`, `Testing`, `Serialization`) are certified for NativeAOT compilation (`PublishAot=true`). For `System.Text.Json`, `EricksonLopez.Result.Serialization` and `EricksonLopez.Result.Serialization.Generators` provide explicit converters and Roslyn source generators that eliminate reflection in runtime execution.

---

### Q8: How do I integrate results with ASP.NET Core Minimal APIs?
**A:** Return `.ToHttpResult()` directly from your endpoint handler, or attach `.AddResultEndpointFilter()` to your route builder. This automatically maps the domain `ErrorType` to the corresponding HTTP status code (`400 Bad Request`, `401 Unauthorized`, `403 Forbidden`, `404 Not Found`, `409 Conflict`, `503 Service Unavailable`, `500 Internal Server Error`) following the RFC 9457 Problem Details specification. To retain strongly-typed OpenAPI documentation when using filters, chain `.ProducesResult<TResponse>()` from `EricksonLopez.Result.OpenApi`.

---

### Q9: How do I handle database persistence and resilience without throwing exceptions?
**A:**
- **Entity Framework Core**: Use `SaveChangesAsyncToResult()` on `DbContext` (`EricksonLopez.Result.EntityFrameworkCore`) to intercept database exceptions and convert concurrency violations to `Error.Conflict` and timeouts to `Error.Unavailable(Transient)`.
- **Polly v8**: Use `ExecuteResultAsync()` and `.AddResultRetry()` (`EricksonLopez.Result.Polly`) to configure retry policies that inspect `Error.Retryability == ErrorRetryability.Transient` without throwing exceptions.
- **MassTransit**: Register `ResultConsumeFilter<TMessage>` (`EricksonLopez.Result.MassTransit`) to automatically bridge message consumer returns and emit `ResultFault` events across message brokers.

---

### Q10: How do I integrate `Result<T>` across distributed microservices using Dapr and gRPC?
**A:**
- **Dapr**: Use `EricksonLopez.Result.Dapr` extension methods such as `InvokeMethodGrpcResultAsync`, `GetStateResultAsync`, and `ExecuteTransactionResultAsync` on `DaprClient`. Unsuccessful invocations and state conflict errors are mapped to structured domain `Error` instances.
- **gRPC**: Use `EricksonLopez.Result.Grpc` to convert unary gRPC responses via `.ExecuteResultAsync(call)` or map between `Result<T>` and `RpcException` / `Status` via `error.ToRpcException()` and `rpcEx.ToResult<T>()`.

