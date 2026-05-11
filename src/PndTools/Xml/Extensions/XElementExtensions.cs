// Copyright (c) milkshakeuk. All rights reserved.
// SPDX-License-Identifier: MIT

using System.Xml;
using System.Xml.Linq;

namespace PndTools.Xml.Extensions;

/// <summary>Extension methods for working with <see cref="XElement"/> instances.</summary>
public static class XElementExtensions
{
    /// <summary>Returns the line number of <paramref name="element"/>, or <c>-1</c> if unavailable.</summary>
    public static int LineNumber(this XElement? element)
    {
        if (element is not IXmlLineInfo info)
        {
            return -1;
        }

        return info.HasLineInfo() ? info.LineNumber : -1;
    }

    /// <summary>Returns the line position of <paramref name="element"/>, or <c>-1</c> if unavailable.</summary>
    public static int LinePosition(this XElement? element)
    {
        if (element is not IXmlLineInfo info)
        {
            return -1;
        }

        return info.HasLineInfo() ? info.LinePosition : -1;
    }

    /// <summary>Returns the line and position of <paramref name="element"/> as a <c>line:position</c> string.</summary>
    public static string Position(this XElement element) =>
        $"{element.LineNumber()}:{element.LinePosition()}";

    /// <summary>
    /// Returns the first child element with the given local <paramref name="name"/>, respecting the
    /// default namespace of <paramref name="element"/>.
    /// </summary>
    public static XElement? XElement(this XElement element, string name) =>
        element.Element(element.GetDefaultNamespace() + name);

    /// <summary>
    /// Returns all child elements with the given local <paramref name="name"/>, respecting the
    /// default namespace of <paramref name="element"/>.
    /// </summary>
    public static IEnumerable<XElement> XElements(this XElement element, string name) =>
        element.Elements(element.GetDefaultNamespace() + name);

    /// <summary>
    /// Returns the value of the attribute with the given local <paramref name="name"/> on
    /// <paramref name="element"/>, parsed as <typeparamref name="T"/>, or <c>default</c> if the
    /// element or attribute is absent.
    /// </summary>
    /// <typeparam name="T">The type to parse the attribute value as.</typeparam>
    public static T? Attribute<T>(this XElement? element, string name)
    {
        if (element is null)
        {
            return default;
        }

        var attr = element.Attributes().FirstOrDefault(x => x.Name.LocalName == name);
        return attr is null ? default : TypeParsingExtensions.Parse<T>(attr.Value);
    }

    /// <summary>
    /// Projects each element in <paramref name="enumerable"/> using <paramref name="parse"/>,
    /// returning the results as a read-only list. Returns an empty list when
    /// <paramref name="enumerable"/> is <c>null</c>.
    /// </summary>
    public static IReadOnlyList<T> List<T>(this IEnumerable<XElement>? enumerable, Func<XElement, T> parse) =>
        enumerable?.Select(parse).ToList() ?? [];

    /// <summary>
    /// Returns the text content of <paramref name="el"/> parsed as <typeparamref name="T"/>,
    /// or <c>default</c> if the element is <c>null</c>.
    /// </summary>
    /// <typeparam name="T">The type to parse the element value as.</typeparam>
    public static T? Value<T>(this XElement? el) =>
        el is null ? default : TypeParsingExtensions.Parse<T>(el.Value);
}
