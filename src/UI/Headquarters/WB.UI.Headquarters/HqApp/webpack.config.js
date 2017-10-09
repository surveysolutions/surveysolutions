const webpack = require('webpack')
const devMode = process.env.NODE_ENV != 'production';
const path = require("path")
const HtmlWebpackPlugin = require('html-webpack-plugin');

const merge = require('webpack-merge')
const hqViewsFolder = path.resolve(__dirname, "..", "Views")
const localization = require("./.build/localization")

console.log(`Building HQ UI js in ${(devMode ? "DEVELOPMENT" : "PRODUCTION")} mode.`)

var commonConfig = require("./.build/webpack.common")

const config = {
    hq: {
        entry: "./src/hqapp/main.js",
        locales: ["Pages", "Common", "Users", "Assignments", "Strings", "Reports", "DevicesInterviewers"]
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

module.exports = webpackConfig