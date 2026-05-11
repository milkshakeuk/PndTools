using System.IO;
using System.Text;

namespace PndTools.Tests.Helpers;

public static class StreamTestHelper
{
    public static MemoryStream GenerateRandomStream() =>
        new(GenerateRandomBytes());

    public static byte[] GenerateRandomBytes()
    {
        var rnd = new Random();
        var buffer = new byte[rnd.Next(1, 512)];
        rnd.NextBytes(buffer);
        return buffer;
    }

    public static MemoryStream GenerateStreamFromString(string input) =>
        new(Encoding.UTF8.GetBytes(input ?? ""));
}
