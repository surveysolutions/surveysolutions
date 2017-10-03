const webpack = require('webpack')
const devMode = process.env.NODE_ENV != 'production';
const path = require("path")
const HtmlWebpackPlugin = require('html-webpack-plugin');

const merge = require('webpack-merge')
const hqViewsFolder = path.resolve(__dirname, "..", "Views")

console.log(`Building HQ UI js in ${(devMode ? "DEVELOPMENT" : "PRODUCTION")} mode.`)

var commonConfig = require("./.build/webpack.common")

var webpackConfig = merge(commonConfig, {
    entry: {
        hqapp: "./src/hqapp/main.js",        
        webinterview: "./src/webinterview/main.js"
    },
    plugins: [
        new HtmlWebpackPlugin({
            inject: false,
            filename: path.resolve(hqViewsFolder, "shared", "partial.webInterview.cshtml"),
            excludeChunks: ["hqapp"],
            template: '!!handlebars-loader!src/template.hbs'
        }),

        new HtmlWebpackPlugin({
            inject: false,
            filename: path.resolve(hqViewsFolder, "shared", "partial.hq.cshtml"),
            excludeChunks: ["webinterview"],
            template: '!!handlebars-loader!src/template.hbs'
        })
    ]
})

module.exports = webpackConfig