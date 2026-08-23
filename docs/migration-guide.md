# Migration Guide

This guide assists developers in migrating existing applications to `EricksonLopez.Result` from traditional exception-driven logic or alternative Result pattern libraries.

---

## 1. Migrating from Traditional Exceptions

### Before (Exceptions for Business Control Flow)

```csharp
public User GetUser(Guid id)
{
    var user = _repository.Find(id);
    if (user == null)
        throw new NotFoundException($"User {id} not found."); // ❌ High allocation & stack trace overhead
    
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

### FluentResults → `EricksonLopez.Result`

| Feature | FluentResults | `EricksonLopez.Result` |
|---|---|---|
| Struct vs Class | Class (`Result` is heap allocated) | `readonly struct` (Zero heap envelope) |
| Monadic Operators | `ToResult()`, `Bind()`, `Map()` | `Map`, `Bind`, `TapOnSuccess`, `TapOnFailure`, `Ensure`, `Recover`, `Match` |
| Zero-Allocation Closures | Not supported | Supported via `TState` overloads |
| ProblemDetails Mapping | Custom extension required | Built-in `ToHttpResult()` & `ResultEndpointFilter` |
| OpenTelemetry | Manual Activity instrumentation | Built-in `TraceOutcome()` & `ResultMetrics` |

```csharp
// FluentResults:
// Result<User> result = Result.Fail<User>("User not found");

// EricksonLopez.Result:
Result<User> result = Error.NotFound("User.NotFound", "User not found");
```

---

### CSharpFunctionalExtensions → `EricksonLopez.Result`

```csharp
// CSharpFunctionalExtensions:
// Result<User> result = Result.Failure<User>("User not found");

// EricksonLopez.Result:
Result<User> result = Error.NotFound("User.NotFound", "User not found");
```

---

### ErrorOr / OneOf → `EricksonLopez.Result`

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
| CSharpFunctionalExtensions | `result.OnSuccess(val => ...)` | `result.TapOnSuccess(val => ...)` |
| ErrorOr | `result.Match(val => ..., errors => ...)` | `result.Match(val => ..., error => ...)` |
| LINQ Monadic Bind | `from a in resA from b in resB select ...` | `from a in resA from b in resB select ...` (Identical syntax supported) |
| Option Type | `Option<T>` | `Maybe<T>` (`EricksonLopez.Result.Maybe`) |
| Strongly Typed Error | `Result<TValue, TError>` | `Result<TValue, TError>` (`EricksonLopez.Result.Generic`) |
