import { defineConfig } from 'vite';
import path from 'path';
import vue from '@vitejs/plugin-vue'
import envCompatible from 'vite-plugin-env-compatible';
import mpaPlugin from 'vite-plugin-mpa-plus'
//import { createHtmlPlugin } from 'vite-plugin-html';
import { viteCommonjs } from '@originjs/vite-plugin-commonjs';
//import cleanPlugin from 'vite-plugin-clean';
import LocalizationPlugin from './tools/vite-plugin-localization'
import inject from '@rollup/plugin-inject';
//import vitePluginRequire from "vite-plugin-require";
import { rimrafSync } from 'rimraf';
import fs from 'fs';
import { viteStaticCopy } from 'vite-plugin-static-copy';
import { ViteFilemanager } from 'filemanager-plugin';
import saveSelectedFilesPlugin from './tools/saveSelectedFilesPlugin.cjs';

const baseDir = path.relative(__dirname, "./");
const join = path.join.bind(path, baseDir);
const uiFolder = join("..");
const hqFolder = path.join(uiFolder, "WB.UI.Headquarters.Core");
const webTesterFolder = path.join(uiFolder, "WB.UI.WebTester");

const locales = {
    hq: ["Assignments", "Common", "Dashboard", "DataExport", "DataTables",
        "Details", "DevicesInterviewers", "Diagnostics", "Interviews", "MainMenu", "MapReport",
        "Pages", "Report", "Reports", "Settings", "Strings", "TabletLogs", "UploadUsers",
        "Users", "WebInterview", "WebInterviewSettings", "WebInterviewSetup", "WebInterviewUI",
        "FieldsAndValidations", "PeriodicStatusReport", "LoginToDesigner", "ImportQuestionnaire", "QuestionnaireImport",
        "QuestionnaireClonning", "Archived", "BatchUpload", "ControlPanel", "AuditLog", "OutdatedBrowser", "InterviewerAuditRecord"
        , "Roles", "Workspaces"],
    webtester: ["WebInterviewUI", "WebInterview", "Common", "Details"],
    webinterview: ["WebInterviewUI", "WebInterview", "Common", "Details"]
}

const isPack = process.argv.indexOf("--package") >= 0;

const hqDist = !isPack ? hqFolder : join("dist", "package", "hq")
const webTesterDist = !isPack ? webTesterFolder : join("dist", "package", "webtester")

const pages = {

    finishInstallation: {
        entry: "src/pages/finishInstallation.js",
        filename: path.join(hqDist, "Views", "Shared", "_FinishInstallation.cshtml"),
        template: path.join(hqFolder, "Views", "Shared", "_FinishInstallation.Template.cshtml")
    },

    logon: {
        entry: "src/pages/logon.js",
        filename: path.join(hqDist, "Views", "Shared", "_Logon.cshtml"),
        template: path.join(hqFolder, "Views", "Shared", "_Logon.Template.cshtml")
    },

    hq_vue: {
        entry: "src/hqapp/main.js",
        filename: path.join(hqDist, "Views", "Shared", "_Layout.cshtml"),
        template: path.join(hqFolder, "Views", "Shared", "_Layout.Template.cshtml"),
    },

    webinterview: {
        entry: "src/hqapp/main.js",
        filename: path.join(hqDist, "Views", "WebInterview", "_WebInterviewLayout.cshtml"),
        template: path.join(hqFolder, "Views", "WebInterview", "_WebInterviewLayout.Template.cshtml")
    },
    webinterviewRun: {
        entry: "src/webinterview/main.js",
        filename: path.join(hqDist, "Views", "WebInterview", "Index.cshtml"),
        template: path.join(hqFolder, "Views", "WebInterview", "Index.Template.cshtml")
    },

    webtester: {
        entry: "src/webinterview/main.js",
        filename: path.join(webTesterDist, "Views", "Shared", "_Layout.cshtml"),
        template: path.join(webTesterFolder, "Views", "Shared", "_Layout.Template.cshtml")
    },

    under_construction: {
        entry: "src/pages/under_construction.js",
        filename: path.join(hqDist, "Views", "UnderConstruction", "Index.cshtml"),
        template: path.join(hqFolder, "Views", "UnderConstruction", "Index.Template.cshtml")
    },

    empty_layout: {
        entry: "src/hqapp/main.js",
        //entry: path.join("src", "hqapp", "main.js"),
        filename: path.join(hqDist, "Views", "Shared", "_EmptyLayout.cshtml"),
        template: path.join(hqFolder, "Views", "Shared", "_EmptyLayout.Template.cshtml")
    }
};

