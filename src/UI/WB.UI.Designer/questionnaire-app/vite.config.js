import path from 'path'
import { defineConfig } from 'vite'
import Vue from '@vitejs/plugin-vue'
import Components from 'unplugin-vue-components/vite'
//import { VuetifyResolver } from 'unplugin-vue-components/resolvers'
import LocalizationPlugin  from './tools/vite-plugin-localization'
import Vuetify from 'vite-plugin-vuetify'


const baseDir = path.resolve(__dirname, './');
const join = path.join.bind(path, baseDir);

const resxFiles = [
  join('../Resources/QuestionnaireEditor.resx'),
  join('../Resources/QuestionnaireEditor.*.resx')
];


export default defineConfig(({ mode }) => {

  const isDevMode = mode === 'development';
  const isProdMode = !isDevMode

  return {
    plugins: [
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
        ignored: ['**/src/locale/**'],
      },
    },
    build: {
      minify: isProdMode,
      rollupOptions: {
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
    rollupOptions: {
      input: 'src/main.js',
      format: 'system',
      preserveEntrySignatures: true
    },
  }
})
