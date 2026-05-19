# Contract: GitHub Repository Rulesets

**Feature**: 001-github-pr-merge | **Date**: 2026-05-18

Three rulesets are configured on the `main` branch. Each has a single concern and does not overlap with the others. All three are enforced for all actors except where a bypass is explicitly noted.

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
| Required status checks | strict mode (branch must be up to date); checks listed below |

**Required checks** (replace with actual workflow job names):

- `build` — `dotnet build` zero-warnings gate
- `test` — `dotnet test` all-pass gate

Add further checks here as new workflows are introduced. The strict mode setting enforces FR-010 (branch must be up to date with main before merge).

**Enforces**: FR-001, FR-010

---

## Ruleset 3: Review Gates

**Target**: `main` branch
**Bypass actors**: Mergify GitHub App — scoped to this ruleset only, allowing the fast-forward push to `main` after all conditions have been validated by Mergify

| Rule | Configuration |
| --- | --- |
| Require pull request | enabled |
| Required approving review count | 1 |
| Require code owner review | enabled |
| Dismiss stale reviews on push | enabled |

**Enforces**: FR-002

---

## Notes

Rulesets are configured via the GitHub repository settings UI under **Settings → Rules → Rulesets**, or via the GitHub REST API (`/repos/{owner}/{repo}/rulesets`). They supersede any legacy branch protection rules on `main`; legacy rules should be removed once rulesets are active to avoid conflicting enforcement.
