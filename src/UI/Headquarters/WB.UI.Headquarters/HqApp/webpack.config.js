const fs = require("fs")
const webpack = require('webpack')
const devMode = process.env.NODE_ENV != 'production';
const path = require("path")
const HtmlWebpackPlugin = require('html-webpack-plugin');

const merge = require('webpack-merge')
const hqViewsFolder = path.resolve(__dirname, "..", "Views")
const localization = require("./.build/localization")


module.exports = new Promise(async (resolve) => {

    const dllConfig = require("./webpack.vendor.config");

    function buildMainConfig() {
        var commonConfig = require("./.build/webpack.common")

        const config = {
            hq: {
                entry: "./src/hqapp/main.js",
                locales: ["Pages", "WebInterviewUI", "Common", "Users", "Assignments", "Strings", "Reports", "DevicesInterviewers"]
            },
            webinterview: {
                entry: "./src/webinterview/main.js",
                locales: ["WebInterviewUI", "Common"]
            }
        };

        const localizationInfo = localization.buildLocalizationFiles(config);

        const entryNames = Object.keys(config)

        const plugins = entryNames.map((e) => {
            return new HtmlWebpackPlugin({
                inject: false,
                filename: path.resolve(hqViewsFolder, "shared", `partial.${e}.cshtml`),
                excludeChunks: entryNames.filter((name) => name !== e),
                template: '!!handlebars-loader!src/template.hbs',
                locales: JSON.stringify(localizationInfo[e]),
                entry: e
            })
        });

        const webpackConfig = merge(commonConfig, {
            entry: entryNames.reduce((result, key) => { result[key] = config[key].entry; return result; }, {}),
            plugins
        })

        return webpackConfig;
    }

    fs.stat("./dist/shared_vendor.manifest.json", (err, stats) => {
        if (err) {
            webpack(dllConfig, (err, stats) => {
                resolve(buildMainConfig());
            });
        } else {
            resolve(buildMainConfig());
        }
    });
});