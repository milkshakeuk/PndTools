# PndTools

A .NET 10 library for parsing, validating, and inspecting PND (Pandora) package files. Originally a port of the PHP library [PndAid](https://github.com/milkshakeuk/PndAid).

## Stack

- .NET 10 / C# 13, `LangVersion: latest`
- xUnit v3 for tests (`TestContext.Current.CancellationToken`, `Assert.Single` not `Assert.Equal(1, ...)`)
- `LTRData.DiscUtils.SquashFs` and `LTRData.DiscUtils.Iso9660` for archive access — both extend `DiscFileSystem`, no shelling out
- `TreatWarningsAsErrors: true` — zero warnings policy

## Project structure

```
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
  Intergration/   — note: directory name has a typo, keep as-is to avoid churn
  Unit/
```

## Code conventions

- File-scoped namespaces throughout
- Braces on all `if` statements — no single-line ifs
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
- Preferred naming: `MethodName_WillBehaviour_WhenCondition`
- Use `expected` variable rather than inlining expected values in assertions
- `Assert.Single` for single-item collections, not `Assert.Equal(1, collection.Count)`
- Null `InlineData` entries require nullable parameter types in the test method

## Commit style

Conventional commits. Feature commits include their tests in the same commit.

## What we decided NOT to do

- No `PndFileInfo` wrapper — .NET's native `FileInfo` already provides Name, FullName, DirectoryName, Extension, Length
- No `PndArchive.Open(string path)` overload — avoids stream ownership complexity; callers manage `File.OpenRead`
- No `PndFileType` / `DetectFileType` naming — renamed to `PndArchiveType` / `DetectArchiveType` to reflect that it describes the archive format, not the file type
