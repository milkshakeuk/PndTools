---
title: Getting started
description: Install PndTools and open your first PND archive.
---

PndTools is a .NET 10 library for reading and inspecting [OpenPandora][openpandora] PND package files. It supports both SquashFS and ISO-based archives and exposes the embedded PXML metadata as typed .NET objects.

## Installation

```bash
dotnet add package PndTools
```

## Open an archive

```csharp
using PndTools.IO;

using var stream = File.OpenRead("game.pnd");
using var archive = PndArchive.Open(stream);

Console.WriteLine(archive.ArchiveType); // SquashFs or Iso
```

## List files

```csharp
var files = archive.ListFiles();

foreach (var path in files)
{
    Console.WriteLine(path);
}
```

## Next steps

- [Extracting files](guides/extracting-files) — extract a single file, a subset, or the entire archive
- [Reading PXML and icon](guides/pxml-and-icon) — read metadata and the embedded icon directly from the PND stream
- [Parsing PXML](guides/parsing-pxml) — work with the typed model: applications, titles, categories, and more
- [Validating PXML](guides/validation) — validate metadata against the schema and business rules
- [Searching streams](guides/stream-extensions) — search any seekable stream for a byte pattern

[openpandora]: https://openpandora.org
