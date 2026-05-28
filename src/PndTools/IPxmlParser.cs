// Copyright (c) milkshakeuk. All rights reserved.
// SPDX-License-Identifier: MIT

using PndTools.Models;

namespace PndTools;

/// <summary>Defines the ability to parse a PXML string into a <see cref="Pxml"/> object graph.</summary>
public interface IPxmlParser
{
    /// <summary>Parses a PXML string into a <see cref="Pxml"/> object graph.</summary>
    /// <param name="xml">The PXML string to parse.</param>
    /// <returns>The parsed <see cref="Pxml"/>.</returns>
    /// <exception cref="System.Xml.XmlException"><paramref name="xml"/> is not valid XML.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="xml"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentException"><paramref name="xml"/> is empty or whitespace.</exception>
    Pxml Parse(string xml);
}
