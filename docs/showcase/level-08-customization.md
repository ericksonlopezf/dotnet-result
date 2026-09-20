# Level 08 — Customization: Extensibility, Comparers & Polymorphism

> **Ecosystem:** `EricksonLopez.Result` | **Audience:** Framework Designers, Tech Leads | **Complexity:** Level 8 (Advanced) | **Code Reference:** `12_ErrorEqualityAndMutation.cs`, `21_AdvancedApiCoverage.cs` | **Language:** English

---

## 1. The Polymorphic Interface `IResultOutcome`

Both the non-generic `Result` struct and the typed `Result<TValue>` struct implement the public interface:

```csharp
public interface IResultOutcome
{
    bool IsSuccess { get; }
    bool IsFailure { get; }
    bool IsUninitialized { get; }
}
```

### Use Case: Agnostic Middleware & Interceptors
This interface enables infrastructure components, filters, and logging behaviors to inspect execution outcomes without knowing the concrete generic type parameter `TValue`:

```csharp
public static void AuditOutcome(IResultOutcome outcome, string operationName)
{
    if (outcome.IsSuccess)
    {
        Console.WriteLine($"[AUDIT] Operation '{operationName}' completed successfully.");
    }
    else if (outcome.IsFailure)
    {
        Console.WriteLine($"[AUDIT] Operation '{operationName}' failed.");
    }
}
```

---

## 2. Error Equality: Semantic vs Structural (`ErrorEqualityComparer`)

The design of `Error` distinguishes semantic equivalence from deep structural identity:

### 2.1 Semantic Equality (`Error.Equals` / `ErrorEqualityComparer.Default`)
Compares the functional domain identity of the error:
- `Code`
- `Description`
- `Type`
- `Severity`
- `Retryability`

> **Note:** Two errors with the same code and classification are semantically equal **even if they carry distinct request-scoped IDs (`TraceId`, `CorrelationId`)**.

### 2.2 Deep Structural Equality (`Error.StrictEquals` / `ErrorEqualityComparer.Strict`)
Compares all diagnostic properties, including:
- `TraceId` and `CorrelationId`
- `Metadata` key-value pairs
- Recursive child `InnerErrors`

```csharp
Error e1 = Error.Validation("Email.Invalid", "Invalid format").WithTraceId("trace-1");
Error e2 = Error.Validation("Email.Invalid", "Invalid format").WithTraceId("trace-2");

// 1. Default equality (Semantic)
bool defaultEqual = e1.Equals(e2); // TRUE

// 2. Strict equality (Structural)
bool strictEqual = e1.StrictEquals(e2); // FALSE

// 3. Usage in collections (HashSet / Dictionary)
var semanticSet = new HashSet<Error>(ErrorEqualityComparer.Default);
semanticSet.Add(e1);
semanticSet.Add(e2);
Console.WriteLine(semanticSet.Count); // 1

var strictSet = new HashSet<Error>(ErrorEqualityComparer.Strict);
strictSet.Add(e1);
strictSet.Add(e2);
Console.WriteLine(strictSet.Count); // 2
```

---

## 3. Custom Error Factory (`Error.Custom`)

For domain models with specialized requirements outside well-known factories, `Error.Custom` provides explicit control over all taxonomy parameters:

```csharp
Error customError = Error.Custom(
    code: "Security.AnomalyDetected",
    description: "Geographic IP mismatch with established session.",
    type: ErrorType.Forbidden,
    severity: ErrorSeverity.Critical,
    retryability: ErrorRetryability.Permanent
);
```

---

## Next Level
Proceed to **[Level 09 — Extensions](level-09-extensions.md)** to discover officially supported ecosystem packages for ASP.NET Core, FluentValidation, MediatR, OpenTelemetry, and System.Text.Json.
