# API Reference (Microsoft Learn Style)

> **Namespace:** `EricksonLopez.Result` | **Assemblies:** `EricksonLopez.Result.dll`, `EricksonLopez.Result.*.dll` | **Applicability:** .NET 8.0, 9.0, 10.0

Comprehensive technical reference documentation for public methods, factory functions, combinators, and official extension methods of the `EricksonLopez.Result` ecosystem.

---

## Index of Documented Public APIs

- [Core Primitives: `Result.Success()`](#resultsuccess)
- [Core Primitives: `Result.Failure(Error)`](#resultfailureerror)
- [Core Primitives: `Result<TValue>.Success(TValue)`](#resulttvaluesuccesstvalue)
- [Core Primitives: `Result<TValue>.Failure(Error)`](#resulttvaluefailureerror)
- [Combinators: `Result.Combine(...)`](#resultcombine)
- [Validation: `Result.ValidateAll(...)`](#resultvalidateall)
- [Exception Boundaries: `Result.Try / TryAsync(...)`](#resulttry--tryasync)
- [Monadic Chaining: `Result<TValue>.Bind(...)`](#resulttvaluebind)
- [Transformations: `Result<TValue>.Map(...)`](#resulttvaluemap)
- [Invariant Checking: `Result<TValue>.Ensure(...)`](#resulttvalueensure)
- [Corrective Fallback: `Result<TValue>.Recover(...)`](#resulttvaluerecover)
- [Branching: `Result<TValue>.Match(...)` and `Execute(...)`](#resulttvaluematch-and-execute)
- [Diagnostics: `Error.Create(...)` & Diagnostic Factories](#errorcreate--diagnostic-factories)
- [Typed Error Monad: `Result<TValue, TError>`](#resulttvalue-terror-generic)
- [Option Monad: `Maybe<T>`](#maybet-option-monad)
- [ASP.NET Core: `ResultHttpExtensions.ToHttpResult`](#resulthttpextensionstohttpresult-aspnetcore)
- [Entity Framework Core: `EntityFrameworkResultExtensions`](#entityframeworkresultextensions-ef-core)
- [FluentValidation: `FluentValidationResultExtensions`](#fluentvalidationresultextensions-fluentvalidation)
- [Polly Resilience: `PollyResultExtensions` & `AddResultRetry`](#pollyresultextensions--addresultretry-polly)
- [MassTransit: `ResultFault` & `ResultConsumeFilter`](#resultfault--resultconsumefilter-masstransit)
- [OpenTelemetry: `ResultActivityExtensions` & `ResultMetrics`](#resultactivityextensions--resultmetrics-opentelemetry)
- [Testing: `ResultAssertions` & Test Runner Adapters](#resultassertions--test-runner-adapters-testing)
- [Dapr: `DaprResultStateExtensions` & `DaprResultPubSubExtensions`](#daprresultstateextensions--daprresultpubsubextensions-dapr)
- [gRPC: `GrpcErrorMapper` & `ResultServerInterceptor`](#grpcerrormapper--resultserverinterceptor-grpc)
- [Source Generators: `DomainErrorsGenerator`](#domainerrorsgenerator-source-generators)

---

## `Result.Success()`

Creates a non-generic successful `Result` instance.

### Syntax
```csharp
[Pure]
[MethodImpl(MethodImplOptions.AggressiveInlining)]
public static Result Success();
```

### Parameters
None.

### Return Value
`Result`: Readonly struct success instance where `IsSuccess == true` and `IsFailure == false`.

### Exceptions
None.

### Remarks
This method returns a value-type struct held directly in CPU registers or on the caller's stack frame. It guarantees **0 bytes** of GC heap allocation.

### Basic Example
```csharp
Result result = Result.Success();
Console.WriteLine(result.IsSuccess); // True
```

### Advanced Example
```csharp
public Result MarkOrderAsShipped(Guid orderId)
{
    var order = _orderRepository.Find(orderId);
    if (order is null)
        return Error.NotFound("Order.NotFound", $"Order '{orderId}' does not exist.");

    order.Ship();
    _orderRepository.Update(order);
    return Result.Success();
}
```

### Best Practices
- Use for commands, state mutations, or precondition checks where no payload is returned.

### Performance
- **Execution Time:** ~0.31 ns
- **GC Allocation:** 0 B (Zero-allocation guarantee)

### Common Pitfalls
- Attempting to access `.Value` on a non-generic `Result` instance; only `Result<T>` exposes `.Value`.

### When to Use
- When successfully concluding an operation without returning data.

### When NOT to Use
- When the operation produces an entity or DTO; use `Result.Success<TValue>(value)` or return the value directly via implicit conversion.

---

## `Result.Failure(Error)`

Creates a non-generic failed `Result` containing domain error diagnostics.

### Syntax
```csharp
[Pure]
[MethodImpl(MethodImplOptions.AggressiveInlining)]
public static Result Failure(Error error);
```

### Parameters
- `error` (`Error`): Non-null instance encapsulating the structured error diagnostics.

### Return Value
`Result`: Readonly struct instance where `IsFailure == true` and `IsSuccess == false`.

### Exceptions
- `ArgumentNullException`: Thrown if `error` is `null`.

### Remarks
Guarantees immutability and complete thread-safety. The encapsulated error contains code, description, semantic type, severity, and metadata.

### Basic Example
```csharp
Result failure = Result.Failure(Error.Unauthorized("Auth.Expired", "Token has expired."));
Console.WriteLine(failure.Error.Code); // Auth.Expired
```

### Advanced Example
```csharp
public Result ValidateUserAge(int age)
{
    if (age < 18)
    {
        return Result.Failure(Error.Validation("User.Underage", "User must be at least 18.")
            .ToBuilder()
            .WithMetadata("ProvidedAge", age)
            .WithSeverity(ErrorSeverity.Warning)
            .Build());
    }

    return Result.Success();
}
```

### Best Practices
- Use semantic static factories (`Error.Validation`, `Error.NotFound`, `Error.Conflict`, etc.) instead of instantiating empty or ad-hoc strings.

### Performance
- **Execution Time:** ~0.35 ns
- **GC Allocation:** 0 B for the `Result` struct envelope.

### Common Pitfalls
- Passing `null` as the error, which violates the library's safety invariants.

### When to Use
- To signal a controlled business or infrastructure failure in operations without a return value.

### When NOT to Use
- For unrecoverable CLR exceptions (`OutOfMemoryException`, `StackOverflowException`); allow these to fail fast.

---

## `Result<TValue>.Success(TValue)`

Creates a generic `Result<TValue>` encapsulating a successful outcome and payload.

### Syntax
```csharp
[Pure]
[MethodImpl(MethodImplOptions.AggressiveInlining)]
public static Result<TValue> Success(TValue value);
```

### Parameters
- `value` (`TValue`): Payload produced by the operation.

### Return Value
`Result<TValue>`: Success struct with accessible `.Value`.

### Exceptions
- `ArgumentNullException`: Thrown if `value` is null and `TValue` is a non-nullable value or reference type.

### Remarks
Supports implicit conversion operators: `TValue` converts directly to `Result<TValue>`.

### Basic Example
```csharp
Result<int> result = Result.Success(42);
int val = result.Value; // 42
```

### Advanced Example
```csharp
public Result<CustomerDto> GetCustomer(int id)
{
    Customer? customer = _db.FindCustomer(id);
    if (customer is null)
        return Error.NotFound("Customer.NotFound", $"Customer {id} not found.");

    // Implicit cast from CustomerDto to Result<CustomerDto>
    return new CustomerDto(customer.Id, customer.Name);
}
```

### Best Practices
- Leverage implicit conversions in method returns for cleaner, more concise code.

### Performance
- **GC Allocation:** 0 B (readonly struct in CPU registers or on the stack).

### Common Pitfalls
- Passing `null` for a reference type without marking the generic parameter as `TValue?`.

### When to Use
- Whenever an operation produces a successful resulting value.

### When NOT to Use
- In purely side-effect-based flows that do not produce data.

---

## `Result<TValue>.Failure(Error)`

Creates a generic `Result<TValue>` holding an error diagnostic.

### Syntax
```csharp
[Pure]
[MethodImpl(MethodImplOptions.AggressiveInlining)]
public static Result<TValue> Failure(Error error);
```

### Parameters
- `error` (`Error`): Structured error diagnostic.

### Return Value
`Result<TValue>`: Failure struct.

### Exceptions
- `ArgumentNullException`: If `error` is null.

### Remarks
Supports direct implicit conversion from `Error` to `Result<TValue>`.

### Basic Example
```csharp
Result<string> failure = Result.Failure<string>(Error.NotFound("Item.Missing", "Item not found."));
```

### Advanced Example
```csharp
public Result<OrderReceipt> Checkout(Cart cart)
{
    if (cart.IsEmpty)
        return Error.Validation("Cart.Empty", "Cannot checkout an empty cart."); // Implicit cast

    return new OrderReceipt(Guid.NewGuid(), cart.Total);
}
```

### Best Practices
- Return the `Error` instance directly leveraging implicit conversion.

### Performance
- 0 B allocated for the wrapper struct envelope.

### Common Pitfalls
- Reading `.Value` from a failed instance without verifying `.IsSuccess`, which throws `InvalidOperationException`.

### When to Use
- To return failures from methods with a generic `Result<T>` return type.

### When NOT to Use
- For successful operations.

---

## `Result.Combine(...)`

Aggregates multiple results into a single composite outcome, combining up to 8 heterogeneous results into strongly-typed tuples.

### Syntax
```csharp
public static Result Combine(params Result[] results);
public static Result Combine(ReadOnlySpan<Result> results);
public static Result<IReadOnlyList<T>> Combine<T>(params Result<T>[] results);
public static Result<(T1, T2)> Combine<T1, T2>(in Result<T1> r1, in Result<T2> r2);
public static Result<(T1, T2, T3)> Combine<T1, T2, T3>(in Result<T1> r1, in Result<T2> r2, in Result<T3> r3);
public static Result<(T1, T2, T3, T4)> Combine<T1, T2, T3, T4>(in Result<T1> r1, in Result<T2> r2, in Result<T3> r3, in Result<T4> r4);
public static Result<(T1, T2, T3, T4, T5)> Combine<T1, T2, T3, T4, T5>(in Result<T1> r1, in Result<T2> r2, in Result<T3> r3, in Result<T4> r4, in Result<T5> r5);
```

### Parameters
- Heterogeneous overloads: 2 to 5 `Result<T>` instances passed by `in` reference.
- Homogeneous overloads: Collections or spans of `Result` or `Result<T>`.

### Return Value
- Success: Tuple containing the unwrapped values or a read-only list.
- Failure: `Result.Failure` with accumulated errors in `InnerErrors`.

### Exceptions
None.

### Remarks
If multiple results fail, all errors are aggregated under a composite error with code `WellKnownErrors.CombinedFailuresCode`.

### Basic Example
```csharp
Result<int> r1 = Result.Success(10);
Result<string> r2 = Result.Success("Active");

Result<(int, string)> combined = Result.Combine(r1, r2);
var (num, status) = combined.Value;
```

### Advanced Example
```csharp
public async Task<Result<CheckoutContext>> PrepareCheckoutAsync(int userId, int cartId)
{
    var userTask = _userService.GetUserAsync(userId);
    var cartTask = _cartService.GetCartAsync(cartId);

    await Task.WhenAll(userTask, cartTask);

    return Result.Combine(userTask.Result, cartTask.Result)
                 .Map(tuple => new CheckoutContext(tuple.Item1, tuple.Item2));
}
```

### Best Practices
- Prefer heterogeneous tuple overloads (`in Result<T1>, in Result<T2>`) over lists to eliminate heap array allocations.

### Performance
- Heterogeneous overloads (2 to 5): **0 bytes GC allocation** on the success path.

### Common Pitfalls
- Using `Combine` when the second calculation depends on the result of the first; use `.Bind()` instead.

### When to Use
- When executing independent, concurrent operations whose results must be consumed together.

### When NOT to Use
- When sequential data dependency exists between calls.

---

## `Result.ValidateAll(...)`

Accumulates validation rules over an instance without early short-circuiting, utilizing pooled buffers.

### Syntax
```csharp
public static Result ValidateAll(params ReadOnlySpan<Func<Result>> validators);
public static Result<T> ValidateAll<T>(T instance, params ReadOnlySpan<Func<T, Result>> validators);
```

### Parameters
- `instance`: Object under validation.
- `validators`: Set of validation functions.

### Return Value
`Result<T>` with the original instance if valid, or a composite error if one or more rules fail.

### Exceptions
None.

### Remarks
Leases internal buffers from `ArrayPool<Error>.Shared` to guarantee zero temporary collection allocations on the heap.

### Basic Example
```csharp
Result<Customer> result = Result.ValidateAll(customer,
    c => !string.IsNullOrEmpty(c.Name) ? Result.Success() : Error.Validation("Name.Required", "Name required."),
    c => c.Age >= 18 ? Result.Success() : Error.Validation("Age.Underage", "Must be 18+.")
);
```

### Best Practices
- Keep validation functions pure and free of I/O side effects.

### Performance
- 0 bytes GC allocation when all rules pass.

### Common Pitfalls
- Throwing exceptions inside validation delegates instead of returning `Result.Failure`.

### When to Use
- In domain model or incoming command validations.

### When NOT to Use
- In sequential flows where a second rule requires that the first rule did not fail (use `.Ensure()` or sequential `if` checks).

---

## `Result.Try / TryAsync(...)`

Executes non-functional code within a safe exception boundary, translating exceptions into structured errors.

### Syntax
```csharp
public static Result Try(Action action, Func<Exception, Error> errorHandler);
public static Result<T> Try<T>(Func<T> func, Func<Exception, Error> errorHandler);
public static Result<T> Try<TState, T>(TState state, Func<TState, T> func, Func<TState, Exception, Error> errorHandler);
public static Task<Result<T>> TryAsync<T>(Func<CancellationToken, Task<T>> func, Func<Exception, Error> errorHandler, CancellationToken ct = default);
```

### Parameters
- `func`: Delegate that may throw exceptions.
- `errorHandler`: Factory mapping the caught exception to an `Error` instance.

### Return Value
`Result<T>` containing the returned value or translated error.

### Exceptions
Does not intercept fatal CLR exceptions (`OutOfMemoryException`, `StackOverflowException`).

### Basic Example
```csharp
Result<string> content = Result.Try(
    () => File.ReadAllText("data.json"),
    ex => Error.Infrastructure("File.ReadFailed", ex.Message));
```

### Best Practices
- Map specific exceptions to corresponding semantic error types (`NotFound`, `Unavailable`, `Forbidden`).

### Performance
- Use `TState` overloads on hot paths to avoid closure captures.

### Common Pitfalls
- Catching internal control flow exceptions that should be modeled idiomatically with `Result`.

### When to Use
- In infrastructure adapters or third-party exception-driven libraries.

### When NOT to Use
- In domain core logic.

---

## `Result<TValue>.Bind(...)`

Chains an operation that itself returns another `Result<TNext>` (monadic bind).

### Syntax
```csharp
public Result<TNext> Bind<TNext>(Func<TValue, Result<TNext>> bind);
public Result<TNext> Bind<TState, TNext>(TState state, Func<TState, TValue, Result<TNext>> bind);
```

### Parameters
- `bind`: Function receiving current value and returning a new `Result<TNext>`.
- `state`: Optional contextual state for closure-free execution.

### Return Value
The new `Result<TNext>`, or the original error if the current instance is already a failure.

### Remarks
Automatically short-circuits on the first encountered failure.

### Basic Example
```csharp
Result<Order> order = FindOrder(orderId)
    .Bind(order => ValidateInventory(order))
    .Bind(order => ChargeCustomer(order));
```

### Best Practices
- Use `Bind` to connect sequential dependent steps in a Railway-Oriented Programming (ROP) pipeline.

### Performance
- 0 bytes additional overhead with `TState` overloads.

### Common Pitfalls
- Using `.Map` instead of `.Bind` when the invoked function returns `Result<T>`, producing an undesired nested `Result<Result<T>>`.

### When to Use
- To chain operations that may fail.

### When NOT to Use
- For pure transformations that always succeed; use `.Map()` in that case.

---

## `Result<TValue>.Map(...)`

Transforms the encapsulated value of a successful result, propagating errors unchanged.

### Syntax
```csharp
public Result<TNew> Map<TNew>(Func<TValue, TNew> map);
public Result<TNew> Map<TState, TNew>(TState state, Func<TState, TValue, TNew> map);
```

### Parameters
- `map`: Pure mapping function.

### Return Value
`Result<TNew>` with the mapped value or the pre-existing error.

### Basic Example
```csharp
Result<UserDto> dtoResult = GetUser(id).Map(user => new UserDto(user.Id, user.Name));
```

### Best Practices
- Keep the mapping function pure and free of exceptions.

### Performance
- Zero allocations when using `TState` and static delegates.

### Common Pitfalls
- Mapping values to null without declaring `TNew?`.

### When to Use
- For projections and data transformations that cannot fail.

### When NOT to Use
- When the transformation can fail and needs to return an `Error`.

---

## `Result<TValue>.Ensure(...)`

Validates that a predicate holds true on the success value; otherwise converts to failure.

### Syntax
```csharp
public Result<TValue> Ensure(Func<TValue, bool> predicate, Error error);
public Result<TValue> Ensure(Func<TValue, bool> predicate, Func<Error> errorFactory);
public Result<TValue> Ensure<TState>(TState state, Func<TState, TValue, bool> predicate, Error error);
```

### Parameters
- `predicate`: Condition that must hold true.
- `error` / `errorFactory`: Error to apply if the condition is false.

### Return Value
The original instance if valid, or a new failure with the specified error.

### Basic Example
```csharp
Result<int> positive = Result.Success(5)
    .Ensure(x => x > 0, Error.Validation("Number.NotPositive", "Value must be positive."));
```

### Best Practices
- Use overloads with `Func<Error>` if error construction incurs serialization or formatting overhead.

### Performance
- 0 B on the success path.

### Common Pitfalls
- Inverting predicate logic; the predicate must return `true` for a valid state.

### When to Use
- To verify invariants and preconditions on already computed values.

### When NOT to Use
- For complex multi-branch control flow logic.

---

## `Result<TValue>.Recover(...)`

Provides a corrective fallback result if and only if the current result is a failure.

### Syntax
```csharp
public Result<TValue> Recover(Func<Error, Result<TValue>> recovery);
public Result<TValue> Recover<TState>(TState state, Func<TState, Error, Result<TValue>> recovery);
```

### Parameters
- `recovery`: Delegate inspecting the error and producing an alternative result.

### Return Value
The original result if successful, or the result produced by the recovery delegate.

### Basic Example
```csharp
Result<string> content = ReadCache(key)
    .Recover(err => FetchFromDatabase(key));
```

### Best Practices
- Re-propagate unrecoverable errors by returning the original failure.

### Performance
- Zero overhead on the success path (immediate return without evaluating delegates).

### Common Pitfalls
- Recovering from user input validation errors, which can hide bugs in clients.

### When to Use
- For contingencies on transient network faults, cache misses, or permitted defaults.

### When NOT to Use
- To mask fatal logic errors or permanent data inconsistency.

---

## `Result<TValue>.Match(...)` and `Execute(...)`

Exhaustively branches based on the success or failure outcome.

### Syntax
```csharp
public TResult Match<TResult>(Func<TValue, TResult> onSuccess, Func<Error, TResult> onFailure);
public TResult Match<TState, TResult>(TState state, Func<TState, TValue, TResult> onSuccess, Func<TState, Error, TResult> onFailure);
public void Execute(Action<TValue> onSuccess, Action<Error> onFailure);
```

### Parameters
- `onSuccess`: Branch to execute on success.
- `onFailure`: Branch to execute on failure.

### Return Value
`TResult` in `Match`, `void` in `Execute`.

### Basic Example
```csharp
string status = result.Match(
    onSuccess: val => $"Processed: {val}",
    onFailure: err => $"Error [{err.Code}]: {err.Description}"
);
```

### Best Practices
- Use `Match` in controllers and application boundaries to transform results into final responses.

### Performance
- Aggressive inlining under JIT compilation.

### Common Pitfalls
- Using `Execute` to trigger side effects mid-pipeline; use `.Tap()` instead.

### When to Use
- At application boundaries to complete the `Result` lifecycle.

### When NOT to Use
- Inside domain layers where you intend to continue chaining operations.

---

## `Error.Create(...)` & Diagnostic Factories

Constructs immutable, structured diagnostic errors.

### Syntax
```csharp
public static Error Create(string code, string description);
public static Error Validation(string code, string description);
public static Error NotFound(string code, string description);
public static Error Conflict(string code, string description);
public static Error Unauthorized(string code, string description);
public static Error Forbidden(string code, string description);
public static Error Unavailable(string code, string description);
public static Error Unexpected(string code, string description);
```

### Parameters
- `code`: Human-readable, unique error identifier (e.g. `"User.NotFound"`).
- `description`: Detailed message explaining the failure reason.

### Return Value
Immutable instance of `Error`.

### Basic Example
```csharp
Error error = Error.NotFound("Product.NotFound", "The product was not found.");
```

### Best Practices
- Follow hierarchical naming conventions for codes: `"Entity.Reason"`.

### Performance
- Immutable; suitable for caching in static fields for high-frequency errors.

### Common Pitfalls
- Using `Error.Unexpected` for normal user input validation errors.

### When to Use
- To construct domain and infrastructure error models.

### When NOT to Use
- As a replacement for informational log messages that do not represent failures.

---

## `Result<TValue, TError>` (Generic)

Strictly-typed monad parameterized on both success value and compile-time error type.

### Syntax
```csharp
public readonly struct Result<TValue, TError> : IEquatable<Result<TValue, TError>>
    where TError : notnull
```

### Key Members
- `Success(TValue value)`
- `Failure(TError error)`
- `Match<TResult>(Func<TValue, TResult>, Func<TError, TResult>)`
- `Bind<TNext>(Func<TValue, Result<TNext, TError>>)`

### Basic Example
```csharp
public enum AuthError { ExpiredToken, InvalidSignature }

Result<Claims, AuthError> authResult = ValidateToken(jwt);
```

### Best Practices
- Use in bounded context cores where errors are finite and closed.

### When to Use
- In domains with strict rules and compile-time discriminated unions.

### When NOT to Use
- In public HTTP APIs requiring RFC 9457 ProblemDetails standardization.

---

## `Maybe<T>` (Option Monad)

Readonly struct monad representing presence (`Some`) or absence (`None`) without null references.

### Syntax
```csharp
public readonly struct Maybe<T> : IEquatable<Maybe<T>>
```

### Key Members
- `Maybe.Some<T>(T value)`
- `Maybe.None<T>()`
- `ToResult(Error notFoundError)`
- `Match<TResult>(Func<T, TResult>, Func<TResult>)`

### Basic Example
```csharp
Maybe<User> user = repo.FindById(10);
Result<User> result = user.ToResult(Error.NotFound("User.NotFound", "User not found."));
```

### Best Practices
- Return `Maybe<T>` from repositories or caches in key-based lookup methods.

### When to Use
- When value absence is an expected, ordinary business scenario.

### When NOT to Use
- When absence indicates a catastrophic precondition failure.

---

## `ResultHttpExtensions.ToHttpResult` (AspNetCore)

Maps `Result` or `Result<T>` directly to `Microsoft.AspNetCore.Http.IResult` conforming to RFC 9457.

### Syntax
```csharp
public static IResult ToHttpResult(this in Result result, ResultHttpOptions? options = null);
public static IResult ToHttpResult<T>(this in Result<T> result, ResultHttpOptions? options = null);
```

### Return Value
`IResult` (`TypedResults.Ok`, `TypedResults.NoContent`, or `TypedResults.Problem`).

### Basic Example
```csharp
app.MapGet("/orders/{id}", (Guid id, OrderService s) => s.GetOrder(id).ToHttpResult());
```

### Best Practices
- Configure `AddResultHttpOptions` in DI to standardize titles and status codes across the application.

### Performance
- Certified for Native AOT; zero reflection at runtime.

### When to Use
- In ASP.NET Core Minimal APIs and MVC controllers.

### When NOT to Use
- Outside the Web presentation layer.

---

## `EntityFrameworkResultExtensions` (EF Core)

Exception-safe extensions for Entity Framework Core DbContext operations.

### Syntax
```csharp
public static Task<Result<int>> SaveChangesAsyncToResult(this DbContext context, CancellationToken ct = default);
public static Task<Result<T>> FirstOrDefaultToResultAsync<T>(this IQueryable<T> source, Expression<Func<T, bool>> predicate, Error notFoundError, CancellationToken ct = default) where T : class;
public static Task<Result<T>> SingleOrDefaultToResultAsync<T>(this IQueryable<T> source, Expression<Func<T, bool>> predicate, Error notFoundError, CancellationToken ct = default) where T : class;
public static Task<Result<List<T>>> ToListToResultAsync<T>(this IQueryable<T> source, CancellationToken ct = default);
```

### Caught Exceptions
- `DbUpdateConcurrencyException` -> Mapped to `ErrorType.Conflict` with code `EntityFrameworkErrorCodes.ConcurrencyConflict`.
- `TimeoutException` -> Mapped to `ErrorType.Unavailable` with `ErrorRetryability.Transient`.

### Basic Example
```csharp
Result<int> saveResult = await dbContext.SaveChangesAsyncToResult();
```

### Best Practices
- Inspect `saveResult.Error.Code == EntityFrameworkErrorCodes.ConcurrencyConflict` to coordinate optimistic concurrency retries.

---

## `FluentValidationResultExtensions` (FluentValidation)

Seamless integration bridging FluentValidation and Result pipelines.

### Syntax
```csharp
public static Result ToValidationResult(this ValidationResult validationResult);
public static Result<T> ToValidationResult<T>(this ValidationResult validationResult, T value);
public static Result<T> EnsureValid<T>(this in Result<T> result, IValidator<T> validator);
public static Task<Result<T>> EnsureValidAsync<T>(this Task<Result<T>> resultTask, IValidator<T> validator, CancellationToken ct = default);
```

### Basic Example
```csharp
ValidationResult val = await validator.ValidateAsync(cmd);
Result<CreateOrderCommand> res = val.ToValidationResult(cmd);
```

### Remarks
Automatically sanitizes passwords and sensitive tokens in error metadata.

---

## `PollyResultExtensions` & `AddResultRetry` (Polly)

Resilience pipeline extensions for Polly v8 with transient error filtering.

### Syntax
```csharp
public static ValueTask<Result<T>> ExecuteResultAsync<T>(this ResiliencePipeline<Result<T>> pipeline, Func<CancellationToken, ValueTask<Result<T>>> callback, CancellationToken ct = default);
public static ResiliencePipelineBuilder<Result<T>> AddResultRetry<T>(this ResiliencePipelineBuilder<Result<T>> builder, int maxRetryAttempts = 3, TimeSpan? delay = null);
```

### Remarks
Retries exclusively if `result.IsFailure && result.Error.Retryability == ErrorRetryability.Transient`. Permanent errors return immediately without retrying.

---

## `ResultFault` & `ResultConsumeFilter` (MassTransit)

Distributed message contracts and consumer filters for asynchronous broker architectures.

### Syntax
```csharp
public sealed class ResultFault
{
    public string Code { get; set; }
    public string Description { get; set; }
    public string Type { get; set; }
    public string Severity { get; set; }
    public string Retryability { get; set; }
    public string? CorrelationId { get; set; }
    public string? TraceId { get; set; }
    public IReadOnlyDictionary<string, object> Metadata { get; set; }

    public static ResultFault FromError(Error error, string? correlationId = null, string? traceId = null);
}
```

### Remarks
Enables publishing domain failures without triggering uncontrolled message re-deliveries in messaging queues.

---

## `ResultActivityExtensions` & `ResultMetrics` (OpenTelemetry)

Distributed tracing and metric recording extensions.

### Syntax
```csharp
public static void RecordResult(this Activity? activity, in Result result);
public static void TraceOutcome(this in Result result, string operationName, Activity? activity = null);
public static Task<Result<T>> TraceOutcomeAsync<T>(this Task<Result<T>> resultTask, string operationName, Activity? activity = null);
public static void StaticTrackSuccess(string operationName);
public static void StaticTrackFailure(string operationName, Error error);
```

### Remarks
Enriches W3C spans with semantic attributes and increments BCL runtime metric counters.

---

## `ResultAssertions` & Test Runner Adapters (Testing)

Fluent assertions with runner-native exception adapters.

### Syntax
```csharp
public static T ShouldBeSuccess<T>(this in Result<T> result, string? message = null);
public static Error ShouldBeFailure(this in Result result, string? message = null);
public static void ResultNUnitAssertionConfig.UseNUnitExceptions();
public static void ResultXUnitAssertionConfig.UseXUnitExceptions();
```

### Remarks
Ensures that assertion failures are reported directly in the runner failure column of NUnit or xUnit without cluttering the output with unhandled exceptions.

---

## `DaprResultStateExtensions` & `DaprResultPubSubExtensions` (Dapr)

State management and Pub/Sub topic integration for Dapr microservices.

### Syntax
```csharp
public static Task<Result<T>> GetStateWithResultAsync<T>(this DaprClient client, string storeName, string key, ConsistencyMode? consistency = null, IReadOnlyDictionary<string, string>? metadata = null, CancellationToken ct = default);
public static Task<Result> SaveStateWithResultAsync<T>(this DaprClient client, string storeName, string key, T value, StateOptions? options = null, IReadOnlyDictionary<string, string>? metadata = null, string? etag = null, CancellationToken ct = default);
public static IResult ToDaprTopicResult(this in Result result);
```

### Pub/Sub Mapping
- `Result.Success` -> **HTTP 200 OK** (Dapr ACK)
- `Result.Failure` with `Transient` -> **HTTP 503 Service Unavailable** (Dapr RETRY)
- `Result.Failure` with `Permanent` -> **HTTP 422 Unprocessable Entity** (Dapr DROP / Dead-letter)

---

## `GrpcErrorMapper` & `ResultServerInterceptor` (gRPC)

Canonical bidirectional status mapping and server interception for gRPC services.

### Syntax
```csharp
public static StatusCode ToRpcStatus(ErrorType errorType);
public static RpcException ToRpcException(this Error error);
public static Result<T> ReconstituteFromTrailers<T>(RpcException rpcException);
public static Task<Result<T>> ToResultAsync<T>(this AsyncUnaryCall<T> call);
```

### Server Interceptor
`ResultServerInterceptor`: Intercepts unary calls and converts any failed `IResultOutcome` return or unhandled exception into an `RpcException` with metadata trailers (`error-code`, `error-type`, `error-meta`).

---

## `DomainErrorsGenerator` (Source Generators)

Roslyn incremental source generator creating static, strongly-typed error catalogs from `*.errors.json`.

### Configuration
```xml
<ItemGroup>
  <AdditionalFiles Include="order.errors.json" />
  <ProjectReference Include="..\..\src\EricksonLopez.Result.DomainErrors.Generators\..."
                    OutputItemType="Analyzer"
                    ReferenceOutputAssembly="false" />
</ItemGroup>
```

### Generated Output
Generates static classes with typed factory methods for each declared error, supporting parameter formatting (`{0}`, `{1}`) and zero runtime allocations.
