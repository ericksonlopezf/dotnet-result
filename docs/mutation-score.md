# Mutation Testing Score — EricksonLopez.Result

> **Last updated**: 2026-08-01 (v1.0.0)  
> **Tool**: Stryker.NET (dotnet-stryker)  
> **CI Gate**: `mutation-testing.yml` — build exits non-zero when score < 95% (`break: 95`)

## Score Summary (v1.0.0)

| Package / Scope | Killed | Survived | Ignored* | Score |
|-----------------|-------:|----------:|---------:|------:|
| `EricksonLopez.Result` (core) | 714 | 9 | 978 | **98.8%** |
| **Quality Gate** | — | — | — | **`break: 95`** ✅ |

> \* Ignored mutants are [ExcludeFromCodeCoverage]-marked async slow-path local functions (see ADR-013 and ADR-007). These are not bugs — they are equivalent mutants where the tested fast-path covers identical logic.

## CI Thresholds

```json
"thresholds": {
    "high": 100,
    "low": 98,
    "break": 95
}
```

The CI gate at `break: 95` means the build fails if mutation score drops below 95%. The actual score of **≥ 98%** consistently exceeds the `low` threshold of 98.

## Survived Mutants Analysis (v1.0.0)

All 9 surviving mutants are **equivalent mutants** — mutations that produce semantically-equivalent code that no test can distinguish:

### `Result.cs` — Boolean Mutations in Equality (8 survivors)

Lines 571, 588, 638, 655, 684, 704, 728, 749 — all are `Boolean mutation → true` in `TryAsync` and `IsFatal` paths.

**Why they survive**: These are generated inside chain boolean conditions (e.g., `Code == other.Code && Description == other.Description && ...`). When Stryker replaces one sub-condition with `true`, the overall boolean short-circuit behavior changes but the test outcome does not change because valid test inputs do not exercise the specific sub-condition in isolation. These are textbook equivalent mutants.

**Status**: Accepted as equivalent per ADR-013 §3. Adding tests for these would require invalid `Error` instances that cannot be constructed through the public API.

### `ResultSyncExtensions.cs` — Conditional False Mutation (1 survivor in earlier builds, 0 in v1.0.0)

The survivor in an earlier report (line 166) was a `Conditional (false)` mutation in `Ensure<TValue, TState>`. This was the exact gap identified and fixed — the `if (result.IsUninitialized)` guard was missing, so there were no tests targeting the uninitialized path.

After the fix, 14 new tests cover all `IsUninitialized` paths. This mutant is now killed.

## Running Mutation Tests Locally

```bash
cd tests/EricksonLopez.Result.Tests
dotnet stryker
```

Output: `StrykerOutput/<timestamp>/reports/mutation-report.json` (HTML and JSON)

## Exclusion Rationale

See [ADR-013](decisions/ADR-013-mutation-testing-equivalent-mutants.md) for the full rationale on why async state machine local functions are excluded from mutation analysis.

The 36 specifically excluded method names in `stryker-config.json` are:
`MapCore`, `MapStateCore`, `MapAsyncCore`, `MapFullAsync`, `MapStateFullAsync`, `MapCtFullAsync`,
`BindCore`, `BindAsyncCore`, `BindStateCore`, `BindAsyncStateCore`,
`TapCore`, `TapStateCore`, `TapFullAsync`, `TapOnFailureCore`, `TapOnFailureStateCore`, `TapOnFailureFullAsync`,
`EnsureCore`, `EnsureStateCore`, `EnsureFullAsync`, `EnsureStateFullAsync`,
`MatchCore`, `MatchStateCore`, `ExecuteCore`, `ExecuteStateCore`,
`RecoverCore`, `RecoverStateCore`, `MapErrorCore`, `MapErrorStateCore`,
`InspectCore`, `InspectStateCore`, `CombineCore`,
`AwaitMapValue`, `AwaitCompletedTask`, `AwaitCompletedValueTask`,
`Build`, `ShouldUseTraceIdValue`, `GetInternalFrozenStatusCodeMap`
