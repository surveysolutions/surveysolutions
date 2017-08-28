const webpack = require('webpack')
const path = require('path')
const baseAppPath = "./"
const devMode = process.env.NODE_ENV != 'production';
const BundleAnalyzerPlugin = require('webpack-bundle-analyzer').BundleAnalyzerPlugin


console.log("Building HQ UI js in " + (devMode ? "DEVELOPMENT" : "PRODUCTION") + " mode.")

var fs = require('fs');
if (!fs.existsSync(path.join(baseAppPath, "./dist/vendor.bundle.js"))) {
    const { execSync } = require('child_process');
    console.log("Build missing `vendor.bundle.js`")
    execSync('npm run vendor')
}

var manifest = require("./dist/vendor.manifest.json");

module.exports = {
    entry: {
        "app": baseAppPath + "app/main.js"
    },
    output: {
        path: __dirname,
        filename: path.join(baseAppPath, "./dist/[name].bundle.js")
    },
    resolve: {
        modules: [
            path.join(__dirname, "node_modules"),
            path.join(__dirname, "app")
        ],
        extensions: ['.js', '.vue', '.json'],
        alias: {
            'vue$': 'vue/dist/vue.esm.js'
        }
    },

    stats: { chunks: false },

    devtool: '#source-map',//  '#cheap-module-eval-source-map'

    module: {
        rules: [
            {
                test: /\.vue$/,
                include: path.resolve(__dirname, "app"),
                use: [{ loader: 'vue-loader', options: { loaders: { js: 'babel-loader' } } }]
            }, {
                test: /\.js$/,
                include: path.resolve(__dirname, "app"),
                use: ['babel-loader']
            }, {
                test: /\.(js|vue)$/,
                loader: 'eslint-loader',
                enforce: 'pre',
                options: {
                    formatter: require('eslint-friendly-formatter')
                }
            }
        ]
    },
    plugins: [
        new webpack.DllReferencePlugin({
            manifest
        }),

        new webpack.ProvidePlugin({
            _: 'lodash',
            '$': "jquery",
            "jQuery": 'jquery',
            'moment': 'moment'
        }),

        new webpack.optimize.ModuleConcatenationPlugin(),

        new BundleAnalyzerPlugin({
            analyzerMode: 'static',
            reportFilename: 'dist/stats.html',
            defaultSizes: 'gzip',
            openAnalyzer: false,
            statsOptions: { chunkModules: true, assets: true },
        })
    ].filter(x => x != null)
}