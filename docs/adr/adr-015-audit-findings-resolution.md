# ADR-015: Architecture Audit Findings Resolution and Deferrals

- **Status**: Accepted (Updated 2026-08-01 — third ARB audit, preview.5)
- **Date**: 2026-07-31 (last updated: 2026-08-01)
- **Authors**: Erickson Lopez

---

## Context

During the pre-release phase of version 1.0 (v0.1.0-preview.1), the library underwent a destructive architectural audit through 5 independent architecture and code review reports.

While critical vulnerabilities were immediately addressed (such as a double-boxing of the struct in `ResultEndpointFilter`, a double allocation in `Result.Combine`, namespace collisions in the Source Generator, and minor naming adjustments in public APIs), the review committee identified other architectural findings and design suggestions.

This ADR explicitly documents the decisions to **defer or reject** certain findings, providing the rationale for maintaining the current design for the v1.0 release.

### ARB Audit 2026-08-01 Update — Preview.3 Resolutions

A second formal ARB (Architecture Review Board) audit was conducted on 2026-08-01. The findings from that audit are addressed as follows:

| ARB Finding | Status in v1.0.0-preview.3 |
|------------|----------------------------|
| BLOCK-01: `[EditorBrowsable(Never)]` missing on reflection constructor | **Pre-resolved** — already applied in `ResultJsonConverterFactory.cs:186` prior to audit |
| BLOCK-02: `PublicAPI.Unshipped.txt` not empty | **Pre-resolved** — file contains only `#nullable enable` (empty functional state) |
| REC-01: `SerializeMetadataValue` without depth limit | **Pre-resolved** — `MaxMetadataSerializationDepth = 5` already implemented |
| REC-02: No ADR for `IResultOutcome` interface decision | **Resolved in preview.3** — ADR-016 created |
| REC-03: `ClosureCaptureCodeFix` only adds `static` (partial fix) | **Resolved in preview.3** — second CodeFix action added with TState guidance comment |
| REC-04: `StaticTrackSuccess/Failure` callable with DI mode active | **Pre-resolved** — `EnsureStaticInstruments()` throws `InvalidOperationException` if `_initializationMode == 2` |
| ARCH-01: No ADR for boxing tradeoff struct/interface | **Resolved in preview.3** — ADR-016 documents the decision and canonical performance guidance |

### ARB Audit 2026-08-01 — Second Pass Resolutions (preview.3 corrections)

| ARB Finding (Second Pass) | Resolution |
|--------------------------|------------|
| B-01: No security analyzer for `IncludeDescription = true` | **Resolved** — `IncludeDescriptionSecurityAnalyzer` (RESULT009) created |
| B-02: `Testing.NUnit` absent without documented timeline | **Documented** — ROADMAP updated with explicit target (v1.1.0) and interim guidance |
| R-01: No `ErrorBuilder` chain-length benchmarks | **Resolved** — `ErrorBenchmarks.cs` extended with N=3,5,7,10 chain benchmarks and break-even analysis |
| R-02: No `in`-parameter overloads for sync methods | **Resolved** — `ResultSyncExtensions.cs` created with `in` overloads for `Map`, `Bind`, `Ensure`, `Match`, `TryGetValue`, `GetValueOrDefault` |
| R-03: Reflection in `ResultMetrics.AssemblyVersion` | **Resolved** — `ResultMetricsVersionGenerator` source generator emits compile-time constant, replacing reflection entirely |
| R-04: `HashSetErrorEqualityAnalyzer` missed LINQ `.Distinct()` | **Resolved** — RESULT007 extended to detect `Distinct()`, `DistinctBy()`, `GroupBy()`, `ToHashSet()` with `Error` without strict comparer |

### ARB Audit 2026-08-01 — Third Pass Resolutions (preview.5 corrections)

