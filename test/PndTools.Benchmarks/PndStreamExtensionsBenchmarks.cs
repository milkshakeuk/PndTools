// Copyright (c) milkshakeuk. All rights reserved.
// SPDX-License-Identifier: MIT

using BenchmarkDotNet.Attributes;
using PndTools.IO.Extensions;

namespace PndTools.Benchmarks;

[MarkdownExporterAttribute.GitHub]
[MemoryDiagnoser]
public class PndStreamExtensionsBenchmarks
{
    private byte[] _pndBytes = [];
    private string _outputDirectory = null!;

    [GlobalSetup]
    public void Setup()
    {
        _pndBytes = File.ReadAllBytes("TestCase/SORR.pnd");
        _outputDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(_outputDirectory);
    }

    [GlobalCleanup]
    public void Cleanup() => Directory.Delete(_outputDirectory, recursive: true);

    [Benchmark]
    public void DetectArchiveType()
    {
        using var stream = new MemoryStream(_pndBytes);
        _ = stream.DetectArchiveType();
    }

    [Benchmark]
    public async Task DetectArchiveTypeAsync()
    {
        using var stream = new MemoryStream(_pndBytes);
        _ = await stream.DetectArchiveTypeAsync();
    }

    [Benchmark]
    public void GetPxml()
    {
        using var stream = new MemoryStream(_pndBytes);
        _ = stream.GetPxml();
    }

    [Benchmark]
    public async Task GetPxmlAsync()
    {
        using var stream = new MemoryStream(_pndBytes);
        _ = await stream.GetPxmlAsync();
    }

    [Benchmark]
    public void GetIcon()
    {
        using var stream = new MemoryStream(_pndBytes);
        _ = stream.GetIcon();
    }

    [Benchmark]
    public async Task GetIconAsync()
    {
        using var stream = new MemoryStream(_pndBytes);
        _ = await stream.GetIconAsync();
    }

    [Benchmark]
    public void SavePxml()
    {
        using var stream = new MemoryStream(_pndBytes);
        stream.SavePxml(Path.Combine(_outputDirectory, Path.GetRandomFileName()));
    }

    [Benchmark]
    public async Task SavePxmlAsync()
    {
        using var stream = new MemoryStream(_pndBytes);
        await stream.SavePxmlAsync(Path.Combine(_outputDirectory, Path.GetRandomFileName()));
    }

    [Benchmark]
    public void SaveIcon()
    {
        using var stream = new MemoryStream(_pndBytes);
        stream.SaveIcon(Path.Combine(_outputDirectory, Path.GetRandomFileName()));
    }

    [Benchmark]
    public async Task SaveIconAsync()
    {
        using var stream = new MemoryStream(_pndBytes);
        await stream.SaveIconAsync(Path.Combine(_outputDirectory, Path.GetRandomFileName()));
    }
}
