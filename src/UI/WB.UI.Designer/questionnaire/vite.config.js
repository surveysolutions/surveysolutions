import path from 'path';
import { defineConfig } from 'vite';
import { sync } from 'rimraf';
import fs from 'fs';
import Vue from '@vitejs/plugin-vue';
// import Components from 'unplugin-vue-components/vite';
import LocalizationPlugin from './tools/vite-plugin-localization';
//import { VuetifyResolver } from 'unplugin-vue-components/resolvers'
import Vuetify from 'vite-plugin-vuetify';
import { normalizePath } from 'vite';

const baseDir = path.resolve(__dirname, './');
const join = path.join.bind(path, baseDir);

const outDir = path.resolve(__dirname + '/../wwwroot/assets');
//const outDir = path.resolve(__dirname + '/../wwwroot');

export default defineConfig(({ mode, command }) => {
    const isDevMode = mode === 'development';
    const isProdMode = !isDevMode;

    const base = command == 'serve' ? '/.vite/' : '/assets/';
    //const base = command == 'serve' ? '/.vite/' : '/';

    if (command == 'serve' && mode != 'test') {
        sync(outDir);
        fs.mkdirSync(outDir);
    }

    return {
        base,
        plugins: [
            Vue(),
            Vuetify({ autoImport: { labs: true } }),
            LocalizationPlugin({
                noHash: true,
                inline: true,
                patterns: [
                    normalizePath(
                        join('../Resources/QuestionnaireEditor.resx')
                    ),
                    normalizePath(
                        join('../Resources/QuestionnaireEditor.*.resx')
                    ),
                    normalizePath(join('../Resources/AccountResources.resx')),
                    normalizePath(join('../Resources/AccountResources.*.resx')),
                    normalizePath(
                        join('../Resources/QuestionnaireController.resx')
                    ),
                    normalizePath(
                    join('../Resources/QuestionnaireController.*.resx')
                    )
                ],
                destination: './src/locale',
                locales: {
                    '.': [
                        'QuestionnaireEditor',
                        'AccountResources',
                        'QuestionnaireController'
                    ]
                }
            }),
            {
                name: 'CopyManifest'
            }
            //Components({
            //resolvers: [VuetifyResolver()],
            //}),
        ],
        css: {
            preprocessorOptions: {
                less: {
                    additionalData: '@icon-font-path: "/fonts/";',
                    //relativeUrls: false,
                    //rootpath: '../../../'
                    //rootpath: '../../',
                    javascriptEnabled: true
                }
            }
        },
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
