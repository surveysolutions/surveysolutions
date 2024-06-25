import path from 'path';
import { defineConfig } from 'vite';
import { sync } from 'rimraf';
import fs from 'fs';
import Vue from '@vitejs/plugin-vue';
// import Components from 'unplugin-vue-components/vite';
import LocalizationPlugin from './questionnaire/tools/vite-plugin-localization';
//import { VuetifyResolver } from 'unplugin-vue-components/resolvers'
import Vuetify from 'vite-plugin-vuetify';
import mpaPlugin from 'vite-plugin-mpa-plus';
import multiInput from 'rollup-plugin-multi-input';

const ViteFilemanager = require('filemanager-plugin').ViteFilemanager;

const baseDir = path.resolve(__dirname, './');
console.log(baseDir);
const join = path.join.bind(path, baseDir);

const outDir = path.resolve(__dirname, './wwwroot/assets');
console.log(outDir);

const resxFiles = [
    join('../Resources/QuestionnaireEditor.resx'),
    join('../Resources/QuestionnaireEditor.*.resx'),
];

const pages = {
    logon: {
        entry: 'build/entries/logon.js',
        //entry: path.join(baseDir, 'build', 'entries', 'logon.js'),
        // "Areas/Identity/Pages/Layout.Account.cshtml",
        filename: path.join(
            baseDir,
            'Areas',
            'Identity',
            'Pages',
            'Layout.Account.cshtml'
        ),
        //template: path.join(baseDir, 'Areas', 'Identity', 'Pages', 'Layout.Account.cshtml'),
        template: path.join(
            baseDir,
            'Areas',
            'Identity',
            'Pages',
            'Layout.Account.Template.cshtml'
        ),
        //template: "../../Areas/Identity/Pages/Layout.Account.Template.html",
    },
    folders: {
        entry: 'build/entries/folders.js',
        filename: path.join(
            baseDir,
            'Areas',
            'Admin',
            'Views',
            'PublicFolders',
            'Index.cshtml'
        ),
        template: path.join(
            baseDir,
            'Areas',
            'Admin',
            'Views',
            'PublicFolders',
            'Index.Template.cshtml'
        ),
    },
};
console.log(pages.logon.filename);
console.log(pages.logon.template);

const fileTargets = [
    /*{ source: join(".resources", "**", "*.js"), destination: join("dist", "locale"), isFlat: false  },

    { source: join("dist", "img", "**", "*.*"), destination: path.join(hqDist, "wwwroot", "img"), isFlat: false },
    { source: join("dist", "fonts", "**", "*.*"), destination: path.join(hqDist, "wwwroot", "fonts") },
    { source: join("dist", "css", "*.*"), destination: path.join(hqDist, "wwwroot", "css") },
    { source: join("dist", "js", "*.*"), destination: path.join(hqDist, "wwwroot", "js") },
    { source: join("dist", "locale", "hq", "*.*"), destination: path.join(hqDist, "wwwroot", "locale", "hq") },
    { source: join("dist", "locale", "webinterview", "*.*"), destination: path.join(hqDist, "wwwroot", "locale", "webinterview") },

    { source: join("dist", "img", "**", "*.*"), destination: path.join(webTesterDist, "wwwroot", "img"), isFlat: false },
    { source: join("dist", "fonts", "*.*"), destination: path.join(webTesterDist, "wwwroot", "fonts") },
    { source: join("dist", "css", "*.*"), destination: path.join(webTesterDist, "wwwroot", "css") },
    { source: join("dist", "js", "*.*"), destination: path.join(webTesterDist, "wwwroot", "js") },
    { source: join("dist", "locale", "webtester", "*.*"), destination: path.join(webTesterDist, "wwwroot", "locale", "webtester") },*/
];

var pagesSources = [];
var pagesTargets = [];

