# Level 09 — Extensions: Official Ecosystem Integrations

> **Ecosystem:** `EricksonLopez.Result` | **Audience:** System Integrators, Full-Stack .NET Developers | **Complexity:** Level 9 (Advanced) | **Code Reference:** `samples/*`, `22_SerializationShowcase.cs` to `34_ComprehensiveApiCoverageShowcase.cs` | **Language:** English

---

## 1. ASP.NET Core Minimal APIs Integration (`EricksonLopez.Result.AspNetCore`)

Automatically transforms any `Result` or `Result<T>` into an HTTP `IResult` response conforming to the **RFC 9457 (ProblemDetails)** specification:

```csharp
app.MapGet("/users/{id}", async (Guid id, IUserService service) =>
{
    Result<UserDto> result = await service.GetUserAsync(id);
    return result.ToHttpResult();
})
.AddResultEndpointFilter(); // Automatic unwrapping and error translation filter
```

### Default Error to HTTP Status Code Mapping:
| `ErrorType` | HTTP Status Code | RFC 9457 Title |
|---|:---:|---|
| `Validation` | **400 Bad Request** | Bad Request |
| `NotFound` | **404 Not Found** | Not Found |
| `Conflict` | **409 Conflict** | Conflict |
| `Unauthorized` | **401 Unauthorized** | Unauthorized |
| `Forbidden` | **403 Forbidden** | Forbidden |
| `Unavailable` | **503 Service Unavailable** | Service Unavailable |
| `Unexpected` / `Critical` | **500 Internal Server Error** | Internal Server Error |

---

## 2. FluentValidation Integration (`EricksonLopez.Result.FluentValidation`)

Converts FluentValidation `ValidationResult` directly into `Result` and `Result<T>`:

```csharp
using EricksonLopez.Result.FluentValidation;

CreateOrderCommand command = ...;
ValidationResult validation = await _validator.ValidateAsync(command);

Result<CreateOrderCommand> result = validation.ToValidationResult(command);

if (result.IsFailure)
{
    // Contains all property validation failures aggregated into inner errors
    Console.WriteLine($"Validation failed with {result.Error.InnerErrors?.Count} errors.");
}
```

---

## 3. MediatR Pipeline Behavior (`EricksonLopez.Result.MediatR`)

Provides `ResultExceptionBehavior<TRequest, TResponse>` to intercept unhandled exceptions thrown inside MediatR handlers, translating them into `Result.Failure(Error.Unexpected(...))`:

```csharp
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssemblyContaining<Program>();
    cfg.AddResultExceptionBehavior(); // Exception translation in the CQRS pipeline
});
```

---

## 4. OpenTelemetry W3C Tracing & Metrics (`EricksonLopez.Result.OpenTelemetry`)

Enriches the ambient `Activity.Current` span with result status tags and records BCL runtime metrics (`System.Diagnostics.Metrics`):

```csharp
using EricksonLopez.Result.OpenTelemetry;

Result<Order> orderResult = await CreateOrderAsync(command);

// Tags the current Activity with semantic attributes (error.type, ericksonlopez.result.outcome)
orderResult.TraceOutcome("CreateOrder", Activity.Current);

// Emits metrics via static helper (or via DI with ResultMetrics)
ResultMetrics.StaticTrackSuccess("CreateOrder");
```

---

## 5. OpenAPI / Swagger Metadata (`EricksonLopez.Result.OpenApi`)

Annotates endpoint metadata for Minimal APIs OpenAPI schemas:

```csharp
app.MapPost("/orders", CreateOrderHandler)
   .ProducesResult<OrderDto>(StatusCodes.Status201Created)
   .ProducesResultProblemDetails(StatusCodes.Status400BadRequest, StatusCodes.Status409Conflict);
```

---

## 6. Entity Framework Core Integration (`EricksonLopez.Result.EntityFrameworkCore`)

Provides exception-safe query and persistence extensions for `DbContext` and `IQueryable<T>`:

```csharp
using EricksonLopez.Result.EntityFrameworkCore;

// Exception-safe query with strongly-typed domain error fallback
Result<User> userResult = await dbContext.Users
    .FirstOrDefaultToResultAsync(
        u => u.Id == userId,
        notFoundError: Error.NotFound("User.NotFound", "User not found"));

// Exception-safe commit mapping concurrency conflicts and deadlocks
Result<int> saveResult = await dbContext.SaveChangesAsyncToResult();
```

---

## 7. Polly v8 Resilience Integration (`EricksonLopez.Result.Polly`)

Executes monadic pipelines through Polly v8 resilience pipelines with smart retries on transient errors:

```csharp
using EricksonLopez.Result.Polly;
using Polly;

var pipeline = new ResiliencePipelineBuilder<Result<OrderReceipt>>()
    .AddResultRetry(maxRetryAttempts: 3, delay: TimeSpan.FromMilliseconds(200))
    .Build();

Result<OrderReceipt> outcome = await pipeline.ExecuteResultAsync(async ct =>
{
    return await paymentGateway.ChargeAsync(command, ct);
});
```

---

## 8. MassTransit Bus Integration (`EricksonLopez.Result.MassTransit`)

Provides `ResultFault` message contracts and consume filters for distributed systems:

