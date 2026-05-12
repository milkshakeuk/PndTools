// Copyright (c) milkshakeuk. All rights reserved.
// SPDX-License-Identifier: MIT

using System.IO;
using PndTools.IO.Extensions;
using PndTools.Tests.Helpers;

namespace PndTools.Tests.Integration.IO.Extensions;

public class PndStreamExtensionsTests
{
    [Fact]
    public void GetPxml_ValidPndStream_ReturnsPxml()
    {
        // Arrange
        var expectedStart = $"<?xml version=\"1.0\" encoding=\"UTF-8\"?>{Environment.NewLine}<PXML";
        const string expectedEnd = "</PXML>";

        using Stream stream = File.OpenRead("Integration/TestCase/SORR.pnd");

        // Act
        var result = stream.GetPxml();

        // Assert
        Assert.StartsWith(expectedStart, result, StringComparison.Ordinal);
        Assert.EndsWith(expectedEnd, result, StringComparison.Ordinal);
    }

    [Fact]
    public void GetIcon_ValidPndStream_ReturnsIconBytes()
    {
        // Arrange
        var expected = File.ReadAllBytes("Integration/TestExpectation/SORR.png");

        using Stream stream = File.OpenRead("Integration/TestCase/SORR.pnd");

        // Act
        var result = stream.GetIcon();

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void SavePxml_ValidPndStream_WritesPxmlToFile()
    {
        // Arrange
        var path = Path.GetTempFileName();

        try
        {
            using Stream stream = File.OpenRead("Integration/TestCase/SORR.pnd");
            var expectedPxml = stream.GetPxml();

            stream.Position = 0;

            // Act
            stream.SavePxml(path);

            // Assert
            var written = File.ReadAllText(path);
            Assert.Equal(expectedPxml, written);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void SaveIcon_ValidPndStream_WritesIconToFile()
    {
        // Arrange
        var path = Path.GetTempFileName();

        try
        {
            using Stream stream = File.OpenRead("Integration/TestCase/SORR.pnd");
            var expectedIcon = stream.GetIcon();

            stream.Position = 0;

            // Act
            stream.SaveIcon(path);

            // Assert
            var written = File.ReadAllBytes(path);
            Assert.Equal(expectedIcon, written);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void GetPxml_PxmlNotInArchive_ThrowsInvalidPndException()
    {
        // Arrange
        using Stream stream = StreamTestHelper.GenerateRandomStream();

        // Act
        // Assert
        Assert.Throws<InvalidPndException>(() => stream.GetPxml());
    }

    [Fact]
    public void GetPxml_NullStream_ThrowsArgumentNullException()
    {
        // Arrange
        // Act
        // Assert
        Assert.Throws<ArgumentNullException>(() => (null as Stream)!.GetPxml());
    }

    [Fact]
    public void GetIcon_IconNotInArchive_ThrowsInvalidPndException()
    {
        // Arrange
        using Stream stream = StreamTestHelper.GenerateRandomStream();

        // Act
        // Assert
        Assert.Throws<InvalidPndException>(() => stream.GetIcon());
    }

    [Fact]
    public void GetIcon_NullStream_ThrowsArgumentNullException()
    {
        // Arrange
        // Act
        // Assert
        Assert.Throws<ArgumentNullException>(() => (null as Stream)!.GetIcon());
    }

    [Fact]
    public void SavePxml_NullStream_ThrowsArgumentNullException()
    {
        // Arrange
        // Act
        // Assert
        Assert.Throws<ArgumentNullException>(() => (null as Stream)!.SavePxml("output.xml"));
    }

    [Fact]
    public void SaveIcon_NullStream_ThrowsArgumentNullException()
    {
        // Arrange
        // Act
        // Assert
        Assert.Throws<ArgumentNullException>(() => (null as Stream)!.SaveIcon("output.png"));
    }

    [Fact]
    public void DetectArchiveType_LittleEndianMagicBytes_ReturnsSquashFs()
    {
        // Arrange
        using var stream = new MemoryStream([0x68, 0x73, 0x71, 0x73]); // "hsqs"

        // Act
        var result = stream.DetectArchiveType();

        // Assert
        Assert.Equal(PndArchiveType.SquashFs, result);
    }

    [Fact]
    public void DetectArchiveType_BigEndianMagicBytes_ReturnsSquashFs()
    {
        // Arrange
        using var stream = new MemoryStream([0x73, 0x71, 0x73, 0x68]); // "sqsh"

        // Act
        var result = stream.DetectArchiveType();

        // Assert
        Assert.Equal(PndArchiveType.SquashFs, result);
    }

    [Fact]
    public void DetectArchiveType_Iso9660MagicBytes_ReturnsIso()
    {
        // Arrange
        const int isoOffset = 0x8001;
        byte[] isoMagic = [0x43, 0x44, 0x30, 0x30, 0x31]; // "CD001"
        var buffer = new byte[isoOffset + isoMagic.Length + 1];
        Array.Copy(isoMagic, 0, buffer, isoOffset, isoMagic.Length);
        using var stream = new MemoryStream(buffer);

        // Act
        var result = stream.DetectArchiveType();

        // Assert
        Assert.Equal(PndArchiveType.Iso, result);
    }

    [Fact]
    public void DetectArchiveType_UnknownMagicBytes_ReturnsUnknown()
    {
        // Arrange
        using var stream = new MemoryStream([0x00, 0x01, 0x02, 0x03]);

        // Act
        var result = stream.DetectArchiveType();

        // Assert
        Assert.Equal(PndArchiveType.Unknown, result);
    }

    [Fact]
    public void DetectArchiveType_ShortStream_ReturnsUnknown()
    {
        // Arrange
        using var stream = new MemoryStream([0x01, 0x02]);

        // Act
        var result = stream.DetectArchiveType();

        // Assert
        Assert.Equal(PndArchiveType.Unknown, result);
    }

    [Fact]
    public void DetectArchiveType_NullStream_ThrowsArgumentNullException()
    {
        // Arrange
        // Act
        // Assert
        Assert.Throws<ArgumentNullException>(() => (null as Stream)!.DetectArchiveType());
    }

    [Fact]
    public void DetectArchiveType_AnyStream_PreservesStreamPosition()
    {
        // Arrange
        using var stream = new MemoryStream([0x68, 0x73, 0x71, 0x73]);
        stream.Position = 2;
        const long expected = 2;

        // Act
        stream.DetectArchiveType();

        // Assert
        Assert.Equal(expected, stream.Position);
    }

    [Theory]
    [InlineData("Integration/TestCase/SORR.pnd", PndArchiveType.SquashFs)]
    [InlineData("Integration/TestCase/Bump3.pnd", PndArchiveType.Iso)]
    [InlineData("Integration/TestCase/abbaye.pnd", PndArchiveType.SquashFs)]
    public void DetectArchiveType_RealPndFile_ReturnsCorrectArchiveType(string path, PndArchiveType expected)
    {
        // Arrange
        using var stream = File.OpenRead(path);

        // Act
        var result = stream.DetectArchiveType();

        // Assert
        Assert.Equal(expected, result);
    }
}
