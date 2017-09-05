/// <binding />
const webpack = require('webpack')
const path = require('path')
const baseAppPath = "./"
const devMode = process.env.NODE_ENV != 'production';

console.log("Building HQ UI js in " + (devMode ? "DEVELOPMENT" : "PRODUCTION") + " mode.")

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
    externals: {
        "jquery": "jQuery",
        "$": "jQuery",
    },

    devtool: '#source-map',//  '#cheap-module-eval-source-map'

    module: {
        rules: [
            {
                test: /\.vue$/,
                exclude: /(node_modules)/,
                use: { loader: 'vue-loader', options: { loaders: { js: 'babel-loader' } } }
            }, {
                test: /\.js$/,
                exclude: /(node_modules)/,
                use: { loader: 'babel-loader' }
            }
        ]
    },
    plugins: [
        new webpack.DllReferencePlugin({
            manifest: require('./dist/vendor.manifest.json')
        }),

        devMode ? null : function () {
            const BundleAnalyzerPlugin = require('webpack-bundle-analyzer').BundleAnalyzerPlugin
            
            return new BundleAnalyzerPlugin({
                analyzerMode: 'static',
                reportFilename: 'dist/stats.html',
                openAnalyzer: false,
                statsOptions: { chunkModules: true, assets: true },
            });
        }()
    ].filter(x => x != null)
}