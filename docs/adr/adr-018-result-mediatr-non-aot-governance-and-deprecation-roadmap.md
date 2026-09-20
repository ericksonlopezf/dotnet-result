# ADR-018: Result.MediatR Legacy Status, Non-AOT Governance, and Staged Deprecation Roadmap

## Status
Accepted — August 2026

## Date
2026-09-04

## Context
`EricksonLopez.Result.MediatR` provides MediatR pipeline behaviors integrated with `EricksonLopez.Result`. 

Due to MediatR's runtime reflection and dynamic type construction (`MakeGenericMethod`), this adapter cannot satisfy Native AOT and Trimming requirements.

With the introduction of [`EricksonLopez.Mediator`](https://github.com/ericksonlopezf/dotnet-mediator) (source-generated compile-time CQRS) and [`EricksonLopez.Mediator.Result`](https://github.com/ericksonlopezf/dotnet-mediator), the ecosystem possesses a fully AOT-compliant, zero-allocation alternative.

## Decision

### 1. Explicit AOT/Trimming Metadata
- Enforce `<IsAotCompatible>false</IsAotCompatible>` and `<IsTrimmable>false</IsTrimmable>` in `EricksonLopez.Result.MediatR.csproj`.
- Suppress trim analyzer warnings in this legacy project to keep the repository build warning-free while clearly signaling non-AOT support.

### 2. Staged Deprecation Schedule

> **Note:** The current release milestone of the `EricksonLopez.Result` ecosystem is **v3.0.0** (see `Directory.Build.props`). During v3.0.0, MediatR integration continues to be maintained as an explicit `[INTEROP / COMPATIBILITY BRIDGE]` with `<IsAotCompatible>false</IsAotCompatible>`, with the formal `[Obsolete]` compiler diagnostic scheduled to provide an orderly transition window without prematurely breaking consumer builds enforcing `TreatWarningsAsErrors=true`.

| Version | Status | Architectural Action |
|---|---|---|
| **v1.x** | Legacy Supported | Maintained for bugfixes; explicitly marked as non-AOT. Documentation recommends migration to `EricksonLopez.Mediator`. |
| **v2.0.0** | Interop Bridge | Packaged as `[INTEROP / COMPATIBILITY BRIDGE]`. Non-AOT/Trimming metadata explicitly declared (`IsAotCompatible=false`). Deprecation announced in package description, documentation, and migration guides. |
| **v3.0.0 (Current)** | Interop Bridge / Staged | Maintained as `[INTEROP / COMPATIBILITY BRIDGE]`. Package deprecation active. Full EOL package removal deferred to allow consumers migration time without compounding v3.0 monadic breaking changes. |
| **v4.0.0 (Future)** | End of Life / Removal | Package officially removed from supported ecosystem releases. |

### 3. Canonical Obsolete Signature (Staged)
```csharp
[Obsolete(
    "EricksonLopez.Result.MediatR is deprecated and will be removed in a future major version. " +
    "Migrate to EricksonLopez.Mediator.",
    DiagnosticId = "ELMED001",
    UrlFormat = "https://docs.ericksonlopez.dev/migration/{0}")]
```

### 4. Implementation Status per Version (Audit Resolution)
- **AOT & Trimming Isolation**: Fully implemented in `EricksonLopez.Result.MediatR.csproj` via `<IsAotCompatible>false</IsAotCompatible>` and `<IsTrimmable>false</IsTrimmable>`.
- **Package Role**: Formally designated as `[INTEROP / COMPATIBILITY BRIDGE]` in package metadata and documentation.
- **`[Obsolete]` Compiler Warning**: Deferred from initial major releases to allow consumers a deprecation notice period via package description and migration documentation prior to emitting warnings under `TreatWarningsAsErrors=true`. Scheduled for application with canonical signature `ELMED001`.

### 5. Addendum (v3.0.0 Update)
- During the v3.0.0 release cycle, `EricksonLopez.Result.MediatR` was retained as an `[INTEROP / COMPATIBILITY BRIDGE]` to ensure non-AOT legacy consumers remain functional while transitioning to `EricksonLopez.Mediator`. Complete removal has been rescheduled to a future major milestone (v4.0.0).

## Consequences
- **Developer Experience**: Consumers receive a clear, predictable migration window without unexpected build breakage.
- **Architectural Coherence**: Avoids maintaining two parallel in-process mediator abstractions indefinitely.
