# Performance Optimization Guide

This document explains the zero-allocation architecture, memory mechanics, and performance diagnostic analyzers in `EricksonLopez.Result`.

---

## 1. Zero-Allocation Happy Path

In high-throughput microservices (10,000+ QPS), object allocations trigger frequent Garbage Collection (GC) pauses and CPU cache thrashing. `EricksonLopez.Result` enforces zero heap allocations on successful domain execution:

1. **`readonly struct` Envelopes**: Both `Result` and `Result<TValue>` reside on the execution stack or directly inside CPU registers during JIT compilation.
2. **`ReadOnlySpan<T>` and `ArrayPool<T>`**: Combinator methods (`Result.Combine`, `Result.ValidateAll`) rent buffers from `ArrayPool<Error>.Shared`, avoiding heap allocations when aggregating errors.
3. **Lazy `TraceId` Resolution**: Captures `System.Diagnostics.ActivityTraceId` as a raw value type without allocating strings until `.TraceId` is read.

---

## 2. Diagnostic Rules

### `RESULT001` — `Result<T>` Value Type is Excessively Large

#### Cause
A generic `Result<T>` is instantiated where `T` is a value type (`struct`) that causes the total `Result<T>` struct size to exceed the **32-byte threshold**.

#### Severity
⚠️ **`Warning`**

#### Rationale
`Result<T>` is a `readonly struct` that is passed by value across monadic pipeline stages (`Map`, `Bind`, `Ensure`, `Match`). While small structs (such as `int`, `Guid`, `decimal`, `DateTimeOffset` — up to 16 bytes) fit comfortably in CPU registers, large structs require copying dozens of bytes in memory on every method call, causing significant CPU memory-bus overhead in hot paths.

#### How to Fix
Wrap large struct data in a `class` or `record` (reference type):

```csharp
// ❌ WRONG (Triggers RESULT001 — Large struct causes heavy pass-by-value copying):
public struct LargePayload // 64+ bytes
{
    public decimal A, B, C, D;
}
public Result<LargePayload> Process() => ...

// ✅ CORRECT (Reference type allocated once, passed by 8-byte reference):
public sealed record LargePayload(decimal A, decimal B, decimal C, decimal D);
public Result<LargePayload> Process() => ...
```

---

### `RESULT004` — Lambda Captures Locals in Result Pipeline (Closure Allocation)

#### Cause
A lambda expression passed to a Result monadic operator (`Map`, `Bind`, `Ensure`, `TapOnSuccess`, `TapOnFailure`, `Recover`, `Inspect`) captures local variables or parameters from its enclosing scope.

#### Severity
⚠️ **`Warning`**

#### Rationale
When a lambda captures an outer variable, the C# compiler generates a hidden display class (`<>c__DisplayClass`) on the GC heap for every execution. In high-throughput hot paths, this defeats the zero-allocation guarantees of the Result struct.

#### How to Fix
Use the **`TState` overloads** with a `static` lambda. The external state is passed directly into the method argument, eliminating the compiler display class:

```csharp
// ❌ WRONG (Triggers RESULT004 — Allocates display class on every iteration):
Guid tenantId = userContext.TenantId;
var result = service.GetOrder(orderId)
    .Ensure(order => order.TenantId == tenantId, DomainErrors.Order.InvalidTenant);

// ✅ CORRECT (Zero GC allocation via TState overload and static lambda):
Guid tenantId = userContext.TenantId;
var result = service.GetOrder(orderId)
    .Ensure(
        state: tenantId,
        predicate: static (tId, order) => order.TenantId == tId,
        error: DomainErrors.Order.InvalidTenant);
```
