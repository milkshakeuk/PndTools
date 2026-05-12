// Copyright (c) milkshakeuk. All rights reserved.
// SPDX-License-Identifier: MIT

using BenchmarkDotNet.Attributes;
using System.Xml.Linq;

namespace PndTools.Benchmarks;

[MarkdownExporterAttribute.GitHub]
[MemoryDiagnoser]
public class PxmlParserBenchmarks
{
    private XDocument _document = null!;

    [GlobalSetup]
    public void Setup() =>
        _document = XDocument.Load("TestCase/validPxml.xml");

    [Benchmark]
    public void Parse() => PxmlParser.Parse(_document);
}
