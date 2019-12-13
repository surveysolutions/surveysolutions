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

const pages = {
    hq: {
        entry: "src/hqapp/main.js",
        filename: path.join(hqFolder, "Views", "Shared", "partial.hq.cshtml"),
        template: "tools/template.ejs",
        assetsPath: "~/static/",
        locales: [
            "Details",
            "Pages",
            "WebInterviewUI",
            "WebInterview",
            "DataTables",
            "Common",
            "Users",
            "Interviews",
            "Assignments",
            "Strings",
            "Report",
            "Reports",
            "DevicesInterviewers",
            "UploadUsers",
            "MainMenu",
            "WebInterviewSetup",
            "WebInterviewSettings",
            "MapReport",
            "Settings",
            "DataExport",
            "Dashboard",
            "TabletLogs"
        ]
    },
    webinterview: {
        entry: "src/webinterview/main.js",
        filename: path.join(
            hqFolder,
            "Views",
            "Shared",
            "partial.webinterview.cshtml"
        ),
        assetsPath: "~/static/",
        template: "tools/template.ejs",
        locales: ["WebInterviewUI", "WebInterview", "Common"]
    },
    webtester: {
        entry: "src/webinterview/main.js",
        filename: path.join(
            uiFolder,
            "WB.UI.WebTester",
            "Views",
            "Shared",
            "partial.webtester.cshtml"
        ),
        template: "tools/template.ejs",
        assetsPath: "~/Content/app/",
        locales: ["WebInterviewUI", "WebInterview", "Common"]
    }
};

module.exports = {
    pages: Object.assign(pages, {
        "hq_legacy": { entry: "src/hq_legacy/index.js" },
        "markup": { entry: "src/assets/css/markup.scss" },
        "markup-specific": { entry: "src/assets/css/markup-specific.scss", output: 'library '},
        "markup-web-interview": { entry: "src/assets/css/markup-web-interview.scss" },
        "markup-interview-review": { entry: "src/assets/css/markup-interview-review.scss" }
    }),

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
                        commons: {
                            name: 'chunk-common',
                            test: /[\\\/]node_modules[\\\/](autonumeric|moment)/,
                            priority: 1,
                            chunks: 'all'
                        }
                    }
                }
            }
        });

        config.plugin("provide").use(webpack.ProvidePlugin, [{
            _: "lodash",
            $: "jquery",
            jQuery: "jquery",
            moment: "moment"
        }]);

        config.plugin('cleanup-dists')
            .use(CleanupPlugin, [{
              //  verbose: true,
                dangerouslyAllowCleanPatternsOutsideProject: true,
                dry: false,
                cleanOnceBeforeBuildPatterns: [
                    path.join(hqFolder, 'Views', 'Shared', 'partial.*.cshtml'),
                    path.join(hqFolder, 'wwwroot', 'static', '**', '*.*'),
                    path.join(webTesterFolder, 'Views', 'Shared', 'partial.*.cshtml')]
            }]);

        config.plugin("runtime").use(RuntimePublicPathPlugin, [{
            runtimePublicPath: "window.CONFIG.assetsPath"
        }]);

        config.plugin("fileManager").use(FileManagerPlugin, [
            {
               // verbose: true,
                onEnd: {
                    copy: [
                        {
                            source: join("locale", ".resources", "**", "*.json"),
                            destination: join("dist")
                        },
                        {
                            source: join("dist", "img", "*.*"),
                            destination: path.join(hqFolder, "wwwroot", "static", "img")
                        },
                        {
                            source: join("dist", "fonts", "*.*"),
                            destination: path.join(hqFolder, "wwwroot", "static", "fonts")
                        },
                        {
                            source: join("dist", "js", "*.*"),
                            destination: path.join(hqFolder, "wwwroot", "static", "js")
                        },
                        {
                            source: join("dist", "css", "*.*"),
                            destination: path.join(hqFolder, "wwwroot", "static", "css")
                        }
                    ]
                }
            }
        ]);

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

            if (pages[page].filename == null) {
                config.plugins.delete("html-" + page);
                config.plugins.delete("prefetch-" + page);
                config.plugins.delete("preload-" + page);
            } else {
                config.plugin("html-" + page).tap(args => {
                    const options = args[0];

                    const baseTempalateGenerator = options.templateParameters;

                    options.templateParameters = function (compilation, assets, options) {
                        const result = baseTempalateGenerator(compilation, assets, options)
                        const localization = compilation.compiler.localization;
                        const locales = localization.writeFiles(
                            join("locale", ".resources"),
                            path.join("locale", page),
                            pages[page].locales
                        );

                        result.htmlWebpackPlugin.options.locales = {
                            json: JSON.stringify(locales),
                            dictionary: localization.getDictionaryDefinition(locales)
                        };

                        return result
                    }

                    options.page = page;
                    options.isHot = isHot;
                    options.assetsPath = pages[page].assetsPath || assetsPath;
                    options.minify = false;
                    options.inject = false;
                    return args;
                });
            }
        });
    }
};
