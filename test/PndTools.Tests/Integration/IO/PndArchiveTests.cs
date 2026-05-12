// Copyright (c) milkshakeuk. All rights reserved.
// SPDX-License-Identifier: MIT

using System.IO;
using PndTools.IO;
using PndTools.IO.Extensions;
using PndTools.Models;

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
    public void Open_UnknownFileType_ThrowsInvalidPndException()
    {
        // Arrange
        using var stream = new MemoryStream([0x00, 0x01, 0x02, 0x03]);

        // Act
        // Assert
        Assert.Throws<InvalidPndException>(() => PndArchive.Open(stream));
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
}
