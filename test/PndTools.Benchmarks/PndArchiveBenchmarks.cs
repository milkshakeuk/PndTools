// Copyright (c) milkshakeuk. All rights reserved.
// SPDX-License-Identifier: MIT

using BenchmarkDotNet.Attributes;
using PndTools.IO;

namespace PndTools.Benchmarks;

[MarkdownExporterAttribute.GitHub]
[MemoryDiagnoser]
public class PndArchiveBenchmarks
{
    private byte[] _squashFsBytes = [];
    private byte[] _isoBytes = [];

    [GlobalSetup]
    public void Setup()
    {
        _squashFsBytes = File.ReadAllBytes("TestCase/SORR.pnd");
        _isoBytes = File.ReadAllBytes("TestCase/Bump3.pnd");
    }

    [Benchmark]
    public void Open_SquashFs()
    {
        using var stream = new MemoryStream(_squashFsBytes);
        using var archive = PndArchive.Open(stream);
    }

    [Benchmark]
    public void Open_Iso()
    {
        using var stream = new MemoryStream(_isoBytes);
        using var archive = PndArchive.Open(stream);
    }

    [Benchmark]
    public void ListFiles_SquashFs()
    {
        using var stream = new MemoryStream(_squashFsBytes);
        using var archive = PndArchive.Open(stream);
        _ = archive.ListFiles();
    }

    [Benchmark]
    public void ListFiles_Iso()
    {
        using var stream = new MemoryStream(_isoBytes);
        using var archive = PndArchive.Open(stream);
        _ = archive.ListFiles();
    }
}
