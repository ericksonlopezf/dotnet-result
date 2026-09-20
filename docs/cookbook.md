# Cookbook: Official Recipes for `EricksonLopez.Result`

This cookbook contains practical, production-ready recipes for using `EricksonLopez.Result` and its official ecosystem extensions across all architectural layers.

Every recipe follows a strict 6-part specification:
1. **Problem**
2. **Solution**
3. **Complete Code**
4. **Explanation**
5. **Best Practices**
6. **Common Pitfalls**

---

## Recipe 1: Complex Validation and Error Accumulation with `ErrorBuilder` & `Result.ValidateAll`

### Problem
Validating an incoming complex payload requires executing multiple independent validation rules, collecting all failures without aborting at the first error, and returning a single aggregated `Error` if any rule fails.

### Solution
Use `Result.ValidateAll` to evaluate multiple validation predicates with zero extra heap allocations (leveraging pooled memory), or use `ErrorBuilder` to fluently accumulate child errors into an immutable `Error` containing nested `InnerErrors`.

### Complete Code
```csharp
using System;
using EricksonLopez.Result;

public sealed record RegisterUserRequest(string Username, string Email, int Age);

public static class UserValidationService
{
    public static Result Validate(RegisterUserRequest request)
    {
        return Result.ValidateAll(
            request,
            req => !string.IsNullOrWhiteSpace(req.Username)
                ? Result.Success()
                : Error.Validation("User.UsernameRequired", "Username cannot be empty."),
            req => !string.IsNullOrWhiteSpace(req.Email) && req.Email.Contains('@')
                ? Result.Success()
                : Error.Validation("User.InvalidEmail", "A valid email address is required."),
            req => req.Age >= 18
                ? Result.Success()
                : Error.Validation("User.Underage", "User must be at least 18 years old.")
        );
    }
}
```

### Explanation
`Result.ValidateAll` executes each validator against the supplied state. If all rules succeed, it returns `Result.Success()`. If a single rule fails, it returns that specific `Error`. If two or more rules fail, it constructs a compound `Error.Validation` with code `WellKnownErrors.CombinedFailuresCode` and aggregates the individual errors in its `InnerErrors` array.

### Best Practices
- Use `Result.ValidateAll` when validating domain models to present comprehensive feedback to clients in a single round-trip.
- Keep individual validator functions pure and free of side-effects.

### Common Pitfalls
- Short-circuiting manually with `if (!result.IsSuccess) return result;`, which forces users to fix validation errors one by one.

---

## Recipe 2: ASP.NET Core Minimal APIs & RFC 9457 ProblemDetails

### Problem
HTTP endpoints need to translate domain `Result` and `Result<T>` outcomes into standardized HTTP responses without repetitive boilerplate or leaked internal exceptions, adhering to RFC 9457 ProblemDetails.

### Solution
Use `.ToHttpResult()` on `Result<T>` to map outcomes to `IResult`, or attach `.AddResultEndpointFilter()` to minimal API routes to automatically unwrap results.

### Complete Code
```csharp
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using EricksonLopez.Result;
using EricksonLopez.Result.AspNetCore;
using EricksonLopez.Result.OpenApi;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddResultHttpOptions(options =>
{
    options.SetStatusCode(ErrorType.Validation, StatusCodes.Status400BadRequest);
    options.SetStatusCode(ErrorType.NotFound, StatusCodes.Status404NotFound);
    options.SetStatusCode(ErrorType.Conflict, StatusCodes.Status409Conflict);
});

var app = builder.Build();

app.MapGet("/users/{id:guid}", async (Guid id) =>
{
    Result<string> result = id != Guid.Empty
        ? Result.Success($"User-{id}")
        : Error.NotFound("User.NotFound", $"User '{id}' does not exist.");

    return result.ToHttpResult();
})
.ProducesResult<string>(StatusCodes.Status200OK)
.ProducesResultProblemDetails(StatusCodes.Status404NotFound);

app.Run();
```

### Explanation
`.ToHttpResult()` inspects the outcome: if successful, it emits `TypedResults.Ok(value)`. If failed, it translates `Error.Type` to the configured HTTP status code and emits a `ProblemDetails` response with error code, description, and metadata extensions.

### Best Practices
- Centralize status code mappings using `builder.Services.AddResultHttpOptions(...)`.
- Annotate endpoints with `.ProducesResult<T>()` and `.ProducesResultProblemDetails()` for Swagger/OpenAPI schema generation.

### Common Pitfalls
- Returning `result.Value` directly without checking `IsSuccess`, which throws `InvalidOperationException` on failure.

---

## Recipe 3: Distributed Tracing & OpenTelemetry Metrics

### Problem
Critical business operations need W3C-compliant distributed tracing and metrics instrumentation without polluting domain logic with APM-specific boilerplate.

### Solution
Use `ResultActivityExtensions.TraceOutcome` to enrich ambient `System.Diagnostics.Activity.Current` spans and emit BCL runtime metrics using `ResultMetrics`.

### Complete Code
```csharp
using System.Diagnostics;
using System.Threading.Tasks;
using EricksonLopez.Result;
using EricksonLopez.Result.OpenTelemetry;

public static class OrderProcessor
{
    private static readonly ActivitySource Source = new("ECommerce.Orders");

    public static async Task<Result<string>> ProcessOrderAsync(string orderId)
    {
        using Activity? activity = Source.StartActivity("ProcessOrder");

        Result<string> outcome = orderId.StartsWith("ORD-")
            ? Result.Success("ORDER_CONFIRMED")
            : Error.Validation("Order.InvalidFormat", "Order ID must start with ORD-");

        outcome.TraceOutcome("ProcessOrder", activity);

        if (outcome.IsSuccess)
            ResultMetrics.StaticTrackSuccess("ProcessOrder");
        else
            ResultMetrics.StaticTrackFailure("ProcessOrder", outcome.Error);

        return outcome;
    }
}
```

