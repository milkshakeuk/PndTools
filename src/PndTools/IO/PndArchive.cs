// Copyright (c) milkshakeuk. All rights reserved.
// SPDX-License-Identifier: MIT

using DiscUtils;
using DiscUtils.Iso9660;
using DiscUtils.SquashFs;
using PndTools.IO.Extensions;
using PndTools.Models;

namespace PndTools.IO;

/// <summary>
/// Provides read access to the filesystem inside a PND archive.
/// Supports both SquashFS and ISO 9660 based PND files.
/// </summary>
/// <remarks>
/// Open an archive with <see cref="Open"/> and dispose it when finished — the
/// underlying <see cref="DiscFileSystem"/> holds a reference to the stream.
/// </remarks>
public sealed class PndArchive : IDisposable
{
    private readonly DiscFileSystem _fileSystem;
    private bool _disposed;

    private PndArchive(DiscFileSystem fileSystem, PndArchiveType archiveType)
    {
        _fileSystem = fileSystem;
        ArchiveType = archiveType;
    }

    /// <summary>The filesystem format of this archive.</summary>
    public PndArchiveType ArchiveType { get; }

    /// <summary>
    /// Opens the PND archive from <paramref name="stream"/>, detecting whether it is
    /// SquashFS or ISO 9660.
    /// </summary>
    /// <param name="stream">The PND file stream to read from.</param>
    /// <returns>A <see cref="PndArchive"/> ready for use.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="stream"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="stream"/> is not positioned at its origin.</exception>
    /// <exception cref="NotSupportedException"><paramref name="stream"/> is not seekable.</exception>
    /// <exception cref="PndArchiveException">The stream is not a recognised SquashFS or ISO 9660 archive.</exception>
    public static PndArchive Open(Stream stream)
    {
        ArgumentNullException.ThrowIfNull(stream);
        ArgumentOutOfRangeException.ThrowIfNotEqual(stream.Position, 0L, nameof(stream));
        var archiveType = stream.DetectArchiveType();
        return new PndArchive(CreateFileSystem(stream, archiveType), archiveType);
    }

