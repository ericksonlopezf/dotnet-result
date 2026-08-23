# ADR-003: Native OpenTelemetry Activity & System.Diagnostics.Metrics Integration

- **Status**: Accepted
- **Date**: 2026-07-28
- **Authors**: Erickson Lopez

---

## Context

Cloud-native applications require distributed tracing and metrics observability without incurring performance overhead or tightly coupling core domain logic to external monitoring SDKs.

## Decision

We created a dedicated companion package `EricksonLopez.Result.OpenTelemetry` that integrates directly with standard BCL `System.Diagnostics.Activity` and `System.Diagnostics.Metrics.Meter` primitives.

## Consequences

### Positive
- Zero external third-party SDK dependencies (uses built-in `System.Diagnostics` APIs).
- Seamless integration with OpenTelemetry exporters (OTLP, Jaeger, Prometheus, Azure Monitor).
- Automatic tagging of `error.code`, `error.type`, `error.severity`, and `error.retryable` on active Activity spans.
- Lazy stringification of `ActivityTraceId` avoids string allocations when tracing is inactive.
