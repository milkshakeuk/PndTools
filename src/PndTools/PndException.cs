// Copyright (c) milkshakeuk. All rights reserved.
// SPDX-License-Identifier: MIT

namespace PndTools;

/// <summary>The base class for all PndTools-specific exceptions.</summary>
public abstract class PndException : Exception
{
    /// <inheritdoc/>
    protected PndException() { }

    /// <inheritdoc/>
    protected PndException(string message) : base(message) { }

    /// <inheritdoc/>
    protected PndException(string message, Exception inner) : base(message, inner) { }
}
