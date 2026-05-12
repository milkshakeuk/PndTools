---
title: Benchmarks
description: BenchmarkDotNet results for PndTools.
---

Benchmark results are generated automatically by [BenchmarkDotNet](https://benchmarkdotnet.org) as part of the CI build and published here on each release.

To run benchmarks locally:

```bash
dotnet run --project test/PndTools.Benchmarks/PndTools.Benchmarks.csproj --configuration Release
```

Results are written to `BenchmarkDotNet.Artifacts/results/`.
