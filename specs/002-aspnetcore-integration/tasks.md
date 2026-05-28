# Tasks: PndTools.AspNetCore Integration Package

**Input**: Design documents from `specs/002-aspnetcore-integration/`

**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/interfaces.md, quickstart.md

**Note**: Tests are included inline with each phase per the project constitution (III. Test-First —
every new public method must be accompanied by tests in the same commit).

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no shared state)
- **[Story]**: Which user story this task belongs to
- All file paths are relative to the repository root

---

## Phase 1: Setup

**Purpose**: Create new projects and wire them into the solution before any implementation begins

- [ ] T001 Create `src/PndTools.AspNetCore/PndTools.AspNetCore.csproj` — `net10.0`, `Nullable enable`, `TreatWarningsAsErrors true`, `EnforceCodeStyleInBuild true`, `AnalysisMode Recommended`; reference `PndTools` and `Microsoft.Extensions.DependencyInjection.Abstractions`; the `ProjectReference` to `PndTools` is the enforcement mechanism for the edge case "companion installed without core" — NuGet will require the core package as a transitive dependency
- [ ] T002 [P] Create `test/PndTools.AspNetCore.Tests/PndTools.AspNetCore.Tests.csproj` — `net10.0`, xUnit v3, `AnalysisMode All`; reference `PndTools.AspNetCore` and test infrastructure matching `PndTools.Tests`
- [ ] T003 [P] Add both new projects to the solution file (`PndTools.sln`) and verify `dotnet build` succeeds with empty projects

**Checkpoint**: Solution builds with the two new empty projects — no implementation yet

---

## Phase 2: Foundational — Core Library Changes

**Purpose**: Add interfaces, refactor `PxmlParser`, rename the exception type, and add the stream
position guard to `PndArchive.Open`. The companion package depends on all of these.

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

- [ ] T004 [P] Rename `InvalidPndException` → `PndArchiveException` throughout the core library — verify `PndException` base class exists in `src/PndTools/` (create it if not, as `public abstract class PndException : Exception`); update `PndArchiveException` to inherit from `PndException`; update the class declaration in `src/PndTools/IO/Extensions/PndStreamExtensions.cs`, all `throw` sites, all XML doc `<exception>` references, and any usages in `test/PndTools.Tests/`
- [ ] T005 [P] Add `IPxmlParser` interface to `src/PndTools/IPxmlParser.cs` — single method `Pxml Parse(string xml)` with full XML doc comments; place in `PndTools` namespace
- [ ] T006 [P] Add `IPxmlValidator` interface to `src/PndTools/Validation/IPxmlValidator.cs` — single method `ValidationResult Validate(string input)` with full XML doc comments; place in `PndTools.Validation` namespace
- [ ] T007 [P] Add stream position guard to `PndArchive.Open` in `src/PndTools/IO/PndArchive.cs` — throw `ArgumentOutOfRangeException` when `stream.Position != 0`; update XML doc to document the new exception
- [ ] T008 Refactor `PxmlParser` in `src/PndTools/PxmlParser.cs` — change from `static` class to `sealed` instance class implementing `IPxmlParser`; all private helpers remain `static`; instance `Parse` method delegates to existing logic; depends on T005
- [ ] T009 [P] Add `: IPxmlValidator` to `PxmlValidator` class declaration in `src/PndTools/Validation/PxmlValidator.cs`; depends on T006
- [ ] T010 Update `test/PndTools.Tests/Integration/PxmlParserTests.cs` — replace static `PxmlParser.Parse(...)` call sites with instance-based `new PxmlParser().Parse(...)`; depends on T008
- [ ] T011 [P] Add tests for the stream position guard in `test/PndTools.Tests/Integration/IO/PndArchiveTests.cs` — `Open_StreamNotAtOrigin_ThrowsArgumentOutOfRangeException`; depends on T007

**Checkpoint**: `dotnet build` produces zero warnings; `dotnet test` (core project only) all green

---

## Phase 3: User Story 1 — Register PndTools with a single call (Priority: P1) 🎯 MVP

