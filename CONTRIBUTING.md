# Contributing to EricksonLopez.Result

Thank you for your interest in contributing to `EricksonLopez.Result`! We welcome contributions, bug reports, documentation enhancements, and feature suggestions.

---

## 📜 Code of Conduct

All contributors are expected to adhere to our [Code of Conduct](CODE_OF_CONDUCT.md). Please read it before participating.

---

## 🛠️ Development Setup

### Prerequisites

- **.NET 8.0 SDK**, **.NET 9.0 SDK**, or **.NET 10.0 SDK** (recommended: latest)
- Git
- An IDE such as JetBrains Rider, Visual Studio 2022+, or VS Code with C# Dev Kit.

### Repository Structure

```
dotnet-result/
├── src/
│   ├── EricksonLopez.Result/                         # Core struct-based Result & Error (+ bundled Analyzers)
│   ├── EricksonLopez.Result.Generic/                 # Strongly-typed Result<TValue, TError>
│   ├── EricksonLopez.Result.Maybe/                   # Struct-based Maybe<T> option type
│   ├── EricksonLopez.Result.AspNetCore/              # HTTP ProblemDetails & Minimal API filter
│   ├── EricksonLopez.Result.OpenApi/                 # Minimal API OpenAPI metadata extensions
│   ├── EricksonLopez.Result.FluentValidation/        # FluentValidation → Result conversion
│   ├── EricksonLopez.Result.MediatR/                 # MediatR pipeline exception behavior
│   ├── EricksonLopez.Result.OpenTelemetry/           # Activity tracing & BCL Metrics
│   ├── EricksonLopez.Result.Serialization/           # System.Text.Json converters
│   ├── EricksonLopez.Result.Serialization.Generators/# Source generator for AOT-safe converters
│   ├── EricksonLopez.Result.Analyzers/               # Roslyn diagnostic analyzers & code fixes
│   ├── EricksonLopez.Result.Testing/                 # Fluent test assertions (framework-agnostic)
│   ├── EricksonLopez.Result.Testing.XUnit/           # xUnit-specific assertion helpers
│   └── EricksonLopez.Result.Testing.NUnit/           # NUnit-specific assertion helpers
├── tests/
│   ├── EricksonLopez.Result.Tests/                   # Core solution unit tests
│   ├── EricksonLopez.Result.Generic.Tests/           # Generic Result unit tests
│   ├── EricksonLopez.Result.Maybe.Tests/             # Maybe<T> unit tests
│   ├── EricksonLopez.Result.AspNetCore.Tests/        # ASP.NET Core integration tests
│   ├── EricksonLopez.Result.OpenApi.Tests/           # OpenAPI integration tests
│   ├── EricksonLopez.Result.FluentValidation.Tests/  # FluentValidation tests
│   ├── EricksonLopez.Result.MediatR.Tests/           # MediatR behavior tests
│   ├── EricksonLopez.Result.OpenTelemetry.Tests/     # Activity & Metrics tests
│   ├── EricksonLopez.Result.Serialization.Tests/     # JSON converter tests
│   ├── EricksonLopez.Result.Serialization.Generators.Tests/ # Source generator tests
│   ├── EricksonLopez.Result.Analyzers.Tests/         # Roslyn analyzer verification tests
│   ├── EricksonLopez.Result.Testing.Tests/           # Test assertions unit tests
│   ├── EricksonLopez.Result.Testing.XUnit.Tests/     # xUnit assertion tests
│   ├── EricksonLopez.Result.Testing.NUnit.Tests/     # NUnit assertion tests
│   └── EricksonLopez.Result.AotSmokeTest/            # NativeAOT publish smoke test app
├── samples/
│   ├── EricksonLopez.Result.Sample/                  # Comprehensive usage samples
│   ├── EricksonLopez.Result.AspNetCore.Sample/       # Minimal API sample
│   ├── EricksonLopez.Result.FluentValidation.Sample/ # Validation pipeline sample
│   ├── EricksonLopez.Result.MediatR.Sample/          # MediatR CQRS sample
│   ├── EricksonLopez.Result.OpenTelemetry.Sample/    # Distributed tracing sample
│   └── EricksonLopez.Result.Serialization.Sample/    # JSON serialization sample
├── benchmarks/
│   └── EricksonLopez.Result.Benchmarks/              # BenchmarkDotNet performance benchmarks
├── docs/                                             # Technical architecture & guides
│   ├── adr/                                          # Architectural Decision Records (ADR-001–021)
│   ├── analysis/                                     # Allocation & memory analysis
│   └── community/                                    # Community discussion templates
└── .github/
    └── workflows/                                    # CI, build, publish, AOT, mutation, benchmarks
```

### Building the Project

Run the following command from the repository root:

```bash
dotnet build EricksonLopez.Result.slnx
```

### Running Tests

Execute all unit and integration tests:

```bash
dotnet test EricksonLopez.Result.slnx --configuration Release
```

