# Level 01 — Getting Started & Core Primitives

> **Ecosystem:** `EricksonLopez.Result` | **Audience:** Developers & Architects | **Language:** English

---

## 1. Package Installation

Install the core package via NuGet:

```bash
dotnet add package EricksonLopez.Result
```

---

## 2. Instantiating Results

### 2.1 Non-Generic `Result`
Used when an operation succeeds without returning a data payload (e.g., commands, state mutations):

```csharp
using EricksonLopez.Result;

// Success
Result okResult = Result.Success();

// Failure with Error factory
Result failResult = Result.Failure(Error.Validation(
    code: "User.InvalidEmail",
    description: "The provided email format is invalid."));
```

### 2.2 Generic `Result<TValue>`
Used when an operation produces a payload on success:

```csharp
// Explicit creation
Result<User> successUser = Result<User>.Success(new User("Erickson", "dev@ericksonlopez.dev"));
Result<User> failedUser = Result<User>.Failure(Error.NotFound(
    code: "User.NotFound",
    description: "The user with ID 42 does not exist."));

// Implicit conversion (clean & idiomatic)
Result<User> implicitSuccess = new User("Erickson", "dev@ericksonlopez.dev");
Result<User> implicitFailure = Error.Unauthorized("Auth.InvalidToken", "Token has expired.");
```

---

## 3. Invariant Checks with `Ensure`

Validate preconditions declaratively in pipelines:

```csharp
// Returns Success() if age >= 18; otherwise Failure(error)
Result validatedAge = Result.Success()
    .Ensure(() => age >= 18, Error.Validation("User.Underage", "User must be 18+"));

// Validate with Result<T>
Result<User> validatedUser = Result<User>.Success(user)
    .Ensure(u => !u.IsBlocked, Error.Forbidden("User.Blocked", "Account is blocked"));
```

---

## 4. Safe Value Consumption

### 4.1 Pattern Matching via `Match`
Forces explicit handling of both success and failure branches:

```csharp
string greeting = result.Match(
    onSuccess: user => $"Welcome back, {user.Name}!",
    onFailure: error => $"Error [{error.Code}]: {error.Description}");
```

### 4.2 Side-Effect Branching via `Execute`
Executes action blocks without returning values:

```csharp
result.Execute(
    onSuccess: user => Console.WriteLine($"Persisted user: {user.Id}"),
    onFailure: error => Console.Error.WriteLine($"Operation failed: {error.Description}"));
```

### 4.3 Safe Unwrapping with Defaults
```csharp
User fallbackUser = User.Guest;
User actualUser = result.GetValueOrDefault(fallbackUser);
```

---

## Next Steps
Proceed to [Level 02 — Domain Modeling & Rich Errors](level-02-domain-modeling-and-errors.md) to explore error taxonomy, severity, retryability, and lazy ambient trace correlation.
