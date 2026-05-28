// Copyright (c) milkshakeuk. All rights reserved.
// SPDX-License-Identifier: MIT

using PndTools.IO;
using PndTools.IO.Extensions;

namespace PndTools.Tests.Integration.IO;

public class PndArchiveTests
{
    [Fact]
    public void Open_NullStream_ThrowsArgumentNullException()
    {
        // Arrange
        // Act
        // Assert
        Assert.Throws<ArgumentNullException>(() => PndArchive.Open(null!));
    }

    [Fact]
    public void Open_UnknownFileType_ThrowsPndArchiveException()
    {
        // Arrange
        using var stream = new MemoryStream([0x00, 0x01, 0x02, 0x03]);

        // Act
        // Assert
        Assert.Throws<PndArchiveException>(() => PndArchive.Open(stream));
    }

    [Fact]
    public void Open_StreamNotAtOrigin_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        using var stream = new MemoryStream([0x00, 0x01, 0x02, 0x03]);
        stream.Position = 1;

        // Act
        // Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => PndArchive.Open(stream));
    }

    [Fact]
    public void Open_SquashFsPnd_SetsArchiveTypeToSquashFs()
    {
        // Arrange
        using var stream = File.OpenRead("Integration/TestCase/SORR.pnd");

        // Act
        using var archive = PndArchive.Open(stream);

        // Assert
        Assert.Equal(PndArchiveType.SquashFs, archive.ArchiveType);
    }

    [Fact]
    public void Open_IsoPnd_SetsArchiveTypeToIso()
    {
        // Arrange
        using var stream = File.OpenRead("Integration/TestCase/Bump3.pnd");

        // Act
        using var archive = PndArchive.Open(stream);

        // Assert
        Assert.Equal(PndArchiveType.Iso, archive.ArchiveType);
    }

    [Fact]
    public void ListFiles_SquashFsPnd_ReturnsFiles()
    {
        // Arrange
        using var stream = File.OpenRead("Integration/TestCase/SORR.pnd");
        using var archive = PndArchive.Open(stream);

        // Act
        var result = archive.ListFiles();

        // Assert
        Assert.NotEmpty(result);
    }

    [Fact]
    public void ListFiles_IsoPnd_ReturnsFiles()
    {
        // Arrange
        using var stream = File.OpenRead("Integration/TestCase/Bump3.pnd");
        using var archive = PndArchive.Open(stream);

        // Act
        var result = archive.ListFiles();

        // Assert
        Assert.NotEmpty(result);
    }

    [Fact]
    public void ExtractFile_SquashFsPnd_ExtractsFile()
    {
        // Arrange
        var outputPath = Path.GetTempFileName();

        try
        {
            using var stream = File.OpenRead("Integration/TestCase/SORR.pnd");
            using var archive = PndArchive.Open(stream);
            var internalPath = archive.ListFiles()[0];

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
    public void ExtractFile_IsoPnd_ExtractsFile()
    {
        // Arrange
        var outputPath = Path.GetTempFileName();

        try
        {
            using var stream = File.OpenRead("Integration/TestCase/Bump3.pnd");
            using var archive = PndArchive.Open(stream);
            var internalPath = archive.ListFiles()[0];

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
    public void ExtractFile_PathNotInArchive_ThrowsFileNotFoundException()
    {
        // Arrange
        using var stream = File.OpenRead("Integration/TestCase/SORR.pnd");
        using var archive = PndArchive.Open(stream);

        // Act
        // Assert
        Assert.Throws<FileNotFoundException>(() => archive.ExtractFile("/does-not-exist.bin", Path.GetTempFileName()));
    }

    [Fact]
    public void ExtractAll_SquashFsPnd_ExtractsAllFiles()
    {
        // Arrange
        var outputDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

        try
        {
            Directory.CreateDirectory(outputDirectory);

            using var stream = File.OpenRead("Integration/TestCase/SORR.pnd");
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
    public void ExtractAll_IsoPnd_ExtractsAllFiles()
    {
        // Arrange
        var outputDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

        try
        {
            Directory.CreateDirectory(outputDirectory);

            using var stream = File.OpenRead("Integration/TestCase/Bump3.pnd");
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
    public void ExtractFiles_SquashFsPnd_ExtractsRequestedFiles()
    {
        // Arrange
        var outputDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

        try
        {
            Directory.CreateDirectory(outputDirectory);

            using var stream = File.OpenRead("Integration/TestCase/SORR.pnd");
            using var archive = PndArchive.Open(stream);
            var paths = archive.ListFiles().Take(2).ToList();

            // Act
            archive.ExtractFiles(paths, outputDirectory);

            // Assert
            var extracted = Directory.GetFiles(outputDirectory, "*", SearchOption.AllDirectories).Length;
            Assert.Equal(2, extracted);
        }
        finally
        {
            Directory.Delete(outputDirectory, recursive: true);
        }
    }

    [Fact]
    public void ExtractFiles_IsoPnd_ExtractsRequestedFiles()
    {
        // Arrange
        var outputDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

        try
        {
            Directory.CreateDirectory(outputDirectory);

            using var stream = File.OpenRead("Integration/TestCase/Bump3.pnd");
            using var archive = PndArchive.Open(stream);
            var paths = archive.ListFiles().Take(2).ToList();

            // Act
            archive.ExtractFiles(paths, outputDirectory);

            // Assert
            var extracted = Directory.GetFiles(outputDirectory, "*", SearchOption.AllDirectories).Length;
            Assert.Equal(2, extracted);
        }
        finally
        {
            Directory.Delete(outputDirectory, recursive: true);
        }
    }

    [Fact]
    public void ExtractFiles_PathNotInArchive_ThrowsFileNotFoundException()
    {
        // Arrange
        using var stream = File.OpenRead("Integration/TestCase/SORR.pnd");
        using var archive = PndArchive.Open(stream);

        // Act
        // Assert
        Assert.Throws<FileNotFoundException>(() => archive.ExtractFiles(["/does-not-exist.bin"], Path.GetTempPath()));
    }

    [Fact]
    public void ExtractFiles_DisposedArchive_ThrowsObjectDisposedException()
    {
        // Arrange
        using var stream = File.OpenRead("Integration/TestCase/SORR.pnd");
        var archive = PndArchive.Open(stream);
        archive.Dispose();

        // Act
        // Assert
        Assert.Throws<ObjectDisposedException>(() => archive.ExtractFiles(["/PXML.xml"], Path.GetTempPath()));
    }

    [Fact]
    public void ListFiles_DisposedArchive_ThrowsObjectDisposedException()
    {
        // Arrange
        using var stream = File.OpenRead("Integration/TestCase/SORR.pnd");
        var archive = PndArchive.Open(stream);
        archive.Dispose();

        // Act
        // Assert
        Assert.Throws<ObjectDisposedException>(() => archive.ListFiles());
    }

    [Fact]
    public void ExtractFile_DisposedArchive_ThrowsObjectDisposedException()
    {
        // Arrange
        using var stream = File.OpenRead("Integration/TestCase/SORR.pnd");
        var archive = PndArchive.Open(stream);
        archive.Dispose();

        // Act
        // Assert
        Assert.Throws<ObjectDisposedException>(() => archive.ExtractFile("/PXML.xml", Path.GetTempFileName()));
    }

    [Fact]
    public void ExtractAll_DisposedArchive_ThrowsObjectDisposedException()
    {
        // Arrange
        using var stream = File.OpenRead("Integration/TestCase/SORR.pnd");
        var archive = PndArchive.Open(stream);
        archive.Dispose();

        // Act
        // Assert
        Assert.Throws<ObjectDisposedException>(() => archive.ExtractAll(Path.GetTempPath()));
    }

    [Fact]
    public async Task ExtractFileAsync_SquashFsPnd_ExtractsFile()
    {
        // Arrange
        var outputPath = Path.GetTempFileName();

        try
        {
            using var stream = File.OpenRead("Integration/TestCase/SORR.pnd");
            using var archive = PndArchive.Open(stream);
            var internalPath = archive.ListFiles()[0];

            // Act
            await archive.ExtractFileAsync(internalPath, outputPath, TestContext.Current.CancellationToken);

            // Assert
            Assert.True(new FileInfo(outputPath).Length > 0);
        }
        finally
        {
            File.Delete(outputPath);
        }
    }

    [Fact]
    public async Task ExtractFileAsync_IsoPnd_ExtractsFile()
    {
        // Arrange
        var outputPath = Path.GetTempFileName();

        try
        {
            using var stream = File.OpenRead("Integration/TestCase/Bump3.pnd");
            using var archive = PndArchive.Open(stream);
            var internalPath = archive.ListFiles()[0];

            // Act
            await archive.ExtractFileAsync(internalPath, outputPath, TestContext.Current.CancellationToken);

            // Assert
            Assert.True(new FileInfo(outputPath).Length > 0);
        }
        finally
        {
            File.Delete(outputPath);
        }
    }

    [Fact]
    public async Task ExtractFileAsync_PathNotInArchive_ThrowsFileNotFoundException()
    {
        // Arrange
        using var stream = File.OpenRead("Integration/TestCase/SORR.pnd");
        using var archive = PndArchive.Open(stream);

        // Act
        // Assert
        await Assert.ThrowsAsync<FileNotFoundException>(() =>
            archive.ExtractFileAsync("/does-not-exist.bin", Path.GetTempFileName(), TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task ExtractFileAsync_DisposedArchive_ThrowsObjectDisposedException()
    {
        // Arrange
        using var stream = File.OpenRead("Integration/TestCase/SORR.pnd");
        var archive = PndArchive.Open(stream);
        archive.Dispose();

        // Act
        // Assert
        await Assert.ThrowsAsync<ObjectDisposedException>(() =>
            archive.ExtractFileAsync("/PXML.xml", Path.GetTempFileName(), TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task ExtractFilesAsync_SquashFsPnd_ExtractsRequestedFiles()
    {
        // Arrange
        var outputDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

        try
        {
            Directory.CreateDirectory(outputDirectory);

            using var stream = File.OpenRead("Integration/TestCase/SORR.pnd");
            using var archive = PndArchive.Open(stream);
            var paths = archive.ListFiles().Take(2).ToList();

            // Act
            await archive.ExtractFilesAsync(paths, outputDirectory, TestContext.Current.CancellationToken);

            // Assert
            var extracted = Directory.GetFiles(outputDirectory, "*", SearchOption.AllDirectories).Length;
            Assert.Equal(2, extracted);
        }
        finally
        {
            Directory.Delete(outputDirectory, recursive: true);
        }
    }

    [Fact]
    public async Task ExtractFilesAsync_PathNotInArchive_ThrowsFileNotFoundException()
    {
        // Arrange
        using var stream = File.OpenRead("Integration/TestCase/SORR.pnd");
        using var archive = PndArchive.Open(stream);

        // Act
        // Assert
        await Assert.ThrowsAsync<FileNotFoundException>(() =>
            archive.ExtractFilesAsync(["/does-not-exist.bin"], Path.GetTempPath(), TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task ExtractAllAsync_SquashFsPnd_ExtractsAllFiles()
    {
        // Arrange
        var outputDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

        try
        {
            Directory.CreateDirectory(outputDirectory);

            using var stream = File.OpenRead("Integration/TestCase/SORR.pnd");
            using var archive = PndArchive.Open(stream);
            var expected = archive.ListFiles().Count;

            // Act
            await archive.ExtractAllAsync(outputDirectory, TestContext.Current.CancellationToken);

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
    public async Task ExtractAllAsync_IsoPnd_ExtractsAllFiles()
    {
        // Arrange
        var outputDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

        try
        {
            Directory.CreateDirectory(outputDirectory);

            using var stream = File.OpenRead("Integration/TestCase/Bump3.pnd");
            using var archive = PndArchive.Open(stream);
            var expected = archive.ListFiles().Count;

            // Act
            await archive.ExtractAllAsync(outputDirectory, TestContext.Current.CancellationToken);

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
    public async Task ExtractAllAsync_DisposedArchive_ThrowsObjectDisposedException()
    {
        // Arrange
        using var stream = File.OpenRead("Integration/TestCase/SORR.pnd");
        var archive = PndArchive.Open(stream);
        archive.Dispose();

        // Act
        // Assert
        await Assert.ThrowsAsync<ObjectDisposedException>(() =>
            archive.ExtractAllAsync(Path.GetTempPath(), TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task ExtractPreviewPicsAsync_SquashFsPnd_ExtractsPreviewPics()
    {
        // Arrange
        var outputDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

        try
        {
            using var stream = File.OpenRead("Integration/TestCase/SORR.pnd");
            using var archive = PndArchive.Open(stream);
            var pxmlString = await stream.GetPxmlAsync(TestContext.Current.CancellationToken);
            var parser = new PxmlParser();
            var pxml = parser.Parse(pxmlString);

            // Act
            var result = await archive.ExtractPreviewPicsAsync(pxml, outputDirectory, TestContext.Current.CancellationToken);

            // Assert
            Assert.NotEmpty(result);
            Assert.All(result, path => Assert.True(File.Exists(path)));
        }
        finally
        {
            if (Directory.Exists(outputDirectory))
            {
                Directory.Delete(outputDirectory, recursive: true);
            }
        }
    }

    [Fact]
    public async Task ExtractPreviewPicsAsync_DisposedArchive_ThrowsObjectDisposedException()
    {
        // Arrange
        using var stream = File.OpenRead("Integration/TestCase/SORR.pnd");
        var archive = PndArchive.Open(stream);
        var pxmlString = await stream.GetPxmlAsync(TestContext.Current.CancellationToken);
        var parser = new PxmlParser();
        var pxml = parser.Parse(pxmlString);
        archive.Dispose();

        // Act
        // Assert
        await Assert.ThrowsAsync<ObjectDisposedException>(() =>
            archive.ExtractPreviewPicsAsync(pxml, Path.GetTempPath(), TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task ExtractFileAsync_NullInternalPath_ThrowsArgumentNullException()
    {
        // Arrange
        using var stream = File.OpenRead("Integration/TestCase/SORR.pnd");
        using var archive = PndArchive.Open(stream);

        // Act
        // Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            archive.ExtractFileAsync(null!, Path.GetTempFileName(), TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task ExtractFileAsync_NullOutputPath_ThrowsArgumentNullException()
    {
        // Arrange
        using var stream = File.OpenRead("Integration/TestCase/SORR.pnd");
        using var archive = PndArchive.Open(stream);

        // Act
        // Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            archive.ExtractFileAsync("/PXML.xml", null!, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task ExtractFilesAsync_NullInternalPaths_ThrowsArgumentNullException()
    {
        // Arrange
        using var stream = File.OpenRead("Integration/TestCase/SORR.pnd");
        using var archive = PndArchive.Open(stream);

        // Act
        // Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            archive.ExtractFilesAsync(null!, Path.GetTempPath(), TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task ExtractFilesAsync_NullOutputDirectory_ThrowsArgumentNullException()
    {
        // Arrange
        using var stream = File.OpenRead("Integration/TestCase/SORR.pnd");
        using var archive = PndArchive.Open(stream);

        // Act
        // Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            archive.ExtractFilesAsync(["/PXML.xml"], null!, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task ExtractFilesAsync_DisposedArchive_ThrowsObjectDisposedException()
    {
        // Arrange
        using var stream = File.OpenRead("Integration/TestCase/SORR.pnd");
        var archive = PndArchive.Open(stream);
        archive.Dispose();

        // Act
        // Assert
        await Assert.ThrowsAsync<ObjectDisposedException>(() =>
            archive.ExtractFilesAsync(["/PXML.xml"], Path.GetTempPath(), TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task ExtractAllAsync_NullOutputDirectory_ThrowsArgumentNullException()
    {
        // Arrange
        using var stream = File.OpenRead("Integration/TestCase/SORR.pnd");
        using var archive = PndArchive.Open(stream);

        // Act
        // Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            archive.ExtractAllAsync(null!, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task ExtractPreviewPicsAsync_NullPxml_ThrowsArgumentNullException()
    {
        // Arrange
        using var stream = File.OpenRead("Integration/TestCase/SORR.pnd");
        using var archive = PndArchive.Open(stream);

        // Act
        // Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            archive.ExtractPreviewPicsAsync(null!, Path.GetTempPath(), TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task ExtractPreviewPicsAsync_NullOutputDirectory_ThrowsArgumentNullException()
    {
        // Arrange
        using var stream = File.OpenRead("Integration/TestCase/SORR.pnd");
        using var archive = PndArchive.Open(stream);
        var pxmlString = await stream.GetPxmlAsync(TestContext.Current.CancellationToken);
        var parser = new PxmlParser();
        var pxml = parser.Parse(pxmlString);

        // Act
        // Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            archive.ExtractPreviewPicsAsync(pxml, null!, TestContext.Current.CancellationToken));
    }
}
