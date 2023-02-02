import path from 'path'
import { defineConfig } from 'vite'
import Vue from '@vitejs/plugin-vue'
import Components from 'unplugin-vue-components/vite'
//import { VuetifyResolver } from 'unplugin-vue-components/resolvers'
import  LocalizationPlugin  from './tools/vite-plugin-localization'
import Vuetify from 'vite-plugin-vuetify'

const baseDir = path.resolve(__dirname, './');
const join = path.join.bind(path, baseDir);

const resxFiles = [
  join('../Resources/QuestionnaireEditor.resx'),
  join('../Resources/QuestionnaireEditor.*.resx')
];

export default defineConfig({
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
    resolve: {
        alias: {
            // eslint-disable-next-line no-undef
            '@': path.resolve(__dirname, './src'),
        },
    },
})
