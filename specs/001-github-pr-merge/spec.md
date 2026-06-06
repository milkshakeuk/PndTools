# Feature Specification: GitHub PR Merge Automation

**Feature Branch**: `001-github-pr-merge`

**Created**: 2026-05-18

**Status**: Draft

**Input**: User description: "I want to add support for fast forward merges from pull requests in github, this has to support dependabot pull requests. when handling standard pull requests no merge should be allowed until all checks have passed and the PR has been approved by the codeowner. when handling dependabot pull requests it should automatically merge when its minor or patch update but wait for approval if its a major update. Ideally it should be able to batch dependabot updates into a single merge (not a single commit)."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Standard PR Merge Gate (Priority: P1)

A contributor opens a pull request against the main branch. The repository enforces that no merge is permitted until every required status check has passed and at least one code owner has approved the PR. If checks are still running or a reviewer has requested changes, the merge button remains unavailable.

**Why this priority**: This is the foundational safety gate for all human-authored changes. Without it, broken or unreviewed code can reach the main branch.

**Independent Test**: Can be fully tested by opening a PR, attempting to merge before checks pass, then again before codeowner approval, and finally after both conditions are satisfied — each attempt produces a clear outcome.

**Acceptance Scenarios**:

1. **Given** a PR with all checks passing, **When** no codeowner has approved, **Then** merging is blocked with a clear reason.
2. **Given** a PR with codeowner approval, **When** one or more required checks are still failing, **Then** merging is blocked.
3. **Given** a PR with all checks passing and at least one codeowner approval, **When** a merge is triggered, **Then** the PR is merged using a fast-forward strategy.
4. **Given** a PR where a reviewer has requested changes after approval, **When** a merge is attempted, **Then** merging is blocked until the review is re-approved.

---

### User Story 2 - Dependabot Minor/Patch Auto-Merge (Priority: P2)

Dependabot opens a PR for a dependency update that is classified as a minor or patch version bump. Once all required checks pass the PR merges automatically without requiring any human review or approval.

**Why this priority**: Automating low-risk dependency updates reduces maintenance toil and keeps dependencies current without blocking contributor attention.

**Independent Test**: Can be fully tested by observing a Dependabot PR for a minor or patch update: it should merge automatically after checks pass with no human action required.

**Acceptance Scenarios**:

1. **Given** a Dependabot PR for a patch update, **When** all required checks pass, **Then** the PR is merged automatically without human approval.
2. **Given** a Dependabot PR for a minor update, **When** all required checks pass, **Then** the PR is merged automatically without human approval.
3. **Given** a Dependabot PR for a minor update, **When** one or more checks are failing, **Then** the PR is not merged and remains open for investigation.
4. **Given** a Dependabot PR that has been auto-merged, **When** reviewing the git log, **Then** the merge appears as a distinct commit preserving the Dependabot authorship.

---

### User Story 3 - Dependabot Major Update Approval Gate (Priority: P3)

Dependabot opens a PR for a dependency update that is classified as a major version bump. The PR does not auto-merge; a codeowner must review and approve it before it can be merged.

**Why this priority**: Major version bumps often include breaking changes that require human judgement, so they follow the same gate as standard PRs.

**Independent Test**: Can be fully tested by observing a Dependabot major-version PR: it should remain open after checks pass and only merge once a codeowner approves.

**Acceptance Scenarios**:

1. **Given** a Dependabot PR for a major update, **When** all checks pass but no codeowner has approved, **Then** the PR is not merged automatically.
2. **Given** a Dependabot PR for a major update, **When** a codeowner approves and all checks pass, **Then** the PR is merged.
3. **Given** a Dependabot PR for a major update, **When** a codeowner requests changes, **Then** the PR remains blocked until re-approved.

---

### User Story 4 - Dependabot Batch Merge (Priority: P4)

Multiple open Dependabot PRs for minor/patch updates are grouped and merged together in a single merge operation. Each update remains as its own commit in the history, but they land on the main branch together rather than triggering a separate CI run per PR.

