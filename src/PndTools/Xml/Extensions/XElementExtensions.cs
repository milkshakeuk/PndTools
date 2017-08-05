using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

namespace PndTools.Xml.Extensions
{
    public static class XElementExtensions
    {
        public static int LineNumber(this XElement element)
        {
            if (element == null)
            {
                return -1;
            }

            var el = element as IXmlLineInfo;
            return el.HasLineInfo() ? el.LineNumber : -1;
        }

        public static int LinePosition(this XElement element)
        {
            if (element == null)
            {
                return -1;
            }

            var el = element as IXmlLineInfo;
            return el.HasLineInfo() ? el.LinePosition : -1;
        }

        public static string Position(this XElement element)
        {
            return $"{element.LineNumber()}:{element.LinePosition()}";
        }

        public static XElement XElement(this XElement element, string name)
        {
            return element.Element(element.GetDefaultNamespace() + name);
        }

        public static IEnumerable<XElement> XElements(this XElement element, string name)
        {
            return element.Elements(element.GetDefaultNamespace() + name);
        }

        public static T Attribute<T>(this XElement element, string name)
        {
            if (element == null)
            {
                return (T)(object)null;
            }

            var attr = element.Attributes().FirstOrDefault(x => x.Name.LocalName == name);
            return attr == null ? (T)(object)null : TypeParsingExtensions.Parse<T>(attr.Value);
        }

        public static IList<T> List<T>(this IEnumerable<XElement> enumerable, Func<XElement, T> parse)
        {
            return enumerable == null ? null : enumerable.Select(parse).ToList();
        }

        public static T Value<T>(this XElement el)
        {
            if (el == null)
            {
                return (T)(object)null;
            }

            return TypeParsingExtensions.Parse<T>(el.Value);
        }
    }
}