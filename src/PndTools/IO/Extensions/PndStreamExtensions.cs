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

    private static Position FindPxml(this Stream stream)
    {
        var start = stream.Find(PxmlStart, Direction.Backwards);
        var end = stream.Find(PxmlEnd, Direction.Backwards);

        if (start == -1 || end == -1)
        {
            throw new InvalidPndException("Pxml is missing or incomplete");
        }

        end += PxmlEnd.Length;

        return new Position(start, end);
    }

    /// <summary>
    /// Retrieves the PXML as a string from the <paramref name="stream"/>, prepending an XML
    /// declaration if one is not already present.
    /// </summary>
    /// <param name="stream">The PND file stream to read from.</param>
    /// <returns>The PXML as a UTF-8 string with an XML declaration prepended.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="stream"/> is <c>null</c>.</exception>
    /// <exception cref="InvalidPndException"><paramref name="stream"/> does not contain valid PXML.</exception>
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
    /// <exception cref="InvalidPndException"><paramref name="stream"/> does not contain an embedded icon.</exception>
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
    /// <exception cref="InvalidPndException"><paramref name="stream"/> does not contain valid PXML.</exception>
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
    /// <exception cref="InvalidPndException"><paramref name="stream"/> does not contain an embedded icon.</exception>
    public static void SaveIcon(this Stream stream, string path)
    {
        ArgumentNullException.ThrowIfNull(stream);
        ArgumentNullException.ThrowIfNull(path);
        File.WriteAllBytes(path, stream.GetIcon());
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

    private static Position FindIcon(this Stream stream)
    {
        var start = stream.Find(PngMagicNumber, Direction.Backwards);

        if (start == -1)
        {
            throw new InvalidPndException("Icon is missing or incomplete");
        }

        return new Position(start, stream.Length);
    }
}

internal readonly record struct Position(long Start, long End);

/// <summary>
/// The exception thrown when a PND file stream is missing required data, such as PXML or an
/// embedded icon.
/// </summary>
public class InvalidPndException : Exception
{
    /// <inheritdoc/>
    public InvalidPndException() { }
    /// <inheritdoc/>
    public InvalidPndException(string message) : base(message) { }
    /// <inheritdoc/>
    public InvalidPndException(string message, Exception inner) : base(message, inner) { }
}

/// <summary>The filesystem type embedded in a PND file.</summary>
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
