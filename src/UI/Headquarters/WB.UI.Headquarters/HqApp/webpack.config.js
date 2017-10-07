const webpack = require('webpack')
const devMode = process.env.NODE_ENV != 'production';
const path = require("path")
const HtmlWebpackPlugin = require('html-webpack-plugin');

const merge = require('webpack-merge')
const hqViewsFolder = path.resolve(__dirname, "..", "Views")

console.log(`Building HQ UI js in ${(devMode ? "DEVELOPMENT" : "PRODUCTION")} mode.`)

var commonConfig = require("./.build/webpack.common")

const entry = {
    hq: "./src/hqapp/main.js",
    test: "./src/hqapp/test.js",
    webinterview: "./src/webinterview/main.js"
};

const entryNames = Object.keys(entry)

const plugins = entryNames.map((e) => {
    return new HtmlWebpackPlugin({
        inject: false,
        filename: path.resolve(hqViewsFolder, "shared", `partial.${e}.cshtml`),
        excludeChunks: entryNames.filter((name) => name !== e),
        template: '!!handlebars-loader!src/template.hbs'
    })
});

module.exports = merge(commonConfig, { entry, plugins })