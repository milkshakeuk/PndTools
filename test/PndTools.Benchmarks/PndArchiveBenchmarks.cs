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
    private string _squashFsFirstFile = null!;
    private string _outputDirectory = null!;

    [GlobalSetup]
    public void Setup()
    {
        _squashFsBytes = File.ReadAllBytes("TestCase/SORR.pnd");
        _isoBytes = File.ReadAllBytes("TestCase/Bump3.pnd");

        using var stream = new MemoryStream(_squashFsBytes);
        using var archive = PndArchive.Open(stream);
        _squashFsFirstFile = archive.ListFiles()[0];

        _outputDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(_outputDirectory);
    }

    [GlobalCleanup]
    public void Cleanup() => Directory.Delete(_outputDirectory, recursive: true);

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

    [Benchmark]
    public void ExtractFile_SquashFs()
    {
        using var stream = new MemoryStream(_squashFsBytes);
        using var archive = PndArchive.Open(stream);
        archive.ExtractFile(_squashFsFirstFile, Path.Combine(_outputDirectory, Path.GetRandomFileName()));
    }

    [Benchmark]
    public async Task ExtractFileAsync_SquashFs()
    {
        using var stream = new MemoryStream(_squashFsBytes);
        using var archive = PndArchive.Open(stream);
        await archive.ExtractFileAsync(_squashFsFirstFile, Path.Combine(_outputDirectory, Path.GetRandomFileName()));
    }

    [Benchmark]
    public void ExtractAll_SquashFs()
    {
        var outputDirectory = Path.Combine(_outputDirectory, Path.GetRandomFileName());
        using var stream = new MemoryStream(_squashFsBytes);
        using var archive = PndArchive.Open(stream);
        archive.ExtractAll(outputDirectory);
    }

    [Benchmark]
    public async Task ExtractAllAsync_SquashFs()
    {
        var outputDirectory = Path.Combine(_outputDirectory, Path.GetRandomFileName());
        using var stream = new MemoryStream(_squashFsBytes);
        using var archive = PndArchive.Open(stream);
        await archive.ExtractAllAsync(outputDirectory);
    }
}
