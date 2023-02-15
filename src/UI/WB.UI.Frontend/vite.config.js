import { defineConfig } from 'vite';
import path from 'path';
import vue from '@vitejs/plugin-vue2'
import envCompatible from 'vite-plugin-env-compatible';
import mpaPlugin from 'vite-plugin-mpa-plus'
import { viteCommonjs } from '@originjs/vite-plugin-commonjs';
import commonjs from '@rollup/plugin-commonjs';
import cleanPlugin from 'vite-plugin-clean';
import LocalizationPlugin  from './tools/vite-plugin-localization'
import { globalExternals } from "@fal-works/esbuild-plugin-global-externals";
import inject from '@rollup/plugin-inject';
//import Components from 'unplugin-vue-components/vite'
//import 'jquery';
import legacy from '@vitejs/plugin-legacy'
import vitePluginRequire from "vite-plugin-require";
import VueI18nPlugin from '@intlify/unplugin-vue-i18n/vite'

const ViteFilemanager = require('filemanager-plugin').ViteFilemanager;


const baseDir = path.relative(__dirname, "./");
//const baseDir = path.resolve(__dirname, "./");
console.log("baseDir:  " + baseDir)
const join = path.join.bind(path, baseDir);
const uiFolder = join("..");
console.log("uiFolder: " + uiFolder)
const hqFolder = path.join(uiFolder, "WB.UI.Headquarters.Core");
console.log("hqFolder: " + hqFolder)
const webTesterFolder = path.join(uiFolder, "WB.UI.WebTester");
console.log("webTesterFolder: " + webTesterFolder)

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
        filename: path.join(hqDist, "Views", "Shared", "_EmptyLayout.cshtml"),
        template: path.join(hqFolder, "Views", "Shared", "_EmptyLayout.Template.cshtml")
    }
};

var pagesSources = [];
var pagesTargets = [];
var deleteTemplates = [];
for (var attr in pages) {
  const pageObj = pages[attr]
  var filename = attr + ".html"
  var template = attr + "_s.html"
  var filenamePath = path.join(baseDir, "dist", ".templates", template)
  var templatePath = path.join(baseDir, ".templates", template)
  pagesSources.push({ source: pageObj.template, destination: templatePath, iFileCopy: true })
  pagesTargets.push({ source: filenamePath, destination: pageObj.filename, iFileCopy: true })
  deleteTemplates.push(templatePath)
  pageObj.filename = filename
  pageObj.template = templatePath
}


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
    path.join(uiFolder, "WB.UI.Headquarters.Core/**/*.resx"),
    path.join(uiFolder, "../Core/SharedKernels/Enumerator/WB.Enumerator.Native/Resources/*.resx"),
    path.join(uiFolder, "../Core/BoundedContexts/Headquarters/WB.Core.BoundedContexts.Headquarters/Resources/*.resx")
]

Object.keys(pages).forEach(page => {
    resxFiles.push(path.join(uiFolder, pages[page].template))
})


// https://vitejs.dev/config/
export default defineConfig({
  resolve: {
    alias: [
      /*{
        find: /^~/,
        replacement: ''
      },*/
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
        '@google/markerclustererplus'
  ],
  define: {
	  //global: 'window'
	  //global: {
		//$: require("jquery"),
		//jquery: require("jquery"),
		//jQuery: require("jquery"),
	  //},
		/*$: require("jquery"),
		jquery: require("jquery"),
		'window.jQuery': require("jquery"),
		jQuery: require("jquery"),*/
  },
  /*externals: {
    //jquery: 'jQuery'
    $: 'jQuery',
    jquery: 'jQuery',
    //'window.jQuery': 'jQuery',
    jQuery: 'jQuery',
  },*/
  optimizeDeps: {
	  //disabled: false,
    include: ['jquery'],
  },
  plugins: [
	/*inject({
            $: 'jquery',
            //jquery: 'jquery',
            //'window.jQuery': 'jquery',
            jQuery: 'jquery',
        }),*/
    vue({ jsx: true }),
	/*legacy({
      targets: ['defaults', 'not IE 11'],
    }),*/
	VueI18nPlugin({ /* options */ }),
	vitePluginRequire(),
    viteCommonjs(),
    //commonjs(),
    envCompatible(),
	cleanPlugin({
      targetFiles: fileTargets.map(target => target.destination)
    }),
    ViteFilemanager({
	  customHooks: [
	    {
          hookName: 'buildStart',
          commands: {
            copy: { items: pagesSources }
          }
        },
        {
          hookName: 'writeBundle',
          commands: {
            copy: { items: pagesTargets.concat(fileTargets) },
			//del: { items: deleteTemplates },
          }
        }
      ],
    
      options: {
        parallel: 1,
        //log: 'all'
        log: 'error'
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
	minify: false,
    rollupOptions: {
		plugins: [
			inject({   // => that should be first under plugins array
				$: 'jquery',
				jQuery: 'jquery',
			})
		],
      //external: ['jquery'],
      output: {
		  //strict: false,
		  globals: {
			//$: 'jquery',
            //jquery: 'jquery',
            //'window.jQuery': 'jquery',
            //jQuery: 'jquery',
              // jquery: 'window.jQuery',
              // jquery: 'window.jquery',
              // jquery: 'window.$'
          },
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
