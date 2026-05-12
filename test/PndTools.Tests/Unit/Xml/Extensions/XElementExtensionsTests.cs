// Copyright (c) milkshakeuk. All rights reserved.
// SPDX-License-Identifier: MIT

using System.Xml.Linq;
using PndTools.Xml.Extensions;

namespace PndTools.Tests.Unit.Xml.Extensions;

public class XElementExtensionsTests
{
    [Fact]
    public void LineNumber_NullElement_ReturnsMinusOne()
    {
        // Arrange
        // Act
        var result = ((XElement?)null).LineNumber();

        // Assert
        Assert.Equal(-1, result);
    }

    [Fact]
    public void LineNumber_ElementWithNoLineInfo_ReturnsMinusOne()
    {
        // Arrange
        var element = new XElement("root");

        // Act
        var result = element.LineNumber();

        // Assert
        Assert.Equal(-1, result);
    }

    [Fact]
    public void LineNumber_ElementWithLineInfo_ReturnsLineNumber()
    {
        // Arrange
        var element = XDocument.Parse("<root><child /></root>", LoadOptions.SetLineInfo)
            .Root!.Element("child")!;

        // Act
        var result = element.LineNumber();

        // Assert
        Assert.True(result > 0);
    }

    [Fact]
    public void LinePosition_NullElement_ReturnsMinusOne()
    {
        // Arrange
        // Act
        var result = ((XElement?)null).LinePosition();

        // Assert
        Assert.Equal(-1, result);
    }

    [Fact]
    public void Position_ElementWithLineInfo_ReturnsLineAndPositionString()
    {
        // Arrange
        var element = XDocument.Parse("<root><child /></root>", LoadOptions.SetLineInfo)
            .Root!.Element("child")!;

        // Act
        var result = element.Position();

        // Assert
        Assert.Matches(@"^\d+:\d+$", result);
    }

    [Fact]
    public void Attribute_NullElement_ReturnsDefault()
    {
        // Arrange
        // Act
        var result = ((XElement?)null).Attribute<string>("lang");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void Attribute_MissingAttribute_ReturnsDefault()
    {
        // Arrange
        var element = new XElement("title");

        // Act
        var result = element.Attribute<string>("lang");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void Attribute_PresentAttribute_ReturnsParsedValue()
    {
        // Arrange
        const string expected = "en_US";
        var element = new XElement("title", new XAttribute("lang", "en_US"));

        // Act
        var result = element.Attribute<string>("lang");

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void Attribute_IntAttribute_ReturnsParsedInt()
    {
        // Arrange
        const int expected = 42;
        var element = new XElement("item", new XAttribute("count", "42"));

        // Act
        var result = element.Attribute<int>("count");

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void List_NullEnumerable_ReturnsEmptyList()
    {
        // Arrange
        // Act
        var result = ((IEnumerable<XElement>?)null).List(e => e.Value);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void List_EnumerableWithElements_ProjectsEachElement()
    {
        // Arrange
        var elements = new[]
        {
            new XElement("item", "first"),
            new XElement("item", "second")
        };
        var expected = new[] { "first", "second" };

        // Act
        var result = elements.List(e => e.Value);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void Value_NullElement_ReturnsDefault()
    {
        // Arrange
        // Act
        var result = ((XElement?)null).Value<string>();

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void Value_ElementWithValue_ReturnsParsedValue()
    {
        // Arrange
        const int expected = 7;
        var element = new XElement("count", "7");

        // Act
        var result = element.Value<int>();

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void XElement_ExistingChildName_ReturnsChildElement()
    {
        // Arrange
        var parent = new XElement("parent", new XElement("child", "value"));

        // Act
        var result = parent.XElement("child");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("value", result.Value);
    }

    [Fact]
    public void XElement_NonExistentChildName_ReturnsNull()
    {
        // Arrange
        var parent = new XElement("parent");

        // Act
        var result = parent.XElement("child");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void XElements_MatchingChildren_ReturnsAll()
    {
        // Arrange
        var parent = new XElement("parent",
            new XElement("item", "a"),
            new XElement("item", "b"),
            new XElement("other", "c"));

        // Act
        var result = parent.XElements("item").ToList();

        // Assert
        Assert.Equal(2, result.Count);
    }
}
