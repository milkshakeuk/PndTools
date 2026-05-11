// Copyright (c) milkshakeuk. All rights reserved.
// SPDX-License-Identifier: MIT

using System;
using System.IO;
using System.Threading.Tasks;
using PndTools.Validation;

namespace PndTools.Tests.Integration.Validation;

public class PxmlValidatorTests
{
    [Theory]
    [InlineData(null, typeof(ArgumentNullException))]
    [InlineData("", typeof(ArgumentException))]
    [InlineData("  ", typeof(ArgumentException))]
    public void Validate_WillThrowExceptionWhenPxmlIsNullOrEmpty(string? pxml, Type exceptionType)
    {
        // Arrange
        var sut = new PxmlValidator();

        // Act
        // Assert
        Assert.Throws(exceptionType, () => sut.Validate(pxml!));
    }

    [Fact]
    public async Task Validate_WillValidateValidPxmlIsValid()
    {
        // Arrange
        var pxml = await File.ReadAllTextAsync("Integration/TestCase/validPxml.xml", TestContext.Current.CancellationToken);
        var sut = new PxmlValidator();

        // Act
        var result = sut.Validate(pxml);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public async Task Validate_WillValidateInvalidPxmlIsNotValid()
    {
        // Arrange
        var pxml = await File.ReadAllTextAsync("Integration/TestCase/invalidPxml.xml", TestContext.Current.CancellationToken);
        var expectedErrors = new[]
        {
            "PXML The 'type' attribute is invalid - The value 'other' is invalid according to its datatype 'releaseType' - The Enumeration constraint failed. (4:54)",
            "PXML The element 'package' has incomplete content. List of possible elements expected: 'titles'. (3:3)",
            "PXML At least one 'description' element with 'lang' attribute of value 'en_US' is required for the 'descriptions' element. (6:6)",
            "PXML The element 'subcategory' with name 'Midi' is invalid for element 'category' with name 'Game'. - See Free Desktop Standards for acceptable values. (39:6)"
        };
        var sut = new PxmlValidator();

        // Act
        var result = sut.Validate(pxml);

        // Assert
        Assert.False(result.IsValid);
        Assert.Equal(4, result.Errors.Count);
        Assert.Contains(expectedErrors[0], result.Errors);
        Assert.Contains(expectedErrors[1], result.Errors);
        Assert.Contains(expectedErrors[2], result.Errors);
        Assert.Contains(expectedErrors[3], result.Errors);
    }
}
