import { defineConfig } from 'vite';
import path from 'path';
import vue from '@vitejs/plugin-vue2'
import envCompatible from 'vite-plugin-env-compatible';
import mpaPlugin from 'vite-plugin-mpa-plus'
import { viteCommonjs } from '@originjs/vite-plugin-commonjs';
import cleanPlugin from 'vite-plugin-clean';
import LocalizationPlugin  from './tools/vite-plugin-localization'
import inject from '@rollup/plugin-inject';
import vitePluginRequire from "vite-plugin-require";

const ViteFilemanager = require('filemanager-plugin').ViteFilemanager;


const baseDir = path.relative(__dirname, "./");
//const baseDir = path.resolve(__dirname, "./");
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
console.log("isPackage:  " + isPack)

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
        filename: path.join(hqDist, "Views", "Shared", "_EmptyLayout.cshtml"),
        template: path.join(hqFolder, "Views", "Shared", "_EmptyLayout.Template.cshtml")
    }
};

const fileTargets = [
    { source: join(".resources", "**", "*.json"), destination: join("dist", "locale"), isFlat: false  },

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
    { source: join("dist", "locale", "webtester", "*.*"), destination: path.join(webTesterDist, "wwwroot", "locale", "webtester") },
]


const resxFiles = [
    "../WB.UI.Headquarters.Core/**/*.resx",
    "../../Core/SharedKernels/Enumerator/WB.Enumerator.Native/Resources/*.resx",
    "../../Core/BoundedContexts/Headquarters/WB.Core.BoundedContexts.Headquarters/Resources/*.resx"     
]

var pagesSources = [];
var pagesTargets = [];

for (var attr in pages) {
  const pageObj = pages[attr]
  const filename = path.basename(pageObj.filename)
  const filenameHtml = attr + '.html'
  const origFolder = path.dirname(pageObj.filename)
  const templateFilename = path.basename(pageObj.template)
  const templateFilenameHtml = attr + '.html'
  var templatesFolderFull = path.join(baseDir, ".templates")
  var destFileFolderFull = path.join(baseDir, "dist", ".templates")
  var templateHtmlPath = path.join(templatesFolderFull, templateFilenameHtml)
  var filenameHtmlPath = path.join(destFileFolderFull, filenameHtml)
  var filenamePath = path.join(destFileFolderFull, filename)
  pagesSources.push({ source: pageObj.template, destination: templatesFolderFull, name : templateFilenameHtml })
  pagesTargets.push({ source: filenameHtmlPath, destination: origFolder, name: filename })
  pagesTargets.push({ source: path.join(baseDir, "dist", filenameHtml), destination: origFolder, name: filename })

  pageObj.filename = filenameHtml
  pageObj.template = templateHtmlPath
}

// https://vitejs.dev/config/
export default defineConfig({
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
      }
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
    vue({ jsx: true }),
    vitePluginRequire(),
    viteCommonjs(),
    envCompatible(),
    cleanPlugin({
      targetFiles: fileTargets.map(target => target.destination)
    }),
    ViteFilemanager({
      customHooks: [
        {
          hookName: 'options',
          commands: {
			del: {
               items: ['./dist']
            },
            copy: { items: pagesSources },
          }
        },
        {
          hookName: 'closeBundle',
          commands: {
            copy: { items: pagesTargets.concat(fileTargets) },
          }
        }
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
      locales: locales
    }),
	mpaPlugin({
		pages: pages
	})
  ],
  build: {
	//minify: false,
    rollupOptions: {
		plugins: [
			inject({
				$: 'jquery',
				jQuery: 'jquery',
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
			  return `${extType}/[name]-[hash][extname]`;
		  },
		  chunkFileNames: 'js/[name]-[hash].js',
          entryFileNames: 'js/[name]-[hash].js'
      },
    },
  },
})
