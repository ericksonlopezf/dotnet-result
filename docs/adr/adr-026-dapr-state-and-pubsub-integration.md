# ADR-026: Dapr Distributed State & PubSub Integration

## Status
Accepted

## Date
2026-09-08

## Context
Dapr (Distributed Application Runtime) is a cloud-native distributed application runtime widely used for microservice state management, service invocation, and event-driven pub/sub messaging. When interacting with Dapr state stores, operations can encounter missing entries, sidecar communication timeouts, and optimistic concurrency conflicts governed by ETags. In pub/sub subscriptions, message disposition must be signaled to the Dapr runtime via HTTP status codes (`ACK`, `RETRY`, `DROP`). Traditional exception throwing leads to thread contention, unhandled drops, and loss of domain error context.

## Decision
We created `EricksonLopez.Result.Dapr`:
1. **State Store Result Extensions**:
   - `GetStateWithResultAsync<T>`: Retrieves state entries, converting missing keys to `Error.NotFound(DaprErrorCodes.StateNotFound, ...)` without throwing.
   - `SaveStateWithResultAsync<T>`: Saves state with ETag optimistic concurrency enforcement, capturing `DaprException` and mapping ETag mismatches to `Error.Conflict(DaprErrorCodes.EtagMismatch, ...)`.
   - `ExecuteStateTransactionWithResultAsync` and `GetBulkStateWithResultAsync`: Wrap transactional and batch operations in structured `Result` envelopes.
2. **Pub/Sub Topic Result Mapping**:
   - `ToDaprTopicResult(this Result)` and `ToDaprTopicResult<T>(this Result<T>)`: Translates `Result` outcomes into HTTP `IResult` responses matching Dapr runtime semantics:
     - `IsSuccess == true`: HTTP `200 OK` (`ACK`, message committed).
     - `IsFailure == true` and `Retryability == Transient`: HTTP `503 Service Unavailable` (`RETRY`, triggers backoff redelivery).
     - `IsFailure == true` and `Retryability == Permanent`: HTTP `422 Unprocessable Entity` with `DaprErrorResponse("DROP", ...)` (`DROP`, moves message to dead-letter queue).
3. **Canonical Error Codes**: Standardized constants in `DaprErrorCodes` (`StateNotFound`, `EtagMismatch`, `SidecarUnavailable`, `OperationFailed`).

## Consequences
### Positive
- Idiomatic integration with cloud-native microservices orchestrated by Dapr.
- Eliminates exception control flow in high-throughput state storage and event processing.
- Preserves error retryability semantics, enabling automated Dapr broker redelivery without custom boilerplate.

### Negative / Trade-offs
- Introduces package dependency on `Dapr.Client` and `Dapr.AspNetCore`.
