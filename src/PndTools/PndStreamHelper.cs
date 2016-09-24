using PndTools.Extentions;
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

        public static string GetPxml(Stream stream)
        {
            var position = FindPxml(stream);
            var data = stream.GetBytes(position.Start, position.End);

            var xmlDeclaration = $"<?xml version=\"1.0\" encoding=\"UTF-8\"?>{Environment.NewLine}";

            return xmlDeclaration + Encoding.UTF8.GetString(data);
        }

        public static byte[] GetIcon(Stream stream)
        {
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
