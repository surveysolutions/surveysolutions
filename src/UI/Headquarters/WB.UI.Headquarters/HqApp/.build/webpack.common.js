const webpack = require('webpack')
const path = require('path')
const baseAppPath = "../"
const baseDir = path.resolve(__dirname, baseAppPath);
const devMode = process.env.NODE_ENV != 'production';

const BundleAnalyzerPlugin = require('webpack-bundle-analyzer').BundleAnalyzerPlugin
const merge = require('webpack-merge')

const sharedManifest = "dist/shared_vendor.manifest.json";

var fs = require('fs');
if (!fs.existsSync(path.resolve(sharedManifest))) {
    const { execSync } = require('child_process');
    console.log("Build missing `shared_vendor.bundle.js`")
    execSync('npm run vendor')
}

var sharedVendor = require(baseAppPath + sharedManifest);

module.exports = {
    output: {
        path: path.resolve(__dirname, baseAppPath, "dist"),
        filename: "[name].bundle.js"
    },
    resolve: {
        extensions: ['.js', '.vue', '.json'],
        alias: {
            "shared": path.resolve(baseDir, 'src/shared'),
            'vue$': 'vue/dist/vue.esm.js'
        }
    },

    stats: { chunks: false },

    devtool: '#source-map',//  '#cheap-module-eval-source-map'
    module: {
        rules: [
            {
                test: /\.vue$/,
                include: path.resolve(baseDir, "src"),
                use: [{ loader: 'vue-loader', options: { loaders: { js: 'babel-loader' } } }]
            }, {
                test: /\.js$/,
                include: path.resolve(baseDir, "src"),
                use: ['babel-loader']
            }
            // , {
            //     test: /\.(js|vue)$/,
            //     loader: 'eslint-loader',
            //     enforce: 'pre',
            //     options: {
            //         formatter: require('eslint-friendly-formatter')
            //     }
            // }
        ]
    },
    plugins: [
        new webpack.DllReferencePlugin({
            manifest: sharedVendor
        }),

        new webpack.ProvidePlugin({
            _: 'lodash',
            '$': "jquery",
            "jQuery": 'jquery',
            'moment': 'moment'
        }),
   
        devMode ? null : new webpack.optimize.UglifyJsPlugin({
            sourceMap: true
        }),

        devMode ? null : new webpack.DefinePlugin({
            'process.env.NODE_ENV': JSON.stringify('production')
        }),

        new webpack.optimize.ModuleConcatenationPlugin(),

        new BundleAnalyzerPlugin({
            analyzerMode: 'static',
            reportFilename: 'stats.html',
            defaultSizes: 'gzip',
            openAnalyzer: false,
            statsOptions: { chunkModules: true, assets: true },
        })

    ].filter(x => x != null)
}