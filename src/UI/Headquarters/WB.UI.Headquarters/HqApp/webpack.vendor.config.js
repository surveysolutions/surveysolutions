const webpack = require('webpack')
const path = require('path')
const baseAppPath = "./"
const cleanWebpackPlugin = require('clean-webpack-plugin');

console.log("Building HQ UI vendor libs");

module.exports = {
    entry: {
        "vendor": [
            "vue", "vuex", "vue-router",
            "i18next", "pnotify", "vee-validate",
            "core-js/es6/promise",
            "core-js/modules/es6.object.assign"
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
    module: {
        rules: [
            {
                test: /\.js$/,
                exclude: /(node_modules)/,
                use: { loader: 'babel-loader' }
            }
        ]
    },
    plugins: [
        new cleanWebpackPlugin(["dist"]),
        new webpack.DllPlugin({
            path:  path.join(baseAppPath, "./dist/[name].manifest.json"),
            name: '[name]_[hash]'
        })
    ]
}