using System.IO;
using PndTools.IO;
using PndTools.IO.Extensions;
using PndTools.Models;

namespace PndTools.Tests.Intergration.IO;

public class PndArchiveTests
{
    [Fact]
    public void Open_WillThrowArgumentNullExceptionWhenStreamIsNull()
    {
        // Arrange
        // Act
        // Assert
        Assert.Throws<ArgumentNullException>(() => PndArchive.Open(null!));
    }

    [Fact]
    public void Open_WillThrowInvalidPndExceptionWhenFileTypeIsUnknown()
    {
        // Arrange
        using var stream = new MemoryStream([0x00, 0x01, 0x02, 0x03]);

        // Act
        // Assert
        Assert.Throws<InvalidPndException>(() => PndArchive.Open(stream));
    }

    [Fact]
    public void Open_WillSetArchiveTypeToSquashFsForSquashFsPnd()
    {
        // Arrange
        using var stream = File.OpenRead("Intergration/TestCase/SORR.pnd");

        // Act
        using var archive = PndArchive.Open(stream);

        // Assert
        Assert.Equal(PndArchiveType.SquashFs, archive.ArchiveType);
    }

    [Fact]
    public void Open_WillSetArchiveTypeToIsoForIsoPnd()
    {
        // Arrange
        using var stream = File.OpenRead("Intergration/TestCase/Bump3.pnd");

        // Act
        using var archive = PndArchive.Open(stream);

        // Assert
        Assert.Equal(PndArchiveType.Iso, archive.ArchiveType);
    }

    [Fact]
    public void ListFiles_WillReturnFilesFromSquashFsPnd()
    {
        // Arrange
        using var stream = File.OpenRead("Intergration/TestCase/SORR.pnd");
        using var archive = PndArchive.Open(stream);

        // Act
        var result = archive.ListFiles();

        // Assert
        Assert.NotEmpty(result);
    }

    [Fact]
    public void ListFiles_WillReturnFilesFromIsoPnd()
    {
        // Arrange
        using var stream = File.OpenRead("Intergration/TestCase/Bump3.pnd");
        using var archive = PndArchive.Open(stream);

        // Act
        var result = archive.ListFiles();

        // Assert
        Assert.NotEmpty(result);
    }

    [Fact]
    public void ExtractFile_WillExtractFileFromSquashFsPnd()
    {
        // Arrange
        var outputPath = Path.GetTempFileName();

        try
        {
            using var stream = File.OpenRead("Intergration/TestCase/SORR.pnd");
            using var archive = PndArchive.Open(stream);
            var internalPath = archive.ListFiles().First();

            // Act
            archive.ExtractFile(internalPath, outputPath);

            // Assert
            Assert.True(new FileInfo(outputPath).Length > 0);
        }
        finally
        {
            File.Delete(outputPath);
        }
    }

    [Fact]
    public void ExtractFile_WillExtractFileFromIsoPnd()
    {
        // Arrange
        var outputPath = Path.GetTempFileName();

        try
        {
            using var stream = File.OpenRead("Intergration/TestCase/Bump3.pnd");
            using var archive = PndArchive.Open(stream);
            var internalPath = archive.ListFiles().First();

            // Act
            archive.ExtractFile(internalPath, outputPath);

            // Assert
            Assert.True(new FileInfo(outputPath).Length > 0);
        }
        finally
        {
            File.Delete(outputPath);
        }
    }

    [Fact]
    public void ExtractFile_WillThrowFileNotFoundExceptionWhenPathDoesNotExistInArchive()
    {
        // Arrange
        using var stream = File.OpenRead("Intergration/TestCase/SORR.pnd");
        using var archive = PndArchive.Open(stream);

        // Act
        // Assert
        Assert.Throws<FileNotFoundException>(() => archive.ExtractFile("/does-not-exist.bin", Path.GetTempFileName()));
    }

    [Fact]
    public void ExtractAll_WillExtractAllFilesFromSquashFsPnd()
    {
        // Arrange
        var outputDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

        try
        {
            Directory.CreateDirectory(outputDirectory);

            using var stream = File.OpenRead("Intergration/TestCase/SORR.pnd");
            using var archive = PndArchive.Open(stream);
            var expected = archive.ListFiles().Count;

            // Act
            archive.ExtractAll(outputDirectory);

            // Assert
            var extracted = Directory.GetFiles(outputDirectory, "*", SearchOption.AllDirectories).Length;
            Assert.Equal(expected, extracted);
        }
        finally
        {
            Directory.Delete(outputDirectory, recursive: true);
        }
    }

    [Fact]
    public void ExtractAll_WillExtractAllFilesFromIsoPnd()
    {
        // Arrange
        var outputDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

        try
        {
            Directory.CreateDirectory(outputDirectory);

            using var stream = File.OpenRead("Intergration/TestCase/Bump3.pnd");
            using var archive = PndArchive.Open(stream);
            var expected = archive.ListFiles().Count;

            // Act
            archive.ExtractAll(outputDirectory);

            // Assert
            var extracted = Directory.GetFiles(outputDirectory, "*", SearchOption.AllDirectories).Length;
            Assert.Equal(expected, extracted);
        }
        finally
        {
            Directory.Delete(outputDirectory, recursive: true);
        }
    }

    [Fact]
    public void ListFiles_WillThrowObjectDisposedExceptionAfterDispose()
    {
        // Arrange
        using var stream = File.OpenRead("Intergration/TestCase/SORR.pnd");
        var archive = PndArchive.Open(stream);
        archive.Dispose();

        // Act
        // Assert
        Assert.Throws<ObjectDisposedException>(() => archive.ListFiles());
    }

    [Fact]
    public void ExtractFile_WillThrowObjectDisposedExceptionAfterDispose()
    {
        // Arrange
        using var stream = File.OpenRead("Intergration/TestCase/SORR.pnd");
        var archive = PndArchive.Open(stream);
        archive.Dispose();

        // Act
        // Assert
        Assert.Throws<ObjectDisposedException>(() => archive.ExtractFile("/PXML.xml", Path.GetTempFileName()));
    }

    [Fact]
    public void ExtractAll_WillThrowObjectDisposedExceptionAfterDispose()
    {
        // Arrange
        using var stream = File.OpenRead("Intergration/TestCase/SORR.pnd");
        var archive = PndArchive.Open(stream);
        archive.Dispose();

        // Act
        // Assert
        Assert.Throws<ObjectDisposedException>(() => archive.ExtractAll(Path.GetTempPath()));
    }
}
