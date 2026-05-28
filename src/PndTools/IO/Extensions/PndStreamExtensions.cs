// Copyright (c) milkshakeuk. All rights reserved.
// SPDX-License-Identifier: MIT

using System.Text;

namespace PndTools.IO.Extensions;

/// <summary>Extension methods for reading PND-specific data from a <see cref="Stream"/>.</summary>
public static class PndStreamExtensions
{
    private static ReadOnlySpan<byte> PxmlStart => "<PXML"u8;
    private static ReadOnlySpan<byte> PxmlEnd => "</PXML>"u8;
    private const long IsoVolumeDescriptorOffset = 0x8001;
    private static ReadOnlySpan<byte> PngMagicNumber => [0x89, 0x50, 0x4e, 0x47, 0x0d, 0x0a, 0x1a, 0x0a];
    private static ReadOnlySpan<byte> SquashFsMagicLE => [0x68, 0x73, 0x71, 0x73]; // "hsqs"
    private static ReadOnlySpan<byte> SquashFsMagicBE => [0x73, 0x71, 0x73, 0x68]; // "sqsh"
    private static ReadOnlySpan<byte> IsoMagic => [0x43, 0x44, 0x30, 0x30, 0x31];  // "CD001"

    private static ReadOnlyMemory<byte> PxmlStartMemory => "<PXML"u8.ToArray();
    private static ReadOnlyMemory<byte> PxmlEndMemory => "</PXML>"u8.ToArray();
    private static ReadOnlyMemory<byte> PngMagicNumberMemory => new byte[] { 0x89, 0x50, 0x4e, 0x47, 0x0d, 0x0a, 0x1a, 0x0a };

    private static Position FindPxml(this Stream stream)
    {
        var start = stream.Find(PxmlStart, Direction.Backwards);
        var end = stream.Find(PxmlEnd, Direction.Backwards);

        if (start == -1 || end == -1)
        {
            throw new PndArchiveException("Pxml is missing or incomplete");
        }

        end += PxmlEnd.Length;

        return new Position(start, end);
    }

    private static async Task<Position> FindPxmlAsync(this Stream stream, CancellationToken cancellationToken)
    {
        var start = await stream.FindAsync(PxmlStartMemory, Direction.Backwards, cancellationToken).ConfigureAwait(false);
        var end = await stream.FindAsync(PxmlEndMemory, Direction.Backwards, cancellationToken).ConfigureAwait(false);

        if (start == -1 || end == -1)
        {
            throw new PndArchiveException("Pxml is missing or incomplete");
        }

        end += PxmlEndMemory.Length;

        return new Position(start, end);
    }

    private static async Task<Position> FindIconAsync(this Stream stream, CancellationToken cancellationToken)
    {
        var start = await stream.FindAsync(PngMagicNumberMemory, Direction.Backwards, cancellationToken).ConfigureAwait(false);

        if (start == -1)
        {
            throw new PndArchiveException("Icon is missing or incomplete");
        }

        return new Position(start, stream.Length);
    }

    /// <summary>
    /// Retrieves the PXML as a string from the <paramref name="stream"/>, prepending an XML
    /// declaration if one is not already present.
    /// </summary>
    /// <param name="stream">The PND file stream to read from.</param>
    /// <returns>The PXML as a UTF-8 string with an XML declaration prepended.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="stream"/> is <c>null</c>.</exception>
    /// <exception cref="PndArchiveException"><paramref name="stream"/> does not contain valid PXML.</exception>
    public static string GetPxml(this Stream stream)
    {
        ArgumentNullException.ThrowIfNull(stream);

        var position = stream.FindPxml();
        var data = stream.GetBytes(position.Start, position.End);

        var xmlDeclaration = Encoding.UTF8.GetBytes($"<?xml version=\"1.0\" encoding=\"UTF-8\"?>{Environment.NewLine}");
        var buffer = new byte[xmlDeclaration.Length + data.Length];

        Buffer.BlockCopy(xmlDeclaration, 0, buffer, 0, xmlDeclaration.Length);
        Buffer.BlockCopy(data, 0, buffer, xmlDeclaration.Length, data.Length);

        return Encoding.UTF8.GetString(buffer);
    }