### Explanation
`TraceOutcome` sets `Activity.SetStatus` to `ActivityStatusCode.Ok` on success or `ActivityStatusCode.Error` on failure, attaching standard tags (`ericksonlopez.result.outcome`, `error.code`, `error.type`). `ResultMetrics` increments BCL `System.Diagnostics.Metrics` counters.

### Best Practices
- Pass the ambient `Activity.Current` or created `Activity` directly to `TraceOutcome`.
- Register `.AddResultInstrumentation()` in `IServiceCollection` during application bootstrap.

### Common Pitfalls
- Neglecting to pass the operation name to `TraceOutcome`, making it harder to filter spans in Grafana/Jaeger.

---

## Recipe 4: Strongly-Typed Domain Errors with `Result<TValue, TError>`

### Problem
Certain domain contexts require compile-time exhaustive checking of error types, preventing callers from handling arbitrary string-based errors.

### Solution
Use `Result<TValue, TError>` from `EricksonLopez.Result.Generic` with a closed enum, record hierarchy, or discriminated union.

### Complete Code
```csharp
using EricksonLopez.Result.Generic;

public enum PaymentErrorCode
{
    InsufficientFunds,
    CardExpired,
    NetworkTimeout
}

public static class PaymentGateway
{
    public static Result<string, PaymentErrorCode> Authorize(decimal amount, decimal balance)
    {
        if (amount > balance)
            return PaymentErrorCode.InsufficientFunds;

        return "AUTH_SUCCESS_TOKEN";
    }
}
```

### Explanation
`Result<TValue, TError>` is a zero-allocation readonly struct parameterized on both success payload and error type. Implicit conversions allow returning either `TValue` or `TError` directly.

### Best Practices
- Use `Result<TValue, TError>` for internal domain boundaries where all failure modes are finite and statically known.
- Map `Result<TValue, TError>` to standard `Result<TValue>` at the application boundary if interacting with ASP.NET Core or MassTransit.

### Common Pitfalls
- Using generic error monads for public APIs where extensible error codes and metadata are required.

---

## Recipe 5: Optional DDD Repositories with `Maybe<T>`

### Problem
Querying a repository or cache for an entity may return nothing. Using `null` leads to `NullReferenceException` risks, while returning `Result<T>` with an error may mischaracterize an expected absence as a system failure.

### Solution
Use `Maybe<T>` from `EricksonLopez.Result.Maybe` to model presence (`Some`) or absence (`None`) safely.

### Complete Code
```csharp
using System.Collections.Generic;
using EricksonLopez.Result;
using EricksonLopez.Result.Maybe;

public sealed class UserRepository
{
    private readonly Dictionary<int, string> _users = new() { [1] = "Alice" };

    public Maybe<string> FindById(int id)
    {
        return _users.TryGetValue(id, out string? name)
            ? Maybe.Some(name)
            : Maybe.None<string>();
    }

    public Result<string> GetUserOrError(int id)
    {
        return FindById(id).ToResult(Error.NotFound("User.NotFound", $"User {id} does not exist."));
    }
}
```

### Explanation
`Maybe<T>` is a value-type struct that cannot be null. It provides `.HasValue`, `.Value`, `.Match()`, and `.ToResult(error)` to transition seamlessly between optionality and Railway-Oriented error handling.

### Best Practices
- Return `Maybe<T>` from read-only lookup methods where entity absence is an expected, normal condition.
- Use `.ToResult(notFoundError)` at the service layer when absence should halt execution as a failure.

### Common Pitfalls
- Accessing `.Value` without checking `.HasValue`, which throws `InvalidOperationException`.

---

## Recipe 6: FluentValidation Pipeline Integration

### Problem
Validating command models using FluentValidation produces a third-party `ValidationResult` that must be manually checked and converted into the application's domain error model.

### Solution
Use `.ToValidationResult()` or `.EnsureValid()` from `EricksonLopez.Result.FluentValidation` to bridge FluentValidation with `Result<T>`.

### Complete Code
```csharp
using System.Threading.Tasks;
using FluentValidation;
using EricksonLopez.Result;
using EricksonLopez.Result.FluentValidation;

public sealed record CreateProductCommand(string Sku, decimal Price);

public sealed class CreateProductValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductValidator()
    {
        RuleFor(x => x.Sku).NotEmpty().MinimumLength(3);
        RuleFor(x => x.Price).GreaterThan(0);
    }
}

public static class ProductService
{
    public static async Task<Result<CreateProductCommand>> ValidateCommandAsync(
        CreateProductCommand command,
        IValidator<CreateProductCommand> validator)
    {
        var validation = await validator.ValidateAsync(command);
        return validation.ToValidationResult(command);
    }
}
```

### Explanation
`ToValidationResult` evaluates `ValidationResult.IsValid`. If false, it creates an `Error.Validation` with code `"Validation.Failed"` and maps each `ValidationFailure` into nested inner errors, capturing property names, error messages, and attempted values.

### Best Practices
- Use `.EnsureValid(validator)` inside monadic pipelines to validate intermediate objects fluently.
- Avoid logging sensitive fields (like passwords); the converter sanitizes common credential fields.

### Common Pitfalls
- Calling `.Validate(command)` synchronously on asynchronous validators.

---

