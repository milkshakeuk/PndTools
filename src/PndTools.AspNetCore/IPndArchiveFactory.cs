// Copyright (c) milkshakeuk. All rights reserved.
// SPDX-License-Identifier: MIT

using PndTools.IO;

namespace PndTools.AspNetCore;

/// <summary>
/// Defines a factory for obtaining a <see cref="PndArchive"/> from a caller-owned stream.
/// </summary>
/// <remarks>
/// The caller retains ownership of both the stream and the returned archive — the factory
/// does not dispose either. Both methods require a seekable stream positioned at its origin
/// (<c>Position == 0</c>); <see cref="PndArchive.Open"/> reads and sets <c>Stream.Position</c>
/// during archive-type detection and will throw <see cref="NotSupportedException"/> on non-seekable streams.
/// </remarks>
public interface IPndArchiveFactory
{
    /// <summary>
    /// Opens a <see cref="PndArchive"/> from <paramref name="stream"/>.
    /// Throws on invalid or unrecognised input.
    /// </summary>
    /// <param name="stream">The stream to read from, positioned at its origin.</param>
    /// <returns>A <see cref="PndArchive"/> ready for use.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="stream"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="stream"/> is not positioned at its origin.</exception>
    /// <exception cref="NotSupportedException"><paramref name="stream"/> is not seekable.</exception>
    /// <exception cref="PndArchiveException">The stream is not a recognised PND archive.</exception>
    PndArchive Open(Stream stream);

    /// <summary>
    /// Attempts to open a <see cref="PndArchive"/> from <paramref name="stream"/>.
    /// Returns <c>false</c> without throwing if the stream is <c>null</c>, not seekable,
    /// not at its origin, or not a recognised PND archive.
    /// </summary>
    /// <param name="stream">The stream to read from, positioned at its origin.</param>
    /// <param name="archive">
    /// When this method returns <c>true</c>, contains the opened <see cref="PndArchive"/>;
    /// otherwise <c>null</c>.
    /// </param>
    /// <returns>
    /// <c>true</c> if the archive was opened successfully; <c>false</c> otherwise.
    /// </returns>
    bool TryOpen(Stream? stream, out PndArchive? archive);
}
