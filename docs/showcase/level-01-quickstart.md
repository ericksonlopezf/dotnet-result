# Level 01 — Quick Start: Primitives & First Functional Use

> **Ecosystem:** `EricksonLopez.Result` | **Audience:** Developers, Software Engineers | **Complexity:** Level 1 (Beginner) | **Code Reference:** `01_BasicCreation.cs`, `03_MatchingAndExecuting.cs` | **Language:** English

---

## 1. Installing NuGet Packages

Install the core package and optional extensions using the .NET CLI or NuGet Package Manager:

```bash
# Core Result pattern ecosystem
dotnet add package EricksonLopez.Result

# Optional integrations for ASP.NET Core and validation
dotnet add package EricksonLopez.Result.AspNetCore
dotnet add package EricksonLopez.Result.FluentValidation
```

---

## 2. Minimal Startup Configuration & Dependency Injection

For console applications, worker processes, or class libraries, **no startup registration is required (zero ceremony)**.

For ASP.NET Core applications and dependency injection environments (`IServiceCollection`), you can optionally register HTTP options and runtime metrics:

```csharp
var builder = WebApplication.CreateBuilder(args);

// Optional configuration for RFC 9457 HTTP ProblemDetails and runtime metrics
builder.Services.AddResultHttp(options =>
{
    // Configure environment-aware description exposure or custom status mappings
});
builder.Services.AddResultMetrics();

var app = builder.Build();
```

---

## 3. Core Primitives: Creating Results

### Operations Without a Return Value (`Result`)
```csharp
using EricksonLopez.Result;

// 1. Successful operation (0 bytes allocated on GC heap)
Result success = Result.Success();

// 2. Failed operation with domain error diagnostics
Result failure = Result.Failure(Error.NotFound("User.NotFound", "The requested user does not exist."));
```

### Operations With a Return Value (`Result<TValue>`)
```csharp
// 1. Success with typed payload
Result<int> answer = Result.Success(42);

// 2. Typed failure
Result<int> failedCalc = Result.Failure<int>(Error.Validation("Math.DivisionByZero", "Cannot divide by zero."));

// 3. Idiomatic implicit conversions
Result<string> greeting  = "Hello, World!";                                    // TValue → Result<TValue>
Result<string> denied    = Error.Unauthorized("Auth.Denied", "Invalid credentials."); // Error → Result<TValue>

// 4. Result<TValue> → Result widening (discards typed value, preserves outcome)
Result<string> typed  = Result.Success("order-123");
Result widened        = typed;  // implicit — equivalent to typed.DiscardValue()
```

### Domain Error Factory Quick Reference

```csharp
// Semantic factories — each pre-sets the correct ErrorType + ErrorSeverity combination
Error.Validation("Order.Invalid",    "Order is invalid.");              // Validation  / Warning
Error.NotFound("User.Missing",       "User was not found.");            // NotFound    / Warning
Error.NotFound<Guid>("Order", id)    // strongly-typed: "Order.NotFound", "Order with Id '...' was not found."
Error.Conflict("Email.Duplicate",    "Email already registered.");      // Conflict    / Warning
Error.Unauthorized("Auth.Expired",   "Token expired.");                 // Unauthorized / Error
Error.Forbidden("Policy.Denied",     "Access denied.");                 // Forbidden   / Error
Error.Unexpected("System.Crash",     "Unhandled exception occurred.");  // Unexpected  / Critical
Error.Unavailable("Cache.Down",      "Cache layer is unavailable.");    // Unavailable / Transient
Error.Infrastructure("Db.Offline",   "Database is offline.");           // Infrastructure / Transient
Error.Domain("Rule.Violated",        "Domain invariant violated.");     // Domain      / Error
Error.Business("Order.CannotShip",   "Order has not been confirmed.");  // Domain alias (same as Error.Domain)

// Sentinel instances — allocation-free constants
Error noError  = Error.None;      // Represents "no error" in optional error fields
Error nullErr  = Error.NullValue; // Null-guard sentinel

// Error.Message is an alias for Error.Description (exception-pattern compatibility)
string msg = Error.NotFound("X", "X was not found.").Message; // same as .Description
```

---

## 4. Safe Result Consumption & Value Unwrapping

`EricksonLopez.Result` prohibits blind access to `.Value` to prevent unhandled runtime exceptions. It provides multiple safe, declarative unwrapping mechanisms:

### Approach 1: Exhaustive Pattern Matching with `.Match()`
```csharp
string display = greeting.Match(
    onSuccess: val => $"Success: {val}",
    onFailure: err => $"Error [{err.Code}]: {err.Description}"
);
```

### Approach 2: Standard BCL Idiom with `.TryGetValue()`
```csharp
if (greeting.TryGetValue(out string? message))
{
    Console.WriteLine($"Found value: {message}");
}
else
{
    Console.WriteLine($"Operation failed with code: {greeting.Error.Code}");
}
```

### Approach 3: Safe Fallback with `.GetValueOrDefault()`
```csharp
// Never throws, even if the result represents a failure
string finalValue = greeting.GetValueOrDefault("Default Fallback Value");
```

### Approach 4: C# Tuple Deconstruction (`Deconstruct`)
```csharp
var (isSuccess, value, error) = greeting;
if (isSuccess)
{
    Console.WriteLine($"Deconstructed: {value}");
}
else
{
    Console.WriteLine($"Failed: {error?.Description}");
}
```

---

## Next Level
Proceed to **[Level 02 — Complete Configuration](level-02-complete-configuration.md)** to master the domain error taxonomy, `ErrorBuilder`, severity levels, retryability classifications, and metadata enrichment.
