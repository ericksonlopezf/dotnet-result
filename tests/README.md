# Testing Guide for EricksonLopez.Result

This document details the test suite architecture, naming conventions, tooling, and best practices used throughout the `EricksonLopez.Result` ecosystem.

---

## 1. Test Suite Architecture

The test suite follows a strict **1:1 symmetry** model with the library source projects to ensure each component is thoroughly validated in isolation and in integration:

- `EricksonLopez.Result.Tests`: Core monadic operations, operators (`Map`, `Bind`, `Ensure`, `Recover`, `Tap`, `Match`, `Execute`), LINQ syntax, and error hierarchy.
- `EricksonLopez.Result.Generic.Tests`: Strongly-typed `Result<TValue, TError>` tests.
- `EricksonLopez.Result.Maybe.Tests`: Option monad `Maybe<T>` tests.
- `EricksonLopez.Result.AspNetCore.Tests`: HTTP response mapping, `ToHttpResult()`, and `ResultEndpointFilter` integration.
- `EricksonLopez.Result.OpenApi.Tests`: OpenAPI schema metadata generation and transformer tests.
- `EricksonLopez.Result.FluentValidation.Tests`: `ValidationResult` bidirectional mapping and pipeline extensions.
- `EricksonLopez.Result.MediatR.Tests`: `ResultExceptionBehavior` pipeline behavior tests.
- `EricksonLopez.Result.OpenTelemetry.Tests`: Activity tracing and `ResultMetrics` runtime counters.
- `EricksonLopez.Result.Serialization.Tests`: System.Text.Json custom converter serialization tests.
- `EricksonLopez.Result.Serialization.Generators.Tests`: Roslyn incremental source generator unit tests.
- `EricksonLopez.Result.Analyzers.Tests`: Roslyn diagnostic analyzer and code fix verification tests.
- `EricksonLopez.Result.Testing.Tests`: Core assertion library tests (`ShouldBeSuccess()`, `ShouldBeFailure()`).
- `EricksonLopez.Result.Testing.XUnit.Tests`: xUnit assertion adapter tests.
- `EricksonLopez.Result.Testing.NUnit.Tests`: NUnit assertion adapter tests.
- `EricksonLopez.Result.AotSmokeTest`: NativeAOT publication and runtime execution smoke test application.

---

## 2. Naming Conventions & Structure (ADR-017)

All tests in the solution institutionally adopt the **Roy Osherove Pattern** (`Method_Scenario_Result` / `UnitOfWork_StateUnderTest_ExpectedBehavior`):

$$\text{[Method/UnitOfWork]}\_\text{[Scenario/State]}\_\text{[ExpectedResult/Invariant]}$$

### Canonical Examples

```csharp
// ✅ Correct: Instantly readable in CI logs and test explorers
public void Bind_OnSuccess_ChainsToNextResult() { ... }
public void Bind_OnFailure_ShortCircuitsCallback() { ... }
public void Ensure_WhenPredicateReturnsFalse_ReturnsFilteredOutError() { ... }
public async Task TryAsync_WhenOperationCanceledExceptionThrown_PropagatesException() { ... }

// ❌ Incorrect: Numeric names without semantics (Bind_1, Bind_2) or unseparated PascalCase
```

### Style Warning Suppressions (`IDE1006` and `CA1707`)

Tests are **living executable specifications**, not public APIs for external consumers. Therefore, naming rules `IDE1006` and `CA1707` are suppressed exclusively in test projects (`tests/Directory.Build.props` and `tests/.editorconfig`) to allow underscores as structural delimiters. For further details, see [ADR-017](../docs/adr/adr-017-method-scenario-result-test-naming.md).

---

## 3. Common Commands

**Run all tests:**
```bash
dotnet test EricksonLopez.Result.slnx
```

**Run only unit tests:**
```bash
dotnet test EricksonLopez.Result.slnx --filter "Category!=Integration"
```

**Run only integration tests:**
```bash
dotnet test EricksonLopez.Result.slnx --filter "Category=Integration"
```

**Run optimized CI build and test:**
```bash
dotnet test EricksonLopez.Result.slnx --configuration Release --no-build
```

---

## 4. Test Categorization

To optimize CI pipelines and local feedback loops, tests are categorized using traits (`[Trait("Category", "Integration")]`):
- **Unit Tests** (default): Fast in-memory tests without external dependencies or hosting infrastructure.
- **Integration Tests** (`Category=Integration`): End-to-end tests utilizing `TestServer`/`WebApplicationFactory` or full source generator pipelines.

---

## 5. Uninitialized State Invariant (`default(Result)`)

All pipeline operators (`Map`, `Bind`, `Ensure`, `Recover`, `Match`, `Execute`, `Combine`, `ValidateAll`, `Merge`, `Where`, `Select`, `SelectMany`) apply fail-fast semantics and throw `InvalidOperationException` when invoked on a `default(Result)` or `default(Result<T>)` instance. The only deliberate exception is `TryGetError(out Error, out bool isUninitialized)`, designed specifically for safe state introspection.

---

## 6. Mutation Testing with Stryker.NET

Stryker.NET evaluates test resilience against code mutations:

```bash
dotnet tool restore
dotnet stryker
```

Configuration is defined in `stryker-config.json` with thresholds: `high: 100`, `low: 98`, `break: 95`.

---

## 7. NativeAOT Compatibility Validations

The test suite validates NativeAOT compilation and trimming safety through the dedicated `EricksonLopez.Result.AotSmokeTest` project executed in the `aot-smoke-test.yml` workflow with `PublishAot=true` and `TreatWarningsAsErrors=true`.
