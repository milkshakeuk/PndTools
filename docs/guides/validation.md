---
title: Validating PXML
description: Validate PXML metadata against the schema and business rules.
sidebar:
  order: 4
editUrl: 'https://github.com/milkshakeuk/PndTools/edit/master/docs/guides/validation.md'
---

PndTools provides two layers of validation: schema validation against the official PXML XSD, and a set of additional rules that the schema cannot express.

## Schema validation

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

## Additional rules

The schema cannot enforce constraints such as duplicate application IDs or malformed category combinations. `NonSchemaEnforcableValidationExtensions` adds these checks on top of a valid document.

```csharp
var result = validator.Validate(xmlString)
    .ValidateNonSchemaRules();

Console.WriteLine(result.IsValid);
```

## Combining results

Both validation steps return a `ValidationResult`, which can be chained:

```csharp
var result = validator.Validate(xmlString)
    .ValidateNonSchemaRules();

foreach (var error in result.Errors)
{
    Console.WriteLine(error);
}
```
