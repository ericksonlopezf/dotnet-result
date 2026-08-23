# ADR-017: Institutional Adoption of Osherove's Test Naming Pattern (Method_Scenario_Result) and Local IDE1006 Suppression

- **Status**: Accepted
- **Date**: 2026-08-17
- **Authors**: Principal Software Engineer (QA), Erickson Lopez
- **Supersedes**: (none — formalizes existing testing convention across the repository)
- **Related**: ADR-006 (Testing Fluent Assertions), ADR-012 (Code Coverage Strategy), ADR-015 (Audit Findings Resolution)

---

## Context

In an enterprise-grade, high-performance open-source library like `EricksonLopez.Result`, automated tests fulfill two critical roles:
1. **Automated Verification:** Preventing functional and performance regressions across multiple target frameworks (`net8.0`, `net9.0`, `net10.0`).
2. **Living Executable Specifications:** Documenting the contract, invariants, and edge-case behaviors of the domain model in a form directly readable by developers, code reviewers, and automated CI/CD diagnostics.

### The Problem with Default PascalCase in Test Method Names

Standard C# coding style guidelines (enforced by Microsoft Roslyn analyzer rule `IDE1006` and Code Analysis rule `CA1707`) require all methods to follow strict PascalCase with no underscores (e.g., `BindWhenSourceIsFailureShortCircuitsCallback`).

While PascalCase is optimal for public API surface consumption in production libraries, applying it indiscriminately to test methods introduces significant friction:
- **Low Readability in CI Logs:** In automated build pipelines (GitHub Actions, Azure Pipelines, TRX test result summaries, CLI `dotnet test` output), test names are displayed in plain text without syntax highlighting. Compound sentences in PascalCase without visual boundaries (such as `BindGenericResultWithStateWhenUninitializedThrowsInvalidOperationException`) require cognitive decoding to separate the unit of work, context, and expectation.
- **Diagnostic Triage Latency:** When a build breaks in CI, on-call engineers or contributors need to immediately recognize *what unit failed*, *under what specific scenario*, and *what invariant was violated*, without having to pull the branch, open the IDE, and read the test body.
- **Arbitrary Numeric Suffixes (`Bind_1`, `Map_2`):** In the absence of an explicit, structured convention, tests tend to degrade into numeric identifiers (e.g., `Bind_1` through `Bind_48`), which provide zero diagnostic context when reported as failed in CI.

---

## Decision

We establish the following institutional standards for all test projects (`tests/**/*.Tests`) in the `EricksonLopez.Result` repository:

### 1. Mandatory Adoption of Roy Osherove's Pattern (`Method_Scenario_Result`)

All test methods across all test projects MUST follow **Roy Osherove's canonical test naming pattern**:

$$\text{[UnitOfWork/Method]}\_\text{[Scenario/StateUnderTest]}\_\text{[ExpectedBehavior/Result]}$$

Where:
- **`UnitOfWork / Method`**: The exact method, operator, property, or pipeline component under test (e.g., `Bind`, `Map`, `Ensure`, `ToProblemDetails`, `TryAsync`, `ResultHttpOptions`).
- **`Scenario / StateUnderTest`**: The specific inputs, system preconditions, or state variation being exercised (e.g., `OnSuccess`, `WhenUninitialized`, `WithState_OnFailure`, `WhenPredicateReturnsFalse`, `WithCancellationToken`).
- **`ExpectedBehavior / Result`**: The explicit observable outcome, return state, or invariant assertion (e.g., `ReturnsSuccess`, `ThrowsInvalidOperationException`, `ShortCircuitsCallback`, `PropagatesError`, `ProducesProblemDetailsWithMatchingStatus`).

#### Canonical Examples

```csharp
// ✅ Canonical: Clear visual separation into Unit under test, Context, and Expectation
public void Bind_OnSuccess_ChainsToNextResult() { ... }
public void Bind_OnFailure_ShortCircuitsCallback() { ... }
public void Ensure_WhenPredicateReturnsFalse_ReturnsFailureWithFilteredOutError() { ... }
public async Task TryAsync_WhenOperationCanceledExceptionThrown_PropagatesException() { ... }
public void ToProblemDetails_WhenCalledOnUninitializedResult_ThrowsInvalidOperationException() { ... }

// ❌ Anti-pattern: Numeric naming with zero semantic information
public void Bind_1() { ... }
public void Bind_2() { ... }

// ❌ Anti-pattern: Monolithic unpunctuated PascalCase
public void BindWhenSourceIsFailureShortCircuitsCallback() { ... }
```

