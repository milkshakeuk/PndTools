// Copyright (c) milkshakeuk. All rights reserved.
// SPDX-License-Identifier: MIT

using PndTools.IO;
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
    public void GetPxml_PxmlNotInArchive_ThrowsPndArchiveException()
    {
        // Arrange
        using Stream stream = StreamTestHelper.GenerateRandomStream();

        // Act
        // Assert
        Assert.Throws<PndArchiveException>(() => stream.GetPxml());
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
    public void GetIcon_IconNotInArchive_ThrowsPndArchiveException()
    {
        // Arrange
        using Stream stream = StreamTestHelper.GenerateRandomStream();

        // Act
        // Assert
        Assert.Throws<PndArchiveException>(() => stream.GetIcon());
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
    public async Task GetPxmlAsync_ValidPndStream_ReturnsPxml()
    {
        // Arrange
        var expectedStart = $"<?xml version=\"1.0\" encoding=\"UTF-8\"?>{Environment.NewLine}<PXML";
        const string expectedEnd = "</PXML>";

        using Stream stream = File.OpenRead("Integration/TestCase/SORR.pnd");

        // Act
        var result = await stream.GetPxmlAsync(TestContext.Current.CancellationToken);

        // Assert
        Assert.StartsWith(expectedStart, result, StringComparison.Ordinal);
        Assert.EndsWith(expectedEnd, result, StringComparison.Ordinal);
    }

    [Fact]
    public async Task GetPxmlAsync_PxmlNotInStream_ThrowsPndArchiveException()
    {
        // Arrange
        using Stream stream = StreamTestHelper.GenerateRandomStream();

        // Act
        // Assert
        await Assert.ThrowsAsync<PndArchiveException>(() => stream.GetPxmlAsync(TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task GetPxmlAsync_NullStream_ThrowsArgumentNullException()
    {
        // Arrange
        // Act
        // Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => (null as Stream)!.GetPxmlAsync(TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task GetIconAsync_ValidPndStream_ReturnsIconBytes()
    {
        // Arrange
        var expected = await File.ReadAllBytesAsync("Integration/TestExpectation/SORR.png", TestContext.Current.CancellationToken);

        using Stream stream = File.OpenRead("Integration/TestCase/SORR.pnd");

        // Act
        var result = await stream.GetIconAsync(TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public async Task GetIconAsync_IconNotInStream_ThrowsPndArchiveException()
    {
        // Arrange
        using Stream stream = StreamTestHelper.GenerateRandomStream();

        // Act
        // Assert
        await Assert.ThrowsAsync<PndArchiveException>(() => stream.GetIconAsync(TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task GetIconAsync_NullStream_ThrowsArgumentNullException()
    {
        // Arrange
        // Act
        // Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => (null as Stream)!.GetIconAsync(TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task SavePxmlAsync_ValidPndStream_WritesPxmlToFile()
    {
        // Arrange
        var path = Path.GetTempFileName();

        try
        {
            using Stream stream = File.OpenRead("Integration/TestCase/SORR.pnd");
            var expected = await stream.GetPxmlAsync(TestContext.Current.CancellationToken);

            stream.Position = 0;

            // Act
            await stream.SavePxmlAsync(path, TestContext.Current.CancellationToken);

            // Assert
            var written = await File.ReadAllTextAsync(path, TestContext.Current.CancellationToken);
            Assert.Equal(expected, written);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public async Task SavePxmlAsync_NullStream_ThrowsArgumentNullException()
    {
        // Arrange
        // Act
        // Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => (null as Stream)!.SavePxmlAsync("output.xml", TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task SavePxmlAsync_NullPath_ThrowsArgumentNullException()
    {
        // Arrange
        using Stream stream = File.OpenRead("Integration/TestCase/SORR.pnd");

        // Act
        // Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => stream.SavePxmlAsync(null!, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task SaveIconAsync_ValidPndStream_WritesIconToFile()
    {
        // Arrange
        var path = Path.GetTempFileName();

        try
        {
            using Stream stream = File.OpenRead("Integration/TestCase/SORR.pnd");
            var expected = await stream.GetIconAsync(TestContext.Current.CancellationToken);

            stream.Position = 0;

            // Act
            await stream.SaveIconAsync(path, TestContext.Current.CancellationToken);

            // Assert
            var written = await File.ReadAllBytesAsync(path, TestContext.Current.CancellationToken);
            Assert.Equal(expected, written);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public async Task SaveIconAsync_NullStream_ThrowsArgumentNullException()
    {
        // Arrange
        // Act
        // Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => (null as Stream)!.SaveIconAsync("output.png", TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task SaveIconAsync_NullPath_ThrowsArgumentNullException()
    {
        // Arrange
        using Stream stream = File.OpenRead("Integration/TestCase/SORR.pnd");

        // Act
        // Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => stream.SaveIconAsync(null!, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task DetectArchiveTypeAsync_SquashFsPnd_ReturnsSquashFs()
    {
        // Arrange
        const PndArchiveType expected = PndArchiveType.SquashFs;
        using Stream stream = File.OpenRead("Integration/TestCase/SORR.pnd");

        // Act
        var result = await stream.DetectArchiveTypeAsync(TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public async Task DetectArchiveTypeAsync_IsoPnd_ReturnsIso()
    {
        // Arrange
        const PndArchiveType expected = PndArchiveType.Iso;
        using Stream stream = File.OpenRead("Integration/TestCase/Bump3.pnd");

        // Act
        var result = await stream.DetectArchiveTypeAsync(TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public async Task DetectArchiveTypeAsync_UnknownMagicBytes_ReturnsUnknown()
    {
        // Arrange
        const PndArchiveType expected = PndArchiveType.Unknown;
        using var stream = new MemoryStream([0x00, 0x01, 0x02, 0x03]);

        // Act
        var result = await stream.DetectArchiveTypeAsync(TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public async Task DetectArchiveTypeAsync_ShortStream_ReturnsUnknown()
    {
        // Arrange
        const PndArchiveType expected = PndArchiveType.Unknown;
        using var stream = new MemoryStream([0x01, 0x02]);

        // Act
        var result = await stream.DetectArchiveTypeAsync(TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public async Task DetectArchiveTypeAsync_NullStream_ThrowsArgumentNullException()
    {
        // Arrange
        // Act
        // Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => (null as Stream)!.DetectArchiveTypeAsync(TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task DetectArchiveTypeAsync_AnyStream_PreservesStreamPosition()
    {
        // Arrange
        using var stream = new MemoryStream([0x68, 0x73, 0x71, 0x73]);
        stream.Position = 2;
        const long expected = 2;

        // Act
        await stream.DetectArchiveTypeAsync(TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(expected, stream.Position);
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
