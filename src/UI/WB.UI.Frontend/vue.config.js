const path = require("path");
const baseDir = path.resolve(__dirname, "./");
const join = path.join.bind(path, baseDir);
const webpack = require("webpack");
const WriteFilePlugin = require("write-file-webpack-plugin");
const RuntimePublicPathPlugin = require("./tools/RuntimePublicPathPlugin");
const FileManagerPlugin = require("filemanager-webpack-plugin");
const LocalizationPlugin = require("./tools/LocalizationPlugin")

const isHot = process.env.HOT_MODE == 1;
const assetsPath = isHot ? "http://localhost:8080/" : "~/dist/";

const uiFolder = join("..");
const hqFolder = path.join(uiFolder, "WB.UI.Headquarters.Core");
const webTesterFolder = path.join(uiFolder, "WB.UI.WebTester");

const StatsPlugin = require('stats-webpack-plugin')
const CleanupPlugin = require('clean-webpack-plugin').CleanWebpackPlugin;


const locales = {
    hq: ["Assignments", "Common", "Dashboard", "DataExport", "DataTables",
        "Details", "DevicesInterviewers", "Interviews", "MainMenu", "MapReport",
        "Pages", "Report", "Settings", "Strings", "TabletLogs", "UploadUsers",
        "Users", "WebInterview", "WebInterviewSettings", "WebInterviewSetup", "WebInterviewUI"],
    webtester: ["WebInterviewUI", "WebInterview", "Common"],
    webinterview: ["WebInterviewUI", "WebInterview", "Common"]
}

const pages = {

    finishInstallation: {
        entry: "src/pages/finishInstallation.js",
        filename: path.join(hqFolder, "Views", "Shared", "_FinishInstallation.cshtml"),
        template: path.join(hqFolder, "Views", "Shared", "_FinishInstallation.Template.cshtml"),
        locales: null
    },

    hq_legacy: {
        entry: "src/pages/hq_legacy.js",
        filename: path.join(hqFolder, "Views", "Shared", "_AdminLayout_Legacy.cshtml"),
        template: path.join(hqFolder, "Views", "Shared", "_AdminLayout_Legacy.Template.cshtml"),
        locales: null
    },

    logon: {
        entry: "src/pages/logon.js",
        filename: path.join(hqFolder, "Views", "Shared", "_Logon.cshtml"),
        template: path.join(hqFolder, "Views", "Shared", "_Logon.Template.cshtml"),
        locales: null
    },

    hq_vue: {
        entry: "src/hqapp/main.js",
        filename: path.join(hqFolder, "Views", "Shared", "_AdminLayout.cshtml"),
        template: path.join(hqFolder, "Views", "Shared", "_AdminLayout.Template.cshtml"),
        locales: "hq"
    },

    webinterview: {
        entry: "src/webinterview/main.js",
        filename: path.join(hqFolder, "Views", "Shared", "_WebInterview.cshtml"),
        template: path.join(hqFolder, "Views", "Shared", "_WebInterview.Template.cshtml"),
        locales: locales.webinterview
    },

    webtester: {
        entry: "src/webinterview/main.js",
        filename: path.join(webTesterFolder, "Views", "Shared", "_Layout.cshtml"),
        template: path.join(webTesterFolder, "Views", "Shared", "_Layout.Template.cshtml"),
        locales: "webtester"
    }
};

const fileTargets = [
    { source: join("locale", ".resources", "**", "*.json"), destination: join("dist") },

    { source: join("dist", "img", "**" , "*.*"), destination: path.join(hqFolder, "wwwroot", "img") },
    { source: join("dist", "fonts", "**", "*.*"), destination: path.join(hqFolder, "wwwroot", "fonts") },
    { source: join("dist", "css", "*.*"), destination: path.join(hqFolder, "wwwroot", "css") },
    { source: join("dist", "js", "*.*"), destination: path.join(hqFolder, "wwwroot", "js") },
    { source: join("dist", "locale", "hq", "*.*"), destination: path.join(hqFolder, "wwwroot", "locale", "hq") },
    { source: join("dist", "locale", "webinterview", "*.*"), destination: path.join(hqFolder, "wwwroot", "locale", "webinterview") },

    { source: join("dist", "img", "**", "*.*"), destination: path.join(webTesterFolder, "wwwroot", "img") },
    { source: join("dist", "fonts", "*.*"), destination: path.join(webTesterFolder, "wwwroot", "fonts") },
    { source: join("dist", "css", "*.*"), destination: path.join(webTesterFolder, "wwwroot", "css") },
    { source: join("dist", "js", "*.*"), destination: path.join(webTesterFolder, "wwwroot", "js") },
]

module.exports = {
    pages,

    devServer: {
        contentBase: join("dist"),
        publicPath: "/headquarters/hqapp/dist/",
        hot: true,
        compress: false,
        headers: { "Access-Control-Allow-Origin": "*" },
        host: "localhost",
        port: 8080,
        quiet: true
    },

    transpileDependencies: [
        'autonumeric'
    ],

    chainWebpack: config => {
        config.plugin("fileManager").use(FileManagerPlugin, [{
            //verbose: true,
            onEnd: { copy: fileTargets }
        }]);

        config.plugin('cleanup-dists').use(CleanupPlugin, [{
            //  verbose: true,
            dangerouslyAllowCleanPatternsOutsideProject: true,
            dry: false,
            cleanOnceBeforeBuildPatterns: fileTargets.map(target => target.destination)
        }]);

        config.plugin("localization")
            .use(LocalizationPlugin, [{
                patterns: [
                    path.join(hqFolder, "**/*.resx"),
                    path.join(uiFolder, "Headquarters/WB.UI.Headquarters/**/*.resx"),
                    path.join(uiFolder, "../Core/SharedKernels/Enumerator/WB.Enumerator.Native/Resources/*.resx"),
                    path.join(uiFolder, "../Core/BoundedContexts/Headquarters/WB.Core.BoundedContexts.Headquarters/Resources/*.resx")
                ],
                destination: "./locale/.resources"
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
        // splitChunks: {
        //     cacheGroups: {
        //         commons: {
        //             name: 'chunk-common',
        //             test: /[\\\/]node_modules[\\\/](autonumeric|moment)/,
        //             priority: 1,
        //             chunks: 'all'
        //         }
        //     }
        // }
        //}
        //});

         config.plugin("provide").use(webpack.ProvidePlugin, [{
        //     _: "lodash",
             $: "jquery",
             jQuery: "jquery",
        //     moment: "moment"
         }]);


        config.plugin("runtime").use(RuntimePublicPathPlugin, [{
            runtimePublicPath: "window.CONFIG == null ? '/' : window.CONFIG.assetsPath || '/'"
        }]);


        if (isHot) {
            config.plugin("writefile").use(WriteFilePlugin, [
                {
                    test: /\.cshtml$/,
                    useHashIndex: true,
                    force: true,
                    log: true
                }
            ]);

            config.devServer
                .clientLogLevel("info")
                .contentBase(join("dist"))
                .publicPath("/")
                .headers({ "Access-Control-Allow-Origin": "*" });
        }

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