### 2. Scoped Suppression of Analyzer Rules `IDE1006` and `CA1707` in Test Projects

Because test methods are invoked dynamically by test frameworks (xUnit, NUnit) and are never consumed as public API contracts by downstream consumers, we officially suppress naming style warnings for underscores locally within test assemblies:

- **`CA1707`** (*Identifiers should not contain underscores*) is suppressed in `tests/Directory.Build.props` via `<NoWarn>`.
- **`IDE1006`** (*Naming rule violation*) is suppressed in `tests/Directory.Build.props` via `<NoWarn>` and configured via `.editorconfig`.
- **Strict Enforcement in Production Code:** Production library projects (`src/**`) remain under 100% strict enforcement with `TreatWarningsAsErrors=true`, `EnforceCodeStyleInBuild=true`, and zero suppression of `IDE1006` or `CA1707`.

---

## Rationale & Justification

### 1. Tests Are Living Executable Specifications
Unit tests represent the authoritative, living technical specification of the library. A test failure is a bug report generated by the specification. The underscore delimiters provide structural parsing for human eyes in high-stress debugging scenarios.

### 2. Elimination of Diagnostic Ambiguity in CI Output
Consider the following comparison in a raw CI console log:

```text
[FAIL] EricksonLopez.Result.Tests.ResultExtensionsBindBehaviorTests.BindWhenSourceIsFailureShortCircuitsCallback
vs
[FAIL] EricksonLopez.Result.Tests.ResultExtensionsBindBehaviorTests.Bind_WhenSourceIsFailure_ShortCircuitsCallback
```

The Osherove-delimited format allows immediate triaging:
- **Unit**: `Bind`
- **Condition**: Source result is `Failure`
- **Contract**: Callback should be short-circuited (not invoked)

### 3. Clean Separation of Governance
Production assemblies (`src/`) and test assemblies (`tests/`) have fundamentally different consumer models:
- **Production Code (`src/`)**: Consumer = External software applications. Must adhere strictly to Microsoft Framework Design Guidelines and PascalCase API standards.
- **Test Code (`tests/`)**: Consumer = Developers, Reviewers, CI Log Parsers, and Test Explorers. Readability as a structured sentence takes precedence over API naming conventions.

---

## Implementation

### 1. `tests/Directory.Build.props`
The centralized test build properties file explicitly suppresses `IDE1006` and `CA1707` for all test projects:

```xml
<PropertyGroup Condition="$(MSBuildProjectName.EndsWith('.Tests'))">
  <!-- 
    Suppression Rationale (ADR-017):
    - CA1707 & IDE1006: Suppressed to institutionalize Roy Osherove's Method_Scenario_Result 
      test naming pattern. Test methods are living executable specifications whose readability 
      in CI reporting and failure logs is paramount.
  -->
  <NoWarn>$(NoWarn);IDE1006;CA1707;CA1852;CA1305;CS0619;CS0618;xUnit1051</NoWarn>
</PropertyGroup>
```

### 2. `tests/.editorconfig`
A test-specific `.editorconfig` ensures IDEs and command-line analyzers honor this decision without raising style warnings:

```ini
[*.cs]
# ADR-017: Allow underscores in test method names to follow Osherove's Method_Scenario_Result pattern
dotnet_diagnostic.IDE1006.severity = none
dotnet_diagnostic.CA1707.severity = none
```

### 3. `tests/README.md`
Updated to document the pattern and direct contributors to this standard.

---

## Consequences

### Positive
- **Instant Triage:** CI failure logs immediately communicate what broke, under what condition, and what was expected.
- **No Build Noise:** Zero compiler warnings or IDE diagnostic squiggles on test methods across Visual Studio, Rider, VS Code, and `dotnet build`.
- **Architectural Clarity:** Strict segregation between production API conventions and test specification conventions.
- **High Developer Velocity:** New tests follow a consistent, predictable template rather than arbitrary naming schemes.

### Negative / Tradeoffs
- Contributors must be instructed not to copy test naming styles into production code in `src/`. (Mitigated by automated CI enforcing `TreatWarningsAsErrors=true` on `src/`).
