using System.Collections.Generic;
using System.Xml.Linq;
using System.Xml.Schema;

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

            return new ValidationResult(errors);
        }

        private static string WithoutNamespace(string input)
        {
            return input.Replace(@" in namespace 'http://openpandora.org/namespaces/PXML'", string.Empty)
                        .Replace(@"http://openpandora.org/namespaces/PXML:", string.Empty);
        }

    }
}
