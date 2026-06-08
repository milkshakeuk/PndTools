# Configuration Model: GitHub PR Merge Automation

**Feature**: 001-github-pr-merge | **Date**: 2026-05-18

This feature is configuration-only. There is no application data model. This document describes the configuration entities and their valid values.

## Mergify Queue

Represents a named merge queue in `.mergify.yml`.

| Field | Type | Valid values | Notes |
| --- | --- | --- | --- |
| `name` | string | `standard`, `nuget-deps`, `npm-deps`, `actions-deps` | One queue per ecosystem plus one for standard/major PRs |
| `merge_method` | enum | `fast-forward` | Fixed; preserves GPG signatures; rebase is incompatible with required_signatures ruleset |
| `max_checks_retries` | integer | ≥ 0 | Configurable per FR-011b circuit-breaker intent |

## Mergify Pull Request Rule

Represents a routing rule that enqueues a PR into a named queue.

| Field | Type | Notes |
| --- | --- | --- |
| `name` | string | Human-readable description of the rule |
| `conditions` | list | Conditions that must all be true for the rule to apply |
| `actions.queue.name` | string | Must match a defined queue name |

### Condition vocabulary

| Condition | Meaning |
| --- | --- |
| `base = main` | PR targets the main branch |
| `author = dependabot[bot]` | PR was opened by Dependabot |
| `-author = dependabot[bot]` | PR was NOT opened by Dependabot |
| `label = version-update:semver-patch` | Dependabot patch update label |
| `label = version-update:semver-minor` | Dependabot minor update label |
| `label = version-update:semver-major` | Dependabot major update label |
| `label = dependencies` | PR is a dependency update (any type) |
| `check-success = <name>` | Named CI check has passed |
| `approved-reviews-by = @<team>` | At least one approval from the team |
| `review-requested = @<team>` | Review has been requested from the team |
| `#changes-requested-reviews-by = 0` | No outstanding change requests |

## GitHub Rulesets

Three distinct rulesets are applied to the `main` branch, each with a single concern. The Mergify bypass is scoped only to the ruleset where it is needed.

### Ruleset 1: Commit Integrity

Enforces the quality of commits that land on main. No bypass actors — these constraints apply to everyone including Mergify.

| Rule | Setting | Enforces |
| --- | --- | --- |
| Require signed commits | enabled | FR-003b |
| Require linear history | enabled | FR-003a |

### Ruleset 2: CI Quality Gates

Enforces that all required checks pass and that the branch is up to date before any merge. No bypass actors — Mergify validates these conditions before executing the fast-forward push, so the push only happens when they are already satisfied.

| Rule | Setting | Enforces |
| --- | --- | --- |
| Required status checks | checks enumerated in contracts; "up to date" requirement disabled — Mergify satisfies FR-010 via `update_method: rebase` | FR-001 |

### Ruleset 3: Review Gates

Enforces human approval requirements. Mergify is added as a bypass actor here because rebase merge is executed as a direct push to main, not via a GitHub PR merge action. All other review gate requirements are validated by Mergify before the push is attempted.

| Rule | Setting | Enforces |
| --- | --- | --- |
| Require pull request reviews | `required_approving_review_count: 1`, `require_code_owner_review: true`, `dismiss_stale_reviews_on_push: true` | FR-002 |
| Bypass actor | Mergify GitHub App | Allows fast-forward push after conditions are met |

## Dependabot Configuration

Represents entries in `.github/dependabot.yml`.

| Field | Value | Notes |
| --- | --- | --- |
| `package-ecosystem` | `nuget`, `npm`, `github-actions` | One entry per ecosystem |
| `directory` | `/` | Repo root |
| `schedule.interval` | `weekly` | Reasonable default; adjust as needed |
| `open-pull-requests-limit` | integer | Controls how many open Dependabot PRs exist per ecosystem at once; default 10 |

## State Transitions

### Standard PR

```text
opened → checks running → checks pass + codeowner approves → enqueued (standard queue) → merged
                       → checks fail                       → blocked (stays open)
                       → change requested                  → blocked (stays open)
```

### Dependabot minor/patch PR

```text
opened → checks running → checks pass → enqueued (ecosystem queue, fill window starts)
                                      → window fills or 30 min elapses → batch merged
       → checks fail                  → excluded from batch (stays open)
       → branch diverged from main    → Mergify rebases branch via update_method: rebase (FR-010)
```

### Dependabot major PR

```text
opened → checks running → checks pass + codeowner approves → enqueued (standard queue) → merged
                       → checks fail                       → blocked (stays open)
```
