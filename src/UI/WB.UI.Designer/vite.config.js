import path from 'path';
import { defineConfig } from 'vite';
import viteCommonjs from 'vite-plugin-commonjs';
import mpaPlugin from 'vite-plugin-mpa-plus';
import inject from '@rollup/plugin-inject';
import requireTransform from 'vite-plugin-require-transform';

import { ViteFilemanager } from 'filemanager-plugin';

const baseDir = path.resolve(__dirname, './');
const join = path.join.bind(path, baseDir);

const outDir = path.resolve(__dirname, './wwwroot');

const pages = {
    logon: {
        entry: path.join(baseDir, 'build/entries/logon.js'),
        filename: path.join(
            baseDir,
            'Areas',
            'Identity',
            'Pages',
            'Layout.Account.cshtml'
        ),
        template: path.join(
            baseDir,
            'Areas',
            'Identity',
            'Pages',
            'Layout.Account.Template.cshtml'
        ),
    },
    validationIdentity: {
        entry: path.join(baseDir, 'build/entries/validation.js'),
        filename: path.join(
            baseDir,
            'Areas/Identity/Pages/_ValidationScriptsPartial.cshtml'
        ),
        template: path.join(
            baseDir,
            'Areas/Identity/Pages/_ValidationScriptsPartial.Template.cshtml'
        ),
    },
    folders: {
        entry: path.join(baseDir, 'build/entries/folders.js'),
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
    questionnaireList: {
        entry: 'build/entries/questionnaireList.js',
        filename: path.join(baseDir, 'Views/QuestionnaireList/Public.cshtml'),
        template: path.join(
            baseDir,
            'Views/QuestionnaireList/Public.Template.cshtml'
        ),
    },
    validationScriptsPartial: {
        entry: 'build/entries/validation.js',
        filename: path.join(
            baseDir,
            'Views/Shared/_ValidationScriptsPartial.cshtml'
        ),
        template: path.join(
            baseDir,
            'Views/Shared/_ValidationScriptsPartial.Template.cshtml'
        ),
    },
    sharedLayout: {
        entry: 'build/entries/validation.js',
        filename: path.join(baseDir, 'Views/Shared/Layout.cshtml'),
        template: path.join(baseDir, 'Views/Shared/Layout.Template.cshtml'),
    },
    errorLayout: {
        entry: 'build/entries/validation.js',
        filename: path.join(baseDir, 'Views/Error/Layout.Error.cshtml'),
        template: path.join(
            baseDir,
            'Views/Error/Layout.Error.Template.cshtml'
        ),
    },
    controlPanel: {
        entry: 'build/entries/validation.js',
        filename: path.join(
            baseDir,
            'Areas/Admin/Views/Shared/Layout.ControlPanel.cshtml'
        ),
        template: path.join(
            baseDir,
            'Areas/Admin/Views/Shared/Layout.ControlPanel.Template.cshtml'
        ),
    },
    pdf: {
        entry: 'build/entries/pdf.js',
        filename: path.join(baseDir, 'Areas/Pdf/Views/Pdf/Pdf.empty.cshtml'),
        template: path.join(baseDir, 'Areas/Pdf/Views/Pdf/Pdf.Template.cshtml'),
    },
};

const fileTargets = [
    {
        source: join('questionnaire', 'content', 'i', '*.*'),
        destination: path.join(outDir, 'i'),
        isFlat: false,
    },
    {
        source: join('questionnaire', 'content', 'lib', 'i', '*.*'),
        destination: path.join(outDir, 'i'),
        isFlat: false,
    },
    {
        source: join('questionnaire', 'content', 'fonts', '*.*'),
        destination: path.join(outDir, 'fonts'),
        isFlat: false,
    },
    {
        source: join('Content', 'images', '*.*'),
        destination: path.join(outDir, 'images'),
        isFlat: false,
    },
    {
        source: join('Content', 'fonts', '*.*'),
        destination: path.join(outDir, 'fonts'),
        isFlat: false,
    },
    {
        source: join('node_modules/@mdi/font/fonts', '*.*'),
        destination: path.join(outDir, 'fonts'),
        isFlat: false,
    },
    {
        source: join('Content', 'identity', '*.*'),
        destination: outDir,
        isFlat: false,
    },
    {
        source: join('Content', 'qbank', '*.*'),
        destination: path.join(outDir, 'qbank'),
        isFlat: false,
    },
    {
        source: join('Areas', 'Pdf', 'Content', 'images', '*.*'),
        destination: path.join(outDir, 'images'),
        isFlat: false,
    },
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

    //const base = command == 'serve' ? '/.vite/' : '/assets/';
    const base = command == 'serve' ? '/.vite/' : '/';

    return {
        base,
        optimizeDeps: {
            include: ['jquery'],
        },
        plugins: [
            viteCommonjs(),
            requireTransform({
                fileRegex: /.\js$/,
            }),
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
                            copy: {
                                items: pagesTargets.concat(fileTargets),
                            },
                            del: {
                                items: ['./.templates', outDir + '/.templates'],
                            },
                        },
                    },
                ],
                options: {
                    parallel: 1,
                    //log: 'all',
                    log: 'error',
                },
            }),
            /*{
                name: 'CopyManifest',
            },*/
            mpaPlugin({
                pages: pages,
            }),
            inject({
                jQuery: 'jquery',
                $: 'jquery',
            }),
        ],
        css: {
            preprocessorOptions: {
                less: {
                    additionalData: '@icon-font-path: "/fonts/";',
                    relativeUrls: true,
                    rootpath: './',
                    javascriptEnabled: true,
                },
            },
        },
        resolve: {
            alias: [
                {
                    find: '@',
                    replacement: path.resolve(__dirname, './'),
                },
                {
                    find: '~',
                    replacement: path.resolve(__dirname, './'),
                },
                {
                    find: 'jquery',
                    replacement: 'jquery/dist/jquery.min.js',
                },
                {
                    find: 'jquery-ui',
                    replacement: 'jquery-ui-dist/jquery-ui.js',
                },
            ],
            extensions: ['.mjs', '.js', '.ts', '.jsx', '.tsx', '.json', '.vue'],
        },
        /*define: {
            'window.jQuery': 'jquery',
            'window.$': 'jquery',
        },*/
        server: {
            host: 'localhost',
            strictPort: true,
            base: base,
            port: 3000,
            watch: {
                ignored: ['**/src/locale/**'],
            },
        },
        build: {
            minify: isProdMode,
            outDir,
            //manifest: true,
            rollupOptions: {
                //external: ['jquery'],
                //preserveEntrySignatures: true,
                cache: false,
                //maxParallelFileOps: 2,
                plugins: [
                    inject({
                        $: 'jquery',
                        jQuery: 'jquery',
                    }),
                ],
                /*input: {
                    list: '/build/entries/list.js',
                    simplepage: '/build/entries/simplepage.js',
                    utils: '/build/entries/utils.js',
                },*/
                output: {
                    //inlineDynamicImports: false,
                    //manualChunks: undefined,
                    //format: 'es',
                    //globals: {
                    //    jquery: '$', // global variable name for the external library
                    //},
                    assetFileNames: (assetInfo) => {
                        if (assetInfo.name == 'pdf.css') {
                            return `css/[name][extname]`;
                        }
                        let extType = assetInfo.name.split('.').slice(-1);
                        if (/png|jpe?g|svg|gif|tiff|bmp|ico/i.test(extType)) {
                            extType = 'i';
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
                    },
                    //manualChunks: id => {}
                },
            },
        },
    };
});
