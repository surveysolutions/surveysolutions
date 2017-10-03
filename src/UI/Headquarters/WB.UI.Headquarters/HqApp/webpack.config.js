const webpack = require('webpack')
const devMode = process.env.NODE_ENV != 'production';
const path = require("path")

const merge = require('webpack-merge')

console.log("Building HQ UI js in " + (devMode ? "DEVELOPMENT" : "PRODUCTION") + " mode.")

var commonConfig = require("./.build/webpack.common")

var webpackConfig = merge(commonConfig, {
    entry: {
        hqapp: "./src/hqapp/main.js",
        webinterview: "./src/webinterview/main.js"
    }
})

module.exports = webpackConfig