**Goal**: A developer calls `services.AddPndTools()` and all three services become resolvable from
the container with no additional configuration

**Independent Test**: Create a `ServiceCollection`, call `AddPndTools()`, build the provider,
and resolve `IPxmlParser`, `IPxmlValidator`, and `IPndArchiveFactory` — all resolve without error.
Call `AddPndTools()` a second time and confirm no duplicate registrations.

- [ ] T012 [P] [US1] Add `IPndArchiveFactory` interface to `src/PndTools.AspNetCore/IPndArchiveFactory.cs` — `Open(Stream)` and `TryOpen(Stream, out PndArchive?)` with full XML doc comments per `contracts/interfaces.md`; place in `PndTools.AspNetCore` namespace
- [ ] T013 [US1] Implement `PndArchiveFactory` in `src/PndTools.AspNetCore/PndArchiveFactory.cs` — sealed class implementing `IPndArchiveFactory`; `Open` delegates to `PndArchive.Open`; `TryOpen` wraps in try/catch over `PndArchiveException`, `ArgumentOutOfRangeException`, and `ArgumentNullException`; depends on T012
- [ ] T014 [US1] Implement `PndToolsServiceCollectionExtensions.AddPndTools()` in `src/PndTools.AspNetCore/DependencyInjection/PndToolsServiceCollectionExtensions.cs` — place in `Microsoft.Extensions.DependencyInjection` namespace; use `TryAddSingleton` for all three services; return `IServiceCollection` for chaining; XML doc per `contracts/interfaces.md`; depends on T012, T013
- [ ] T015 [US1] Add `ServiceCollectionExtensionsTests` in `test/PndTools.AspNetCore.Tests/ServiceCollectionExtensionsTests.cs` — `AddPndTools_RegistersIPxmlParser_AsSingleton`, `AddPndTools_RegistersIPxmlValidator_AsSingleton`, `AddPndTools_RegistersIPndArchiveFactory_AsSingleton`, `AddPndTools_CalledTwice_DoesNotDuplicateRegistrations`, `AddPndTools_NullServices_ThrowsArgumentNullException`, `AddPndTools_WhenIPxmlParserAlreadyRegistered_DoesNotOverrideExistingRegistration`; depends on T014
- [ ] T016 [P] [US1] Add `PndArchiveFactoryTests` in `test/PndTools.AspNetCore.Tests/PndArchiveFactoryTests.cs` — `Open_ValidStream_ReturnsPndArchive`, `Open_NullStream_ThrowsArgumentNullException`, `Open_StreamNotAtOrigin_ThrowsArgumentOutOfRangeException`, `Open_InvalidStream_ThrowsPndArchiveException`, `TryOpen_ValidStream_ReturnsTrueAndArchive`, `TryOpen_NullStream_ReturnsFalse`, `TryOpen_StreamNotAtOrigin_ReturnsFalse`, `TryOpen_InvalidStream_ReturnsFalse`; depends on T013

**Checkpoint**: US1 fully functional — `AddPndTools()` registers all services; all US1 tests green

---

## Phase 4: User Story 2 — Inject services into controllers and minimal API handlers (Priority: P2)

**Goal**: Confirm that registered services resolve correctly from a hosted DI container in both the
controller and minimal API composition patterns — no additional setup beyond `AddPndTools()`

**Independent Test**: Build a `WebApplication` with only `AddPndTools()` registered; resolve
`IPxmlParser` via constructor injection and as a parameter in a minimal API handler; confirm both
return a functional instance that produces correct output on a known PXML input.

- [ ] T017 [US2] Add `HostedInjectionTests` in `test/PndTools.AspNetCore.Tests/HostedInjectionTests.cs` — use `WebApplicationFactory` or `Host.CreateDefaultBuilder` + `AddPndTools()`; verify `IPxmlParser` resolves and parses a known PXML fixture; verify `IPxmlValidator` resolves and validates the same fixture; verify `IPndArchiveFactory` resolves as a Singleton (same instance across two resolutions); depends on T014

