# Tasks: GitHub PR Merge Automation

**Input**: Design documents from `specs/001-github-pr-merge/`

**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/

**Note**: This is a configuration-only feature. No .NET library code is modified. All tasks produce YAML files or GitHub repository settings changes. True parallelism is limited because most tasks touch the same files (.mergify.yml); [P] is marked where tasks genuinely operate on independent files.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no shared state)
- **[Story]**: Which user story this task belongs to
- All file paths are relative to the repository root

---

## Phase 1: Setup

**Purpose**: Install tooling and gather the inputs needed before any configuration can be written

- [ ] T001 Install the Mergify GitHub App on the repository via the GitHub Marketplace; confirm it appears under Settings → GitHub Apps with read/write access to contents, pull requests, and checks
- [X] T002 [P] Audit `CODEOWNERS` at the repository root — verify every source path under `src/` is covered by at least one owner entry; add any missing entries
- [X] T003 [P] Open `.github/workflows/` and record the exact job names used for the build and test steps — these are required as `check-success` values in `.mergify.yml` and as status check names in Ruleset 2

**Checkpoint**: Mergify is installed, CODEOWNERS is complete, and CI job names are documented

---

## Phase 2: Foundational — Rulesets

**Purpose**: Enforce commit integrity and CI quality at the GitHub layer before Mergify is wired in. These rulesets protect `main` independently of Mergify and MUST be active before any PR merges.

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

- [ ] T004 Configure Ruleset 1 (Commit Integrity) on `main` via Settings → Rules → Rulesets — enable **Require signed commits** and **Require linear history**; set no bypass actors
- [ ] T005 Configure Ruleset 2 (CI Quality Gates) on `main` — enable **Required status checks** in strict mode; add the check names captured in T003; set no bypass actors

**Checkpoint**: `main` now requires signed commits, linear history, and passing CI on an up-to-date branch — enforceable before Mergify executes any merge

---

## Phase 3: User Story 1 — Standard PR Merge Gate (Priority: P1) 🎯 MVP

**Goal**: No standard PR can be merged without all checks passing and at least one codeowner approval; all merges are fast-forward preserving GPG signatures

**Independent Test**: Open a test PR; confirm it cannot be merged before checks pass, cannot be merged without codeowner approval, and once both conditions are met Mergify merges it via fast-forward leaving the commit signature intact on `main`

- [X] T006 [US1] Create `.mergify.yml` at the repository root containing the `standard` queue: `merge_method: fast-forward`, `max_checks_retries: 3`, `merge_conditions` requiring `#approved-reviews-by >= 1`, `#changes-requested-reviews-by = 0`, and `check-success` for each job name from T003
- [X] T007 [US1] Add the `merge standard PRs` pull_request_rule to `.mergify.yml` — conditions: `base = main`, `-author = dependabot[bot]`, approval count, no change requests, all checks pass; action: `queue: standard`
- [ ] T008 [US1] Configure Ruleset 3 (Review Gates) on `main` — enable **Require pull request**, set `required_approving_review_count: 1`, enable **Require code owner review** and **Dismiss stale reviews on push**; add the Mergify GitHub App as a bypass actor
- [X] T009 [US1] Add the `notify on merge failure` pull_request_rule to `.mergify.yml` — condition matching all terminal dequeue reasons (`checks-timeout`, `merge-failed`, `pr-dequeued`); actions: post a comment on the PR explaining the failure and add a `merge-failed` label
- [ ] T010 [US1] Remove any legacy branch protection rules on `main` that conflict with the three new rulesets (Settings → Branches → Branch protection rules)

**Checkpoint**: US1 is fully functional — standard PRs are gated, merged fast-forward, and GPG signatures are preserved on `main`

---

## Phase 4: User Story 2 — Dependabot Minor/Patch Auto-Merge (Priority: P2)

**Goal**: Dependabot minor and patch PRs for NuGet, npm, and GitHub Actions merge automatically once checks pass, without human approval, using per-ecosystem batch queues with a 30-minute fill window

