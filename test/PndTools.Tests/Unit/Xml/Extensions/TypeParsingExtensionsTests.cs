// Copyright (c) milkshakeuk. All rights reserved.
// SPDX-License-Identifier: MIT

using PndTools.Xml.Extensions;

namespace PndTools.Tests.Unit.Xml.Extensions;

public class TypeParsingExtensionsTests
{
    [Fact]
    public void Parse_StringInput_ReturnsStringValue()
    {
        // Arrange
        const string expected = "hello";

        // Act
        var result = TypeParsingExtensions.Parse<string>(expected);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void Parse_NullStringInput_ReturnsNull()
    {
        // Arrange
        // Act
        var result = TypeParsingExtensions.Parse<string>(null);

        // Assert
        Assert.Null(result);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Parse_NullOrEmptyInput_NullableValueType_ReturnsNull(string? input)
    {
        // Arrange
        // Act
        var result = TypeParsingExtensions.Parse<int?>(input);

        // Assert
        Assert.Null(result);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Parse_NullOrEmptyInput_NonNullableValueType_ThrowsArgumentException(string? input)
    {
        // Arrange
        // Act
        // Assert
        Assert.Throws<ArgumentException>(() => TypeParsingExtensions.Parse<int>(input));
    }

    [Fact]
    public void Parse_BoolString_ReturnsBool()
    {
        // Arrange
        // Act
        var result = TypeParsingExtensions.Parse<bool>("true");

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Parse_IntString_ReturnsInt()
    {
        // Arrange
        const int expected = 42;

        // Act
        var result = TypeParsingExtensions.Parse<int>("42");

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void Parse_DoubleString_ReturnsDouble()
    {
        // Arrange
        const double expected = 3.14;

        // Act
        var result = TypeParsingExtensions.Parse<double>("3.14");

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void Parse_DecimalString_ReturnsDecimal()
    {
        // Arrange
        const decimal expected = 1.5m;

        // Act
        var result = TypeParsingExtensions.Parse<decimal>("1.5");

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void Parse_DateTimeString_ReturnsDateTime()
    {
        // Arrange
        var expected = new DateTime(2024, 1, 15);

        // Act
        var result = TypeParsingExtensions.Parse<DateTime>("2024-01-15");

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void Parse_GuidString_ReturnsGuid()
    {
        // Arrange
        var expected = new Guid("12345678-1234-1234-1234-123456789abc");

        // Act
        var result = TypeParsingExtensions.Parse<Guid>("12345678-1234-1234-1234-123456789abc");

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void Parse_UnsupportedType_ThrowsInvalidOperationException()
    {
        // Arrange
        // Act
        // Assert
        Assert.Throws<InvalidOperationException>(() => TypeParsingExtensions.Parse<Uri>("http://example.com"));
    }
}
