# Cookbook

This cookbook contains practical, copy-pasteable recipes for using `EricksonLopez.Result` across various application layers.

---

## Recipe 1: Complex Validation with `ErrorBuilder` & `Result.ValidateAll`

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
            builder = builder.WithInnerError(Error.Validation("User.UsernameRequired", "Username cannot be empty."));

        if (string.IsNullOrWhiteSpace(request.Email) || !request.Email.Contains('@'))
            builder = builder.WithInnerError(Error.Validation("User.InvalidEmail", "A valid email address is required."));

        if (request.Age < 18)
            builder = builder.WithInnerError(Error.Validation("User.Underage", "User must be at least 18 years old."));

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
Build an ASP.NET Core Minimal API endpoint that maps domain results directly to HTTP responses with OpenAPI schema support.

```csharp
using EricksonLopez.Result;
using EricksonLopez.Result.AspNetCore;
using EricksonLopez.Result.OpenApi;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddScoped<OrderService>();

var app = builder.Build();

// Method 1: Using .ToHttpResult() (Zero boxing, full OpenAPI metadata)
app.MapGet("/orders/{id:guid}", async (Guid id, OrderService service) =>
{
    Result<OrderDto> result = await service.GetOrderByIdAsync(id);
    return result.ToHttpResult();
});

// Method 2: Automatic Filter Unwrapping with ProducesResult<T>()
app.MapPost("/orders", async (CreateOrderCommand command, OrderService service) =>
{
    return await service.CreateOrderAsync(command); // Returns Result<OrderDto>
})
.AddResultEndpointFilter()
.ProducesResult<OrderDto>(StatusCodes.Status200OK);

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
    private static readonly ActivitySource ActivitySource = new("MyApp.Payments");

    public async Task<Result<PaymentReceipt>> ProcessPaymentAsync(PaymentRequest request)
    {
        using var activity = ActivitySource.StartActivity("ProcessPayment");

        Result<PaymentReceipt> result = await ExecutePaymentGatewayAsync(request);

        // Attaches tags (error.code, error.type, error.severity, error.retryable) and status
        result.TraceOutcome("ProcessPayment", activity);

        // Records metrics counter
        ResultMetrics.StaticTrackSuccess("ProcessPayment");

        return result;
    }
}
```

---

## Recipe 4: Strongly-Typed Domain Errors with `Result<TValue, TError>`

### Scenario
Model strict domain pipelines where compile-time errors are typed to specific domain error classes using `EricksonLopez.Result.Generic`.

```csharp
using EricksonLopez.Result.Generic;

public abstract record DomainError(string Message);
public record UserNotFoundError(Guid UserId) : DomainError($"User {UserId} was not found.");
public record InactiveAccountError(Guid UserId) : DomainError($"User {UserId} is inactive.");

public class UserService
{
    public Result<User, DomainError> GetActiveUser(Guid id)
    {
        var user = _repository.Find(id);
        if (user is null)
            return Result<User, DomainError>.Failure(new UserNotFoundError(id));

        if (!user.IsActive)
            return Result<User, DomainError>.Failure(new InactiveAccountError(id));

        return Result<User, DomainError>.Success(user);
    }
}
```

---

## Recipe 5: Optional DDD Repositories with `Maybe<T>`

### Scenario
Model queries where absence of an entity is a natural, non-error state using `EricksonLopez.Result.Maybe`.

```csharp
using EricksonLopez.Result;
using EricksonLopez.Result.Maybe;

public class UserRepository
{
    public async Task<Maybe<User>> FindByEmailAsync(string email)
    {
        User? user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == email);
        return Maybe<User>.From(user);
    }
}

// Seamless conversion from Maybe<T> to Result<T>
Maybe<User> maybeUser = await userRepo.FindByEmailAsync("alice@example.com");
Result<User> result = maybeUser.ToResult(Error.NotFound("User.NotFound", "User not found"));
```

---

## Recipe 6: FluentValidation Pipeline Integration

