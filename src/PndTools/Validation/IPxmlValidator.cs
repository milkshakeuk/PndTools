// Copyright (c) milkshakeuk. All rights reserved.
// SPDX-License-Identifier: MIT

namespace PndTools.Validation;

/// <summary>
/// Defines the ability to validate a PXML string against the OpenPandora schema and
/// non-schema-enforceable rules.
/// </summary>
public interface IPxmlValidator
{
    /// <summary>Validates a PXML string and returns the result.</summary>
    /// <param name="input">The PXML string to validate.</param>
    /// <returns>A <see cref="ValidationResult"/> describing any validation errors.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="input"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentException"><paramref name="input"/> is empty or whitespace.</exception>
    ValidationResult Validate(string input);
}
