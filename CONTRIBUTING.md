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
│   ├── EricksonLopez.Result/                        # Core struct-based Result & Error (+ bundled Analyzers)
│   ├── EricksonLopez.Result.Analyzers/              # Roslyn diagnostic analyzers & code fixes (RESULT001–009)
│   ├── EricksonLopez.Result.AspNetCore/             # HTTP ProblemDetails & Minimal API filter
│   ├── EricksonLopez.Result.FluentValidation/       # FluentValidation → Result conversion
│   ├── EricksonLopez.Result.MediatR/                # MediatR pipeline exception behavior
│   ├── EricksonLopez.Result.OpenTelemetry/          # Activity tracing & Metrics
│   ├── EricksonLopez.Result.Serialization/          # System.Text.Json converters
│   ├── EricksonLopez.Result.Serialization.Generators/ # Source generator for AOT-safe converters
│   ├── EricksonLopez.Result.Testing/                # Fluent test assertions (framework-agnostic)
│   ├── EricksonLopez.Result.Testing.NUnit/          # NUnit-specific assertion helpers
│   └── EricksonLopez.Result.Testing.XUnit/          # xUnit-specific assertion helpers
├── tests/
│   ├── EricksonLopez.Result.Tests/                  # Solution unit tests
│   └── EricksonLopez.Result.AotSmokeTest/           # NativeAOT publish validation
├── benchmarks/
│   └── EricksonLopez.Result.Benchmarks/             # BenchmarkDotNet performance benchmarks
├── docs/                                            # Architectural & usage documentation
│   └── decisions/                                   # Architectural Decision Records (ADR-001–016)
└── .github/
    └── workflows/                                   # CI, build, publish, AOT, mutation, benchmarks
```

### Building the Project

Run the following command from the repository root:

```bash
dotnet build EricksonLopez.Result.slnx
```

### Running Tests

Execute unit tests across all frameworks:

```bash
dotnet test EricksonLopez.Result.slnx --configuration Release
```

---

## 🔑 Strong Name Signing

All assemblies are strong-name signed in CI using a private key stored as the GitHub secret `SNK_KEY`.

> [!NOTE]
> A `public.snk` file for consumer verification is planned but not yet committed. If your use case requires strong name verification, contact the maintainer.

### For contributors (local builds)

You do **not** need the private key to build locally. The `Directory.Build.props` signing is conditional:

```xml
<SignAssembly Condition="Exists('$(MSBuildThisFileDirectory)EricksonLopez.Result.snk')">true</SignAssembly>
<AssemblyOriginatorKeyFile Condition="Exists('$(MSBuildThisFileDirectory)EricksonLopez.Result.snk')">
  $(MSBuildThisFileDirectory)EricksonLopez.Result.snk
</AssemblyOriginatorKeyFile>
```

If `EricksonLopez.Result.snk` is not present (which it won't be for contributors), the build succeeds without signing.

### For maintainers: generating or rotating the key pair

```powershell
# 1. Generate a new key pair (contains BOTH public and private key)
sn -k EricksonLopez.Result.snk

# 2. Extract only the public key (safe to commit to the repo)
sn -p EricksonLopez.Result.snk public.snk

# 3. Base64-encode the full key pair for the GitHub Secret:
[Convert]::ToBase64String([IO.File]::ReadAllBytes('EricksonLopez.Result.snk')) | clip
# Paste the clipboard contents into the GitHub repository Secret named 'SNK_KEY'.

# 4. Verify the signing after a build:
sn -vf src/EricksonLopez.Result/bin/Release/net8.0/EricksonLopez.Result.dll
```

> [!CAUTION]
> **Never commit the private `.snk` file** (`EricksonLopez.Result.snk`) to the repository.
> It is listed in `.gitignore`. Only the public key file (`public.snk`) is safe to commit.

> [!NOTE]
> `sn.exe` is included in the Visual Studio Developer Command Prompt and in the .NET SDK tools.
> On non-Windows, use `sn` from the Mono toolchain or cross-compile on Windows.

---

## 💡 How to Contribute


### 1. Reporting Bugs

Before creating a bug report, check the [issue tracker](https://github.com/ericksonlopez/dotnet-result/issues) to see if the issue has already been reported.

When creating a bug report, please include:
- A clear, descriptive title.
- Steps to reproduce the bug.
- Target framework version (.NET 8, .NET 10).
- Expected vs. actual behavior.
- Code snippets or minimal reproduction samples.

### 2. Suggesting Features

We welcome feature proposals! Before submitting a PR for a major feature, please open a [Discussion](https://github.com/ericksonlopez/dotnet-result/discussions) or issue to discuss the design first.

### 3. Pull Request Guidelines

1. **Fork & Branch**: Create a feature branch off of `develop` (e.g., `feat/add-analyzer` or `fix/error-traceid`).
2. **Coding Standards**:
   - Follow standard C# code formatting conventions (enforced via `.editorconfig`).
   - All public APIs must have XML documentation comments (`/// <summary>`).
   - Prefer `readonly struct` for performance value types.
   - Maintain zero heap allocation guarantees for happy paths and `TState` monadic operators.
   - **Strong Name Signing**: All assemblies are signed. New projects must include the `EricksonLopez.Result.snk` key file in the build path.
3. **Commit Message Format (Conventional Commits — required)**: 
   This repository uses [Conventional Commits](https://www.conventionalcommits.org) to automate SemVer bumps and CHANGELOG generation via Release Please. Every commit to `main` or `develop` **must** follow this format:

   ```
   <type>(<scope>): <short description>
   ```

   | Type | Effect on version | Example |
   |------|------------------|---------|
   | `feat` | MINOR bump | `feat(core): add ErrorBuilder.WithCorrelationId()` |
   | `fix` | PATCH bump | `fix(aspnetcore): correct freeze guard in ResultHttpOptions` |
   | `feat!` or `BREAKING CHANGE:` | MAJOR bump | `feat!: remove deprecated Result.Switch()` |
   | `perf` | PATCH bump | `perf(core): avoid allocating Error[] in Combine()` |
   | `docs` | no bump | `docs: update ADR-016 with C# 13 note` |
   | `chore` | no bump | `chore(deps): bump FluentValidation from 11.9 to 11.11` |
   | `refactor` | no bump | `refactor(serialization): simplify converter factory` |
   | `test` | no bump | `test: add freeze guard post-first-request assertions` |

   > [!IMPORTANT]
   > PRs with non-conventional commit messages will fail the Release Please automation. If in doubt, use `fix:` for bugs or `feat:` for additions.

4. **Add Tests**: 
   - Ensure all new functionality or bug fixes have corresponding unit tests in `EricksonLopez.Result.Tests`.
   - **Test Comments**: Do not use `// Arrange`, `// Act`, `// Assert`, or `/// <summary>` in test methods. Test names must be self-descriptive.
   - **Regression Comments**: Any test documenting a bug fix or regression must include a comment starting with `// Regression (Issue #XXX):` linking to the specific GitHub issue or PR.
5. **Run Verification**: Ensure `dotnet test` passes without warnings before submitting.


---

## 📄 License

By contributing to this repository, you agree that your contributions will be licensed under the project's [MIT License](LICENSE).
