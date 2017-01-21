// see http://vuejs-templates.github.io/webpack for documentation.
var path = require('path')

var argv = require('minimist')(process.argv.slice(2));

var projectPath = path.resolve(__dirname, '../../../WB.UI.Headquarters')

const config = {
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
        assetsRelativePath: "../../Dependencies/",
        productionGzipExtensions: ['js', 'css'],
        template: '_IndexTemplate.cshtml'
    },
    dev: {
        env: require('./dev.env'),
        port: 8080,
        index: path.resolve(projectPath, 'Views/WebInterview/Index.cshtml'),
        assetsRoot: path.resolve(projectPath, 'InterviewApp'),
        assetsSubDirectory: '',
        assetsPublicPath: '~/InterviewApp/',
        assetsRelativePath: "/Headquarters/Dependencies/",
        proxyTable: {},
        // CSS Sourcemaps off by default because relative paths are "buggy"
        // with this option, according to the CSS-Loader README
        // (https://github.com/webpack/css-loader#sourcemaps)
        // In our experience, they generally work as expected,
        // just be aware of this issue when enabling this option.
        cssSourceMap: false,
        template: '_IndexTemplate.cshtml'
    },

    designer: {
        env: require('./dev.env'),
        port: 8080,
        index: 'index.html',
        assetsRoot: path.resolve(__dirname, '../.designTime'),
        assetsSubDirectory: '',
        assetsPublicPath: '/',
        proxyTable: {
            "/signalr": argv.uri == null ? "https://superhq-dev.mysurvey.solutions/signalr" : argv.uri
        },
        cssSourceMap: false,
        template: 'Index.html'
    }
}

config.current = () => {
    var isProd = process.env.NODE_ENV === 'production';
    var isDesign = process.env.DEV_MODE === 'design';

    if (isProd === true) {
        return config.build
    } else if (isDesign) {
        return config.designer
    } else {
        return config.dev
    }
}

module.exports = config
