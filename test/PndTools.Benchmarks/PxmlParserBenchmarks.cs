// Copyright (c) milkshakeuk. All rights reserved.
// SPDX-License-Identifier: MIT

using BenchmarkDotNet.Attributes;

namespace PndTools.Benchmarks;

[MarkdownExporterAttribute.GitHub]
[MemoryDiagnoser]
public class PxmlParserBenchmarks
{
    private string _pxmlString = null!;

    [GlobalSetup]
    public void Setup() =>
        _pxmlString = File.ReadAllText("TestCase/validPxml.xml");

    [Benchmark]
    public void Parse() => PxmlParser.Parse(_pxmlString);
}
