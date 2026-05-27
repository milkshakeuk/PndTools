# Implementation Plan: PndTools.AspNetCore Integration Package

**Branch**: `002-aspnetcore-integration` | **Date**: 2026-05-27 | **Spec**: [spec.md]

**Input**: Feature specification from `specs/002-aspnetcore-integration/spec.md`

## Summary

Add `IPxmlParser` and `IPxmlValidator` interfaces to the core `PndTools` library, refactor
`PxmlParser` from a static class to an instance class implementing `IPxmlParser`, and create a new
`PndTools.AspNetCore` NuGet package that registers all three PndTools services (including
`IPndArchiveFactory`) as Singletons via a single `AddPndTools()` extension method on
`IServiceCollection`. The companion package follows Microsoft's extension-package conventions so
that adoption in a web or API project requires no boilerplate beyond the single registration call.

## Technical Context

**Language/Version**: C# 13 / .NET 10 (`LangVersion: latest`, `net10.0`)

**Primary Dependencies**:

- `Microsoft.Extensions.DependencyInjection.Abstractions` — `IServiceCollection`, `TryAddSingleton`
- `PndTools` (companion package declares core package as a direct dependency per FR-009)
- `LTRData.DiscUtils.SquashFs` / `LTRData.DiscUtils.Iso9660` (transitive via `PndTools`)

**Storage**: N/A

**Testing**: xUnit v3 — `TestContext.Current.CancellationToken`, `Assert.Single`

**Target Platform**: `net10.0` — matches core `PndTools` package (FR-009)

**Project Type**: Library (companion NuGet package distributed separately from core)

**Performance Goals**: No measurable startup overhead relative to manually registering the same
three services (SC-003)

**Constraints**:

- `TreatWarningsAsErrors: true` + `EnforceCodeStyleInBuild: true` — zero-warning policy
- `AnalysisMode: All` for test projects, `Recommended` for library projects
- SPDX licence header required on all `.cs` files (IDE0073)
- XML documentation required on all public members (UK English spelling)
- `PndArchiveFactory` must not dispose the stream or archive — caller owns both lifetimes
- `TryAddSingleton` (not `AddSingleton`) used for all registrations to satisfy FR-007

## Constitution Check

| Principle | Status | Notes |
| --------- | ------ | ----- |
| I. Library-First | ✅ Pass | Both packages are library code; `PndTools.AspNetCore` depends only on `Microsoft.Extensions.DependencyInjection.Abstractions` — no web framework runtime |
| II. Zero-Warning Build | ✅ Pass | All new code must carry XML docs, SPDX headers, and pass `dotnet format` before merging |
| III. Test-First | ✅ Pass | All new public members require tests in the same commit: interfaces, instance parser API, factory methods, and extension method |
| IV. No Shelling Out | ✅ Pass | `PndArchiveFactory.Open` delegates to `PndArchive.Open` — no process invocation |
| V. Simplicity Over Abstraction | ✅ Pass | `IPndArchiveFactory` justified by the "new is glue" antipattern; no options class or configuration delegate in v1 — empty POCO would confuse the API without adding value |

## Project Structure

### Documentation (this feature)

```text
specs/002-aspnetcore-integration/
├── plan.md              ← this file
├── research.md          ← phase 0 output
├── data-model.md        ← phase 1 output
├── quickstart.md        ← phase 1 output
├── contracts/
│   └── interfaces.md    ← phase 1 output
└── tasks.md             ← phase 2 output (/speckit-tasks)
```

### Source Code (repository root)

```text
src/
├── PndTools/                                        ← existing, modified
│   ├── IPxmlParser.cs                               ← new interface
│   ├── PxmlParser.cs                                ← refactored: static → instance class
│   ├── Validation/
│   │   ├── IPxmlValidator.cs                        ← new interface
│   │   └── PxmlValidator.cs                         ← add : IPxmlValidator
│   ├── IO/
│   │   ├── PndArchive.cs                            ← add ArgumentOutOfRangeException guard for stream.Position != 0
│   │   └── Extensions/
│   │       └── PndStreamExtensions.cs               ← rename InvalidPndException → PndArchiveException
└── PndTools.AspNetCore/                             ← new project
    ├── PndTools.AspNetCore.csproj
    ├── IPndArchiveFactory.cs
    ├── PndArchiveFactory.cs
    └── DependencyInjection/
        └── PndToolsServiceCollectionExtensions.cs

test/
├── PndTools.Tests/                                  ← existing, modified
│   └── Integration/
│       └── PxmlParserTests.cs                       ← updated for instance-based API
└── PndTools.AspNetCore.Tests/                       ← new project
    ├── PndTools.AspNetCore.Tests.csproj
    ├── ServiceCollectionExtensionsTests.cs
    └── PndArchiveFactoryTests.cs

docs/
├── getting-started.md                               ← updated: note PxmlParser is now instantiatable
└── guides/
    ├── parsing-pxml.md                              ← updated: instance-based usage examples
    ├── validation.md                                ← updated: IPxmlValidator interface reference
    └── aspnetcore.md                                ← new: AddPndTools(), factory injection, TryOpen pattern
```

**Structure Decision**: Two-project layout (`PndTools` + `PndTools.AspNetCore`) with a parallel
test project for the companion. The companion declares the core package as a direct dependency and
is distributed separately so non-web consumers carry no dependency on the hosting stack.

## Complexity Tracking

No constitution violations — no justification table required.

[spec.md]: spec.md
