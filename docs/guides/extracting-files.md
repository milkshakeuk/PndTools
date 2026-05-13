---
title: Extracting files
description: Extract a single file, a named subset, or the entire contents of a PND archive.
---

All extraction methods preserve the internal directory structure of the archive under the output path. The output directory is created automatically if it does not exist.

## Extract a single file

Use `ExtractFile` when you know the exact internal path of the file you need.

```csharp
using PndTools.IO;

using var stream = File.OpenRead("game.pnd");
using var archive = PndArchive.Open(stream);

archive.ExtractFile("/PXML.xml", "/tmp/PXML.xml");
```

`ExtractFile` throws `FileNotFoundException` if the path does not exist in the archive.

## Extract a subset of files

Use `ExtractFiles` to extract a specific set of paths in one call, preserving directory structure under the output directory.

```csharp
var files = archive.ListFiles();

// Extract only image files
var images = files.Where(f => f.EndsWith(".png"));

archive.ExtractFiles(images, "/tmp/output");
```

`ExtractFiles` throws `FileNotFoundException` on the first path that does not exist in the archive.

## Extract all files

Use `ExtractAll` to extract the entire archive contents.

```csharp
archive.ExtractAll("/tmp/output");
```

## Extract PXML and parse metadata

A common pattern is to extract PXML to a temporary file and parse it immediately.

```csharp
using PndTools;
using System.Xml.Linq;

var temp = Path.GetTempFileName();

try
{
    archive.ExtractFile("/PXML.xml", temp);

    var xml = XDocument.Load(temp);
    var pxml = PxmlParser.Parse(xml);

    Console.WriteLine(pxml.Applications[0].Id);
}
finally
{
    File.Delete(temp);
}
```

## Extract preview images

`ExtractPreviewPics` extracts all preview images referenced in the PXML, skipping any paths that do not exist in the archive.

```csharp
var pxml = PxmlParser.Parse(XDocument.Load(temp));

var extracted = archive.ExtractPreviewPics(pxml, "/tmp/previews");

foreach (var path in extracted)
{
    Console.WriteLine(path);
}
```

## Async variants

Every extraction method has an async counterpart — `ExtractFileAsync`, `ExtractFilesAsync`, `ExtractAllAsync`, and `ExtractPreviewPicsAsync` — each accepting an optional `CancellationToken`. See the [async IO guide][async-io] for usage examples and guidance on when to prefer the async API.

[async-io]: ../async-io
