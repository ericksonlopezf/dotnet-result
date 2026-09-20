# ADR-023: Entity Framework Core Result Adapter

## Status
Accepted

## Date
2026-09-08

## Context
Applications using Entity Framework Core frequently encounter persistence exceptions (`DbUpdateConcurrencyException`, `DbUpdateException`, timeout exceptions) that disrupt monadic railway-oriented pipelines. Translating these exceptions manually into domain `Result` or `Result<T>` envelopes across repositories introduces significant boilerplate.

## Decision
We created `EricksonLopez.Result.EntityFrameworkCore` as a dedicated companion package:
1. `SaveChangesAsyncToResult`: Executes `SaveChangesAsync` and traps `DbUpdateConcurrencyException` as `Error.Conflict`, `DbUpdateException` as `Error.Failure`, and `TimeoutException` as `Error.Unavailable` with `ErrorRetryability.Transient`. `OperationCanceledException` is preserved and rethrown.
2. Query extensions on `IQueryable<T>`: `FirstOrDefaultToResultAsync`, `SingleOrDefaultToResultAsync`, and `ToListToResultAsync` returning clean `Result<T>` and `Result<List<T>>` envelopes without throwing on missing records.
3. The core `EricksonLopez.Result` package remains completely decoupled with zero EF Core dependencies.

## Consequences
### Positive
- Standardized, exception-safe persistence pipeline operations.
- Clean integration with Railway-Oriented Programming (ROP) in Clean Architecture repositories.
- Fully compatible with Native AOT when using compiled models.

### Negative / Trade-offs
- Adds a dependency on `Microsoft.EntityFrameworkCore` for applications referencing this package.
