# Quickstart: PndTools.AspNetCore

**Feature**: 002-aspnetcore-integration | **Date**: 2026-05-27

## Prerequisites

- .NET 10 ASP.NET Core project
- `PndTools.AspNetCore` NuGet package installed (declares `PndTools` as a dependency — no
  separate install needed)

## Step 1 — Register services

In `Program.cs`, call `AddPndTools()` on the service collection:

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddPndTools();

var app = builder.Build();
```

All PndTools services are now available for injection throughout the application. The call is
idempotent — calling it more than once has no effect.

## Step 2 — Inject into a controller

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

## Step 3 — Inject into a minimal API handler

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

## Step 4 — Use Open when failure is unexpected

When invalid input is a programming error rather than a user error, use `Open` directly and let
the exception propagate:

```csharp
app.MapGet("/archive/{id}/files", (string id, IPndArchiveFactory factory) =>
{
    using var stream = GetTrustedArchiveStream(id); // known-good internal source
    using var archive = factory.Open(stream);       // throws PndArchiveException if corrupt
    return Results.Ok(archive.ListFiles());
});
```

## Stream position contract

Both `Open` and `TryOpen` require the stream to be positioned at its origin (`Position == 0`).
If the stream has been partially read upstream (e.g. for content-type sniffing), seek back before
calling:

```csharp
stream.Position = 0;
factory.TryOpen(stream, out var archive);
```

`Open` throws `ArgumentOutOfRangeException` if the stream is not at position zero. `TryOpen`
returns `false`.

## Verify

- Call `AddPndTools()` in `Program.cs` with no arguments — application starts without error.
- Declare `IPxmlParser` in a controller constructor — the framework resolves and injects it.
- Pass a valid PND stream to `IPndArchiveFactory.TryOpen` — returns `true` and a usable archive.
- Pass an invalid stream to `IPndArchiveFactory.TryOpen` — returns `false`, no exception thrown.
- Call `AddPndTools()` twice — no duplicate registration errors at startup or resolution time.