    /// <summary>
    /// Retrieves the embedded icon as a byte array from the <paramref name="stream"/>.
    /// The icon is a PNG image appended after the SquashFS or ISO image.
    /// </summary>
    /// <param name="stream">The PND file stream to read from.</param>
    /// <returns>The raw PNG bytes of the icon.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="stream"/> is <c>null</c>.</exception>
    /// <exception cref="PndArchiveException"><paramref name="stream"/> does not contain an embedded icon.</exception>
    public static byte[] GetIcon(this Stream stream)
    {
        ArgumentNullException.ThrowIfNull(stream);

        var position = stream.FindIcon();
        return stream.GetBytes(position.Start, position.End);
    }

    /// <summary>
    /// Saves the PXML from the <paramref name="stream"/> to the file at <paramref name="path"/>.
    /// </summary>
    /// <param name="stream">The PND file stream to read from.</param>
    /// <param name="path">The destination file path.</param>
    /// <exception cref="ArgumentNullException"><paramref name="stream"/> or <paramref name="path"/> is <c>null</c>.</exception>
    /// <exception cref="PndArchiveException"><paramref name="stream"/> does not contain valid PXML.</exception>
    public static void SavePxml(this Stream stream, string path)
    {
        ArgumentNullException.ThrowIfNull(stream);
        ArgumentNullException.ThrowIfNull(path);
        File.WriteAllText(path, stream.GetPxml(), Encoding.UTF8);
    }

    /// <summary>
    /// Saves the embedded icon from the <paramref name="stream"/> to the file at <paramref name="path"/>.
    /// </summary>
    /// <param name="stream">The PND file stream to read from.</param>
    /// <param name="path">The destination file path.</param>
    /// <exception cref="ArgumentNullException"><paramref name="stream"/> or <paramref name="path"/> is <c>null</c>.</exception>
    /// <exception cref="PndArchiveException"><paramref name="stream"/> does not contain an embedded icon.</exception>
    public static void SaveIcon(this Stream stream, string path)
    {
        ArgumentNullException.ThrowIfNull(stream);
        ArgumentNullException.ThrowIfNull(path);
        File.WriteAllBytes(path, stream.GetIcon());
    }

    /// <summary>
    /// Asynchronously retrieves the PXML as a string from the <paramref name="stream"/>,
    /// prepending an XML declaration if one is not already present.
    /// </summary>
    /// <param name="stream">The PND file stream to read from.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The PXML as a UTF-8 string with an XML declaration prepended.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="stream"/> is <c>null</c>.</exception>
    /// <exception cref="PndArchiveException"><paramref name="stream"/> does not contain valid PXML.</exception>
    public static async Task<string> GetPxmlAsync(this Stream stream, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(stream);

        var position = await stream.FindPxmlAsync(cancellationToken).ConfigureAwait(false);
        var data = await stream.GetBytesAsync(position.Start, position.End, cancellationToken).ConfigureAwait(false);

        var xmlDeclaration = Encoding.UTF8.GetBytes($"<?xml version=\"1.0\" encoding=\"UTF-8\"?>{Environment.NewLine}");
        var buffer = new byte[xmlDeclaration.Length + data.Length];

        Buffer.BlockCopy(xmlDeclaration, 0, buffer, 0, xmlDeclaration.Length);
        Buffer.BlockCopy(data, 0, buffer, xmlDeclaration.Length, data.Length);

        return Encoding.UTF8.GetString(buffer);
    }