**Checkpoint**: US2 fully functional — both composition patterns verified; all tests green

---

## Phase 5: Polish & Cross-Cutting Concerns

**Purpose**: Documentation updates for all public API changes and the new package

- [ ] T018 [P] Update `docs/getting-started.md` — note that `PxmlParser` is now an instantiatable class and link to the new ASP.NET Core guide
- [ ] T019 [P] Update `docs/guides/parsing-pxml.md` — replace static `PxmlParser.Parse(...)` examples with instance-based usage
- [ ] T020 [P] Update `docs/guides/validation.md` — add `IPxmlValidator` interface reference alongside the existing `PxmlValidator` examples
- [ ] T021 [P] Add `docs/guides/aspnetcore.md` — cover `AddPndTools()` registration, constructor injection, minimal API injection, `IPndArchiveFactory.Open` and `TryOpen` patterns, and stream position contract; content derived from `specs/002-aspnetcore-integration/quickstart.md`
- [ ] T022 Run `dotnet format && dotnet build && dotnet test` — verify zero warnings, zero style violations, all tests pass across both library projects and both test projects; manually confirm `AddPndTools()` startup overhead is negligible by comparing application startup time with and without the call (SC-003 — three `TryAddSingleton` calls have no measurable impact)

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies — start immediately; T002 and T003 can run in parallel with T001
- **Foundational (Phase 2)**: Requires Phase 1 complete — T004, T005, T006, T007 can all run in parallel; T008 depends on T005; T009 depends on T006; T010 depends on T008; T011 depends on T007
- **US1 (Phase 3)**: Requires Phase 2 complete — T012 can start immediately; T013 depends on T012; T014 depends on T012 and T013; T015 depends on T014; T016 can run in parallel with T015
- **US2 (Phase 4)**: Requires Phase 3 complete — T017 depends on T014
- **Polish (Phase 5)**: Requires Phase 4 complete; all doc tasks [P] can run in parallel

### User Story Dependencies

- **US1 (P1)**: Depends on Foundational only
- **US2 (P2)**: Depends on US1 — builds a hosted container using `AddPndTools()`

### Parallel Opportunities Within Phases

- Phase 2: T004, T005, T006, T007 can all run in parallel immediately; T008/T009/T011 can run in parallel once their respective prerequisites complete
- Phase 3: T015 and T016 can run in parallel
- Phase 5: T018, T019, T020, T021 can all run in parallel

---

## Parallel Example: Phase 2

```text
In parallel immediately:
  T004: Rename InvalidPndException → PndArchiveException (+ PndException base)
  T005: Add IPxmlParser interface
  T006: Add IPxmlValidator interface
  T007: Add stream position guard to PndArchive.Open

Once T005 complete:
  T008: Refactor PxmlParser to instance class

Once T006 complete:
  T009: Add : IPxmlValidator to PxmlValidator

Once T008 complete:
  T010: Update PxmlParserTests for instance-based API

Once T007 complete:
  T011: Add stream position guard tests
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup
2. Complete Phase 2: Foundational core library changes
3. Complete Phase 3: US1 — single registration call
4. **STOP and VALIDATE**: Resolve all three services from a `ServiceCollection`; confirm no duplicate registrations on double call
5. Ship — companion package is functional for DI consumers

### Incremental Delivery

1. Setup → solution builds with empty projects
2. Foundational → core library has interfaces, refactored parser, renamed exception, position guard
3. US1 → `AddPndTools()` registers all services (MVP)
4. US2 → injection in hosted container patterns verified
5. Polish → docs complete

---

## Notes

- All `.cs` files require the SPDX licence header — `dotnet format` adds it automatically
- XML documentation is required on all public members (UK English spelling)
- Commit after each phase or logical group following Conventional Commits; `feat:` for the new public API additions, `refactor:` for `PxmlParser` static → instance, `fix:` for the exception rename if it corrects a naming inconsistency
- `dotnet format` must run before each commit to satisfy the pre-commit hook
- FR-001 notes a semver minor bump on the core package — version number changes are a release activity and are not included as implementation tasks
