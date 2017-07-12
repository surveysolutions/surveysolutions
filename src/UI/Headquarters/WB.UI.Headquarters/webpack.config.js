const webpack = require('webpack')
const path = require('path')
const baseAppPath = "./Dependencies/"
const devMode = process.env.NODE_ENV != 'production';
const BundleAnalyzerPlugin = require('webpack-bundle-analyzer').BundleAnalyzerPlugin

function resolve(dir) {
    return path.join(__dirname, '..', dir)
}

module.exports = {
    entry: {
        interviewer: baseAppPath + "app/interviewer.js",
        assignments: baseAppPath + "app/assignments.js"
    },
    output: {
        path: __dirname,
        filename: path.join(baseAppPath, "./build/[name].bundle.js")
    },
    resolve: {
        modules: [
            path.join(__dirname, "node_modules"),
            resolve("Dependencies/app")
        ],
        extensions: ['.js', '.vue', '.json'],
        alias: {
            'vue$': 'vue/dist/vue.esm.js'
        }
    },
    externals: {
        // require("jquery") is external and available
        //  on the global var jQuery
        "jquery": "jQuery",
        "$": "jQuery",
    },
    // devtool: devMode ? '#cheap-module-eval-source-map' : null,
    devtool: '#source-map',

    module: {
        rules: [
            {
                test: /\.vue$/,
                loader: 'vue-loader',
            },
            {
                test: /\.js$/,
                loader: 'babel-loader',
                exclude: /node_modules/
            }
        ]
    },
    plugins: [
        devMode ? null :new webpack.DefinePlugin({
            'process.env': {
                NODE_ENV: '"production"'
            }
        }),
        //devMode ? null :
        // split vendor js into its own file
        new webpack.optimize.CommonsChunkPlugin({
            name: 'vendor',
            minChunks: function (module, count) {
                // any required modules inside node_modules are extracted to vendor
                return (
                    module.resource &&
                    /\.(js|ts)$/.test(module.resource) &&
                    module.resource.indexOf(
                        path.join(__dirname, 'node_modules')
                    ) === 0
                )
            }
        }),
        // // extract webpack runtime and module manifest to its own file in order to
        // // prevent vendor hash from being updated whenever app bundle is updated
        // new webpack.optimize.CommonsChunkPlugin({
        //     name: 'manifest',
        //     chunks: ['vendor']
        // }),
        new webpack.optimize.ModuleConcatenationPlugin(),
        devMode ? null : new webpack.optimize.UglifyJsPlugin({
            compress: {
                warnings: false
            },
            beautify: false, // Don't beautify output (uglier to read)
            comments: false // Eliminate comments
        }),
        new BundleAnalyzerPlugin({
            analyzerMode: 'static',
            reportFilename: 'stats.html',
            openAnalyzer: false,
            statsOptions: { chunkModules: true, assets: true },
        })
    ].filter(x => x != null)
}