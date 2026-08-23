# Architectural Boundary Specification: EricksonLopez.Result

## 1. Purpose
`EricksonLopez.Result` provides the unified, high-performance, allocation-conscious functional error handling foundation (`Result`, `Result<T>`, `Error`) for the entire `EricksonLopez.*` ecosystem. It enables Railway-Oriented Programming and eliminates exceptions for business validation and control flow.

## 2. Owns
- `Result` and `Result<T>` struct representations.
- `Error`, `ErrorBuilder`, `ErrorType`, `ErrorSeverity`, and `ErrorRetryability`.
- `WellKnownErrors` catalog.
- Functional combinators: `Map`, `Bind`, `Match`, `Ensure`, `Tap`, `Combine`, `ValidateAll`.
- Diagnostic TraceId string capture and OpenTelemetry integration primitives.

## 3. Does Not Own
- HTTP Problem Details or ASP.NET Core response mapping (`EricksonLopez.Result.AspNetCore`).
- FluentValidation execution (`EricksonLopez.Result.FluentValidation`).
- JSON serialization converters (`EricksonLopez.Result.Serialization`).
- Domain entity identity or strongly-typed IDs (`EricksonLopez.DomainPrimitives.Abstractions`).
- CQRS commands/queries or pipeline dispatch (`EricksonLopez.Mediator`).

## 4. Allowed Dependencies
- **.NET BCL only** (`System.*`, `System.Collections.Immutable`, `System.Diagnostics`).
- **Zero** `EricksonLopez.*` package references.

## 5. Forbidden Dependencies
- Any other `EricksonLopez.*` package.
- Any ORM or database provider (`Npgsql`, `Microsoft.Data.SqlClient`, `Dapper`, `EFCore`).
- Any message broker client (`RabbitMQ.Client`, `Confluent.Kafka`, `AWSSDK`).
- `Microsoft.AspNetCore.*` (confined to `EricksonLopez.Result.AspNetCore`).

## 6. Who Can Depend On It
- **All Layers (L0..L5)**: Foundation, Domain, Application, Integration, Adapters, Providers, Tooling, and Testing.

## 7. Public API Rules
- Struct layout must remain auto/sequential with zero allocations on happy paths.
- `Error` must remain a sealed class with deep value equality semantics.
- Public APIs must not accept or return external library types.

## 8. AOT Expectations
- `IsAotCompatible=true`.
- Zero runtime reflection or dynamic code generation.

## 9. Trimming Expectations
- `IsTrimmable=true`.
- Strict compiler analysis with `EnableTrimAnalyzer=true` and `TreatWarningsAsErrors=true`.

## 10. Provider Isolation
- 100% database- and broker-agnostic.

## 11. Testing Isolation
- Unit test helpers and assertion extensions are strictly confined to `EricksonLopez.Result.Testing`, `EricksonLopez.Result.Testing.XUnit`, and `EricksonLopez.Result.Testing.NUnit`.
