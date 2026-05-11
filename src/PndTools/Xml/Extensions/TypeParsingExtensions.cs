// Copyright (c) milkshakeuk. All rights reserved.
// SPDX-License-Identifier: MIT

namespace PndTools.Xml.Extensions;

/// <summary>Utility methods for parsing string values into common .NET types.</summary>
public static class TypeParsingExtensions
{
    /// <summary>
    /// Parses <paramref name="value"/> into <typeparamref name="T"/>.
    /// Returns <c>default</c> for nullable or reference types when <paramref name="value"/> is
    /// <c>null</c> or empty. Throws for non-nullable value types with no value.
    /// </summary>
    /// <typeparam name="T">
    /// The target type. Supported types are <see cref="string"/>, <see cref="bool"/>,
    /// <see cref="int"/>, <see cref="double"/>, <see cref="decimal"/>, <see cref="DateTime"/>,
    /// and <see cref="Guid"/>, plus their nullable equivalents.
    /// </typeparam>
    /// <param name="value">The string value to parse.</param>
    /// <returns>The parsed value, or <c>default</c> when the value is absent and <typeparamref name="T"/> is nullable.</returns>
    /// <exception cref="ArgumentException"><paramref name="value"/> is null or empty and <typeparamref name="T"/> is a non-nullable value type.</exception>
    /// <exception cref="InvalidOperationException"><typeparamref name="T"/> is not a supported type.</exception>
    public static T? Parse<T>(string? value)
    {
        var type = typeof(T);

        if (type == typeof(string))
        {
            return (T?)(object?)value;
        }

        if (string.IsNullOrEmpty(value))
        {
            if (!type.IsValueType || Nullable.GetUnderlyingType(type) is not null)
            {
                return default;
            }

            throw new ArgumentException($"Cannot parse null or empty string as {type.Name}.", nameof(value));
        }

        if (type == typeof(bool) || type == typeof(bool?)) { return (T)(object)bool.Parse(value); }
        if (type == typeof(int) || type == typeof(int?)) { return (T)(object)int.Parse(value, System.Globalization.CultureInfo.InvariantCulture); }
        if (type == typeof(double) || type == typeof(double?)) { return (T)(object)double.Parse(value, System.Globalization.CultureInfo.InvariantCulture); }
        if (type == typeof(decimal) || type == typeof(decimal?)) { return (T)(object)decimal.Parse(value, System.Globalization.CultureInfo.InvariantCulture); }
        if (type == typeof(DateTime) || type == typeof(DateTime?)) { return (T)(object)DateTime.Parse(value, System.Globalization.CultureInfo.InvariantCulture); }
        if (type == typeof(Guid) || type == typeof(Guid?)) { return (T)(object)Guid.Parse(value); }

        throw new InvalidOperationException($"Unable to parse type {type}");
    }
}
