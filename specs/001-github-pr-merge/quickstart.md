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

## Step 3 — Deploy Mergify configuration and auto-merge workflow

Copy the contract at `specs/001-github-pr-merge/contracts/mergify.yml` to `.mergify.yml` at the repository root. Update the `check-success` values in the pull request rule to match the actual GitHub Actions workflow job names used in CI.

Copy `.github/workflows/dependabot-auto-merge.yml` from the repository. Ensure the `MILKSHAKE_WRITER_BOT_CLIENT_ID` and `MILSHAKE_WRITER_BOT_APP_PRIVATE_KEY` secrets are configured on the repository. Major Dependabot PRs require any human approval before the workflow will merge them; the Review Gates ruleset enforces codeowner review at the push layer — no additional configuration is needed.

## Step 4 — Configure GitHub Repository Rulesets

In **Settings → Rules → Rulesets**, create the three rulesets defined in `specs/001-github-pr-merge/contracts/rulesets.md`:

1. **Commit Integrity** — require signed commits, require linear history; no bypass actors.
2. **CI Quality Gates** — required status checks; list the same check names used in `.mergify.yml`; disable "Require branches to be up to date" (standard PR authors rebase manually; Dependabot branches are updated via the `update` action); no bypass actors.
3. **Review Gates** — require pull request with codeowner approval and stale-review dismissal; add the Mergify GitHub App as a bypass actor.

Remove any legacy branch protection rules on `main` once the rulesets are active.

## Step 5 — Verify

Open a test PR and confirm:

- The PR cannot be merged without checks passing (Ruleset 2).
- The PR cannot be merged without codeowner approval (Ruleset 3).
- A Dependabot minor/patch PR is auto-approved and fast-forward merged by the workflow once checks pass, with no human action.
- A Dependabot major PR posts an "awaiting approval" comment and only merges once any human approves.
- Commits on `main` retain their GPG signatures after merging (Ruleset 1 + fast-forward push).
- A standard PR branch that has fallen behind main is refused by Mergify until the author rebases manually.

[mergify]: https://mergify.com
