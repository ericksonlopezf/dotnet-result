# ADR-004: Standardized HTTP ProblemDetails Mapping (RFC 9457)

## Status
Accepted

## Date
2026-07-28

- **Status**: Accepted (Updated 2026-08-01 — full mapping table added)
- **Date**: 2026-07-28 (last updated: 2026-08-01)
- **Authors**: Erickson Lopez

---

## Context

ASP.NET Core Web APIs need a standardized, compliant mechanism for representing domain errors in HTTP responses according to the RFC 9457 ProblemDetails standard.

## Decision

We created `EricksonLopez.Result.AspNetCore`, introducing `ToHttpResult()` extension methods and `ResultEndpointFilter` for ASP.NET Core Minimal APIs.

## Consequences

### Positive
- Automatic RFC 9457 ProblemDetails payload generation on failure.
- Deterministic HTTP status mapping for all 10 `ErrorType` values (see table below). The canonical source of truth is `ResultHttpOptions.cs` (the default `StatusCodeMap` dictionary):

  | `ErrorType` | Default HTTP Status |
  |---|---|
  | `Validation` | **400 Bad Request** |
  | `Unauthorized` | **401 Unauthorized** |
  | `Forbidden` | **403 Forbidden** |
  | `NotFound` | **404 Not Found** |
  | `Conflict` | **409 Conflict** |
  | `Domain` | **422 Unprocessable Entity** |
  | `Unavailable` | **503 Service Unavailable** |
  | `Failure` | **500 Internal Server Error** |
  | `Infrastructure` | **500 Internal Server Error** |
  | `Unexpected` | **500 Internal Server Error** |
  | `Custom` | **500 Internal Server Error** |

  > The `Domain` → `422` mapping reflects that domain/business rule violations are caused by the caller's input or business invariants — `422` communicates well-formed but semantically invalid requests. `Infrastructure`, `Unexpected`, and `Custom` default to `500` to avoid incorrectly implying client error for server-side failures.

- Minimal API filter `ResultEndpointFilter` unwraps `Result` and `Result<T>` automatically.

### Negative / Trade-Offs
- Web API endpoints must reference `EricksonLopez.Result.AspNetCore` to access HTTP mapping primitives.
- All status code defaults are configurable via `ResultHttpOptions.ConfigureStatusCode(ErrorType, int)` — teams can override any mapping to match their HTTP API contracts.
