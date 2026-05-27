# Interface Contracts: PndTools.AspNetCore Integration Package

**Branch**: `002-aspnetcore-integration` | **Date**: 2026-05-27

## Core library additions (`PndTools`)

### IPxmlParser

```csharp
namespace PndTools;

/// <summary>Parses a PXML string into a <see cref="Pxml"/> object graph.</summary>
public interface IPxmlParser
{
    /// <summary>Parses a PXML string into a <see cref="Pxml"/> object graph.</summary>
    /// <param name="xml">The PXML string to parse.</param>
    /// <returns>The parsed <see cref="Pxml"/>.</returns>
    /// <exception cref="System.Xml.XmlException"><paramref name="xml"/> is not valid XML.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="xml"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentException"><paramref name="xml"/> is empty or whitespace.</exception>
    Pxml Parse(string xml);
}
```

### IPxmlValidator

```csharp
namespace PndTools.Validation;

/// <summary>
/// Validates a PXML string against the OpenPandora schema and additional non-schema rules.
/// </summary>
public interface IPxmlValidator
{
    /// <summary>Validates a PXML string.</summary>
    /// <param name="input">The PXML string to validate.</param>
    /// <returns>
    /// A <see cref="ValidationResult"/> whose <see cref="ValidationResult.IsValid"/> is
    /// <c>true</c> when the PXML is valid, or <c>false</c> with errors populated.
    /// </returns>
    /// <exception cref="ArgumentNullException"><paramref name="input"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentException"><paramref name="input"/> is empty or whitespace.</exception>
    ValidationResult Validate(string input);
}
```

### PndArchive.Open — updated contract

```csharp
/// <exception cref="ArgumentNullException"><paramref name="stream"/> is <c>null</c>.</exception>
/// <exception cref="ArgumentOutOfRangeException">
///   <paramref name="stream"/> is not positioned at its origin
///   (<see cref="Stream.Position"/> is not zero).
/// </exception>
/// <exception cref="PndArchiveException">
///   The stream is not a recognised SquashFS or ISO 9660 archive.
/// </exception>
public static PndArchive Open(Stream stream)
```

---

## Companion package (`PndTools.AspNetCore`)

### IPndArchiveFactory

```csharp
namespace PndTools.AspNetCore;

/// <summary>
/// Creates <see cref="PndArchive"/> instances from caller-owned streams.
/// The caller retains ownership of both the stream and the returned archive.
/// </summary>
public interface IPndArchiveFactory
{
    /// <summary>
    /// Opens a <see cref="PndArchive"/> from <paramref name="stream"/>.
    /// </summary>
    /// <param name="stream">
    /// The PND file stream to read from. Must be positioned at its origin.
    /// </param>
    /// <returns>A <see cref="PndArchive"/> ready for use.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="stream"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentOutOfRangeException">
    ///   <paramref name="stream"/> is not positioned at its origin.
    /// </exception>
    /// <exception cref="PndArchiveException">
    ///   The stream is not a recognised PND archive.
    /// </exception>
    PndArchive Open(Stream stream);

    /// <summary>
    /// Attempts to open a <see cref="PndArchive"/> from <paramref name="stream"/>.
    /// Returns <c>false</c> without throwing if the stream is <c>null</c>, not positioned
    /// at its origin, or not a recognised PND archive.
    /// </summary>
    /// <param name="stream">The PND file stream to read from.</param>
    /// <param name="archive">
    /// When this method returns <c>true</c>, contains the opened <see cref="PndArchive"/>;
    /// otherwise <c>null</c>.
    /// </param>
    /// <returns>
    /// <c>true</c> if the archive was opened successfully; otherwise <c>false</c>.
    /// </returns>
    bool TryOpen(Stream stream, out PndArchive? archive);
}
```

### AddPndTools extension method

```csharp
namespace Microsoft.Extensions.DependencyInjection;

public static class PndToolsServiceCollectionExtensions
{
    /// <summary>
    /// Registers PndTools services with the dependency injection container.
    /// Registers <see cref="IPxmlParser"/>, <see cref="IPxmlValidator"/>, and
    /// <see cref="IPndArchiveFactory"/> as Singletons.
    /// Safe to call multiple times — subsequent calls are no-ops.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <returns>The <paramref name="services"/> instance for chaining.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="services"/> is <c>null</c>.</exception>
    public static IServiceCollection AddPndTools(this IServiceCollection services)
}
```

**Registration behaviour**:

- Uses `TryAddSingleton` for all three services — subsequent calls are no-ops (FR-007)
- Returns `services` to support method chaining (`services.AddPndTools().AddSomethingElse()`)
- Throws `ArgumentNullException` if `services` is `null`
