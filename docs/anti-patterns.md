# Anti-Patterns

This document highlights common mistakes, unsafe patterns, and performance pitfalls to avoid when working with `EricksonLopez.Result`.

---

## 1. Accessing `.Value` Without Validating Success

**Anti-Pattern:** Attempting to access `.Value` directly without verifying `IsSuccess` or using `Match` / `TryGetValue`.

```csharp
// ❌ ANTI-PATTERN: Throws InvalidOperationException if GetUser returns a failure!
var result = userService.GetUser(id);
Console.WriteLine(result.Value.Name); 
```

**Solution:** Use `.Match()`, `.TryGetValue()`, or explicit guard checks.

```csharp
// ✅ CORRECT: Safe retrieval via TryGetValue
if (result.TryGetValue(out var user))
{
    Console.WriteLine(user.Name);
}

// ✅ CORRECT: Functional pattern matching
result.Match(
    user => Console.WriteLine(user.Name),
    error => _logger.LogWarning("Failed to retrieve user: {Description}", error.Description)
);
```

---

## 2. Accessing `.Error` on a Successful Result

**Anti-Pattern:** Accessing `.Error` on a successful `Result` instance.

```csharp
// ❌ ANTI-PATTERN: Throws InvalidOperationException on success!
var result = Result.Success(42);
_logger.LogError(result.Error.Description);
```

**Solution:** Access `.Error` only inside a failure branch or via `.Match()`.

```csharp
// ✅ CORRECT: Guarded check
if (result.IsFailure)
{
    _logger.LogError(result.Error.Description);
}

// ✅ CORRECT: Functional matching
result.Match(
    value => Console.WriteLine($"Success: {value}"),
    error => _logger.LogError(error.Description)
);
```

---

## 3. Treating `default(Result)` as a Valid Failure

**Anti-Pattern:** Treating `default(Result)` or `default(Result<T>)` as a valid failure without checking `IsUninitialized`.

```csharp
// ❌ ANTI-PATTERN: default(Result<T>) has _state == ResultState.Uninitialized
Result<int> res = default;
if (res.IsFailure) // evaluates to false!
{
    // Missed failure path because state is Uninitialized, not Failure!
}
```

**Solution:** Always initialize results using `Result.Success(...)` or `Result.Failure(...)`. When receiving a result from an external boundary that might be default, check `res.IsUninitialized`.

```csharp
// ✅ CORRECT: Explicit uninitialized check
if (res.IsUninitialized)
{
    _logger.LogWarning("Uninitialized Result struct encountered.");
    return Error.Failure("Result.Uninitialized", "Result was not properly initialized.");
}
```

> [!NOTE]
> The Roslyn analyzer **`RESULT012`** (`DefaultResultReturnAnalyzer`) warns at compile time when `default(Result)` or `default(Result<T>)` is returned from a method.

---

## 4. Overusing `Result.Try` for Non-Throwing Domain Code

**Anti-Pattern:** Wrapping basic, non-throwing business logic inside `Result.Try` "just to be safe".

```csharp
// ❌ ANTI-PATTERN: Unnecessary delegate and exception handling overhead
var result = Result.Try(
    () => user.CalculateDiscount(),
    ex => Error.Unexpected("Calc.Failed", ex.Message)
);
```

**Solution:** Reserve `Result.Try` exclusively for integration points with third-party libraries, legacy APIs, file systems, or network operations that are known to throw exceptions.

```csharp
// ✅ CORRECT: Direct domain invocation returning Result<T>
Result<decimal> result = user.CalculateDiscount();
```

---

## 5. Closure Capturing in High-Throughput Pipelines

**Anti-Pattern:** Capturing variables inside lambda expressions in hot loops or high-throughput endpoints.

```csharp
// ❌ ANTI-PATTERN: Allocates a new closure object on every iteration!
foreach (var item in items)
{
    var threshold = item.Limit;
    var result = service.GetStatus(item.Id)
        .Ensure(s => s.Count < threshold, Error.Validation("Limit.Exceeded", "Limit exceeded"));
}
```

**Solution:** Use the `TState` overloads with `static` lambdas to pass external variables without closure heap allocations.

```csharp
// ✅ CORRECT: Zero heap allocation via TState overload
foreach (var item in items)
{
    var threshold = item.Limit;
    var result = service.GetStatus(item.Id)
        .Ensure(threshold, static (limit, s) => s.Count < limit, Error.Validation("Limit.Exceeded", "Limit exceeded"));
}
```

