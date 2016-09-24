using System;
using System.IO;
using System.Text;

namespace PndToolsTests.Helpers
{
    public static class StreamTestHelper
    {
        public static MemoryStream GenerateRandomStream()
        {
            return new MemoryStream(GenerateRandomBytes());
        }

        public static byte[] GenerateRandomBytes()
        {
            var rnd = new Random();
            var buffer = new byte[rnd.Next(1, 512)];
            rnd.NextBytes(buffer);
            return buffer;
        }

        public static MemoryStream GenerateStreamFromString(string input)
        {
            return new MemoryStream(Encoding.UTF8.GetBytes(input ?? ""));
        }
    }
}