    /// <summary>Returns the paths of all files inside the archive.</summary>
    /// <returns>A read-only list of file paths within the archive.</returns>
    /// <exception cref="ObjectDisposedException">The archive has been disposed.</exception>
    public IReadOnlyList<string> ListFiles()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        return _fileSystem.GetFiles("", "*", SearchOption.AllDirectories).ToList();
    }

    /// <summary>
    /// Extracts a single file from the archive to <paramref name="outputPath"/>.
    /// </summary>
    /// <param name="internalPath">The path of the file within the archive.</param>
    /// <param name="outputPath">The destination path on disk.</param>
    /// <exception cref="ArgumentNullException"><paramref name="internalPath"/> or <paramref name="outputPath"/> is <c>null</c>.</exception>
    /// <exception cref="ObjectDisposedException">The archive has been disposed.</exception>
    /// <exception cref="FileNotFoundException"><paramref name="internalPath"/> does not exist in the archive.</exception>
    public void ExtractFile(string internalPath, string outputPath)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(internalPath);
        ArgumentNullException.ThrowIfNull(outputPath);

        if (!_fileSystem.FileExists(internalPath))
        {
            throw new FileNotFoundException($"File not found in archive: {internalPath}", internalPath);
        }

        Directory.CreateDirectory(Path.GetDirectoryName(outputPath) ?? string.Empty);

        using var entry = _fileSystem.OpenFile(internalPath, FileMode.Open, FileAccess.Read);
        using var output = File.Create(outputPath);
        entry.CopyTo(output);
    }

    /// <summary>
    /// Asynchronously extracts a single file from the archive to <paramref name="outputPath"/>.
    /// </summary>
    /// <param name="internalPath">The path of the file within the archive.</param>
    /// <param name="outputPath">The destination path on disk.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="internalPath"/> or <paramref name="outputPath"/> is <c>null</c>.</exception>
    /// <exception cref="ObjectDisposedException">The archive has been disposed.</exception>
    /// <exception cref="FileNotFoundException"><paramref name="internalPath"/> does not exist in the archive.</exception>
    public async Task ExtractFileAsync(string internalPath, string outputPath, CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(internalPath);
        ArgumentNullException.ThrowIfNull(outputPath);

        if (!_fileSystem.FileExists(internalPath))
        {
            throw new FileNotFoundException($"File not found in archive: {internalPath}", internalPath);
        }

        Directory.CreateDirectory(Path.GetDirectoryName(outputPath) ?? string.Empty);

        using var entry = _fileSystem.OpenFile(internalPath, FileMode.Open, FileAccess.Read);
        await using var output = File.Create(outputPath);
        await entry.CopyToAsync(output, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Extracts a subset of files from the archive into <paramref name="outputDirectory"/>,
    /// preserving the internal directory structure.
    /// </summary>
    /// <param name="internalPaths">The paths of the files to extract.</param>
    /// <param name="outputDirectory">The destination directory on disk.</param>
    /// <exception cref="ArgumentNullException"><paramref name="internalPaths"/> or <paramref name="outputDirectory"/> is <c>null</c>.</exception>
    /// <exception cref="ObjectDisposedException">The archive has been disposed.</exception>
    /// <exception cref="FileNotFoundException">One of <paramref name="internalPaths"/> does not exist in the archive.</exception>
    public void ExtractFiles(IEnumerable<string> internalPaths, string outputDirectory)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(internalPaths);
        ArgumentNullException.ThrowIfNull(outputDirectory);

        foreach (var internalPath in internalPaths)
        {
            if (!_fileSystem.FileExists(internalPath))
            {
                throw new FileNotFoundException($"File not found in archive: {internalPath}", internalPath);
            }

            ExtractToDirectory(internalPath, outputDirectory);
        }
    }

    /// <summary>
    /// Asynchronously extracts a subset of files from the archive into <paramref name="outputDirectory"/>,
    /// preserving the internal directory structure.
    /// </summary>
    /// <param name="internalPaths">The paths of the files to extract.</param>
    /// <param name="outputDirectory">The destination directory on disk.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="internalPaths"/> or <paramref name="outputDirectory"/> is <c>null</c>.</exception>
    /// <exception cref="ObjectDisposedException">The archive has been disposed.</exception>
    /// <exception cref="FileNotFoundException">One of <paramref name="internalPaths"/> does not exist in the archive.</exception>
    public async Task ExtractFilesAsync(IEnumerable<string> internalPaths, string outputDirectory, CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(internalPaths);
        ArgumentNullException.ThrowIfNull(outputDirectory);

        foreach (var internalPath in internalPaths)
        {
            if (!_fileSystem.FileExists(internalPath))
            {
                throw new FileNotFoundException($"File not found in archive: {internalPath}", internalPath);
            }

            await ExtractToDirectoryAsync(internalPath, outputDirectory, cancellationToken).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Extracts all files from the archive into <paramref name="outputDirectory"/>,
    /// preserving the internal directory structure.
    /// </summary>
    /// <param name="outputDirectory">The destination directory on disk.</param>
    /// <exception cref="ArgumentNullException"><paramref name="outputDirectory"/> is <c>null</c>.</exception>
    /// <exception cref="ObjectDisposedException">The archive has been disposed.</exception>
    public void ExtractAll(string outputDirectory)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(outputDirectory);

        foreach (var internalPath in _fileSystem.GetFiles("", "*", SearchOption.AllDirectories))
        {
            ExtractToDirectory(internalPath, outputDirectory);
        }
    }

    /// <summary>
    /// Asynchronously extracts all files from the archive into <paramref name="outputDirectory"/>,
    /// preserving the internal directory structure.
    /// </summary>
    /// <param name="outputDirectory">The destination directory on disk.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="outputDirectory"/> is <c>null</c>.</exception>
    /// <exception cref="ObjectDisposedException">The archive has been disposed.</exception>
    public async Task ExtractAllAsync(string outputDirectory, CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(outputDirectory);

        foreach (var internalPath in _fileSystem.GetFiles("", "*", SearchOption.AllDirectories))
        {
            await ExtractToDirectoryAsync(internalPath, outputDirectory, cancellationToken).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Extracts all preview images referenced in <paramref name="pxml"/> into
    /// <paramref name="outputDirectory"/>, skipping any paths that do not exist in the archive.
    /// </summary>
    /// <param name="pxml">The parsed PXML containing preview image paths.</param>
    /// <param name="outputDirectory">The destination directory on disk.</param>
    /// <returns>The output paths of all successfully extracted images.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="pxml"/> or <paramref name="outputDirectory"/> is <c>null</c>.</exception>
    /// <exception cref="ObjectDisposedException">The archive has been disposed.</exception>
    public IReadOnlyList<string> ExtractPreviewPics(Pxml pxml, string outputDirectory)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(pxml);
        ArgumentNullException.ThrowIfNull(outputDirectory);

        Directory.CreateDirectory(outputDirectory);

        var extracted = new List<string>();

        foreach (var internalPath in pxml.Applications
            .SelectMany(a => a.PreviewPics)
            .Select(p => p.Path)
            .Where(p => p is not null)
            .Distinct())
        {
            if (!_fileSystem.FileExists(internalPath!))
            {
                continue;
            }

            var outputPath = Path.Combine(outputDirectory, Path.GetFileName(internalPath!));

            using var entry = _fileSystem.OpenFile(internalPath!, FileMode.Open, FileAccess.Read);
            using var output = File.Create(outputPath);
            entry.CopyTo(output);

            extracted.Add(outputPath);
        }

        return extracted;
    }

    /// <summary>
    /// Asynchronously extracts all preview images referenced in <paramref name="pxml"/> into
    /// <paramref name="outputDirectory"/>, skipping any paths that do not exist in the archive.
    /// </summary>
    /// <param name="pxml">The parsed PXML containing preview image paths.</param>
    /// <param name="outputDirectory">The destination directory on disk.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task containing the output paths of all successfully extracted images.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="pxml"/> or <paramref name="outputDirectory"/> is <c>null</c>.</exception>
    /// <exception cref="ObjectDisposedException">The archive has been disposed.</exception>
    public async Task<IReadOnlyList<string>> ExtractPreviewPicsAsync(Pxml pxml, string outputDirectory, CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(pxml);
        ArgumentNullException.ThrowIfNull(outputDirectory);

        Directory.CreateDirectory(outputDirectory);

        var extracted = new List<string>();

        foreach (var internalPath in pxml.Applications
            .SelectMany(a => a.PreviewPics)
            .Select(p => p.Path)
            .Where(p => p is not null)
            .Distinct())
        {
            if (!_fileSystem.FileExists(internalPath!))
            {
                continue;
            }

            var outputPath = Path.Combine(outputDirectory, Path.GetFileName(internalPath!));

            using var entry = _fileSystem.OpenFile(internalPath!, FileMode.Open, FileAccess.Read);
            await using var output = File.Create(outputPath);
            await entry.CopyToAsync(output, cancellationToken).ConfigureAwait(false);

            extracted.Add(outputPath);
        }

        return extracted;
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _fileSystem.Dispose();
        _disposed = true;
    }

    private void ExtractToDirectory(string internalPath, string outputDirectory)
    {
        var relativePath = internalPath.TrimStart('\\', '/').Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar);
        var outputPath = Path.Combine(outputDirectory, relativePath);

        Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);

        using var entry = _fileSystem.OpenFile(internalPath, FileMode.Open, FileAccess.Read);
        using var output = File.Create(outputPath);
        entry.CopyTo(output);
    }

    private async Task ExtractToDirectoryAsync(string internalPath, string outputDirectory, CancellationToken cancellationToken)
    {
        var relativePath = internalPath.TrimStart('\\', '/').Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar);
        var outputPath = Path.Combine(outputDirectory, relativePath);

        Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);

        using var entry = _fileSystem.OpenFile(internalPath, FileMode.Open, FileAccess.Read);
        await using var output = File.Create(outputPath);
        await entry.CopyToAsync(output, cancellationToken).ConfigureAwait(false);
    }

    private static DiscFileSystem CreateFileSystem(Stream stream, PndArchiveType archiveType) =>
        archiveType switch
        {
            PndArchiveType.SquashFs => new SquashFileSystemReader(stream),
            PndArchiveType.Iso => new CDReader(stream, joliet: true),
            _ => throw new PndArchiveException("Cannot open archive: file type is not SquashFS or ISO 9660.")
        };
}
