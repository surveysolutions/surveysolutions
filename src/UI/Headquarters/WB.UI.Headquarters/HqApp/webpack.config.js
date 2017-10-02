const webpack = require('webpack')
const devMode = process.env.NODE_ENV != 'production';
const path = require("path")

const merge = require('webpack-merge')

console.log("Building HQ UI js in " + (devMode ? "DEVELOPMENT" : "PRODUCTION") + " mode.")

var commonConfig = require("./.build/webpack.common")

var webpackConfig = merge(commonConfig, {
    entry: {
        hqapp: "./src/hqapp/main.js",
        webinterview: "./src/webinterview/main.js"
    },
    resolve: {
        alias: {
            wi: path.resolve()
        }
    }
})

webpackConfig.plugins.unshift(new webpack.optimize.CommonsChunkPlugin({
    name: 'common'
}));

// webpackConfig.plugins.unshift(
//     // split vendor js into its own file
//     new webpack.optimize.CommonsChunkPlugin({
//         name: 'vendor',
//         minChunks: function (module, count) {
//             // any required modules inside node_modules are extracted to vendor
//             return (
//                 module.resource &&
//                 /\.(js|ts)$/.test(module.resource) &&
//                 module.resource.indexOf(
//                     path.join(__dirname, './node_modules')
//                 ) === 0
//             )
//         }
//     }),

//     // // extract webpack runtime and module manifest to its own file in order to
//     // // prevent vendor hash from being updated whenever app bundle is updated
//     new webpack.optimize.CommonsChunkPlugin({
//         name: 'manifest',
//         chunks: ['vendor']
//     })
// )

module.exports = webpackConfig