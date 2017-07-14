/// <binding />
const webpack = require('webpack')
const path = require('path')
const baseAppPath = "./Dependencies/"
const devMode = process.env.NODE_ENV != 'production';
const BundleAnalyzerPlugin = require('webpack-bundle-analyzer').BundleAnalyzerPlugin

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
                use: {
                    loader: 'vue-loader',
                    options: {
                        loaders: {
                            js: 'babel-loader?presets[]=env'
                        }
                    }
                }
            }, {
                test: /\.js$/,
                exclude: /(node_modules)/,
                use: {
                    loader: 'babel-loader',
                    options: {
                        presets: [["env", { "modules": false }]]
                    }
                }
            }
        ]
    },
    plugins: [
        new webpack.DefinePlugin({
            'process.env': {
                NODE_ENV: devMode ? '"development"' : '"production"'
            }
        }),
        // new webpack.ProvidePlugin({
        //   //  'Promise': 'es6-promise', // Thanks Aaron (https://gist.github.com/Couto/b29676dd1ab8714a818f#gistcomment-1584602)
        //     fetch: 'imports-loader?this=>global!exports-loader?global.fetch!whatwg-fetch'
        // }),
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

        new webpack.optimize.ModuleConcatenationPlugin(),

        devMode ? null : new webpack.optimize.UglifyJsPlugin({
            compress: {
                warnings: false
            },
            beautify: false, // Don't beautify output (uglier to read)
            comments: false // Eliminate comments
        })
        // devMode ? null :new BundleAnalyzerPlugin({
        //     analyzerMode: 'static',
        //     reportFilename: 'stats.html',
        //     openAnalyzer: false,
        //     statsOptions: { chunkModules: true, assets: true },
        // })
    ].filter(x => x != null)
}