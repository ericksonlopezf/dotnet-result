# Quality Gates & Tooling

This document describes the code quality tools, coverage strategies, and static analysis configurations used in the `EricksonLopez.Result` ecosystem.

---

## Code Coverage

### Tooling

| Tool | Version | Purpose |
|---|---|---|
| Coverlet | `coverlet.collector 6.0.4` | Collects code coverage during `dotnet test` |
| Codecov | `.codecov.yml` | Cloud coverage reporting and PR gating |

### Configuration

**Codecov** (`.codecov.yml`):

```yaml
coverage:
  status:
    project:
      default:
        target: 99%
        threshold: 1%
    patch:
      default:
        target: 90%
        threshold: 5%

ignore:
  - "tests/**/*"
  - "benchmarks/**/*"
  - "samples/**/*"
  - "**/*.g.cs"
  - "**/GeneratedFiles/**"
  - "**/*JsonSerializerContext*"
```

- **Project Target:** 99% line coverage project-wide with 1% tolerance
- **Patch Target:** 90% coverage required on new code in PRs with 5% tolerance
- **Exclusions:** Test, benchmark, sample projects, source-generated files, and `JsonSerializerContext` types

### Coverage Strategy

The project follows a strict 100% genuine code coverage policy documented in [ADR-012](decisions/ADR-012-code-coverage-strategy.md):

1. **Generated code exclusion:** `.runsettings` excludes `**/*.g.cs` and `*JsonSerializerContext*` patterns
2. **`[ExcludeFromCodeCoverage]`:** Applied to compiler-generated infrastructure code (async state machines, NativeAOT adaptations)
3. **Async state machine pattern:** `ValueTask` methods use the wrapper/core split pattern ([ADR-008](decisions/ADR-008-valuetask-coverlet-deadlock-avoidance.md)) to avoid Coverlet deadlocks

---

## Mutation Testing

### Tooling

| Tool | Configuration File | Purpose |
|---|---|---|
| Stryker.NET | `stryker-config.json` | Mutation testing for logical correctness |

### Configuration

The current `stryker-config.json` defines an aggressive quality gate with explicit method exclusions:

```json
{
    "stryker-config": {
        "thresholds": {
            "high": 100,
            "low": 98,
            "break": 95
        },
        "reporters": ["html", "cleartext", "progress", "json"],
        "ignore-methods": [
            "Build", "ShouldUseTraceIdValue", "GetInternalFrozenStatusCodeMap",
            "MapCore", "MapStateCore", "MapAsyncCore", "MapFullAsync", ...
        ]
    }
}
```

- **Thresholds:**
  - `break: 95` — CI fails if score drops below 95% (hard gate)
  - `low: 98` — Warning at 98%
  - `high: 100` — Green indicator at 100%
- **Ignored methods:** 36 explicitly named async slow-path local functions that are equivalent mutants (see [ADR-013](decisions/ADR-013-mutation-testing-equivalent-mutants.md) and [Mutation Score](mutation-score.md) for full analysis)
- **No wildcard patterns:** Wildcards like `*Core*` were replaced with explicit method names in preview.6 to prevent silently excluding future business-logic methods

> [!NOTE]
> As of preview.6, the mutation score is **≥ 98%** against tested business logic. See [docs/mutation-score.md](mutation-score.md) for the full report, score breakdown by file, and analysis of all surviving mutants.

---

## Static Analysis

### SonarCloud

SonarCloud is integrated into the CI pipeline via `dotnet-sonarscanner`:

| Property | Value |
|---|---|
| Organization | `ericksonlopez` |
| Project Key | `ericksonlopez_{repository-name}` |
| Host | `https://sonarcloud.io` |
| Coverage Format | OpenCover (`sonar.cs.opencover.reportsPaths`) |

SonarCloud analysis is conditional — it only runs when `SONAR_TOKEN` is non-empty.

### Compiler Analysis

From `Directory.Build.props`:

| Setting | Value | Effect |
|---|---|---|
| `TreatWarningsAsErrors` | `true` | All warnings fail the build |
| `WarningLevel` | `5` | Maximum warning sensitivity |
| `AnalysisLevel` | `latest-recommended` | Latest .NET SDK analyzers |
| `EnforceCodeStyleInBuild` | `true` | Code style rules enforced at build time |
| `Nullable` | `enable` | Nullable reference types enabled |

### Public API Tracking

The core `EricksonLopez.Result` project uses `Microsoft.CodeAnalysis.PublicApiAnalyzers 3.3.4` with:

- `PublicAPI.Shipped.txt` — Tracks all shipped public API surface
- `PublicAPI.Unshipped.txt` — Tracks new unshipped API additions

This prevents accidental breaking changes to the public API surface.

### Custom Roslyn Analyzers

`EricksonLopez.Result.Analyzers` provides **11 diagnostic analyzers and 2 code fix providers** bundled with the core package:

| ID | File | Severity | Rule |
|---|---|---|---|
| `RESULT001` | `LargeResultValueAnalyzer.cs` | Warning | Large value type (>64 bytes) used as `Result<T>` — copy overhead warning |
| `RESULT003` | `ErrorBuilderDiscardedReturnAnalyzer.cs` | **Error** | `ErrorBuilder.With*()` return value discarded — always a bug |
| `RESULT004` | `ClosureCaptureAnalyzer.cs` | Warning | Lambda captures outer variable in a Result pipeline method (allocation/state risk) |
| `RESULT005` | `MetadataChainingAnalyzer.cs` | Warning | `Error.WithMetadata()` / `ErrorBuilder.WithMetadata()` called in a loop without batching |
| `RESULT007` | `HashSetErrorEqualityAnalyzer.cs` | Warning | `HashSet<Error>`, `Distinct()`, `GroupBy()`, or `ToHashSet()` used without `ErrorEqualityComparer.Strict` |
| `RESULT008` | `EndpointFilterOpenApiAnalyzer.cs` | Warning | Endpoint returning `Result<T>` uses `AddResultEndpointFilter()` without `.Produces<T>()` — OpenAPI schema degradation |
| `RESULT009` | `IncludeDescriptionSecurityAnalyzer.cs` | Warning | `IncludeDescription = true` set without an environment guard — potential information disclosure |
| *(RESULT010?)* | `InnerErrorChainingAnalyzer.cs` | Warning | Excessive `WithInnerError()` chaining depth that indicates a design smell |
| *(RESULT011?)* | `TraceOutcomeWithoutMetricsAnalyzer.cs` | Warning | `TraceOutcome()` used without `ResultMetrics` registered — trace without metrics |
| *(RESULT012?)* | `ResultExceptionBehaviorMessageAnalyzer.cs` | Info | `ResultExceptionBehavior` usage guidance |

**Code Fix Providers:**

| Code Fix | For Diagnostic | Action |
|---|---|---|
| `ClosureCaptureCodeFix.cs` | RESULT004 | Rewrites the lambda to use the `TState` overload, eliminating the closure capture |
| `ErrorBuilderDiscardedReturnCodeFix.cs` | RESULT003 | Assigns the discarded return value to the target variable or wraps in a proper call chain |

> [!NOTE]
> The IDs marked with `?` above (RESULT010-RESULT012) represent internal diagnostic IDs that may differ from the pattern. Run `dotnet build` with the Analyzers package referenced to see actual diagnostic IDs in IDE warnings.

---

## Dependency Management

### Current Approach

Dependencies are pinned per-project in individual `.csproj` files. Central Package Management (`Directory.Packages.props`) is **not used**.

### Dependency Scanning

Dependabot is configured via `.github/dependabot.yml` and monitors two ecosystems:

| Ecosystem | Schedule | Target Branch | PR Limit |
|---|---|---|---|
| NuGet | Weekly (Monday 09:00 ET) | `develop` | 10 |
| GitHub Actions | Monthly (Monday 09:00 ET) | `develop` | 5 |

NuGet dependencies are grouped to reduce PR noise (e.g., `Microsoft.Extensions.*`, `System.*`, `FluentValidation*`, `MediatR*`, `OpenTelemetry*`, `xunit*`, `Microsoft.CodeAnalysis*`). See [CI/CD Pipeline](CICD.md#dependency-management) for the full grouping configuration.

### NuGet Security Auditing

`Directory.Build.props` enables NuGet's built-in security audit:

```xml
<NuGetAudit>true</NuGetAudit>
<NuGetAuditMode>all</NuGetAuditMode>
<NuGetAuditLevel>low</NuGetAuditLevel>
```

This flags any NuGet dependency with known vulnerabilities during `dotnet restore`.

---

## Build Configuration

### Debug vs Release

| Setting | Debug | Release |
|---|---|---|
| Optimization | Disabled (or disabled when `CollectCoverage=true`) | Enabled |
| Deterministic | `true` | `true` |
| CI Build | Set when `CI=true` | Set when `CI=true` |
| SourceLink | Enabled | Enabled |
| Symbol Package | `.snupkg` | `.snupkg` |

### Strong Name Signing

All assemblies are strong-named using `EricksonLopez.Result.snk`:

```xml
<SignAssembly Condition="Exists('$(MSBuildThisFileDirectory)EricksonLopez.Result.snk')">true</SignAssembly>
```

The signing is conditional — if the `.snk` file is not present (e.g., open-source contributor without the secret), builds succeed without signing.

---

## Test Framework

| Component | Version |
|---|---|
| xUnit | `2.9.3` |
| xUnit Runner (Visual Studio) | `2.8.2` |
| Microsoft.NET.Test.Sdk | `17.12.0` |
| Coverlet Collector | `6.0.4` |
| Coverlet MSBuild | `10.0.1` |
| AwesomeAssertions | `9.5.0` |
| NSubstitute | `6.0.0` |
| Target Frameworks | `net8.0`, `net9.0`, `net10.0` |

> [!NOTE]
> The test project targets **all three frameworks**: `net8.0`, `net9.0`, and `net10.0`
> (verified in `EricksonLopez.Result.Tests.csproj`). Tests run in parallel for all three TFMs in CI.