**Independent Test**: Observe a live Dependabot minor or patch PR — after checks pass it should enqueue automatically and merge within 35 minutes without any human action (30-minute fill window plus merge execution time, per SC-002); each dependency update appears as its own commit on `main`

- [X] T011 [P] [US2] Create `.github/dependabot.yml` with three ecosystem entries (`nuget`, `npm`, `github-actions`), each targeting `/`, scheduled weekly, `open-pull-requests-limit: 10`, and a `labels:` entry applying the ecosystem label (`nuget`, `npm`, `github-actions` respectively) — these labels are required for Mergify ecosystem queue routing and are not added by Dependabot by default
- [X] T012 [US2] Add `nuget-deps`, `npm-deps`, and `actions-deps` batch queues to `.mergify.yml` — each with `merge_method: fast-forward`, `batch_size: 10`, `batch_max_wait_time: 30 min`, `max_checks_retries: 3`, and `merge_conditions` requiring `base = main` and all CI checks from T003
- [X] T013 [US2] Add the three auto-merge pull_request_rules to `.mergify.yml` — one per ecosystem (`nuget`, `npm`, `github-actions`) — conditions: `base = main`, `author = dependabot[bot]`, `label ~= ^version-update:semver-(minor|patch)$`, the ecosystem label, and all CI checks; action: `queue` into the matching ecosystem queue

**Checkpoint**: US2 is fully functional — eligible Dependabot minor/patch PRs enqueue automatically into the correct ecosystem queue and batch-merge within the 30-minute window

---

## Phase 5: User Story 3 — Dependabot Major Update Approval Gate (Priority: P3)

**Goal**: Dependabot major-version PRs follow the same approval gate as standard PRs — they do not auto-merge and require codeowner approval

**Independent Test**: Observe a live Dependabot major-version PR — it should remain open after checks pass and only merge once a codeowner approves, following the same path as a standard PR

- [X] T014a [US3] Add the `merge dependabot major updates` pull_request_rule to `.mergify.yml` — conditions: `base = main`, `author = dependabot[bot]`, `label = version-update:semver-major`, `#approved-reviews-by >= 1`, `#changes-requested-reviews-by = 0`, all CI checks; action: `queue: standard`
- [X] T014b [US3] Add the `merge dependabot unlabelled (treat as major)` catch-all pull_request_rule to `.mergify.yml` — conditions: `base = main`, `author = dependabot[bot]`, absence of all three semver labels, `#approved-reviews-by >= 1`, `#changes-requested-reviews-by = 0`, all CI checks; action: `queue: standard` (FR-006)

**Checkpoint**: US3 is fully functional — Dependabot major PRs require approval and route through the standard queue

---

## Phase 6: User Story 4 — Dependabot Batch Merge (Priority: P4)

**Goal**: Multiple eligible Dependabot minor/patch PRs within the same ecosystem land on `main` in a single batch operation, each as its own commit, within the 30-minute fill window

**Independent Test**: With two or more Dependabot minor/patch PRs open in the same ecosystem and all checks passing, confirm they merge together in one batch operation; inspect `git log` to verify each dependency update is a separate commit; confirm a failing PR in one ecosystem does not affect a ready batch in another ecosystem

- [ ] T015 [US4] Verify the batch fill window — trigger or wait for two or more eligible Dependabot PRs in the same ecosystem; confirm they are held until the 30-minute window elapses then merged together in a single Mergify batch operation
- [ ] T016 [US4] Verify per-ecosystem isolation — confirm that a Dependabot PR with a failing check in one ecosystem does not prevent the `nuget-deps`, `npm-deps`, or `actions-deps` queue for a different ecosystem from proceeding
- [ ] T016a [US4] Verify FR-009 batch summary — after a batch merge completes, confirm that Mergify records which PRs were included (via workflow run log or PR comment) in a form that satisfies FR-009
- [ ] T016b [US4] Verify mixed semver label tiebreaker — confirm that a Dependabot PR carrying both `version-update:semver-major` and `version-update:semver-minor` labels is routed to the `standard` queue and requires codeowner approval, not auto-merged (FR-006)

