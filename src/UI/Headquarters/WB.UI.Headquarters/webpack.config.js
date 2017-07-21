/// <binding />
const webpack = require('webpack')
const path = require('path')
const baseAppPath = "./Dependencies/"
const devMode = process.env.NODE_ENV != 'production';

console.log("Building HQ UI js in " + (devMode ? "DEVELOPMENT" : "PRODUCTION") + " mode.")

module.exports = {
    entry: {
        "interviewer_createNew": baseAppPath + "app/interviewer/createNew.js",
        "interviewer_interviews": baseAppPath + "app/interviewer/interviews.js"
    },
    output: {
        path: __dirname,
        filename: path.join(baseAppPath, "./build/[name].bundle.js")
    },
    resolve: {
        modules: [
            path.join(__dirname, "node_modules"),
            path.join(__dirname, "Dependencies/app")
        ],
        extensions: ['.js', '.vue', '.json'],
        alias: {
            'vue$': 'vue/dist/vue.esm.js'
        }
    },
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
                use: { loader: 'vue-loader', options: { loaders: { js: 'babel-loader?presets[]=env' } } }
            }, {
                test: /\.js$/,
                exclude: /(node_modules)/,
                use: { loader: 'babel-loader', options: { presets: [["env", { "modules": false }]] } }
            }
        ]
    },
    plugins: [
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

        new webpack.optimize.ModuleConcatenationPlugin(),

        devMode ? null : function () {
            const BundleAnalyzerPlugin = require('webpack-bundle-analyzer').BundleAnalyzerPlugin
            
            return new BundleAnalyzerPlugin({
                analyzerMode: 'static',
                reportFilename: 'Dependencies/build/stats.html',
                openAnalyzer: false,
                statsOptions: { chunkModules: true, assets: true },
            });
        }()
    ].filter(x => x != null)
}