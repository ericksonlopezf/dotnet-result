# Level 06 — Error Handling: Resilience, Recovery & Retry Strategies

> **Ecosystem:** `EricksonLopez.Result` | **Audience:** Reliability Engineers, Distributed Systems Architects | **Complexity:** Level 6 (Advanced) | **Code Reference:** `17_RecoverAndErrorTransformation.cs`, `20_WellKnownErrorsAndTry.cs` | **Language:** English

---

## 1. Error Classification: Transient vs Permanent

In event-driven architectures and distributed microservices, treating all errors uniformly leads to retry storms, cascading failures, or unrecoverable message loss.

`EricksonLopez.Result` addresses this via the **`ErrorRetryability`** taxonomy:

```csharp
public enum ErrorRetryability : byte
{
    NotApplicable = 0, // Contract or semantic validation failure (retrying will never succeed)
    Transient = 1,     // Network timeout, temporary lock contention (safe to retry with exponential backoff)
    Permanent = 2      // Corrupted entity, blocked account, non-existent resource (dispatch to Dead-Letter Queue)
}
```

### Integration with Resilient Dispatchers & Workers:
```csharp
Result<PaymentResponse> result = await CallPaymentGatewayAsync(request);

if (result.IsFailure)
{
    switch (result.Error.Retryability)
    {
        case ErrorRetryability.Transient:
            // Apply exponential backoff and retry
            await ScheduleRetryAsync(request, delay: TimeSpan.FromSeconds(5));
            break;

        case ErrorRetryability.Permanent:
            // Dispatch immediately to Dead-Letter Queue (DLQ)
            await DeadLetterQueue.PublishAsync(request, result.Error);
            break;

        default:
            // Contract violation: return immediately to caller without retrying
            return result;
    }
}
```

---

## 2. Pipeline Recovery (`Recover`)

The **`.Recover()`** combinator functions as a functional `catch` block. It executes **only** if the preceding computation evaluates to a failure, allowing fallback values or replica fetches to seamlessly rescue the pipeline:

```csharp
Result<StockLevel> stockResult = await FetchLiveInventoryFromWarehouseAsync(productId)
    .Recover(err =>
    {
        _logger.LogWarning("Warehouse unreachable [{Code}]. Falling back to local replica.", err.Code);
        return FetchCachedInventoryReplica(productId);
    });
```

If the original operation succeeded, `.Recover()` is completely bypassed with zero invocation cost.

---

## 3. Error Transformation & Enrichment (`MapError`)

When infrastructure or persistence layers return technical diagnostics, application services can translate them into meaningful domain context:

```csharp
Result<CustomerProfile> profileResult = await _dbGateway.QueryCustomerAsync(id)
    .MapError(dbErr => dbErr.Type switch
    {
        ErrorType.NotFound => Error.NotFound("Customer.NotFound", $"Customer with ID '{id}' was not found."),
        _ => dbErr.WithSeverity(ErrorSeverity.Critical).WithMetadata("QueryTime", DateTime.UtcNow)
    });
```

---

## 4. Fallback Value Extraction (`MapFailure`)

When a degraded value must be derived specifically from a failure outcome (for instance, rendering an audit summary or UI fallback representation):

```csharp
string statusSummary = result.MapFailure(
    onFailure: err => $"Fallback: Operation could not proceed due to [{err.Code}]",
    successDefault: "Success"
);
```

---

## Next Level
Proceed to **[Level 07 — Scalability](level-07-scalability.md)** to master extreme memory optimizations, closure-free `TState` state-passing overloads, and NativeAOT compilation.
