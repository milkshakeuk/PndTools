---
title: Getting started
description: Install PndTools and open your first PND archive.
editUrl: 'https://github.com/milkshakeuk/PndTools/edit/master/docs/getting-started.md'
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

- [Extracting files][extracting-files] — extract a single file, a subset, or the entire archive
- [Reading PXML and icon][pxml-and-icon] — read metadata and the embedded icon directly from the PND stream
- [Parsing PXML][parsing-pxml] — work with the typed model: applications, titles, categories, and more
- [Validating PXML][validation] — validate metadata against the schema and business rules
- [Searching streams][stream-extensions] — search any seekable stream for a byte pattern
- [Using async and await][async-io] — use the async API in ASP.NET Core, background services, and other async contexts
- [ASP.NET Core integration][aspnetcore] — register all PndTools services with a single call and inject them into controllers and minimal API handlers

[aspnetcore]: /guides/aspnetcore
[async-io]: /guides/async-io
[extracting-files]: /guides/extracting-files
[openpandora]: https://openpandora.org
[parsing-pxml]: /guides/parsing-pxml
[pxml-and-icon]: /guides/pxml-and-icon
[stream-extensions]: /guides/stream-extensions
[validation]: /guides/validation