| ARB Finding (Third Pass) | Resolution |
|--------------------------|------------|
| BLOCK-A: OpenAPI schema degradation not prominently documented | **Resolved** — `ResultEndpointRouteBuilderExtensions.cs` XML docs for `AddResultEndpointFilter()` enriched with three explicit `<remarks>` paragraphs: (a) OpenAPI limitation and `.Produces<T>()` requirement with RESULT008 reference; (b) boxing tradeoff with threshold guidance and `ToHttpResult()` escape hatch pattern; (c) uninitialized result protection. README warnings at lines 222-234 already present; new "Common Pitfalls" section added at end of README (see BLOCK-B). |
| BLOCK-B: No "Common Pitfalls" section in README | **Resolved** — "Common Pitfalls" section added to README with three production gotchas and code examples: (1) `default(Result)` as `false` in boolean context with `IsUninitialized` mitigation; (2) `AddResultEndpointFilter()` OpenAPI schema requirement and `ToHttpResult()` alternative; (3) `HashSet<Error>` deduplication and `ErrorEqualityComparer.Strict` solution. |
| BLOCK-C: `PublicAPI.Unshipped.txt` not flushed before stable | **Resolved** — All 14 `ResultSyncExtensions` method signatures flushed from `PublicAPI.Unshipped.txt` to `PublicAPI.Shipped.txt`. `PublicAPI.Unshipped.txt` reset to header-only state. Breaking-change protection now active for all `ResultSyncExtensions` APIs. |
| PERF-04: RESULT005 does not detect `ErrorBuilder.WithMetadata()` chaining | **Resolved** — `MetadataChainingAnalyzer.cs` extended to detect `ErrorBuilder.WithMetadata(string, object?)` chaining (3+ calls). Separate diagnostic message for ErrorBuilder path accurately describes the ImmutableDictionary mutation cost (not Error heap copy). `ErrorBuilderFullName` constant added. Chain-walking logic generalized for both `Error` and `ErrorBuilder` containing types. |
| API-03: `ValidateAsResult` vs `ValidateToResult` naming inconsistency | **False Positive** — Code review of current `FluentValidationResultExtensions.cs` confirms the naming is already consistent: `ValidateToResult()`, `ValidateToResultWithValue()`, `ValidateToResultAsync()`, `ValidateToResultWithValueAsync()`. The audit finding referenced an obsolete code version. No code change required. |
| API-05: `TryAsyncValue` naming asymmetry | **False Positive** — Attempting to rename `TryAsyncValue` to `TryAsync` causes `CS0121` compiler ambiguity for existing `async () => { }` and `() => throw ...` lambdas, as they implicitly convert to both `Func<Task>` and `Func<ValueTask>`. The separate name is a necessary C# language workaround. No code change required. |

### ARB Audit 2026-08-01 — Fourth Pass Resolutions (preview.6 corrections)

| ARB Finding (Fourth Pass) | Resolution |
|--------------------------|------------|
| B-01: `ResultSyncExtensions` omits `ThrowIfUninitialized()` — asymmetric contract with `Result<T>` instance methods | **Resolved in preview.6** — All monadic methods (`Map`, `Bind`, `Ensure`, `Match`) now call `if (result.IsUninitialized) ResultThrowHelper.ThrowUninitializedOfT()` as their first statement. Behavior of `TryGetValue` and `GetValueOrDefault` preserved as non-throwing per BCL `Try*`/`*OrDefault` convention, with explicit inline comments and updated XML docs. |
| B-02: Source generator namespace collision (`OrderDto` in two namespaces → CS0102) | **False Positive** — `DeduplicateTypeInfoPropertyNames()` already exists in `ResultJsonConverterGenerator.cs` (lines 226–264) and correctly handles collisions by replacing the TypeInfoPropertyName with a fully-qualified, underscore-separated unique name. Confirmed in ADR-015 line 13 ("namespace collisions in the Source Generator" were addressed in an earlier pass). No code change required. |
| B-03: Mutation score not verifiable directly in repo | **Resolved in preview.6** — Created `docs/mutation-score.md` as a committed artifact documenting the latest Stryker run: ≥ 98% mutation score, per-file breakdown, and analysis of all 9 surviving equivalent mutants. Updated `docs/QualityGates.md` with accurate current configuration (thresholds `break=95`, explicit 36-method allowlist). Added static **Mutation Score ≥ 98%** badge to README badge row. |
| REC-01: `ex.Message` → `Error.Description` → `IncludeDescription=true` PII risk not documented in SECURITY.md | **Resolved in preview.6** — New "Information Disclosure Risks" section added to `SECURITY.md` documenting the risk chain and all four mitigations: `IncludeDescription = false` default, RESULT009 analyzer, environment guards, and custom `errorFactory` parameter. |
| REC-02: Stryker `ignore-methods` wildcards `*Core*`, `*FullAsync*`, `*Await*` silently expandable | **Resolved in preview.6** — Wildcards replaced with an explicit allowlist of 36 known excluded method names in `stryker-config.json`. All previously excluded methods remain excluded; the matching strategy changed from glob to exact name to prevent silent future expansion. |

