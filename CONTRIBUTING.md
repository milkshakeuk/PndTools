# Contributing

## Getting started

You will need the [.NET 10 SDK](https://dotnet.microsoft.com/download).

```bash
dotnet restore
dotnet build
dotnet test
```

## Code style

This project enforces code style at build time via `.editorconfig` and `EnforceCodeStyleInBuild`. If the build fails with style violations, run:

```bash
dotnet format
```

Then rebuild to confirm everything passes. The CI pipeline runs `dotnet format --verify-no-changes` and will fail on any unformatted code.

All `.cs` files must begin with the SPDX licence header — `dotnet format` will add it automatically.

## Conventions

See [CLAUDE.md](CLAUDE.md) for the full set of project conventions covering patterns, naming, XML documentation, and test structure.

## Commit style

This project uses [Conventional Commits](https://www.conventionalcommits.org). Feature commits should include their tests in the same commit.
