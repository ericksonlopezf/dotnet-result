# ADR-018: Result.MediatR Legacy Status, Non-AOT Governance, and Staged Deprecation Roadmap

## Status
Accepted — August 2026

## Context
`EricksonLopez.Result.MediatR` provides MediatR pipeline behaviors integrated with `EricksonLopez.Result`. 

Due to MediatR's runtime reflection and dynamic type construction (`MakeGenericMethod`), this adapter cannot satisfy Native AOT and Trimming requirements.

With the introduction of [`EricksonLopez.Mediator`](https://github.com/ericksonlopez/dotnet-mediator) (source-generated compile-time CQRS) and [`EricksonLopez.Mediator.Result`](https://github.com/ericksonlopez/dotnet-mediator), the ecosystem possesses a fully AOT-compliant, zero-allocation alternative.

## Decision

### 1. Explicit AOT/Trimming Metadata
- Enforce `<IsAotCompatible>false</IsAotCompatible>` and `<IsTrimmable>false</IsTrimmable>` in `EricksonLopez.Result.MediatR.csproj`.
- Suppress trim analyzer warnings in this legacy project to keep the repository build warning-free while clearly signaling non-AOT support.

### 2. Staged Deprecation Schedule

> **Note:** The current release of the `EricksonLopez.Result` ecosystem is **v1.1.0** (see `Directory.Build.props`). The schedule below is calibrated against the actual current version, not a future planned major.

| Version | Status | Architectural Action |
|---|---|---|
| **v1.x (Current — v1.1.0)** | Legacy Supported | Maintained for bugfixes; explicitly marked as non-AOT. Documentation recommends migration to `EricksonLopez.Mediator`. |
| **v1.x → v2.0 Migration Window** | Final Migration Window | Enhanced migration guides and documentation warnings. No compile-time `[Obsolete]` to avoid noise in existing stable builds. |
| **v2.0.0** | Deprecated (`[Obsolete]`) | Public APIs decorated with `[Obsolete]` attribute (`IsError = false`, `DiagnosticId = "ELMED001"`). Non-breaking compiler warning. |
| **v3.0.0** | End of Life / Removal | Package officially removed from supported ecosystem releases (breaking change). |

### 3. Canonical Obsolete Signature (v2.0.0)
```csharp
[Obsolete(
    "EricksonLopez.Result.MediatR is deprecated and will be removed in v3.0. " +
    "Migrate to EricksonLopez.Mediator.",
    DiagnosticId = "ELMED001",
    UrlFormat = "https://docs.ericksonlopez.dev/migration/{0}")]
```

## Consequences
- **Developer Experience**: Consumers receive a clear, predictable migration window without unexpected build breakage.
- **Architectural Coherence**: Avoids maintaining two parallel in-process mediator abstractions indefinitely.
