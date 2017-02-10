// see http://vuejs-templates.github.io/webpack for documentation.
var path = require('path')

var argv = require('minimist')(process.argv.slice(2));

var projectPath = path.resolve(__dirname, '../../../WB.UI.Headquarters')

const config = {
    build: {
        env: require('./prod.env'),
        index: path.resolve(projectPath, 'Views/WebInterview/Index.cshtml'),
        assetsRoot: path.resolve(projectPath, 'InterviewApp'),
        assetsSubDirectory: '',
        assetsPublicPath: '~/InterviewApp/',
        productionSourceMap: false,
        productionGzip: false,
        assetsRelativePath: "../../Dependencies/",
        productionGzipExtensions: ['js', 'css'],
        template: '_IndexTemplate.cshtml',
        bundleAnalyzerReport: true
    },
    dev: {
        env: require('./dev.env'),
        port: 8080,
        index: path.resolve(projectPath, 'Views/WebInterview/Index.cshtml'),
        assetsRoot: path.resolve(projectPath, 'InterviewApp'),
        assetsSubDirectory: '',
        assetsPublicPath: '/headquarters/interviewapp/',
        assetsRelativePath: "/Headquarters/Dependencies/",
        proxyTable: {},
        cssSourceMap: false,
        template: '_IndexTemplate.cshtml',
        verbose: true
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
