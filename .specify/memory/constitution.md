<!--
## Sync Impact Report

Version change: N/A (initial constitution) → 1.0.0
Modified principles: none (initial authoring)
Added sections: Core Principles (I–V), Quality Standards, Development Workflow, Governance
Removed sections: none
Templates requiring updates:
  - .specify/templates/plan-template.md — ✅ Constitution Check section present; no changes needed
  - .specify/templates/spec-template.md — ✅ no constitution-specific gates required
  - .specify/templates/tasks-template.md — ✅ no constitution-specific task types added
  - .specify/templates/commands/ — ⚠ not present in this project; no action required
Deferred TODOs: none
-->

# PndTools Constitution

## Core Principles

### I. Library-First (NON-NEGOTIABLE)

PndTools MUST remain a pure .NET library with no runtime dependencies beyond the declared NuGet packages. Every public type MUST be independently usable, independently testable, and carry XML documentation comments in UK English. No feature may be added purely for organisational convenience; each addition MUST have a clear consumer rationale.

**Rationale**: The library is distributed as a NuGet package. AOT-safe design and a clean public API surface are non-negotiable for consumers who may embed it in constrained environments.

### II. Zero-Warning Build

The build MUST produce zero warnings and zero style violations. `TreatWarningsAsErrors`, `EnforceCodeStyleInBuild`, and `.editorconfig` enforcement are permanent constraints, not optional settings. A PR that introduces a suppression MUST justify it inline with `#pragma warning disable/restore` and a comment explaining why.

**Rationale**: Warnings are deferred bugs. Maintaining a clean baseline ensures new warnings are immediately visible rather than buried in noise.

### III. Test-First for All Public Behaviour

Every new public method or observable behaviour change MUST be accompanied by tests in the same commit. Tests are organised as unit (fast, no I/O) and integration (real archive files, real streams) and MUST follow the `MethodName_Scenario_ExpectedBehavior` naming convention. `Assert.Single` is used for single-item collections; `Assert.Equal(1, ...)` on counts is forbidden.

**Rationale**: The library parses binary file formats. Correctness cannot be assumed from type-checking alone; real-file integration tests catch boundary conditions that mocks would hide.

### IV. No Shelling Out

Archive access MUST use `LTRData.DiscUtils.SquashFs` and `LTRData.DiscUtils.Iso9660` via the `PndArchive` disposable abstraction. Process invocation (`System.Diagnostics.Process`, `Shell.Execute`, or equivalent) is permanently forbidden for archive operations.

**Rationale**: Shelling out introduces platform dependency, makes testing harder, and creates security surface. The DiscUtils libraries provide the same capabilities in-process.

### V. Simplicity Over Abstraction

New abstractions require justification. Three similar lines of code are preferable to a premature helper. Public API changes — additions, renames, removals — MUST be recorded in the `BREAKING CHANGE:` footer or as a `feat`/`fix` commit type so automated versioning reflects the impact. The "What we decided NOT to do" section of CLAUDE.md is authoritative for rejected patterns.

**Rationale**: A small, stable API is easier to consume and document than a large one. Every abstraction added now is a compatibility obligation for every future release.

## Quality Standards

All `.cs` files MUST carry the SPDX licence header (enforced by `IDE0073`). Magic bytes MUST be expressed as `ReadOnlySpan<byte>` properties (not `static readonly byte[]`) for zero heap allocation; async variants use `ReadOnlyMemory<byte>`. Stream search MUST use 4096-byte chunked traversal with `pattern.Length - 1` overlap to handle boundary-spanning matches.

Performance-sensitive paths MUST have a corresponding benchmark class in `test/PndTools.Benchmarks/` using `[MemoryDiagnoser]` and `[MarkdownExporterAttribute.GitHub]`. Benchmark results inform, but do not gate, merges unless a regression is measurable.

JSON serialisation MUST use `PndToolsJsonContext` (AOT-safe source generation, camelCase). Custom exceptions MUST use short names (`PndParseException`, `PndArchiveException`, `PndValidationException`) with `PndException` as the base, and expose static `ThrowIf` guards following the .NET `ThrowIf` convention.

## Development Workflow

Feature branches follow the sequential naming convention managed by `speckit-git-feature`. Commit messages MUST conform to Conventional Commits as validated by commitlint via the prek `commit-msg` hook. `feat` and `fix` are reserved for changes that NuGet consumers would observe; internal changes use `chore`, `refactor`, `test`, `docs`, `ci`, or `build`.

Before merging, the following gates MUST pass:

- `dotnet build` — zero warnings
- `dotnet test` — all tests green
- `dotnet format` — no outstanding style or licence-header violations

Markdown files are linted by `markdownlint-cli2` on pre-commit via prek. Reference-style links are mandatory; inline links are forbidden (MD054).

## Governance

This constitution supersedes any conflicting guidance in other documents. Amendments require a `docs:` commit that increments the version and updates `LAST_AMENDED_DATE`. The amendment MUST be reflected in the Sync Impact Report comment at the top of this file.

MAJOR version: backward-incompatible principle removal or redefinition. MINOR version: new principle or material guidance expansion. PATCH version: clarification, wording, or non-semantic refinement.

All PRs that introduce new public API, new dependencies, or new tooling MUST include a constitution compliance check in the PR description. Runtime development guidance lives in `CLAUDE.md` at the project root.

**Version**: 1.0.0 | **Ratified**: 2026-05-15 | **Last Amended**: 2026-05-15
