---
title: Validating PXML
description: Validate PXML metadata against the schema and business rules.
---

PndTools provides two layers of validation: schema validation against the official PXML XSD, and a set of additional rules that the schema cannot express.

## Schema validation

```csharp
using PndTools.Validation;
using System.Xml.Linq;

var xml = XDocument.Load("/tmp/PXML.xml");
var result = PxmlValidator.Validate(xml);

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
var result = PxmlValidator.Validate(xml)
    .ValidateNonSchemaRules();

Console.WriteLine(result.IsValid);
```

## Combining results

Both validation steps return a `ValidationResult`, which can be chained or combined:

```csharp
var result = PxmlValidator.Validate(xml)
    .ValidateNonSchemaRules();

foreach (var error in result.Errors)
{
    Console.WriteLine(error);
}
```
