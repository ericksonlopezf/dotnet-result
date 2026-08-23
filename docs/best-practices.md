# Best Practices

This document outlines the recommended design patterns and performance guidelines when developing with `EricksonLopez.Result`.

---

## 1. Never Use Exceptions for Expected Domain Violations

Exceptions incur significant runtime overhead due to stack trace captures and heap allocations. For expected business rule violations (e.g., "User not found", "Insufficient funds", "Validation failed"), always return `Result` or `Result<T>`.

```csharp
// ❌ BAD: Throwing exceptions for business rules
public User GetUser(Guid id)
{
    var user = _repo.Find(id);
    if (user is null)
        throw new NotFoundException($"User {id} not found.");
    return user;
}

// ✅ GOOD: Returning Result<T> with domain error
public Result<User> GetUser(Guid id)
{
    var user = _repo.Find(id);
    return user is null 
        ? Error.NotFound("User.NotFound", $"User with ID '{id}' was not found.") 
        : user; // Implicit conversion to Result<User>.Success
}
```

---

## 2. Eliminate Closure Allocations in Hot Paths with `TState`

Standard lambda expressions capturing local variables allocate heap-allocated closure display classes. Use the `TState` overloads with `static` lambdas to pass arguments without closure allocations:

```csharp
var tenantId = currentContext.TenantId;

// ❌ BAD: Allocates a compiler closure on every invocation
var result = GetOrder(id)
    .Ensure(order => order.TenantId == tenantId, Error.Forbidden("Order.InvalidTenant", "Invalid tenant access."));

// ✅ GOOD: Zero closure allocation via TState parameter and static lambda
var result = GetOrder(id)
    .Ensure(tenantId, static (tId, order) => order.TenantId == tId, Error.Forbidden("Order.InvalidTenant", "Invalid tenant access."));
```

---

## 3. Categorize Errors Semantically

Always assign appropriate `ErrorType`, `ErrorSeverity`, and `ErrorRetryability` when instantiating domain errors. This enables automatic HTTP status code mapping and automated retry behaviors in client libraries and message handlers.

```csharp
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

## 4. Use `ErrorBuilder` and `Result.ValidateAll` for Validation

When validating complex payloads with multiple independent constraints, collect all errors rather than failing fast on the first constraint:

```csharp
// 1. Using ErrorBuilder for imperative validation
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

// 2. Using Result.ValidateAll for declarative rule evaluation
public Result<Order> ValidateOrder(Order order)
{
    return Result.ValidateAll(
        order,
        static o => o.Items.Count > 0 ? Result.Success() : Error.Validation("Order.NoItems", "Order must contain items."),
        static o => o.TotalAmount > 0 ? Result.Success() : Error.Validation("Order.InvalidAmount", "Total must be positive.")
    );
}
```

---

## 5. Prefer `.Match()` over Manual Status Checks

Using `.Match()` forces consumers to handle both success and failure branches, preventing unhandled error bugs:

```csharp
// ✅ GOOD: Exhaustive pattern handling
IResult response = result.Match(
    dto => TypedResults.Ok(dto),
    error => TypedResults.Problem(error.Description, statusCode: (int)error.Type)
);
```

---

## 6. High-Throughput HTTP Endpoints: `.ToHttpResult()`

While `AddResultEndpointFilter()` offers convenient declarative unwrapping in Minimal APIs, it relies on `IResultOutcome` which boxes struct results on each request. For performance-critical endpoints (>10k req/s), call `.ToHttpResult()` directly:

```csharp
// ✅ OPTIMAL: Zero boxing, full OpenAPI type inference
app.MapGet("/orders/{id:guid}", async (Guid id, IOrderService svc) =>
{
    Result<OrderDto> result = await svc.GetOrderAsync(id);
    return result.ToHttpResult();
});
```

---

## 7. Explicit Collection Deduplication with `ErrorEqualityComparer.Strict`

By default, `Error.Equals()` compares the core semantic attributes (`Code`, `Description`, `Type`, `Severity`, `Retryability`). When storing `Error` instances in hash sets where per-request attributes (`TraceId`, `Metadata`) must be preserved, always use `ErrorEqualityComparer.Strict`:

```csharp
var errorSet = new HashSet<Error>(ErrorEqualityComparer.Strict);
```
