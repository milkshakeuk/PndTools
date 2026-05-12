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

## Extract a file

```csharp
archive.ExtractFile("/PXML.xml", "/tmp/PXML.xml");
```

## Extract all files

```csharp
archive.ExtractAll("/tmp/output");
```

## Parse PXML metadata

```csharp
using PndTools;
using System.Xml.Linq;

archive.ExtractFile("/PXML.xml", "/tmp/PXML.xml");

var xml = XDocument.Load("/tmp/PXML.xml");
var pxml = PxmlParser.Parse(xml);

Console.WriteLine(pxml.Applications[0].Id);
```

[openpandora]: https://openpandora.org
