# Level 02 — Complete Configuration: Diagnostics, ErrorBuilder & Metadata

> **Ecosystem:** `EricksonLopez.Result` | **Audience:** Senior Engineers, Domain Architects | **Complexity:** Level 2 (Intermediate) | **Code Reference:** `02_ErrorsAndBuilders.cs`, `05_Tapping.cs`, `12_ErrorEqualityAndMutation.cs`, `22_SerializationShowcase.cs` | **Language:** English

---

## 1. The `Error` Object: Domain Diagnostic Model

In `EricksonLopez.Result`, an `Error` is an immutable, strongly-typed, and serializable object that encapsulates all diagnostic dimensions of a software failure:

```csharp
public sealed class Error : IEquatable<Error>
{
    public string Code { get; }
    public string Description { get; }
    public string? DescriptionKey { get; }
    public ErrorType Type { get; }
    public ErrorSeverity Severity { get; }
    public ErrorRetryability Retryability { get; }
    public string? TraceId { get; }
    public string? CorrelationId { get; }
    public IReadOnlyList<Error>? InnerErrors { get; }
    public IReadOnlyDictionary<string, object>? Metadata { get; }
}
```

---

## 2. Complete Error Taxonomy

### 2.1 Functional Classification (`ErrorType`)
- `Validation`: Contract violation or invalid input data.
- `NotFound`: The requested entity or resource was not found.
- `Conflict`: State conflict (e.g., uniqueness invariant or optimistic concurrency mismatch).
- `Unauthorized`: Missing or invalid authentication credentials.
- `Forbidden`: Insufficient permissions for the requested operation.
- `Unavailable`: Downstream service or external dependency temporarily unreachable.
- `Unexpected`: Unhandled system failure or critical software defect.
- `Domain` / `Business`: Business rule or aggregate root invariant violation.
- `Infrastructure`: Failure in database, message broker, network, or file system.

### 2.2 Severity Level (`ErrorSeverity`)
- `Info`: Informational diagnostics that do not indicate operational failure.
- `Warning`: Anomalous condition that does not compromise transactional integrity.
- `Error`: Standard operational failure.
- `Critical`: Catastrophic event requiring immediate engineering alert (PagerDuty/Ops).

### 2.3 Retryability Classification (`ErrorRetryability`)
- `NotApplicable`: The retry concept is not meaningful (e.g. semantic or syntax validation failure).
- `Transient`: Temporary issue likely to resolve on immediate or backoff retry (e.g. socket timeout).
- `Permanent`: Deterministic failure that will recur if retried with the same input.

---

## 3. Fluent Construction with `ErrorBuilder`

The `ErrorBuilder` is a stack-allocated readonly struct providing copy-on-write semantics:

```csharp
Error richError = Error.Create("Order.PaymentDeclined", "The payment gateway declined the transaction.")
    .WithType(ErrorType.Validation)
    .WithSeverity(ErrorSeverity.Warning)
    .WithRetryability(ErrorRetryability.Permanent)
    .WithCorrelationId("sess-8849-xyz")
    .WithTraceId("4bf92f3577b34da6a3ce929d0e0e4736")
    .WithDescriptionKey("errors.order.payment_declined")
    .WithMetadata("Processor", "Stripe")
    .WithMetadata("DeclineCode", "insufficient_funds")
    .Build();
```

---

## 4. Copy-on-Write Mutations & Aggregate Errors

Existing `Error` instances can be enriched without mutating the original object:

```csharp
Error baseError = Error.NotFound("User.Missing", "User not found.");

// Immutable mutation: returns a new instance leaving baseError unchanged
Error enriched = baseError
    .WithCorrelationId(Guid.NewGuid().ToString())
    .WithMetadata("TenantId", "tenant-100");

// Compound error construction with nested child errors
Error compound = Error.Validation(
    "Form.Invalid", 
    "Multiple fields failed validation.",
    Error.Validation("Field.Email", "Invalid format"),
    Error.Validation("Field.Age", "Must be at least 18")
);
```

---

## 5. Logging & Side Effects (`TapOnSuccess`, `TapOnFailure`, `Inspect`)

Attach non-intrusive logging, metrics, or auditing hooks to the pipeline without breaking monadic flow:

```csharp
Result<Order> result = ProcessOrder()
    .TapOnSuccess(order => _logger.LogInformation("Order {Id} processed successfully", order.Id))
    .TapOnFailure(error => _logger.LogWarning("Order processing failed: [{Code}] {Desc}", error.Code, error.Description))
    .Inspect(r => _metrics.RecordOutcome(r.IsSuccess));
```

---

## 6. Official `System.Text.Json` Serialization

`EricksonLopez.Result.Serialization` provides dedicated converters for Minimal APIs, controllers, and message bus serialization:

```csharp
var options = new JsonSerializerOptions
{
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
};
options.Converters.Add(new ResultJsonConverter());
options.Converters.Add(new ErrorJsonConverter());
options.Converters.Add(new ResultOfTJsonConverter<OrderDto>());

string json = JsonSerializer.Serialize(result, options);
Result<OrderDto> deserialized = JsonSerializer.Deserialize<Result<OrderDto>>(json, options);
```

---

## Next Level
Proceed to **[Level 03 — Real-World Use Cases](level-03-real-world-usecases.md)** to see complete business workflows implemented using only authoritative APIs.
