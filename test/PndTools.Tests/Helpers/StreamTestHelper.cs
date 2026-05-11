// Copyright (c) milkshakeuk. All rights reserved.
// SPDX-License-Identifier: MIT

using System.IO;
using System.Text;

namespace PndTools.Tests.Helpers;

public static class StreamTestHelper
{
    public static MemoryStream GenerateRandomStream() =>
        new(GenerateRandomBytes());

    public static byte[] GenerateRandomBytes()
    {
#pragma warning disable CA5394 // Random is intentionally non-cryptographic here; this is test data only
        var rnd = new Random();
        var buffer = new byte[rnd.Next(1, 512)];
        rnd.NextBytes(buffer);
        return buffer;
#pragma warning restore CA5394
    }

    public static MemoryStream GenerateStreamFromString(string input) =>
        new(Encoding.UTF8.GetBytes(input ?? ""));
}
