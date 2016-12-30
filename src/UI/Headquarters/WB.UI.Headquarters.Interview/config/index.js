// see http://vuejs-templates.github.io/webpack for documentation.
var path = require('path')

var projectPath = path.resolve(__dirname, '../../WB.UI.Headquarters')

module.exports = {
  build: {
    env: require('./prod.env'),
    // index: path.resolve(__dirname, '../dist/index.html'),
    index: path.resolve(projectPath, 'Views/WebInterview/Index.cshtml'),
    assetsRoot: path.resolve(projectPath, 'InterviewApp'),
    assetsSubDirectory: '',
    assetsPublicPath: '~/InterviewApp/',
    productionSourceMap: true,
    // Gzip off by default as many popular static hosts such as
    // Surge or Netlify already gzip all static assets for you.
    // Before setting to `true`, make sure to:
    // npm install --save-dev compression-webpack-plugin
    productionGzip: false,
    productionGzipExtensions: ['js', 'css']
  },
  dev: {
    env: require('./dev.env'),
    port: 8080,
    index: path.resolve(projectPath, 'Views/WebInterview/Index.cshtml'),
    assetsRoot: path.resolve(projectPath, 'InterviewApp'),
    assetsSubDirectory: '',
    assetsPublicPath: '~/InterviewApp/',
    proxyTable: {},
    // CSS Sourcemaps off by default because relative paths are "buggy"
    // with this option, according to the CSS-Loader README
    // (https://github.com/webpack/css-loader#sourcemaps)
    // In our experience, they generally work as expected,
    // just be aware of this issue when enabling this option.
    cssSourceMap: false
  }
}
