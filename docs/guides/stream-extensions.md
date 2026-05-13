---
title: Searching streams
description: Use StreamExtensions to search any seekable stream for a byte sequence or UTF-8 string.
---

`StreamExtensions` provides general-purpose stream search methods that PndTools uses internally to locate PXML and magic bytes. They are public and available for use in your own code whenever you need to search a seekable stream for a known byte pattern.

## Find a byte sequence

`Find` returns the zero-based byte offset of the first match, or `-1` if the pattern is not present.

```csharp
using PndTools.IO.Extensions;

using var stream = File.OpenRead("data.bin");

var offset = stream.Find([0x89, 0x50, 0x4e, 0x47]); // PNG magic bytes
Console.WriteLine(offset);
```

## Find a UTF-8 string

Pass a string directly and the method handles the encoding.

```csharp
var offset = stream.Find("<PXML");
```

## Search direction

Both overloads accept an optional `Direction` parameter. `Direction.Backwards` starts from the end of the stream and returns the last match — useful when the content you need is appended to the end of a larger file.

```csharp
var offset = stream.Find("</PXML>", Direction.Backwards);
```

## Read a byte range

`GetBytes` extracts a slice of the stream by start and end position.

```csharp
var data = stream.GetBytes(start: offset, end: offset + 512);
```

Both positions are zero-based. `start` is inclusive, `end` is exclusive. Throws `ArgumentOutOfRangeException` if `end` exceeds the stream length or `start` is negative.

## Async variants

`FindAsync` and `GetBytesAsync` are async counterparts that accept an optional `CancellationToken`. See the [async IO guide][async-io] for usage examples.

[async-io]: /guides/async-io
