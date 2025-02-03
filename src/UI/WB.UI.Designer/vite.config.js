import path from 'path';
import { defineConfig } from 'vite';
import inject from '@rollup/plugin-inject';
import { ViteFilemanager } from 'filemanager-plugin';
import Vue from '@vitejs/plugin-vue';
import Vuetify from 'vite-plugin-vuetify';
import LocalizationPlugin from './questionnaire/tools/vite-plugin-localization';
import { normalizePath } from 'vite';

const baseDir = path.resolve(__dirname, './');
const join = path.join.bind(path, baseDir);

const outDir = path.resolve(__dirname, './wwwroot');

const pages = {
    logon: {
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
    folders: {
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
    editform: {
        filename: path.join(
            baseDir,
            'Areas',
            'Admin',
            'Views',
            'ControlPanel',
            'MakeAdmin.cshtml'
        ),
        template: path.join(
            baseDir,
            'Areas',
            'Admin',
            'Views',
            'ControlPanel',
            'MakeAdmin.Template.cshtml'
        ),
    },
    foldersScript: {
        filename: path.join(
            baseDir,
            'Views/QuestionnaireList/_FoldersScript.cshtml'
        ),
        template: path.join(
            baseDir,
            'Views/QuestionnaireList/_FoldersScript.Template.cshtml'
        ),
    },
    sharedLayout: {
        filename: path.join(baseDir, 'Views/Shared/Layout.cshtml'),
        template: path.join(baseDir, 'Views/Shared/Layout.Template.cshtml'),
    },
    errorLayout: {
        filename: path.join(baseDir, 'Views/Error/Layout.Error.cshtml'),
        template: path.join(
            baseDir,
            'Views/Error/Layout.Error.Template.cshtml'
        ),
    },
    controlPanel: {
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
        filename: path.join(baseDir, 'Areas/Pdf/Views/Pdf/Pdf.empty.cshtml'),
        template: path.join(baseDir, 'Areas/Pdf/Views/Pdf/Pdf.Template.cshtml'),
    },
    /*questionnare: {
        filename: path.join(baseDir, 'questionnaire/index.html'),
        template: path.join(baseDir, 'questionnaire/index.template.html'),
    },*/
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
        source: join('node_modules', '@mdi', 'font', 'fonts', '*.*'),
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

let inputPages = {};

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

    inputPages[attr] = templateHtmlPath;
}

inputPages.questionnare = path.join(baseDir, 'questionnaire/src/main.js')
//inputPages.questionnare = path.join(baseDir, 'questionnaire/index.html')



export default defineConfig(({ mode, command }) => {
    const isDevMode = mode === 'development';
    const isProdMode = !isDevMode;

    //const base = command == 'serve' ? '/.vite/' : '/assets/';
    const base = command == 'serve' ? '/.vite/' : '/';

    if (command == 'serve' && mode != 'test') {
        sync(outDir);
        fs.mkdirSync(outDir);
    }

    return {
        base,
        optimizeDeps: {
            include: ['jquery'],
        },
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
                    log: 'all',
                    //log: 'error',
                },
            }),
            {
                name: 'CopyManifest'
            }
        ],
        css: {
            preprocessorOptions: {
                less: {
                    //additionalData: '@icon-font-path: "../../fonts/";',
                    //relativeUrls: true,
                    //rootpath: './',
                    javascriptEnabled: true,
                },
            },
        },
        resolve: {
            alias: [
                {
                    find: '@',
                    replacement: path.resolve(__dirname, './questionnare/src'),
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
            manifest: true,
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
                input: inputPages,
                output: {
                    //inlineDynamicImports: false,
                    //manualChunks: undefined,
                    //format: 'es',
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
