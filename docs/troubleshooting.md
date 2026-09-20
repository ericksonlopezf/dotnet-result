# Troubleshooting & Common Issues

> **Ecosystem:** `EricksonLopez.Result` | **Audience:** Developers, DevOps Engineers

This guide covers common pitfalls, diagnostic warnings, Roslyn analyzer messages, and performance gotchas when using the `EricksonLopez.Result` ecosystem.

---

## 1. Uninitialized Struct: `InvalidOperationException`

### Symptom
```text
System.InvalidOperationException: Cannot access Value on an uninitialized default Result<T>.
```

### Cause
A method returned `default(Result<T>)` or `default` without explicitly calling `Result.Success(val)` or `Result.Failure(err)`.

### Solution
Always construct results via factory methods or implicit conversions:
```csharp
// ❌ WRONG
public Result<User> GetUser() => default;

// ✅ CORRECT
public Result<User> GetUser() => Result.Success(user);
// or
public Result<User> GetUser() => Error.NotFound("User.Missing", "User not found");
```

---

## 2. Double-Counting Metrics in OpenTelemetry

### Symptom
Your Prometheus / OpenTelemetry dashboard displays twice the expected transaction volume.

### Cause
Calling both static tracking (`ResultMetrics.StaticTrackSuccess`) and DI-injected metrics simultaneously, or calling `TraceOutcome(metrics: diMetrics)` while also executing manual static counters.

### Solution
Choose one mode per application:
1. **DI Mode (Recommended for Web APIs):**
   ```csharp
   builder.Services.AddResultMetrics();
   // In endpoints / handlers:
   result.TraceOutcome("ProcessOrder", activity, diMetrics);
   ```
2. **Static Mode (Recommended for lightweight console / workers):**
   ```csharp
   ResultMetrics.StaticTrackSuccess("ProcessOrder");
   ```

---

## 3. Roslyn Diagnostic Analyzer Rules

`EricksonLopez.Result.Analyzers` and generator diagnostics run at compile-time to enforce safety, performance, security, and zero-allocation guidelines:

| Diagnostic ID | Severity | Description | Fix |
|---|:---:|---|---|
| **`RESULT001`** | Warning | `Result<T>` struct size exceeds 32-byte threshold | Wrap payload in a class or record (reference type) |
| **`RESULT003`** | Error | Discarded return value from `ErrorBuilder.With*()` | Chain `.Build()` or assign the returned builder |
| **`RESULT004`** | Warning | Lambda captures local variable in Result pipeline (closure allocation) | Refactor to use static lambda with `TState` overload |
| **`RESULT005`** | Warning | `Error.WithMetadata()` chained 3+ times consecutively | Batch metadata via dictionary or use `ErrorBuilder` |
| **`RESULT006`** | Warning | `ErrorBuilder.WithInnerError()` chained 2+ times consecutively | Batch inner errors via array overload |
| **`RESULT007`** | Warning | `HashSet<Error>` or LINQ deduplication without `ErrorEqualityComparer.Strict` | Pass `ErrorEqualityComparer.Strict` explicitly |
| **`RESULT008`** | Warning | Endpoint filter used without `.Produces<T>()` metadata | Add `.ProducesResult<T>()` or `.Produces<T>()` |
| **`RESULT009`** | Warning | `ResultHttpOptions.IncludeDescription = true` without environment guard | Wrap assignment in `builder.Environment.IsDevelopment()` |
| **`RESULT010`** | Warning | `Exception.Message` used in `ResultExceptionBehavior` error factory | Use fixed error code or sanitize exception details |
| **`RESULT012`** | Warning | Method returns uninitialized `default(Result)` or `default(Result<T>)` | Return `Result.Success(...)` or `Result.Failure(...)` |
| **`RESULT013`** | Warning | Implicit bool conversion of `Result` in condition context | Replace with explicit `.IsSuccess` or `.IsFailure` |
| **`RESULT_OTEL_001`** | Info | `TraceOutcome()` called without `ResultMetrics` registered | Pass DI `ResultMetrics` instance or configure instrumentation |
| **`RESULT_GEN_001`** | Warning | `[JsonSerializable(typeof(Result))]` on serializer context | Annotate generic `Result<T>` instead of non-generic `Result` |

---

## 4. OpenAPI / Swagger Documentation Missing Response Types

### Symptom
Minimal API endpoints with `.AddResultEndpointFilter()` show empty 200 OK schemas in Swagger UI.

### Cause
Because `ResultEndpointFilter` works polymorphically across all `Result<T>` types via `IResultOutcome`, the runtime cannot dynamically reflect the concrete generic schema for Swagger.

### Solution
Decorate the route with `ProducesResult<T>`:
```csharp
app.MapGet("/orders/{id}", GetOrderHandler)
   .AddResultEndpointFilter()
   .ProducesResult<OrderDto>(StatusCodes.Status200OK);
```

---

## 5. System.Text.Json Serialization with Native AOT

### Symptom
`System.InvalidOperationException: Reflection-based serialization has been disabled for this application.`

### Cause
Serializing `Result<T>` without registering source-generated type metadata.

### Solution
Use `ResultJsonConverter` and `ResultOfTJsonConverter<T>` explicitly or annotate your `JsonSerializerContext`:
```csharp
[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(Result<OrderDto>))]
[JsonSerializable(typeof(Error))]
internal partial class AppJsonContext : JsonSerializerContext { }
```

---

## 6. Entity Framework Core: Unhandled Concurrency & Timeout Exceptions

### Symptom
`DbUpdateConcurrencyException` or `TimeoutException` escapes the repository layer and triggers unhandled 500 exceptions instead of structured domain errors.

### Cause
Calling `await dbContext.SaveChangesAsync()` directly rather than the Result-aware persistence adapter.

### Solution
Use `SaveChangesAsyncToResult` from `EricksonLopez.Result.EntityFrameworkCore`:
```csharp
Result<int> saveResult = await dbContext.SaveChangesAsyncToResult(cancellationToken);
if (saveResult.IsFailure)
{
    // Concurrency conflicts map to Error.Conflict; timeouts map to Error.Unavailable(Transient)
    return saveResult.Error;
}
```

---

## 7. gRPC: Unmapped Status Codes & Unknown Faults

### Symptom
Remote gRPC client receives `Status(StatusCode="Unknown")` or generic transport fault instead of specific status codes like `NotFound` or `InvalidArgument`.

### Cause
Throwing standard .NET exceptions across the gRPC service boundary instead of mapping domain errors to `RpcException`.

### Solution
Use `ToRpcException()` from `EricksonLopez.Result.Grpc` or invoke unary calls with `ExecuteResultAsync()`:
```csharp
// Server-side:
if (result.IsFailure)
{
    throw result.Error.ToRpcException();
}

// Client-side:
Result<OrderResponse> result = await client.GetOrderAsync(request).ExecuteResultAsync();
```