## Recipe 7: CQRS Handler Exception Shielding with MediatR Pipeline Behavior

### Problem
Unhandled exceptions thrown inside MediatR request handlers bypass functional result handling, leaking internal stack traces to callers.

### Solution
Register `ResultExceptionBehavior<TRequest, TResponse>` from `EricksonLopez.Result.MediatR` to catch unhandled exceptions and convert them into `Result.Failure(Error.Unexpected(...))`.

### Complete Code
```csharp
using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using EricksonLopez.Result;
using EricksonLopez.Result.MediatR;

public sealed record GetUserProfileQuery(int UserId) : IRequest<Result<string>>;

public sealed class GetUserProfileHandler : IRequestHandler<GetUserProfileQuery, Result<string>>
{
    public Task<Result<string>> Handle(GetUserProfileQuery request, CancellationToken ct)
    {
        if (request.UserId <= 0)
            throw new InvalidOperationException("Invalid database query state.");

        return Task.FromResult(Result.Success($"Profile-{request.UserId}"));
    }
}

public static class MediatRRegistration
{
    public static void Configure(IServiceCollection services)
    {
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssemblyContaining<GetUserProfileHandler>();
            cfg.AddResultExceptionBehavior();
        });
    }
}
```

### Explanation
When a handler throws, `ResultExceptionBehavior` catches the exception and inspects `TResponse`. If `TResponse` is `Result` or `Result<T>`, it invokes the configured error factory to return a failed result with `Error.Unexpected`, including exception message and type.

### Best Practices
- Add `ResultExceptionBehavior` as the outermost pipeline behavior in MediatR.
- Customize the error factory in `AddResultExceptionBehavior(factory)` if custom error classification is required.

### Common Pitfalls
- Registering the behavior for handlers whose responses do not implement `Result` or `Result<T>`.

---

## Recipe 8: Zero-Allocation `TState` Monadic Pipeline

### Problem
In high-throughput loops or hot execution paths, standard LINQ or lambda closures capture local variables, causing GC Gen0 allocations that degrade throughput.

### Solution
Use `ResultSyncExtensions` overloads accepting an explicit `TState` argument to execute transformations with static delegates and 0 bytes of heap allocation.

### Complete Code
```csharp
using EricksonLopez.Result;

public static class HighThroughputPipeline
{
    public static Result<int> CalculateDiscount(int basePrice, decimal rate)
    {
        Result<int> initial = Result.Success(basePrice);

        return initial
            .Ensure(
                state: 0,
                predicate: static (price, min) => price > min,
                error: Error.Validation("Price.Invalid", "Price must be greater than zero."))
            .Map(
                state: rate,
                selector: static (price, r) => (int)(price * (1m - r)));
    }
}
```

### Explanation
By passing `state` explicitly and defining the lambda as `static`, C# eliminates closure class instantiation. The arguments are passed on the stack or in CPU registers.

### Best Practices
- Use `static` lambdas to enforce that no ambient variables are captured accidentally.
- Group multiple parameters into a value tuple `(a, b)` for `TState`.

### Common Pitfalls
- Capturing outer variables inside the lambda, which defeats the zero-allocation guarantee.

---

## Recipe 9: Unit Testing with Fluent Assertions

### Problem
Writing unit test assertions with standard `Assert.True(result.IsSuccess)` produces uninformative failure messages and requires manually checking and unwrapping `.Value`.

### Solution
Use `ResultAssertions` from `EricksonLopez.Result.Testing` to perform fluent, type-safe assertions with descriptive diagnostics.

### Complete Code
```csharp
using EricksonLopez.Result;
using EricksonLopez.Result.Testing;

public static class OrderValidationTests
{
    public static void ExecuteTests()
    {
        // 1. Assert Success and safely unwrap value
        Result<int> successResult = Result.Success(42);
        int value = successResult.ShouldBeSuccess(); // returns 42

        // 2. Assert Failure and inspect Error structure
        Result failureResult = Result.Failure(
            Error.Validation("User.InvalidEmail", "Email format is incorrect."));

        failureResult.ShouldBeFailure()
                     .ShouldHaveCode("User.InvalidEmail")
                     .ShouldHaveType(ErrorType.Validation);
    }
}
```

### Explanation
`ShouldBeSuccess()` verifies `IsSuccess == true` and returns the inner `TValue`. If the result was a failure, it throws a descriptive exception containing the error code and description. `ShouldBeFailure()` returns an `ErrorAssertions` builder for chaining assertions.

### Best Practices
- Always use `ShouldBeSuccess()` to obtain the value in tests rather than directly calling `.Value`.
- Chain `.ShouldHaveCode(...)` and `.ShouldHaveType(...)` to assert exact failure semantics.

### Common Pitfalls
- Testing `result.IsSuccess` with equality assertions, which hides the failure error message when tests break.

---

## Recipe 10: Declarative Monadic Composition with LINQ Query Syntax

### Problem
Chaining multiple sequential operations with `.Bind()` can lead to nested callbacks when downstream operations require access to variables from multiple preceding steps.

### Solution
Use `ResultLinqExtensions` (`Select`, `SelectMany`, `Where`) to compose multiple `Result<T>` steps declaratively using C# query syntax.

### Complete Code
```csharp
using EricksonLopez.Result;

public static class OrderCheckoutWorkflow
{
    public static Result<decimal> ProcessCheckout(int customerId, int cartId)
    {
        return from customer in GetCustomer(customerId)
               from cart in GetCart(cartId)
               where cart.ItemCount > 0
               let total = cart.Subtotal * (1m - customer.DiscountRate)
               select total;
    }

    private static Result<CustomerDto> GetCustomer(int id) =>
        Result.Success(new CustomerDto(0.10m));

    private static Result<CartDto> GetCart(int id) =>
        Result.Success(new CartDto(3, 150.00m));

    private record CustomerDto(decimal DiscountRate);
    private record CartDto(int ItemCount, decimal Subtotal);
}
```