### Scenario
Integrate FluentValidation directly into a Result monadic chain using `EricksonLopez.Result.FluentValidation`.

```csharp
using EricksonLopez.Result;
using EricksonLopez.Result.FluentValidation;
using FluentValidation;

public class CreateCustomerCommandValidator : AbstractValidator<CreateCustomerCommand>
{
    public CreateCustomerCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Age).GreaterThanOrEqualTo(18);
    }
}

public async Task<Result<Guid>> HandleAsync(CreateCustomerCommand command)
{
    var validator = new CreateCustomerCommandValidator();

    // Convert ValidationResult directly to Result
    Result validationResult = validator.Validate(command).ToValidationResult();
    if (validationResult.IsFailure)
        return validationResult.Error;

    return await _customerService.CreateCustomerAsync(command);
}
```

---

## Recipe 7: MediatR Pipeline Behavior

### Scenario
Catch unhandled exceptions in MediatR command and query handlers and convert them automatically into structured `Result` failures.

```csharp
using EricksonLopez.Result;
using EricksonLopez.Result.MediatR;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));
builder.Services.AddResultExceptionBehavior();
```

---

## Recipe 8: Zero-Allocation `TState` Monadic Pipeline

### Scenario
Execute high-throughput validation pipelines passing contextual state without allocating closures.

```csharp
public Result<OrderDto> ValidateAndTransform(Order order, UserContext context)
{
    return Result.Success(order)
        .Ensure(context.TenantId, static (tenantId, o) => o.TenantId == tenantId, Error.Forbidden("Order.InvalidTenant", "Invalid tenant"))
        .Ensure(context.MaxAllowedTotal, static (max, o) => o.Total <= max, Error.Validation("Order.ExceedsMax", "Order exceeds limit"))
        .Map(static o => new OrderDto(o.Id, o.Total));
}
```

---

## Recipe 9: Unit Testing with Fluent Assertions

### Scenario
Write declarative unit tests with `EricksonLopez.Result.Testing` across xUnit or NUnit.

```csharp
using EricksonLopez.Result.Testing;
using Xunit;

public class OrderServiceTests
{
    [Fact]
    public void CreateOrder_ShouldSucceed_WhenInputIsValid()
    {
        Result<Order> result = _service.CreateOrder(validCommand);

        Order order = result.ShouldBeSuccess();
        Assert.Equal(100.0m, order.Total);
    }

    [Fact]
    public async Task GetOrderAsync_ShouldFail_WhenNotFound()
    {
        Result<Order> result = await _service.GetOrderAsync(Guid.NewGuid());

        result.ShouldBeFailure()
              .ShouldHaveErrorCode("Order.NotFound")
              .ShouldHaveErrorType(ErrorType.NotFound);
    }
}
```

---

## Recipe 10: LINQ Query Syntax with `Result<T>`

### Scenario
Chain multiple dependent computations using idiomatic C# LINQ query syntax.

```csharp
Result<int> a = Result.Success(10);
Result<int> b = Result.Success(20);

Result<int> total = 
    from x in a
    from y in b
    select x + y;

// total.Value == 30
```

---

## Recipe 11: Corrective Fallback with `Recover`

### Problem
An operation fails with a transient error and you want to attempt a fallback before propagating the failure.

### Solution

```csharp
using EricksonLopez.Result;

public async Task<Result<int>> GetInventoryCountAsync(string sku)
{
    Result<int> liveResult = await _inventoryService.GetLiveCountAsync(sku);

    return liveResult.Recover(error =>
    {
        if (error.Type == ErrorType.Unavailable)
        {
            int cached = _cache.GetInventory(sku);
            return Result.Success(cached);
        }
        return liveResult; // re-propagate non-recoverable errors
    });
}
```

### With TState (allocation-free in hot paths):

```csharp
return liveResult.Recover(
    state: (sku, _cache),
    recover: (ctx, error) =>
        error.Type == ErrorType.Unavailable
            ? Result.Success(ctx._cache.GetInventory(ctx.sku))
            : Result.Failure<int>(error)
);
```

