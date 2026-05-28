---
title: ASP.NET Core integration
description: Register PndTools services with a single call and inject them into controllers and minimal API handlers.
sidebar:
  order: 8
editUrl: 'https://github.com/milkshakeuk/PndTools/edit/master/docs/guides/aspnetcore.md'
---

`PndTools.AspNetCore` is a companion package that registers all PndTools services with a single call, so you can inject them into controllers, minimal API handlers, middleware, and background services without any boilerplate.

## Installation

```bash
dotnet add package PndTools.AspNetCore
```

This package declares `PndTools` as a dependency — no separate install is needed.

## Register services

Call `AddPndTools()` on the service collection in `Program.cs`:

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddPndTools();

var app = builder.Build();
```

All PndTools services (`IPxmlParser`, `IPxmlValidator`, and `IPndArchiveFactory`) are registered as Singletons. The call is idempotent — calling it more than once has no effect.

## Inject into a controller

```csharp
public class PndController(IPxmlParser parser, IPxmlValidator validator) : ControllerBase
{
    [HttpPost("parse")]
    public IActionResult Parse([FromBody] string pxml)
    {
        var result = validator.Validate(pxml);
        if (!result.IsValid)
            return BadRequest(result.Errors);

        var parsed = parser.Parse(pxml);
        return Ok(parsed);
    }
}
```

## Inject into a minimal API handler

```csharp
app.MapPost("/archive/inspect", (IFormFile file, IPndArchiveFactory factory) =>
{
    using var stream = file.OpenReadStream();
    if (!factory.TryOpen(stream, out var archive))
        return Results.BadRequest("Not a valid PND archive.");

    using (archive)
    {
        return Results.Ok(archive.ListFiles());
    }
});
```

## Use Open when failure is unexpected

When invalid input is a programming error rather than a user error, use `Open` and let the exception propagate:

```csharp
app.MapGet("/archive/{id}/files", (string id, IPndArchiveFactory factory) =>
{
    using var stream = GetTrustedArchiveStream(id); // known-good internal source
    using var archive = factory.Open(stream);       // throws PndArchiveException if corrupt
    return Results.Ok(archive.ListFiles());
});
```

## Stream position contract

Both `Open` and `TryOpen` require the stream to be positioned at its origin (`Position == 0`). If the stream has been partially read upstream, seek back before calling:

```csharp
stream.Position = 0;
factory.TryOpen(stream, out var archive);
```

`Open` throws `ArgumentOutOfRangeException` if the stream is not at position zero. `TryOpen` returns `false`.
