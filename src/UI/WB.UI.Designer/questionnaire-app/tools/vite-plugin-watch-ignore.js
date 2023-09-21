/*import type { PluginOption } from 'vite';

export function ignorePathWatch(modules: string[]): PluginOption {
  return {
    name: 'ignore-path-watch',
    config() {
      return {
        server: {
          watch: {
            ignored: modules,
          },
        },
      };
    },
  };
}*/



import { ViteDevServer } from 'vite';

export function ignorePathWatch(path) {
	return {
		name: 'ignore-path-watch',
		configureServer: (server) => {
			server.watcher.options = {
				...server.watcher.options,
				ignored: [
					path,
					'**/.git/**',
				]
			}
		}
	}
}
