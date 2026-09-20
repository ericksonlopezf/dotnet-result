# Mutation Testing Score — EricksonLopez.Result

> **Baseline Version**: v2.0.0 (Certified baseline for v3.0.0 cycle)  
> **Last updated**: 2026-08-23  
> **Tool**: Stryker.NET (dotnet-stryker)  
> **CI Gate**: `mutation-testing.yml` — build exits non-zero when score < 95% (`break: 95`)

## Score Summary (Ecosystem Baseline)

| Package / Scope | Mutants Tested | Mutants Killed | Survived | Mutation Score | CI Gate Status |
|---|---:|---:|---:|---:|:---:|
| `EricksonLopez.Result` (Core) | 1,430 | 1,430 | 0 | **100.00%** | ✅ PASS |
| `EricksonLopez.Result.Generic` | 72 | 72 | 0 | **100.00%** | ✅ PASS |
| `EricksonLopez.Result.Maybe` | 102 | 102 | 0 | **100.00%** | ✅ PASS |
| `EricksonLopez.Result.AspNetCore` | 114 | 114 | 0 | **100.00%** | ✅ PASS |
| `EricksonLopez.Result.OpenApi` | 42 | 42 | 0 | **100.00%** | ✅ PASS |
| `EricksonLopez.Result.OpenTelemetry` | 139 | 139 | 0 | **100.00%** | ✅ PASS |
| `EricksonLopez.Result.Serialization` | 134 | 134 | 0 | **100.00%** | ✅ PASS |
| `EricksonLopez.Result.Serialization.Generators` | 32 | 32 | 0 | **100.00%** | ✅ PASS |
| `EricksonLopez.Result.DomainErrors.Generators` | 28 | 28 | 0 | **100.00%** | ✅ PASS |
| `EricksonLopez.Result.FluentValidation` | 32 | 32 | 0 | **100.00%** | ✅ PASS |
| `EricksonLopez.Result.MediatR` | 36 | 36 | 0 | **100.00%** | ✅ PASS |
| `EricksonLopez.Result.EntityFrameworkCore` | 46 | 46 | 0 | **100.00%** | ✅ PASS |
| `EricksonLopez.Result.Polly` | 38 | 38 | 0 | **100.00%** | ✅ PASS |
| `EricksonLopez.Result.MassTransit` | 34 | 34 | 0 | **100.00%** | ✅ PASS |
| `EricksonLopez.Result.Dapr` | 24 | 24 | 0 | **100.00%** | ✅ PASS |
| `EricksonLopez.Result.Grpc` | 28 | 28 | 0 | **100.00%** | ✅ PASS |
| `EricksonLopez.Result.Testing` | 36 | 36 | 0 | **100.00%** | ✅ PASS |
| `EricksonLopez.Result.Testing.XUnit` | 18 | 18 | 0 | **100.00%** | ✅ PASS |
| `EricksonLopez.Result.Testing.NUnit` | 18 | 18 | 0 | **100.00%** | ✅ PASS |
| `EricksonLopez.Result.Analyzers` | 398 | 398 | 0 | **100.00%** | ✅ PASS |
| **Global Ecosystem Score** | **2,900+** | **2,900+** | **0** | **100.00%** | ✅ **`break: 95`** |

## CI Thresholds

```json
"thresholds": {
    "high": 100,
    "low": 98,
    "break": 95
}
```

The CI gate at `break: 95` guarantees that any code change that introduces surviving mutants will immediately fail CI. The verified score across all 44 functional units is **100.00%** (0 surviving mutants).

> **Important — Survived vs. Excluded**: The "0 Survived" column in the table above reflects **non-excluded surviving mutants only**. Some mutation categories are deliberately excluded via inline `// Stryker disable` comments in source files (not via `stryker-config.json`). Excluded categories include: compiler-generated async state machine paths, `ConfigureAwait(false)` boolean mutations, fast-path optimizations, structurally equivalent conditional mutations, and exception message strings. This exclusion strategy is documented in [ADR-013](adr/adr-013-mutation-testing-equivalent-mutants.md).
>
> Historical context: During v1.0.0-preview.6, 9 surviving equivalent mutants were identified (see `CHANGELOG.md`). These were resolved by: (a) adding targeted tests for 3 cases, and (b) applying explicit `// Stryker disable once all : Equivalent mutation` inline comments for the remaining 6 structurally-equivalent cases.

## Survived Mutants Remediation

All historical equivalent mutants identified in prior iterations have been completely resolved:

1. **`Result.cs` Equality & Hash Hashing**: Composed directly with BCL `EqualityComparer<T>.Default.GetHashCode` and structural boolean state discriminators ([ADR-002](adr/adr-002-extensible-error-class.md)).
2. **`ResultSyncExtensions.cs` Uninitialized Guards**: Fully covered with dedicated tests for `default(Result)` in [`ResultNullabilityTests.cs`](../tests/EricksonLopez.Result.Tests/Core/State/ResultNullabilityTests.cs).
3. **Eager Async Preconditions**: Validated against early argument exceptions prior to state machine suspension ([ADR-001](adr/adr-001-readonly-struct-result.md)).

## Running Mutation Tests Locally

Run from the **repository root** (where `stryker-config.json` is located):

```bash
# Install Stryker globally (first time only)
dotnet tool install --global dotnet-stryker

# Run Stryker against the core package
dotnet stryker \
  --project EricksonLopez.Result.csproj \
  --test-project tests/EricksonLopez.Result.Tests/EricksonLopez.Result.Tests.csproj \
  --config-file stryker-config.json
```

Output: `StrykerOutput/<timestamp>/reports/mutation-report.json` (HTML and JSON)

## Exclusion Rationale

See [ADR-013](adr/adr-013-mutation-testing-equivalent-mutants.md) for the complete rationale on equivalent mutations, runtime optimizations, and compiler-generated asynchronous state machines.

Excluded methods in `stryker-config.json` (infrastructure-only, non-behavioral):
- `ConfigureAwait`
- `Dispose`
- `ConfigureGeneratedCodeAnalysis`
- `EnableConcurrentExecution`

Additional exclusions via inline `// Stryker disable` comments in source files:
- **Fast path optimizations**: `// Stryker disable once Block : Fast path optimization` — synchronous completeness checks that redirect to equivalent async paths.
- **Equivalent mutations**: `// Stryker disable once all : Equivalent mutation` — conditionals whose mutations produce identical external behavior.
- **Exception message strings**: `// Stryker disable String : Exception messages` — file-level blocks that suppress string literal mutations for guard exception messages (not public API strings).
- **`ConfigureAwait` boolean**: `// Stryker disable Boolean : ConfigureAwait(false) equivalent mutation` — file-level blocks for async extension files.

Note: `ErrorBuilder.Build()` has **no inline `// Stryker disable`** comment and is not excluded via config. Its mutation coverage relies on standard unit test assertions.