### Explanation
C#'s `from x in A from y in B select ...` translates into `.SelectMany()` calls. If any step fails, the failure short-circuits the pipeline and is returned immediately.

### Best Practices
- Use LINQ query syntax when 3 or more operations depend on values computed in earlier steps.
- Use `where` to assert business invariants between steps.

### Common Pitfalls
- Relying on LINQ query syntax for asynchronous operations (`Task<Result<T>>`), where standard `async/await` is cleaner.

---

## Recipe 11: Corrective Fallback with `Recover`

### Problem
An operation fails with a transient error (such as a cache miss or temporary network failure) and the application should attempt a fallback mechanism before propagating a failure.

### Solution
Use `.Recover()` to intercept a failed `Result` and conditionally substitute an alternative successful `Result`.

### Complete Code
```csharp
using System.Threading.Tasks;
using EricksonLopez.Result;

public static class ProductCatalogService
{
    public static async Task<Result<string>> GetProductDataAsync(string sku)
    {
        Result<string> primaryResult = await FetchFromPrimaryApiAsync(sku);

        return primaryResult.Recover(error =>
        {
            if (error.Type == ErrorType.Unavailable || error.Retryability == ErrorRetryability.Transient)
            {
                string cachedData = FetchFromBackupCache(sku);
                return Result.Success(cachedData);
            }

            return primaryResult; // Re-propagate unrecoverable errors
        });
    }

    private static Task<Result<string>> FetchFromPrimaryApiAsync(string sku) =>
        Task.FromResult(Result.Failure<string>(Error.Unavailable("API.Offline", "Remote service is offline.")));

    private static string FetchFromBackupCache(string sku) => "CACHED_PRODUCT_INFO";
}
```

### Explanation
If the input result is successful, `.Recover()` returns it immediately. If it is failed, it invokes the fallback handler. Returning `Result.Success` transitions the pipeline back to the success track.

### Best Practices
- Only recover from specific, expected failure types (e.g., `Unavailable` or `NotFound`).
- Always re-propagate unhandled error types.

### Common Pitfalls
- Blindly returning a success fallback for all errors, which masks serious business rule violations.

---

## Recipe 12: Error Enrichment with `MapError`

### Problem
An infrastructure service returns an error that lacks high-level business context, such as customer ID, order ID, or retry instructions.

### Solution
Use `.MapError()` to transform or enrich an `Error` instance while keeping the overall pipeline in the failure state.

### Complete Code
```csharp
using EricksonLopez.Result;

public static class PaymentService
{
    public static Result<string> Authorize(string transactionId, string customerId)
    {
        return ProcessBankCall(transactionId)
            .MapError(err => err.ToBuilder()
                .WithMetadata("CustomerId", customerId)
                .WithMetadata("TransactionId", transactionId)
                .WithSeverity(ErrorSeverity.Critical)
                .Build());
    }

    private static Result<string> ProcessBankCall(string txId) =>
        Error.Failure("Bank.NetworkError", "Payment network timed out.");
}
```

### Explanation
`MapError` is a no-op on success. On failure, it passes the existing `Error` to the transformation delegate, returning a new `Result<T>` with the enriched error.

### Best Practices
- Use `error.ToBuilder()` to preserve existing error metadata while adding new contextual tags.
- Use `MapError` across architectural boundaries (e.g., Infrastructure → Application).

### Common Pitfalls
- Changing the fundamental `ErrorType` arbitrarily, which breaks upstream HTTP or gRPC status mapping.

---

## Recipe 13: Extracting a Value from Failure with `MapFailure`

### Problem
When an operation fails, the application needs to map the failure to a default fallback value or compute a safe response without throwing exceptions.

### Solution
Use `.MapFailure(error => fallbackValue)` to safely unwrap a `Result<T>` into a raw `T` value regardless of outcome.

### Complete Code
```csharp
using EricksonLopez.Result;

public static class FeatureToggleService
{
    public static bool IsFeatureActive(string featureKey)
    {
        Result<bool> toggleResult = ReadToggleConfig(featureKey);

        return toggleResult.MapFailure(error =>
        {
            // Fallback to false on any configuration failure
            return false;
        });
    }

    private static Result<bool> ReadToggleConfig(string key) =>
        Error.NotFound("Config.NotFound", "Feature flag not configured.");
}
```

### Explanation
If `toggleResult.IsSuccess == true`, `MapFailure` returns `toggleResult.Value`. If `toggleResult.IsFailure == true`, it executes the fallback delegate and returns the computed value.

### Best Practices
- Use `MapFailure` at the presentation or edge layer where a guaranteed value is required.
- Consider `GetValueOrDefault(defaultValue)` for simple static fallbacks.

### Common Pitfalls
- Using `MapFailure` deep in the domain layer, which swallows failures prematurely.

---

## Recipe 14: Suppressing the Return Type with `DiscardValue`

### Problem
An operation returns a `Result<T>`, but the calling method's contract is a non-generic `Result` (e.g., a command that does not return data).

### Solution
Call `.DiscardValue()` to convert `Result<T>` into a non-generic `Result` with 0 heap allocations.

