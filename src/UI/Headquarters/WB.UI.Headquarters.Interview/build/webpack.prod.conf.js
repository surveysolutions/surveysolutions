var path = require('path')
var utils = require('./utils')
var webpack = require('webpack')
var config = require('./config').current()
var merge = require('webpack-merge')
var baseWebpackConfig = require('./webpack.base.conf')
var HtmlWebpackPlugin = require('html-webpack-plugin')
var ExtractTextPlugin = require('extract-text-webpack-plugin')
var ScriptExtHtmlWebpackPlugin = require('script-ext-html-webpack-plugin')
var env = config.env

var webpackConfig = merge(baseWebpackConfig, {
    module: {
        rules: utils.styleLoaders({
            sourceMap: config.cssSourceMap,
            extract: true
        })
    },
    devtool: config.cssSourceMap ? '#source-map' : false,
    output: {
        path: config.assetsRoot,
        filename: utils.assetsPath('js/[name].[chunkhash].js'),
        chunkFilename: utils.assetsPath('js/[name].[chunkhash].js')
    },
    node: {
        Buffer: false
    },
    plugins: [
        // http://vuejs.github.io/vue-loader/en/workflow/production.html
        new webpack.optimize.UglifyJsPlugin({
            compress: {
                warnings: false
            },
            beautify: false, // Don't beautify output (uglier to read)
            comments: false // Eliminate comments
        }),
        // extract css into its own file
        new ExtractTextPlugin(utils.assetsPath('css/[name].[contenthash].css')),
        // generate dist index.html with correct asset hash for caching.
        // you can customize output by editing /index.html
        // see https://github.com/ampedandwired/html-webpack-plugin
        new HtmlWebpackPlugin({
            filename: config.index,
            template: config.template,
            inject: true,
            verboseLogging: false,
            minify: {
                removeComments: true,
                collapseWhitespace: true,
                // more options:
                // https://github.com/kangax/html-minifier#options-quick-reference
            },
            // necessary to consistently work with multiple chunks via CommonsChunkPlugin
            chunksSortMode: 'dependency'
        }),
       // new (require('inline-chunk-manifest-html-webpack-plugin'))(),
        new ScriptExtHtmlWebpackPlugin({
            defaultAttribute: 'defer'
        }),
        // split vendor js into its own file
        new webpack.optimize.CommonsChunkPlugin({
            name: 'vendor',
            minChunks: function (module, count) {
                // any required modules inside node_modules are extracted to vendor
                return (
                    module.resource &&
                    /\.(js|ts)$/.test(module.resource) &&
                    module.resource.indexOf(
                        path.join(__dirname, '../node_modules')
                    ) === 0
                )
            }
        }),
        // // extract webpack runtime and module manifest to its own file in order to
        // // prevent vendor hash from being updated whenever app bundle is updated
        new webpack.optimize.CommonsChunkPlugin({
            name: 'manifest',
            chunks: ['vendor']
        })
    ]
})

if (config.productionGzip) {
    var CompressionWebpackPlugin = require('compression-webpack-plugin')

    webpackConfig.plugins.push(
        new CompressionWebpackPlugin({
            asset: '[path].gz[query]',
            algorithm: 'gzip',
            test: new RegExp(
                '\\.(' +
                config.productionGzipExtensions.join('|') +
                ')$'
            ),
            threshold: 10240,
            minRatio: 0.8
        })
    )
}

if (config.bundleAnalyzerReport) {
    var BundleAnalyzerPlugin = require('webpack-bundle-analyzer').BundleAnalyzerPlugin
    webpackConfig.plugins.push(new BundleAnalyzerPlugin({
        analyzerMode: 'static',
        reportFilename: 'stats.html',
        openAnalyzer: false,
        statsOptions: { chunkModules: true, assets: true , reasons: true},
    }))
}

module.exports = webpackConfig
