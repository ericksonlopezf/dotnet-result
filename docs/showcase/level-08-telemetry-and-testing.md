# Level 08 — Observability & Fluent Testing Framework

> **Ecosystem:** `EricksonLopez.Result` | **Audience:** DevOps, QA & Senior Software Engineers | **Language:** English

---

## 1. OpenTelemetry Distributed Tracing & Metrics

Install the observability package:

```bash
dotnet add package EricksonLopez.Result.OpenTelemetry
```

### 1.1 BCL-Only Design Philosophy
`EricksonLopez.Result.OpenTelemetry` is built directly on standard BCL primitives:
- `System.Diagnostics.ActivitySource` for distributed tracing.
- `System.Diagnostics.Metrics` for runtime counters and histograms.

### 1.2 Registration & Tracing

```csharp
// Program.cs
builder.Services.AddOpenTelemetry()
    .WithTracing(tracerProviderBuilder =>
    {
        tracerProviderBuilder
            .AddSource("EricksonLopez.Result") // Registers Result ActivitySource
            .AddOtlpExporter();
    })
    .WithMetrics(meterProviderBuilder =>
    {
        meterProviderBuilder
            .AddMeter("EricksonLopez.Result") // Registers Result Meter
            .AddOtlpExporter();
    });
```

### 1.3 Automatic Activity Tagging
When executing instrumented pipelines with `TraceOutcome`:
- `ericksonlopez.result.outcome`: `"success"` or `"failure"`.
- `ericksonlopez.result.error.code`: e.g. `"Payment.GatewayTimeout"`.
- `error.type`: e.g. `"Validation"`.
- `ericksonlopez.result.error.severity`: e.g. `"Error"`.

---

## 2. Fluent Testing Framework

Install the testing assertion package for your framework of choice:

```bash
dotnet add package EricksonLopez.Result.Testing.XUnit
# or
dotnet add package EricksonLopez.Result.Testing.NUnit
```

### 2.1 Idiomatic Assertion Syntax

Write clean, expressive unit tests:

```csharp
using EricksonLopez.Result;
using EricksonLopez.Result.Testing;
using Xunit;

public class UserServiceTests
{
    [Fact]
    public async Task CreateUser_WhenValid_ReturnsSuccessWithUser()
    {
        // Act
        Result<User> result = await _sut.CreateUserAsync("Erickson", "dev@ericksonlopez.dev");

        // Assert
        User user = result.ShouldBeSuccess();
        Assert.Equal("Erickson", user.Name);
        Assert.True(user.Id != Guid.Empty);
    }

    [Fact]
    public async Task CreateUser_WhenDuplicate_ReturnsConflictError()
    {
        // Act
        Result<User> result = await _sut.CreateUserAsync("Existing", "duplicate@test.com");

        // Assert
        Error error = result.ShouldBeFailure()
                            .ShouldHaveErrorCode("User.DuplicateEmail")
                            .ShouldHaveErrorType(ErrorType.Conflict);
        Assert.Equal(ErrorRetryability.Permanent, error.Retryability);
    }
}
```

---

## 3. Summary of the Showcase Series

Congratulations! You have completed the `EricksonLopez.Result` showcase series:
- **Level 00**: Architecture & Functional Philosophy
- **Level 01**: Getting Started & Core Primitives
- **Level 02**: Domain Modeling & Rich Error Taxonomy
- **Level 03**: Railway Pipelines & Monadic Composition
- **Level 04**: Compound Validation & Maybe Monad
- **Level 05**: ASP.NET Core & RFC 9457 ProblemDetails
- **Level 06**: Integrations: FluentValidation, MediatR & Analyzers
- **Level 07**: Native AOT & Zero-Reflection Serialization
- **Level 08**: Observability & Fluent Testing Framework
