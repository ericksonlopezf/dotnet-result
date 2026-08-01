# Best Practices

This document outlines the recommended design patterns and performance guidelines when using `EricksonLopez.Result`.

---

## 1. Never Use Exceptions for Expected Domain Violations

Exceptions should be reserved for truly exceptional, unrecoverable system failures (e.g., database connection failure, out-of-memory, network timeouts). For expected business rule violations (e.g., "User not found", "Insufficient balance", "Invalid email format"), always return a `Result` or `Result<T>`.

```csharp
// ❌ BAD: Throwing exceptions for business rules
public User GetUser(Guid id)
{
    var user = _repo.Find(id);
    if (user is null)
        throw new NotFoundException($"User {id} not found."); // High overhead!
    return user;
}

// ✅ GOOD: Returning Result<T> with domain error
public Result<User> GetUser(Guid id)
{
    var user = _repo.Find(id);
    return user is null 
        ? Error.NotFound("User.NotFound", $"User {id} not found.") 
        : user;
}
```

---

## 2. Eliminate Closure Allocations in Hot Paths with `TState`

In high-throughput loops or performance-critical services, standard lambda expressions that capture surrounding variables force the runtime to instantiate a heap-allocated closure object. Use the `TState` overloads with `static` lambdas to achieve zero closure allocations.

```csharp
var tenantId = currentContext.TenantId;

// ❌ BAD: Captures 'tenantId' in a closure object on every execution
var result = GetOrder(id)
    .Ensure(order => order.TenantId == tenantId, Error.Forbidden("Order.InvalidTenant", "Invalid tenant"));

// ✅ GOOD: Zero closure allocation via TState parameter and static lambda
var result = GetOrder(id)
    .Ensure(tenantId, static (tId, order) => order.TenantId == tId, Error.Forbidden("Order.InvalidTenant", "Invalid tenant"));
```

---

## 3. Categorize Errors Semantically

Always assign appropriate `ErrorType`, `ErrorSeverity`, and `ErrorRetryability` when instantiating domain errors. This enables automatic HTTP status code mapping and downstream retry logic.

```csharp
// Define specialized static error domain classes
public static class PaymentErrors
{
    public static readonly Error InsufficientFunds =
        Error.Conflict("Payment.InsufficientFunds", "Account balance is insufficient.")
             .WithSeverity(ErrorSeverity.Warning)
             .WithRetryability(ErrorRetryability.Permanent);

    public static readonly Error GatewayTimeout =
        Error.Unavailable("Payment.GatewayTimeout", "Payment gateway did not respond.")
             .WithSeverity(ErrorSeverity.Error)
             .WithRetryability(ErrorRetryability.Transient);
}
```

---

## 4. Use `ErrorBuilder` for Complex Validation Failures

When validating incoming DTOs or command payloads with multiple constraints, collect field errors using `ErrorBuilder` rather than failing on the very first invalid field.

```csharp
public Result<Customer> CreateCustomer(CreateCustomerCommand command)
{
    var builder = ErrorBuilder.Validation("Customer.InvalidPayload", "Customer validation failed.");

    if (string.IsNullOrWhiteSpace(command.Name))
        builder.WithInnerError(Error.Validation("Customer.NameRequired", "Name is required."));

    if (command.Age < 18)
        builder.WithInnerError(Error.Validation("Customer.Underage", "Customer must be at least 18 years old."));

    if (builder.HasInnerErrors)
        return builder.Build();

    return new Customer(command.Name, command.Age);
}
```

---

## 5. Prefer `.Match()` over Manual `.IsSuccess` / `.IsFailure` Checks

Using `.Match()` forces consumers to handle both success and failure branches explicitly, preventing unhandled error bugs.

```csharp
// ❌ RISKY: Manual check can be forgotten or lead to accidental .Value access
if (result.IsSuccess)
{
    DoSomething(result.Value);
}

// ✅ RECOMMENDED: Exhaustive pattern matching
return result.Match(
    value => Results.Ok(value),
    error => Results.BadRequest(error)
);
```

---

## 6. Standardize Web API Responses with `ToHttpResult()`

In ASP.NET Core Minimal APIs, use `ToHttpResult()` or `ResultEndpointFilter` to automatically convert `Result` outcomes into RFC 9457 compliant ProblemDetails.

```csharp
app.MapGet("/products/{id:guid}", (Guid id, ProductService service) =>
{
    return service.GetProduct(id).ToHttpResult();
});
```

---

## 7. Instrument Spans with `RecordResult` in OpenTelemetry

When creating custom OpenTelemetry activities, always record the `Result` outcome so tracing dashboards (Jaeger, Grafana Tempo, Azure Monitor) highlight failures correctly.

```csharp
using var activity = MyActivitySource.StartActivity("ProcessOrder");
var result = await ProcessOrderInternalAsync();

// Sets activity status and attaches tags automatically
activity?.RecordResult(result);
return result;
```