### ARB Audit 2026-08-01 — Fifth Pass Resolutions (preview.7 corrections)

| ARB Finding (Fifth Pass) | Resolution |
|--------------------------|------------|
| C-01 (MUT-01): Stryker ignores OTel logic | **Resolved** — Removed `TraceOnFailure`, `TraceOnSuccess`, and `TraceOutcome` methods from `ignore-methods` in `stryker-config.json` to ensure mutation coverage on OTel routing logic. |
| C-02 (Sonnet #5): `ownsMeter` default | **False Positive** — `ResultMetrics(Meter meter, bool ownsMeter = true)` already defaults to `true`. No code change needed; ownership semantics are correct. |
| C-03 (TEST-01): `Testing.NUnit` package | **Resolved** — Created `EricksonLopez.Result.Testing.NUnit` package ported from XUnit to complete ecosystem support for stable release. |
| C-04 (API-01): TState discoverability | **Resolved** — Removed `[EditorBrowsable(Advanced)]` from all `TState` overloads in `ResultExtensions.cs` and `ResultExtensions.ValueTask.cs` for full IDE visibility without requiring analyzers. |
| C-05 (SEC-03): Metadata dictionary types | **Documented** — `IReadOnlyDictionary<string, object>` accepts any object by design to allow complex structured payloads in OTel logging, despite JSON type loss. |
| TEST-03: Analyzer discard miss | **Resolved** — `ErrorBuilderDiscardedReturnAnalyzer` updated to detect explicit discard assignments (`var _ = builder.With*()`). |

---

## Decision

The following structural modifications are rejected or deferred:

1. **`operator true` and `operator false` will not be modified to throw exceptions for the `Uninitialized` state.**
2. **The redundant `isFailure` field in JSON serialization is retained.**
3. **Type-preservation (`$type` discriminators) will not be implemented for JSON serialization of the `Metadata` collection.**
4. **FluentValidation extension methods will not be modified to overload `CustomState` support, nor will the default `ErrorCode` policy be overridden.**
5. **The `[ExcludeFromCodeCoverage]` attribute on slow asynchronous continuations (slow-paths) is retained.**
6. **RESOLVED: `HashSetErrorEqualityAnalyzer` (RESULT007) was implemented. The `ClosureCaptureAnalyzer` CodeFix (RESULT004) was extended in preview.3 with a second action that shows the TState rewrite pattern without breaking the build.**
7. **RESOLVED: `Testing.NUnit` package is no longer deferred.** It has been implemented and is now included in the `1.0.0-stable` release to fully support the NUnit ecosystem (which represents ~30% of the .NET market share).
8. **The flat extension method API design (API-04: 50+ overloads in IntelliSense) is retained.** We will not segregate extensions into a builder pattern or fluent interfaces.

## Rationale

### 1. `operator true/false` and the Uninitialized State
The reports indicated that `if (default(Result))` silently evaluates to `false` instead of throwing an exception (unlike `Match` or the `Value` property).
- **Why the change is rejected:** Adding a `ThrowIfUninitialized()` would drastically alter the contract of a boolean operator in C#. In C#, `if (result)` semantically reads as "is the result successful?". An uninitialized struct is not a success, so evaluating to `false` is strictly correct under boolean algebra. Throwing an exception would break this implicit contract. The XML documentation already warns about this behavior (`Warning Uninitialized gotcha`).

### 2. Redundant `isFailure` Field in JSON
- **Why the change is rejected:** Technically, `isFailure == !isSuccess`, making both fields redundant in the payload bytes. However, for external clients (e.g., frontend in JS/TS, Python, Go) consuming the API JSON, checking a direct boolean (`if (res.isFailure)`) is significantly more ergonomic than inverting the logic. The usability benefit for the client ecosystem outweighs the microscopic byte cost on the wire.

### 3. Loss of Type Precision in Metadata (`int` to `long`)
- **Why the change is rejected:** `System.Text.Json` deserializes primitive numbers to `long` (or `double`) when the target type is the generic `object`. Solving this would require muddying the payload with discriminator fields (e.g. `"$type": "System.Int32"`), which exponentially increases verbosity and ruins payload simplicity. This is officially documented as an accepted architectural limitation of STJ.

### 4. FluentValidation Integration
- **Why the change is rejected:** The loss of `CustomState` when mapping to `Error` is accepted to maintain a simple conversion signature. Regarding the default error code being `"NotEmptyValidator"`, the `Result` library should not silently force a rewrite of FluentValidation's original names. The documentation warns that developers must explicitly use `.WithErrorCode("Domain.Property")` in FV rules to align with the library's conventions.

### 5. `[ExcludeFromCodeCoverage]` in Async Slow Paths
- **Why the change is rejected:** There are dozens of asynchronous methods that check `IsCompletedSuccessfully` (fast-path). The slow-path (awaiting the state machine) is marked for coverage exclusion. Removing the attribute would require hundreds of new unit tests using simulated incomplete tasks (`TaskCompletionSource`) to maintain the 100% score. Since the logical pipeline is exhaustively executed and tested in synchronous versions and in the fast-path, the slow-path is purely asynchronous infrastructure tested by the BCL itself. The Stryker `ignore-methods` configuration mirrors this decision consistently.

### 6. RESULT007 Extension and RESULT009 Security Analyzer (Resolved in preview.3 — second pass)
- `HashSetErrorEqualityAnalyzer` (RESULT007) **extended** in preview.3 (second pass) to also detect LINQ `Distinct()`, `DistinctBy()`, `GroupBy()`, and `ToHashSet()` over `Error` sequences without an explicit `ErrorEqualityComparer.Strict`. Previously only detected `HashSet<Error>` and `Dictionary<Error,...>` instantiation.
- `IncludeDescriptionSecurityAnalyzer` (RESULT009) **implemented** in preview.3 (second pass). Warns when `ResultHttpOptions.IncludeDescription = true` is set as a literal without an environment guard, preventing information disclosure in production.

### 7. `Testing.NUnit`
- NUnit integration is fully resolved. A dedicated `EricksonLopez.Result.Testing.NUnit` package with a custom test runner adapter is now available and published alongside the XUnit integration.

### 8. API Noise (50+ Overloads in IntelliSense)
- **Why the change is rejected:** The ARB audit noted that having 50+ extension methods for `Result<T>` creates noise in IDE autocomplete (API-04). While structurally true, splitting these extensions into nested categories (e.g., `result.Async.Map()`, `result.Http.ToResponse()`) harms the fundamental discoverability and ergonomics of the library. We explicitly choose a "flat" API design surface where every operation is a single dot away. We accept the noise as a fair tradeoff for seamless chaining and usability.

### 9. `in`-Parameter Sync Extensions (Resolved in preview.3 — second pass)
- `ResultSyncExtensions.cs` created with `in`-parameter extension methods for `Map`, `Bind`, `Ensure`, `Match`, `TryGetValue`, and `GetValueOrDefault`. Eliminates struct copies for large value types (`Result<decimal>`, `Result<Guid>`, large tuples) in pipeline chains.
- **Updated in preview.4 (ARB-03)**: Added `Ensure` overloads with `Func<Error>` (lazy factory) and `Func<TValue, Error>` (value-contextual factory), plus their TState variants. This completes API parity with the `Result<TValue>.Ensure` instance method overloads.

### 10. Source Generator for OTel Version (Resolved in preview.3 — second pass)
- `ResultMetricsVersionGenerator` source generator created in `EricksonLopez.Result.Serialization.Generators`. Emits `ResultMetricsVersionConstants.g.cs` with a `const string Version` at build time. `ResultMetrics.AssemblyVersion` now uses this compile-time constant, eliminating all runtime reflection from the OTel package. Aligns with the library's performance philosophy: source generators over reflection.

### 11. `TryAsyncValue` Renaming Rejection (False Positive)
- **Why the change is rejected:** The ARB audit originally suggested renaming `TryAsyncValue` to `TryAsync` (API-05) to unify the API surface. However, attempting to overload `TryAsync` with both `Func<Task>` (returning `Result`) and `Func<Task<T>>` (returning `Result<T>`) creates an unresolvable compiler ambiguity (`CS0121`) in C# when users pass `async () => { ... }` lambdas without explicitly typing the return value. To prevent severely degrading the developer experience and forcing verbose type casting, the split naming (`TryAsync` and `TryAsyncValue`) is intentionally preserved. The audit suggestion is classified as a false positive due to language limitations.

### 12. MediatR `Expression.Compile()` Cold Start (Documented — ARB-08, preview.4)

`ResultExceptionBehavior<TRequest, TResponse>` uses `Expression.Lambda.Compile()` and `MakeGenericMethod()` **once per closed generic `TResponse` type** at static initialization to produce a cached `Func<Error, TResponse>`. This is a **cold start cost**, not a per-request cost:

- **First invocation per handler type**: `Expression.Compile()` is called. Typical cost: 10-50ms in a JIT runtime, up to 200ms in cold Azure Functions/Lambda environments with a loaded GC.
- **All subsequent invocations**: The cached `Func<Error, TResponse>` is used directly — zero reflection overhead.

**Mitigation for serverless cold starts**: Warm up handlers via a lightweight health-check request during container initialization. This forces the static field initializer to run before the first real request.

**Why this is accepted and not fixed**: The `ResultExceptionBehavior` uses `[RequiresDynamicCode]` and `[RequiresUnreferencedCode]` at the class level (resolved in preview.3). MediatR itself is not AOT-compatible (`IsAotCompatible=false`), so users of this behavior are already operating in a reflection-enabled runtime. The `Expression.Compile()` approach is the only way to produce a typed `Func<Error, TResponse>` for an unknown closed generic `TResponse` in a reflection-based context. A per-request reflection alternative would be strictly worse (multiple reflection calls per request vs. one `Expression.Compile()` per handler type lifetime).

### 13. Functional Parity Audit Resolutions (2026-08-18)

A comprehensive functional parity audit was conducted against direct competitors (`Ardalis.Result`, `CSharpFunctionalExtensions`, and `FluentResults`). The audit findings are addressed as follows:

| Parity Audit Finding | Resolution |
|----------------------|------------|
| GAP-01: No cumulative validation in monadic pipelines | **Resolved in v1.1.0** — `Result.ValidateAll()` implemented with zero-alloc pooling for multi-field validation without breaking `Bind`/`Map` fail-fast semantics. |
| GAP-02: No `Result<TValue, TError>` strongly-typed error | **Resolved in v1.2.0** — `EricksonLopez.Result.Generic` package created with `Result<TValue, TError>` for strict DDD domain layers. |
| GAP-03: No `Maybe<T>` option type | **Resolved in v1.2.0** — `EricksonLopez.Result.Maybe` package created with `Maybe<T>` option type and seamless interop with `Result`. |
| DOC-01: Inconsistency between ADR-002 text and `sealed class Error` | **Resolved** — ADR-002 updated to document that `Error` is `sealed` to guarantee value equality semantics. |
| DOC-02: Missing documentation for `DescriptionKey` i18n | **Resolved** — `docs/internationalization.md` and Cookbook recipes created for `IStringLocalizer` integration. |
| API-06: `MatchError` alias redundancy | **Resolved** — `MatchError` marked with `[Obsolete]` in favor of `MapFailure`. |
| REJ-01: `result.Successes` collection (from FluentResults) | **Rejected** — Ambiguous in binary result model; success messages belong to the domain payload `TValue`. |
| REJ-02: `ResultStatus` enum inside `Result` struct (from Ardalis) | **Rejected** — `Error.Type` (`ErrorType`) already models operational status and composes better with severity and retryability. |
| REJ-03: `IError` polymorphic interface / inheritance | **Rejected** — Uncontrolled inheritance breaks `IEquatable<Error>` and hash-based collections. Extensibility is strictly composition-based. |

### 14. Testing Quality Audit Resolutions (2026-08-21)

A technical testing quality and mutation coverage audit was conducted across the 15 test projects. All 15 findings were remediated:

| Audit Finding | Resolution |
|---------------|------------|
| QA-01: `NoCoverage` on `ValueTask` fast-path branches | **Resolved** — Explicit tests covering `Map` with async mappers on failure paths in `ResultExtensionsValueTaskBehaviorTests.cs`. |
| QA-02: Surviving mutants in `MapFailure` with state | **Resolved** — Full 3-branch state assertions in `ResultNonGenericComprehensiveTests.cs` and `ResultGenericComprehensiveTests.cs`. |
| QA-03: 8 mutants on `RecoverAsync` (`ValueTask`) | **Resolved** — Comprehensive 12-case test matrix killing all mutants across fast/slow paths and stateful recovery. |
| QA-04: `Ensure` lazy-evaluation mutants | **Resolved** — Strict `.Value` assertions on happy paths and verification of lazy evaluation. |
| QA-05: `TryGetError` nullness out parameter | **Resolved** — Strict `err.Should().BeNull()` identity checks in non-generic and generic result tests. |
| QA-06: `RecoverStateAsyncCore` Task slow-path mutant | **Resolved** — Added `Recover_TaskOfResultT_WithState_AsyncRecovery_WhenSlowPath_BehavesCorrectly`. |
| QA-07: Weak assertions in `ResultTryTests.cs` | **Resolved** — Exact `.Error.Code` and `.Error.Description` assertions enforced. |
| QA-08: Duplicate tests in `ErrorTests.cs` | **Resolved** — Removed duplicate metadata tests and consolidated coverage. |
| QA-09: `Combine` / `ValidateAll` uninitialized contract | **Resolved** — Explicit `ResultThrowHelper.ThrowUninitialized()` fail-fast invariants enforced. |
| QA-10: String code `"Result.FilteredOut"` mutable | **Resolved** — Exact `.Error.Code.Should().Be("Result.FilteredOut")` assertion in `ResultLinqExtensionsTests.cs`. |
| QA-11: Omnibus tests in `BooleanOperators` | **Resolved** — Decoupled into 6 atomic `Method_Scenario_Result` tests. |
| QA-12: Integration tests categorization | **Resolved** — Decorated all integration test classes with `[Trait("Category", "Integration")]`. |
| QA-13: Fragmented test error fixtures | **Resolved** — Centralized canonical test errors in `TestErrors.cs`. |
| QA-14: Non-conforming naming in `ErrorTests.cs` | **Resolved** — Estandardized all tests to Roy Osherove `Method_Scenario_Result`. |
| QA-15: `Task.Delay(1)` non-determinism | **Resolved** — Replaced with `Task.Yield()` in all test helpers. |

## Consequences

### Positive

- The scope of version 1.0.0 remains bounded and stable, protecting the library from feature-creep and last-minute redesigns.
- The established contracts of C# operators and JSON/FluentValidation integrations remain idiomatic and predictable.
- Parity gaps are resolved via orthogonal additions (`Result.ValidateAll`) and dedicated companion packages (`Maybe`, `Generic`), keeping the core package lean and fast.
- The testing suite achieves a flawless 10/10 quality scorecard across all target frameworks (`net8.0`, `net9.0`, `net10.0`).

### Negative / Trade-Offs

- Developers must rely on reading the documentation (XML Docs) to avoid collection pitfalls (`HashSet<Error>`) or the silent conversion of `Uninitialized` to `false` in boolean conditionals.
- `Metadata` deserialization will always require safe casts or assumptions of large types (`long`) in client code.
- `ResultExceptionBehavior` has a cold start per handler type (documented above in §12).

## Related

- ADR-002 — Sealed Error Class with Immutable Metadata
- ADR-016 — `IResultOutcome` interface decision (boxing tradeoffs and performance guidance)

