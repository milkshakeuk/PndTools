// Copyright (c) milkshakeuk. All rights reserved.
// SPDX-License-Identifier: MIT

using PndTools.IO;

namespace PndTools.AspNetCore.Tests;

public class PndArchiveFactoryTests
{
    private readonly PndArchiveFactory _factory = new();

    [Fact]
    public void Open_ValidStream_ReturnsPndArchive()
    {
        // Arrange
        using var stream = File.OpenRead("TestCase/SORR.pnd");

        // Act
        using var archive = _factory.Open(stream);

        // Assert
        Assert.NotNull(archive);
    }

    [Fact]
    public void Open_NullStream_ThrowsArgumentNullException()
    {
        // Arrange
        // Act
        // Assert
        Assert.Throws<ArgumentNullException>(() => _factory.Open(null!));
    }

    [Fact]
    public void Open_StreamNotAtOrigin_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        using var stream = new MemoryStream([0x00, 0x01, 0x02, 0x03]);
        stream.Position = 1;

        // Act
        // Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => _factory.Open(stream));
    }

    [Fact]
    public void Open_InvalidStream_ThrowsPndArchiveException()
    {
        // Arrange
        using var stream = new MemoryStream([0x00, 0x01, 0x02, 0x03]);

        // Act
        // Assert
        Assert.Throws<PndArchiveException>(() => _factory.Open(stream));
    }

    [Fact]
    public void TryOpen_ValidStream_ReturnsTrueAndArchive()
    {
        // Arrange
        using var stream = File.OpenRead("TestCase/SORR.pnd");

        // Act
        var result = _factory.TryOpen(stream, out var archive);
        using (archive)
        {
            // Assert
            Assert.True(result);
            Assert.NotNull(archive);
        }
    }

    [Fact]
    public void TryOpen_NullStream_ReturnsFalse()
    {
        // Arrange
        // Act
        var result = _factory.TryOpen(null!, out var archive);
        using (archive)
        {
            // Assert
            Assert.False(result);
            Assert.Null(archive);
        }
    }

    [Fact]
    public void TryOpen_StreamNotAtOrigin_ReturnsFalse()
    {
        // Arrange
        using var stream = new MemoryStream([0x00, 0x01, 0x02, 0x03]);
        stream.Position = 1;

        // Act
        var result = _factory.TryOpen(stream, out var archive);
        using (archive)
        {
            // Assert
            Assert.False(result);
            Assert.Null(archive);
        }
    }

    [Fact]
    public void TryOpen_InvalidStream_ReturnsFalse()
    {
        // Arrange
        using var stream = new MemoryStream([0x00, 0x01, 0x02, 0x03]);

        // Act
        var result = _factory.TryOpen(stream, out var archive);
        using (archive)
        {
            // Assert
            Assert.False(result);
            Assert.Null(archive);
        }
    }
}
