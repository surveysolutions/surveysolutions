var utils = require('./utils')
var config = require('./config').current()
var isProduction = process.env.NODE_ENV === 'production'

module.exports = {
    loaders: Object.assign(utils.cssLoaders({
        sourceMap: config.cssSourceMap,
        extract: isProduction
    }), {
            loaders: {
                js: 'babel-loader'
            }
        }),
    esModule: true,
    postcss: [
        require('autoprefixer')({
            browsers: ['last 2 versions']
        }),
        require('cssnano')() //add this
    ]
}
