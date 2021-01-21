const path = require("path");
const baseDir = path.resolve(__dirname, "./");
const join = path.join.bind(path, baseDir);
const webpack = require("webpack");
const FileManagerPlugin = require("filemanager-webpack-plugin");
const LocalizationPlugin = require("./tools/LocalizationPlugin")
const extraWatch = require("extra-watch-webpack-plugin")

const uiFolder = join("..");
const hqFolder = path.join(uiFolder, "WB.UI.Headquarters.Core");
const webTesterFolder = path.join(uiFolder, "WB.UI.WebTester");

const StatsPlugin = require('stats-webpack-plugin')
const CleanupPlugin = require('clean-webpack-plugin').CleanWebpackPlugin;
const WebpackBuildNotifierPlugin = require('webpack-build-notifier');
const LiveReloadPlugin = require('webpack-livereload-plugin');

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
        template: path.join(hqFolder, "Views", "Shared", "_Layout.Template.cshtml")
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
    { source: join(".resources", "locale", "**", "*.json"), destination: join("dist", "locale") },

    { source: join("dist", "img", "**", "*.*"), destination: path.join(hqDist, "wwwroot", "img") },
    { source: join("dist", "fonts", "**", "*.*"), destination: path.join(hqDist, "wwwroot", "fonts") },
    { source: join("dist", "css", "*.*"), destination: path.join(hqDist, "wwwroot", "css") },
    { source: join("dist", "js", "*.*"), destination: path.join(hqDist, "wwwroot", "js") },
    { source: join("dist", "locale", "hq", "*.*"), destination: path.join(hqDist, "wwwroot", "locale", "hq") },
    { source: join("dist", "locale", "webinterview", "*.*"), destination: path.join(hqDist, "wwwroot", "locale", "webinterview") },

    { source: join("dist", "img", "**", "*.*"), destination: path.join(webTesterDist, "wwwroot", "img") },
    { source: join("dist", "fonts", "*.*"), destination: path.join(webTesterDist, "wwwroot", "fonts") },
    { source: join("dist", "css", "*.*"), destination: path.join(webTesterDist, "wwwroot", "css") },
    { source: join("dist", "js", "*.*"), destination: path.join(webTesterDist, "wwwroot", "js") },
    { source: join("dist", "locale", "webtester", "*.*"), destination: path.join(webTesterDist, "wwwroot", "locale", "webtester") },

]

module.exports = {
    pages,

    transpileDependencies: [
        'autonumeric',
        'vue-page-title',
        '@google/markerclustererplus'
    ],

    chainWebpack: config => {

        config.devtool("source-map")

        Object.keys(pages).forEach(name => {
            config.plugins.delete('prefetch-' + name)
        });

        config.plugin("fileManager").use(FileManagerPlugin, [{
            // verbose: true,
            onEnd: { copy: fileTargets }
        }]);

        config.plugin('cleanup-dists').use(CleanupPlugin, [{
            dangerouslyAllowCleanPatternsOutsideProject: true,
            dry: false,
            cleanOnceBeforeBuildPatterns: fileTargets.map(target => target.destination)
        }]);

        const resxFiles = [
            path.join(uiFolder, "WB.UI.Headquarters.Core/**/*.resx"),
            path.join(uiFolder, "../Core/SharedKernels/Enumerator/WB.Enumerator.Native/Resources/*.resx"),
            path.join(uiFolder, "../Core/BoundedContexts/Headquarters/WB.Core.BoundedContexts.Headquarters/Resources/*.resx")
        ]

        Object.keys(pages).forEach(page => {
            resxFiles.push(path.join(uiFolder, pages[page].template))
        })

        config.plugin('extraWatch')
            .use(extraWatch, [{ files: resxFiles }])

        config.plugin("localization")
            .use(LocalizationPlugin, [{
                patterns: resxFiles,
                destination: "./.resources",
                locales
            }])


        config.plugin("livereload")
            .use(LiveReloadPlugin, [{ appendScriptTag: true, delay: 1000 }])

        config.merge({
            optimization: {
                splitChunks: {
                    cacheGroups: {
                        vendors: {
                            name: 'chunk-vendors',
                            test: /[\\\/]node_modules[\\\/]((?!(moment|vee-validate|autonumeric|bootstrap-select)).*)[\\/]/,
                            priority: -10,
                            chunks: 'initial'
                        }
                    }
                }
            }
        });

        config.plugin("provide").use(webpack.ProvidePlugin, [{
            $: 'jquery',
            jquery: 'jquery',
            'window.jQuery': 'jquery',
            jQuery: 'jquery'
        }]);

        // config.module.rules.delete("eslint");

        config.resolve.alias
            .set("moment$", "moment/moment.js")
            .set("~", join("src"));

        Object.keys(pages).forEach(page => {
            config.plugin("html-" + page).tap(args => {
                args[0].minify = false
                return args;
            });
        });
    },

    pluginOptions: {
        // Apollo-related options
        apollo: {
            // // Enable automatic mocking
            // enableMocks: true,
            // // Enable Apollo Engine
            // enableEngine: false,
            // Enable ESLint for `.gql` files
            lintGQL: true,

            /* Other options (with default values) */
        },
    },
};
