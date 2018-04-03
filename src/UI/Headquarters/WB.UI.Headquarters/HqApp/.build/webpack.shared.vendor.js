const webpack = require('webpack')
const path = require('path')
const baseAppPath = "../"
const BundleAnalyzerPlugin = require('webpack-bundle-analyzer').BundleAnalyzerPlugin
const _ = require("lodash")
const UglifyJsPlugin = require('uglifyjs-webpack-plugin')

module.exports = function(packageName, devMode) {
    return {
        resolve:{
            alias: {
                'vue$': 'vue/dist/vue.esm.js',
                moment$: 'moment/moment.js',
                bootstrap: 'bootstrap/dist/js/bootstrap.js'
            }
        },
        output: {
            path: path.resolve(__dirname, baseAppPath, "dist"),
            filename: "[name].dll.[hash].js",
            library: '[name]_[hash]',
        },
        stats: { chunks: false, modules: false },
        devtool: '#source-map',
        plugins: [
            
            new webpack.ProvidePlugin({
                '$': "jquery",
                "jQuery": 'jquery',
                'moment': 'moment'
            }),

            new webpack.optimize.ModuleConcatenationPlugin(),

            // https://webpack.js.org/guides/production/
            devMode ? null : new UglifyJsPlugin({
                sourceMap: true
            }),

            devMode ? null : new webpack.DefinePlugin({
                'process.env.NODE_ENV': JSON.stringify('production')
            }),

            // https://webpack.js.org/plugins/dll-plugin
            new webpack.DllPlugin({
                path: "dist/[name].manifest.json",
                name: '[name]_[hash]'
            }),

            new BundleAnalyzerPlugin({
                analyzerMode: 'static',
                reportFilename: `${packageName}.stats.html`, //path.join(baseAppPath, 'dist/'+ packageName +'.stats.html'),
                openAnalyzer: false,
                defaultSizes: 'gzip',
                statsOptions: { chunkModules: true, assets: true },
            })
        ].filter(x => x != null)
    }
}