for (var attr in pages) {
    const pageObj = pages[attr];
    const filename = path.basename(pageObj.filename);
    const filenameHtml = attr + '.html';
    const origFolder = path.dirname(pageObj.filename);
    const templateFilename = path.basename(pageObj.template);
    const templateFilenameHtml = attr + '.html';
    var templatesFolderFull = path.join(baseDir, '.templates');
    var destFileFolderFull = path.join(outDir, '.templates');
    var templateHtmlPath = path.join(templatesFolderFull, templateFilenameHtml);
    var filenameHtmlPath = path.join(destFileFolderFull, filenameHtml);
    var filenamePath = path.join(destFileFolderFull, filename);
    pagesSources.push({
        source: pageObj.template,
        destination: templatesFolderFull,
        name: templateFilenameHtml,
    });
    pagesTargets.push({
        source: filenameHtmlPath,
        destination: origFolder,
        name: filename,
    });
    pagesTargets.push({
        source: path.join(outDir, filenameHtml),
        destination: origFolder,
        name: filename,
    });

    pageObj.filename = filenameHtml;
    pageObj.template = templateHtmlPath;
}

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
            Vuetify({ autoImport: { labs: true } }),
            ViteFilemanager({
                customHooks: [
                    {
                        hookName: 'options',
                        commands: {
                            del: {
                                items: [outDir],
                            },
                            copy: { items: pagesSources },
                        },
                    },
                    {
                        hookName: 'closeBundle',
                        commands: {
                            copy: { items: pagesTargets.concat(fileTargets) },
                            del: {
                                items: ['./.templates', outDir + '/.templates'],
                            },
                        },
                    },
                ],
                options: {
                    parallel: 1,
                    log: 'all',
                    //log: 'error'
                },
            }),
            /*LocalizationPlugin({
                noHash: true,
                inline: true,
                patterns: resxFiles,
                destination: './src/locale',
                locales: {
                    '.': ['QuestionnaireEditor'],
                },
            }),*/
            {
                name: 'CopyManifest',
            },
            //Components({
            //resolvers: [VuetifyResolver()],
            //}),
            mpaPlugin({
                pages: pages,
            }),
            /*multiInput({ 
                //relative: 'src/', 
                transformOutputPath: (output, input) => { 
                    console.log('in: ' + input)
                    console.log('out: ' + output)
                    if (input.endWith('.cshtml'))
                        return `awesome/path/${path.basename(output)}` 
                }, 
            })*/
        ],
        css: {
            preprocessorOptions: {
                less: {
                    additionalData: '@icon-font-path: "/fonts/";',
                    relativeUrls: true,
                    rootpath: '../',
                    javascriptEnabled: true,
                },
            },
        },
        server: {
            host: 'localhost',
            strictPort: true,
            base: base,
            port: 3000,
            watch: {
                ignored: ['**/src/locale/**'],
            },
        },
        assetsInclude: ['**/*.cshtml'],
        build: {
            minify: isProdMode,
            outDir,
            manifest: true,
            rollupOptions: {
                //input: 'questionnaire/src/main.js',
                //input: 'src/main.js',
                preserveEntrySignatures: true,
                cache: false,

                output: {
                    /*assetFileNames: (assetInfo) => {
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
                    chunkFileNames: (chunkInfo) => {
                        if (isDevMode)
                            return chunkInfo.name.endsWith('.js')
                                ? 'js/[name]'
                                : 'js/[name].js';
                        return 'js/[name]-[hash].js';
                    },
                    entryFileNames: (chunkInfo) => {
                        if (isDevMode) return 'js/[name].js';
                        return 'js/[name]-[hash].js';
                    },*/
                    //manualChunks: id => {}
                },
            },
        },
        resolve: {
            alias: {
                // eslint-disable-next-line no-undef
                '@': path.resolve(__dirname, './'),
            },
            extensions: ['.mjs', '.js', '.ts', '.jsx', '.tsx', '.json', '.vue'],
        },
    };
});
