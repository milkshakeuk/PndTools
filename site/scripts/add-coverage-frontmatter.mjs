#!/usr/bin/env node
// Injects a Starlight-compatible frontmatter title into the ReportGenerator
// MarkdownSummaryGithub output, which is emitted without frontmatter.

import { readFileSync, writeFileSync } from 'node:fs';
import { readdir } from 'node:fs/promises';
import { join } from 'node:path';

const coverageDir = join(import.meta.dirname, '../../docs/coverage');

const files = await readdir(coverageDir);

for (const file of files) {
  if (!file.endsWith('.md')) continue;

  const path = join(coverageDir, file);
  const content = readFileSync(path, 'utf8');

  if (content.startsWith('---')) continue;

  writeFileSync(path, `---\ntitle: "Coverage summary"\n---\n\n${content}`);
}
