// Copyright (c) milkshakeuk. All rights reserved.
// SPDX-License-Identifier: MIT

using PndTools.IO;

namespace PndTools.AspNetCore;

/// <summary>Default implementation of <see cref="IPndArchiveFactory"/>.</summary>
public sealed class PndArchiveFactory : IPndArchiveFactory
{
    /// <inheritdoc/>
    public PndArchive Open(Stream stream) => PndArchive.Open(stream);

    /// <inheritdoc/>
    public bool TryOpen(Stream? stream, out PndArchive? archive)
    {
        if (stream is null)
        {
            archive = null;
            return false;
        }

        try
        {
            archive = PndArchive.Open(stream);
            return true;
        }
        catch (Exception ex) when (ex is PndArchiveException or ArgumentOutOfRangeException or NotSupportedException)
        {
            archive = null;
            return false;
        }
    }
}
