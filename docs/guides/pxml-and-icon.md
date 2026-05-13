---
title: Reading PXML and icon
description: Read PXML metadata and the embedded icon directly from a PND stream without mounting the archive.
---

A PND file is a SquashFS or ISO image with PXML metadata and a PNG icon appended directly to the end of the file. Launchers and menus rely on this appended PXML to discover and display applications without mounting the archive. See the [PXML specification][pxml-spec] for the full format reference. The `PndStreamExtensions` methods read PXML and icon directly from the raw stream, so you do not need to open the archive filesystem at all. This is faster when you only need metadata or the icon.

## Read PXML as a string

`GetPxml` searches backwards from the end of the stream for the `<PXML>` block and returns it as a UTF-8 string with an XML declaration prepended.

```csharp
using PndTools.IO.Extensions;

using var stream = File.OpenRead("game.pnd");
var xml = stream.GetPxml();

Console.WriteLine(xml);
```

Throws `InvalidPndException` if the stream does not contain valid PXML.

## Parse PXML into typed objects

Combine `GetPxml` with `PxmlParser` to get strongly-typed metadata without writing a temp file.

```csharp
using PndTools;
using PndTools.IO.Extensions;
using System.Xml.Linq;

using var stream = File.OpenRead("game.pnd");
var xmlString = stream.GetPxml();
var pxml = PxmlParser.Parse(XDocument.Parse(xmlString));

Console.WriteLine(pxml.Applications[0].Id);
```

## Save PXML to a file

`SavePxml` writes the PXML directly to a path.

```csharp
using var stream = File.OpenRead("game.pnd");
stream.SavePxml("/tmp/PXML.xml");
```

## Read the embedded icon

`GetIcon` returns the raw PNG bytes of the icon appended to the end of the PND file.

```csharp
using var stream = File.OpenRead("game.pnd");
var png = stream.GetIcon();

await File.WriteAllBytesAsync("/tmp/icon.png", png);
```

Throws `InvalidPndException` if the stream does not contain an embedded icon.

## Save the icon to a file

`SaveIcon` combines the read and write into a single call.

```csharp
using var stream = File.OpenRead("game.pnd");
stream.SaveIcon("/tmp/icon.png");
```

## Detect the archive type

`DetectArchiveType` reads the magic bytes at the start of the stream and returns a `PndArchiveType` value without opening the archive.

```csharp
using var stream = File.OpenRead("game.pnd");
var archiveType = stream.DetectArchiveType();

Console.WriteLine(archiveType); // SquashFs or Iso
```

Returns `PndArchiveType.Unknown` if the stream is too short or does not match either format.

## Async variants

Every method on `PndStreamExtensions` has an async counterpart — `GetPxmlAsync`, `GetIconAsync`, `SavePxmlAsync`, `SaveIconAsync`, and `DetectArchiveTypeAsync` — each accepting an optional `CancellationToken`. See the [async IO guide][async-io] for usage examples and guidance on when to prefer the async API.

[async-io]: ../async-io
[pxml-spec]: https://pandorawiki.org/PXML_specification
