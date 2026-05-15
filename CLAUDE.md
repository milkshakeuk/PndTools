# PndTools

A .NET 10 library for parsing, validating, and inspecting PND (Pandora) package files. Originally a port of the PHP library <https://github.com/milkshakeuk/PndAid>.

## Stack

- .NET 10 / C# 13, `LangVersion: latest`
- xUnit v3 for tests (`TestContext.Current.CancellationToken`, `Assert.Single` not `Assert.Equal(1, ...)`)
- `LTRData.DiscUtils.SquashFs` and `LTRData.DiscUtils.Iso9660` for archive access — both extend `DiscFileSystem`, no shelling out
- `TreatWarningsAsErrors: true` + `EnforceCodeStyleInBuild: true` — zero warnings policy, style violations break the build
- `AnalysisMode: Recommended` (library), `All` (tests) via `Directory.Build.props`
- `.editorconfig` based on `dotnet/runtime` — run `dotnet format` to fix violations automatically
- Node.js devDependencies (`package.json`): `@commitlint/cli`, `@commitlint/config-conventional`, `markdownlint-cli2`, `@j178/prek`
- [prek][prek] for local git hooks (`prek.toml`) — installed via `npm install` (the `prepare` script runs `prek install` automatically)
- [APM][apm] for AI assistant skill management (`apm.yml` + `apm.lock.yaml`) — run `apm install` after cloning to deploy skills to `.claude/skills/` and `.agents/skills/`

## Project structure

