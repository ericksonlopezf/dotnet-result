# ADR-004: Standardized HTTP ProblemDetails Mapping (RFC 9457)

- **Status**: Accepted
- **Date**: 2026-07-28
- **Authors**: Erickson Lopez

---

## Context

ASP.NET Core Web APIs need a standardized, compliant mechanism for representing domain errors in HTTP responses according to the RFC 9457 ProblemDetails standard.

## Decision

We created `EricksonLopez.Result.AspNetCore`, introducing `ToHttpResult()` extension methods and `ResultEndpointFilter` for ASP.NET Core Minimal APIs.

## Consequences

### Positive
- Automatic RFC 9457 ProblemDetails payload generation on failure.
- Deterministic HTTP status mapping (`Validation` -> 400, `Unauthorized` -> 401, `Forbidden` -> 403, `NotFound` -> 404, `Conflict` -> 409, `Unavailable` -> 503, `Failure` -> 500).
- Minimal API filter `ResultEndpointFilter` unwraps `Result` and `Result<T>` automatically.

### Negative / Trade-Offs
- Web API endpoints must reference `EricksonLopez.Result.AspNetCore` to access HTTP mapping primitives.