**Why this priority**: Batching reduces CI noise and keeps the commit graph tidy when many dependencies update simultaneously.

**Independent Test**: Can be fully tested by having two or more eligible Dependabot PRs open simultaneously and verifying that they are merged together in one operation while each retains its individual commit.

**Acceptance Scenarios**:

1. **Given** three open Dependabot PRs all with passing checks, **When** the 30-minute fill window closes, **Then** all three are merged in a single merge operation.
2. **Given** a batch of Dependabot PRs, **When** one PR has a failing check, **Then** that PR is excluded from the batch and the remaining eligible PRs are merged.
3. **Given** a merged batch, **When** inspecting the git log, **Then** each dependency update appears as a separate commit rather than a single squashed commit.
4. **Given** a merged batch, **When** the merge completes, **Then** a summary is posted or logged listing which PRs were included.

---

### Edge Cases

- What happens when a Dependabot PR is for a dependency where the version bump type (major/minor/patch) cannot be determined?
- How does the system handle a Dependabot PR that mixes multiple dependencies where version bump types differ?
- What happens when a codeowner who approved a PR is later removed from the CODEOWNERS file before the merge completes?
- How does the system behave if GitHub's merge API returns a transient error during an auto-merge attempt?
- What happens if a Dependabot PR is manually edited by a contributor — does it still qualify for auto-merge?

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The repository MUST block merging of any standard (non-Dependabot) PR until all required status checks report success.
- **FR-002**: The repository MUST block merging of any standard PR until at least one code owner has approved with no outstanding change requests.
- **FR-003a**: All PRs MUST be merged in a way that preserves a linear commit history without introducing merge commits.
- **FR-003b**: All commits reaching the main branch MUST retain their original GPG signatures. No merge strategy that re-authors or rewrites commits is permitted, as doing so invalidates existing signatures.
- **FR-004**: Dependabot PRs for minor or patch version updates MUST be merged automatically once all required checks pass, without requiring human approval.
- **FR-005**: Dependabot PRs for major version updates MUST follow the same approval gate as standard PRs (all checks pass and codeowner approval required).
- **FR-006**: The system MUST classify Dependabot PR update types (major, minor, patch) using GitHub's Dependabot metadata labels (`version-update:semver-major`, `version-update:semver-minor`, `version-update:semver-patch`). A PR where none of these labels are present MUST be treated as a major update and require codeowner approval. Where a PR carries multiple semver labels simultaneously (e.g. a grouped update spanning both minor and major dependencies), the highest severity label takes precedence — `semver-major` overrides `semver-minor`, which overrides `semver-patch`.
- **FR-007a**: The system MUST support batching multiple eligible Dependabot PRs into a single merge operation while preserving individual commits per dependency update. PRs are grouped by package ecosystem (e.g. NuGet, npm, GitHub Actions) and each ecosystem batch is merged independently. Batching uses a fill window of up to 30 minutes from when the first eligible PR in an ecosystem becomes ready; once the window closes, all currently eligible PRs in that ecosystem are merged together.
- **FR-007b**: The maximum number of PRs included in a single batch MUST be configurable. PRs that exceed the cap remain open and are eligible for the next batch window.
- **FR-008**: PRs excluded from a batch due to failing checks MUST remain open and unaffected.
- **FR-011a**: The system MUST retry failed merge API calls using exponential backoff, honouring GitHub's rate limit response headers (`Retry-After`, `X-RateLimit-Reset`) before each retry attempt.
- **FR-011b**: A circuit breaker MUST be applied after a configurable number of consecutive failures, preventing further attempts until the next batch window.
- **FR-011c**: If a merge operation cannot complete after all retries are exhausted, the system MUST post a comment on the affected PR explaining the failure and fail the workflow run to surface the issue in CI.
- **FR-009**: A summary of which PRs were included in a batch merge MUST be recorded (for example, as a comment on each merged PR or in a workflow run log).
- **FR-010**: Every PR MUST be up to date with main before it is merged. Mergify satisfies this automatically for all PRs by rebasing the branch against main as part of queue processing (`update_method: rebase`).

