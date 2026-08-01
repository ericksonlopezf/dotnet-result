# EricksonLopez.Result

[![NuGet](https://img.shields.io/nuget/v/EricksonLopez.Result?style=for-the-badge&logo=nuget&logoColor=white&color=512BD4)](https://www.nuget.org/packages/EricksonLopez.Result)
[![NuGet Downloads](https://img.shields.io/nuget/dt/EricksonLopez.Result?style=for-the-badge&logo=nuget&logoColor=white&color=004880)](https://www.nuget.org/packages/EricksonLopez.Result)
[![CI](https://img.shields.io/github/actions/workflow/status/ericksonlopez/dotnet-result/ci.yml?branch=main&style=for-the-badge&logo=githubactions&logoColor=white&label=CI)](https://github.com/ericksonlopez/dotnet-result/actions)
[![Coverage](https://img.shields.io/codecov/c/github/ericksonlopez/dotnet-result?style=for-the-badge&logo=codecov&logoColor=white)](https://codecov.io/gh/ericksonlopez/dotnet-result)
[![Mutation Score](https://img.shields.io/badge/Mutation_Score-%E2%89%A598%25-brightgreen?style=for-the-badge&logo=stryker&logoColor=white)](docs/mutation-score.md)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg?style=for-the-badge)](LICENSE)
[![.NET](https://img.shields.io/badge/.NET_8_%7C_9_%7C_10-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)](https://dotnet.microsoft.com)
[![NativeAOT](https://img.shields.io/badge/NativeAOT-Compatible-brightgreen?style=for-the-badge)](https://learn.microsoft.com/en-us/dotnet/core/deploying/native-aot)

A high-performance, struct-based, enterprise-grade **Result Pattern** ecosystem for modern .NET (`.NET 8`, `.NET 9`, `.NET 10`). Designed for production systems demanding zero allocation on happy paths, rich domain error taxonomy, RFC 9457 HTTP ProblemDetails integration, distributed OpenTelemetry tracing, System.Text.Json source generation, FluentValidation integration, MediatR pipeline behaviors, Roslyn analyzers, and fluent testing assertions.

---

## 📦 Ecosystem Packages

| Package | Version | Description |
|---|---|---|
| [`EricksonLopez.Result`](https://www.nuget.org/packages/EricksonLopez.Result) | [![NuGet](https://img.shields.io/nuget/v/EricksonLopez.Result?style=flat-square)](https://www.nuget.org/packages/EricksonLopez.Result) | Core struct-based Result, Error domain model, monadic pipeline, LINQ support, and bundled Roslyn analyzers |
| [`EricksonLopez.Result.AspNetCore`](https://www.nuget.org/packages/EricksonLopez.Result.AspNetCore) | [![NuGet](https://img.shields.io/nuget/v/EricksonLopez.Result.AspNetCore?style=flat-square)](https://www.nuget.org/packages/EricksonLopez.Result.AspNetCore) | ASP.NET Core Minimal APIs filter & RFC 9457 ProblemDetails HTTP response mapper |
| [`EricksonLopez.Result.OpenTelemetry`](https://www.nuget.org/packages/EricksonLopez.Result.OpenTelemetry) | [![NuGet](https://img.shields.io/nuget/v/EricksonLopez.Result.OpenTelemetry?style=flat-square)](https://www.nuget.org/packages/EricksonLopez.Result.OpenTelemetry) | OpenTelemetry `ActivitySource` tracing integration and `System.Diagnostics.Metrics` counters |
| [`EricksonLopez.Result.Serialization`](https://www.nuget.org/packages/EricksonLopez.Result.Serialization) | [![NuGet](https://img.shields.io/nuget/v/EricksonLopez.Result.Serialization?style=flat-square)](https://www.nuget.org/packages/EricksonLopez.Result.Serialization) | `System.Text.Json` custom converters and NativeAOT trim-safe `JsonSerializerContext` |
| [`EricksonLopez.Result.Serialization.Generators`](https://www.nuget.org/packages/EricksonLopez.Result.Serialization.Generators) | [![NuGet](https://img.shields.io/nuget/v/EricksonLopez.Result.Serialization.Generators?style=flat-square)](https://www.nuget.org/packages/EricksonLopez.Result.Serialization.Generators) | Roslyn source generator for AOT-compatible `Result<T>` JSON serialization |
| [`EricksonLopez.Result.FluentValidation`](https://www.nuget.org/packages/EricksonLopez.Result.FluentValidation) | [![NuGet](https://img.shields.io/nuget/v/EricksonLopez.Result.FluentValidation?style=flat-square)](https://www.nuget.org/packages/EricksonLopez.Result.FluentValidation) | FluentValidation integration — converts `ValidationResult` to structured `Result` failures |
| [`EricksonLopez.Result.MediatR`](https://www.nuget.org/packages/EricksonLopez.Result.MediatR) | [![NuGet](https://img.shields.io/nuget/v/EricksonLopez.Result.MediatR?style=flat-square)](https://www.nuget.org/packages/EricksonLopez.Result.MediatR) | MediatR pipeline behavior — catches unhandled exceptions and wraps them as `Result` failures |
| [`EricksonLopez.Result.Testing`](https://www.nuget.org/packages/EricksonLopez.Result.Testing) | [![NuGet](https://img.shields.io/nuget/v/EricksonLopez.Result.Testing?style=flat-square)](https://www.nuget.org/packages/EricksonLopez.Result.Testing) | Framework-agnostic fluent testing assertion library (`ShouldBeSuccess()`, `ShouldHaveError()`) |
| [`EricksonLopez.Result.Testing.XUnit`](https://www.nuget.org/packages/EricksonLopez.Result.Testing.XUnit) | [![NuGet](https://img.shields.io/nuget/v/EricksonLopez.Result.Testing.XUnit?style=flat-square)](https://www.nuget.org/packages/EricksonLopez.Result.Testing.XUnit) | xUnit-specific test helpers — assertion failures surface as `XunitException` |
| [`EricksonLopez.Result.Testing.NUnit`](https://www.nuget.org/packages/EricksonLopez.Result.Testing.NUnit) | [![NuGet](https://img.shields.io/nuget/v/EricksonLopez.Result.Testing.NUnit?style=flat-square)](https://www.nuget.org/packages/EricksonLopez.Result.Testing.NUnit) | NUnit-specific test helpers — assertion failures surface as `AssertionException` |
| [`EricksonLopez.Result.Analyzers`](https://www.nuget.org/packages/EricksonLopez.Result.Analyzers) | [![NuGet](https://img.shields.io/nuget/v/EricksonLopez.Result.Analyzers?style=flat-square)](https://www.nuget.org/packages/EricksonLopez.Result.Analyzers) | Roslyn analyzers & code fixes (RESULT001–009) — bundled with Core, also available standalone |

---

## ⚡ Key Features

- 🚀 **Zero-Allocation Envelope**: Core `Result` and `Result<TValue>` are `readonly struct` value types — zero heap allocation for success results.
- ⚡ **Closure-Free `TState` Pipeline**: All monadic operators (`Map`, `Bind`, `Tap`, `Match`, `Switch`, `Ensure`, `Recover`) offer `TState` overloads to completely eliminate lambda closure allocations in hot execution paths.
- 🔒 **Rich Enterprise Error Taxonomy**: Extensible `Error` class featuring `ErrorType`, `ErrorSeverity`, `ErrorRetryability`, lazy `TraceId` (ambient `Activity`), `CorrelationId`, localized keys, and immutable `Metadata`.
- 🧩 **Span-Based `Result.Combine`**: Aggregates up to 8 typed tuples or `ReadOnlySpan<Result>` using `ArrayPool<Error>` to eliminate temporary array allocations.
- 🌐 **ASP.NET Core RFC 9457 Integration**: Automatic mapping of `Result` to HTTP responses (`200 OK`, `400 Bad Request`, `401 Unauthorized`, `403 Forbidden`, `404 Not Found`, `409 Conflict`, `503 Unavailable`, `500 Server Error`).
- 📊 **First-Class OpenTelemetry**: Automatically attaches error status, code, type, severity, and metadata to active `Activity` spans and records metrics via `System.Diagnostics.Metrics`.
- ⚡ **NativeAOT & Trimming Safe**: Designed with zero reflection in hot paths, featuring source-generated `JsonSerializerContext` definitions.
- 🧪 **Fluent Test Assertions**: Declarative assertion API for unit testing with standard test frameworks (xUnit, NUnit, MSTest). Fully supports asynchronous `ValueTask` validation avoiding testing framework deadlocks.

---

## Table of Contents

- [Installation](#installation)
- [Quick Start](#quick-start)
  - [1. Core Result & Domain Errors](#1-core-result--domain-errors)
  - [2. Monadic Pipeline (Railway-Oriented)](#2-monadic-pipeline-railway-oriented)
  - [3. Zero-Allocation TState Pattern](#3-zero-allocation-tstate-pattern)
  - [4. ASP.NET Core Integration](#4-aspnet-core-integration)
  - [5. Distributed Tracing & Metrics](#5-distributed-tracing--metrics)
  - [6. JSON Serialization](#6-json-serialization)
  - [7. Unit Testing with Fluent Assertions](#7-unit-testing-with-fluent-assertions)
  - [8. FluentValidation Integration](#8-fluentvalidation-integration)
  - [9. MediatR Pipeline Behavior](#9-mediatr-pipeline-behavior)
- [Roslyn Analyzers](#roslyn-analyzers)
- [API Reference](#api-reference)
- [Performance Benchmarks](#performance-benchmarks)
- [NativeAOT & Trimming Compatibility](#nativeaot--trimming-compatibility)
- [Architecture & Design Decisions](#architecture--design-decisions)
- [Part of the EricksonLopez Ecosystem](#part-of-the-ericksonlopez-ecosystem)
- [License](#license)

---

## Installation

Install the required packages using the .NET CLI:

```bash
# Core Package (includes bundled Roslyn analyzers)
dotnet add package EricksonLopez.Result

# Optional Framework & Tooling Packages
dotnet add package EricksonLopez.Result.AspNetCore
dotnet add package EricksonLopez.Result.OpenTelemetry
dotnet add package EricksonLopez.Result.Serialization
dotnet add package EricksonLopez.Result.FluentValidation
dotnet add package EricksonLopez.Result.MediatR
dotnet add package EricksonLopez.Result.Testing
dotnet add package EricksonLopez.Result.Testing.XUnit  # For xUnit test projects
dotnet add package EricksonLopez.Result.Testing.NUnit  # For NUnit test projects
```

---

## Quick Start

### 1. Core Result & Domain Errors

Return `Result` or `Result<T>` instead of throwing control-flow exceptions.

```csharp
using EricksonLopez.Result;

public static class UserErrors
{
    public static Error NotFound(Guid id) =>
        Error.NotFound("User.NotFound", $"User with ID '{id}' was not found.");

    public static readonly Error InvalidEmail =
        Error.Validation("User.InvalidEmail", "The email format is invalid.");

    public static readonly Error Suspended =
        Error.Forbidden("User.Suspended", "User account has been suspended.")
             .WithRetryability(ErrorRetryability.Permanent);
}

public class UserService
{
    public Result<User> GetUser(Guid id)
    {
        var user = _repository.Find(id);
        return user is null 
            ? UserErrors.NotFound(id) 
            : user; // Implicit conversion to Result<User>.Success(user)
    }
}
```

### 2. Monadic Pipeline (Railway-Oriented)

Chain synchronous and asynchronous operations effortlessly without `try/catch` or nested `if/else` checks:

```csharp
public async Task<Result<OrderDto>> ProcessOrderAsync(Guid userId, CreateOrderCommand command, CancellationToken cancellationToken)
{
    return await _userService.GetUserAsync(userId, cancellationToken)
        .Ensure(u => u.IsActive, UserErrors.Suspended, cancellationToken)
        .Bind(u => _orderService.CreateOrderAsync(u, command, cancellationToken))
        .Tap(order => _logger.LogInformation("Order {Id} created", order.Id), cancellationToken)
        .TapError(error => _logger.LogWarning("Order creation failed: {Code}", error.Code), cancellationToken)
        .Map(order => new OrderDto(order.Id, order.TotalAmount), cancellationToken);
}
```

**Pattern Matching & Unwrapping:**

```csharp
// Fluent pattern matching
string response = result.Match(
    dto => $"Success: Order {dto.Id}",
    error => $"Error ({error.Code}): {error.Description}"
);

// Try-Get pattern (Idiomatic .NET)
if (result.TryGetValue(out var order))
{
    Console.WriteLine(order.Id);
}

// Tuple Destructuring
var (isSuccess, value, error) = result;
if (isSuccess)
{
    Console.WriteLine(value.Id);
}
```

---

### 3. Zero-Allocation TState Pattern

Standard lambda expressions capturing local variables create heap-allocated closure objects. High-throughput applications can eliminate closure allocations by passing `TState` as a parameter alongside `static` lambdas:

```csharp
var minPrice = 50.0m;
var maxPrice = 500.0m;

// ❌ Allocates: captures 'minPrice' and 'maxPrice' in a closure object
var result = GetProduct(id)
    .Ensure(p => p.Price >= minPrice && p.Price <= maxPrice, Error.Validation("Price.OutOfRange", "Price out of bounds"));

// ✅ Zero Closure Allocation: state passed via tuple and static lambda
var state = (minPrice, maxPrice);
var result = GetProduct(id)
    .Ensure(state, static (s, p) => p.Price >= s.minPrice && p.Price <= s.maxPrice, Error.Validation("Price.OutOfRange", "Price out of bounds"));
```

Overloads supporting `TState` are available for `Map`, `Bind`, `Tap`, `Match`, `Switch`, `Ensure`, and `Recover`.

---

### 4. ASP.NET Core Integration

`EricksonLopez.Result.AspNetCore` maps `Result` and `Result<T>` directly to HTTP responses according to RFC 9457 (ProblemDetails).

```csharp
using EricksonLopez.Result.AspNetCore;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// Minimal API Endpoint returning Result<T> converted to HttpResult
app.MapGet("/users/{id:guid}", (Guid id, UserService userService) =>
{
    return userService.GetUser(id).ToHttpResult();
});

// Automatic Minimal API Endpoint Filter unwrapping Result responses
app.MapPost("/orders", (CreateOrderCommand command, OrderService orderService) =>
{
    return orderService.CreateOrder(command); // Returns Result<OrderDto>
})
.AddResultEndpointFilter()
.Produces<OrderDto>(StatusCodes.Status200OK)
.ProducesProblem(StatusCodes.Status400BadRequest);

app.Run();
```

**HTTP Status Mapping Matrix:**

| `ErrorType` | HTTP Status Code | RFC Title |
|---|---|---|
| `ErrorType.Validation` | `400 Bad Request` | Bad Request |
| `ErrorType.Unauthorized` | `401 Unauthorized` | Unauthorized |
| `ErrorType.Forbidden` | `403 Forbidden` | Forbidden |
| `ErrorType.NotFound` | `404 Not Found` | Not Found |
| `ErrorType.Conflict` | `409 Conflict` | Conflict |
| `ErrorType.Unavailable` | `503 Service Unavailable` | Service Unavailable |
| `ErrorType.Failure` / `Unexpected` | `500 Internal Server Error` | Internal Server Error |

> [!WARNING]
> **OpenAPI / Swagger Note:** When using `AddResultEndpointFilter()`, the success value is returned as `object?` internally. OpenAPI metadata generators (e.g., Swashbuckle, NSwag) cannot automatically infer the response schema. **You must add `.Produces<T>(200)` to your endpoint** for accurate Swagger documentation, as shown in the example above. We provide a Roslyn Analyzer (RESULT007) that will warn you if you forget to add `.Produces<T>()`.

> [!WARNING]
> **Performance Note (Boxing):** `ResultEndpointFilter` matches `Result<T>` via `is IResultOutcome`, which **boxes** the struct on every request (allocates on the heap regardless of success or failure). For high-throughput endpoints where zero allocation is critical, call `ToHttpResult()` directly from your handler instead:
> ```csharp
> app.MapGet("/orders/{id}", async (Guid id, IOrderService svc) =>
> {
>     Result<OrderDto> result = await svc.GetOrderAsync(id);
>     return result.ToHttpResult(); // no boxing — returns typed Ok<OrderDto>
> });
> ```
> This returns `Ok<OrderDto>` (not `Ok<object?>`) and gives OpenAPI tooling full type inference without `.Produces<T>()`.

---

### 5. Distributed Tracing & Metrics

`EricksonLopez.Result.OpenTelemetry` automatically instruments active OpenTelemetry `Activity` spans and collects system metrics:

```csharp
using EricksonLopez.Result.OpenTelemetry;

public async Task<Result<PaymentReceipt>> ExecutePaymentAsync(PaymentRequest request)
{
    using var activity = MyActivitySource.StartActivity("ExecutePayment");
    
    var result = await _paymentGateway.ProcessAsync(request);

    // Records error status, error code, type, severity, and metadata tags on the Activity span
    activity?.RecordResult(result);
    
    // Instrument metrics counter and duration histogram
    ResultMetrics.RecordOutcome("ExecutePayment", result);

    return result;
}
```

---

### 6. JSON Serialization

`EricksonLopez.Result.Serialization` provides custom converters for `Result`, `Result<T>`, and `Error`.

```csharp
using System.Text.Json;
using EricksonLopez.Result.Serialization;

var options = new JsonSerializerOptions();
options.Converters.Add(new ResultJsonConverterFactory());
options.Converters.Add(new ErrorJsonConverter());

// Serialize
string json = JsonSerializer.Serialize(Result.Success(42), options);
```

#### NativeAOT / Trimming Setup

> ⚠️ **Important:** `ResultJsonConverterFactory` uses `MakeGenericType` and `Activator.CreateInstance` internally, which are **not compatible** with NativeAOT or aggressive trimming. For NativeAOT scenarios, you must register converters explicitly for each `Result<T>` you serialize.

**Step 1: Register concrete converters for each `Result<T>` type:**

```csharp
using System.Text.Json;
using System.Text.Json.Serialization;
using EricksonLopez.Result.Serialization;

// ✅ NativeAOT-safe: register concrete converters directly
var options = new JsonSerializerOptions();
options.Converters.Add(new ResultJsonConverter());              // non-generic Result
options.Converters.Add(new ErrorJsonConverter());               // Error
options.Converters.Add(new ResultOfTJsonConverter<OrderDto>()); // each Result<T> you serialize
options.Converters.Add(new ResultOfTJsonConverter<int>());
options.Converters.Add(new ResultOfTJsonConverter<UserDto>());
```

**Step 2: Register your DTOs in a `JsonSerializerContext` for source generation:**

```csharp
// Ensure ALL types used as T in Result<T> are registered for source generation.
// This is required for NativeAOT to generate the serialization metadata at compile time.
[JsonSerializable(typeof(OrderDto))]
[JsonSerializable(typeof(UserDto))]
[JsonSerializable(typeof(int))]
[JsonSerializable(typeof(Result))]
[JsonSerializable(typeof(Error))]
public partial class AppJsonContext : JsonSerializerContext { }
```

**Step 3: Use in ASP.NET Core Minimal APIs (DI scenario):**

```csharp
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonContext.Default);
    options.SerializerOptions.Converters.Add(new ResultJsonConverter());
    options.SerializerOptions.Converters.Add(new ErrorJsonConverter());
    options.SerializerOptions.Converters.Add(new ResultOfTJsonConverter<OrderDto>());
    options.SerializerOptions.Converters.Add(new ResultOfTJsonConverter<UserDto>());
});
```

> ❌ **Do NOT use `ResultJsonConverterFactory`** in NativeAOT builds — it will fail at runtime with `InvalidOperationException` because `MakeGenericType` is not supported. Use the explicit per-type registration shown above.

> ⚠️ **Metadata Round-Trip Note:** `Error.Metadata` values are serialized preserving native JSON types (numbers, booleans, strings). On deserialization, numeric types are recovered as `long` or `double` (not `int`, `short`, etc.), and `DateTime`/`Guid`/custom objects become `string`. This means `Error.StrictEquals()` **will return `false`** after a serialize → deserialize cycle if metadata contains numeric values (since `int` ≠ `long`). Use `Error.Equals()` (which only compares `Code`, `Description`, `Type`, `Severity`) for post-serialization comparisons. For type-faithful round-tripping, store metadata as a typed DTO instead.

> ⚠️ **ASP.NET Core NativeAOT Note:** The `ResultEndpointFilter` uses `ErrorDetailDto` in ProblemDetails `extensions`. If your app uses a `JsonSerializerContext` for NativeAOT, register `ErrorDetailDto` and `List<ErrorDetailDto>` in your context:
> ```csharp
> [JsonSerializable(typeof(EricksonLopez.Result.AspNetCore.ErrorDetailDto))]
> [JsonSerializable(typeof(List<EricksonLopez.Result.AspNetCore.ErrorDetailDto>))]
> public partial class AppJsonContext : JsonSerializerContext { }
> ```

---

### 7. Unit Testing with Fluent Assertions

`EricksonLopez.Result.Testing` simplifies unit test assertions and fully supports asynchronous execution without causing deadlocks in runners like Coverlet or xUnit:

```csharp
using EricksonLopez.Result.Testing;
using Xunit;

public class UserServiceTests
{
    [Fact]
    public void GetUser_ShouldReturnSuccess_WhenUserExists()
    {
        Result<User> result = _userService.GetUser(existingId);

        result.ShouldBeSuccess()
              .Value.Name.ShouldBe("Erickson");
    }

    [Fact]
    public async Task GetUserAsync_ShouldReturnFailure_WhenNotFound()
    {
        Result<User> result = await _userService.GetUserAsync(nonExistingId);

        await result.ShouldBeFailureAsync()
                    .ShouldHaveErrorAsync("User.NotFound")
                    .ShouldHaveErrorTypeAsync(ErrorType.NotFound);
    }
}
```

---

### 8. FluentValidation Integration

`EricksonLopez.Result.FluentValidation` converts `FluentValidation.ValidationResult` into structured `Result` failures with rich error metadata.

```csharp
using EricksonLopez.Result.FluentValidation;

// Convert ValidationResult to Result
var validator = new OrderValidator();
Result result = validator.Validate(order).ToResult();

// Or validate and wrap the validated object in Result<T>
Result<Order> typedResult = validator.Validate(order).ToResult(order);

// Pipeline integration: validate inside a Result chain
var pipelineResult = await GetOrderAsync()
    .EnsureValid(new OrderValidator())
    .Map(order => ProcessOrder(order));
```

Each `ValidationFailure` is mapped to a structured `Error` with:
- `ErrorType.Validation` type
- Error code from `ValidationFailure.ErrorCode` (or `Validation.{PropertyName}` fallback)
- Metadata: `propertyName`, `attemptedValue`, and FluentValidation severity mapping

---

### 9. MediatR Pipeline Behavior

`EricksonLopez.Result.MediatR` provides a pipeline behavior that catches unhandled exceptions in MediatR handlers and wraps them as `Result` failures.

```csharp
using EricksonLopez.Result.MediatR;

// Register the behavior in DI
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));
builder.Services.AddResultExceptionBehavior();

// Or with a custom error factory
builder.Services.AddResultExceptionBehavior(ex =>
    Error.Unexpected($"Handler.{ex.GetType().Name}", ex.Message)
         .WithMetadata("stackTrace", ex.StackTrace ?? ""));
```

> **Note:** `ResultExceptionBehavior` only activates when the response type is `Result` or `Result<T>`. Non-Result responses pass through unmodified. `OperationCanceledException` is always re-thrown.

---

## Roslyn Analyzers

The `EricksonLopez.Result.Analyzers` package is bundled with the core `EricksonLopez.Result` package and provides compile-time diagnostics:

| Diagnostic ID | Severity | Description |
|---|---|---|
| `RESULT001` | ⚠️ Warning | `Result<T>` used with a struct type larger than 64 bytes — recommends using a class to avoid excessive copying |

| `RESULT003` | ⚠️ Warning | `ErrorBuilder.With*()` return value is discarded — the mutated struct copy is lost |

---

## API Reference

Comprehensive documentation for all primitives, pipeline methods, and packages:

- 📑 [Architecture Overview](docs/Architecture.md) — Mermaid diagrams, pipeline design, and component interaction.
- 🔧 [CI/CD & Build Pipeline](docs/CICD.md) — GitHub Actions workflows, release strategy, and supply chain security.
- 📊 [Quality Gates](docs/QualityGates.md) — Code coverage, mutation testing, and static analysis configuration.
- 💡 [Best Practices](docs/BestPractices.md) — Recommended patterns for production applications.
- ⚠️ [Anti-Patterns](docs/AntiPatterns.md) — Pitfalls, unsafe state accesses, and code smells to avoid.
- 📖 [Cookbook](docs/Cookbook.md) — Copy-pasteable recipes for web APIs, OpenTelemetry, testing, and LINQ syntax.
- 🔄 [Migration Guide](docs/MigrationGuide.md) — Guide for migrating from raw exceptions, `FluentResults`, `CSharpFunctionalExtensions`, `OneOf`, or `ErrorOr`.
- ⚡ [Allocation Analysis](docs/analysis/allocations.md) — Deep dive into memory benchmarks, struct layout, and zero-allocation mechanics.
- 🧬 [Mutation Score](docs/mutation-score.md) — Latest Stryker mutation testing results, threshold configuration (`break: 95`), and analysis of all surviving/equivalent mutants.
- 🏛️ [Architectural Decision Records (ADRs)](docs/decisions/) — ADRs 001 through 016 documenting key architectural choices.
- 📦 [Package Reference](docs/PackageReference.md) — Full compatibility matrix, dependency graph, and per-package details for all 11 NuGet packages.

---

## Performance Benchmarks

> **Environment:** .NET 10.0.10 (10.0.1026.32716), X64 RyuJIT AVX-512, BenchmarkDotNet v0.14.0

### Result Construction — Zero allocation on success

| Method | Mean | Allocated |
|---|---:|---:|
| `Result.Success()` | 0.000 ns | **0 B** |
| `Result.Success("value")` | 0.000 ns | **0 B** |
| `Result.Success(42)` | 0.002 ns | **0 B** |
| Implicit `TValue → Result<T>` | 0.003 ns | **0 B** |
| Implicit `Error → Result<T>` | 0.759 ns | **0 B** |
| `Result.Failure(error)` | 0.777 ns | **0 B** |

### Pipeline Operations — Sub-nanosecond Map/Bind

| Method | Mean | Allocated |
|---|---:|---:|
| `Tap` (success, lambda) | 0.16 ns | **0 B** |
| `Map` (success, lambda) | 0.77 ns | **0 B** |
| `Ensure` (success, passes) | 0.94 ns | **0 B** |
| `Bind` (success, lambda) | 2.12 ns | **0 B** |
| Full pipeline (3-stage, TState) | 7.17 ns | 32 B |

### Combine — ArrayPool-backed aggregation

| Method | Count | Mean | Allocated |
|---|---:|---:|---:|
| All success | 4 | 4.0 ns | **0 B** |
| One failure | 4 | 4.3 ns | **0 B** |
| All success | 64 | 26.3 ns | **0 B** |
| All failures | 64 | 175.7 ns | 696 B |

### Error Builder vs WithMetadata Chain

| Method | Mean | Allocated |
|---|---:|---:|
| `Error.Create(...).Build()` | 3.1 ns | 96 B |
| `Error.WithMetadata()` × 3 chain | 146.7 ns | 752 B |
| `ErrorBuilder.WithMetadata()` × 3 | 85.8 ns | 464 B |

> ⚡ **Key insight:** `ErrorBuilder.WithMetadata()` is **1.7× faster** with **38% fewer allocations** than chaining `Error.WithMetadata()` calls. Always use the builder for multiple metadata entries.

### Async Pipeline — Sync-path optimization

| Method | Mean | Allocated |
|---|---:|---:|
| `Map` (sync completed) | 7.9 ns | 160 B |
| `Map` (async completed) | 464.2 ns | 237 B |

> 💡 When the `Task<Result<T>>` is already completed synchronously (common in cached/pooled scenarios), the async pipeline avoids the state machine entirely — **59× faster** than the true-async path.

Run benchmarks yourself:
```bash
cd benchmarks/EricksonLopez.Result.Benchmarks
dotnet run -c Release
```

---

## NativeAOT & Trimming Compatibility

| Package | NativeAOT Compatible | Trimmable | Notes |
|---|---|---|---|
| `EricksonLopez.Result` | ✅ Yes | ✅ Yes | Core types use zero reflection |
| `EricksonLopez.Result.AspNetCore` | ✅ Yes | ✅ Yes | Compatible with STJ Source Generators |
| `EricksonLopez.Result.OpenTelemetry` | ✅ Yes | ✅ Yes | Native OpenTelemetry Activity API |
| `EricksonLopez.Result.Serialization` | ⚠️ Partial | ⚠️ Partial | `ResultJsonConverterFactory` uses `MakeGenericType`; use explicit `ResultOfTJsonConverter<T>` for AOT (see [NativeAOT Setup](#nativeaot--trimming-setup)) |
| `EricksonLopez.Result.Serialization.Generators` | ✅ Yes | ✅ Yes | Source generator — runs at compile time, dev dependency only |
| `EricksonLopez.Result.FluentValidation` | ✅ Yes | ✅ Yes | No reflection in library code |
| `EricksonLopez.Result.MediatR` | ❌ No | ❌ No | MediatR uses reflection internally; `CreateFailure` uses `MakeGenericMethod` |
| `EricksonLopez.Result.Testing` | ✅ Yes | ✅ Yes | Unit test libraries are not compiled into AOT binaries |


## Common Pitfalls

> [!CAUTION]
> The following patterns compile without errors but produce incorrect runtime behavior. Read these before shipping to production.

### Pitfall 1 — `default(Result)` evaluates silently as `false` in boolean context

`Result` and `Result<T>` are structs. When a field is not initialized (e.g., a not-null field in a class, or a struct obtained from `new MyClass()`), their default value is `ResultState.Uninitialized` — which is **neither success nor failure**.

```csharp
// Anti-pattern: default(Result) in a boolean context
Result result = default; // Uninitialized — not Success, not Failure

if (result)              // evaluates as false — looks like "failure" but is NOT
    Console.WriteLine("Success");
else
    Console.WriteLine("Failure");  // prints this, silently wrong

// The safe check:
if (result.IsUninitialized)
    throw new InvalidOperationException("Result was never assigned.");
```

**Why this happens:** `operator true` and `operator false` delegate to `IsSuccess` and `IsFailure`. An uninitialized result returns `false` for both, so `if (result)` silently evaluates to `false`.

**Solution:** Always obtain a `Result` through `Result.Success()` or `Result.Failure(error)`, never through `default` or field initialization. If you receive a `Result` from a method that might return `default`, check `IsUninitialized` before using it.

```csharp
// Safe: check for uninitialized state before using
var result = GetResultFromSomewhere();
if (result.IsUninitialized)
    throw new InvalidOperationException("Service returned an uninitialized result.");
return result.Match(/* ... */);
```

### Pitfall 2 — `AddResultEndpointFilter()` requires `.Produces<T>()` for OpenAPI

`AddResultEndpointFilter()` uses the `IResultOutcome` interface to detect `Result<T>` at runtime. This causes two side effects:

1. **Boxing on every request:** `Result<T>` (a struct) is boxed to the heap per request (1–2 allocations).
2. **OpenAPI schema degradation:** The filter returns `Ok<object?>` internally. Swagger/NSwag cannot infer `T` — the schema shows `object` without `.Produces<T>()`.

```csharp
// Anti-pattern: no .Produces<T>() — OpenAPI shows object schema
app.MapGet("/orders/{id}", (Guid id) => orderService.GetOrder(id))
   .AddResultEndpointFilter(); // OpenAPI: responses: { "200": { schema: object } }

// Correct: explicit .Produces<T>() for full OpenAPI metadata
app.MapGet("/orders/{id}", (Guid id) => orderService.GetOrder(id))
   .AddResultEndpointFilter()
   .Produces<OrderDto>(StatusCodes.Status200OK)       // required for OpenAPI
   .ProducesProblem(StatusCodes.Status404NotFound);
```

**For high-throughput endpoints (> 10k req/s),** use `ToHttpResult()` directly to avoid boxing and get full OpenAPI inference automatically:

```csharp
// Zero boxing + full OpenAPI inference — preferred for performance-sensitive paths
app.MapGet("/orders/{id}", async (Guid id, IOrderService svc) =>
{
    var result = await svc.GetOrderAsync(id);
    return result.ToHttpResult(); // returns typed Ok<OrderDto>, no boxing
});
```

> The Roslyn analyzer **RESULT008** (`EndpointFilterOpenApiAnalyzer`) warns at compile time when `.AddResultEndpointFilter()` is called without `.Produces<T>()`. Install `EricksonLopez.Result.Analyzers` to enable it.

### Pitfall 3 — `HashSet<Error>` deduplicates errors with the same semantic fields

`Error.Equals()` compares 5 semantic fields: `Code`, `Description`, `Type`, `Severity`, `Retryability`. It intentionally excludes `TraceId`, `CorrelationId`, and `Metadata` (which vary per request).

```csharp
// Two errors with same semantic fields but different trace IDs
var e1 = Error.NotFound("Order.NotFound", "Order not found").WithTraceId("trace-1");
var e2 = Error.NotFound("Order.NotFound", "Order not found").WithTraceId("trace-2");

var set = new HashSet<Error> { e1, e2 };
Console.WriteLine(set.Count); // 1 — silently deduplicated!

// Safe: use ErrorEqualityComparer.Strict for strict equality (includes all fields)
var strictSet = new HashSet<Error>(ErrorEqualityComparer.Strict) { e1, e2 };
Console.WriteLine(strictSet.Count); // 2
```

> The Roslyn analyzer **RESULT007** (`HashSetErrorEqualityAnalyzer`) warns at compile time when `new HashSet<Error>()`, `.Distinct()`, `.ToHashSet()`, or `.GroupBy()` is used on `Error` sequences without `ErrorEqualityComparer.Strict`.

---
---

## Part of the EricksonLopez Ecosystem

`EricksonLopez.Result` is a foundational component of the **EricksonLopez** library ecosystem:

- 🧱 [EricksonLopez.SharedKernel](https://github.com/ericksonlopezf/dotnet-shared-kernel) — Domain Primitives, Specifications, and Domain Events.
- ⚡ **EricksonLopez.Result** — High-Performance Struct-Based Result Pattern & Telemetry.

---

## License

Distributed under the [MIT License](LICENSE). Copyright © 2026 Erickson Lopez.