### Complete Code
```csharp
using EricksonLopez.Result;

public static class UserCommandService
{
    public static Result UpdateEmail(int userId, string newEmail)
    {
        Result<UserEntity> updateResult = ExecuteUpdate(userId, newEmail);

        // Convert Result<UserEntity> to non-generic Result
        return updateResult.DiscardValue();
    }

    private static Result<UserEntity> ExecuteUpdate(int id, string email) =>
        Result.Success(new UserEntity(id, email));

    private record UserEntity(int Id, string Email);
}
```

### Explanation
`DiscardValue()` preserves `IsSuccess`, `IsFailure`, and `Error`, but discards the typed `Value` payload without boxing or object instantiation.

### Best Practices
- Use `DiscardValue()` in CQRS command handlers that do not return entity state.

### Common Pitfalls
- Re-wrapping manually with `result.IsSuccess ? Result.Success() : Result.Failure(result.Error)`, which adds boilerplate.

---

## Recipe 15: Safe Value Extraction with `TryGetValue` and `Deconstruct`

### Problem
Developers need to inspect both success value and failure error using idiomatic C# pattern matching or tuple deconstruction without throwing `InvalidOperationException`.

### Solution
Use `.TryGetValue(out T value)` or C# deconstruction `var (isSuccess, value, error) = result`.

### Complete Code
```csharp
using System;
using EricksonLopez.Result;

public static class ResultUnwrapper
{
    public static void DemonstrateUnwrapping(Result<int> result)
    {
        // Method 1: TryGetValue pattern
        if (result.TryGetValue(out int val))
        {
            Console.WriteLine($"Extracted value: {val}");
        }

        // Method 2: C# Tuple Deconstruction
        var (isSuccess, outputValue, error) = result;
        if (!isSuccess)
        {
            Console.WriteLine($"Failed with error: {error.Code}");
        }
    }
}
```

### Explanation
`TryGetValue` returns `true` and assigns the out parameter if `IsSuccess == true`; otherwise it assigns `default` and returns `false`. Deconstruct allows unpacking `(isSuccess, value, error)` cleanly.

### Best Practices
- Prefer `TryGetValue` over manual `if (result.IsSuccess)` checks when reading `.Value`.
- Use deconstruction in switch statements or pattern-matching expressions.

### Common Pitfalls
- Accessing `.Value` when `result.IsFailure == true`, which throws `InvalidOperationException`.

---

## Recipe 16: Exception Safety Boundaries with `Result.Try` and `Result.TryAsync`

### Problem
Third-party libraries (e.g., JSON parsers, cryptographic providers, legacy SDKs) throw exceptions that must be contained at the boundary and converted into `Result<T>`.

### Solution
Wrap third-party calls in `Result.Try` or `Result.TryAsync` with a custom error factory.

### Complete Code
```csharp
using System;
using System.IO;
using EricksonLopez.Result;

public static class FileReaderService
{
    public static Result<string> ReadFileSafely(string path)
    {
        return Result.Try(
            work: () => File.ReadAllText(path),
            errorFactory: ex => ex switch
            {
                FileNotFoundException => Error.NotFound("File.NotFound", $"File '{path}' does not exist."),
                UnauthorizedAccessException => Error.Forbidden("File.Unauthorized", "Permission denied."),
                _ => Error.Unexpected("File.ReadFailed", ex.Message)
            });
    }
}
```

### Explanation
`Result.Try` executes the delegate within an exception handler. If no exception occurs, it returns `Result.Success(value)`. If an exception is caught, it passes the exception to `errorFactory` and returns `Result.Failure(error)`.

### Best Practices
- Always map specific exception types to appropriate `ErrorType` values.
- Use `Result.TryAsync` for asynchronous I/O operations.

### Common Pitfalls
- Wrapping pure domain logic in `Result.Try`; domain code should use explicit `Result` returning methods instead of throwing.

---

## Recipe 17: Aggregating Multi-Operation Outcomes with `Result.Combine`

### Problem
An orchestration workflow invokes multiple independent services (e.g., user service, account service, permissions service) and needs to combine their outputs into a single typed tuple result.

### Solution
Use `Result.Combine(res1, res2, ...)` to evaluate up to 8 heterogeneous results into a strongly-typed tuple.

### Complete Code
```csharp
using System;
using EricksonLopez.Result;

public static class AccountDashboardOrchestrator
{
    public static Result<(UserProfile Profile, AccountBalance Balance)> LoadDashboard(int userId)
    {
        Result<UserProfile> profileResult = GetProfile(userId);
        Result<AccountBalance> balanceResult = GetBalance(userId);

        // Combines both results into Result<(UserProfile, AccountBalance)>
        return Result.Combine(profileResult, balanceResult);
    }

    private static Result<UserProfile> GetProfile(int id) =>
        Result.Success(new UserProfile("Alice"));

    private static Result<AccountBalance> GetBalance(int id) =>
        Result.Success(new AccountBalance(1250.50m));

    public record UserProfile(string Name);
    public record AccountBalance(decimal Balance);
}
```

### Explanation
If all combined results succeed, `Combine` returns a `Result` holding the tuple of unwrapped values. If any fail, it aggregates all failures into an `InnerErrors` compound error.

### Best Practices
- Use heterogeneous `Result.Combine` overloads for up to 8 distinct typed components.
- Use `Result.Combine(params Result<T>[] results)` for homogeneous collections returning `IReadOnlyList<T>`.

### Common Pitfalls
- Combining results that depend sequentially on each other; use `.Bind()` instead for dependent workflows.

---

## Recipe 18: Resilient Data Persistence & Concurrency Handling with Entity Framework Core

### Problem
Database updates with EF Core can throw `DbUpdateConcurrencyException` or deadlock exceptions that crash the request pipeline if unhandled.

