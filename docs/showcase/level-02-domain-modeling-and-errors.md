# Level 02 — Domain Modeling & Rich Error Taxonomy

> **Ecosystem:** `EricksonLopez.Result` | **Audience:** Domain Architects & Backend Engineers | **Language:** English

---

## 1. The Multi-Dimensional `Error` Model

In enterprise distributed systems, a simple string error message is insufficient. `EricksonLopez.Result.Error` encapsulates comprehensive semantic dimensions:

```csharp
public sealed class Error : IEquatable<Error>
{
    public string Code { get; }
    public string Description { get; }
    public ErrorType Type { get; }
    public ErrorSeverity Severity { get; }
    public ErrorRetryability Retryability { get; }
    public string? DescriptionKey { get; }
    public string? TraceId { get; }
    public string? CorrelationId { get; }
    public IReadOnlyDictionary<string, object> Metadata { get; }
    public ImmutableArray<Error> InnerErrors { get; }
}
```

---

## 2. Standard Error Classification (`ErrorType`)

| Semantic Factory | `ErrorType` | Default HTTP Code | Purpose |
|---|---|:---:|---|
| `Error.Validation(...)` | `Validation` | 400 | Data contract or format violations |
| `Error.Unauthorized(...)` | `Unauthorized` | 401 | Missing or invalid authentication credentials |
| `Error.Forbidden(...)` | `Forbidden` | 403 | Caller authenticated but lacks resource permissions |
| `Error.NotFound(...)` | `NotFound` | 404 | Aggregate root or entity does not exist |
| `Error.Conflict(...)` | `Conflict` | 409 | Optimistic concurrency conflict or duplicate key |
| `Error.Failure(...)` | `Failure` | 500 | Business rule or domain invariant violation |
| `Error.Unexpected(...)` | `Unexpected` | 500 | Exception caught during pipeline execution |

---

## 3. Creating Rich Domain Errors

### 3.1 Domain-Specific Error Catalogs
Define centralized, strongly-typed error catalogs for domain aggregates:

```csharp
public static class DomainErrors
{
    public static class Order
    {
        public static Error NotFound(Guid orderId) =>
            Error.NotFound(
                code: "Order.NotFound",
                description: $"Order with ID '{orderId}' was not found.")
            .WithMetadata("OrderId", orderId);

        public static Error InsufficientStock(string sku, int requested, int available) =>
            Error.Conflict(
                code: "Order.InsufficientStock",
                description: $"SKU '{sku}' has only {available} units available, but {requested} were requested.")
            .WithMetadata("Sku", sku)
            .WithMetadata("RequestedUnits", requested)
            .WithMetadata("AvailableUnits", available);

        public static Error PaymentFailed(string reason, ErrorRetryability retryability = ErrorRetryability.Transient) =>
            Error.Failure(
                code: "Order.PaymentFailed",
                description: $"Payment gateway rejected transaction: {reason}")
            .WithRetryability(retryability)
            .WithSeverity(ErrorSeverity.Error);
    }
}
```

---

## 4. Lazy Ambient `TraceId` Capture

When an `Error` is created, it automatically and lazily captures the ambient distributed tracing `TraceId` from `System.Diagnostics.Activity.Current`:

```csharp
// If an OpenTelemetry Activity is active, Error.TraceId captures it automatically:
var error = DomainErrors.Order.NotFound(orderId);

// Zero runtime dependency on heavy OpenTelemetry SDK packages in the domain layer!
Console.WriteLine($"Correlated Trace ID: {error.TraceId}");
```

---

## Next Steps
Proceed to [Level 03 — Railway Pipelines & Monadic Composition](level-03-railway-pipelines.md) to learn how to chain complex business workflows using `Bind`, `Map`, `Tap`, `Ensure`, and closure-free `TState` overloads.
