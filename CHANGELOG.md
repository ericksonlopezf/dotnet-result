# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/), and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [2.0.0](https://github.com/ericksonlopezf/dotnet-result/compare/v1.0.0...v2.0.0) (2026-08-24)


### ⚠ BREAKING CHANGES

* introduce comprehensive Result framework with analyzers, serialization generators, and integration supporty

### ✨ Features

* introduce comprehensive Result framework with analyzers, serialization generators, and integration supporty ([ed8fe87](https://github.com/ericksonlopezf/dotnet-result/commit/ed8fe879144fc1f32a4116f0026f8d907f0377b5))

## [Unreleased]

## [2.0.0] - 2026-08-23

### Added
- **`EricksonLopez.Result.Generic` Package**:
  - Introduced `Result<TValue, TError>` readonly struct for compile-time strongly-typed domain error hierarchies with zero heap allocation.
  - Includes monadic operators (`Map`, `MapError`, `Bind`, `Match`), `TryGetValue`/`TryGetError` guards, implicit conversions, and `ToResult(Func<TError, Error>)` projection to standard Result.
- **`EricksonLopez.Result.Maybe` Package**:
  - Introduced `Maybe<T>` readonly struct for allocation-free optionality modeling.
  - Supports monadic transformations (`Map`, `Bind`, `Match`, `Ensure`), fallback accessors (`GetValueOrDefault`, `GetValueOrFallback`), and conversion to `Result<T>` via `.ToResult(Error)`.
- **`EricksonLopez.Result.OpenApi` Package**:
  - Added Minimal API endpoint metadata extensions: `ProducesResult<TResponse>()`, `ProducesResult()`, and `ProducesResultProblemDetails()` for RFC 9457 OpenAPI / Swagger schema generation.
- **Compound Async Validation**:
  - Added `Result.ValidateAll` and `Result.ValidateAllAsync` overloads supporting sequential and parallel validation pipelines with error accumulation.
- **Roslyn Analyzers**:
  - Added **`RESULT012`** (`DefaultResultReturnAnalyzer`): Emits a compile-time warning when `default` or `default(Result)` / `default(Result<T>)` is returned from a method, preventing uninitialized struct bugs.

### Changed
- **Centralized Package Management (CPM)**: Enabled `Directory.Packages.props` across the solution to centrally manage all NuGet package versions.
- **Project Structure**: Modularized test and sample suites for 1:1 symmetry across all ecosystem packages.

### Breaking Changes
- **Core — Removed `Finally` Monadic Operators (BC-001 & BC-002)**:
  - Removed `Result.Finally(Action<Result>)` and `Result<TValue>.Finally(Action<Result<TValue>>)` instance methods from `Result` and `Result<TValue>`.
  - Removed `ResultExtensions.Finally` extension methods on `Task<Result>` and `Task<Result<T>>`.
  - **Impact**: Code calling `.Finally(...)` will fail compilation with `CS1061`.
  - **Migration**: Replace `.Finally(...)` with `.Inspect(...)` (or `await task.Inspect(...)`), which provides identical inspection semantics without naming ambiguity.
- **Core — Removed Obsolete `FoldError` Methods (BC-003)**:
  - Removed `FoldError<TOut>` and `FoldError<TState, TOut>` from `Result` and `Result<TValue>`.
  - **Impact**: Removed from assembly metadata; causes compilation errors and runtime `MissingMethodException` for un-recompiled binaries.
  - **Migration**: Replace `.FoldError(onFailure, default)` with `.MapFailure(onFailure, default)`.
- **Core — Removed Obsolete `ToResult()` and `WithoutValue()` Methods (BC-004)**:
  - Removed `Result<TValue>.ToResult()` and `Result<TValue>.WithoutValue()` from `Result<TValue>`.
  - **Impact**: Removed from assembly metadata.
  - **Migration**: Replace with `.DiscardValue()`.
- **Core — Strict Uninitialized Guard on `Merge` and `Combine` (BC-009)**:
  - `Result.Merge` and `Result.Combine` now throw `InvalidOperationException` if any operand is an uninitialized default struct (`default(Result)` / `default(Result<T>)`).
  - **Impact**: Uninitialized results that previously returned a fallback or passed through silently will now fail fast at the call site.
  - **Migration**: Always construct results using `Result.Success` or `Result.Failure`.
- **OpenTelemetry — Removed Obsolete Static `RecordSuccess` / `RecordFailure` (BC-005)**:
  - Removed `ResultMetrics.RecordSuccess(string)` and `ResultMetrics.RecordFailure(string, string, string)`.
  - **Impact**: Compile-time error `CS0117` when referencing `RecordSuccess` or `RecordFailure`.
  - **Migration**: Replace with `ResultMetrics.StaticTrackSuccess(...)` and `ResultMetrics.StaticTrackFailure(...)`.
- **FluentValidation — Dependency Upgrade to v12 (BC-006)**:
  - Upgraded minimum package dependency from `FluentValidation 11.11.0` to `FluentValidation 12.1.1`.
  - **Impact**: NuGet package dependency resolution will require FluentValidation 12.x+.
  - **Migration**: Update host applications to FluentValidation 12.1.1 or higher.
- **MediatR — Dependency Upgrade to v14 (BC-007)**:
  - Upgraded minimum package dependency from `MediatR 12.4.1` to `MediatR 14.2.0`.
  - **Impact**: NuGet package dependency resolution will require MediatR 14.x+.
  - **Migration**: Update host applications to MediatR 14.2.0 or higher.
- **Testing — NUnit & xUnit Dependency Upgrades (BC-008 & BC-010)**:
  - Fixed `EricksonLopez.Result.Testing.NUnit.csproj` package reference and upgraded `NUnit` to `4.6.1`.
  - Upgraded `xunit.v3.assert` to `4.0.0` in `EricksonLopez.Result.Testing.XUnit`.
  - **Impact**: Test projects using these assertions should align their test framework runner versions.

## [1.0.0] - 2026-08-01

### Added
- `ProducesResult<T>()` extension method on `RouteHandlerBuilder` to enforce OpenAPI typed schemas and mitigate the `object` degradation from Endpoint Filters.
- `EricksonLopez.Result.Testing.NUnit` package for native NUnit assertion support.
- Cross-TFM integration tests for STJ Property Name generation.
- Cold start benchmarks for `Expression.Compile()` in `ResultExceptionBehavior`.

### Changed
- None. (The planned rename of `TryAsyncValue` to `TryAsync` was aborted as it introduced `CS0121` compiler ambiguity for async lambdas).

## [1.0.0-preview.6] - 2026-07-25

### Fixed

- **API Contract (B-01 from ARB-04 audit): `ResultSyncExtensions` now throws `InvalidOperationException` for uninitialized results**:
  All monadic methods in `ResultSyncExtensions` (`Map`, `Bind`, `Ensure`, `Match`) previously accessed `result.IsSuccess`, `result.Value`, and `result.Error` directly without first checking for the `Uninitialized` state. This created an **asymmetric contract** with the instance methods on `Result<TValue>` (which all call `ThrowIfUninitialized()` as their first action). The symptom was subtle: calling `default(Result<Guid>).Map(x => x.ToString())` would not throw at the guard point — it would silently return a failure result (because `IsSuccess == false` for uninitialized), or throw with a confusing stack trace inside `result.Error` for `Ensure`-path variants.
  
  **Fix**: Each public method now calls `if (result.IsUninitialized) ResultThrowHelper.ThrowUninitializedOfT()` as its first statement, producing a clear `InvalidOperationException` with an actionable message, consistent with the instance methods.
  
  **Preserved behavior**: `TryGetValue` and `GetValueOrDefault` intentionally **do not** throw for uninitialized results, matching the BCL `Try*`/`*OrDefault` convention. Their XML docs and inline comments explain this explicitly.

### Changed

- **Mutation Testing — Stryker `ignore-methods` wildcards replaced with explicit allowlist (REC-02 from ARB-04 audit)**:
  `stryker-config.json` previously used `*Core*`, `*FullAsync*`, and `*Await*` glob patterns to exclude async slow-path local functions from mutation analysis. While functionally correct, these wildcards were **fragile**: any future method with "Core" in its name would be silently excluded from mutation testing without any warning.
  
  The wildcards are replaced with an explicit list of the 36 known excluded method names (`MapCore`, `BindCore`, `TapCore`, `EnsureCore`, `MatchCore`, `ExecuteCore`, `RecoverCore`, `MapErrorCore`, `InspectCore`, `CombineCore`, and their `*State*`, `*Async*`, `*FullAsync*` variants). All previously excluded methods remain excluded — only the matching strategy changes from glob to explicit.

- **Mutation score now directly verifiable in repo (B-03 from ARB-04 audit)**:
  Added [`docs/mutation-score.md`](docs/mutation-score.md) as a committed artifact documenting the latest Stryker run results: **≥ 98% mutation score** against tested business logic, with a full breakdown per file and analysis of all surviving/equivalent mutants. Updated [`docs/quality-gates.md`](docs/quality-gates.md) to accurately reflect the current `stryker-config.json` (thresholds, explicit method allowlist). Added a static **Mutation Score ≥ 98%** badge to README.

- **Version bump**: `Directory.Build.props` `VersionSuffix` updated from `preview.5` → `preview.6`.

### Security

- **`SECURITY.md` — documented `ResultExceptionBehavior` + `IncludeDescription` PII risk (REC-01 from ARB-04 audit)**:
  Added a new **"Information Disclosure Risks"** section to `SECURITY.md` documenting the risk chain:
  `ResultExceptionBehavior` captures `ex.Message` as `Error.Description`. If the application also sets `IncludeDescription = true` in `ResultHttpOptions`, exception messages (which may contain connection strings, file paths, PII, or infrastructure names) are included in HTTP response bodies.
  
  The section documents all four mitigations: the secure-by-default `IncludeDescription = false`, the RESULT009 compile-time analyzer, environment guards, and the custom `errorFactory` parameter on `AddResultExceptionBehavior`.

---

## [1.0.0-preview.5] - 2026-07-15

### Added
- **Analyzers**:
  - **RESULT005** (`MetadataChainingAnalyzer`) extended to also detect `ErrorBuilder.WithMetadata(string, object?)` chaining (3+ consecutive calls). Previously the analyzer only flagged `Error.WithMetadata()` chaining. `ErrorBuilder` does not create `Error` heap copies, but each single-key call still performs an O(log k) AVL-tree mutation on the backing `ImmutableDictionary`; the recommended pattern is `WithMetadata(IReadOnlyDictionary<string, object?>)` or `WithMetadata(IEnumerable<KeyValuePair<string, object>>)` for batch application. The diagnostic message is distinct between Error and ErrorBuilder paths to accurately describe the allocation model of each.
- **Documentation**:
  - **README.md**: Added new **"Common Pitfalls"** section (before the ecosystem footer) documenting three production gotchas with code examples and mitigations: (1) `default(Result)` silently evaluates to `false` in boolean context — explains `IsUninitialized` as the safe guard; (2) `AddResultEndpointFilter()` requires explicit `.Produces<T>()` for OpenAPI schema accuracy, with a comparison against `ToHttpResult()` for boxing-sensitive paths; (3) `HashSet<Error>` deduplicates errors that share the 5 semantic fields — demonstrates `ErrorEqualityComparer.Strict` as the solution.
  - **`ResultEndpointRouteBuilderExtensions.cs`**: Enriched XML docs for `AddResultEndpointFilter(RouteHandlerBuilder)` with three `<remarks>` paragraphs: (a) OpenAPI schema limitation and requirement for `.Produces<T>()`, with reference to RESULT008; (b) Boxing tradeoff explanation (1–2 allocations/request) with the zero-allocation `ToHttpResult()` alternative pattern; (c) Uninitialized result protection via `IResultOutcome.IsUninitialized`.

### Fixed
- **API Contract**:
  - **`PublicAPI.Unshipped.txt` flushed to `PublicAPI.Shipped.txt`** (BLOCK-C from ARB audit): All 14 `ResultSyncExtensions` method signatures added in preview.3/preview.4 are now tracked in `PublicAPI.Shipped.txt`. The `PublicApiAnalyzers` will now enforce breaking-change protection for these APIs going forward. `PublicAPI.Unshipped.txt` is reset to the header-only state (empty functional state).

### Changed
- **Version bump**: `Directory.Build.props` `VersionSuffix` updated from `preview.4` → `preview.5`.

### ARB Audit — Preview.5 False Positive Resolutions
- **FluentValidation naming (API-03)**: The audit noted inconsistency between `ValidateAsResult` and `ValidateToResult`. Code review confirms that the current API consistently uses the `ValidateTo*` naming family: `ValidateToResult()`, `ValidateToResultWithValue()`, `ValidateToResultAsync()`, `ValidateToResultWithValueAsync()`. The noted inconsistency was a false positive against an earlier code version.

---

## [1.0.0-preview.4] - 2026-06-30

### Added
- **Core**:
  - `ResultSyncExtensions.Ensure` factory delegate overloads (ARB-03): Added `Func<Error>` (lazy factory — avoids allocating `Error` on the success path) and `Func<TValue, Error>` (value-contextual factory — receives the failed value to build context-aware errors) overloads, plus their `TState` variants. This completes full API parity between `ResultSyncExtensions.Ensure` and the instance method `Result<TValue>.Ensure` overloads. Six new overloads total.

### Fixed
- **CI/Mutation Testing** (ARB-01): Synchronized thresholds in `mutation-testing.yml` Python step summary script with `stryker-config.json`. Previously `HIGH=90, LOW=78, BREAK=75` in the script while `stryker-config.json` defines `high=100, low=98, break=95`. A mutation score of 80% would have shown "🟡 LOW" in the GitHub Step Summary while Stryker had already exited with code 1 — producing actively misleading CI output. Now both sources use the same thresholds.
- **Build Configuration** (ARB-04): Added clear documentation comment in `EricksonLopez.Result.csproj` explaining why `RS0016`, `RS0017`, and `RS0026` are suppressed locally. The previous bare `<NoWarn>` entry created a false contradiction with the CHANGELOG statement that "RS0016/RS0017 are enforced as errors." The comment now accurately documents that PublicApiAnalyzers tracks incremental changes only (not the full API surface) and that RS0026 suppression is required for InternalsVisibleTo false positives.

### Documentation
- **ADR-015** (ARB-08): Added §11 documenting `ResultExceptionBehavior<TRequest, TResponse>`'s `Expression.Compile()` cold start characteristic. Documents the per-handler-type cost (10-50ms JIT, up to 200ms in cold serverless environments), why it is accepted (MediatR itself requires dynamic code; caching eliminates per-request overhead), and the mitigation pattern for serverless warm-up.

### ARB False Positives (confirmed resolved prior to this release)
- **ARB-05** (`RESULT008` `EndpointFilterOpenApiAnalyzer`): Confirmed implemented in `EndpointFilterOpenApiAnalyzer.cs`. The analyzer exists and warns when `AddResultEndpointFilter()` is called without `.Produces<T>()`.
- **ARB-07** (`GeneratedCodeAttribute` hardcoded version): `ResultJsonConverterGenerator.GeneratorVersion` already uses `AssemblyInformationalVersionAttribute` dynamically with `"1.0.0"` only as a fallback when the attribute is missing. Not hardcoded.

### Changed
- **Version bump**: `Directory.Build.props` `VersionSuffix` updated from `preview.3` → `preview.4`.

---

## [1.0.0-preview.3] - 2026-06-10


### Added
- **Architecture**:
  - **ADR-016**: New Architecture Decision Record documenting the `IResultOutcome` managed interface decision — why interface over duck-typing/generic constraints, the accepted boxing tradeoff, canonical performance guidance (filter vs. `ToHttpResult<T>()`), and the v2.0 evolution path. This resolves the ARB audit finding REC-02.
- **Core**:
  - `operator true` and `operator false` for `Result` and `Result<T>` enabling direct boolean evaluation in `if` statements (fast paths).
  - New `TryAsync(Func<ValueTask>)` and `TryAsync(Func<ValueTask<T>>)` overloads explicitly preventing allocations and boxing when wrapping `ValueTask` operations.
  - `Result.MapFailure<TOut>(Func<Error, TOut>, TOut)` and `Result<T>.MapFailure<TOut>(Func<Error, TOut>, TOut)` — .NET-idiomatic aliases for `FoldError` that are more discoverable by developers without functional programming backgrounds. Corresponding `TState` overloads included. `FoldError` is now marked `[EditorBrowsable(Advanced)]` in favor of `MapFailure` for IDE discoverability.
- **AspNetCore**:
  - Propagate `TraceId` and `CorrelationId` recursively from `Error` into `ProblemDetails` extensions dictionary for better observability correlation.
  - Added `TraceId` to `ErrorDetailDto` serialization payload.
- **Analyzers**:
  - **RESULT005**: New `MetadataChainingAnalyzer` — warns when `Error.WithMetadata(string, object)` is chained 3 or more times consecutively (creates N intermediate `Error` copies). Suggests using `WithMetadata(IReadOnlyDictionary)` or `ToBuilder()` for batch operations.
  - **RESULT006**: New `InnerErrorChainingAnalyzer` — warns when `ErrorBuilder.WithInnerError(Error)` is chained 2 or more times consecutively (O(n²) `ImmutableArray` copying). Suggests using `WithInnerErrors(IEnumerable<Error>)` for O(n) batch addition.
  - **RESULT_OTEL_001**: New `TraceOutcomeWithoutMetricsAnalyzer` — emits an `Info`-level hint when `TraceOutcome`, `TraceOnFailure`, or `TraceOnSuccess` are called without the `metrics` parameter (or with explicit `null`). Prevents silent metric omission when using DI-registered `ResultMetrics`. Suppressible for intentional static-mode usage.

### Fixed
- **Core**:
  - Aliasing bug in `ErrorBuilder` where metadata dictionaries were shared by reference between mutations. Internal dictionary is now strictly immutable.
  - Exposed `Error` constructor via direct instantiation made `protected`, preventing invalid initialization bypasses.
  - Fix ambiguous async lambda resolution for `TryAsync` in tests.
  - **`operator false` XML documentation corrected**: the previous doc incorrectly stated that `default(Result)` (Uninitialized state) returns `true` for `operator false`. In reality, `IsFailure` returns `false` for Uninitialized (it checks `_state == ResultState.Failure`, which is false for the default byte value 0). An uninitialized result returns `false` for both `operator true` and `operator false` — it is neither success nor failure in boolean context.
- **Serialization**:
  - `ResultJsonConverterFactory` visibility reduced to `internal` as it's an infrastructural detail.
  - Recursively parse objects and arrays within metadata into `Dictionary<string, object?>` and `List<object?>` respectively, aligning with native JSON primitives rather than raw JSON strings.
- **Testing**:
  - Optimized `ResultAssertions.GetFriendlyTypeName` with a zero-allocation fast-path for single-argument generics (e.g. `Result<T>`).
- **AspNetCore**:
  - `ResultEndpointFilter`: when `outcome.IsFailure == true` and `outcome.Error == null` (an invalid state impossible through public APIs but achievable via reflection or external `IResultOutcome` implementations), the filter now throws `InvalidOperationException` with a descriptive message instead of silently returning the raw object (which would produce a 200 OK response with the struct serialized as body).
  - `GetDescriptiveTitle(ErrorType.Validation)` now returns `"Validation Error"` instead of `"Bad Request"`. The previous value was identical to the canonical HTTP reason phrase, making the descriptive title redundant for this case. This affects `ProblemDetails.Title` when using a non-blank `TypeUriBase`.
- **MediatR**:
  - `ResultExceptionBehavior<TRequest, TResponse>` now carries `[RequiresDynamicCode]` and `[RequiresUnreferencedCode]` at the **class level** (not just on the private `BuildFailureFactory()` method). This ensures the trimmer and NativeAOT toolchain warn consumers at the correct call site (`Handle()`) rather than at the internal implementation detail.
- **FluentValidation**:
  - `ValidationResult.ToResult()` and `ValidationResult.ToResult<T>(T value)` renamed to `ToValidationResult()` and `ToValidationResult<T>(T value)`. The name `ToResult` created conceptual confusion with the error-obsoleted `Result<T>.ToResult()` method (which instructs users to use `DiscardValue()` instead). `ToValidationResult` is self-descriptive and eliminates the nominal ambiguity entirely.
- **OpenTelemetry**:
  - `ResultMetrics.GetAssemblyVersion()` now reads `AssemblyInformationalVersionAttribute` (a standard SDK-injected attribute present in every .NET assembly) instead of a custom `AssemblyMetadataAttribute` injected via an MSBuild `<ItemGroup>`. This eliminates the custom `<AssemblyAttribute>` ItemGroup from the `.csproj`, reduces the reflection surface to a single well-known framework type, and trims the SemVer build metadata suffix (`+abc123`) from the returned version string. The `[DynamicDependency]` + `[UnconditionalSuppressMessage]` safety pattern is preserved.
- **Analyzers**:
  - **RESULT004** (`ClosureCaptureCodeFix`): Added a second code fix action — **"Insert TState rewrite guidance comment"** — that inserts an inline comment above the flagged invocation showing the before/after TState refactor pattern using the actual method name. This resolves ARB audit finding API-02/REC-03: the previous "make static" code fix caused a compile error as a hint but gave no guidance on how to complete the rewrite. The new action provides the zero-allocation pattern template directly in the editor without breaking the build.

### Changed
- **OpenTelemetry**:
  - `ResultActivityExtensions` and `ResultActivityAsyncExtensions` signatures reordered for standard compatibility.
  - Added `ResultMetrics(Meter meter, bool disposeMeter)` overload.
- **Analyzers**:
  - **RESULT003** (`LargeResultValueAnalyzer`): Raised the struct size warning threshold from **24 bytes to 32 bytes**. The previous threshold of 24 bytes incorrectly flagged `Result<decimal>`, `Result<Guid>`, and `Result<DateTimeOffset>` (all ~25B) as "excessively large", which are common and appropriate use cases. The new 32-byte threshold suppresses noise for these types while still warning on genuinely large structs.
  - **RESULT004** (`ClosureCaptureAnalyzer`): The diagnostic message now includes the **names** of captured variables (e.g., `"Lambda in Map() captures 2 local variable(s) (userId, orderId)"`) instead of just the count. Additionally, closures that implicitly or explicitly capture `this` are now detected and included in the report.
- **API (Core)**:
  - TState overloads in `Task<Result<T>>` and `ValueTask<Result<T>>` extension methods are no longer marked `[EditorBrowsable(Advanced)]`. Previously, sync TState overloads (on `Result`/`Result<T>` directly) were visible in IDE IntelliSense while async TState overloads were hidden, creating an inconsistent API surface. All TState overloads are now uniformly visible to maximize discoverability of the allocation-free pipeline pattern.
- **Version bump**: `Directory.Build.props` `VersionSuffix` updated from `preview.2` → `preview.3`.

### Breaking Changes
- **Core**:
  - `Error.InnerErrors` return type changed from `IReadOnlyList<Error>` to `ImmutableArray<Error>`. The property now always returns a non-default `ImmutableArray<Error>` — never null, never a default struct. The dual backing-field design (`_innerErrors: IReadOnlyList` + `_innerErrorsImmutable: ImmutableArray?`) is eliminated; a single `ImmutableArray<Error>` field is used throughout, simplifying the field layout and delivering stronger type guarantees. **Migration**: replace `IReadOnlyList<Error>` type annotations with `ImmutableArray<Error>` or `var`; use `.Length` instead of `.Count`; use `.IsEmpty` instead of null checks. Indexer and `foreach` patterns are unchanged.

---

## [1.0.0-preview.2] - 2026-05-20

### Added
- **CI/Infrastructure**:
  - **NativeAOT smoke test** (`tests/EricksonLopez.Result.AotSmokeTest`) — 73-assertion console program published with `PublishAot=true` and `TreatWarningsAsErrors=true`. Any IL2026/IL3050 warning from the ILC trimmer fails the build. Integrated as a blocking PR gate in `ci.yml`.
  - **Mutation score CI gate** — `stryker-config.json` now specifies `break=95`, `low=99`, `high=100` thresholds. CI exits non-zero when the mutation score drops below 95%.

- **Analyzers**:
  - **RESULT003 CodeFixProvider** (`ErrorBuilderDiscardedReturnCodeFix`) — auto-fix for discarded `ErrorBuilder.With*()` return values. Corrects `builder.WithType(...)` → `builder = builder.WithType(...)` (simple case) and `GetBuilder().WithType(...)` → `var builder = GetBuilder().WithType(...)` (complex receiver). Supports fix-all-in-document via `BatchFixer`.

### Changed
- **Benchmark infrastructure**: Updated `BenchmarkDotNet` from `0.14.0` → `0.15.8`, added `RuntimeMoniker.Net10_0` job to all benchmark classes, added `net8.0` and `net10.0` to `TargetFrameworks`. `benchmarks.yml` CI workflow rewritten to install both SDKs, run with `--job short`, and commit results to `benchmarks/results/`.
- **Version bump**: `Directory.Build.props` `VersionSuffix` updated from `preview.1` → `preview.2`.
- **Stricter public API surface**: Removed `RS0016` and `RS0017` from global `NoWarn`. These `PublicApiAnalyzers` rules are now enforced as errors: undeclared public APIs and removed public APIs both fail the build.



---

## [1.0.0-preview.1] - 2026-04-15

### Added
- **Core (`EricksonLopez.Result`)**:
  - `readonly struct Result` and `readonly struct Result<TValue>` for zero-allocation Result envelope.
  - Immutability and explicit uninitialized state detection (`IsUninitialized`, `ResultState`).
  - Rich `Error` class supporting `ErrorType`, `ErrorSeverity`, `ErrorRetryability`, lazy `TraceId` (capturing ambient `ActivityTraceId`), `CorrelationId`, and immutable `Metadata`.
  - Fluent `ErrorBuilder` for constructing compound and contextual validation errors.
  - Closure-free monadic operators supporting `TState` state parameters (`Map`, `Bind`, `Tap`, `TapError`, `Match`, `Switch`, `Ensure`, `Recover`).
  - Async overloads for `Task<Result<T>>` and `ValueTask<Result<T>>`.
  - `Result.Combine` overload supporting up to 8 typed tuples and `ReadOnlySpan<Result>` backed by `ArrayPool<Error>`.
  - LINQ query syntax extension support (`Select`, `SelectMany`).

- **ASP.NET Core Integration (`EricksonLopez.Result.AspNetCore`)**:
  - Extension method `ToHttpResult()` mapping `Result` and `Result<T>` to RFC 9457 HTTP ProblemDetails.
  - `ResultEndpointFilter` for automatic Endpoint result unwrapping in ASP.NET Core Minimal APIs.
  - `ResultHttpOptions` for customizing HTTP status code mappings and ProblemDetails formatters.
  - NativeAOT `AspNetCoreJsonSerializerContext` source generator context.

- **Telemetry & Observability (`EricksonLopez.Result.OpenTelemetry`)**:
  - `ResultActivityExtensions` for recording result outcomes, error status, tags, and exceptions directly onto active OpenTelemetry `Activity` spans.
  - `ResultMetrics` for capturing metric counters (`result.outcomes.count`) and duration histograms (`result.execution.duration`).

- **JSON Serialization (`EricksonLopez.Result.Serialization`)**:
  - `ResultJsonConverterFactory` and `ErrorJsonConverter` for System.Text.Json serialization and deserialization.
  - `ResultJsonSerializerContext` for NativeAOT trim-safe JSON serialization.

- **Unit Testing Assertions (`EricksonLopez.Result.Testing`)**:
  - `ShouldBeSuccess()`, `ShouldBeFailure()`, `ShouldHaveError()`, `ShouldHaveErrorType()`, `ShouldHaveMetadata()` assertions for unit tests.
  - Custom `ResultAssertionException` with detailed failure diagnostics.

- **FluentValidation Integration (`EricksonLopez.Result.FluentValidation`)**:
  - `ToResult()` and `ToResult<T>()` extension methods converting `FluentValidation.ValidationResult` to structured `Result` failures.
  - `Validate()` and `ValidateToResult()` extensions on `IValidator<T>`.
  - `EnsureValid()` pipeline operator for validating values within a `Result<T>` chain.
  - Async variants: `ValidateAsync()`, `ValidateToResultAsync()`, `EnsureValidAsync()`.
  - FluentValidation `Severity` mapping to `ErrorSeverity`.
  - Rich metadata on each validation error: `propertyName`, `attemptedValue`, and placeholder values.

- **MediatR Pipeline Behavior (`EricksonLopez.Result.MediatR`)**:
  - `ResultExceptionBehavior<TRequest, TResponse>` pipeline behavior that catches unhandled exceptions and wraps them as `Result.Failure` with `ErrorType.Unexpected`.
  - `AddResultExceptionBehavior()` DI extension method with optional custom error factory.
  - Automatic pass-through for non-`Result` response types. `OperationCanceledException` is always re-thrown.

- **Roslyn Analyzers (`EricksonLopez.Result.Analyzers`)**:
  - `RESULT001`: Warning when `Result<T>` is used with a struct type exceeding 64 bytes.

  - `RESULT003`: Warning when `ErrorBuilder.With*()` return value is discarded (struct mutation lost).
  - Bundled automatically with the core `EricksonLopez.Result` package.

- **Serialization Source Generator (`EricksonLopez.Result.Serialization.Generators`)**:
  - Roslyn source generator producing AOT-compatible `ResultOfTJsonConverter<T>` implementations.
  - Eliminates reflection-based `MakeGenericType` / `Activator.CreateInstance` in NativeAOT scenarios.
  - Bundled as a dev dependency with `EricksonLopez.Result.Serialization`.