### Solution
Use `.SaveChangesAsyncToResult()` and query extensions from `EricksonLopez.Result.EntityFrameworkCore`.

### Complete Code
```csharp
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using EricksonLopez.Result;
using EricksonLopez.Result.EntityFrameworkCore;

public sealed class OrderRepository
{
    private readonly DbContext _context;

    public OrderRepository(DbContext context) => _context = context;

    public async Task<Result<int>> SaveOrderAsync()
    {
        Result<int> saveResult = await _context.SaveChangesAsyncToResult();

        if (saveResult.IsFailure && saveResult.Error.Code == EntityFrameworkErrorCodes.ConcurrencyConflict)
        {
            // Handle optimistic concurrency conflict
        }

        return saveResult;
    }
}
```

### Explanation
`SaveChangesAsyncToResult` intercepts database exceptions: concurrency exceptions are mapped to `ErrorType.Conflict` with code `Database.ConcurrencyConflict`, timeout exceptions to `ErrorType.Unavailable`, and generic update errors to `Database.UpdateFailed`.

### Best Practices
- Use `FirstOrDefaultToResultAsync(predicate, notFoundError)` to query single records safely.
- Check `saveResult.Error.Code == EntityFrameworkErrorCodes.ConcurrencyConflict` for optimistic concurrency handling.

### Common Pitfalls
- Calling standard `_context.SaveChangesAsync()` without a try/catch, bypassing functional error flow.

---

## Recipe 19: Asynchronous Messaging & Dead-Letter Routing with MassTransit and `ResultFault`

### Problem
Message consumers in event-driven architectures encounter business failures that should be published back to the message broker or sent to dead-letter queues without crashing the transport channel.

### Solution
Use `ResultFault` and `ResultConsumeFilter<TMessage>` from `EricksonLopez.Result.MassTransit`.

### Complete Code
```csharp
using System;
using System.Threading.Tasks;
using MassTransit;
using EricksonLopez.Result;
using EricksonLopez.Result.MassTransit;

public sealed record ProcessPaymentMessage(string OrderId, decimal Amount);

public sealed class PaymentConsumer : IConsumer<ProcessPaymentMessage>
{
    public async Task Consume(ConsumeContext<ProcessPaymentMessage> context)
    {
        Result paymentResult = await AuthorizeAsync(context.Message);

        if (paymentResult.IsFailure)
        {
            // Publish standard ResultFault message contract
            ResultFault fault = ResultFault.FromError(
                paymentResult.Error,
                correlationId: context.CorrelationId?.ToString());

            await context.Publish(fault);
        }
    }

    private static Task<Result> AuthorizeAsync(ProcessPaymentMessage msg) =>
        Task.FromResult(Result.Failure(Error.Validation("Payment.CardDeclined", "Card was declined.")));
}
```

### Explanation
`ResultFault` is a serializable DTO capturing `ErrorCode`, `Description`, `ErrorType`, `Severity`, `Retryability`, and tracing IDs. The `ResultConsumeFilter<TMessage>` intercepts consumer failures automatically.

### Best Practices
- Configure `cfg.UseResultFilter<ProcessPaymentMessage>()` in your MassTransit bus configuration.
- Inspect `fault.Retryability` on downstream saga coordinators to decide between re-queueing and compensating transactions.

### Common Pitfalls
- Throwing exceptions from consumers for known domain errors, which triggers broker redelivery unnecessarily.

---

## Recipe 20: Resilient Execution & Transient Retry with Polly v8 Resilience Pipelines

### Problem
External API calls fail intermittently due to network glitches, and retry policies should only retry transient errors, never permanent ones like validation or unauthorized access.

### Solution
Use `PollyResultExtensions` and `ResultResilienceStrategyBuilderExtensions` from `EricksonLopez.Result.Polly`.

### Complete Code
```csharp
using System;
using System.Threading.Tasks;
using Polly;
using EricksonLopez.Result;
using EricksonLopez.Result.Polly;

public static class ResilientHttpCaller
{
    private static readonly ResiliencePipeline<Result<string>> Pipeline =
        new ResiliencePipelineBuilder<Result<string>>()
            .AddResultRetry(maxRetryAttempts: 3, delay: TimeSpan.FromMilliseconds(200))
            .Build();

    public static async Task<Result<string>> CallExternalApiAsync(string endpoint)
    {
        return await Pipeline.ExecuteResultAsync(async ct =>
        {
            return await ExecuteHttpCallAsync(endpoint);
        });
    }

    private static Task<Result<string>> ExecuteHttpCallAsync(string url) =>
        Task.FromResult(Result.Success("PAYLOAD_OK"));
}
```

### Explanation
`.AddResultRetry()` inspects the returned `Result<T>`. It triggers a retry only if `result.IsFailure == true` AND `result.Error.Retryability == ErrorRetryability.Transient`. Permanent failures return immediately without wasting retry attempts.

### Best Practices
- Mark network timeouts and 503 responses with `ErrorRetryability.Transient`.
- Use the allocation-free `ExecuteResultAsync(state, ...)` overload in high-throughput services.

### Common Pitfalls
- Retrying permanent errors (such as 400 Bad Request or 404 Not Found), which increases latency without fixing the root cause.

---

## Recipe 21: High-Performance JSON Serialization & Native AOT Contexts

### Problem
Applications compiled with Native AOT need to serialize and deserialize `Result` and `Result<T>` without reflection or code generation warnings.

### Solution
Use `ResultJsonSerializerContext` and custom converters from `EricksonLopez.Result.Serialization`.

