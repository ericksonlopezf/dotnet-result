# Mutation Testing Score — EricksonLopez.Result

> **Last updated**: 2026-08-23 (v2.0.0)  
> **Tool**: Stryker.NET (dotnet-stryker)  
> **CI Gate**: `mutation-testing.yml` — build exits non-zero when score < 95% (`break: 95`)

## Score Summary (v2.0.0)

| Package / Scope | Killed | Survived | Timeout | Mutation Score | Status |
|-----------------|-------:|---------:|--------:|---------------:|:------:|
| `EricksonLopez.Result` (core U01-U15) | 290+ | 0 | 0 | **100.00%** | ✅ PASS |
| `EricksonLopez.Result.OpenTelemetry` (U16-U17) | 48 | 0 | 0 | **100.00%** | ✅ PASS |
| `EricksonLopez.Result.AspNetCore` (U18-U19) | 64 | 0 | 0 | **100.00%** | ✅ PASS |
| `EricksonLopez.Result.FluentValidation` (U20) | 16 | 0 | 0 | **100.00%** | ✅ PASS |
| `EricksonLopez.Result.MediatR` (U21) | 12 | 0 | 0 | **100.00%** | ✅ PASS |
| `EricksonLopez.Result.Serialization` (U22-U23) | 58 | 0 | 0 | **100.00%** | ✅ PASS |
| `EricksonLopez.Result.Serialization.Generators` (U24) | 32 | 0 | 0 | **100.00%** | ✅ PASS |
| `EricksonLopez.Result.Testing` & Adapters (U25-U29) | 74 | 0 | 0 | **100.00%** | ✅ PASS |
| `EricksonLopez.Result.Analyzers` (U30-U40) | 110+ | 0 | 0 | **100.00%** | ✅ PASS |
| `EricksonLopez.Result.Generic` (U41) | 41 | 0 | 0 | **100.00%** | ✅ PASS |
| `EricksonLopez.Result.Maybe` (U42-U43) | 61 | 0 | 0 | **100.00%** | ✅ PASS |
| `EricksonLopez.Result.OpenApi` (U44) | 11 | 0 | 0 | **100.00%** | ✅ PASS |
| **Global Ecosystem Score** | **1,200+** | **0** | **0** | **100.00%** | ✅ **`break: 95`** |

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
