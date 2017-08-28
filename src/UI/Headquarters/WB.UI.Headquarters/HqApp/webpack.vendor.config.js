const webpack = require('webpack')
const path = require('path')
const baseAppPath = "./"
const cleanWebpackPlugin = require('clean-webpack-plugin');
const BundleAnalyzerPlugin = require('webpack-bundle-analyzer').BundleAnalyzerPlugin
const _ = require("lodash")
const devMode = process.env.NODE_ENV != 'production';

console.log("Building HQ UI vendor libs");

module.exports = {
    entry: {
        "vendor": [
            "bootstrap/js/dropdown.js",
            "core-js/es6/promise", 
            "core-js/modules/es6.object.assign",
            "datatables.net-select", 
            "datatables.net", 
            "i18next", 
            "jquery-contextmenu", 
            "jquery",
            "lodash", 
            "moment",
            "pnotify", 
            "vee-validate",
            "vue-router",
            "vue", 
            "vuex"
        ]
    },
    output: {
        path: __dirname,
        filename: path.join(baseAppPath, "./dist/[name].bundle.js"),
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
            "jQuery": 'jquery',
            'moment': 'moment'
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
            path: path.join(baseAppPath, "./dist/[name].manifest.json"),
            name: '[name]_[hash]'
        }),

        new BundleAnalyzerPlugin({
            analyzerMode: 'static',
            reportFilename: 'dist/stats.vendor.html',
            openAnalyzer: false,
            defaultSizes: 'gzip',
            statsOptions: { chunkModules: true, assets: true },
        })
    ].filter(x => x != null)
}