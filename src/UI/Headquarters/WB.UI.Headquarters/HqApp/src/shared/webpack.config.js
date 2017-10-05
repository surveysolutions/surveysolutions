const webpack = require('webpack')
const path = require('path')
const baseAppPath = "./"
const cleanWebpackPlugin = require('clean-webpack-plugin');
const BundleAnalyzerPlugin = require('webpack-bundle-analyzer').BundleAnalyzerPlugin
const _ = require("lodash")
const devMode = process.env.NODE_ENV != 'production';

const distFolder = path.join(baseAppPath, "../../dist")
const pjson = require("./package.json")

console.log("Building HQ Shared vendor libs");

module.exports = {
    entry: {
        [pjson.name]: [
            "axios",
            "bootstrap/dist/js/bootstrap.js",
            "i18next",
            "jquery",
            "vue-router",
            "vue",
            "vuex"
        ]
    },
    output: {
        path: __dirname,
        filename: path.join(distFolder, "[name].bundle.js"),
        library: '[name]_[hash]',
    },
    resolve: {
        modules: [
            path.join(__dirname, "node_modules"),
            path.join(__dirname, "app")
        ],
        extensions: ['.js'],
        alias: {
            'vue$': 'vue/dist/vue.esm.js'
        }
    },
    stats: { chunks: false },
    devtool: '#source-map',
    plugins: [
        new cleanWebpackPlugin(["dist/*.*"]),

        new webpack.ProvidePlugin({
            _: 'lodash',
            '$': "jquery",
            "jQuery": 'jquery'
        }),
        new webpack.optimize.ModuleConcatenationPlugin(),

        // https://webpack.js.org/guides/production/
        devMode ? null : new webpack.optimize.UglifyJsPlugin({
            sourceMap: true
        }),

        devMode ? null : new webpack.DefinePlugin({
            'process.env.NODE_ENV': JSON.stringify('production')
        }),

        // https://webpack.js.org/plugins/dll-plugin
        new webpack.DllPlugin({
            path: path.join(distFolder, "[name].manifest.json"),
            name: '[name]_[hash]'
        }),

        new BundleAnalyzerPlugin({
            analyzerMode: 'static',
            reportFilename: path.join(distFolder, 'stats.' + pjson.name + '.html'),
            openAnalyzer: false,
            defaultSizes: 'gzip',
            statsOptions: { chunkModules: true, assets: true },
        })
    ].filter(x => x != null)
}