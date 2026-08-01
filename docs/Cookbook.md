# Cookbook

This cookbook contains practical, copy-pasteable recipes for using `EricksonLopez.Result` across various application layers.

---

## Recipe 1: Complex Validation with `ErrorBuilder` & `Result.Combine`

### Scenario
Validate an incoming user registration payload and aggregate all validation errors into a single compound `Error`.

```csharp
using EricksonLopez.Result;

public record RegisterUserRequest(string Username, string Email, int Age);

public class UserValidator
{
    public Result Validate(RegisterUserRequest request)
    {
        var builder = ErrorBuilder.Validation("User.ValidationFailed", "User registration validation failed.");

        if (string.IsNullOrWhiteSpace(request.Username))
            builder.WithInnerError(Error.Validation("User.UsernameRequired", "Username cannot be empty."));

        if (string.IsNullOrWhiteSpace(request.Email) || !request.Email.Contains('@'))
            builder.WithInnerError(Error.Validation("User.InvalidEmail", "A valid email address is required."));

        if (request.Age < 18)
            builder.WithInnerError(Error.Validation("User.Underage", "User must be at least 18 years old."));

        return builder.HasInnerErrors 
            ? builder.Build() 
            : Result.Success();
    }
}
```

### Combining Multiple Typed Results (Tuples)

```csharp
Result<User> userResult = GetUser(userId);
Result<Account> accountResult = GetAccount(accountId);

// Combine returns Result<(User, Account)> if both succeed, or aggregates failures
Result<(User User, Account Account)> combined = Result.Combine(userResult, accountResult);

if (combined.TryGetValue(out var pair))
{
    Console.WriteLine($"User {pair.User.Name} has account {pair.Account.Id}");
}
```

---

## Recipe 2: ASP.NET Core Minimal APIs & RFC 9457 ProblemDetails

### Scenario
Build an ASP.NET Core endpoint that maps domain results directly to HTTP responses.

```csharp
using EricksonLopez.Result;
using EricksonLopez.Result.AspNetCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddScoped<OrderService>();

var app = builder.Build();

// Method 1: Using .ToHttpResult()
app.MapGet("/orders/{id:guid}", async (Guid id, OrderService service) =>
{
    Result<OrderDto> result = await service.GetOrderByIdAsync(id);
    return result.ToHttpResult();
});

// Method 2: Automatic Filter Unwrapping
app.MapPost("/orders", async (CreateOrderCommand command, OrderService service) =>
{
    return await service.CreateOrderAsync(command); // Returns Result<OrderDto>
})
.AddEndpointFilter<ResultEndpointFilter>();

app.Run();
```

---

## Recipe 3: Distributed Tracing & OpenTelemetry Metrics

### Scenario
Instrument a critical payment operation with OpenTelemetry `ActivitySource` tracing and metrics.

```csharp
using System.Diagnostics;
using EricksonLopez.Result;
using EricksonLopez.Result.OpenTelemetry;

public class PaymentProcessor
{
    private static readonly ActivitySource ActivitySource = new("MyCompany.PaymentSystem");

    public async Task<Result<PaymentConfirmation>> ProcessPaymentAsync(PaymentRequest request)
    {
        using var activity = ActivitySource.StartActivity("ProcessPayment");
        activity?.SetTag("payment.amount", request.Amount);

        var result = await ExecutePaymentGatewayCallAsync(request, cancellationToken);

        // Attaches error tags (code, type, severity, retryability) and sets ActivityStatus
        activity?.RecordResult(result);

        // Increments counters and records duration in System.Diagnostics.Metrics
        ResultMetrics.RecordOutcome("ProcessPayment", result);

        return result;
    }
}
```

---

## Recipe 4: NativeAOT-Safe System.Text.Json Serialization

### Scenario
Serialize `Result<T>` in a NativeAOT-compiled application without reflection warnings.

```csharp
using System.Text.Json;
using System.Text.Json.Serialization;
using EricksonLopez.Result;
using EricksonLopez.Result.Serialization;

// 1. Define custom JsonSerializerContext for NativeAOT
[JsonSerializable(typeof(Result<UserDto>))]
[JsonSerializable(typeof(Error))]
public partial class AppJsonContext : JsonSerializerContext
{
}

// 2. Serialize using NativeAOT Context
public class JsonSerializerService
{
    public string SerializeResult(Result<UserDto> result)
    {
        return JsonSerializer.Serialize(result, AppJsonContext.Default.ResultUserDto);
    }

    public Result<UserDto> DeserializeResult(string json)
    {
        return JsonSerializer.Deserialize(json, AppJsonContext.Default.ResultUserDto);
    }
}
```

---

## Recipe 5: Fluent Unit Testing with `EricksonLopez.Result.Testing`

### Scenario
Write expressive unit tests for a domain service using fluent assertions.

```csharp
using EricksonLopez.Result;
using EricksonLopez.Result.Testing;
using Xunit;

public class InventoryServiceTests
{
    [Fact]
    public void ReserveStock_ShouldSucceed_WhenStockIsAvailable()
    {
        var service = new InventoryService();
        Result<StockReservation> result = service.ReserveStock(productId: Guid.NewGuid(), quantity: 5);

        // Fluent Assertion Chain
        result.ShouldBeSuccess()
              .Value.Quantity.ShouldBe(5);
    }

    [Fact]
    public async Task ReserveStockAsync_ShouldFail_WhenInsufficientStock()
    {
        var service = new InventoryService();
        Result<StockReservation> result = await service.ReserveStockAsync(productId: Guid.NewGuid(), quantity: 9999);

        // Assert Failure and specific Error code/type asynchronously without deadlock
        await result.ShouldBeFailureAsync()
                    .ShouldHaveErrorAsync("Inventory.InsufficientStock")
                    .ShouldHaveErrorTypeAsync(ErrorType.Conflict);
    }
}
```

---

## Recipe 6: LINQ Query Syntax for Monadic Composition

### Scenario
Compose multiple dependent operations using LINQ comprehension syntax (`from ... in ... select`).

```csharp
using EricksonLopez.Result;

public Result<OrderSummary> CreateOrderSummary(Guid userId, Guid productId)
{
    var summaryResult =
        from user in GetUser(userId)
        from product in GetProduct(productId)
        from discount in CalculateDiscount(user, product)
        select new OrderSummary(user.Name, product.Title, product.Price - discount);

    return summaryResult;
}
```

---

## Recipe 7: High-Performance Async Pipelines with `ValueTask<Result<T>>`

### Scenario
Execute async pipelines using `ValueTask<Result<T>>` to avoid task object heap allocations on sync paths.

```csharp
using EricksonLopez.Result;

public async ValueTask<Result<CachedProductDto>> GetProductAsync(Guid productId, CancellationToken cancellationToken = default)
{
    // Synchronous hot path (Cache Hit) returns zero-allocation ValueTask
    if (_cache.TryGetValue(productId, out CachedProductDto? dto))
    {
        return dto!;
    }

    // Asynchronous fallback path
    return await _db.GetProductByIdAsync(productId, cancellationToken)
        .Map(p => new CachedProductDto(p.Id, p.Name), cancellationToken)
        .Tap(dto => _cache.Set(productId, dto), cancellationToken);
}
```
