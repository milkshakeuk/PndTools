using System.Text;

namespace PndTools.IO.Extensions;

/// <summary>Extension methods for searching and reading <see cref="Stream"/> instances.</summary>
public static class StreamExtensions
{
    /// <summary>
    /// Searches for a UTF-8 encoded string in the stream, returning the zero-based byte index of
    /// the first match in the given direction, or <c>-1</c> if not found.
    /// </summary>
    /// <param name="stream">The stream to search.</param>
    /// <param name="input">The string to search for.</param>
    /// <param name="direction">The direction to traverse the stream.</param>
    /// <returns>The zero-based byte index of the first match, or <c>-1</c> if not found.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="stream"/> or <paramref name="input"/> is <c>null</c>.</exception>
    public static long Find(this Stream stream, string input, Direction direction = Direction.Forward)
    {
        ArgumentNullException.ThrowIfNull(stream);
        ArgumentNullException.ThrowIfNull(input);
        return stream.Find(Encoding.UTF8.GetBytes(input).AsSpan(), direction);
    }

    /// <summary>
    /// Searches for a byte sequence in the stream, returning the zero-based byte index of
    /// the first match in the given direction, or <c>-1</c> if not found.
    /// </summary>
    /// <param name="stream">The stream to search.</param>
    /// <param name="input">The byte sequence to search for.</param>
    /// <param name="direction">The direction to traverse the stream.</param>
    /// <returns>The zero-based byte index of the first match, or <c>-1</c> if not found.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="stream"/> is <c>null</c>.</exception>
    public static long Find(this Stream stream, byte[] input, Direction direction = Direction.Forward)
    {
        ArgumentNullException.ThrowIfNull(stream);
        return stream.Find(input.AsSpan(), direction);
    }

    /// <summary>
    /// Searches for a byte sequence in the stream, returning the zero-based byte index of
    /// the first match in the given direction, or <c>-1</c> if not found.
    /// </summary>
    /// <param name="stream">The stream to search.</param>
    /// <param name="input">The byte sequence to search for.</param>
    /// <param name="direction">The direction to traverse the stream.</param>
    /// <returns>The zero-based byte index of the first match, or <c>-1</c> if not found.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="stream"/> is <c>null</c>.</exception>
    public static long Find(this Stream stream, ReadOnlySpan<byte> input, Direction direction = Direction.Forward)
    {
        ArgumentNullException.ThrowIfNull(stream);
        return direction switch
        {
            Direction.Forward => TraverseForwards(stream, input),
            Direction.Backwards => TraverseBackwards(stream, input),
            _ => -1
        };
    }

    /// <summary>Returns the bytes between <paramref name="start"/> and <paramref name="end"/> in the stream.</summary>
    /// <param name="stream">The stream to read from.</param>
    /// <param name="start">The zero-based start position, inclusive.</param>
    /// <param name="end">The zero-based end position, exclusive.</param>
    /// <returns>A byte array containing the bytes in the specified range.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="stream"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="start"/> is less than zero, or <paramref name="end"/> is greater than the stream length.
    /// </exception>
    public static byte[] GetBytes(this Stream stream, long start, long end)
    {
        ArgumentNullException.ThrowIfNull(stream);

        if (end > stream.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(end), $"End position cannot be greater than stream length ({stream.Length}).");
        }

        if (start < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(start), "Start position cannot be less than 0.");
        }

        var buffer = new byte[end - start];
        stream.Position = start;
        stream.ReadExactly(buffer);
        return buffer;
    }

    /// <summary>Returns the entire contents of the stream as a byte array.</summary>
    /// <param name="stream">The stream to read.</param>
    /// <returns>A byte array containing all bytes in the stream.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="stream"/> is <c>null</c>.</exception>
    public static byte[] GetBytes(this Stream stream)
    {
        ArgumentNullException.ThrowIfNull(stream);
        var buffer = new byte[stream.Length];
        stream.ReadExactly(buffer);
        return buffer;
    }

    // 4096 matches the OS memory page size and typical storage block size, giving efficient
    // read-ahead behaviour without loading large PND files into memory.
    private const int ChunkSize = 4096;

    private static long TraverseForwards(Stream stream, ReadOnlySpan<byte> pattern)
    {
        var overlap = pattern.Length - 1;
        var step = ChunkSize - overlap;
        var buffer = new byte[ChunkSize];
        var streamLength = stream.Length;

        stream.Position = 0;

        for (long chunkStart = 0; chunkStart <= streamLength - pattern.Length; chunkStart += step)
        {
            var toRead = (int)Math.Min(ChunkSize, streamLength - chunkStart);
            stream.ReadExactly(buffer, 0, toRead);

            var idx = buffer.AsSpan(0, toRead).IndexOf(pattern);

            if (idx >= 0)
            {
                return chunkStart + idx;
            }

            // Rewind to preserve the overlap region for the next chunk
            stream.Position = chunkStart + step;
        }

        return -1;
    }

    private static long TraverseBackwards(Stream stream, ReadOnlySpan<byte> pattern)
    {
        var overlap = pattern.Length - 1;
        var step = ChunkSize - overlap;
        var buffer = new byte[ChunkSize];
        var streamLength = stream.Length;

        for (long chunkEnd = streamLength; chunkEnd > 0; chunkEnd -= step)
        {
            var chunkStart = Math.Max(0, chunkEnd - ChunkSize);
            var toRead = (int)(chunkEnd - chunkStart);

            stream.Position = chunkStart;
            stream.ReadExactly(buffer, 0, toRead);

            var idx = buffer.AsSpan(0, toRead).LastIndexOf(pattern);

            if (idx >= 0)
            {
                return chunkStart + idx;
            }
        }

        return -1;
    }
}

/// <summary>The direction in which to traverse a stream when searching.</summary>
public enum Direction
{
    /// <summary>Traverse the stream forwards from the beginning.</summary>
    Forward,
    /// <summary>Traverse the stream backwards from the end.</summary>
    Backwards
}
