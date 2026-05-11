// Copyright (c) milkshakeuk. All rights reserved.
// SPDX-License-Identifier: MIT

using System.Xml.Linq;
using PndTools.Xml.Extensions;

namespace PndTools.Tests.Unit.Xml.Extensions;

public class XElementExtensionsTests
{
    [Fact]
    public void LineNumber_WillReturnMinusOneWhenElementIsNull()
    {
        // Arrange
        // Act
        var result = ((XElement?)null).LineNumber();

        // Assert
        Assert.Equal(-1, result);
    }

    [Fact]
    public void LineNumber_WillReturnMinusOneWhenElementHasNoLineInfo()
    {
        // Arrange
        var element = new XElement("root");

        // Act
        var result = element.LineNumber();

        // Assert
        Assert.Equal(-1, result);
    }

    [Fact]
    public void LineNumber_WillReturnLineNumberWhenElementHasLineInfo()
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
    public void LinePosition_WillReturnMinusOneWhenElementIsNull()
    {
        // Arrange
        // Act
        var result = ((XElement?)null).LinePosition();

        // Assert
        Assert.Equal(-1, result);
    }

    [Fact]
    public void Position_WillReturnLineAndPositionAsString()
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
    public void Attribute_WillReturnDefaultWhenElementIsNull()
    {
        // Arrange
        // Act
        var result = ((XElement?)null).Attribute<string>("lang");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void Attribute_WillReturnDefaultWhenAttributeIsMissing()
    {
        // Arrange
        var element = new XElement("title");

        // Act
        var result = element.Attribute<string>("lang");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void Attribute_WillReturnParsedValueWhenAttributeIsPresent()
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
    public void Attribute_WillReturnParsedIntValue()
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
    public void List_WillReturnEmptyListWhenEnumerableIsNull()
    {
        // Arrange
        // Act
        var result = ((IEnumerable<XElement>?)null).List(e => e.Value);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void List_WillProjectEachElement()
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
    public void Value_WillReturnDefaultWhenElementIsNull()
    {
        // Arrange
        // Act
        var result = ((XElement?)null).Value<string>();

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void Value_WillReturnParsedValue()
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
    public void XElement_WillReturnChildElementByLocalName()
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
    public void XElement_WillReturnNullWhenChildNotFound()
    {
        // Arrange
        var parent = new XElement("parent");

        // Act
        var result = parent.XElement("child");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void XElements_WillReturnAllMatchingChildren()
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
