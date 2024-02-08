import path from 'path';
import { defineConfig } from 'vite';
import { sync } from 'rimraf';
import fs from 'fs';
import Vue from '@vitejs/plugin-vue';
// import Components from 'unplugin-vue-components/vite';
import LocalizationPlugin from './tools/vite-plugin-localization';
//import { VuetifyResolver } from 'unplugin-vue-components/resolvers'
import Vuetify from 'vite-plugin-vuetify';

const baseDir = path.resolve(__dirname, './');
const join = path.join.bind(path, baseDir);

const outDir = path.resolve(__dirname + '/../wwwroot/assets');

const resxFiles = [
    join('../Resources/QuestionnaireEditor.resx'),
    join('../Resources/QuestionnaireEditor.*.resx')
];

export default defineConfig(({ mode, command }) => {
    const isDevMode = mode === 'development';
    const isProdMode = !isDevMode;

    const base = command == 'serve' ? '/.vite/' : '/assets/';

    if (command == 'serve' && mode != 'test') {
        sync(outDir);
        fs.mkdirSync(outDir);
    }

    return {
        base,
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
            {
                name: 'CopyManifest'
            }
            //Components({
            //resolvers: [VuetifyResolver()],
            //}),
        ],
        server: {
            host: 'localhost',
            strictPort: true,
            base: base,
            port: 3000,
            watch: {
                ignored: ['**/src/locale/**']
            }
        },
        build: {
            minify: isProdMode,
            outDir,
            manifest: true,
            rollupOptions: {
                input: 'src/main.js',
                preserveEntrySignatures: true,

                output: {
                    assetFileNames: assetInfo => {
                        let extType = assetInfo.name.split('.').at(1);
                        if (/png|jpe?g|svg|gif|tiff|bmp|ico/i.test(extType)) {
                            extType = 'img';
                        }
                        if (/ttf|woff|woff2|eot/i.test(extType)) {
                            extType = 'fonts';
                        }
                        if (isDevMode) return `${extType}/[name][extname]`;
                        return `${extType}/[name]-[hash][extname]`;
                    },
                    chunkFileNames: chunkInfo => {
                        if (isDevMode)
                            return chunkInfo.name.endsWith('.js')
                                ? 'js/[name]'
                                : 'js/[name].js';
                        return 'js/[name]-[hash].js';
                    },
                    entryFileNames: chunkInfo => {
                        if (isDevMode) return 'js/[name].js';
                        return 'js/[name]-[hash].js';
                    }
                    //manualChunks: id => {}
                }
            }
        },
        resolve: {
            alias: {
                // eslint-disable-next-line no-undef
                '@': path.resolve(__dirname, './src')
            }
        }
    };
});
