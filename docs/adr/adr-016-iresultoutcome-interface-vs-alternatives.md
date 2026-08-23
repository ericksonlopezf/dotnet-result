# ADR-016: IResultOutcome as a Managed Interface on a Value-Type Struct

- **Status**: Accepted
- **Date**: 2026-08-01
- **Authors**: Erickson Lopez
- **Supersedes**: (none — this is the first explicit ADR for this decision)
- **Related**: ADR-001 (readonly struct), ADR-004 (ASP.NET Core integration)

---

## Context

`Result` and `Result<T>` are implemented as `readonly struct` value types (ADR-001). A common
pattern in ASP.NET Core Minimal APIs is to register an endpoint filter that automatically
unwraps `Result`/`Result<T>` return values into HTTP responses, eliminating boilerplate from
each handler. This requires the filter to detect a `Result`-typed return value at runtime
without knowing the concrete `T` at filter registration time.

Two broad strategies were evaluated for this runtime polymorphism:

1. **Managed Interface (`IResultOutcome`)** — `Result` and `Result<T>` implement a shared interface
   that the filter matches via `is IResultOutcome`.
2. **Duck-typing / Source-generated switch** — The filter uses `switch(result.GetType())` or a
   Roslyn source-generator-aware registration that produces a typed `Func<object?, IResult>` for
   each concrete `Result<T>` at startup.

---

## Decision

`Result` and `Result<T>` implement `IResultOutcome`. This interface is the public polymorphism
boundary used by:
- `ResultEndpointFilter` — to detect and unwrap `Result`/`Result<T>` at the HTTP boundary.
- Any user middleware, `IPipelineBehavior`, or interceptor that needs to observe result state
  without knowing `T` at compile time.

---

## Rationale

### Why Interface over Duck-Typing

**Duck-typing / `switch(result.GetType())`** requires either:
- A `typeof`-keyed dictionary populated at app startup for every `Result<T>` variant the app uses,
  which requires explicit registration per type, breaking the "just return `Result<T>`" ergonomics.
- `is` checks against an open generic (`is Result<>`) which is not supported in C# pattern matching.
- Reflection-based detection which is incompatible with NativeAOT goals.

**Managed Interface** requires zero registration, works at runtime without reflection, and is
fully compatible with NativeAOT (interface dispatch is a vtable lookup, not a reflection operation).

### Why Not Generic Constraints

A generic constraint approach (`where TResult : IResultOutcome`) would require the filter to be
generic at the ASP.NET Core framework integration point, which is not supported by
`IEndpointFilter.InvokeAsync` (it returns `ValueTask<object?>`).

### Accepted Boxing Tradeoff

Implementing `IResultOutcome` on a `readonly struct` causes boxing when the struct is assigned to
an interface variable. This occurs in `ResultEndpointFilter`:

```csharp
if (result is IResultOutcome outcome) // boxes Result<T> struct to heap
{
    // outcome.RawValue boxes T if T is a value type (second allocation on success path)
    return TypedResults.Ok(outcome.RawValue);
}
```

**Per-request cost**: 1-2 heap allocations (boxing of `Result<T>` + boxing of `T` on the success
path if `T` is a value type). For reference types `T` (e.g., `MyDto`), only one boxing occurs.

At high throughput (>50k req/s) where GC pressure matters, the boxing allocation from the
endpoint filter is measurable. In that scenario, the recommended pattern is to call
`ToHttpResult<T>()` directly from the handler:

```csharp
// Zero boxing — returns typed Ok<T> with full OpenAPI metadata
app.MapGet("/orders/{id}", async (int id, IOrderService svc) =>
{
    var result = await svc.GetOrderAsync(id);
    return result.ToHttpResult(_options);  // No boxing. No filter needed.
})
.Produces<OrderDto>(StatusCodes.Status200OK)
.ProducesProblem(StatusCodes.Status404NotFound);
```

---

## Consequences

### Positive

- **Zero registration overhead**: Any endpoint returning `Result<T>` is automatically handled
  by `AddResultEndpointFilter()` without per-type registration.
- **NativeAOT compatible**: Interface dispatch is a vtable call — no reflection, no
  `MakeGenericMethod`, no `Activator.CreateInstance`. The `IsAotCompatible=true` declaration
  is truthful.
- **Polymorphic observability**: Middleware, pipeline behaviors, and custom filters can check
  `is IResultOutcome` to observe any Result-typed return without knowing `T`.
- **`IsUninitialized` is exposed**: `IResultOutcome.IsUninitialized` lets the filter detect
  and reject `default(Result<T>)` with an explicit `InvalidOperationException` rather than
  producing a silent 200 OK.

### Negative / Trade-Offs

- **Per-request boxing in `ResultEndpointFilter`**: Every request processed by the filter
  allocates 1-2 managed objects (boxing of the struct + boxing of `T` if `T` is a value type).
  This is documented in the filter's XML docs and the performance guide.
- **`RawValue` returns `object?`**: The covariant success value is accessed as `object?`,
  boxing value types. This is inherent to the interface design.
- **OpenAPI schema degradation**: The filter returns `Ok<object?>` (not `Ok<T>`), which means
  Swagger/OpenAPI shows `object` instead of the concrete `T`. Mitigated by `RESULT008`
  (`EndpointFilterOpenApiAnalyzer`) and the recommendation to add `.Produces<T>()` declarations.

---

## Performance Guidance (Canonical Reference)

| Scenario | Recommended API | Boxing | OpenAPI |
|----------|----------------|--------|---------|
| General use (<10k req/s) | `AddResultEndpointFilter()` | 1-2 allocs/req | Needs `.Produces<T>()` |
| High throughput (>10k req/s) | `.ToHttpResult<T>()` in handler | 0 allocs | Full inference |
| Non-generic `Result` | Either | 0 extra allocs | N/A |

---

## Evolution Path (v2.0 Considerations)

C# and the .NET runtime may provide mechanisms in future versions that enable zero-allocation
polymorphism on value types (e.g., static abstract interface members, `ref` struct interfaces
in C# 13+). If such mechanisms become practical at the ASP.NET Core boundary, `IResultOutcome`
could be replaced with a source-generated dispatch table or static interface dispatch,
eliminating the boxing cost without breaking the `AddResultEndpointFilter()` ergonomics.

Any such change would be a **breaking change** for consumers who have implemented
`IResultOutcome` in their own types. It will be gated behind a major version bump (v2.0) with
a migration guide.

## Related

- ADR-001 — `readonly struct` Result implementation
- ADR-004 — ASP.NET Core ProblemDetails integration
- ADR-015 — Audit findings resolution (boxing tradeoff documented as accepted)
- `ResultEndpointFilter.cs` — boxing commentary at lines 87-104
