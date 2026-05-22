# Research: GitHub PR Merge Automation

**Feature**: 001-github-pr-merge | **Date**: 2026-05-18

## Decisions

### Merge execution tool

**Decision**: Mergify (free tier)

**Rationale**: Mergify's `merge_method: fast-forward` performs a true fast-forward push to the base branch without rewriting commits, preserving GPG signatures. It provides native batch queue support with configurable fill windows and batch size caps. GitHub's native merge queue cannot satisfy FR-003a + FR-003b simultaneously — its rebase strategy rewrites commits and its merge commit strategy introduces merge commits.

**Alternatives considered**:

- GitHub native merge queue — free and built-in, but cannot preserve GPG signatures with a linear history strategy.
- Custom GitHub Actions workflow — technically capable but high implementation cost; would duplicate what Mergify provides out of the box.

---

### Per-ecosystem queue structure

**Decision**: Separate Mergify queues per package ecosystem (`nuget-deps`, `npm-deps`, `actions-deps`), no batching

**Rationale**: Separate queues ensure a failing CI check in one ecosystem cannot block merges in another. Batching was removed because Dependabot already raises individual PRs per package (except the intentional `ltrdatadiscutils` group), and batching added complexity without a meaningful benefit at this scale. All queues use `merge_method: fast-forward`, `update_method: rebase`, and `branch_protection_injection_mode: none` to prevent Mergify from injecting the Review Gates approval requirement on Dependabot branches.

**Alternatives considered**:

- Single cross-ecosystem queue — simpler but allows an unrelated ecosystem failure to delay an otherwise ready PR.
- Batch queues with fill window — considered and removed; unnecessary complexity for a small repository with infrequent dependency updates.

---

### Dependabot PR classification

**Decision**: GitHub Dependabot metadata labels (`version-update:semver-major`, `version-update:semver-minor`, `version-update:semver-patch`)

**Rationale**: Machine-generated, structured, and present on every Dependabot PR. No parsing logic required. A PR missing all three labels is treated as major (FR-006).

---

### Branch protection enforcement

**Decision**: GitHub Repository Rulesets (not legacy branch protection rules)

**Rationale**: Rulesets support bypass actors, allowing Mergify's GitHub App to be exempted from the "Require pull request" rule so its fast-forward push succeeds, while all other rules (signed commits, status checks, codeowner approval, branch up to date) remain enforced. Legacy branch protection rules do not support granular bypass actor configuration.

**Rules required**:

- `require_signed_commits` — enforces FR-003b at the GitHub layer
- `require_linear_history` — enforces FR-003a; prevents merge commits from any source
- `required_status_checks` (strict mode) — enforces FR-001 and FR-010; strict mode requires the branch to be up to date before merge is permitted
- `required_pull_request_reviews` with `require_code_owner_review: true` and `required_approving_review_count: 1` — enforces FR-002
- Bypass actor: Mergify GitHub App (scoped to the merge push action only)

### Mergify Merge Protections activation

**Decision**: Merge Protections must be explicitly enabled in the Mergify dashboard, and the resulting `Mergify Merge Protections` check must be added as a required status check in a GitHub Ruleset

**Rationale**: Defining `merge_protections` in `.mergify.yml` is not sufficient on its own. Mergify only runs the composite check once Merge Protections are enabled in the Mergify dashboard. Even then, the check is advisory unless `Mergify Merge Protections` is marked as required in a GitHub Ruleset — without that, GitHub will allow a PR to merge even if the check fails. A dedicated ruleset requiring this check ensures the codeowner approval protection on Dependabot major PRs is actually enforced.

---

### Resilience

**Decision**: Rely on Mergify's native resilience; no custom retry logic required

**Rationale**: Mergify uses Redis Streams to persist events. It retries failed GitHub API calls automatically, honours rate limit headers natively, and scales workers to avoid exhausting rate limits. The `max_checks_retries` queue rule setting allows configuring how many times Mergify retries failed CI checks before dequeuing a PR. FR-011a, FR-011b, and FR-011c are satisfied by Mergify's built-in behaviour for transient failures; the PR comment + failed workflow run on final failure is configured via a Mergify `pull_request_rules` action.

---

### Mergify configuration syntax

**Decision**: `merge_conditions` are omitted from all queue rules; `queue_conditions` only.

