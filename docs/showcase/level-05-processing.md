# Level 05 — Processing: Asynchronous Workflows, Concurrency & Cancellation

> **Ecosystem:** `EricksonLopez.Result` | **Audience:** Senior Engineers, High-Throughput Specialists | **Complexity:** Level 5 (Advanced) | **Code Reference:** `07_AsyncOperations.cs`, `15_AdvancedAsyncOperations.cs`, `20_WellKnownErrorsAndTry.cs` | **Language:** English

---

## 1. Exception Boundaries (`Result.Try` / `Result.TryAsync`)

When integrating with third-party libraries, cloud SDKs, or legacy drivers that throw unexpected exceptions, `Result.Try` and `Result.TryAsync` act as defensive fault isolation boundaries, translating exceptions into structured domain error results:

```csharp
// Synchronous invocation with state passing (0 closures allocated on GC heap)
Result<int> parsed = Result.Try(
    state: "12345",
    func: static s => int.Parse(s),
    errorHandler: static (s, ex) => Error.Validation("Parse.Failed", $"Could not parse '{s}': {ex.Message}")
);

// Asynchronous invocation with cooperative CancellationToken
Result<string> remoteData = await Result.TryAsync(
    action: async ct => await httpClient.GetStringAsync("https://api.example.com/data", ct),
    errorHandler: ex => Error.Unavailable("Http.Failed", ex.Message),
    cancellationToken: ct
);
```

---

## 2. Cooperative Cancellation with `CancellationToken`

All asynchronous monadic combinators (`Map`, `Bind`, `Ensure`, `ValidateAllAsync`, `TryAsync`) accept `CancellationToken`. When cancellation is triggered, standard `OperationCanceledException` propagates cooperatively according to .NET runtime conventions:

```csharp
public async Task<Result<ProcessedBatch>> ProcessBatchAsync(BatchRequest request, CancellationToken ct)
{
    return await FetchItemsAsync(request.Id, ct)
        .Bind(items => ValidateItemsAsync(items, ct), ct)
        .Bind(validItems => PersistItemsAsync(validItems, ct), ct)
        .TapOnSuccess(batch => _logger.LogInformation("Batch {Id} completed", batch.Id), ct);
}
```

---

## 3. High-Frequency I/O Optimization with `ValueTask`

In ultra-low-latency microservices (such as local memory caches or fast in-process lookups), the heap allocation of a `Task<Result<T>>` object introduces unnecessary GC overhead on synchronous completion paths.

`EricksonLopez.Result` provides first-class support for `ValueTask<Result>` and `ValueTask<Result<T>>`:

```csharp
public ValueTask<Result<CachedUser>> GetUserFromCacheAsync(Guid id)
{
    if (_memoryCache.TryGetValue(id, out CachedUser? user))
    {
        // 0 heap allocations: struct Result wrapped inside struct ValueTask
        return ValueTask.FromResult(Result.Success(user!));
    }

    return FetchFromDatabaseAsync(id);
}
```

---

## 4. Concurrent Batch Processing

To execute asynchronous operations concurrently and consolidate their outcomes:

```csharp
public async Task<Result<IReadOnlyList<ProcessedOrder>>> ProcessAllOrdersAsync(
    IEnumerable<Order> orders, 
    CancellationToken ct)
{
    var tasks = orders.Select(order => ProcessSingleOrderAsync(order, ct));
    Result<ProcessedOrder>[] results = await Task.WhenAll(tasks);

    // Combines all results: if any failed, returns an aggregated failure
    return Result.Combine(results);
}
```

---

## Next Level
Proceed to **[Level 06 — Error Handling](level-06-error-handling.md)** to master recovery workflows (`Recover`), distributed retries, dead-letter strategies, and error classification.
