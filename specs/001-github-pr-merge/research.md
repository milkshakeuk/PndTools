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

### Batch queue structure

**Decision**: Separate Mergify batch queues per package ecosystem (e.g., `nuget-deps`, `npm-deps`, `actions-deps`)

**Rationale**: Mergify's free tier for public repositories includes the full feature set with no queue count restrictions. Separate queues per ecosystem satisfy clarification Q5 (per-ecosystem batching) and ensure a failing CI check in one ecosystem cannot block an otherwise ready batch in another. Each queue uses the same 30-minute `batch_max_wait_time` fill window and a configurable `batch_size` cap. A shared `standard` queue handles standard PRs and Dependabot major-version PRs.

**Alternatives considered**:

- Single cross-ecosystem queue — simpler configuration but allows an unrelated ecosystem failure to delay an otherwise ready batch; does not satisfy clarification Q5.

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

### Manually amended Dependabot PRs

**Decision**: A Dependabot PR that has been force-pushed or amended by a non-Dependabot actor loses auto-merge eligibility and is routed to the standard PR queue

**Rationale**: Matches the assumption in the spec. Implemented in Mergify by conditioning auto-merge routing on `author = dependabot[bot]`; a force-push from another actor changes the head commit author and removes the condition match.
