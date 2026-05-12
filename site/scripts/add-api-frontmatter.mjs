#!/usr/bin/env node
// Injects a Starlight-compatible frontmatter title into DefaultDocumentation output.
// DefaultDocumentation emits plain Markdown with no frontmatter; Starlight requires a title.

import { readFileSync, writeFileSync } from 'node:fs';
import { readdir } from 'node:fs/promises';
import { join } from 'node:path';

const apiDir = join(import.meta.dirname, '../src/content/docs/api');

const files = await readdir(apiDir);

for (const file of files) {
  if (!file.endsWith('.md')) continue;

  const path = join(apiDir, file);
  const content = readFileSync(path, 'utf8');

  if (content.startsWith('---')) continue;

  const heading = content.match(/^## (.+)$/m)?.[1]?.trim();
  if (!heading) continue;

  const title = heading
    .replace(/\\/g, '')
    .replace(' Namespace', '')
    .replace(' Class', '')
    .replace(' Struct', '')
    .replace(' Enum', '')
    .replace(' Interface', '');

  writeFileSync(path, `---\ntitle: "${title}"\n---\n\n${content}`);
}
