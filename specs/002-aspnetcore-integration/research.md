# Research: PndTools.AspNetCore Integration Package

**Branch**: `002-aspnetcore-integration` | **Date**: 2026-05-27

## Decision: Extension method namespace

**Decision**: Place `PndToolsServiceCollectionExtensions` in the `Microsoft.Extensions.DependencyInjection`
namespace, not `PndTools.AspNetCore`.

**Rationale**: Consumers already have `using Microsoft.Extensions.DependencyInjection` when
configuring services in `Program.cs`. Placing the extension method there means `AddPndTools()`
is discoverable without an additional `using` directive — the same pattern used by
`AddLogging()`, `AddHttpClient()`, `AddHealthChecks()`, and every first-party Microsoft
extension package. The class itself lives in the `PndTools.AspNetCore` assembly; only the
namespace is borrowed.

**Alternatives considered**: `PndTools.AspNetCore` namespace — would require an extra `using`
statement and diverge from the "native feel" goal in the spec.

---

## Decision: Duplicate registration prevention

**Decision**: Use `TryAddSingleton<TService, TImplementation>()` for all three service
registrations rather than `AddSingleton`.

**Rationale**: `TryAddSingleton` is a no-op when a registration for `TService` already exists.
This means calling `AddPndTools()` multiple times (FR-007) produces no duplicates and no
runtime errors. It also respects consumer overrides — if a consumer registers their own
`IPxmlParser` before calling `AddPndTools()`, their registration is preserved.

**Alternatives considered**: Guard with `services.Any(d => d.ServiceType == typeof(IPxmlParser))`
— more verbose, non-atomic, and duplicates what `TryAddSingleton` already does.

---

## Decision: Exception type rename

**Decision**: Rename `InvalidPndException` to `PndArchiveException` as part of the core library
changes in this feature.

**Rationale**: `CLAUDE.md` establishes the naming convention `PndArchiveException` for archive
errors, with `PndException` as the base. `InvalidPndException` predates this convention and
diverges from it. Since no version of the library has been publicly released, this is a clean
rename with no backwards-compatibility cost. The factory's `Open` and `TryOpen` contracts
(FR-002b) reference `PndArchiveException` — unifying on that name keeps the public API
consistent.

**Alternatives considered**: Introduce `PndArchiveException` as a new type alongside
`InvalidPndException` — creates two exception types for the same concept and leaves the old
name in the public API indefinitely.

---

## Decision: PxmlParser refactor approach

**Decision**: Convert `PxmlParser` from a `static` class to a `sealed` instance class. All
private static helper methods remain static (they are pure functions with no instance state).
Only the public `Parse` method moves from `static` to an instance method on `IPxmlParser`.

**Rationale**: `PxmlParser` has no mutable state — the refactor is purely structural. Keeping
the helpers `static` avoids any allocation overhead and passes the zero-warning build (the
analyser would flag instance methods that could be static). The class is `sealed` because there
is no intended inheritance path.

**Alternatives considered**: Leave as `static` and add a separate adapter class — unnecessary
indirection. `PxmlParser` itself is the natural implementation of `IPxmlParser`.

---

## Decision: PxmlValidator — no structural changes

**Decision**: `PxmlValidator` gains `: IPxmlValidator` on the class declaration only. No other
changes.

**Rationale**: The class is already an instance class with a public `Validate(string)` method.
Adding the interface is a one-line change. The `XmlSchemaSet` field remains private; it is
read-only after construction and safe for Singleton use under concurrent load.

---

## Decision: IPndArchiveFactory placement

**Decision**: `IPndArchiveFactory` and its implementation `PndArchiveFactory` live in
`PndTools.AspNetCore`, not the core library.

**Rationale**: Mirrors how `IHttpClientFactory` lives in `Microsoft.Extensions.Http` rather than
`System.Net.Http`. The factory is a hosting/DI concern — it exists to solve the "new is glue"
problem in a DI container. The core library has no dependency on DI infrastructure.

---

## Decision: TryOpen implementation

**Decision**: The stream position precondition (`stream.Position == 0`) is enforced in
`PndArchive.Open` itself, not in the factory. `PndArchive.Open` throws
`ArgumentOutOfRangeException` when `stream.Position != 0`. `PndArchiveFactory.Open` propagates
this exception; `PndArchiveFactory.TryOpen` catches `ArgumentOutOfRangeException`,
`PndArchiveException`, and `ArgumentNullException`, returning `false` for all of them.

**Rationale**: `PndArchive.Open` is the type that requires position zero — placing the guard
there means every direct caller benefits, not just those going through the factory. A single
enforcement point is preferable to duplicated checks. `ArgumentOutOfRangeException` is the
correct type for a numeric argument (stream position) outside its valid range (zero), and it
is meaningfully distinct from `PndArchiveException` ("unrecognised format") — callers of `Open`
can catch them separately if they need to distinguish "wrong usage" from "wrong file".

**Alternatives considered**: Guard in the factory only — direct users of `PndArchive.Open`
would get an obscure failure (misidentified format or DiscUtils exception) rather than a clear
precondition error.
