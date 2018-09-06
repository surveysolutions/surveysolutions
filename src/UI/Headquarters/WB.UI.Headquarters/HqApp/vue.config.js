const path = require("path");
const baseDir = path.resolve(__dirname, "./");
const join = path.join.bind(path, baseDir);
const webpack = require("webpack");
const WriteFilePlugin = require("write-file-webpack-plugin");
const RuntimePublicPathPlugin = require("./tools/RuntimePublicPathPlugin");
const FileManagerPlugin = require("filemanager-webpack-plugin");

const LocalizationBuilder = require("./tools/localization");

const isHot = process.env.HOT_MODE == 1;
const assetsPath = isHot ? "http://localhost:8080/" : "~/HqApp/dist/";

const uiFolder = join("..", "..", "..");
const hqFolder = join("..");

const pages = {
    hq: {
        entry: "src/hqapp/main.js",
        filename: path.join(hqFolder, "Views", "Shared", "partial.hq.cshtml"),
        template: "tools/template.ejs",
        locales: [
            "Details",
            "Pages",
            "WebInterviewUI",
            "WebInterview",
            "DataTables",
            "Common",
            "Users",
            "Assignments",
            "Strings",
            "Report",
            "Reports",
            "DevicesInterviewers",
            "UploadUsers",
            "MainMenu",
            "WebInterviewSetup",
            "MapReport"
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
        assetsPath: "~/Content/Dist/",
        locales: ["WebInterviewUI", "WebInterview", "Common"]
    }
};

const localization = new LocalizationBuilder({
    patterns: [
        join("../**/*.resx"),
        join(
            "../../../../Core/SharedKernels/Enumerator/WB.Enumerator.Native/Resources/*.resx"
        ),
        join(
            "../../../../Core/BoundedContexts/Headquarters/WB.Core.BoundedContexts.Headquarters/Resources/*.resx"
        )
    ],
    destination: "./locale/.resources"
});

localization.prepareLocalizationFiles();

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

    chainWebpack: config => {
        config.plugin("provide").use(webpack.ProvidePlugin, [
            {
                _: "lodash",
                $: "jquery",
                jQuery: "jquery",
                moment: "moment"
            }
        ]);

        config.plugin("runtime").use(RuntimePublicPathPlugin, [
            {
                runtimePublicPath: "window.CONFIG.assetsPath"
            }
        ]);

        config.plugin("fileManager").use(FileManagerPlugin, [
            {
                onEnd: {
                    copy: [
                        {
                            source: join(
                                "locale",
                                ".resources",
                                "**",
                                "*.json"
                            ),
                            destination: join("dist")
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
            config.plugin("html-" + page).tap(args => {
                const options = args[0];

                const locales = localization.writeFiles(
                    join("locale", ".resources"),
                    path.join("locale", page),
                    pages[page].locales
                );

                //const locales = localizationFiles[page];
                options.locales = {
                    json: JSON.stringify(locales),
                    dictionary: localization.getDictionaryDefinition(locales)
                };
                options.isHot = isHot;
                options.assetsPath = pages[page].assetsPath || assetsPath;
                options.minify = false;
                options.inject = false;
                return args;
            });
        });
    }
};