```csharp
using EricksonLopez.Result.MassTransit;

// Map domain Error to distributed fault contract
ResultFault fault = ResultFault.FromError(result.Error);

// Configure filter on consume pipe
cfg.UseResultFilter<ProcessOrderMessage>();
```

---

## 9. Test Runner Adapters (`Testing.NUnit` & `Testing.XUnit`)

Configures `ResultAssertions` to throw native `AssertionException` (NUnit) or `XunitException` (xUnit):

```csharp
// In test assembly setup or module initializer:
ResultXUnitAssertionConfig.UseXUnitExceptions();
// Or:
ResultNUnitAssertionConfig.UseNUnitExceptions();
```

---

## 10. Domain Errors Source Generator (`DomainErrors.Generators`)

Generates compile-time, zero-overhead, Native AOT-safe domain error factories from `*.errors.json` additional files:

```csharp
// Declared in order.errors.json:
// Generated class with strongly-typed parameterized methods:
Error notFound = OrderErrors.NotFound("ORD-98765");
Error paymentFailed = OrderErrors.PaymentFailed("ORD-98765");

Result<Order> result = Result.Failure<Order>(notFound);
```

---

## 11. Dapr Distributed State & Pub/Sub Integration (`EricksonLopez.Result.Dapr`)

Provides exception-safe extension methods for Dapr distributed state store operations with ETag concurrency conflict mapping and Pub/Sub topic response translation:

```csharp
using Dapr.Client;
using EricksonLopez.Result.Dapr;

// 1. Retrieve state safely with Result<T> wrapping and automatic Error.NotFound mapping:
Result<CustomerProfile> profileResult = await daprClient.GetStateWithResultAsync<CustomerProfile>(
    storeName: "statestore",
    key: "customer-10293");

// 2. Save state with ETag verification, mapping concurrency conflicts to ErrorType.Conflict:
Result saveResult = await daprClient.SaveStateWithResultAsync(
    storeName: "statestore",
    key: "customer-10293",
    value: updatedProfile,
    etag: profileResult.Value.ETag);

if (saveResult.IsFailure && saveResult.Error.Code == DaprErrorCodes.EtagMismatch)
{
    // Handle concurrent modification
}

// 3. Map Result outcome to Dapr Pub/Sub topic response (ACK, RETRY, DROP):
app.MapPost("/orders/topic", async (OrderMessage message) =>
{
    Result processResult = await orderService.ProcessAsync(message);
    return processResult.ToDaprTopicResult();
});
```

### Dapr Pub/Sub Disposition Mapping:
| `Result` Status | `ErrorRetryability` | HTTP Status Code | Dapr Action |
|---|---|:---:|---|
| **Success** | N/A | **200 OK** | **ACK** (Message committed) |
| **Failure** | `Transient` | **503 Service Unavailable** | **RETRY** (Backoff retry scheduled) |
| **Failure** | `Permanent` | **422 Unprocessable Entity** | **DROP** (Moved to dead-letter queue) |

---

## 12. gRPC Status Code Mapping & Server Interceptors (`EricksonLopez.Result.Grpc`)

Provides canonical bidirectional translation between `ErrorType` classifications and standard gRPC `StatusCode` values, together with a server interceptor that unwraps `IResultOutcome` failures into `RpcException` with metadata trailers:

```csharp
using EricksonLopez.Result.Grpc;
using Grpc.Core;

// 1. Configure server interceptor in Program.cs:
builder.Services.AddGrpc(options =>
{
    options.Interceptors.Add<ResultServerInterceptor>();
});

// 2. Implement gRPC service returning standard DTOs; interceptor catches failures:
public override async Task<UserResponse> GetUser(GetUserRequest request, ServerCallContext context)
{
    Result<UserResponse> result = await userService.GetByIdAsync(request.UserId);

    if (result.IsFailure)
    {
        // Interceptor or manual extension translates domain Error to RpcException:
        throw result.Error.ToRpcException();
    }

    return result.Value;
}

// 3. Client-side consumption with automated Result reconstitution:
AsyncUnaryCall<OrderResponse> call = client.CreateOrderAsync(request);
Result<OrderResponse> clientResult = await call.ToResultAsync();

if (clientResult.IsFailure)
{
    // Reconstituted with original Code, Description, Type, Severity, and Metadata trailers
    Console.WriteLine($"gRPC error: [{clientResult.Error.Code}] {clientResult.Error.Description}");
}
```

### ErrorType to gRPC StatusCode Mapping Matrix:
| `ErrorType` | gRPC `StatusCode` | Description |
|---|---|---|
| `Validation` | **`InvalidArgument`** | Client specified an invalid argument |
| `NotFound` | **`NotFound`** | Some requested entity was not found |
| `Conflict` | **`AlreadyExists`** | The entity that a client attempted to create already exists |
| `Unauthorized` | **`Unauthenticated`** | The request does not have valid authentication credentials |
| `Forbidden` | **`PermissionDenied`** | The caller does not have permission to execute the specified operation |
| `Unavailable` | **`Unavailable`** | The service is currently unavailable (transient) |
| `Infrastructure` | **`DataLoss`** | Unrecoverable data loss or corruption |
| `Unexpected` / `Failure` | **`Internal`** | Internal system invariant violation |

---

## Next Level
Proceed to **[Level 10 — Enterprise Architecture](level-10-enterprise-architecture.md)** to explore Clean Architecture, Domain-Driven Design boundaries, and CQRS handler topologies.

