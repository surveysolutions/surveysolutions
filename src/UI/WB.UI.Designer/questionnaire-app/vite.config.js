import path from 'path'
import { defineConfig } from 'vite'
import Vue from '@vitejs/plugin-vue'
import Components from 'unplugin-vue-components/vite'
//import { VuetifyResolver } from 'unplugin-vue-components/resolvers'
import LocalizationPlugin  from './tools/vite-plugin-localization'
import { ignorePathWatch } from './tools/vite-plugin-watch-ignore'
import Vuetify from 'vite-plugin-vuetify'
import { readdirSync } from 'node:fs'

const baseDir = path.resolve(__dirname, './');
const join = path.join.bind(path, baseDir);

const resxFiles = [
  join('../Resources/QuestionnaireEditor.resx'),
  join('../Resources/QuestionnaireEditor.*.resx')
];

function getExcludeDirs() {
  const resxDirectoryPath = path.resolve(baseDir, 'src', 'locale');
  const filesInResxDirectory = readdirSync(resxDirectoryPath);
  filesInResxDirectory.map(file => console.log( join(resxDirectoryPath, file)));
  return filesInResxDirectory.map(file => join(resxDirectoryPath, file));
}

export default defineConfig(({ mode }) => {

  const isDevMode = mode === 'development';
  const isProdMode = !isDevMode

  return {
    plugins: [
      ignorePathWatch('**/src/locale/**'),
      Vue(),
      Vuetify({ autoImport: true }),
      LocalizationPlugin({
        noHash: true,
        inline: true,
        patterns: resxFiles,
        destination: './src/locale',
        locales: {
            '.': ['QuestionnaireEditor']
        }
      }),
      Components({
        //resolvers: [VuetifyResolver()],
      }),
    ],
    server: {
      watch: {
        ignored: ['**/src/locale/**'],// ['**/src/locale/**'],
      },
    },
    optimizeDeps: {
      exclude: 'locale',
    },
    build: {
      minify: isProdMode,
      rollupOptions: {
        //external: [
        //  ...getExcludeDirs()
        //],
        output: {
          assetFileNames: (assetInfo) => {
            let extType = assetInfo.name.split('.').at(1);
            if (/png|jpe?g|svg|gif|tiff|bmp|ico/i.test(extType)) {
              extType = 'img';
            }
            if (/ttf|woff|woff2|eot/i.test(extType)) {
              extType = 'fonts';
            }
            if (isDevMode) 
              return `${extType}/[name][extname]` 
            return `${extType}/[name]-[hash][extname]`;
          },
          chunkFileNames: (chunkInfo) => { 
            if (isDevMode) 
              return chunkInfo.name.endsWith('.js') ? 'js/[name]' : 'js/[name].js' 
            return 'js/[name]-[hash].js' 
          },
          entryFileNames: (chunkInfo) => { 
            if (isDevMode) 
              return 'js/[name].js' 
            return 'js/[name]-[hash].js' 
          },
          manualChunks: (id) => {
            if (id.includes('node_modules')) {
              return 'vendor';
            }
          },
        },
      },
    },
    resolve: {
        alias: {
            // eslint-disable-next-line no-undef
            '@': path.resolve(__dirname, './src'),
        },
    },
  }
})
