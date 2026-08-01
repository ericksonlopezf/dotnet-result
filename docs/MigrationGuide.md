# Migration Guide

This guide assists developers in migrating existing applications to `EricksonLopez.Result` from traditional exception-driven logic or alternative Result pattern libraries.

---

## 1. Migrating from Traditional Exceptions

### Before (Exceptions for Control Flow)

```csharp
public User GetUser(Guid id)
{
    var user = _repository.Find(id);
    if (user == null)
        throw new NotFoundException($"User {id} not found."); // ❌ High allocation & stack trace capture overhead
    
    if (!user.IsActive)
        throw new BusinessRuleException("User is not active.");

    return user;
}
```

### After (`EricksonLopez.Result`)

```csharp
public Result<User> GetUser(Guid id)
{
    var user = _repository.Find(id);
    if (user == null)
        return Error.NotFound("User.NotFound", $"User {id} not found."); // ✅ Zero-allocation struct envelope
    
    if (!user.IsActive)
        return Error.Forbidden("User.Inactive", "User is not active.");

    return user; // Implicit conversion to Result<User>.Success(user)
}
```

---

## 2. Migrating from Alternative Libraries

### FluentResults -> `EricksonLopez.Result`

| Feature | FluentResults | `EricksonLopez.Result` |
|---|---|---|
| Struct vs Class | Class (`Result` is heap allocated) | `readonly struct` (Zero heap envelope) |
| Monadic Operators | `ToResult()`, `Bind()`, `Map()` | `Map`, `Bind`, `Tap`, `Ensure`, `Recover`, `Match` |
| Zero-Allocation Closure | Not supported | Supported via `TState` overloads |
| ProblemDetails Mapping | Manual / Extension | Native `ToHttpResult()` & `ResultEndpointFilter` |

```csharp
// FluentResults:
// Result<User> result = Result.Fail<User>("User not found");

// EricksonLopez.Result:
Result<User> result = Error.NotFound("User.NotFound", "User not found");
```

---

### CSharpFunctionalExtensions -> `EricksonLopez.Result`

```csharp
// CSharpFunctionalExtensions:
// Result<User> result = Result.Failure<User>("User not found");

// EricksonLopez.Result:
Result<User> result = Error.NotFound("User.NotFound", "User not found");
```

---

### OneOf / ErrorOr -> `EricksonLopez.Result`

```csharp
// ErrorOr:
// ErrorOr<User> result = Error.NotFound("User.NotFound", "User not found");

// EricksonLopez.Result:
Result<User> result = Error.NotFound("User.NotFound", "User not found");
```

---

## 3. Key API Mappings Summary

| Paradigm / Library | Legacy Expression | `EricksonLopez.Result` Equivalent |
|---|---|---|
| Raw Exception | `throw new InvalidOperationException(msg)` | `return Error.Failure("Code", msg);` |
| FluentResults | `Result.Ok(val)` | `Result.Success(val)` |
| CSharpFunctionalExtensions | `result.OnSuccess(val => ...)` | `result.Tap(val => ...)` |
| ErrorOr | `result.Match(val => ..., errors => ...)` | `result.Match(val => ..., error => ...)` |

---

## 4. Notes for Internal Pre-Release Users

> [!NOTE]
> **v1.0.0 is the first public release** of `EricksonLopez.Result`. There are no prior published NuGet packages. The notes below document changes made during the internal development phase that may affect early adopters who used pre-release builds from source.

### Asynchronous Operations & CancellationToken
In the final v1.0.0 API, all asynchronous methods (`Map`, `Bind`, `Tap`, `Match`, `Ensure`, etc.) require or accept a `CancellationToken` for proper cooperative cancellation propagation.

```csharp
// Early pre-release:
await _userService.GetUserAsync(userId)
    .Bind(u => _orderService.CreateOrderAsync(u));

// v1.0.0:
await _userService.GetUserAsync(userId, cancellationToken)
    .Bind(u => _orderService.CreateOrderAsync(u, cancellationToken));
```

### Unified Testing Assertions
The `EricksonLopez.Result.Testing` package consolidates all assertion extensions into a single `ResultAssertions` class. The extensions natively support both `Task` and `ValueTask` return types, preventing test-runner deadlocks (e.g. Coverlet hanging on `ValueTask`).

Framework-specific packages are now available:
- `EricksonLopez.Result.Testing.XUnit` — assertion failures as `XunitException`
- `EricksonLopez.Result.Testing.NUnit` — assertion failures as `AssertionException`

```csharp
// using EricksonLopez.Result.Testing;
await resultTask.ShouldBeSuccessAsync();
```