const resourcesTargets = [
    { source: join(".resources", "**", "*.js"), destination: join("dist", "locale"), isFlat: false },
    { source: join("dist", "locale", "hq", "*.*"), destination: path.join(hqDist, "wwwroot", "locale", "hq") },
    { source: join("dist", "locale", "webinterview", "*.*"), destination: path.join(hqDist, "wwwroot", "locale", "webinterview") },
    { source: join("dist", "locale", "webtester", "*.*"), destination: path.join(webTesterDist, "wwwroot", "locale", "webtester") },
]

const fileTargets = [
    //{ source: join(".resources", "**", "*.js"), destination: join("dist", "locale"), isFlat: false },

    { source: join("dist", "img", "**", "*.*"), destination: path.join(hqDist, "wwwroot", "img"), isFlat: false },
    { source: join("dist", "fonts", "**", "*.*"), destination: path.join(hqDist, "wwwroot", "fonts") },
    { source: join("dist", "css", "*.*"), destination: path.join(hqDist, "wwwroot", "css") },
    { source: join("dist", "js", "*.*"), destination: path.join(hqDist, "wwwroot", "js") },
    //{ source: join("dist", "locale", "hq", "*.*"), destination: path.join(hqDist, "wwwroot", "locale", "hq") },
    //{ source: join("dist", "locale", "webinterview", "*.*"), destination: path.join(hqDist, "wwwroot", "locale", "webinterview") },

    { source: join("dist", "img", "**", "*.*"), destination: path.join(webTesterDist, "wwwroot", "img"), isFlat: false },
    { source: join("dist", "fonts", "*.*"), destination: path.join(webTesterDist, "wwwroot", "fonts") },
    { source: join("dist", "css", "*.*"), destination: path.join(webTesterDist, "wwwroot", "css") },
    { source: join("dist", "js", "*.*"), destination: path.join(webTesterDist, "wwwroot", "js") },
    //{ source: join("dist", "locale", "webtester", "*.*"), destination: path.join(webTesterDist, "wwwroot", "locale", "webtester") },

    //{ source: join("dist", ".vite"), destination: path.join(hqDist, "wwwroot") },
]


const resxFiles = [
    "../WB.UI.Headquarters.Core/**/*.resx",
    "../../Core/SharedKernels/Enumerator/WB.Enumerator.Native/Resources/*.resx",
    "../../Core/BoundedContexts/Headquarters/WB.Core.BoundedContexts.Headquarters/Resources/*.resx"
]

let inputPages = {};

var pagesSources = [];
var pagesTargets = [];

for (var attr in pages) {
    const pageObj = pages[attr]
    const filename = path.basename(pageObj.filename)
    const filenameHtml = attr + '.html'
    const origFolder = path.dirname(pageObj.filename)
    //const templateFilename = path.basename(pageObj.template)
    const templateFilenameHtml = attr + '.html'
    var templatesFolderFull = path.join(baseDir, ".templates")

    var destFileFolderFull = path.join(baseDir, "dist", ".templates")
    var templateHtmlPath = path.join(templatesFolderFull, templateFilenameHtml)
    var filenameHtmlPath = path.join(destFileFolderFull, filenameHtml)
    var distFileName = path.join(baseDir, "dist", filenameHtml)
    var filenameHtmlPathVite = path.join(destFileFolderFull, ".vite", filenameHtml)
    var distFileNameVite = path.join(baseDir, "dist", ".vite", filenameHtml)

    //console.log("templateHtmlPath: " + templateHtmlPath, " distFileName: " + distFileName)
    //console.log("filenameHtmlPath: " + filenameHtmlPath)

    pagesSources.push({ source: pageObj.template, destination: templatesFolderFull, name: templateFilenameHtml })
    pagesTargets.push({ source: filenameHtmlPath, destination: origFolder, name: filename })
    pagesTargets.push({ source: distFileName, destination: origFolder, name: filename })
    pagesTargets.push({ source: filenameHtmlPathVite, destination: origFolder, name: filename })
    pagesTargets.push({ source: distFileNameVite, destination: origFolder, name: filename })

    pageObj.filename = filenameHtml
    pageObj.template = templateHtmlPath

    inputPages[attr] = templateHtmlPath;
    //pageObj.entry = join(pageObj.entry)
    //inputPages[attr] = join(pageObj.entry);
}

//const allTargets = pagesTargets.concat(fileTargets).map(i => { return { src: i.source, dest: i.destination } })
const allTargets = fileTargets.map(i => { return { src: i.source, dest: path.resolve(__dirname, i.destination) } })