**Rationale**: Mergify automatically injects conditions from active GitHub Repository Rulesets into its queue evaluation. The four active rulesets (CI Quality Gates, Security Scanning, Review Gates, Commit Integrity) already enforce every condition that would otherwise appear in `merge_conditions`. Repeating them in Mergify configuration is redundant and creates a maintenance burden if check names change. The `merge_protections` success_conditions still list CI checks explicitly because `auto_merge_conditions: true` requires them to evaluate protections and trigger automatic queueing — they serve a different purpose there (signalling readiness) rather than gatekeeping the merge itself.

Note: the `conditions` attribute in `queue_rules` was deprecated on 2026-05-06 and removed on 2026-07-31.

---

### GitHub Ruleset injection into Mergify queue conditions

**Decision**: Rely on Mergify's automatic ruleset injection; do not duplicate ruleset conditions in `merge_conditions`.

**Rationale**: Mergify reads active GitHub Repository Rulesets and injects their requirements directly into queue condition evaluation. This was confirmed by inspecting the live Mergify queue status comment, which showed every CI check condition tagged `[🛡 GitHub repository ruleset rule ...]` and passing without any corresponding `merge_conditions` entry. The Review Gates ruleset (approval requirement) is also injected — controlled per-PR-author via the ruleset bypass list: Mergify has `bypass_mode: always` (needed for its direct fast-forward push to `main`); Dependabot has `bypass_mode: pull_request` (prevents approval injection on minor/patch PRs while retaining it for major PRs via the `codeowner approval` merge_protection).

---

### Dependabot grouped updates

**Decision**: Grouped updates used only for `LTRData.DiscUtils.*`; all other dependencies get individual PRs.

**Rationale**: The three `LTRData.DiscUtils.*` packages (`Iso9660`, `SquashFs`, `Streams`) share a version and must be updated together — separate PRs trigger NU1605 package downgrade errors at build time. A targeted group (`ltrdatadiscutils`) consolidates them into a single PR. All other ecosystems use individual PRs to guarantee each PR carries exactly one semver label, keeping major and minor/patch updates routable to separate queues independently.

**Alternatives considered**:

- No grouping at all — causes NU1605 build failures when `LTRData.DiscUtils.*` packages are bumped individually.
- Full ecosystem grouping — simpler PR list but risks a major update holding minor/patch updates hostage to human review within the same group.

---

### Dependabot NuGet commit message fix

**Decision**: A GitHub Actions workflow (`dependabot-nuget-fix.yml`) replaces the Dependabot NuGet commit with a corrected one via the GitHub REST API, using a dedicated GitHub App (`milkshake-writer-bot`)

**Rationale**: Dependabot NuGet commits use non-conventional message formats (e.g. `Bump X from A to B`) that fail commitlint. The fix creates a replacement commit via `POST /git/commits` with the corrected message, then moves the branch ref via `PATCH /git/refs`. Both calls use the App token.

Two constraints drive the App token requirement. First, GitHub only signs the new commit automatically when no explicit author or committer fields are set — the App token identity owns both, which is what triggers signing. Setting explicit fields (to preserve Dependabot attribution) caused GitHub not to sign the commit. Second, `github.token` force pushes do not fire a `pull_request: synchronize` event — a deliberate GitHub safeguard against circular workflow triggers — so CI would never re-run against the fixed commit. The App token bypasses this restriction.

Credentials (`MILKSHAKE_WRITER_BOT_CLIENT_ID`, `MILSHAKE_WRITER_BOT_APP_PRIVATE_KEY`) are stored as Dependabot secrets so they are accessible to Dependabot-triggered workflows.

**Alternatives considered**:

- Squash merge with PR title as commit message — fixes the message but requires a different merge strategy, ruling out fast-forward.
- GraphQL `createCommitOnBranch` — always signed but appends on top of HEAD rather than replacing the existing commit; leaves the original non-compliant commit in history. This could be worked around by first moving the branch ref back to the parent commit and then calling `createCommitOnBranch`, which would produce a single signed commit on top of the parent — functionally equivalent to the chosen approach but with two calls in opposite directions and a small race window between them.

---

### Queue branch update strategy

**Decision**: `update_method: rebase` on all Mergify queue rules

**Rationale**: The default `update_method: merge` causes Mergify to merge main into its temporary queue branch when the branch falls behind, introducing a merge commit. This triggers dequeue due to the `require_linear_history` ruleset rule. Rebase keeps the queue branch linear.

---

### Manually amended Dependabot PRs

**Decision**: A Dependabot PR that has been force-pushed or amended by a non-Dependabot actor loses auto-merge eligibility and is routed to the standard PR queue

**Rationale**: Matches the assumption in the spec. Implemented in Mergify by conditioning auto-merge routing on `author = dependabot[bot]`; a force-push from another actor changes the head commit author and removes the condition match.
