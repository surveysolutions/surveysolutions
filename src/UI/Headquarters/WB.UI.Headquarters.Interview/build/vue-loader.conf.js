var utils = require('./utils')
var config = require('./config').current()
var isProduction = process.env.NODE_ENV === 'production'

module.exports = {
  loaders: utils.cssLoaders({
    sourceMap: config.cssSourceMap,
    extract: isProduction
  }),
  esModule: true,
  postcss: [
    require('autoprefixer')({
      browsers: ['last 2 versions']
    }),
    require('cssnano')() //add this
  ]
}
