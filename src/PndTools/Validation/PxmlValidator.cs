// Copyright (c) milkshakeuk. All rights reserved.
// SPDX-License-Identifier: MIT

using System.Xml.Linq;
using System.Xml.Schema;
using PndTools.Validation.Extensions;
using PndTools.Xml.Extensions;

namespace PndTools.Validation;

/// <summary>
/// Validates a PXML string against the OpenPandora schema and additional rules that cannot be
/// expressed in XSD, such as FreeDesktop.org category/subcategory pairings and locale requirements.
/// </summary>
public class PxmlValidator
{
    private const string SchemaNamespace = "http://openpandora.org/namespaces/PXML";
    private const string SchemaResourceName = "PndTools.Resource.schema.xsd";
    private static readonly string NamespaceInClause = $" in namespace '{SchemaNamespace}'";
    private static readonly string NamespacePrefix = $"{SchemaNamespace}:";

    private readonly XmlSchemaSet _schemaSet = LoadSchemaSet();

    private static XmlSchemaSet LoadSchemaSet()
    {
        var schemaSet = new XmlSchemaSet();
        using var stream = typeof(PxmlValidator).Assembly.GetManifestResourceStream(SchemaResourceName)
            ?? throw new InvalidOperationException($"Embedded resource '{SchemaResourceName}' not found.");
        schemaSet.Add(SchemaNamespace, System.Xml.XmlReader.Create(stream));
        return schemaSet;
    }

    /// <summary>Validates a PXML string.</summary>
    /// <param name="input">The PXML string to validate.</param>
    /// <returns>
    /// A <see cref="ValidationResult"/> whose <see cref="ValidationResult.IsValid"/> is <c>true</c>
    /// when the PXML is valid, or <c>false</c> with <see cref="ValidationResult.Errors"/> populated.
    /// </returns>
    /// <exception cref="ArgumentNullException"><paramref name="input"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentException"><paramref name="input"/> is empty or whitespace only.</exception>
    public ValidationResult Validate(string input)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(input);

        var document = XDocument.Parse(input, LoadOptions.PreserveWhitespace | LoadOptions.SetLineInfo);
        var errors = new List<string>();

        document.Validate(_schemaSet, (_, e) =>
            errors.Add($"PXML {WithoutNamespace(e.Message)} ({e.Exception.LineNumber}:{e.Exception.LinePosition})")
        );

        errors.AddRange(ValidateDefaultLangForPackage(document));
        errors.AddRange(ValidateDefaultLangForApplications(document));
        errors.AddRange(ValidateSubcategoriesAgainstCategory(document));

        return new ValidationResult(errors);
    }

    private static List<string> ValidateDefaultLangForPackage(XDocument document)
    {
        var package = document.Root!.XElement("package");
        var errors = new List<string>();

        package?.XElement("titles")?.XElements("title").ValidateDefaultLocale(errors);
        package?.XElement("descriptions")?.XElements("description").ValidateDefaultLocale(errors);

        return errors;
    }

    private static List<string> ValidateDefaultLangForApplications(XDocument document)
    {
        var errors = new List<string>();

        foreach (var application in document.Root!.XElements("application"))
        {
            var titles = application.XElement("titles")?.XElements("title")
                      ?? application.XElements("title");

            var descriptions = application.XElement("descriptions")?.XElements("description")
                            ?? application.XElements("description");

            titles.ValidateDefaultLocale(errors);
            descriptions.ValidateDefaultLocale(errors);
        }

        return errors;
    }

    private static List<string> ValidateSubcategoriesAgainstCategory(XDocument document)
    {
        var errors = new List<string>();

        foreach (var application in document.Root!.XElements("application"))
        {
            application.XElement("categories")!.XElements("category").ValidateCategorySubcategoryPairing(errors);
        }

        return errors;
    }

    private static string WithoutNamespace(string input) =>
        input.Replace(NamespaceInClause, string.Empty)
             .Replace(NamespacePrefix, string.Empty);
}
