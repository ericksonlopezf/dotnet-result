# ADR-008: ValueTask Asynchronous State Machine Separation to Avoid Testing Deadlocks

## Status
Accepted

## Context
As the `EricksonLopez.Result` ecosystem grew to include `EricksonLopez.Result.Testing` and OpenTelemetry instrumentations in `EricksonLopez.Result.OpenTelemetry`, an attempt was made to achieve 100% test coverage using standard testing tools (xUnit + Coverlet). 

A critical bottleneck was encountered: attempting to execute code coverage instrumentation on standard `async ValueTask` or `async ValueTask<T>` methods in `Release` mode caused permanent deadlocks. Coverlet, while injecting coverage hooks into the compiler-generated `IAsyncStateMachine` for `ValueTask`, inadvertently locks the thread context on synchronous completion paths. This completely hung the xUnit test runner indefinitely. 

## Decision
Instead of abandoning `ValueTask` (which is critical for the library's zero-allocation goals) or excluding the testing/telemetry packages from coverage metrics, a structural design pattern was adopted: **The Async State-Machine Wrapper Pattern**.

Any method requiring `ValueTask` and asynchronous awaits must be split into two components:
1. **The Public Wrapper**: A standard, non-`async` method returning `ValueTask`. It immediately evaluates if the precursor Task is completed successfully. If so, it returns a new synchronous `ValueTask` (avoiding state machine generation entirely). If not, it falls back to the Slow Path.
2. **The Private Core (Slow Path)**: A private `async Task` (or `async Task<T>`) method that handles the `await`. 

```csharp
// Example Wrapper
public static ValueTask<Result> TraceOnSuccess(this ValueTask<Result> task, string operationName)
{
    if (task.IsCompletedSuccessfully)
    {
        return new ValueTask<Result>(task.Result.TraceOnSuccess(operationName));
    }
    
    // Defer to Task-based core which Coverlet can instrument without deadlocks
    return new ValueTask<Result>(TraceOnSuccessCore(task, operationName));
}

private static async Task<Result> TraceOnSuccessCore(ValueTask<Result> task, string op)
{
    var result = await task.ConfigureAwait(false);
    return result.TraceOnSuccess(op);
}
```

## Consequences
- **Positive:**
  - `ValueTask` deadlocks during test coverage runs are entirely eliminated because the actual `await` operations happen inside a `Task` context, which instrumentation tools handle safely.
  - The hot-path (synchronous completion) completely avoids allocating an `IAsyncStateMachine`, boosting performance in high-throughput environments.
  - Test coverage reached >95% without compromising the `Release` build structure.
- **Negative:**
  - Slightly more verbose code, requiring the explicit writing of two methods for every single `ValueTask` pipeline operator.