const clearBeforeBuild = fileTargets.map(i => path.resolve(__dirname, i.destination))
//const clearBeforeBuild = []
//clearBeforeBuild.concat(fileTargets.map(i => path.resolve(__dirname, i.destination)))
//clearBeforeBuild.concat(resourcesTargets.map(i => path.resolve(__dirname, i.destination)))
clearBeforeBuild.push('./dist')

// https://vitejs.dev/config/
export default defineConfig(({ mode, command }) => {

    const isDevMode = mode === 'development';
    const isProdMode = !isDevMode
    const isServe = command === 'serve'

    const base = command == 'serve' ? '/.vite/' : '/';
    //const base = '/.vite/';

    //const outDir = path.join(hqDist, "wwwroot");
    //const outDir = path.join(hqDist, "dist");
    //const outDir = join("dist");
    const outDir = "dist";
    //const outDir = path.join(hqDist, "wwwroot");

    if (isServe && mode != 'test') {
        rimrafSync(outDir);
        fs.mkdirSync(outDir);
    }

    return {
        base,
        css: {
            devSourcemap: isDevMode,
        },
        resolve: {
            alias: [
                {
                    find: '@',
                    replacement: path.resolve(__dirname, 'src')
                },
                {
                    find: 'moment$',
                    replacement: path.resolve(__dirname, 'moment/moment.js')
                },
                {
                    find: '~',
                    replacement: path.resolve(__dirname, 'src')
                },
                {
                    find: 'vue',
                    replacement: 'vue/dist/vue.esm-bundler.js',
                },
                /*{
                    find: 'jquery',
                    replacement: 'jquery/dist/jquery.min.js',
                },
                {
                    find: 'jquery-ui',
                    replacement: 'jquery-ui-dist/jquery-ui.js',
                },*/
            ],
            extensions: [
                '.mjs',
                '.js',
                '.ts',
                '.jsx',
                '.tsx',
                '.json',
                '.vue'
            ]
        },
        transpile: [
            'autonumeric',
            'vue-page-title',
            '@googlemaps/markerclusterer'
        ],
        optimizeDeps: {
            include: ['jquery'],
        },
        plugins: [
            vue(
                {
                    jsx: true,
                    template: {
                        compilerOptions: {
                            compatConfig: {
                                MODE: 2
                            }
                        }
                    }
                }),
            //vitePluginRequire.default(),
            //viteCommonjs(),
            //envCompatible(),
            //cleanPlugin({
            //    targetFiles: fileTargets.map(target => target.destination)
            //}),
            saveSelectedFilesPlugin({
                filesToSave: pagesTargets
            }),
            /*viteStaticCopy({
                targets: allTargets
            }),*/
            ViteFilemanager({
                customHooks: [
                    {
                        hookName: 'options',
                        commands: {
                            del: {
                                items: clearBeforeBuild
                            },
                            copy: { items: pagesSources.concat(resourcesTargets) },
                        }
                    },
                    /*{
                        hookName: 'buildEnd',
                        commands: {
                            copy: { items: resourcesTargets },
                        }
                    },*/
                    /*{
                        hookName: 'writeBundle',
                        commands: {
                            copy: { items: resourcesTargets },
                        }
                    },*/
                    {
                        hookName: 'closeBundle',
                        commands: {
                            copy: { items: isServe ? [] : pagesTargets.concat(fileTargets) },
                        }
                    },
                    /*{
                        hookName: 'handleHotUpdate',
                        commands: {
                            copy: { items: pagesTargets.concat(fileTargets) },
                        }
                    }*/
                ],

                options: {
                    parallel: 1,
                    log: 'all'
                    //log: 'error'
                }
            }),
            LocalizationPlugin({
                patterns: resxFiles,
                destination: "./.resources",
                locales: locales,
                noHash: isDevMode
            }),
            /*mpaPlugin({
                pages: pages
            }),*/
            /*createHtmlPlugin({
                minify: false,
                pages: pagesArray
            })*/
            //eslintPlugin()
        ],
        /*define: {
            global: {},
        },*/
        server: {
            host: 'localhost',
            strictPort: true,
            base: base,
            port: 3001,
            watch: {
                paths: ['src/**/*'],
                ignored: ['**/.resources/**']
            },
            fs: {
                allow: ['..'],
            },
        },
        build: {
            minify: isProdMode,
            outDir,
            manifest: isDevMode,
            rollupOptions: {
                input: inputPages,
                maxParallelFileOps: 1,
                cache: false,
                plugins: [
                    inject({
                        jQuery: "jquery",
                        //"window.jQuery": "jquery",
                        //"window.$": "jquery",
                        $: "jquery"
                    })
                ],
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
                },
            },
        },
    }
})
