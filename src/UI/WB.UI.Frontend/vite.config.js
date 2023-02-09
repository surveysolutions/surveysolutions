import { defineConfig } from 'vite';
import path from 'path';
import vue from '@vitejs/plugin-vue2'
import envCompatible from 'vite-plugin-env-compatible';
//import { createHtmlPlugin } from 'vite-plugin-html';
//import { vitePluginHtmlTpl, InjectOptions } from 'vite-plugin-html-tpl'
import mpaPlugin from 'vite-plugin-mpa-plus'
import { viteCommonjs } from '@originjs/vite-plugin-commonjs';
import cleanPlugin from 'vite-plugin-clean';
import LocalizationPlugin  from './tools/vite-plugin-localization'
import cshtmlPlugin from './tools/vite-plugin-cshtml2'
import { globalExternals } from "@fal-works/esbuild-plugin-global-externals";

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


console.log(path.join(hqFolder, "Views", "Shared", "_FinishInstallation.Template.cshtml"))
console.log(path.join(baseDir, "src/hqapp/main.js"))


const pages2 = [

    /*{
        entry: "src/pages/finishInstallation.js",
        filename: path.join(hqDist, "Views", "Shared", "_FinishInstallation.cshtml"),
        template: path.join(hqFolder, "Views", "Shared", "_FinishInstallation.Template.cshtml"),
		injectOptions: {
            data: {
              title: 'index',
              injectScript: `<script src="./inject.js"></script>`,
            },
            tags: [
              {
                injectTo: 'body-prepend',
                tag: 'div',
                attrs: {
                  id: 'tag1',
                },
              },
            ],
          },
    },*/
    {
        entry: "src/hqapp/main.js",
        //entry: path.join(baseDir, "src/hqapp/main.js"),
        //filename: path.join(hqDist, "Views", "Shared", "_Layout.cshtml"),
        filename: 'dist/app-offer.html',
        //filename: "TestLayout.html",
        //template: path.join(hqFolder, "Views", "Shared", "_Layout.Template.cshtml")
        template: path.join(hqFolder, "Views", "Shared", "_Layout.Template.cshtml")
        //template: "./"
    },

    /*{
        entry: "src/pages/logon.js",
        filename: path.join(hqDist, "Views", "Shared", "_Logon.cshtml"),
        template: path.join(hqFolder, "Views", "Shared", "_Logon.Template.cshtml")
    },

    {
        entry: "src/hqapp/main.js",
        filename: path.join(hqDist, "Views", "WebInterview", "_WebInterviewLayout.cshtml"),
        template: path.join(hqFolder, "Views", "WebInterview", "_WebInterviewLayout.Template.cshtml")
    },
    {
        entry: "src/webinterview/main.js",
        filename: path.join(hqDist, "Views", "WebInterview", "Index.cshtml"),
        template: path.join(hqFolder, "Views", "WebInterview", "Index.Template.cshtml")
    },

    {
        entry: "src/webinterview/main.js",
        filename: path.join(webTesterDist, "Views", "Shared", "_Layout.cshtml"),
        template: path.join(webTesterFolder, "Views", "Shared", "_Layout.Template.cshtml")
    },

    {
        entry: "src/pages/under_construction.js",
        filename: path.join(hqDist, "Views", "UnderConstruction", "Index.cshtml"),
        template: path.join(hqFolder, "Views", "UnderConstruction", "Index.Template.cshtml")
    },

    {
        entry: "src/hqapp/main.js",
        filename: path.join(hqDist, "Views", "Shared", "_EmptyLayout.cshtml"),
        template: path.join(hqFolder, "Views", "Shared", "_EmptyLayout.Template.cshtml")
    }*/
];
//const pagesRenames = pages.map( page => { return {source: page.template, destination: page.template.replace('.cshtml', '.html') }})
//pages.forEach(page => { page.template = page.template.replace('.cshtml', '.html') })


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
  optimizeDeps: {
    include: ["jquery"],
  },
  plugins: [
    vue({ jsx: true }),
    viteCommonjs(),
    envCompatible(),
    //cshtmlPlugin(),
	cleanPlugin({
      targetFiles: fileTargets.map(target => target.destination)
    }),
    ViteFilemanager({
      events: {
        //start: { copy: { items: pagesSources }},
        //end: { copy: { items: pagesTargets.concat(fileTargets) }}
        //end:   { copy: { items: fileTargets }}
        //end: { copy: { items: pagesTargets }}
      },
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
            //copy: { items: fileTargets }
            copy: { items: pagesTargets.concat(fileTargets) },
            //copy: { items: pagesTargets },
			//del: { items: deleteTemplates },
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
      //noHash: true,
      //inline: true,
      /*patterns: resxFiles,
      destination: './src/locale',
      locales: {
          '.': ['QuestionnaireEditor']
      }*/
      patterns: resxFiles,
      destination: "./.resources",
      locales: locales
    }),
    /*createHtmlPlugin({
      minify: false,
        entry: "src/hqapp/main.js",
        filename: 'dist/app-offer.html',
        //filename: "TestLayout.html",
        //template: path.join(hqFolder, "Views", "Shared", "_Layout.Template.cshtml")
        template: path.join(hqFolder, "Views", "Shared", "_Layout.Template.html"),

      //pages: pages,
	  injectOptions: {
            data: {
              title: 'index',
              injectScript: `<script src="./inject.js"></script>`,
            },
            tags: [
              {
                injectTo: 'body-prepend',
                tag: 'div',
                attrs: {
                  id: 'tag1',
                },
              },
            ],
          },
    })*/
	mpaPlugin({
		pages: pages
	})
  ],
  build: {
    rollupOptions: {
      external: ['jquery'],
      output: {
		  globals: {
			$: 'jquery',
            jquery: 'jquery',
            'window.jQuery': 'jquery',
            jQuery: 'jquery',
               //jquery: 'window.jQuery',
               //jquery: 'window.$'
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
		  /*chunkFileNames: (assetInfo) => {
			  console.log("chunkFileNames " + assetInfo.name);
			  return 'assets/js/[hash].js';
		  },
		  entryFileNames: (assetInfo) => {
			  console.log("entryFileNames " + assetInfo.name);
			  return 'assets/js/[hash].js';
		  },*/
        /*assetFileNames: (assetInfo) => {
		  console.log(assetInfo.name);
          let extType = assetInfo.name.split('.').at(1);
          if (/png|jpe?g|svg|gif|tiff|bmp|ico/i.test(extType)) {
            extType = 'img';
          }
          return `assets/${extType}/[name]-[hash][extname]`;
        },
        chunkFileNames: 'assets/js/[name]-[hash].js',
        entryFileNames: 'assets/js/[hash].js',*/
      },
    },
  },
})
