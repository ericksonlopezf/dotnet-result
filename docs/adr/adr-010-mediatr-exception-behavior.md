# ADR-010: MediatR Exception Behavior Pipeline

- **Status**: Accepted
- **Date**: 2026-07-28
- **Authors**: Erickson Lopez

---

## Context

In CQRS architectures using MediatR, unhandled exceptions in request handlers propagate as raw exceptions, forcing consumers to wrap every `Send()` call in try-catch blocks. For handlers returning `Result` or `Result<T>`, exceptions should be captured and converted to `Result.Failure` to maintain the Result pattern contract throughout the application.

## Decision

We created `EricksonLopez.Result.MediatR` providing:

1. `ResultExceptionBehavior<TRequest, TResponse>` — A sealed `IPipelineBehavior` that catches unhandled exceptions and converts them to `Result.Failure` using `ErrorType.Unexpected`.
2. `AddResultExceptionBehavior()` — A DI extension method for registration with an optional custom `Func<Exception, Error>` factory.
3. Automatic type checking — the behavior only activates when `TResponse` is `Result` or `Result<T>`. Non-Result responses pass through unmodified.
4. `OperationCanceledException` is always re-thrown to preserve cooperative cancellation semantics.

## Consequences

### Positive
- Eliminates the need for try-catch blocks in every MediatR handler returning `Result`.
- Custom error factories allow domain-specific exception mapping.
- Pass-through behavior for non-Result responses ensures backward compatibility.

### Negative / Trade-Offs
- `CreateFailure` uses `MakeGenericMethod` for `Result<T>` construction, making the package **not compatible with NativeAOT**.
- MediatR itself uses reflection internally, so AOT incompatibility is inherited.
- Introduces a dependency on `MediatR 12.4.1`.