### Key Entities

- **Pull Request**: A proposed change targeting the main branch; classified as either standard or Dependabot-authored, and in the Dependabot case as major, minor, or patch update.
- **Required Status Check**: A CI job or external check that must report success before a PR is eligible for merge.
- **Code Owner**: A team member or group designated in CODEOWNERS whose approval is required for human-authored and Dependabot major-version PRs.
- **Batch**: A grouped set of eligible Dependabot minor/patch PRs within a single package ecosystem that are merged together in one operation. Each ecosystem produces its own independent batch.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Zero standard PRs can be merged without all required checks passing and at least one codeowner approval.
- **SC-002**: Dependabot minor and patch PRs merge without any human action within 35 minutes of all checks completing (up to 30 minutes for the batch fill window to close, plus merge execution time).
- **SC-003**: Dependabot major PRs follow exactly the same gate as standard PRs — no merge without codeowner approval.
- **SC-004**: When two or more eligible Dependabot PRs are open simultaneously, they are merged in a single batch operation rather than sequentially.
- **SC-005**: Each dependency update in a batch merge is traceable as an individual commit in the repository history.
- **SC-006**: Every commit that lands on the main branch carries a valid GPG signature from its original author.

## Clarifications

### Session 2026-05-18

- Q: When a PR branch has diverged from main and a true fast-forward is impossible without rewriting commits, how should the system behave? → A: Require the PR branch to be up to date with main before merge is permitted; block the merge if the branch has diverged.
- Q: What triggers a Dependabot batch merge? → A: A time-window approach — the system waits up to 30 minutes for eligible PRs to accumulate before merging the batch. A configurable maximum batch size caps how many PRs land in a single operation.
- Q: What is the source used to classify Dependabot PRs as major, minor, or patch? → A: GitHub's Dependabot metadata labels (e.g. `version-update:semver-major`, `version-update:semver-minor`, `version-update:semver-patch`).
- Q: How should the system handle transient merge failures (API errors, rate limits)? → A: Resilience-first — retry with exponential backoff honouring GitHub rate limit headers, apply a circuit breaker after a configurable number of consecutive failures, then post a PR comment and fail the workflow run if the operation still cannot complete.
- Q: Should Dependabot batch merges group PRs across all ecosystems or per ecosystem? → A: Per ecosystem — PRs are grouped by ecosystem (e.g. NuGet, npm, GitHub Actions) and each ecosystem batch is merged independently.

## Assumptions

- GitHub Actions is the CI platform; required status checks are configured as branch protection rules in the repository settings.
- The repository already has a CODEOWNERS file defining at least one code owner.
- Dependabot is already enabled and configured on the repository; this feature adds the merge behaviour on top.
- "Fast forward merge" means the default branch advances without a merge commit. GitHub's "Rebase and merge" strategy is explicitly ruled out because it rewrites commits (changing their SHA and author timestamp), which invalidates any GPG signatures already on those commits. The chosen strategy must preserve commits exactly as signed.
- A Dependabot PR that a human contributor has force-pushed to or amended is treated as a standard PR and loses auto-merge eligibility.
- Batch merging is limited to Dependabot minor/patch PRs; standard PRs and Dependabot major PRs are always merged individually.
- Dependabot grouped updates (a single PR per ecosystem group) are intentionally not used. Individual per-dependency PRs ensure each PR carries exactly one semver label, preventing a major dependency bump from blocking auto-merge of unrelated minor/patch updates within the same ecosystem. The separate queue structure (major PRs → `standard` queue; minor/patch PRs → ecosystem queues) already isolates them, but grouped updates would undermine this by mixing severity levels in a single PR.
- The version bump type (major/minor/patch) is determined exclusively from GitHub's Dependabot metadata labels (`version-update:semver-major`, `version-update:semver-minor`, `version-update:semver-patch`); PR title is not used. PRs where none of these labels are present are treated as major and require codeowner approval.
