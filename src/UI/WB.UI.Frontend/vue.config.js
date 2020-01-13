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

const locales = {
    hq: ["Assignments", "Common", "Dashboard", "DataExport", "DataTables",
        "Details", "DevicesInterviewers", "Interviews", "MainMenu", "MapReport",
        "Pages", "Report", "Reports", "Settings", "Strings", "TabletLogs", "UploadUsers",
        "Users", "WebInterview", "WebInterviewSettings", "WebInterviewSetup", "WebInterviewUI",
        "FieldsAndValidations", "PeriodicStatusReport", "LoginToDesigner", "ImportQuestionnaire", "QuestionnaireImport"],
    webtester: ["WebInterviewUI", "WebInterview", "Common"],
    webinterview: ["WebInterviewUI", "WebInterview", "Common", "Details"]
}

const isPack = process.argv.indexOf("--package") >= 0;

const hqDist = !isPack ? hqFolder :  join("dist", "package", "hq")
const webTesterDist = !isPack ? webTesterFolder : join("dist", "package", "webtester")

const pages = {

    finishInstallation: {
        entry: "src/pages/finishInstallation.js",
        filename: path.join(hqDist, "Views", "Shared", "_FinishInstallation.cshtml"),
        template: path.join(hqFolder, "Views", "Shared", "_FinishInstallation.Template.cshtml")
    },

    hq_legacy: {
        entry: "src/pages/hq_legacy.js",
        filename: path.join(hqDist, "Views", "Shared", "_AdminLayout_Legacy.cshtml"),
        template: path.join(hqFolder, "Views", "Shared", "_AdminLayout_Legacy.Template.cshtml")
    },

    logon: {
        entry: "src/pages/logon.js",
        filename: path.join(hqDist, "Views", "Shared", "_Logon.cshtml"),
        template: path.join(hqFolder, "Views", "Shared", "_Logon.Template.cshtml")
    },

    hq_vue: {
        entry: "src/hqapp/main.js",
        filename: path.join(hqDist, "Views", "Shared", "_AdminLayout.cshtml"),
        template: path.join(hqFolder, "Views", "Shared", "_AdminLayout.Template.cshtml")
    },

    webinterview: {
        entry: "src/webinterview/main.js",
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
    { source: join("dist", "locale", "webtester", "*.*"), destination: path.join(webTesterDist, "wwwroot", "js", "locale") },

]

module.exports = {
    pages,

    transpileDependencies: [
        'autonumeric'
    ],

    chainWebpack: config => {

        config.devtool("source-map")

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
        

        config.plugin('extraWatch')
            .use(extraWatch, [{ files: resxFiles }])

        config.plugin("localization")
            .use(LocalizationPlugin, [{
                patterns: resxFiles,
                destination: "./.resources",
                locales
            }])

        config.plugin('stats')
            .use(StatsPlugin, ['stats.json',
                {
                    chunks: true,
                    assets: false,
                    chunkModules: false,
                    modules: false,
                    children: false
                }]);

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
            $: "jquery",
            jQuery: "jquery",
        }]);

        config.module.rules.delete("eslint");

        config.resolve.alias
            .set("moment$", "moment/moment.js")
            .set("~", join("src"));

        Object.keys(pages).forEach(page => {
            config.plugin("html-" + page).tap(args => {
                args[0].minify = false
                return args;
            });
        });
    }
};
