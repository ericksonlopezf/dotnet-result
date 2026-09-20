# ADR-024: Polly v8 Resilience Pipeline Integration

## Status
Accepted

## Date
2026-09-08

## Context
Polly v8 introduced the high-performance, allocation-conscious `ResiliencePipeline` architecture. Traditional resilience patterns rely on catching exceptions to trigger retries and circuit breakers. In the Result pattern, errors are values rather than exceptions. Forcing callers to throw exceptions to trigger Polly retries degrades throughput by orders of magnitude.

## Decision
We created `EricksonLopez.Result.Polly` integrating directly with `Polly.Core` v8:
1. `AddResultRetry`: Configures `RetryStrategyOptions` with a predicate evaluating `result.IsFailure && result.Error.Retryability == ErrorRetryability.Transient`.
2. `ExecuteResult` and `ExecuteResultAsync`: Provide execution overloads directly on `ResiliencePipeline` and `ResiliencePipeline<Result<T>>`, including `TState` overloads to eliminate closure allocations.
3. Zero exceptions are thrown during retry evaluation, maintaining high throughput and full Native AOT compatibility.

## Consequences
### Positive
- Exception-free retry execution matching the core performance philosophy.
- Native AOT certified out-of-the-box via `Polly.Core`.
- Seamless alignment with the `ErrorRetryability` taxonomy.

### Negative / Trade-offs
- References `Polly.Core` (v8.x).
