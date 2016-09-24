using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace PndTools.Extentions
{
    public static class StreamExtentions
    {
        /// <summary>
        /// Searches for a byte sequence that matches the sequence defined by
        /// <paramref name="input" />, starting at the end of <paramref name="stream" /> 
        /// and working backwards returning the zero-based index of the first occurrence
        /// within the byte array of the <paramref name="stream" />
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="input"></param>
        /// <param name="direction"></param>
        /// <returns>
        /// The zero-based index of the first occurrence of an byte sequence that matches the
        /// sequence defined by <paramref name="input" />, if found; otherwise, –1.
        /// </returns>
        public static long Find(this Stream stream, string input, Direction direction = Direction.Forward)
        {
            var pattern = Encoding.UTF8.GetBytes(input);

            return stream.Find(pattern, direction);
        }

        /// <summary>
        /// Searches for a byte sequence that matches the sequence defined by
        /// <paramref name="input" />, starting at the end of <paramref name="stream" /> 
        /// and working backwards returning the zero-based index of the first occurrence
        /// within the byte array of the <paramref name="stream" />
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="input"></param>
        /// <param name="direction"></param>
        /// <returns>
        /// The zero-based index of the first occurrence of an byte sequence that matches the
        /// sequence defined by <paramref name="input" />, if found; otherwise, –1.
        /// </returns>
        public static long Find(this Stream stream, byte[] input, Direction direction = Direction.Forward)
        {
            switch (direction)
            {
                case Direction.Forward:
                    return TraverseForwards(stream, input);
                case Direction.Backwards:
                    return TraverseBackwards(stream, input);
                default:
                    return -1;
            }
        }

        /// <summary>
        /// Returns a sub section of the stream designated by the <paramref name="start"/> and <paramref name="end"/> positions
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns>A byte array containing the bytes located between the <paramref name="start"/> and <paramref name="end"/> position of the <paramref name="stream"/></returns>
        public static byte[] GetBytes(this Stream stream, long start, long end)
        {
            if (end > stream.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(end), $"End position cannot be greater than stream length ({stream.Length}).");
            }

            if (start < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(start), "Start position cannot be less than 0.");
            }

            var length = end - start;
            var buffer = new byte[length];

            stream.Position = start;
            stream.Read(buffer, 0, buffer.Length);

            return buffer;
        }

        /// <summary>
        /// Returns the byte array of the <paramref name="stream"/>
        /// </summary>
        /// <param name="stream"></param>
        /// <returns>A byte array</returns>
        public static byte[] GetBytes(this Stream stream)
        {
            var length = stream.Length;
            var buffer = new byte[length];

            stream.Read(buffer, 0, buffer.Length);

            return buffer;
        }

        private static long TraverseForwards(Stream stream, IReadOnlyCollection<byte> pattern)
        {
            var buffer = new byte[pattern.Count];
            var end = stream.Length - pattern.Count;

            for (var position = 0; position < end; position++)
            {
                stream.Position = position;
                stream.Read(buffer, 0, buffer.Length);

                if (pattern.SequenceEqual(buffer))
                {
                    return position;
                }
            }

            return -1;
        }

        private static long TraverseBackwards(Stream stream, IReadOnlyCollection<byte> pattern)
        {
            var buffer = new byte[pattern.Count];
            var start = stream.Length - pattern.Count;

            for (var position = start; position > 0; position--)
            {
                stream.Position = position;
                stream.Read(buffer, 0, buffer.Length);

                if (pattern.SequenceEqual(buffer))
                {
                    return position;
                }
            }

            return -1;
        }
    }

    public enum Direction
    {
        /// <summary>
        /// Traverse the stream forwards starting at the beginning
        /// </summary>
        Forward,

        /// <summary>
        /// Traverse the stream backwards starting from the end
        /// </summary>
        Backwards
    }
}
