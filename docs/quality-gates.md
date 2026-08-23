# Quality Gates & Tooling

This document describes the code quality tools, test coverage strategies, mutation testing gates, and static analysis configurations used in the `EricksonLopez.Result` ecosystem.

---

## 1. Code Coverage

### Tooling

| Tool | Version / Configuration | Purpose |
|---|---|---|
| **Coverlet** | `coverlet.collector 10.0.1` | Collects code coverage in OpenCover and Cobertura formats during `dotnet test`. |
| **Codecov** | `.codecov.yml` | Cloud coverage reporting, PR comment generation, and quality gating. |

### Configuration (`.codecov.yml`)

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

- **Project Target**: 99% line coverage project-wide with a 1% tolerance threshold.
- **Patch Target**: 90% coverage required on new code in pull requests.
- **Exclusions**: Test suites, benchmarks, sample projects, source-generated code, and serializer contexts are excluded from coverage calculations.

---

## 2. Mutation Testing (Stryker.NET)

### Configuration (`stryker-config.json`)

Stryker.NET validates test suite effectiveness by introducing logical mutations into the code base:

```json
{
  "stryker-config": {
    "thresholds": {
      "high": 100,
      "low": 98,
      "break": 95
    },
    "reporters": [
      "html",
      "cleartext",
      "progress",
      "json"
    ],
    "mutate": [
      "**/*.cs"
    ],
    "ignore-methods": [
      "ConfigureAwait",
      "Dispose",
      "ConfigureGeneratedCodeAnalysis",
      "EnableConcurrentExecution"
    ],
    "ignore-mutations": []
  }
}
```

- **Thresholds**:
  - `break: 95` — Hard quality gate failure if score drops below 95%.
  - `low: 98` — Warning threshold.
  - `high: 100` — Green quality target.
- **Current Score**: **≥ 98% mutation score** against core domain logic. See [`docs/mutation-score.md`](mutation-score.md) for full mutant breakdown.

### Execution Strategy & Quality Gates

| Scope | Trigger | Behavior | Quality Gate Policy |
|---|---|---|---|
| **Pull Requests** | `pull_request` to `main`, `develop` | Fast CI only (`build`, `test`, `coverage`, `aot-smoke-test`). | **Stryker is NOT run** on PRs to avoid blocking merges with 60+ min runs. |
| **`main` Branch** | `push` to `main`, `workflow_dispatch`, weekly cron | Runs `mutation-testing.yml` asynchronously. Sets commit status. | Score $< 95\%$ fails the workflow job. Score $\ge 95\%$ passes. |
| **Release** | Tag `v*.*.*`, `workflow_dispatch` | Validates the latest valid mutation score recorded for `main`. | **Score $\ge 95\%$ permits release**; score $< 95\%$ or no test blocks release. No redundant re-run. |

---

## 3. Static Analysis & Compiler Enforcement

### MSBuild Quality Settings (`Directory.Build.props`)

| Setting | Value | Effect |
|---|---|---|
| `TreatWarningsAsErrors` | `true` | All compiler warnings fail the build. |
| `WarningLevel` | `5` | Maximum compiler warning sensitivity. |
| `AnalysisLevel` | `latest-recommended` | Enforces latest SDK Roslyn code quality analyzers. |
| `EnforceCodeStyleInBuild` | `true` | Enforces `.editorconfig` code style rules during compilation. |
| `Nullable` | `enable` | Enforces nullable reference type annotations. |
| `NuGetAudit` | `true` | Performs automated security vulnerability scans during package restore. |

### SonarCloud Integration

Integrated in CI workflows via `dotnet-sonarscanner`:
- **Organization**: `ericksonlopez`
- **Project Key**: `ericksonlopez_dotnet-result`
- **Coverage Format**: OpenCover XML (`**/coverage.opencover.xml`)

---

## 4. Custom Roslyn Analyzers & Code Fixes

The repository ships custom Roslyn analyzers in `EricksonLopez.Result.Analyzers` and source generators in `EricksonLopez.Result.Serialization.Generators`:

| Diagnostic ID | Category | Severity | Description | Code Fix Available |
|---|---|---|---|---|
| `RESULT001` | Performance | Warning | Large struct (>32B) used as `Result<T>` — excessive copying overhead. | No |
| `RESULT003` | Usage | **Error** | `ErrorBuilder.With*()` return value discarded — mutated struct copy is lost. | `ErrorBuilderDiscardedReturnCodeFix` |
| `RESULT004` | Performance | Warning | Lambda expression captures outer variable in Result pipeline (closure allocation). | `ClosureCaptureCodeFix` (Make static / insert TState guidance) |
| `RESULT005` | Performance | Warning | `Error.WithMetadata()` / `ErrorBuilder.WithMetadata()` chained 3+ times consecutively. | No |
| `RESULT006` | Performance | Warning | `ErrorBuilder.WithInnerError()` chained 2+ times consecutively without batching. | No |
| `RESULT007` | Reliability | Warning | `HashSet<Error>`, `Distinct()`, `GroupBy()`, or `ToHashSet()` used without `ErrorEqualityComparer.Strict`. | No |
| `RESULT008` | Usage | Warning | Endpoint returning `Result<T>` uses `AddResultEndpointFilter()` without `.Produces<T>()`. | No |
| `RESULT009` | Security | Warning | `IncludeDescription = true` set without environment guard — potential information disclosure. | No |
| `RESULT010` | Security | Warning | `ResultExceptionBehavior` default error factory may expose internal exception type names. | No |
| `RESULT012` | Usage | Warning | Method returning `default(Result)` or `default(Result<T>)` — uninitialized state bug. | No |
| `RESULT_OTEL_001` | Observability | Info | `TraceOutcome()` called without `ResultMetrics` registered. | No |
| `RESULT_GEN_001` | Usage | Warning | `[JsonSerializable(typeof(Result))]` on serializer context has no effect (non-generic Result is handled automatically). | No |

---

## 5. Public API Surface Tracking

The core library enforces strict semantic versioning and breaking change protection via `Microsoft.CodeAnalysis.PublicApiAnalyzers`:
- `PublicAPI.Shipped.txt`: Canonical tracked public API surface.
- `PublicAPI.Unshipped.txt`: Staging buffer for new unreleased public APIs.
