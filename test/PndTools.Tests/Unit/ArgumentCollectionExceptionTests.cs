// Copyright (c) milkshakeuk. All rights reserved.
// SPDX-License-Identifier: MIT

namespace PndTools.Tests.Unit;

public class ArgumentCollectionExceptionTests
{
    [Fact]
    public void ThrowIfNullOrEmpty_NullCollection_ThrowsArgumentNullException()
    {
        // Arrange
        List<string>? collection = null;

        // Act
        // Assert
        Assert.Throws<ArgumentNullException>(() => ArgumentCollectionException.ThrowIfNullOrEmpty(collection));
    }

    [Fact]
    public void ThrowIfNullOrEmpty_EmptyCollection_ThrowsArgumentCollectionException()
    {
        // Arrange
        var collection = new List<string>();

        // Act
        // Assert
        Assert.Throws<ArgumentCollectionException>(() => ArgumentCollectionException.ThrowIfNullOrEmpty(collection));
    }

    [Fact]
    public void ThrowIfNullOrEmpty_CollectionWithItems_DoesNotThrow()
    {
        // Arrange
        var collection = new List<string> { "item" };

        // Act
        // Assert
        ArgumentCollectionException.ThrowIfNullOrEmpty(collection);
    }

    [Fact]
    public void ThrowIfNullOrEmpty_NullCollection_IncludesParamNameInMessage()
    {
        // Arrange
        var myCollection = new List<string>();

        // Act
        var ex = Assert.Throws<ArgumentCollectionException>(() => ArgumentCollectionException.ThrowIfNullOrEmpty(myCollection));

        // Assert
        Assert.Contains("myCollection", ex.Message, StringComparison.Ordinal);
    }
}
