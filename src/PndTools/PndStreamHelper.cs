using PndTools.Extensions;
using System;
using System.IO;
using System.Text;

namespace PndTools
{
    public static class PndStreamHelper
    {
        private const string PxmlStart = "<PXML";
        private const string PxmlEnd = "</PXML>";
        private static readonly byte[] PngMagicNumber = { 0x89, 0x50, 0x4e, 0x47, 0x0d, 0x0a, 0x1a, 0x0a };

        private static Position FindPxml(Stream stream)
        {
            var start = stream.Find(PxmlStart, Direction.Backwards);
            var end = stream.Find(PxmlEnd, Direction.Backwards);

            if (start == -1 || end == -1)
            {
                throw new InvalidPndException("Pxml is missing or incomplete");
            }

            // add byte length of "</PXML>" to the end position
            end += Encoding.UTF8.GetBytes(PxmlEnd).Length;

            return new Position(start, end);
        }

        /// <summary>
        ///     Retreives the PXML as a string from the <paramref name="stream" />.
        /// </summary>
        /// <param name="stream">File stream from which to search</param>
        /// <returns>PXML as a string</returns>
        /// <exception cref="PndTools.InvalidPndException"><paramref name="stream" /> is missing PXML.</exception>
        /// <exception cref="System.ArgumentNullException"><paramref name="stream" /> is <c>null</c>.</exception>
        public static string GetPxml(Stream stream)
        {
            Guard.AgainstNullArgument(nameof(stream), stream);

            var position = FindPxml(stream);
            var data = stream.GetBytes(position.Start, position.End);

            var xmlDeclaration = Encoding.UTF8.GetBytes($"<?xml version=\"1.0\" encoding=\"UTF-8\"?>{Environment.NewLine}");

            var buffer = new byte[xmlDeclaration.Length + data.Length];

            Buffer.BlockCopy(xmlDeclaration, 0, buffer, 0, xmlDeclaration.Length);
            Buffer.BlockCopy(data, 0, buffer, xmlDeclaration.Length, data.Length);

            return Encoding.UTF8.GetString(buffer);
        }

        /// <summary>
        ///     Retreives the Icon as a byte array from the <paramref name="stream" />.
        /// </summary>
        /// <param name="stream">File stream from which to search</param>
        /// <returns>Icon as a byte array</returns>
        /// <exception cref="PndTools.InvalidPndException"><paramref name="stream" /> is missing Icon.</exception>
        /// <exception cref="System.ArgumentNullException"><paramref name="stream" /> is <c>null</c>.</exception>
        public static byte[] GetIcon(Stream stream)
        {
            Guard.AgainstNullArgument(nameof(stream), stream);

            var position = FindIcon(stream);
            return stream.GetBytes(position.Start, position.End);   
        }

        private static Position FindIcon(Stream stream)
        {
            var start = stream.Find(PngMagicNumber, Direction.Backwards);

            if (start == -1)
            {
                throw new InvalidPndException("Icon is missing or incomplete");
            }

            var end = stream.Length;

            return new Position(start, end);
        }
    }

    internal class Position : ValueObject<Position>
    {
        public readonly long Start, End;

        public Position(long start, long end)
        {
            Start = start;
            End = end;
        }
    }

    public class InvalidPndException : Exception
    {
        public InvalidPndException(string message) : base(message)
        {
        }
    }
}