### Complete Code
```csharp
using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using EricksonLopez.Result;
using EricksonLopez.Result.Serialization;

public sealed record ProductDto(int Id, string Sku);

[JsonSerializable(typeof(Result))]
[JsonSerializable(typeof(Result<ProductDto>))]
[JsonSerializable(typeof(Error))]
public partial class AppJsonContext : JsonSerializerContext { }

public static class SerializationWorkflow
{
    public static void Roundtrip()
    {
        var options = new JsonSerializerOptions
        {
            TypeInfoResolver = AppJsonContext.Default
        };
        options.Converters.Add(new ResultJsonConverter());
        options.Converters.Add(new ResultOfTJsonConverter<ProductDto>());
        options.Converters.Add(new ErrorJsonConverter());

        Result<ProductDto> result = Result.Success(new ProductDto(1, "SKU-99"));
        string json = JsonSerializer.Serialize(result, options);

        Result<ProductDto> deserialized = JsonSerializer.Deserialize<Result<ProductDto>>(json, options);
    }
}
```

### Explanation
The custom converters serialize `Result<T>` into a compact JSON schema (`{"isSuccess": true, "isFailure": false, "value": {...}}`). On failure, the `error` object contains full diagnostics. The source generator context enables zero-reflection Native AOT compilation.

### Best Practices
- Register the custom converters in ASP.NET Core `ConfigureHttpJsonOptions`.
- Use source generator contexts for all microservices targeting Native AOT.

### Common Pitfalls
- Relying on default reflection-based serialization in trimmed Native AOT applications.

---

## Recipe 22: Advanced W3C Trace Enrichment & Injected Runtime Metrics

### Problem
Enterprise applications running under dependency injection need injectable metrics objects for testability and automated activity enrichment across asynchronous pipelines.

### Solution
Register `AddResultInstrumentation()` in `IServiceCollection` and inject `ResultMetrics`.

### Complete Code
```csharp
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using EricksonLopez.Result;
using EricksonLopez.Result.OpenTelemetry;

public sealed class BillingService
{
    private readonly ResultMetrics _metrics;

    public BillingService(ResultMetrics metrics) => _metrics = metrics;

    public async Task<Result<string>> BillCustomerAsync(int customerId, decimal amount)
    {
        Result<string> outcome = await ChargeCardAsync(customerId, amount)
            .TraceOutcomeAsync("BillCustomer", Activity.Current);

        if (outcome.IsSuccess)
            _metrics.TrackSuccess("BillCustomer");
        else
            _metrics.TrackFailure("BillCustomer", outcome.Error);

        return outcome;
    }

    private static Task<Result<string>> ChargeCardAsync(int id, decimal amt) =>
        Task.FromResult(Result.Success("TX-88339"));
}
```

### Explanation
`ResultMetrics` provides non-static, injectable meters for unit test mocking. `.TraceOutcomeAsync()` is an extension method directly available on `Task<Result<T>>` to enrich spans fluently.

### Best Practices
- Inject `ResultMetrics` into service classes instead of calling static methods when test isolation is needed.
- Use `.TraceOutcomeAsync()` directly at the end of asynchronous task chains.

### Common Pitfalls
- Forgetting to pass the operation name, leading to missing meter dimensions.

---

## Recipe 23: Test Runner Adapters for NUnit and xUnit

### Problem
Fluent assertions in test suites should integrate directly with runner-native exception types so that failures appear in test explorers as test assertion failures rather than unhandled exceptions.

### Solution
Install `EricksonLopez.Result.Testing.NUnit` or `EricksonLopez.Result.Testing.XUnit` and call `.UseNUnitExceptions()` or `.UseXUnitExceptions()`.

### Complete Code
```csharp
using System.Runtime.CompilerServices;
using EricksonLopez.Result;
using EricksonLopez.Result.Testing;
using EricksonLopez.Result.Testing.XUnit;
using Xunit;

internal static class TestAssemblyInitializer
{
    [ModuleInitializer]
    internal static void Initialize()
    {
        ResultXUnitAssertionConfig.UseXUnitExceptions();
    }
}

public class CustomerTests
{
    [Fact]
    public void Customer_ShouldBeValid()
    {
        Result<string> result = Result.Success("VALID_CUSTOMER");
        string customer = result.ShouldBeSuccess();
        Assert.Equal("VALID_CUSTOMER", customer);
    }
}
```

### Explanation
`UseXUnitExceptions()` configures `ResultAssertionException.ExceptionFactory` to produce `ResultAssertionXUnitException` (derived from `XunitException`), ensuring clean failure reports in Visual Studio, Rider, and `dotnet test`.

### Best Practices
- Put the configuration in a `[ModuleInitializer]` method in your test project so it applies globally.

### Common Pitfalls
- Setting the exception factory inside individual test methods repeatedly.

---

## Recipe 24: Distributed State Management & Pub/Sub Topic Ingestion with Dapr

### Problem
Microservices built on Dapr need to interact with distributed state stores (handling ETag concurrency conflicts safely) and handle Pub/Sub topic subscription messages with automatic ACK, RETRY, or DROP status codes.

### Solution
Use `DaprResultStateExtensions` and `DaprResultPubSubExtensions` from `EricksonLopez.Result.Dapr`.

