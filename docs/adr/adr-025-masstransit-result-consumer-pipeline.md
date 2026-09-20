# ADR-025: MassTransit Result Consumer Pipeline

## Status
Accepted

## Date
2026-09-08

## Context
MassTransit is a widely adopted message bus framework in enterprise .NET distributed systems. When consumers return or encounter domain failures expressed as `Result` or `Result<T>`, propagating these failures across transport boundaries requires translation into structured message fault contracts without losing error metadata, codes, or severity classifications.

## Decision
We created `EricksonLopez.Result.MassTransit`:
1. `ResultFault`: An immutable, transport-safe message contract capturing `Code`, `Description`, `Type`, `Severity`, `Retryability`, `TraceId`, `CorrelationId`, and `Metadata`.
2. `ResultConsumeFilter<TMessage>`: An `IFilter<ConsumeContext<TMessage>>` interceptor that coordinates message pipelines and fault transformations.
3. Explicit Non-AOT Isolation: Consistent with ADR-018 for MediatR, `EricksonLopez.Result.MassTransit` explicitly declares `<IsAotCompatible>false</IsAotCompatible>` and `<IsTrimmable>false</IsTrimmable>` to reflect MassTransit's dynamic runtime reflection model while protecting the remainder of the ecosystem.

## Consequences
### Positive
- Structured error propagation across message queues (RabbitMQ, Azure Service Bus, Amazon SQS).
- Preserves distributed tracing and error taxonomy across asynchronous message boundaries.

### Negative / Trade-offs
- Non-AOT compatible due to upstream MassTransit architectural constraints.