### Running Mutation Tests (Local)

Mutation testing runs Stryker.NET against the core package. Execute from the repository root:

```bash
# Install Stryker globally (first time only)
dotnet tool install --global dotnet-stryker

# Run Stryker against the core package (config loaded from stryker-config.json at root)
dotnet stryker \
  --project EricksonLopez.Result.csproj \
  --test-project tests/EricksonLopez.Result.Tests/EricksonLopez.Result.Tests.csproj \
  --config-file stryker-config.json
```

> **Note:** Full mutation runs can take 60–90 minutes. Stryker exits with code 1 if the mutation score drops below `break: 95`.

### Running NativeAOT Smoke Tests

The AOT smoke test validates that all packages remain NativeAOT-compatible:

```bash
# Requires .NET 8 SDK and clang/lld on Linux
dotnet publish tests/EricksonLopez.Result.AotSmokeTest/EricksonLopez.Result.AotSmokeTest.csproj \
  --configuration Release \
  --runtime linux-x64 \
  --self-contained \
  -p:PublishAot=true \
  -p:TreatWarningsAsErrors=true
```

### Running Benchmarks (Local)

Benchmarks are run via BenchmarkDotNet. Run from the repository root:

```bash
dotnet run --project benchmarks/EricksonLopez.Result.Benchmarks/EricksonLopez.Result.Benchmarks.csproj \
  --configuration Release --framework net10.0 -- \
  --filter "*" --job short --runtimes net8.0 net10.0
```

> Benchmark results are committed to `benchmarks/results/` for regression tracking.

---

## 🔑 Strong Name Signing

All assemblies are strong-name signed in CI using a private key stored as the GitHub secret `SNK_KEY`.

### For contributors (local builds)

You do **not** need the private key to build locally. The `Directory.Build.props` signing is conditional:

```xml
<SignAssembly Condition="Exists('$(MSBuildThisFileDirectory)EricksonLopez.Result.snk')">true</SignAssembly>
```

If `EricksonLopez.Result.snk` is not present (which it won't be in external contributor checkouts), the build succeeds without signing.

---

## 💡 How to Contribute

### 1. Reporting Bugs

Before creating a bug report, check the [issue tracker](https://github.com/ericksonlopezf/dotnet-result/issues). When reporting a bug, please provide:
- A clear, descriptive title.
- Steps to reproduce.
- Target framework version (.NET 8, .NET 9, .NET 10).
- Expected vs actual behavior.
- Minimal reproduction sample.

### 2. Suggesting Features

Before submitting a PR for a major feature or new package, please open a [Discussion](https://github.com/ericksonlopezf/dotnet-result/discussions) to discuss the design first.

### 3. Pull Request Guidelines

1. **Fork & Branch**: Create a feature branch off of `develop` (e.g., `feat/add-analyzer` or `fix/error-traceid`).
2. **Coding Standards**:
   - Follow standard C# code formatting conventions (enforced via `.editorconfig`).
   - All public APIs must include XML documentation comments (`/// <summary>`).
   - Prefer `readonly struct` for performance value types.
   - Maintain zero heap allocation guarantees for happy paths and `TState` monadic operators.
3. **Commit Message Format (Conventional Commits — required)**:
   This repository uses [Conventional Commits](https://www.conventionalcommits.org) to automate SemVer bumps and CHANGELOG generation via Release Please. Every commit **must** follow this format:

   ```
   <type>(<scope>): <short description>
   ```

   | Type | Effect on version | Example |
   |---|---|---|
   | `feat` | MINOR bump | `feat(core): add ErrorBuilder.WithCorrelationId()` |
   | `fix` | PATCH bump | `fix(aspnetcore): correct freeze guard in ResultHttpOptions` |
   | `feat!` or `BREAKING CHANGE:` | MAJOR bump | `feat!: remove deprecated Result.Switch()` |
   | `perf` | PATCH bump | `perf(core): avoid allocating Error[] in Combine()` |
   | `docs` | no bump | `docs: update ADR-016 with C# 13 note` |
   | `chore` | no bump | `chore(deps): bump FluentValidation from 11.9 to 11.11` |
   | `refactor` | no bump | `refactor(serialization): simplify converter factory` |
   | `test` | no bump | `test: add freeze guard post-first-request assertions` |

4. **Test Conventions (Roy Osherove Pattern — ADR-017)**:
   - Ensure all new functionality or bug fixes have corresponding unit tests.
   - Adopt the `Method_Scenario_Result` naming convention (e.g., `Create_WhenInvalidInput_ReturnsValidationFailure`).
   - Do not include `// Arrange`, `// Act`, `// Assert` or `/// <summary>` in test methods.
   - Document bug fixes with `// Regression (Issue #XXX):`.

---

## 📄 License

By contributing to this repository, you agree that your contributions will be licensed under the project's [MIT License](LICENSE).
