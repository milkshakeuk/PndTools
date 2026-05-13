import { defineConfig } from 'astro/config';
import starlight from '@astrojs/starlight';
import { visit } from 'unist-util-visit';

const base = '/PndTools';

// Prefixes absolute internal links with the site base path so markdown content
// links like /guides/foo work correctly when served under a subpath.
function remarkPrefixInternalLinks() {
  return function (tree) {
    visit(tree, ['link', 'definition'], (node) => {
      if (node.url?.startsWith('/') && !node.url.startsWith(base)) {
        node.url = base + node.url;
      }
    });
  };
}

export default defineConfig({
  site: 'https://milkshakeuk.github.io',
  base,
  markdown: {
    remarkPlugins: [remarkPrefixInternalLinks],
  },
  integrations: [
    starlight({
      title: 'PndTools',
      description: 'A .NET library for parsing, validating, and inspecting PND (Pandora) package files.',
      social: [
        { icon: 'github', label: 'GitHub', href: 'https://github.com/milkshakeuk/PndTools' },
      ],
      lastUpdated: true,
      sidebar: [
        { label: 'Getting started', slug: 'getting-started' },
        {
          label: 'Guides',
          items: [{ autogenerate: { directory: 'guides' } }],
        },
        {
          label: 'Benchmarks',
          items: [{ autogenerate: { directory: 'benchmarks' } }],
        },
        {
          label: 'Code coverage',
          items: [{ autogenerate: { directory: 'coverage' } }],
        },
      ],
    }),
  ],
});
