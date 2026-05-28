# Feature Specification: PndTools.AspNetCore Integration Package

**Feature Branch**: `002-aspnetcore-integration`

**Created**: 2026-05-22

**Status**: Draft

**Input**: User description: "Add a companion NuGet package called PndTools.AspNetCore that makes PndTools easy to use in ASP.NET Core applications. Developers should be able to register relevant PndTools services in one call during app startup, configure behaviour (such as default paths or validation settings) through the standard options pattern, and inject those services directly into controllers, minimal API handlers, or other components without writing any boilerplate themselves. The package should feel native to the ecosystem — following the same conventions developers already know from packages like Microsoft.Extensions.* — so that adopting PndTools in a web or API project is frictionless."

## Clarifications

### Session 2026-05-22 (continued)

- Q: Which services are in scope for DI registration? → A: `IPxmlParser`, `IPxmlValidator`, and `IPndArchiveFactory` — a typed factory that accepts a caller-owned `Stream` and returns a `PndArchive`. The caller retains ownership of both stream and archive; the factory does not dispose either. This prevents the "new is glue" antipattern and provides a testable seam without transferring lifetime responsibility to the library.
- Q: What configurable options are in scope for v1? → A: No configurable options in v1. No options class or configuration delegate is included — an empty POCO with no properties would confuse the API. When configurable behaviour is added in a future version, a new overload accepting a configuration delegate will be added as a non-breaking additive change.
- Q: Should the core library gain `IPxmlParser` and `IPxmlValidator` interfaces as part of this feature? → A: Yes — `IPxmlParser` and `IPxmlValidator` are added to the core `PndTools` library as they describe domain behaviour. `PxmlParser` becomes a standard instantiatable class implementing `IPxmlParser`; the static `Parse` method is removed as there is no released version to maintain backwards compatibility with. `PxmlValidator` implements `IPxmlValidator` with no structural changes. This is a semver minor bump on the core package.
- Q: Where should `IPndArchiveFactory` live? → A: In `PndTools.AspNetCore`, not the core library — consistent with how Microsoft places `IHttpClientFactory` in `Microsoft.Extensions.Http` rather than `System.Net.Http`. The factory is a hosting/DI concern, not a domain concern. `IPxmlParser` and `IPxmlValidator` belong in core because they describe what the types do; `IPndArchiveFactory` belongs in the companion package because it describes how to obtain an instance in a DI context.
- Q: How should US2 and configuration success criteria be handled given v1 has no configurable options? → A: Remove US2, SC-003, and SC-004 entirely. The options class as an extensibility point is an implementation detail; it does not warrant a user story until there is actual behaviour to configure. Stale edge cases referencing file paths and invalid configuration are also removed.
- Q: Should `IPndArchiveFactory` expose an `OpenAsync` method? → A: No. Although `DetectArchiveTypeAsync` exists, `CreateFileSystem` passes the stream directly to `SquashFileSystemReader` or `CDReader` which have no async constructors — so filesystem initialisation is inherently synchronous. An `OpenAsync` would be partially async at best and misleading about what work is actually deferred. The factory exposes `Open(Stream)` only.
- Q: What lifetimes should the registered services use, given concurrent upload scenarios? → A: Singleton for all three.
- Q: Should `IPndArchiveFactory` expose a non-throwing path for invalid archives, given user uploads are a common failure case? → A: Yes — expose both `Open(Stream)` (throws `PndArchiveException`) and `TryOpen(Stream, out PndArchive?)` (returns `false` without throwing), following the .NET BCL `Parse`/`TryParse` convention.
- Q: Should the factory seek the stream to position zero before reading, or require the caller to do so? → A: Precondition — both `Open` and `TryOpen` require the stream to be positioned at its origin. The factory does not seek. Auto-seeking would be a hidden side effect that silently resets caller-controlled stream position and behaves inconsistently for non-seekable streams. An unpositioned stream is treated as invalid input: `TryOpen` returns `false`, `Open` throws `ArgumentOutOfRangeException` (enforced by `PndArchive.Open`).

## User Scenarios & Testing *(mandatory)*

### User Story 1 — Register PndTools with a single call (Priority: P1)

A developer building an ASP.NET Core web or API project wants to start using PndTools without manually wiring up dependencies. They add the companion package, call one method during app startup, and all PndTools services become available for injection throughout the application.

**Why this priority**: This is the core value proposition. Without this, none of the other stories are reachable. A developer who cannot register services in one call has no reason to use the companion package.

**Independent Test**: Add the package to a new ASP.NET Core project, call the registration method in `Program.cs` with no additional configuration, and confirm that a PndTools service can be resolved from the container and used successfully.

**Acceptance Scenarios**:

1. **Given** a new ASP.NET Core project with the companion package installed, **When** the developer calls the registration method with no arguments, **Then** all core PndTools services are registered with appropriate lifetimes and no exception is thrown at startup or resolution time.
2. **Given** PndTools services are registered, **When** a constructor declares a PndTools service as a parameter, **Then** the container resolves it without any additional setup by the developer.
3. **Given** the registration method is called more than once, **When** the application starts, **Then** no duplicate registrations or runtime errors occur.

---

### User Story 2 — Inject services into controllers and minimal API handlers (Priority: P2)

A developer writing a controller action or a minimal API endpoint wants to receive a PndTools service via constructor or parameter injection without any additional setup beyond the initial registration call.