**Checkpoint**: US4 is fully functional — batching, fill window, size cap, and per-ecosystem isolation all behave as specified

---

## Phase 7: Polish & Cross-Cutting Concerns

**Purpose**: End-to-end validation, documentation accuracy, and housekeeping

- [ ] T017 Run the full `specs/001-github-pr-merge/quickstart.md` end-to-end verification checklist — confirm all five verification steps pass against the live repository configuration
- [X] T018 [P] Update `specs/001-github-pr-merge/contracts/mergify.yml` and `contracts/rulesets.md` if any conditions or check names required adjustment during implementation to reflect the final deployed configuration
- [ ] T019 [P] Update the `<!-- SPECKIT START -->` block in `CLAUDE.md` to remove the plan reference once the feature branch is merged to `main`

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies — start immediately; T002 and T003 can run in parallel with T001
- **Foundational (Phase 2)**: Requires Phase 1 complete — T003 job names feed into T004/T005
- **US1 (Phase 3)**: Requires Phase 2 complete — rulesets must be active before Mergify is configured
- **US2 (Phase 4)**: Requires Phase 3 complete — ecosystem queues build on the standard queue; T011 can start in parallel with T012/T013
- **US3 (Phase 5)**: Requires Phase 3 complete — routes Dependabot major PRs to the standard queue from US1
- **US4 (Phase 6)**: Requires Phase 4 complete — validates batching behaviour from US2
- **Polish (Phase 7)**: Requires all desired user stories complete

### User Story Dependencies

- **US1 (P1)**: Depends on Foundational (Phase 2) only — no dependency on other user stories
- **US2 (P2)**: Depends on US1 — ecosystem queues sit alongside the standard queue in the same `.mergify.yml`
- **US3 (P3)**: Depends on US1 — reuses the standard queue created in US1
- **US4 (P4)**: Depends on US2 — validates the batch queues created in US2; US3 can run in parallel with US4

### Parallel Opportunities Within Phases

- Phase 1: T002 and T003 can run in parallel with each other (different files/systems)
- Phase 4: T011 (dependabot.yml) can run in parallel with T012/T013 (.mergify.yml) as they are different files

---

## Parallel Example: Phase 1

```text
Task T001: Install Mergify GitHub App (GitHub UI)

In parallel once T001 is complete:
  Task T002: Audit CODEOWNERS (CODEOWNERS file)
  Task T003: Record CI job names (.github/workflows/)
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup
2. Complete Phase 2: Foundational rulesets
3. Complete Phase 3: US1 — standard PR merge gate
4. **STOP and VALIDATE**: Open a test PR and confirm the full gate works end to end
5. Ship — `main` is now protected with fast-forward merges and GPG preservation

### Incremental Delivery

1. Setup + Foundational → `main` is protected
2. Add US1 → standard PRs gated and fast-forward merged (MVP)
3. Add US2 → Dependabot minor/patch auto-merges in ecosystem batches
4. Add US3 → Dependabot major PRs gated like standard PRs
5. Add US4 → validate and document batch behaviour
6. Polish → end-to-end sign-off

---

## Notes

- Tasks T004, T005, T008, T010 are GitHub UI operations (Settings → Rules / Branches) with no corresponding file in the repository
- Tasks that modify `.mergify.yml` (T006, T007, T009, T012, T013, T014a, T014b) are sequential because they edit the same file; the contracts/ reference file provides the target structure
- Commit after each task or logical group following Conventional Commits (`ci:` type for workflow/ruleset changes, `chore:` for config files)
- Mergify validates `.mergify.yml` syntax automatically on push — watch the Mergify dashboard for configuration errors after each commit
