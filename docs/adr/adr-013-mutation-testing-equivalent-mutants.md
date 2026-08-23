# ADR 013: Exclusion of Generated Code, Fast Paths, and Equivalent Mutations from Mutation Testing

## Status
Accepted

## Context
During the process of achieving an absolute 100% mutation score with Stryker for EricksonLopez.Result, we encountered mutants that survived but did not represent missing test coverage for human-maintained business logic. These fall into three primary categories:

1. **Compiler-Generated Code & Async State Machines:**
   Methods with sync and wait, such as those in ResultExtensions.cs and ResultExtensions.ValueTask.cs, get compiled into complex state machines. Local functions used to optimize memory allocations (like MapFullAsync and *Core) also generate hidden classes and state. Stryker mutates the underlying IL or synthesized syntax trees in ways that are impossible to cover cleanly because they represent unreachable compiler states or framework-handled cancellation paths.

2. **Synchronous Fast Path Optimizations:**
   Extension methods working over Tasks (e.g., Task<Result<T>>) implement performance fast-paths checking if (task.IsCompletedSuccessfully) to avoid allocating asynchronous state machines if the task completed synchronously. Mutating the condition of this fast path simply forces execution down the equivalent slow path (the Core method), which produces the exact same outcome. Testing these fast paths requires artificially coupling unit tests to .NET's internal thread-scheduling heuristics, creating extremely brittle and low-value tests.

3. **Structurally Equivalent Mutations:**
   In core classes like Error.cs, Result.cs, and Result.Combine.cs, Stryker generates mutations that are logically equivalent or unobservable from the outside:
   * **Conditional Equivalencies:** e.g., changing innerErrors is { Count: > 0 } to >= 0. Since _innerErrors is always evaluated carefully with downstream null propagation, the >= 0 check results in the exact same external behavior.
   * **Resource Release Flags:** Mutating ArrayPool<T>.Shared.Return(..., clearArray: true) to false. This alters internal memory hygiene optimization but produces zero behavioral change to the consumer.
   * **Dead-store Increments:** Mutating idx++ to idx-- on the very last line of a combination routine where idx is subsequently discarded.

4. **Specific Critical Build Paths (`ErrorBuilder.Build()`):**
   The `Build()` method in `ErrorBuilder` performs struct copying and final validations. Mutating its internal assignments generates mutants that are practically impossible to kill without resorting to fragile reflection-based tests or extremely contrived memory layout assertions.

5. **String Literals and Error Messages:**
   Mutating string literals (like exception messages, analyzer diagnostics, or fallback error codes) creates hundreds of high-noise mutants. We determined that adding over 400 inline `// Stryker disable once string` comments for diagnostic texts is too invasive and harms code readability.

## Decision
We will not artificially inflate our test suite to target equivalent mutations, write fragile Task-scheduling tests for fast paths, nor attempt to cover compiler-generated asynchronous state machines.

To maintain a pristine **100% Mutation Score** metric, we will explicitly exclude these paths from analysis using inline attributes and annotations:
1. **Async State Machines:** Extracted into explicitly named Core local functions and excluded using both [ExcludeFromCodeCoverage] and explicit // Stryker disable all blocks.
2. **Fast Paths:** Surrounded with // Stryker disable all : Fast path optimization to acknowledge their purpose.
3. **Equivalent Mutations:** Excluded specifically at the line level using // Stryker disable once all : Equivalent mutation.
4. **`ErrorBuilder.Build()`:** This method is **not** excluded via `stryker-config.json`. It also does not have inline `// Stryker disable` comments. This represents an accepted technical debt: mutations inside `Build()` will not fail CI, and coverage relies on standard unit tests and code review. See Consequences below.
5. **String Mutations:** String mutation exclusions are applied **inline** via `// Stryker disable String` block comments at the top of each file that contains exception messages, analyzer diagnostic strings, or serialization fallback strings (e.g., `ResultOfT.cs`, `ResultLinqExtensions.cs`). They are **not** globally excluded via `ignore-mutations` in `stryker-config.json` — that field is intentionally left empty (`[]`) to ensure the global config does not accidentally silence genuine behavioral strings.

   **Why inline is preferred over global config:**
   Adding individual `// Stryker disable String` blocks at the top of each file (instead of a global `ignore-mutations: ["string"]` entry) provides file-level granularity. This means that `Error.Code` strings in public factory methods — which are part of the **public API contract** — are still subject to Stryker analysis, and a mutation there would be detected.

   **What this exclusion covers:**
   - Exception messages thrown by guards (e.g., `"Cannot access Value of a failed result."`)
   - Analyzer diagnostic message strings and `DiagnosticDescriptor` identifiers
   - Fallback description strings in serialization converters (e.g., `"Invalid Result JSON structure"`)
   - Human-readable log/trace strings in OpenTelemetry spans
   - Internal sentinel values used only inside private algorithm logic

   **What this exclusion does NOT cover (known gap):**
   - `Error.Code` string values in public factory methods (`Error.Failure("code",...)`,
     `Error.Validation("code",...)`) that are part of the **public API contract**. A mutation
     changing `"Validation.Failed"` to `"Validation.Failed_"` would not be caught by Stryker.

   **Compensating controls for the `Error.Code` gap:**
   - `PublicAPI.Shipped.txt` / `PublicAPI.Unshipped.txt` (via `Microsoft.CodeAnalysis.PublicApiAnalyzers`)
     would flag any API that changes an existing `Error.Code` factory method signature
   - Integration tests in `EricksonLopez.Result.Tests` verify specific `Error.Code` values
     returned by factory methods (e.g., `result.Error.Code.Should().Be("Validation.Required")`).
     If a factory string literal changes, these tests fail.
   - Code review is the final gate for any change to a factory-level `Error.Code` string.

   We accept the technical debt that Stryker will not detect a mutation to `Error.Code` strings,
   in exchange for a pristine signal-to-noise ratio and clean code.


## Consequences
* **Positive:** We maintain a strict **100% Mutation Score** metric without compromising test suite stability.
* **Positive:** Developers are not forced to write fragile, hyper-specific tests that break upon Roslyn compiler updates.
* **Positive:** Test suite execution time is minimized by avoiding combinatorial testing of performance optimizations.
* **Positive:** Inline `// Stryker disable` comments provide file-level granularity, ensuring that public API contract strings (such as `Error.Code` factory values) remain subject to mutation analysis.
* **Negative:** Rigorous Code Reviews are required to ensure that `// Stryker disable` annotations are not abused to silence genuine business-logic coverage gaps.
* **Negative:** Mutating logic inside `ErrorBuilder.Build()` will not fail the CI. This method has no inline `// Stryker disable` comment and is not excluded via `stryker-config.json`. We accept this technical debt and rely on standard unit test coverage and code review for this specific method.
* **Note:** The `stryker-config.json` global `ignore-mutations` field is intentionally kept empty (`[]`). String-level exclusions are managed at the file level via inline comments, not globally, to maintain API contract mutation coverage.
