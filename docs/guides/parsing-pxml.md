---
title: Parsing PXML
description: Parse PXML metadata into typed .NET objects and work with applications, titles, categories, and more.
sidebar:
  order: 3
editUrl: 'https://github.com/milkshakeuk/PndTools/edit/master/docs/guides/parsing-pxml.md'
---

`PxmlParser` converts a PXML document into a `Pxml` record containing strongly-typed .NET objects. You can obtain the XML string from a PND stream using `GetPxml`, or load it from a file. The [PXML specification][pxml-spec] documents the full XML format.

`PxmlParser` is an instantiatable class implementing `IPxmlParser` — create an instance directly, or inject it via DI in ASP.NET Core projects (see [ASP.NET Core integration][aspnetcore]).

## Parse from a PND stream

```csharp
using PndTools;
using PndTools.IO.Extensions;

using var stream = File.OpenRead("game.pnd");
var xmlString = stream.GetPxml();
var pxml = new PxmlParser().Parse(xmlString);
```

## Parse from a file

```csharp
var xmlString = File.ReadAllText("/tmp/PXML.xml");
var pxml = new PxmlParser().Parse(xmlString);
```

## The Pxml model

A `Pxml` object has two top-level members:

- `Package` — metadata shared across the whole PND (id, version, author, titles, descriptions, icon)
- `Applications` — one or more applications bundled in the PND, each extending `Package` with its own executable info, categories, licences, and preview images

Most PNDs contain a single application, but the format supports bundling multiple.

## Package metadata

```csharp
var pkg = pxml.Package;

Console.WriteLine(pkg.Id);
Console.WriteLine(pkg.Version?.Major);
Console.WriteLine(pkg.Author?.Name);
Console.WriteLine(pkg.Author?.Website);
```

`PxmlVersion` exposes `Major`, `Minor`, `Release`, `Build`, and `Type` (e.g. `release` or `beta`) — all as nullable strings as the PXML spec does not require them all to be present.

## Localised titles and descriptions

Titles and descriptions are localised by locale identifier. At least one `en_US` entry is required by the PXML spec.

```csharp
var app = pxml.Applications[0];

var english = app.Titles.FirstOrDefault(t => t.Lang == "en_US");
Console.WriteLine(english?.Text);

foreach (var desc in app.Descriptions)
{
    Console.WriteLine($"{desc.Lang}: {desc.Text}");
}
```

## Applications

```csharp
foreach (var app in pxml.Applications)
{
    Console.WriteLine(app.Id);
    Console.WriteLine(app.Info?.Name);   // executable name
    Console.WriteLine(app.Info?.Type);   // e.g. "gui"
    Console.WriteLine(app.Info?.Path);   // path within the archive
}
```

## Categories

Categories follow the [FreeDesktop.org menu specification][freedesktop]. Each `Category` has a `Name` and an optional `Subcategory`.

```csharp
foreach (var category in app.Categories)
{
    Console.WriteLine(category.Name);             // e.g. "Game"
    Console.WriteLine(category.Subcategory?.Name); // e.g. "ActionGame"
}
```

## Licences

```csharp
foreach (var licence in app.Licenses)
{
    Console.WriteLine(licence.Name);          // e.g. "GPL"
    Console.WriteLine(licence.Url);
    Console.WriteLine(licence.SourceCodeUrl);
}
```

## Preview images

`PreviewPics` holds paths to images within the archive. Use `ExtractPreviewPics` on a `PndArchive` to extract them — see [Extracting files][extracting-files].

```csharp
foreach (var pic in app.PreviewPics)
{
    Console.WriteLine(pic.Path);
}
```

[aspnetcore]: /guides/aspnetcore
[extracting-files]: /guides/extracting-files
[freedesktop]: https://specifications.freedesktop.org/menu-spec/latest/
[pxml-spec]: https://pandorawiki.org/PXML_specification
