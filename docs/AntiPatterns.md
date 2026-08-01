# Anti-Patterns

This document highlights common mistakes, unsafe patterns, and performance pitfalls to avoid when working with `EricksonLopez.Result`.

---

## 1. Accessing `.Value` Without Validating Success

**Anti-Pattern:** Attempting to access `.Value` directly without verifying `IsSuccess` or using `Match` / `TryGetValue`.

```csharp
// ❌ ANTI-PATTERN: Throws InvalidOperationException if GetUser returns a failure!
var result = userService.GetUser(id);
Console.WriteLine(result.Value.Name); 
```

**Solution:** Use `.Match()`, `.TryGetValue()`, or explicit checks.

```csharp
// ✅ CORRECT: Safe retrieval
if (result.TryGetValue(out var user))
{
    Console.WriteLine(user.Name);
}
```

---

## 2. Accessing `.Error` on a Successful Result

**Anti-Pattern:** Accessing `.Error` on a successful `Result` instance.

```csharp
// ❌ ANTI-PATTERN: Throws InvalidOperationException!
var result = Result.Success(42);
_logger.LogError(result.Error.Description);
```

**Solution:** Access `.Error` only inside a failure branch or via `.Match()`.

```csharp
// ✅ CORRECT
result.Switch(
    value => Console.WriteLine(value),
    error => _logger.LogError(error.Description)
);
```

---

## 3. Ignoring `default(Result)` Uninitialized Struct States

**Anti-Pattern:** Treating `default(Result)` or `default(Result<T>)` as a valid failure without checking `IsUninitialized`.

```csharp
// ❌ ANTI-PATTERN: default(Result<T>) has _state == ResultState.Uninitialized
Result<int> res = default;
if (res.IsFailure) // false!
{
    // Missed failure path because state is Uninitialized, not Failure!
}
```

**Solution:** Always initialize results using `Result.Success(...)` or `Result.Failure(...)`. If handling default structs, check `res.IsUninitialized`.

```csharp
// ✅ CORRECT
if (res.IsUninitialized)
{
    _logger.LogWarning("Uninitialized Result struct encountered.");
}
```

---

## 4. Overusing `Result.Try` for Non-Throwing Domain Code

**Anti-Pattern:** Wrapping basic, non-throwing business logic inside `Result.Try` "just to be safe".

```csharp
// ❌ ANTI-PATTERN: Unnecessary delegate and exception handling overhead
var result = Result.Try(
    () => user.CalculateDiscount(),
    ex => Error.Unexpected("Calc.Failed", ex.Message)
);
```

**Solution:** Reserve `Result.Try` exclusively for integration points with third-party libraries, legacy APIs, or IO operations that are known to throw exceptions.

---

## 5. Closure Capturing inside High-Throughput Pipelines

**Anti-Pattern:** Capturing variables inside lambda expressions in hot loops.

```csharp
// ❌ ANTI-PATTERN: Allocates a new closure object on every iteration!
foreach (var item in items)
{
    var threshold = item.Limit;
    result = result.Ensure(val => val > threshold, Error.Validation("Limit.Exceeded", "Over limit"));
}
```

**Solution:** Use the `TState` overload with `static` lambdas.

```csharp
// ✅ CORRECT: Zero closure allocation
foreach (var item in items)
{
    var threshold = item.Limit;
    result = result.Ensure(threshold, static (limit, val) => val > limit, Error.Validation("Limit.Exceeded", "Over limit"));
}
```

---

## 6. Fire-and-Forget Async Side Effects inside `.Tap`

**Anti-Pattern:** Invoking an async method inside a synchronous `.Tap` action without awaiting it.

```csharp
// ❌ ANTI-PATTERN: Async work is un-awaited fire-and-forget! Exceptions will be lost!
result.Tap(user => _emailService.SendWelcomeEmailAsync(user));
```

**Solution:** Use the async `Tap` overload and `await` the result pipeline.

```csharp
// ✅ CORRECT: Awaits the task cleanly
await result.Tap(async user => await _emailService.SendWelcomeEmailAsync(user));
```
