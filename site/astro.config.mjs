import { defineConfig } from 'astro/config';
import starlight from '@astrojs/starlight';

export default defineConfig({
  integrations: [
    starlight({
      title: 'PndTools',
      description: 'A .NET library for parsing, validating, and inspecting PND (Pandora) package files.',
      social: [
        { icon: 'github', label: 'GitHub', href: 'https://github.com/milkshakeuk/PndTools' },
      ],
      sidebar: [
        { label: 'Getting started', slug: 'getting-started' },
        {
          label: 'Guides',
          items: [{ autogenerate: { directory: 'guides' } }],
        },
        {
          label: 'API reference',
          items: [{ autogenerate: { directory: 'api' } }],
        },
        {
          label: 'Benchmarks',
          items: [{ autogenerate: { directory: 'benchmarks' } }],
        },
      ],
    }),
  ],
});
