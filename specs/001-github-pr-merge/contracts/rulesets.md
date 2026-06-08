# Contract: GitHub Repository Rulesets

**Feature**: 001-github-pr-merge | **Date**: 2026-05-18

Four rulesets are configured on the `main` branch. Each has a single concern and does not overlap with the others. All four are enforced for all actors except where a bypass is explicitly noted.

## Ruleset 1: Commit Integrity

**Target**: `main` branch
**Bypass actors**: none

| Rule | Configuration |
| --- | --- |
| Require signed commits | enabled |
| Require linear history | enabled |

**Enforces**: FR-003a, FR-003b

---

## Ruleset 2: CI Quality Gates

**Target**: `main` branch
**Bypass actors**: none — Mergify satisfies these conditions before executing the fast-forward push, so no bypass is required

| Rule | Configuration |
| --- | --- |
| Required status checks | checks listed below; "Require branches to be up to date" disabled — Mergify satisfies FR-010 via `update_method: rebase` in the queue |

**Required checks**:

- `commitlint` — conventional commit message format gate
- `lint` — markdownlint zero-error gate
- `dependency-review` — blocks PRs introducing vulnerable packages
- `build` — `dotnet build` zero-warnings gate and `dotnet test` all-pass gate

**Enforces**: FR-001

---

## Ruleset 3: Security Scanning

**Target**: `main` branch
**Bypass actors**: none

| Rule | Configuration |
| --- | --- |
| Required status checks | checks listed below; "Require branches to be up to date" disabled — Mergify satisfies FR-010 via `update_method: rebase` in the queue |
| Require code scanning results | CodeQL; thresholds listed below |

**Required checks**:

- `Analyze (csharp)` — CodeQL static analysis security gate

**Code scanning thresholds**:

- Alerts threshold: `errors` — blocks on error-level findings
- Security alerts threshold: `high_or_higher` — blocks on high and critical severity vulnerabilities

**Enforces**: FR-001

---

## Ruleset 4: Review Gates

**Target**: `main` branch
**Bypass actors**:

- Mergify GitHub App (ID: 10562) — `bypass_mode: always`; allows the fast-forward push to `main` after all conditions have been validated by Mergify
- Dependabot (ID: 29110) — `bypass_mode: pull_request`; prevents approval conditions from being injected into Mergify queue evaluation for Dependabot PRs; the `codeowner approval` merge_protection still gates major updates

| Rule | Configuration |
| --- | --- |
| Require pull request | enabled |
| Required approving review count | 1 |
| Require code owner review | enabled |
| Dismiss stale reviews on push | enabled |
| Require last push approval | enabled |
| Required review thread resolution | enabled |

**Enforces**: FR-002

---

## Notes

Rulesets are configured via the GitHub repository settings UI under **Settings → Rules → Rulesets**, or via the GitHub REST API (`/repos/{owner}/{repo}/rulesets`). They supersede any legacy branch protection rules on `main`; legacy rules should be removed once rulesets are active to avoid conflicting enforcement.
