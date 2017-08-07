var path = require('path')
var utils = require('./utils')
var config = require('./config').current()
var webpack = require('webpack')
var vueLoaderConfig = require('./vue-loader.conf')

var projectRoot = path.resolve(__dirname, '../')

function resolve(dir) {
    return path.join(__dirname, '..', dir)
}

module.exports = {
    entry:
    ["babel-polyfill",
        './src/main.js']
    ,
    output: {
        path: config.assetsRoot,
        filename: '[name].js',
        publicPath: config.assetsPublicPath
    },
    resolve: {
        extensions: ['.js', '.vue', '.json'],
        modules: [
            resolve('src'),
            resolve('node_modules')
        ],
        alias: {
            'src': resolve('src'),
            'assets': resolve('src/assets'),
            'components': resolve('src/components')
        }
    },
    plugins: [
        new webpack.DefinePlugin({
            'process.env': env
        }),
        new webpack.ProvidePlugin({
            $: 'jquery',
            jQuery: 'jquery',
            "window.jQuery": 'jquery',
            "window.$": 'jquery'
        }),
    ],
    module: {
        rules: [
            {
                test: /\.js$/,
                exclude: /(node_modules)/,
                use: { loader: 'babel-loader' }
            },
            {
                test: /\.vue$/,
                loader: 'vue-loader',
                options: vueLoaderConfig
            },
            {
                test: /\.(png|jpe?g|gif|svg)(\?.*)?$/,
                loader: 'url-loader',
                options: {
                    publicPath: config.assetsRelativePath,
                    limit: 10000,
                    name: utils.assetsPath('img/[name].[ext]')
                }
            },
            {
                test: /\.(woff2?|eot|ttf|otf)(\?.*)?$/,
                loader: 'url-loader',
                options: {
                    publicPath: config.assetsRelativePath,
                    limit: 10000,
                    name: utils.assetsPath('fonts/[name].[ext]')
                }
            }
        ]
    }
};
