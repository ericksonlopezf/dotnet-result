# ADR-014: Obsolete(error: false) vs Obsolete(error: true) on reflection APIs with trimming risks

- **Status**: Accepted
- **Date**: 2026-07-31
- **Authors**: Erickson Lopez

---

## Context

`ResultOfTJsonConverter<T>()` (parameterless constructor) uses `JsonSerializerOptions` internally, which requires runtime reflection and prevents correct metadata preservation under aggressive trimming or NativeAOT. During the architectural audit (v1.x), it was evaluated whether to mark this constructor with `[Obsolete(error: true)]` to prevent its direct use.

## Decision

The parameterless constructor `ResultOfTJsonConverter<T>()` is preserved for reflection-based runtimes but is NOT marked with `[Obsolete]`. Instead, it is strictly guarded using standard .NET trimming and Native AOT runtime attributes:

1. **`[RequiresUnreferencedCode(...)]`**: Emits `IL2026` compile-time trimming warnings when trimming is enabled.
2. **`[RequiresDynamicCode(...)]`**: Emits `IL3050` compile-time AOT warnings when Native AOT is enabled.
3. **`[EditorBrowsable(EditorBrowsableState.Never)]`**: Hides the reflection-based constructor from general IDE IntelliSense discovery to steer developers toward `ResultOfTJsonConverter<T>(JsonTypeInfo<T>)`.

**Neither `[Obsolete(error: true)]` nor `[Obsolete(error: false)]` is used**, ensuring zero `CS0618`/`CS0619` compiler warnings while providing compile-time trim/AOT safety through the official .NET analyzer toolchain.

The distinction between both approaches is structural, not one of degree:

### `[Obsolete(error: true)]` generates `CS0619`

`CS0619` is a **static** compilation error:

- It acts during ordinary compile time, completely independent of the deployment context.
- **It is not suppressible** via `#pragma warning disable CS0619` or via `<NoWarn>` in the project file. Unlike `CS0618` (the equivalent warning), `CS0619` cannot be silenced in the Roslyn compiler without modifying the member declaration.
- It blocks reflection-based testing scenarios that are completely safe in non-trimmed `net8.0`/`net10.0` environments, failing to distinguish between a NativeAOT production consumer and a test runner on the full CLR.
- It provides no additional protection during `dotnet publish`, which is the phase where trimming and NativeAOT actually operate.

### `[RequiresUnreferencedCode]` + `[RequiresDynamicCode]` generate `IL2026` / `IL3050`

These attributes operate at the level of the **ILC linker and trimming analyzer**:

- The `IL2026` and `IL3050` diagnostics are understood by the publishing toolchain (`dotnet publish /p:PublishAot=true`, `TrimmerRootAssembly`, `SuppressTrimAnalysisWarnings`).
- The risk **propagates transitively** through the call graph: a consumer invoking this constructor from an unannotated method automatically inherits the diagnostic, pushing the warning to the correct system boundary.
- They surface in the publish phase, which constitutes the true deployment gate where the risk materializes.
- They integrate natively with standard ecosystem mechanisms: `[UnconditionalSuppressMessage]`, `[DynamicDependency]`, `ILLink.Substitutions.xml`.

### Conclusion

| Dimension | `Obsolete(error: true)` | `[RequiresUnreferencedCode/DynamicCode]` |
|---|---|---|
| Compilation gate | ✅ Static | ❌ Not applicable |
| AOT publish gate | ❌ Not applicable | ✅ `dotnet publish /p:PublishAot=true` |
| Transitive propagation | ❌ No | ✅ Across the full call graph |
| Suppressible in tests | ❌ Impossible | ✅ Via `[UnconditionalSuppressMessage]` |
| Trimmer toolchain integration | ❌ None | ✅ Native (`SuppressTrimAnalysisWarnings`, etc.) |
| Internal testability | ❌ Blocked | ✅ Preserved in non-trimmed runtimes |

For a library whose stated goal is to offer robust compatibility with trimming and NativeAOT, `[RequiresUnreferencedCode]` + `[RequiresDynamicCode]` provide considerably superior semantic and operational value: **they communicate and propagate a structural risk to the analysis system responsible for validating the final artifact**, rather than blocking the use of an API in a context that may be perfectly safe.

## Consequences

### Positive

- Internal tests validating the reflection path (`SerializationTests`, `ResultJsonConverterCoverageTests`, `AuditCorrectionTests`) compile without additional modifications on non-trimmed runtimes.
- Consumers publishing with `PublishAot=true` receive `IL2026`/`IL3050` at the correct point in their code, not at the library's declaration site.
- The choice is consistent with the design of the rest of the .NET ecosystem (e.g., `JsonSerializer.Deserialize<T>()`, `Activator.CreateInstance<T>()` use the same pattern).

### Negative / Trade-Offs

- Consumers who only read `CS0618` (IDE warning) without publishing with AOT might not perceive the real impact. The `[Obsolete]` warning mitigates this by being visible in IntelliSense and during ordinary compilation.

## Related

- `ResultJsonConverterFactory.cs` — rationale comment on the constructor declaration.
- ADR-005 — general NativeAOT serialization strategy.
- [IL2026 documentation](https://learn.microsoft.com/dotnet/core/deploying/trimming/trimming-options)
- [IL3050 documentation](https://learn.microsoft.com/dotnet/core/deploying/native-aot)