    /// <summary>
    /// Asynchronously retrieves the embedded icon as a byte array from the <paramref name="stream"/>.
    /// The icon is a PNG image appended after the SquashFS or ISO image.
    /// </summary>
    /// <param name="stream">The PND file stream to read from.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The raw PNG bytes of the icon.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="stream"/> is <c>null</c>.</exception>
    /// <exception cref="PndArchiveException"><paramref name="stream"/> does not contain an embedded icon.</exception>
    public static async Task<byte[]> GetIconAsync(this Stream stream, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(stream);

        var position = await stream.FindIconAsync(cancellationToken).ConfigureAwait(false);
        return await stream.GetBytesAsync(position.Start, position.End, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Asynchronously saves the PXML from the <paramref name="stream"/> to the file at <paramref name="path"/>.
    /// </summary>
    /// <param name="stream">The PND file stream to read from.</param>
    /// <param name="path">The destination file path.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="stream"/> or <paramref name="path"/> is <c>null</c>.</exception>
    /// <exception cref="PndArchiveException"><paramref name="stream"/> does not contain valid PXML.</exception>
    public static async Task SavePxmlAsync(this Stream stream, string path, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(stream);
        ArgumentNullException.ThrowIfNull(path);
        await File.WriteAllTextAsync(path, await stream.GetPxmlAsync(cancellationToken).ConfigureAwait(false), Encoding.UTF8, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Asynchronously saves the embedded icon from the <paramref name="stream"/> to the file at <paramref name="path"/>.
    /// </summary>
    /// <param name="stream">The PND file stream to read from.</param>
    /// <param name="path">The destination file path.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="stream"/> or <paramref name="path"/> is <c>null</c>.</exception>
    /// <exception cref="PndArchiveException"><paramref name="stream"/> does not contain an embedded icon.</exception>
    public static async Task SaveIconAsync(this Stream stream, string path, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(stream);
        ArgumentNullException.ThrowIfNull(path);
        await File.WriteAllBytesAsync(path, await stream.GetIconAsync(cancellationToken).ConfigureAwait(false), cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Detects whether the PND file is SquashFS-based or ISO 9660-based by reading magic bytes.
    /// Returns <see cref="PndArchiveType.Unknown"/> if the stream is too short or does not match
    /// either format.
    /// </summary>
    /// <param name="stream">The PND file stream to inspect.</param>
    /// <returns>The detected <see cref="PndArchiveType"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="stream"/> is <c>null</c>.</exception>
    public static PndArchiveType DetectArchiveType(this Stream stream)
    {
        ArgumentNullException.ThrowIfNull(stream);

        var originalPosition = stream.Position;
        try
        {
            stream.Position = 0;
            Span<byte> magic = stackalloc byte[4];

            if (stream.Read(magic) < 4)
            {
                return PndArchiveType.Unknown;
            }

            if (magic.SequenceEqual(SquashFsMagicLE) || magic.SequenceEqual(SquashFsMagicBE))
            {
                return PndArchiveType.SquashFs;
            }

            // ISO 9660: "CD001" identifier at offset 0x8001 (volume descriptor sector 16)
            if (stream.Length > IsoVolumeDescriptorOffset + IsoMagic.Length)
            {
                stream.Position = IsoVolumeDescriptorOffset;
                Span<byte> iso = stackalloc byte[5];

                if (stream.Read(iso) == IsoMagic.Length && iso.SequenceEqual(IsoMagic))
                {
                    return PndArchiveType.Iso;
                }
            }

            return PndArchiveType.Unknown;
        }
        finally
        {
            stream.Position = originalPosition;
        }
    }

    /// <summary>
    /// Asynchronously detects whether the PND file is SquashFS-based or ISO 9660-based by reading
    /// magic bytes. Returns <see cref="PndArchiveType.Unknown"/> if the stream is too short or does
    /// not match either format.
    /// </summary>
    /// <param name="stream">The PND file stream to inspect.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The detected <see cref="PndArchiveType"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="stream"/> is <c>null</c>.</exception>
    public static async Task<PndArchiveType> DetectArchiveTypeAsync(this Stream stream, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(stream);

        var originalPosition = stream.Position;
        try
        {
            stream.Position = 0;
            var magic = new byte[4];

            if (await stream.ReadAsync(magic, cancellationToken).ConfigureAwait(false) < 4)
            {
                return PndArchiveType.Unknown;
            }

            if (magic.AsSpan().SequenceEqual(SquashFsMagicLE) || magic.AsSpan().SequenceEqual(SquashFsMagicBE))
            {
                return PndArchiveType.SquashFs;
            }

            // ISO 9660: "CD001" identifier at offset 0x8001 (volume descriptor sector 16)
            if (stream.Length > IsoVolumeDescriptorOffset + IsoMagic.Length)
            {
                stream.Position = IsoVolumeDescriptorOffset;
                var iso = new byte[5];

                if (await stream.ReadAsync(iso, cancellationToken).ConfigureAwait(false) == IsoMagic.Length && iso.AsSpan().SequenceEqual(IsoMagic))
                {
                    return PndArchiveType.Iso;
                }
            }

            return PndArchiveType.Unknown;
        }
        finally
        {
            stream.Position = originalPosition;
        }
    }

    private static Position FindIcon(this Stream stream)
    {
        var start = stream.Find(PngMagicNumber, Direction.Backwards);

        if (start == -1)
        {
            throw new PndArchiveException("Icon is missing or incomplete");
        }

        return new Position(start, stream.Length);
    }
}

internal readonly record struct Position(long Start, long End);

/// <summary>The filesystem format of the archive embedded in a PND file.</summary>
public enum PndArchiveType
{
    /// <summary>The file type could not be determined from its magic bytes.</summary>
    Unknown,
    /// <summary>The PND uses a SquashFS filesystem image.</summary>
    SquashFs,
    /// <summary>The PND uses an ISO 9660 filesystem image.</summary>
    Iso
}
