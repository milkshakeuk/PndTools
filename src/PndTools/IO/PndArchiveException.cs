// Copyright (c) milkshakeuk. All rights reserved.
// SPDX-License-Identifier: MIT

namespace PndTools.IO;

/// <summary>
/// The exception thrown when a stream does not contain a valid or recognisable PND archive.
/// </summary>
public class PndArchiveException : PndException
{
    /// <inheritdoc/>
    public PndArchiveException() { }

    /// <inheritdoc/>
    public PndArchiveException(string message) : base(message) { }

    /// <inheritdoc/>
    public PndArchiveException(string message, Exception inner) : base(message, inner) { }
}
