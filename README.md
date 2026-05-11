# PndTools [![CI][ci-image]][ci-url]

A .NET 10 library for parsing, validating, and inspecting PND (Pandora) package files. Supports both SquashFS and ISO 9660 based PND files.

## Features

- Detect whether a PND file is SquashFS or ISO 9660 based
- List, extract, and extract all files from a PND archive
- Extract and parse PXML metadata
- Validate PXML against the OpenPandora schema, including FreeDesktop.org category rules
- Extract the embedded icon
- Save PXML and icon directly to disk

## Installation

```shell
dotnet add package PndTools
```

## Usage

### Detect archive type

```csharp
using PndTools.IO.Extensions;

using var stream = File.OpenRead("game.pnd");
PndArchiveType archiveType = stream.DetectArchiveType(); // SquashFs, Iso, or Unknown
```

### List and extract files

```csharp
using PndTools.IO;

using var stream = File.OpenRead("game.pnd");
using var archive = PndArchive.Open(stream);

Console.WriteLine(archive.ArchiveType); // SquashFs or Iso

IReadOnlyList<string> files = archive.ListFiles();

archive.ExtractFile("/PXML.xml", "output/PXML.xml");
archive.ExtractAll("output/");
```

### Extract and parse PXML

```csharp
using var stream = File.OpenRead("game.pnd");
string xml = stream.GetPxml();

var parser = new PxmlParser();
Pxml pxml = parser.Parse(xml);

Console.WriteLine(pxml.Package.Id);
Console.WriteLine(pxml.Package.Version?.Major);

foreach (var app in pxml.Applications)
{
    Console.WriteLine(app.Titles.FirstOrDefault(t => t.Lang == "en_US")?.Text);
}
```

### Validate PXML

```csharp
using PndTools.Validation;

var validator = new PxmlValidator();
ValidationResult result = validator.Validate(xml);

if (!result.IsValid)
{
    foreach (var error in result.Errors)
        Console.WriteLine(error);
}
```

### Extract icon

```csharp
using var stream = File.OpenRead("game.pnd");
byte[] icon = stream.GetIcon();
```

### Save PXML and icon to disk

```csharp
using var stream = File.OpenRead("game.pnd");
stream.SavePxml("game.xml");
stream.SaveIcon("icon.png");
```

## Requirements

.NET 10 or later.

## License

MIT

[ci-url]: https://github.com/milkshakeuk/PndTools/actions/workflows/ci.yml
[ci-image]: https://github.com/milkshakeuk/PndTools/actions/workflows/ci.yml/badge.svg
