# Roslyn Diagnostic Analyzers Reference

`EricksonLopez.Result` includes a suite of Roslyn diagnostic analyzers and code fix providers in the `EricksonLopez.Result.Analyzers` package (bundled automatically with the core package). These analyzers enforce performance, security, reliability, and correctness invariants at compile time.

---

## Diagnostic Rules Summary

| Diagnostic ID | Category | Default Severity | Description | CodeFix Available |
|---|---|:---:|---|:---:|
| [`RESULT001`](performance.md#result001---resultt-value-type-is-excessively-large) | Performance | Warning | `Result<T>` value type is excessively large (>32 bytes) | No |
| [`RESULT003`](error-builder.md#result003---errorbuilder-method-return-value-is-discarded) | Usage | **Error** | `ErrorBuilder` method return value is discarded | Assign return value |
| [`RESULT004`](performance.md#result004---lambda-captures-locals-in-result-pipeline) | Performance | Warning | Lambda captures locals in Result pipeline (closure allocation) | Make static / use TState |
| [`RESULT005`](error-builder.md#result005---avoid-chaining-errorwithmetadata-calls) | Performance | Warning | Avoid chaining `Error.WithMetadata()` calls consecutively | Batch via dictionary |
| [`RESULT006`](error-builder.md#result006---chained-errorbuilderwithinnererror-calls-are-on) | Performance | Warning | Chained `ErrorBuilder.WithInnerError()` calls are $O(n^2)$ | Batch via array |
| [`RESULT007`](#result007---missing-errorequalitycomparerstrict-in-collection-or-linq-deduplication) | Reliability | Warning | Missing `ErrorEqualityComparer.Strict` in collection or LINQ deduplication | No |
| [`RESULT008`](#result008---resultendpointfilter-hides-openapi-metadata-without-explicit-producest) | Usage | Warning | `ResultEndpointFilter` hides OpenAPI metadata without explicit `.Produces<T>()` | Add `.Produces<T>()` |
| [`RESULT009`](#result009---resulthttpoptionsincludedescription-set-to-true-without-environment-guard) | Security | Warning | `ResultHttpOptions.IncludeDescription` set to `true` without environment guard | No |
| [`RESULT010`](#result010---avoid-using-exceptionmessage-in-resultexceptionbehavior) | Security | Warning | Avoid using `Exception.Message` in `ResultExceptionBehavior` error factory | No |
| [`RESULT012`](#result012---avoid-returning-defaultresult-or-defaultresultt) | Usage | Warning | Avoid returning `default(Result)` or `default(Result<T>)` | Use `Success`/`Failure` |
| [`RESULT_OTEL_001`](#result_otel_001---traceoutcometraceonfailuretraceonsuccess-called-without-metrics-instance) | Observability | Info | `TraceOutcome`/`TraceOnFailure`/`TraceOnSuccess` called without `metrics` argument | Pass DI `metrics` instance |
| [`RESULT_GEN_001`](serialization.md#result_gen_001---jsonserializabletypeofresult-has-no-effect-for-converter-generation) | Usage | Warning | `[JsonSerializable(typeof(Result))]` has no effect for converter generation | Use generic `Result<T>` |

---

## Rule Details

### `RESULT007` — Missing `ErrorEqualityComparer.Strict` in Collection or LINQ Deduplication

#### Cause
A collection (such as `HashSet<Error>`, `Dictionary<Error, ...>`) or a LINQ deduplication operator (`Distinct()`, `GroupBy()`, `ToHashSet()`) is used with `Error` without explicitly passing `ErrorEqualityComparer.Strict`.

#### Rationale
By default, `Error.Equals` implements **semantic equality**, comparing the 5 core domain properties (`Code`, `Description`, `Type`, `Severity`, `Retryability`). It intentionally ignores request-scoped diagnostics such as `TraceId`, `CorrelationId`, and `Metadata`. When storing or deduplicating `Error` instances where distinct trace IDs or metadata must be preserved, you must pass `ErrorEqualityComparer.Strict`.

#### How to Fix
Pass `ErrorEqualityComparer.Strict` to the collection constructor or LINQ method:

```csharp
// ❌ Triggers RESULT007:
var errors = new HashSet<Error>();
var unique = errorList.Distinct();

// ✅ Correct:
var errors = new HashSet<Error>(ErrorEqualityComparer.Strict);
var unique = errorList.Distinct(ErrorEqualityComparer.Strict);
```

---

### `RESULT008` — `ResultEndpointFilter` Hides OpenAPI Metadata without Explicit `.Produces<T>()`

#### Cause
An ASP.NET Core Minimal API endpoint uses `AddResultEndpointFilter()` on a route returning `Result<T>` without an explicit `.Produces<T>()` or `.ProducesResult<T>()` annotation.

#### Rationale
`ResultEndpointFilter` inspects returned values via the non-generic `IResultOutcome` interface, unwrapping the payload as `object?` via `IResultOutcome.RawValue`. As a consequence, OpenAPI generators (Swagger, Swashbuckle, NSwag, Microsoft.AspNetCore.OpenApi) infer the response type as `object` rather than the strongly-typed DTO `T`.

#### How to Fix
Add `.Produces<T>()` or use the `.ProducesResult<T>()` extension from `EricksonLopez.Result.OpenApi`:

```csharp
// ❌ Triggers RESULT008:
app.MapGet("/orders/{id}", (Guid id, IOrderService svc) => svc.GetOrder(id))
   .AddResultEndpointFilter();

// ✅ Correct:
app.MapGet("/orders/{id}", (Guid id, IOrderService svc) => svc.GetOrder(id))
   .AddResultEndpointFilter()
   .ProducesResult<OrderDto>(StatusCodes.Status200OK);
```

---

### `RESULT009` — `ResultHttpOptions.IncludeDescription` Set to `true` Without Environment Guard

#### Cause
`ResultHttpOptions.IncludeDescription` is assigned `true` unconditionally (e.g. outside `if (app.Environment.IsDevelopment())`).

#### Rationale
Setting `IncludeDescription = true` in production environments can expose sensitive internal exception details, database query strings, or server paths to API consumers inside RFC 9457 ProblemDetails payloads.

#### How to Fix
Guard `IncludeDescription` by checking the hosting environment:

```csharp
// ❌ Triggers RESULT009:
services.Configure<ResultHttpOptions>(options =>
{
    options.IncludeDescription = true;
});

// ✅ Correct:
services.Configure<ResultHttpOptions>(options =>
{
    options.IncludeDescription = builder.Environment.IsDevelopment();
});
```

---

### `RESULT010` — Avoid Using `Exception.Message` in `ResultExceptionBehavior`

#### Cause
A custom `errorFactory` lambda passed to `ResultExceptionBehavior` accesses `Exception.Message`.

#### Rationale
Unhandled exception messages in production (e.g., `SqlException.Message`, `SocketException.Message`, `HttpRequestException.Message`) often contain database server IPs, table names, connection strings, or query parameters. If mapped into a `Result` error description, this sensitive information may be serialized into client-facing responses.

#### How to Fix
Use static error descriptions or sanitize the message:

```csharp
// ❌ Triggers RESULT010:
builder.Services.AddResultExceptionBehavior(ex =>
    Error.Unexpected("Handler.Fault", ex.Message));

// ✅ Correct:
builder.Services.AddResultExceptionBehavior(ex =>
    Error.Unexpected("Handler.Fault", "An unexpected error occurred while processing your request."));
```

---

### `RESULT012` — Avoid Returning `default(Result)` or `default(Result<T>)`

#### Cause
A method with return type `Result` or `Result<T>` returns `default` or `default(Result<T>)`.

#### Rationale
`Result` and `Result<T>` are structs with a 3-state discriminant (`Uninitialized = 0`, `Success = 1`, `Failure = 2`). An uninitialized `default` struct evaluates to `IsSuccess = false`, `IsFailure = false`, and `IsUninitialized = true`. Calling `.Value` or `.Error` on an uninitialized result throws `InvalidOperationException`.

#### How to Fix
Always construct results via `Result.Success()`, `Result.Success(value)`, `Result.Failure(error)`, or implicit conversion:

```csharp
// ❌ Triggers RESULT012:
public Result<User> GetUser() => default;

// ✅ Correct:
public Result<User> GetUser() => Result.Success(user);
// or:
public Result<User> GetUser() => Result.Failure<User>(Error.NotFound("User.NotFound", "User not found."));
```

---

### `RESULT_OTEL_001` — `TraceOutcome`/`TraceOnFailure`/`TraceOnSuccess` Called Without Metrics Instance

#### Cause
The OpenTelemetry extension methods `TraceOutcome`, `TraceOnFailure`, or `TraceOnSuccess` are invoked without providing the optional `metrics` argument.

#### Rationale
When using dependency-injected `ResultMetrics` (registered via `services.AddResultMetrics()`), omitting the `metrics` parameter means only the `Activity` span tags will be recorded, while `Meter` operations counter metrics will be silently skipped.

#### How to Fix
Pass the injected `ResultMetrics` instance:

```csharp
// ❌ Triggers RESULT_OTEL_001:
result.TraceOutcome("CreateOrder", activity);

// ✅ Correct:
result.TraceOutcome("CreateOrder", activity, _metrics);
```
