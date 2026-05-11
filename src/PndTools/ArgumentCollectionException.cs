using System.Runtime.CompilerServices;

namespace PndTools;

/// <summary>The exception thrown when a collection argument is null or empty.</summary>
public class ArgumentCollectionException : ArgumentException
{
    /// <inheritdoc/>
    public ArgumentCollectionException() { }
    /// <inheritdoc/>
    public ArgumentCollectionException(string message) : base(message) { }
    /// <inheritdoc/>
    public ArgumentCollectionException(string message, Exception inner) : base(message, inner) { }

    /// <summary>Throws if <paramref name="argument"/> is <c>null</c> or an empty collection.</summary>
    /// <typeparam name="T">The element type of the collection.</typeparam>
    /// <param name="argument">The collection to validate.</param>
    /// <param name="paramName">The name of the parameter, for the exception message.</param>
    /// <exception cref="ArgumentNullException"><paramref name="argument"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentCollectionException"><paramref name="argument"/> is empty.</exception>
    public static void ThrowIfNullOrEmpty<T>(IEnumerable<T>? argument, [CallerArgumentExpression(nameof(argument))] string? paramName = null)
    {
        ArgumentNullException.ThrowIfNull(argument, paramName);

        if (!argument.Any())
        {
            throw new ArgumentCollectionException($"Collection cannot be empty: {paramName}");
        }
    }
}
