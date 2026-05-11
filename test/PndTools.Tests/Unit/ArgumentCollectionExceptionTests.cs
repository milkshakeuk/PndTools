// Copyright (c) milkshakeuk. All rights reserved.
// SPDX-License-Identifier: MIT

namespace PndTools.Tests.Unit;

public class ArgumentCollectionExceptionTests
{
    [Fact]
    public void ThrowIfNullOrEmpty_WillThrowArgumentNullExceptionWhenCollectionIsNull()
    {
        // Arrange
        List<string>? collection = null;

        // Act
        // Assert
        Assert.Throws<ArgumentNullException>(() => ArgumentCollectionException.ThrowIfNullOrEmpty(collection));
    }

    [Fact]
    public void ThrowIfNullOrEmpty_WillThrowArgumentCollectionExceptionWhenCollectionIsEmpty()
    {
        // Arrange
        var collection = new List<string>();

        // Act
        // Assert
        Assert.Throws<ArgumentCollectionException>(() => ArgumentCollectionException.ThrowIfNullOrEmpty(collection));
    }

    [Fact]
    public void ThrowIfNullOrEmpty_WillNotThrowWhenCollectionHasItems()
    {
        // Arrange
        var collection = new List<string> { "item" };

        // Act
        // Assert
        ArgumentCollectionException.ThrowIfNullOrEmpty(collection);
    }

    [Fact]
    public void ThrowIfNullOrEmpty_WillIncludeParamNameInExceptionMessage()
    {
        // Arrange
        var myCollection = new List<string>();

        // Act
        var ex = Assert.Throws<ArgumentCollectionException>(() => ArgumentCollectionException.ThrowIfNullOrEmpty(myCollection));

        // Assert
        Assert.Contains("myCollection", ex.Message);
    }
}
