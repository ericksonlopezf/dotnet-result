# ErrorBuilder and Error Mutation Reference

This document describes best practices, performance guidelines, and diagnostic analyzers related to `ErrorBuilder` and `Error` construction in `EricksonLopez.Result`.

---

## Overview of `ErrorBuilder`

`ErrorBuilder` is a high-performance, stack-allocated `readonly struct` designed for constructing compound or heavily-annotated `Error` instances in a single allocation pass.

Because `ErrorBuilder` is an **immutable `readonly struct`**, every mutation method (such as `.WithMetadata()`, `.WithInnerError()`, `.WithTraceId()`) returns a **new copy** of the builder rather than modifying the instance in place.

---

## Diagnostic Rules

### `RESULT003` — `ErrorBuilder` Method Return Value is Discarded

#### Cause
A method on `ErrorBuilder` (`WithMetadata`, `WithInnerError`, `WithTraceId`, `WithCorrelationId`, `WithSeverity`, `WithRetryability`, `WithDescriptionKey`) was called without assigning or chaining the returned `ErrorBuilder` value.

#### Severity
🛑 **`Error`** (Compile-time failure)

#### Rationale
Because `ErrorBuilder` is a value type (`readonly struct`), calling a mutation method without assigning its return value discards the modified struct. The original variable remains completely unmodified, leading to silent data loss bugs.

#### How to Fix
Chain the calls into a single fluent expression ending in `.Build()`, or reassign the variable:

```csharp
// ❌ WRONG (Triggers RESULT003 — mutations are discarded!):
var builder = ErrorBuilder.Validation("User.Invalid", "Validation failed");
builder.WithInnerError(Error.Validation("User.Name", "Name is required"));
builder.WithMetadata("Field", "Name");
return builder.Build(); // The built Error will have NO inner errors and NO metadata!

// ✅ CORRECT (Fluent method chaining):
return ErrorBuilder.Validation("User.Invalid", "Validation failed")
    .WithInnerError(Error.Validation("User.Name", "Name is required"))
    .WithMetadata("Field", "Name")
    .Build();

// ✅ CORRECT (Reassignment):
var builder = ErrorBuilder.Validation("User.Invalid", "Validation failed");
builder = builder.WithInnerError(Error.Validation("User.Name", "Name is required"));
builder = builder.WithMetadata("Field", "Name");
return builder.Build();
```

---

### `RESULT005` — Avoid Chaining `Error.WithMetadata()` Calls

#### Cause
`Error.WithMetadata()` or `ErrorBuilder.WithMetadata()` is called 3 or more times consecutively on the same instance.

#### Severity
⚠️ **`Warning`**

#### Rationale
`Error` is an immutable class. Each call to `.WithMetadata(key, value)` allocates a new `Error` object and an underlying copy of the metadata dictionary. Chaining $N$ metadata calls causes $N$ intermediate heap allocations.

#### How to Fix
Use `ErrorBuilder` or pass a single dictionary to the batch overload:

```csharp
// ❌ WRONG (Triggers RESULT005 — 3 intermediate Error heap allocations):
var error = Error.Validation("Order.Invalid", "Invalid order")
    .WithMetadata("OrderId", orderId)
    .WithMetadata("UserId", userId)
    .WithMetadata("Attempt", attempt);

// ✅ CORRECT (Single allocation via dictionary batch):
var metadata = new Dictionary<string, object?>
{
    ["OrderId"] = orderId,
    ["UserId"] = userId,
    ["Attempt"] = attempt
};
var error = Error.Validation("Order.Invalid", "Invalid order")
    .WithMetadata(metadata);

// ✅ CORRECT (Single allocation via ErrorBuilder):
var error = ErrorBuilder.Validation("Order.Invalid", "Invalid order")
    .WithMetadata("OrderId", orderId)
    .WithMetadata("UserId", userId)
    .WithMetadata("Attempt", attempt)
    .Build();
```

---

### `RESULT006` — Chained `ErrorBuilder.WithInnerError()` Calls are $O(n^2)$

#### Cause
`ErrorBuilder.WithInnerError()` is called 2 or more times consecutively.

#### Severity
⚠️ **`Warning`**

#### Rationale
Each invocation of `ErrorBuilder.WithInnerError(singleError)` re-allocates and copies the internal array of inner errors. For $N$ sequential calls, this results in $O(n^2)$ copying overhead.

#### How to Fix
Use the batch overload `ErrorBuilder.WithInnerErrors(IEnumerable<Error>)` or `ErrorBuilder.WithInnerErrors(ReadOnlySpan<Error>)`:

```csharp
// ❌ WRONG (Triggers RESULT006 — repeated array resize allocations):
var error = ErrorBuilder.Validation("Form.Invalid", "Form validation failed")
    .WithInnerError(e1)
    .WithInnerError(e2)
    .WithInnerError(e3)
    .Build();

// ✅ CORRECT (Single batch allocation):
var error = ErrorBuilder.Validation("Form.Invalid", "Form validation failed")
    .WithInnerErrors([e1, e2, e3])
    .Build();
```
