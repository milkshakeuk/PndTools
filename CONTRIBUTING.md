# Contributing

## Getting started

You will need the [.NET 10 SDK][dotnet] and [Node.js][nodejs] (for commit hooks).

```bash
dotnet restore
npm install
```

`npm install` installs commitlint, markdownlint-cli2, and [prek][prek] — a lightweight Rust-based git hook manager. The `prepare` script then runs `prek install` automatically, wiring up a `pre-commit` hook that lints staged markdown files and a `commit-msg` hook that validates commit messages against the [Conventional Commits][conventional-commits] spec. Both run locally before anything reaches CI.

## Code style

This project enforces code style at build time via `.editorconfig` and `EnforceCodeStyleInBuild`. If the build fails with style violations, run:

```bash
dotnet format
```

Then rebuild to confirm everything passes. The CI pipeline runs `dotnet format --verify-no-changes` and will fail on any unformatted code.

All `.cs` files must begin with the SPDX licence header — `dotnet format` will add it automatically.

## Markdown style

All markdown files are linted with [markdownlint-cli2][markdownlint-cli2] using the rules in `.markdownlint.json`. To check locally:

```bash
npx markdownlint-cli2 "**/*.md"
```

The pre-commit hook runs this automatically on any staged `.md` files.

## Conventions

See [CLAUDE.md][claude-md] for the full set of project conventions covering patterns, naming, XML documentation, and test structure.

## Conventional commits

We use [Conventional Commits][conventional-commits] for every commit in this repository. The structured format serves two purposes.

The first is consistency. A uniform commit style makes the history easier to read, code reviews easier to follow, and blame output more meaningful. When every commit clearly states what changed and why, the log becomes documentation rather than noise.

The second is automation. Tools like [Release Please][release-please] and [semantic-release][semantic-release] parse commit messages to determine the next version number and generate a changelog automatically. A `feat:` commit bumps the minor version, a `fix:` bumps the patch, and a breaking change bumps the major — no manual version decisions needed. The changelog groups commits by type so consumers can see at a glance what changed between releases. This pipeline is enforced locally by the commitlint hook (installed via `prek install`) and again in CI, so a non-conforming message never reaches the main branch.

All commits must follow the [Conventional Commits][conventional-commits] specification. The format is:

```text
<type>(<scope>): <description>

[optional body]

[optional footer(s)]
```

The `scope` is optional but encouraged — use the affected area, e.g. `io`, `xml`, `validation`, `models`.

### Commit types

| Type | Purpose | SemVer impact |
|------|---------|---------------|
| `feat` | New feature or capability | Minor bump (1.**x**.0) |
| `fix` | Bug fix | Patch bump (1.0.**x**) |
| `docs` | Documentation only | None |
| `refactor` | Code change that is neither a fix nor a feature | None |
| `test` | Adding or correcting tests | None |
| `perf` | Performance improvement | Patch bump |
| `build` | Build system, dependencies, or tooling | None |
| `ci` | CI/CD pipeline changes | None |
| `chore` | Housekeeping that does not fit elsewhere | None |
| `revert` | Reverts a previous commit | Depends on reverted commit |

A breaking change is indicated by appending `!` after the type (e.g. `feat!:`) or by adding a `BREAKING CHANGE:` footer. Either triggers a major bump (**x**.0.0).

### Examples

```text
feat(io): add PndArchive.ExtractAll overload accepting a filter predicate

fix(xml): handle null attribute value in TypeParsingExtensions

feat!: remove PndArchive.Open(string path) overload

BREAKING CHANGE: callers must now pass an open Stream directly.
```

Feature commits should include their tests in the same commit.

## Commit message template

To load the project commit template automatically, run:

```bash
git config commit.template .gitmessage
```

The template at `.gitmessage` provides a guided scaffold the first time you open your editor for a new commit.

[claude-md]: CLAUDE.md
[conventional-commits]: https://www.conventionalcommits.org
[dotnet]: https://dotnet.microsoft.com/download
[markdownlint-cli2]: https://github.com/DavidAnson/markdownlint-cli2
[nodejs]: https://nodejs.org
[release-please]: https://github.com/googleapis/release-please
[semantic-release]: https://github.com/semantic-release/semantic-release
