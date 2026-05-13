#!/usr/bin/env node
// Injects a Starlight-compatible frontmatter title into BenchmarkDotNet GitHub report output.
// Reports are named {Namespace}.{ClassName}-report-github.md with no frontmatter.

import { readFileSync, writeFileSync } from 'node:fs';
import { readdir } from 'node:fs/promises';
import { join } from 'node:path';

const benchmarksDir = join(import.meta.dirname, '../../docs/benchmarks');

const files = await readdir(benchmarksDir);

for (const file of files) {
  if (!file.endsWith('.md')) continue;

  const path = join(benchmarksDir, file);
  const content = readFileSync(path, 'utf8');

  if (content.startsWith('---')) continue;

  // Extract class name from filename: PndTools.Benchmarks.PndArchiveBenchmarks-report-github.md
  const className = file.replace(/-report-github\.md$/, '').split('.').at(-1);
  if (!className) continue;

  // PndArchiveBenchmarks → Archive benchmarks
  // PndStreamExtensionsBenchmarks → Stream extension benchmarks
  // PxmlParserBenchmarks → PXML parser benchmarks
  const title = className
    .replace(/Benchmarks$/, '')
    .replace(/^Pnd/, '')
    .replace(/([A-Z])/g, ' $1')
    .trim()
    .replace(/\s+/g, ' ')
    .toLowerCase()
    .replace('pxml', 'PXML')
    .replace(/^./, c => c.toUpperCase())
    + ' benchmarks';

  writeFileSync(path, `---\ntitle: "${title}"\n---\n\n${content}`);
}
