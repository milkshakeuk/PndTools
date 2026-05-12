# PndTools

A .NET 10 library for parsing, validating, and inspecting PND (Pandora) package files. Originally a port of the PHP library [PndAid](https://github.com/milkshakeuk/PndAid).

## Stack

- .NET 10 / C# 13, `LangVersion: latest`
- xUnit v3 for tests (`TestContext.Current.CancellationToken`, `Assert.Single` not `Assert.Equal(1, ...)`)
- `LTRData.DiscUtils.SquashFs` and `LTRData.DiscUtils.Iso9660` for archive access — both extend `DiscFileSystem`, no shelling out
- `TreatWarningsAsErrors: true` + `EnforceCodeStyleInBuild: true` — zero warnings policy, style violations break the build
- `AnalysisMode: Recommended` (library), `All` (tests) via `Directory.Build.props`
- `.editorconfig` based on `dotnet/runtime` — run `dotnet format` to fix violations automatically
- Node.js devDependencies (`package.json`): `@commitlint/cli`, `@commitlint/config-conventional`, `markdownlint-cli2`, `@j178/prek`
- [prek][prek] for local git hooks (`prek.toml`) — installed via `npm install` (the `prepare` script runs `prek install` automatically)

## Project structure

```text
src/PndTools/
  IO/
    Extensions/   — PndStreamExtensions, StreamExtensions
    PndArchive.cs — disposable archive reader (SquashFS + ISO)
  Models/         — Pxml records, PndToolsJsonContext
  Validation/     — PxmlValidator, ValidationResult, NonSchemaEnforcableValidationExtensions
  Xml/Extensions/ — XElementExtensions, TypeParsingExtensions
  ArgumentCollectionException.cs
  PxmlParser.cs

test/PndTools.Tests/
  Integration/
  Unit/
```

## Code conventions

- File-scoped namespaces throughout
- Braces, spacing, naming, and expression-body style are enforced by `.editorconfig` — run `dotnet format` to auto-fix
- All `.cs` files must begin with the SPDX licence header (enforced by `IDE0073`; `dotnet format` adds it)
- Prefer `#pragma warning disable/restore` over `[SuppressMessage]` for diagnostic suppressions — pragmas are less verbose and keep the suppression close to the code
- No comments unless the WHY is non-obvious; no summary-of-what comments
- XML docs on all public members using UK English spelling (e.g. "serialisation" not "serialization")
- Positional records use `<param>` tags on the record declaration; non-positional use `<summary>` on each property
- `required` + `init` for non-positional records where construction should go through a factory

## Patterns

**Exception guards** follow the .NET `ThrowIf` convention — static method on the exception type itself:

```csharp
ArgumentCollectionException.ThrowIfNullOrEmpty(collection);    // ours
ArgumentNullException.ThrowIfNull(value);                      // framework
ArgumentException.ThrowIfNullOrWhiteSpace(value);              // framework
```

**Magic bytes** are `ReadOnlySpan<byte>` properties (not fields, not `static readonly byte[]`) for zero heap allocation:

```csharp
private static ReadOnlySpan<byte> PxmlStart => "<PXML"u8;
```

**Stream search** uses 4096-byte chunked traversal with `pattern.Length - 1` overlap between chunks to handle boundary-spanning matches. `ReadExactly` not `Read`.

**Archive access** via `PndArchive` — disposable, created with `PndArchive.Open(stream)`. Holds a `DiscFileSystem` open for the lifetime of the instance. `ArchiveType` property exposes the detected `PndArchiveType` (SquashFs / Iso).

**JSON serialisation** uses `PndToolsJsonContext` (AOT-safe source generation, camelCase).

## Test conventions

- Arrange / Act / Assert comments in every test
- Naming follows the [MS convention][ms-test-naming]: `MethodName_Scenario_ExpectedBehavior` — e.g. `Open_NullStream_ThrowsArgumentNullException`
- Use `expected` variable rather than inlining expected values in assertions
- `Assert.Single` for single-item collections, not `Assert.Equal(1, collection.Count)`
- Null `InlineData` entries require nullable parameter types in the test method

## Commit style

[Conventional Commits][conventional-commits]. Feature commits include their tests in the same commit. Commit messages are validated by commitlint — enforced locally via the prek `commit-msg` hook and again in CI.

Type → SemVer mapping: `feat` = minor, `fix`/`perf` = patch, `feat!`/`BREAKING CHANGE:` footer = major, everything else = no bump. This drives automated versioning and changelog generation.

## Markdown style

All `.md` files are linted with `markdownlint-cli2` using `.markdownlint.json`. The prek `pre-commit` hook runs it on staged files. To check manually: `npx markdownlint-cli2 "**/*.md"`. Inline links are avoided in favour of reference-style links collected at the bottom of each file.

## What we decided NOT to do

- No `PndFileInfo` wrapper — .NET's native `FileInfo` already provides Name, FullName, DirectoryName, Extension, Length
- No `PndArchive.Open(string path)` overload — avoids stream ownership complexity; callers manage `File.OpenRead`
- No `PndFileType` / `DetectFileType` naming — renamed to `PndArchiveType` / `DetectArchiveType` to reflect that it describes the archive format, not the file type

[conventional-commits]: https://www.conventionalcommits.org
[ms-test-naming]: https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-best-practices#follow-test-naming-standards
[prek]: https://prek.j178.dev
