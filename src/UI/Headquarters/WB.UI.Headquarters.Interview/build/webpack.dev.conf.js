var utils = require('./utils')
var webpack = require('webpack')
var config = require('./config').current()
var merge = require('webpack-merge')
var baseWebpackConfig = require('./webpack.base.conf')
var HtmlWebpackPlugin = require('html-webpack-plugin')
var FriendlyErrorsPlugin = require('friendly-errors-webpack-plugin')
var WriteFiles = require('write-file-webpack-plugin')

// add hot-reload related code to entry chunks
Object.keys(baseWebpackConfig.entry).forEach(function (name) {
    baseWebpackConfig.entry[name] = ['./build/dev-client'].concat(baseWebpackConfig.entry[name])
})

module.exports = merge(baseWebpackConfig, {
    module: {
        rules: utils.styleLoaders({ sourceMap: config.cssSourceMap })
    },
    // cheap-module-eval-source-map is faster for development
    devtool: '#cheap-module-eval-source-map',
    // devtool: '#source-map',
    plugins: [
        // https://github.com/glenjamin/webpack-hot-middleware#installation--usage
        new webpack.HotModuleReplacementPlugin(),
        new webpack.NoEmitOnErrorsPlugin(),
        // https://github.com/ampedandwired/html-webpack-plugin
        new HtmlWebpackPlugin({
            filename: config.index,
            template: config.template,
            inject: true,
            verboseLogging: true
        }),
        new WriteFiles({
            useHashIndex: true,
            log: false,
            // we don't need anything other that index.cshtml and app.js for dev env to work
            //test: /(\.cshtml)|(app.js)|(app.js.map)$/
        }),
        new FriendlyErrorsPlugin()
    ]
})
