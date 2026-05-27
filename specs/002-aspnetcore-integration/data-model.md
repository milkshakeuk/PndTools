# Data Model: PndTools.AspNetCore Integration Package

**Branch**: `002-aspnetcore-integration` | **Date**: 2026-05-27

## Core Library (`PndTools`) — new types

### IPxmlParser

**Assembly**: `PndTools` | **Namespace**: `PndTools`

```text
IPxmlParser
└── Pxml Parse(string xml)
      throws XmlException        — xml is not valid XML
      throws ArgumentNullException  — xml is null
      throws ArgumentException      — xml is empty or whitespace
```

Implemented by `PxmlParser` (sealed instance class). Stateless — safe for Singleton
registration.

---

### IPxmlValidator

**Assembly**: `PndTools` | **Namespace**: `PndTools.Validation`

```text
IPxmlValidator
└── ValidationResult Validate(string input)
      throws ArgumentNullException  — input is null
      throws ArgumentException      — input is empty or whitespace
```

Implemented by `PxmlValidator` (instance class). Holds a read-only `XmlSchemaSet` loaded once
at construction — safe for Singleton registration under concurrent load.

---

## Companion Package (`PndTools.AspNetCore`) — new types

### IPndArchiveFactory

**Assembly**: `PndTools.AspNetCore` | **Namespace**: `PndTools.AspNetCore`

```text
IPndArchiveFactory
├── PndArchive Open(Stream stream)
│     throws ArgumentNullException      — stream is null
│     throws ArgumentOutOfRangeException — stream.Position != 0 (enforced by PndArchive.Open)
│     throws PndArchiveException         — stream is not a recognised PND archive
│
└── bool TryOpen(Stream stream, out PndArchive? archive)
      returns true  + archive set   — stream opened successfully
      returns false + archive null  — stream null, not at origin, or unrecognised format
      never throws
```

**Ownership contract**: the factory does not dispose the stream or the returned `PndArchive`.
The caller retains ownership of both and is responsible for disposal.

**Stream position contract**: `PndArchive.Open` enforces the position-zero precondition and
throws `ArgumentOutOfRangeException` when `stream.Position != 0`. The factory delegates to
`PndArchive.Open` — it does not duplicate this check. `TryOpen` catches both
`ArgumentOutOfRangeException` and `PndArchiveException` to satisfy its no-throw contract.

Implemented by `PndArchiveFactory` (sealed class). Stateless — safe for Singleton registration.

---

## Relationships

```text
PndTools (core)
  IPxmlParser  ←  PxmlParser
  IPxmlValidator  ←  PxmlValidator
  PndArchive  (existing — Open gains ArgumentOutOfRangeException guard for stream.Position != 0)
  PndArchiveException  (renamed from InvalidPndException)

PndTools.AspNetCore (companion)
  IPndArchiveFactory  ←  PndArchiveFactory
    delegates → PndArchive.Open(Stream)
    TryOpen catches → PndArchiveException, ArgumentOutOfRangeException, ArgumentNullException
  PndToolsServiceCollectionExtensions
    registers → IPxmlParser, IPxmlValidator, IPndArchiveFactory as Singletons
```

## Exception hierarchy (core library)

```text
PndException  (base)
├── PndArchiveException   (renamed from InvalidPndException — invalid/unrecognised archive)
├── PndParseException     (PXML parse failures)
└── PndValidationException  (validation rule failures)
```
