# ADR-027: gRPC Server Interceptor & Status Mapping

## Status
Accepted

## Date
2026-09-08

## Context
gRPC is the primary binary RPC protocol for high-performance service-to-service communication in cloud and distributed architectures. In gRPC, failures are signaled across the wire via status codes (`StatusCode`) and optional metadata trailers (`Metadata`). When services adopt the Result pattern internally, handlers return `Result<T>` or custom response DTOs implementing `IResultOutcome`. Bridging these internal domain outcomes to the wire without repetitive try/catch and manual exception throwing requires an automated, non-invasive interceptor and bidirectional status mapper.

## Decision
We created `EricksonLopez.Result.Grpc`:
1. **Bidirectional Error-to-StatusCode Mapping (`GrpcErrorMapper`)**:
   - Maps each `ErrorType` classification to its canonical gRPC `StatusCode`:
     - `Validation` $\rightarrow$ `StatusCode.InvalidArgument`
     - `NotFound` $\rightarrow$ `StatusCode.NotFound`
     - `Conflict` $\rightarrow$ `StatusCode.AlreadyExists`
     - `Unauthorized` $\rightarrow$ `StatusCode.Unauthenticated`
     - `Forbidden` $\rightarrow$ `StatusCode.PermissionDenied`
     - `Unavailable` $\rightarrow$ `StatusCode.Unavailable`
     - `Infrastructure` $\rightarrow$ `StatusCode.DataLoss`
     - `Unexpected` / `Failure` $\rightarrow$ `StatusCode.Internal`
   - Encodes domain `Error` attributes into standard metadata trailers (`error-code`, `error-type`, `error-severity`, `error-retryability`, and `error-meta-*`).
   - Reverse reconstitutes `RpcException` instances into domain `Error` objects on client consumers (`rpcException.ToError()`).
2. **Server Unary Interceptor (`ResultServerInterceptor`)**:
   - Intercepts `UnaryServerHandler` invocations.
   - Inspects response objects implementing `IResultOutcome`.
   - If `outcome.IsFailure == true`, unwraps `outcome.Error` and throws `RpcException` populated with the appropriate status code and metadata trailers.
   - Intercepts unhandled CLR exceptions, converting them to `StatusCode.Internal` without crashing the service pipeline while allowing `RpcException` and `OperationCanceledException` to pass through transparently.
3. **Client Call Extensions (`GrpcResultExtensions`)**:
   - `ToResultAsync<T>(this AsyncUnaryCall<T>)` and `ToResultAsync<T>(this Task<T>)`: Safely awaits remote gRPC calls, wrapping successful responses in `Result<T>.Success(response)` and mapping caught `RpcException` instances to `Result<T>.Failure(rpcException.ToError())`.

## Consequences
### Positive
- Seamless, reflection-free bridging of domain `Error` instances across gRPC protocol boundaries.
- Clean server handler signatures without boilerplate error mapping.
- Preserves complete diagnostic telemetry across network hops via trailers.

### Negative / Trade-offs
- Requires `where TResponse : class` constraint on server handlers due to upstream gRPC Core API definitions.
