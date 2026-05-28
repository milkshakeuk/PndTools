---
title: Validating PXML
description: Validate PXML metadata against the schema and business rules.
sidebar:
  order: 4
editUrl: 'https://github.com/milkshakeuk/PndTools/edit/master/docs/guides/validation.md'
---

PndTools validates PXML against the official OpenPandora XSD schema and a set of additional rules that the schema cannot express, such as locale requirements and FreeDesktop.org category/subcategory pairings. Both layers run in a single `Validate` call.

`PxmlValidator` implements `IPxmlValidator` — use it directly, or inject it via DI in ASP.NET Core projects (see [ASP.NET Core integration][aspnetcore]).

## Validate a PXML string

```csharp
using PndTools.Validation;

var xmlString = File.ReadAllText("/tmp/PXML.xml");
var validator = new PxmlValidator();
var result = validator.Validate(xmlString);

if (!result.IsValid)
{
    foreach (var error in result.Errors)
    {
        Console.WriteLine(error);
    }
}
```

## ValidationResult

`ValidationResult` exposes two members:

- `IsValid` — `true` when there are no errors
- `Errors` — a read-only list of error message strings

[aspnetcore]: /guides/aspnetcore
