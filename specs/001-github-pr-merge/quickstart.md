# Quickstart: GitHub PR Merge Automation

**Feature**: 001-github-pr-merge | **Date**: 2026-05-18

## Prerequisites

- Repository is public on GitHub
- GitHub Actions is enabled
- Mergify GitHub App is installed on the repository (install at [mergify.com][mergify])
- A `CODEOWNERS` file exists at the repository root

## Step 1 — Install Mergify

Install the Mergify GitHub App on the repository via the GitHub Marketplace or from the Mergify dashboard. Grant it read/write access to pull requests, contents, and checks.

## Step 2 — Configure Dependabot

Copy the contract at `specs/001-github-pr-merge/contracts/dependabot.yml` to `.github/dependabot.yml`. Adjust the `open-pull-requests-limit` per ecosystem as needed — PRs are merged individually so there is no batch size to align with.

## Step 3 — Deploy Mergify configuration

Copy the contract at `specs/001-github-pr-merge/contracts/mergify.yml` to `.mergify.yml` at the repository root. Update the `check-success` values in every queue rule and pull request rule to match the actual GitHub Actions workflow job names used in CI.

## Step 4 — Configure GitHub Repository Rulesets

In **Settings → Rules → Rulesets**, create the three rulesets defined in `specs/001-github-pr-merge/contracts/rulesets.md`:

1. **Commit Integrity** — require signed commits, require linear history; no bypass actors.
2. **CI Quality Gates** — required status checks; list the same check names used in `.mergify.yml`; disable "Require branches to be up to date" (Mergify satisfies this via `update_method: rebase`); no bypass actors.
3. **Review Gates** — require pull request with codeowner approval and stale-review dismissal; add the Mergify GitHub App as a bypass actor.

Remove any legacy branch protection rules on `main` once the rulesets are active.

## Step 5 — Verify

Open a test PR and confirm:

- The PR cannot be merged without checks passing (Ruleset 2).
- The PR cannot be merged without codeowner approval (Ruleset 3).
- A Dependabot minor/patch PR enqueues automatically in the correct ecosystem queue after checks pass.
- A Dependabot major PR does not auto-enqueue and requires approval.
- Commits on `main` retain their GPG signatures after merging (Ruleset 1 + Mergify fast-forward).

## Tuning queue behaviour

To adjust the number of parallel speculative checks, edit `merge_queue.max_parallel_checks` in `.mergify.yml`. Each PR is merged individually via fast-forward — batching is not used as it is incompatible with the `required_signatures` ruleset and commitlint.

[mergify]: https://mergify.com