```text
src/PndTools/
  IO/
    Extensions/   — PndStreamExtensions, StreamExtensions
    PndArchive.cs — disposable archive reader (SquashFS + ISO)
  Models/         — Pxml records, PndToolsJsonContext
  Validation/
    Extensions/   — NonSchemaEnforcableValidationExtensions
    PxmlValidator.cs, ValidationResult.cs
  Xml/Extensions/ — XElementExtensions, TypeParsingExtensions
  ArgumentCollectionException.cs
  PxmlParser.cs

test/PndTools.Tests/
  Helpers/        — StreamTestHelper
  Integration/
    IO/           — PndArchiveTests, PndStreamExtensionsTests
    Validation/   — PxmlValidatorTests
    PxmlParserTests.cs
  Unit/
    IO/Extentions/ — StreamExtensionsTests
    Models/        — PndToolsJsonContextTests
    Xml/Extensions/ — TypeParsingExtensionsTests, XElementExtensionsTests
    ArgumentCollectionExceptionTests.cs

test/PndTools.Benchmarks/
  PndArchiveBenchmarks.cs, PndStreamExtensionsBenchmarks.cs, PxmlParserBenchmarks.cs

docs/             — markdown source for the Starlight site (getting-started, guides/, benchmarks/, coverage/)
site/             — Astro/Starlight project; built and deployed to GitHub Pages via docs.yml
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

For async methods, use `ReadOnlyMemory<byte>` instead — spans cannot cross `await` boundaries:

```csharp
private static ReadOnlyMemory<byte> PxmlStartMemory => "<PXML"u8.ToArray();
```

**Stream search** uses 4096-byte chunked traversal with `pattern.Length - 1` overlap between chunks to handle boundary-spanning matches. `ReadExactly` not `Read`.

**Archive access** via `PndArchive` — disposable, created with `PndArchive.Open(stream)`. Holds a `DiscFileSystem` open for the lifetime of the instance. `ArchiveType` property exposes the detected `PndArchiveType` (SquashFs / Iso).

**Always read the relevant source files before planning.** Check what public methods, patterns, and types already exist in the actual code — do not assume the API surface from memory or documentation.

**Async conventions**: async methods use the `Async` suffix and accept `CancellationToken cancellationToken = default` as the last parameter.

**Exception types**: custom exceptions use short names — `PndParseException` (not `PndParsingException`), `PndArchiveException`, `PndValidationException`, with `PndException` as the base. `ArgumentCollectionException` already exists and must remain unchanged.

**Benchmarks**: for performance-sensitive features, add or update a class in `test/PndTools.Benchmarks/` using `[MemoryDiagnoser]` + `[MarkdownExporterAttribute.GitHub]`.

**JSON serialisation** uses `PndToolsJsonContext` (AOT-safe source generation, camelCase).

## Verification

After any code change, run:

```bash
dotnet build    # zero warnings required
dotnet test     # all tests must pass
dotnet format   # fixes style and licence-header violations
```

## Test conventions

- Arrange / Act / Assert comments in every test
- Naming follows the [MS convention][ms-test-naming]: `MethodName_Scenario_ExpectedBehavior` — e.g. `Open_NullStream_ThrowsArgumentNullException`
- Use `expected` variable rather than inlining expected values in assertions
- `Assert.Single` for single-item collections, not `Assert.Equal(1, collection.Count)`
- Null `InlineData` entries require nullable parameter types in the test method

## Commit style

[Conventional Commits][conventional-commits]. Feature commits include their tests in the same commit. Commit messages are validated by commitlint — enforced locally via the prek `commit-msg` hook and again in CI.

Type → SemVer mapping: `feat` = minor, `fix`/`perf` = patch, `feat!`/`BREAKING CHANGE:` footer = major, everything else = no bump. This drives automated versioning and changelog generation.

Type usage rules — only `feat`, `fix`, and `perf` affect semver, so apply them only to changes that NuGet consumers would notice:

- `feat` — new public API or behaviour a library consumer gains
- `fix` — corrects wrong behaviour in the library (not docs or tooling)
- `perf` — measurable performance improvement to library code itself
- `feat!` / `BREAKING CHANGE:` footer — removes or changes existing public API
- `docs` — changes to `.md` files, XML doc comments, or the docs site content
- `chore` — project tooling, config, conventions, or scripts with no runtime effect
- `ci` — changes to GitHub Actions workflows or hook configuration
- `build` — changes to build system files (`.csproj`, `Directory.Build.props`, `package.json` structure)
- `test` — adding or updating tests or benchmarks
- `refactor` — internal restructuring with no public API change

When in doubt: if a NuGet consumer's code would not compile or behave differently, use `feat`/`fix`/`perf`; if only contributors are affected, use one of the no-bump types.

Commit message bodies use imperative-mood prose paragraphs, not bullet lists. Group related changes into separate paragraphs separated by a blank line. Changelog tooling ignores the body entirely (it reads only the subject line and `BREAKING CHANGE:` footer), so the body is for human readers in `git log`.

## npm script conventions

Follow the [ESLint package.json conventions][eslint-pkg-conventions]:

- Script names use only lowercase letters, colons (`:`), hyphens (`-`), and plus signs (`+`)
- Colons separate parts; hyphens separate words
- Scripts must start with one of: `build`, `fetch`, `lint`, `fmt`, `start`, `test`, `release` — no other prefixes are permitted, including `clean`, `dev`, `check`, or similar
- Modifiers are appended with colons in order: `:fix`, `:check`, `:target`, `:options`, `:watch`
- Scripts must appear in alphabetical order
- npm lifecycle hooks (`prepare`, `postinstall`) are exempt — they must keep their npm-defined names to run automatically

## Markdown style

All `.md` files are linted with `markdownlint-cli2` using `.markdownlint-cli2.jsonc` (config and ignores combined). The prek `pre-commit` hook runs it on staged files. To check manually: `npm run lint:md`. Inline links are avoided in favour of reference-style links collected at the bottom of each file. MD054 enforces this — only `autolink`, `collapsed`, `full`, and `shortcut` styles are permitted; `inline` and `url_inline` are disabled.

## AI assistant skills

Skills are managed by [APM][apm]. Run `apm install` to deploy them after cloning. To invoke a skill during a session, use `/conventional-commits` — Claude Code loads the skill and applies its guidance for the remainder of that interaction.

## What we decided NOT to do

- No `PndFileInfo` wrapper — .NET's native `FileInfo` already provides Name, FullName, DirectoryName, Extension, Length
- No `PndArchive.Open(string path)` overload — avoids stream ownership complexity; callers manage `File.OpenRead`
- No `PndFileType` / `DetectFileType` naming — renamed to `PndArchiveType` / `DetectArchiveType` to reflect that it describes the archive format, not the file type

[apm]: https://github.com/microsoft/apm
[conventional-commits]: https://www.conventionalcommits.org
[eslint-pkg-conventions]: https://eslint.org/docs/latest/contribute/package-json-conventions
[ms-test-naming]: https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-best-practices#follow-test-naming-standards
[prek]: https://prek.j178.dev

<!-- SPECKIT START -->
For additional context about technologies to be used, project structure,
shell commands, and other important information, read the current plan
<!-- SPECKIT END -->
