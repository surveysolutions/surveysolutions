const webpack = require('webpack')
const path = require('path')
const baseAppPath = "../"
const baseDir = path.resolve(__dirname, baseAppPath);
const devMode = process.env.NODE_ENV != 'production';
var WebpackNotifierPlugin = require('webpack-notifier');
const cleanWebpackPlugin = require('clean-webpack-plugin');
const ExtractTextPlugin = require("extract-text-webpack-plugin");
const BundleAnalyzerPlugin = require('webpack-bundle-analyzer').BundleAnalyzerPlugin
const merge = require('webpack-merge')

const babelLoader = devMode ? "babel-loader?cacheDirectory=true" : "babel-loader"
const RuntimePublicPathPlugin = require("./RuntimePublicPathPlugin")

const join = path.join.bind(path, baseDir);
const manifest = require("../dist/shared_vendor.manifest.json")

module.exports = {
    output: {
        path: path.resolve(__dirname, baseAppPath, "dist"),
        filename: devMode ? "[name].bundle.js" : "[name].bundle.[chunkhash].js",
        chunkFilename: devMode ? "[name].chunk.js" : "[name].chunk.[chunkhash].js"
    },
    resolve: {
        unsafeCache: true,
        extensions: ['.js', '.vue', '.json'],
        symlinks: false,
        alias: {
            "~": path.resolve(baseDir, 'src'),
            'vue$': 'vue/dist/vue.esm.js',
            moment$: 'moment/moment.js'
        }
    },

    stats: { chunks: false },

    devtool: '#source-map', // '#cheap-module-eval-source-map',
    module: {
        rules: [
            {
                test: /\.vue$/,
                include: path.resolve(baseDir, "src"),
                use: [{
                    loader: 'vue-loader', options: {
                        loaders: { js: babelLoader }
                    }
                }]
            }, {
                test: /\.js$/,
                include: path.resolve(baseDir, "src"),
                use: [babelLoader]
            },

            { test: /\.css$/, use: ExtractTextPlugin.extract({ use: "css-loader" }) }

            // , {
            //     test: /\.(js|vue)$/,
            //     loader: 'eslint-loader',
            //     enforce: 'pre',
            //     options: {
            //         formatter: require('eslint-friendly-formatter')
            //     }
            // }
        ].filter((x) => x)
    },

    plugins: [
        new webpack.PrefetchPlugin(join("./src/webinterview/componentsRegistry.js")),
        new webpack.PrefetchPlugin(join("./src/hqapp/components/index.js")),
        new webpack.PrefetchPlugin(join("./src/hqapp/main.js")),//
        new webpack.ProvidePlugin({
            _: 'lodash',
            '$': "jquery",
            "jQuery": 'jquery',
            'moment': 'moment'
        }),

        new RuntimePublicPathPlugin({
            runtimePublicPath: "window.CONFIG.assetsPath"
        }),

        new webpack.DllReferencePlugin({
            manifest
        }),

        new ExtractTextPlugin(`styles${devMode ? "" : ".[chunkhash]"}.css`),

        devMode ? null : new webpack.optimize.CommonsChunkPlugin({
            name: 'common'//,
            //async: true,
            //names: ['datatables.net', 'jquery-contextmenu', 'bootstrap-select', 'moment', 'flatpickr']
        }),

        devMode ? null : new webpack.optimize.CommonsChunkPlugin({
            name: "manifest",
            minChunks: Infinity
        }),

        devMode ? null : new webpack.HashedModuleIdsPlugin(),

        devMode ? null : new webpack.optimize.ModuleConcatenationPlugin(),

        devMode ? null : new webpack.optimize.UglifyJsPlugin({
            sourceMap: true
        }),

        devMode ? null : new webpack.DefinePlugin({
            'process.env.NODE_ENV': JSON.stringify('production')
        }),

        devMode ? new WebpackNotifierPlugin({ alwaysNotify: true }) : null,

        devMode ? null : new BundleAnalyzerPlugin({
            analyzerMode: 'static',
            reportFilename: 'stats.html',
            defaultSizes: 'gzip',
            openAnalyzer: false,
            statsOptions: { chunkModules: true, assets: true },
        })

    ].filter(x => x != null)
}