using System.IO;
using PndTools.IO.Extensions;
using PndTools.Tests.Helpers;

namespace PndTools.Tests.Intergration.IO.Extensions;

public class PndStreamExtensionsTests
{
    [Fact]
    public void GetPxml_WillReturnPxmlFromPndFileStream()
    {
        // Arrange
        var expectedStart = $"<?xml version=\"1.0\" encoding=\"UTF-8\"?>{Environment.NewLine}<PXML";
        const string expectedEnd = "</PXML>";

        using Stream stream = File.OpenRead("Intergration/TestCase/SORR.pnd");

        // Act
        var result = stream.GetPxml();

        // Assert
        Assert.StartsWith(expectedStart, result);
        Assert.EndsWith(expectedEnd, result);
    }

    [Fact]
    public void GetIcon_WillReturnIconBytesFromPndFile()
    {
        // Arrange
        var expected = File.ReadAllBytes("Intergration/TestExpectation/SORR.png");

        using Stream stream = File.OpenRead("Intergration/TestCase/SORR.pnd");

        // Act
        var result = stream.GetIcon();

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void SavePxml_WillWritePxmlToFile()
    {
        // Arrange
        var path = Path.GetTempFileName();

        try
        {
            using Stream stream = File.OpenRead("Intergration/TestCase/SORR.pnd");
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
    public void SaveIcon_WillWriteIconToFile()
    {
        // Arrange
        var path = Path.GetTempFileName();

        try
        {
            using Stream stream = File.OpenRead("Intergration/TestCase/SORR.pnd");
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
    public void GetPxml_WillThrowInvalidPndExceptionIfPxmlNotFound()
    {
        // Arrange
        using Stream stream = StreamTestHelper.GenerateRandomStream();

        // Act
        // Assert
        Assert.Throws<InvalidPndException>(() => stream.GetPxml());
    }

    [Fact]
    public void GetPxml_WillThrowNullArgumentExceptionWhenNullStreamIsSupplied()
    {
        // Arrange
        // Act
        // Assert
        Assert.Throws<ArgumentNullException>(() => (null as Stream)!.GetPxml());
    }

    [Fact]
    public void GetIcon_WillThrowInvalidPndExceptionIfIconNotFound()
    {
        // Arrange
        using Stream stream = StreamTestHelper.GenerateRandomStream();

        // Act
        // Assert
        Assert.Throws<InvalidPndException>(() => stream.GetIcon());
    }

    [Fact]
    public void GetIcon_WillThrowNullArgumentExceptionWhenNullStreamIsSupplied()
    {
        // Arrange
        // Act
        // Assert
        Assert.Throws<ArgumentNullException>(() => (null as Stream)!.GetIcon());
    }

    [Fact]
    public void SavePxml_WillThrowArgumentNullExceptionWhenNullStreamIsSupplied()
    {
        // Arrange
        // Act
        // Assert
        Assert.Throws<ArgumentNullException>(() => (null as Stream)!.SavePxml("output.xml"));
    }

    [Fact]
    public void SaveIcon_WillThrowArgumentNullExceptionWhenNullStreamIsSupplied()
    {
        // Arrange
        // Act
        // Assert
        Assert.Throws<ArgumentNullException>(() => (null as Stream)!.SaveIcon("output.png"));
    }

    [Fact]
    public void DetectArchiveType_WillReturnSquashFsForLittleEndianMagic()
    {
        // Arrange
        using var stream = new MemoryStream([0x68, 0x73, 0x71, 0x73]); // "hsqs"

        // Act
        var result = stream.DetectArchiveType();

        // Assert
        Assert.Equal(PndArchiveType.SquashFs, result);
    }

    [Fact]
    public void DetectArchiveType_WillReturnSquashFsForBigEndianMagic()
    {
        // Arrange
        using var stream = new MemoryStream([0x73, 0x71, 0x73, 0x68]); // "sqsh"

        // Act
        var result = stream.DetectArchiveType();

        // Assert
        Assert.Equal(PndArchiveType.SquashFs, result);
    }

    [Fact]
    public void DetectArchiveType_WillReturnIsoForIso9660Magic()
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
    public void DetectArchiveType_WillReturnUnknownWhenMagicBytesDoNotMatch()
    {
        // Arrange
        using var stream = new MemoryStream([0x00, 0x01, 0x02, 0x03]);

        // Act
        var result = stream.DetectArchiveType();

        // Assert
        Assert.Equal(PndArchiveType.Unknown, result);
    }

    [Fact]
    public void DetectArchiveType_WillReturnUnknownWhenStreamIsTooShort()
    {
        // Arrange
        using var stream = new MemoryStream([0x01, 0x02]);

        // Act
        var result = stream.DetectArchiveType();

        // Assert
        Assert.Equal(PndArchiveType.Unknown, result);
    }

    [Fact]
    public void DetectArchiveType_WillThrowArgumentNullExceptionWhenStreamIsNull()
    {
        // Arrange
        // Act
        // Assert
        Assert.Throws<ArgumentNullException>(() => (null as Stream)!.DetectArchiveType());
    }

    [Fact]
    public void DetectArchiveType_WillNotAlterStreamPosition()
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
    [InlineData("Intergration/TestCase/SORR.pnd", PndArchiveType.SquashFs)]
    [InlineData("Intergration/TestCase/Bump3.pnd", PndArchiveType.Iso)]
    [InlineData("Intergration/TestCase/abbaye.pnd", PndArchiveType.SquashFs)]
    public void DetectArchiveType_WillReturnCorrectTypeForRealPndFiles(string path, PndArchiveType expected)
    {
        // Arrange
        using var stream = File.OpenRead(path);

        // Act
        var result = stream.DetectArchiveType();

        // Assert
        Assert.Equal(expected, result);
    }
}