### Complete Code
```csharp
using System.Threading.Tasks;
using Dapr.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using EricksonLopez.Result;
using EricksonLopez.Result.Dapr;

public sealed record CustomerProfile(string Name, string Email, string ETag);

public static class DaprMicroservice
{
    public static async Task<Result> UpdateProfileAsync(
        DaprClient client,
        string storeName,
        string key,
        CustomerProfile profile)
    {
        // 1. Save state with ETag verification; concurrency conflicts return ErrorType.Conflict
        return await client.SaveStateWithResultAsync(
            storeName,
            key,
            profile,
            etag: profile.ETag);
    }

    public static void MapEndpoints(WebApplication app)
    {
        // 2. Pub/Sub endpoint with automatic disposition mapping
        app.MapPost("/orders/subscribe", async (CustomerProfile profile, DaprClient client) =>
        {
            Result result = await UpdateProfileAsync(client, "statestore", "cust-1", profile);

            // Returns 200 OK (ACK), 503 (RETRY for transient), or 422 (DROP for permanent)
            return result.ToDaprTopicResult();
        });
    }
}
```

### Explanation
`SaveStateWithResultAsync` catches Dapr exceptions. Concurrency mismatches are converted to `ErrorType.Conflict` with code `DaprErrorCodes.EtagMismatch`. `.ToDaprTopicResult()` inspects `Error.Retryability`: `Transient` yields HTTP 503 (Dapr re-attempts delivery with backoff), while `Permanent` yields HTTP 422 (Dapr moves message to Dead Letter Queue).

### Best Practices
- Always specify `etag` when saving state in concurrent microservice environments.
- Use `.ToDaprTopicResult()` on all Dapr subscription endpoints to control message disposition declaratively.

### Common Pitfalls
- Returning HTTP 500 on validation errors, which causes Dapr to retry invalid messages indefinitely.

---

## Recipe 25: Canonical gRPC Error Interception & Status Code Mapping

### Problem
gRPC services returning domain `Result<T>` must translate business errors into standard gRPC `StatusCode` values and trailers without leaking unhandled exceptions or writing repetitive try/catch blocks in every RPC handler.

### Solution
Register `ResultServerInterceptor` on the gRPC server and use client extensions from `EricksonLopez.Result.Grpc`.

### Complete Code
```csharp
using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using EricksonLopez.Result;
using EricksonLopez.Result.Grpc;

public static class GrpcServiceConfiguration
{
    public static void ConfigureServer(WebApplicationBuilder builder)
    {
        builder.Services.AddGrpc(options =>
        {
            // Interceptor translates Result failures and exceptions to RpcException with trailers
            options.Interceptors.Add<ResultServerInterceptor>();
        });
    }

    public static async Task ConsumeGrpcClientAsync(AsyncUnaryCall<string> clientCall)
    {
        // Client extension catches RpcException and reconstitutes original Error
        Result<string> clientResult = await clientCall.ToResultAsync();

        if (clientResult.IsFailure)
        {
            // Access original Code, Description, Type, and metadata trailers
            string code = clientResult.Error.Code;
            ErrorType type = clientResult.Error.Type;
        }
    }
}
```

### Explanation
`ResultServerInterceptor` catches any handler returning an `IResultOutcome` where `IsFailure == true`, invoking `GrpcErrorMapper.ToRpcException()` to set standard gRPC status codes (e.g., `Validation` → `InvalidArgument`, `NotFound` → `NotFound`, `Conflict` → `AlreadyExists`) and serializing error metadata into trailers. `ToResultAsync()` on the client side reconstructs the exact domain `Error`.

### Best Practices
- Register `ResultServerInterceptor` in `AddGrpc()` options.
- Use `call.ToResultAsync()` on client applications for transparent error reconstitution.

### Common Pitfalls
- Manually constructing `RpcException` with hardcoded string messages instead of using `error.ToRpcException()`.

---

## Recipe 26: Compile-Time Domain Error Catalog Generation from `*.errors.json`

### Problem
Hardcoded error strings scattered across codebases lead to duplication, typos, inconsistent codes, and missing localization keys.

### Solution
Use the Roslyn incremental source generator from `EricksonLopez.Result.DomainErrors.Generators` with an `*.errors.json` additional file.

### Complete Code
```json
/* File: order.errors.json (Configured as AdditionalFiles in csproj) */
{
  "Namespace": "ECommerce.Domain.Errors",
  "ClassName": "OrderErrors",
  "Errors": [
    {
      "Name": "NotFound",
      "Code": "Order.NotFound",
      "Description": "Order '{0}' was not found.",
      "Type": "NotFound",
      "Severity": "Warning",
      "Retryability": "Permanent"
    },
    {
      "Name": "PaymentFailed",
      "Code": "Order.PaymentFailed",
      "Description": "Payment failed for order '{0}'.",
      "Type": "Failure",
      "Severity": "Error",
      "Retryability": "Transient"
    }
  ]
}
```

```csharp
using EricksonLopez.Result;
using ECommerce.Domain.Errors;

public static class OrderDomainService
{
    public static Result<string> CancelOrder(string orderId)
    {
        bool found = false;
        if (!found)
        {
            // Strongly-typed static method generated at compile time with zero runtime overhead
            Error error = OrderErrors.NotFound(orderId);
            return Result.Failure<string>(error);
        }

        return Result.Success("CANCELLED");
    }
}
```

### Explanation
The generator detects files matching `*.errors.json` marked as `<AdditionalFiles>`. It emits strongly-typed C# classes with static factory methods. Formatted placeholders like `{0}` generate typed parameters (`string param0`), ensuring compile-time safety.

### Best Practices
- Mark the json file in `.csproj`: `<AdditionalFiles Include="order.errors.json" />`.
- Reference the analyzer with `OutputItemType="Analyzer" ReferenceOutputAssembly="false"`.

### Common Pitfalls
- Forgetting to include the file as `<AdditionalFiles>`, which prevents the source generator from executing.