**Why this priority**: The injection experience is the day-to-day developer touchpoint. It validates the end-to-end flow in the two most common composition patterns.

**Independent Test**: Create a controller with a PndTools service in its constructor and a minimal API endpoint with a PndTools service as a parameter; confirm both resolve and execute correctly with only the one-call registration in place.

**Acceptance Scenarios**:

1. **Given** PndTools services are registered, **When** a controller declares a PndTools service via constructor injection, **Then** the framework resolves and supplies the service without any extra configuration.
2. **Given** PndTools services are registered, **When** a minimal API handler declares a PndTools service as a parameter, **Then** the framework resolves and supplies the service correctly.
3. **Given** a PndTools service is injected into a handler, **When** the handler is invoked, **Then** the service behaves identically to one resolved directly from the container.

---

### Edge Cases

- What happens when the companion package is installed but the core PndTools package is not?
- What happens if registration is attempted after the application host has already been built?
- What happens when a stream passed to `Open` or `TryOpen` is not positioned at its origin — `PndArchive.Open` throws `ArgumentOutOfRangeException`; the factory's `Open` propagates it, `TryOpen` catches it and returns `false`.
- What happens when a non-seekable stream is passed — the factory reads from current position; no seek is attempted. If the stream happens to be at a non-zero position and is non-seekable, the caller cannot recover, so the stream position precondition is documented as a caller responsibility.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The core `PndTools` library MUST gain `IPxmlParser` and `IPxmlValidator` interfaces. `PxmlParser` MUST become a standard instantiatable class implementing `IPxmlParser`; the static `Parse` method is removed. `PxmlValidator` MUST implement `IPxmlValidator` with no structural changes. These changes constitute a semver minor bump on the core package.
- **FR-002**: The companion package MUST provide a single extension method on the service collection that registers `IPxmlParser`, `IPxmlValidator`, and `IPndArchiveFactory` as Singletons. All three implementations are stateless or hold only read-only shared state after construction, making Singleton safe under concurrent load. Any future implementation of these interfaces MUST also be thread-safe to honour this contract.
- **FR-002a**: Registered services MUST be injectable into any component that participates in the host's dependency injection container, including controllers, minimal API handlers, middleware, and background services.
- **FR-002b**: `IPndArchiveFactory` MUST be defined in `PndTools.AspNetCore` (not the core library), consistent with the Microsoft pattern of placing factory interfaces in the extensions package rather than the core type library. It MUST expose two synchronous methods: `Open(Stream)`, which returns a `PndArchive` and throws `PndArchiveException` on invalid input; and `TryOpen(Stream, out PndArchive?)`, which returns `false` and sets the out parameter to `null` on invalid input without throwing. No async variants are provided — filesystem initialisation via `DiscUtils` is inherently synchronous. The caller retains ownership of both the stream and the returned archive — the factory does not dispose either. Both methods require the stream to be positioned at its origin; neither the factory nor `PndArchive.Open` seeks. `PndArchive.Open` enforces this precondition directly — it throws `ArgumentOutOfRangeException` when `stream.Position != 0`. The factory's `Open` propagates that exception; `TryOpen` catches it and returns `false`.
- **FR-005**: The registration method takes no parameters. Calling it MUST produce a fully working setup with no required configuration. When configurable behaviour is introduced in a future version, a new overload accepting a configuration delegate will be added — this is a non-breaking additive change, so no placeholder parameter is needed in v1.
- **FR-006**: Reserved for future validation configuration options.
- **FR-007**: Calling the registration method more than once MUST NOT result in duplicate service registrations or runtime errors.
- **FR-008**: The companion package MUST NOT introduce any services, middleware, or behaviour beyond what is needed for PndTools integration — it must not interfere with the host application's existing registrations.
- **FR-009**: The companion package MUST target the same minimum runtime version as the core PndTools package and declare it as a direct dependency.

### Key Entities

- **IPxmlParser**: Interface in `PndTools` describing the ability to parse a PXML string into a `Pxml` object graph. Implemented by `PxmlParser`.
- **IPxmlValidator**: Interface in `PndTools.Validation` describing the ability to validate a PXML string against the OpenPandora schema and non-schema rules. Implemented by `PxmlValidator`.
- **IPndArchiveFactory**: Interface in `PndTools.AspNetCore` for obtaining a `PndArchive` from a caller-owned stream. Implemented by `PndArchiveFactory`. Exposes `Open(Stream)` (throws on failure) and `TryOpen(Stream, out PndArchive?)` (returns false on failure).
- **PndArchive**: Existing disposable type in `PndTools.IO` that provides read access to a PND archive filesystem. Created by the factory; owned and disposed by the caller.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: A developer with no prior knowledge of PndTools internals can go from package installation to a working injected service in under five minutes, following only the package README.
- **SC-002**: All PndTools services resolve correctly from the container with zero additional setup beyond the single registration call.
- **SC-003**: The companion package adds no measurable overhead to application startup time relative to manually registering the same services.

## Assumptions

- The target developer is already familiar with dependency injection and the options pattern; no introductory explanation is needed in the package API.
- Only the services from the core PndTools library that are meaningful in a web or API context are registered; the full set of public services qualifies unless there is a specific reason to exclude one.
- The companion package is distributed as a separate NuGet package (`PndTools.AspNetCore`) rather than bundled into the core package, so that non-web consumers do not take a dependency on the web hosting stack.
- Middleware is out of scope for this feature; the package registers services only.
- Health checks and diagnostics integration are out of scope for this initial version.
