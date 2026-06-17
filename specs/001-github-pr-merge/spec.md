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

### User Story 4 - Dependabot PR Isolation (Priority: P4)

Dependabot PRs merge independently of one another via a direct fast-forward merge action so that a failing check on one PR cannot block merges of others. Each PR is merged individually; there is no batching and no shared queue.

**Why this priority**: Independence ensures a broken NuGet update cannot hold up an unrelated GitHub Actions update.

**Independent Test**: Can be fully tested by having a Dependabot PR failing CI while another has passing checks — the passing PR should merge independently.

**Acceptance Scenarios**:

1. **Given** a Dependabot NuGet PR failing CI, **When** a Dependabot npm PR passes all checks, **Then** the npm PR merges independently without being blocked.
2. **Given** two Dependabot PRs both passing checks, **When** the first merges and main advances, **Then** Mergify triggers a rebase on the second so it stays current before merging.
3. **Given** a Dependabot PR that has been merged, **When** inspecting the git log, **Then** the PR's commit appears individually with its original authorship and GPG signature intact.

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
- **FR-007**: Dependabot minor/patch PRs MUST be merged directly via fast-forward once all required CI checks pass. No merge queue is used for Dependabot PRs; each PR merges independently so that a failing check on one PR cannot block merges of others.
- **FR-008**: PRs with failing checks MUST remain open and unaffected while other eligible PRs in the same or different ecosystems continue to merge.
- **FR-011a**: The system MUST retry failed merge attempts up to a configurable number of times before dequeuing the PR.
- **FR-011b**: A circuit breaker MUST be applied after a configurable number of consecutive failures, preventing further attempts.
- **FR-011c**: If a merge operation cannot complete after all retries are exhausted, the system MUST post a comment on the affected PR explaining the failure.
- **FR-010**: Every PR MUST be up to date with main before it is merged. Standard PR authors are responsible for keeping their branches current; Mergify will refuse to fast-forward a branch that has fallen behind. For Dependabot PRs, Mergify triggers a branch update via the `update` action whenever the branch falls behind main, relying on Dependabot's own rebase so that commit signatures are preserved.

### Key Entities

- **Pull Request**: A proposed change targeting the main branch; classified as either standard or Dependabot-authored, and in the Dependabot case as major, minor, or patch update.
- **Required Status Check**: A CI job or external check that must report success before a PR is eligible for merge.
- **Code Owner**: A team member or group designated in CODEOWNERS whose approval is required for human-authored and Dependabot major-version PRs.
- **Ecosystem Queue**: A named Mergify merge queue scoped to a single package ecosystem (NuGet, npm, GitHub Actions). PRs enter the matching queue based on their ecosystem label and are merged individually via fast-forward.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Zero standard PRs can be merged without all required checks passing and at least one codeowner approval.
- **SC-002**: Dependabot minor and patch PRs merge without any human action once all checks complete and the queue processes them.
- **SC-003**: Dependabot major PRs follow exactly the same gate as standard PRs — no merge without codeowner approval.
- **SC-004**: A failing check on a Dependabot PR in one ecosystem does not block merges in any other ecosystem.
- **SC-005**: Every dependency update that lands on main is traceable as an individual commit in the repository history.
- **SC-006**: Every commit that lands on the main branch carries a valid GPG signature from its original author.

## Clarifications

### Session 2026-05-18

- Q: When a PR branch has diverged from main and a true fast-forward is impossible without rewriting commits, how should the system behave? → A: Require the PR branch to be up to date with main before merge is permitted; block the merge if the branch has diverged.
- Q: What is the source used to classify Dependabot PRs as major, minor, or patch? → A: GitHub's Dependabot metadata labels (e.g. `version-update:semver-major`, `version-update:semver-minor`, `version-update:semver-patch`).
- Q: How should the system handle transient merge failures? → A: Retry up to a configurable number of times; post a PR comment if all retries are exhausted.
- Q: Should Dependabot PRs be batched or queued? → A: No. Queuing creates speculative branches with Mergify-authored merge commits that fail commitlint; rebase is incompatible with the `required_signatures` ruleset. Dependabot PRs merge directly via a fast-forward merge action once CI passes; Dependabot manages its own branch currency so GPG signatures are never invalidated.

## Assumptions

- GitHub Actions is the CI platform; required status checks are configured as branch protection rules in the repository settings.
- The repository already has a CODEOWNERS file defining at least one code owner.
- Dependabot is already enabled and configured on the repository; this feature adds the merge behaviour on top.
- "Fast forward merge" means the default branch advances without a merge commit. GitHub's "Rebase and merge" strategy is explicitly ruled out because it rewrites commits (changing their SHA and author timestamp), which invalidates any GPG signatures already on those commits. The chosen strategy must preserve commits exactly as signed.
- A Dependabot PR that a human contributor has force-pushed to or amended is treated as a standard PR and loses auto-merge eligibility.
- All PRs are merged individually; there is no batching. fast-forward with batching creates Mergify-authored merge commits incompatible with commitlint, and rebase is incompatible with the required_signatures ruleset.
- Dependabot grouped updates (a single PR per ecosystem group) are intentionally not used. Individual per-dependency PRs ensure each PR carries exactly one semver label, preventing a major dependency bump from blocking auto-merge of unrelated minor/patch updates. Grouped updates would undermine this by mixing severity levels in a single PR, forcing codeowner approval on the entire group if any dependency is a major bump.
- The version bump type (major/minor/patch) is determined exclusively from GitHub's Dependabot metadata labels (`version-update:semver-major`, `version-update:semver-minor`, `version-update:semver-patch`); PR title is not used. PRs where none of these labels are present are treated as major and require codeowner approval.