### Best Practices
- Only recover from errors your code genuinely handles.
- Re-propagate unhandled errors by returning the original failure.
- Prefer the `<TState>` overload in hot paths to avoid closure allocations.

---

## Recipe 12: Error Enrichment with `MapError`

### Problem
An infrastructure layer returns a generic error; the application layer needs to enrich it with business context.

### Solution

```csharp
return await _paymentService.AuthorizeAsync(cmd.PaymentToken)
    .MapError(error => error.ToBuilder()
        .WithMetadata("orderId", cmd.OrderId)
        .WithRetryability(ErrorRetryability.Transient)
        .Build())
    .Bind(auth => _orderRepository.SaveAsync(cmd, auth));
```

### Common Pitfalls
- Do not change the semantic error type with `MapError`; use it only for enrichment.
- `MapError` is a no-op on success.

---

## Recipe 13: Extracting a Value from Failure with `MapFailure`

### Problem
Convert a `Result<T>` into a plain value regardless of outcome.

### Solution

```csharp
public string GetUserDisplayName(Guid userId)
{
    Result<User> result = _userRepository.Find(userId);

    return result.MapFailure(
        onSuccess: user => user.DisplayName,
        onFailure: error => $"[Unknown user — {error.Code}]"
    );
}
```

---

## Recipe 14: Removing the Value Type with `DiscardValue`

### Problem
Your application service returns `Result<T>` but the calling code only cares whether the operation succeeded.

### Solution

```csharp
public async Task<Result> DispatchAsync(CreateOrderCommand cmd)
{
    Result<Order> result = await _orderService.CreateOrderAsync(cmd);
    return result.DiscardValue();
}
```

---

## Recipe 15: Safe Value Extraction — `TryGetValue` and `Deconstruct`

### Problem
Extract the value from a `Result<T>` using the BCL TryXxx pattern.

### Solution

```csharp
Result<Order> result = _orderService.GetOrder(orderId);

// TryGetValue — BCL TryXxx convention
if (result.TryGetValue(out Order? order))
    Console.WriteLine($"Order total: {order.Total:C}");

// Deconstruct (3-ple)
var (isSuccess, value, error) = result;

// Detect uninitialized state
bool hasValue = maybeUninitialized.TryGetValue(out var val, out bool isUninitialized);
```

---

## Recipe 16: Exception Safety with `Result.Try` and `Result.TryAsync`

### Problem
Wrap legacy or external APIs that throw exceptions into Result pipelines.

### Solution

```csharp
// Sync
Result<string> config = Result.Try(
    () => File.ReadAllText("/etc/config.json"),
    ex => Error.Infrastructure("Config.ReadFailed", ex.Message)
);

// Async with cancellation
Result<PaymentReceipt> payment = await Result.TryAsync(
    state: operationName,
    action: async (ct) => await _gateway.ChargeAsync(amount, ct),
    errorHandler: (opName, ex) => Error.Unexpected($"{opName}.Failed", ex.Message),
    cancellationToken: cancellationToken
);
```

### Best Practices
- Handle `OperationCanceledException` explicitly if you want cancellation to propagate.
- Fatal exceptions (`OutOfMemoryException`, `StackOverflowException`) are never caught.

---

## Recipe 17: Detecting Compound Errors with `WellKnownErrors`

### Problem
After `Result.Combine` or `Result.ValidateAll`, detect and handle compound errors specifically.

### Solution

```csharp
if (combined.IsFailure && combined.Error.Code == WellKnownErrors.CombinedFailuresCode)
{
    foreach (Error inner in combined.Error.InnerErrors)
        _logger.LogWarning("[{Code}] {Desc}", inner.Code, inner.Description);
}
```

### Best Practices
- Use `WellKnownErrors.CombinedFailuresCode` — never hardcode `"Result.CombinedErrors"`.
- Use `WellKnownErrors.UninitializedError` to detect `default(Result)` in diagnostic code.
