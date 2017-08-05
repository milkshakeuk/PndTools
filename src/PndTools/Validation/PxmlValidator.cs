using System.Collections.Generic;
using System.Xml.Linq;
using System.Xml.Schema;
using PndTools.Xml.Extensions;
using PndTools.Validation.Extensions;

namespace PndTools.Validation
{
    public class PxmlValidator
    {
        private XmlSchemaSet SchemaSet { get; } = new XmlSchemaSet();

        public PxmlValidator()
        {
            this.SchemaSet.Add("http://openpandora.org/namespaces/PXML", "Resource/schema.xsd");
        }

        /// <summary>
        ///     Validates the PXML from the <paramref name="input" />.
        /// </summary>
        /// <param name="input">String representation of PXML</param>
        /// <returns>Validation Result <see cref="ValidationResult"/></returns>
        /// <exception cref="System.ArgumentNullException"><paramref name="input" /> is <c>null</c>.</exception>
        /// <exception cref="PndTools.EmptyStringException"><paramref name="input" /> is empty string.</exception>
        /// <exception cref="PndTools.WhitespaceException"><paramref name="input" /> is not empty but only contains <c>whitespace</c>.</exception>
        public ValidationResult Validate(string input)
        {
            Guard.AgainstNullOrWhitespaceArgument(nameof(input), input);

            var document = XDocument.Parse(input, LoadOptions.PreserveWhitespace | LoadOptions.SetLineInfo);

            var errors = new List<string>();

            document.Validate(this.SchemaSet, (o, e) =>
            {
                errors.Add($"PXML {WithoutNamespace(e.Message)} ({e.Exception.LineNumber}:{e.Exception.LinePosition})");
            });

            errors.AddRange(this.ValidateDefaultLangForPackage(document));
            errors.AddRange(this.ValidateDefaultLangForApplications(document));
            errors.AddRange(this.ValidateSubcategoriesAgainstCategory(document));

            return new ValidationResult(errors);
        }

        private IEnumerable<string> ValidateDefaultLangForPackage(XDocument document)
        {
            var package = document.Root.XElement("package");

            var errors = new List<string>();

            package.XElement("titles")?.XElements("title")?.ValidateDefaultLocale(errors);
            package.XElement("descriptions")?.XElements("description")?.ValidateDefaultLocale(errors);

            return errors;
        }

        private IEnumerable<string> ValidateDefaultLangForApplications(XDocument document)
        {
            var applications = document.Root.XElements("application");

            var errors = new List<string>();

            foreach (var application in applications)
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

        private IEnumerable<string> ValidateSubcategoriesAgainstCategory(XDocument document)
        {
            var applications = document.Root.XElements("application");

            var errors = new List<string>();

            foreach (var application in applications)
            {
                application.XElement("categories").XElements("category").ValidateCategorySubcategoryPairing(errors);
            }

            return errors;
        }

        private static string WithoutNamespace(string input)
        {
            return input.Replace(@" in namespace 'http://openpandora.org/namespaces/PXML'", string.Empty)
                        .Replace(@"http://openpandora.org/namespaces/PXML:", string.Empty);
        }

    }
}
