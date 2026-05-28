---
title: Using async and await
description: Use the async API for non-blocking IO in ASP.NET Core, background services, and other async contexts.
sidebar:
  order: 6
editUrl: 'https://github.com/milkshakeuk/PndTools/edit/master/docs/guides/async-io.md'
---

Every IO method in PndTools has an async counterpart. The `CancellationToken` parameter is optional on all of them, so you can omit it in scripts and short-lived tools, or wire it up in ASP.NET Core and hosted services where cancellation matters.

Prefer the async API in ASP.NET Core controllers, minimal API handlers, Blazor components, and anywhere else you are already in an async context. The sync methods are a better fit for console tools, scripts, and benchmarks where async overhead is not warranted.

## Reading PXML and the icon

```csharp
using PndTools;
using PndTools.IO.Extensions;

using var stream = File.OpenRead("game.pnd");

var xmlString = await stream.GetPxmlAsync();
var parser = new PxmlParser();
var pxml = parser.Parse(xmlString);

Console.WriteLine(pxml.Applications[0].Id);
```

```csharp
var png = await stream.GetIconAsync();
await File.WriteAllBytesAsync("/tmp/icon.png", png);
```

To write directly to a path without buffering the bytes yourself:

```csharp
await stream.SavePxmlAsync("/tmp/PXML.xml");
await stream.SaveIconAsync("/tmp/icon.png");
```

## Detecting the archive type

```csharp
var archiveType = await stream.DetectArchiveTypeAsync();
Console.WriteLine(archiveType); // SquashFs or Iso
```

## Extracting files from the archive

```csharp
using PndTools.IO;

using var stream = File.OpenRead("game.pnd");
using var archive = PndArchive.Open(stream);

await archive.ExtractFileAsync("/PXML.xml", "/tmp/PXML.xml");
await archive.ExtractAllAsync("/tmp/output");
```

`ExtractFilesAsync` accepts any `IEnumerable<string>` of internal paths:

```csharp
var files = archive.ListFiles();
var images = files.Where(f => f.EndsWith(".png"));

await archive.ExtractFilesAsync(images, "/tmp/output");
```

`ExtractPreviewPicsAsync` extracts images referenced in the PXML, returning the list of paths that were actually written:

```csharp
using var stream = File.OpenRead("game.pnd");
using var archive = PndArchive.Open(stream);

var xmlString = await stream.GetPxmlAsync();
var parser = new PxmlParser();
var pxml = parser.Parse(xmlString);

var extracted = await archive.ExtractPreviewPicsAsync(pxml, "/tmp/previews");

foreach (var path in extracted)
{
    Console.WriteLine(path);
}
```

## Searching a stream

`FindAsync` and `GetBytesAsync` mirror the sync `StreamExtensions` methods:

```csharp
using PndTools.IO.Extensions;

using var stream = File.OpenRead("data.bin");

var offset = await stream.FindAsync("<PXML");
var data = await stream.GetBytesAsync(offset, offset + 512);
```

`FindAsync` accepts the same `Direction` parameter as the [synchronous][stream-extensions-direction] version:

```csharp
// Forward (default) — first match from the start
var offset = await stream.FindAsync("<PXML", Direction.Forward);

// Backwards — last match from the end
var offset = await stream.FindAsync("</PXML>", Direction.Backwards);
```

`FindAsync` also accepts `ReadOnlyMemory<byte>` for raw byte patterns.

## Passing a CancellationToken

Every async method accepts `CancellationToken cancellationToken = default` as its last parameter. In ASP.NET Core, pass the token injected into your action method or minimal API handler — it is cancelled automatically when the client disconnects or the request times out.

```csharp
app.MapGet("/pxml", async (IFormFile file, CancellationToken cancellationToken) =>
{
    await using var stream = file.OpenReadStream();
    var xmlString = await stream.GetPxmlAsync(cancellationToken);
    return Results.Text(xmlString, "application/xml");
});
```

In a hosted service, use the token passed to `ExecuteAsync`:

```csharp
protected override async Task ExecuteAsync(CancellationToken stoppingToken)
{
    using var stream = File.OpenRead("game.pnd");
    await stream.SavePxmlAsync("/tmp/PXML.xml", stoppingToken);
}
```

[stream-extensions-direction]: /guides/stream-extensions#search-direction
