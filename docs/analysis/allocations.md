# Allocation & Memory Footprint Analysis

This document details the memory layout, heap allocation guarantees, and zero-allocation techniques implemented in `EricksonLopez.Result`.

---

## 1. Struct-Based Envelope Layout

`Result` and `Result<TValue>` are defined as **`readonly struct`** value types. Value types reside on the stack or inline inside containing data structures (or directly inside CPU registers during JIT compilation), eliminating heap allocations for the Result envelope.

```
+-------------------------------------------------------------+
| Result<TValue> Struct Memory Layout (Stack / Register)      |
+--------------------------+----------------------------------+
| Field                    | Type / Size                      |
+--------------------------+----------------------------------+
| _state                   | ResultState enum (byte/int)      |
| _value                   | TValue (TValue size)             |
| _error                   | Error reference (IntPtr - 8B)    |
+--------------------------+----------------------------------+
```

### Memory Allocation Matrix

| Execution Scenario | Result Struct Allocation | Heap Allocation |
|---|---|---|
| `Result.Success()` | **0 bytes** | **0 bytes** |
| `Result.Success(42)` (Value Type `int`) | **0 bytes** | **0 bytes** |
| `Result.Success(user)` (Reference Type `User`) | **0 bytes** | 0 bytes extra (User instance created separately) |
| `Result.Failure(error)` | **0 bytes** | `Error` class instance allocated on heap |

---

## 2. Closure Elimination via the `TState` Pattern

In standard C# delegates, passing a lambda that accesses variables outside its parameter list forces the compiler to generate a display class instance on the heap:

```csharp
// ❌ CONVENTIONAL LAMBDA (Closure Allocation)
var limit = 100;
result.Ensure(val => val < limit, error);
```

**Decompiled C# Compiler Output for Conventional Lambda:**
```csharp
class <>c__DisplayClass0_0
{
    public int limit;
}
var display = new <>c__DisplayClass0_0(); // 🚨 Heap Allocation!
display.limit = 100;
result.Ensure(new Func<int, bool>(display.<Method>b__0), error);
```

### `TState` Closure-Free Pipeline

All monadic operators in `EricksonLopez.Result` supply a `TState` overload:

```csharp
// ✅ TState ZERO-ALLOCATION LAMBDA
var limit = 100;
result.Ensure(limit, static (l, val) => val < l, error);
```

Because `state` is passed directly as a method argument and the lambda is marked `static`, no display class is instantiated. The JIT compiler optimizes the static method call directly.

---

## 3. Memory Optimization in `Result.Combine`

When combining multiple `Result` instances, naive implementations allocate temporary arrays (e.g. `params Result[]`).

`EricksonLopez.Result` optimizes `Result.Combine` using **`ReadOnlySpan<Result>`** and **`ArrayPool<Error>`**:

```csharp
public static Result Combine(params ReadOnlySpan<Result> results)
{
    // Rent array from ArrayPool to avoid heap allocation for temporary collections
    Error[]? rented = null;
    int errorCount = 0;

    for (int i = 0; i < results.Length; i++)
    {
        if (results[i].IsFailure)
        {
            rented ??= ArrayPool<Error>.Shared.Rent(results.Length);
            rented[errorCount++] = results[i].Error;
        }
    }

    if (errorCount == 0) return Success();

    // Create aggregated validation error and return rented array to pool
    var errors = rented.AsSpan(0, errorCount).ToArray();
    ArrayPool<Error>.Shared.Return(rented!);
    
    return Error.Validation("Result.CombineFailed", "One or more operations failed.", errors);
}
```

---

## 4. Lazy `TraceId` Stringification

Every `Error` instance captures the ambient OpenTelemetry trace ID when created within an active `Activity`.

Standard implementations call `Activity.Current.TraceId.ToString()` immediately, allocating a 32-character `string` on the heap even if the `TraceId` is never inspected.

`EricksonLopez.Result` stores the raw 32-byte `ActivityTraceId` struct on the stack/class layout and materializes the string lazily:

```csharp
private readonly ActivityTraceId? _traceIdValue; // 32-byte struct
private readonly string? _traceIdOverride;

// String stringified ONLY when accessed
public string? TraceId => _traceIdOverride ?? (_traceIdValue.HasValue ? _traceIdValue.Value.ToString() : null);
```

This saves **32 string bytes + heap header overhead** for every domain error created in traced applications!