> [!NOTE]
> The Roslyn analyzer **`RESULT004`** (`ClosureCaptureAnalyzer`) detects closures in Result pipeline chains and offers a code fix (`ClosureCaptureCodeFix`) to refactor to `TState`.

---

## 6. Chaining `Error.WithMetadata` in Loops

**Anti-Pattern:** Repeatedly chaining `.WithMetadata()` calls on `Error`.

```csharp
// ❌ ANTI-PATTERN: Each WithMetadata creates a new Error instance (N copies)
var error = Error.Validation("Order.Invalid", "Order validation failed")
    .WithMetadata("Key1", val1)
    .WithMetadata("Key2", val2)
    .WithMetadata("Key3", val3);
```

**Solution:** Use `ErrorBuilder` or pass a batch dictionary to `.WithMetadata(IReadOnlyDictionary<string, object?>)`.

```csharp
// ✅ CORRECT: Single allocation via ErrorBuilder
var error = ErrorBuilder.Validation("Order.Invalid", "Order validation failed")
    .WithMetadata("Key1", val1)
    .WithMetadata("Key2", val2)
    .WithMetadata("Key3", val3)
    .Build();
```

> [!NOTE]
> The Roslyn analyzer **`RESULT005`** (`MetadataChainingAnalyzer`) warns when `Error.WithMetadata()` is chained 3 or more times consecutively.

---

## 7. Using `HashSet<Error>` Without `ErrorEqualityComparer.Strict`

**Anti-Pattern:** Using standard `HashSet<Error>`, `Distinct()`, `GroupBy()`, or `ToHashSet()` on `Error` instances expecting per-request attributes (`TraceId`, `CorrelationId`, `Metadata`) to differentiate items.

```csharp
// ❌ ANTI-PATTERN: Error.Equals only compares Code, Description, Type, Severity, Retryability
var e1 = Error.NotFound("User.NotFound", "User not found").WithTraceId("trace-1");
var e2 = Error.NotFound("User.NotFound", "User not found").WithTraceId("trace-2");

var set = new HashSet<Error> { e1, e2 };
Console.WriteLine(set.Count); // Outputs 1 — silently deduplicated!
```

**Solution:** Explicitly provide `ErrorEqualityComparer.Strict` for strict reference and metadata comparisons.

```csharp
// ✅ CORRECT: Preserves distinct trace IDs and metadata
var set = new HashSet<Error>(ErrorEqualityComparer.Strict) { e1, e2 };
Console.WriteLine(set.Count); // Outputs 2
```

> [!NOTE]
> The Roslyn analyzer **`RESULT007`** (`HashSetErrorEqualityAnalyzer`) warns when collection deduplication is performed on `Error` without `ErrorEqualityComparer.Strict`.

---

## 8. Missing `.Produces<T>()` with `AddResultEndpointFilter()`

**Anti-Pattern:** Relying on `AddResultEndpointFilter()` in Minimal APIs without `.Produces<T>()` or `ProducesResult<T>()`.

```csharp
// ❌ ANTI-PATTERN: OpenAPI schema degrades to 'object' because filter returns Ok<object?>
app.MapGet("/orders/{id}", (Guid id, IOrderService svc) => svc.GetOrder(id))
   .AddResultEndpointFilter();
```

**Solution:** Use `.ProducesResult<T>()` from `EricksonLopez.Result.OpenApi` or call `.ToHttpResult()` directly.

```csharp
// ✅ CORRECT: Explicit OpenAPI schema
app.MapGet("/orders/{id}", (Guid id, IOrderService svc) => svc.GetOrder(id))
   .AddResultEndpointFilter()
   .ProducesResult<OrderDto>(StatusCodes.Status200OK);

// ✅ ALTERNATIVE (Zero boxing): Direct ToHttpResult()
app.MapGet("/orders/{id}", async (Guid id, IOrderService svc) =>
{
    var result = await svc.GetOrderAsync(id);
    return result.ToHttpResult();
});
```

> [!NOTE]
> The Roslyn analyzer **`RESULT008`** (`EndpointFilterOpenApiAnalyzer`) warns at compile time when `AddResultEndpointFilter()` is called without `.Produces<T>()`.
