require('shelljs/global')
require('./check-versions')()
var config = require('./config')
if (!process.env.NODE_ENV) process.env.NODE_ENV = JSON.parse(config.dev.env.NODE_ENV)
var path = require('path')
var express = require('express')
var webpack = require('webpack')
var ora = require('ora')

var spinner = ora('Starting dev session... ')
spinner.start()

var opn = require('opn')
var proxyMiddleware = require('http-proxy-middleware')
var webpackConfig = process.env.NODE_ENV === 'testing'
    ? require('./webpack.prod.conf')
    : require('./webpack.dev.conf')

console.log("Cleaning up content of", config.dev.assetsRoot, "folder")
rm('-rf', path.join(config.dev.assetsRoot, '**/*'))

// default port where dev server listens for incoming traffic
var port = process.env.PORT || config.dev.port

var app = express()
var compiler = webpack(webpackConfig)

var devMiddleware = require('webpack-dev-middleware')(compiler, {
    publicPath: webpackConfig.output.publicPath,
    quiet: true
})

var hotMiddleware = require('webpack-hot-middleware')(compiler, {
    log: () => { }
})

// // force page reload when html-webpack-plugin template changes
// compiler.plugin('compilation', function (compilation) {
//     compilation.plugin('html-webpack-plugin-after-emit', function (data, cb) {
//         hotMiddleware.publish({ action: 'reload' })
//         cb()
//     })
// })

// Define HTTP proxies to your custom API backend
// https://github.com/chimurai/http-proxy-middleware
var proxyTable = config.current().proxyTable

// proxy api requests
Object.keys(proxyTable).forEach(function (context) {
    var options = proxyTable[context]
    if (typeof options === 'string') {
        options = { target: options, changeOrigin: true }
    }
    app.use(proxyMiddleware(context, options))
})

// handle fallback for HTML5 history API
app.use(require('connect-history-api-fallback')())

// serve webpack bundle output
app.use(devMiddleware)

// enable hot-reload and state-preserving
// compilation error display
app.use(hotMiddleware)

// serve pure static assets
var staticPath = config.dev.assetsRoot //path.posix.join(config.dev.asset, config.dev.assetsSubDirectory)
app.use(staticPath, express.static('./'))

app.use(function (req, res, next) {
    res.header("Access-Control-Allow-Origin", "*");
    res.header("Access-Control-Allow-Headers", "Origin, X-Requested-With, Content-Type, Accept");
    next();
});

app.get('/*', function(req, res){
  res.sendFile(__dirname + '/index.html');
});

var uri = 'http://localhost:' + port

var _resolve
var readyPromise = new Promise(resolve => {
  _resolve = resolve
})

console.log("Waiting for dev server")

devMiddleware.waitUntilValid(function () {
    if (process.env.DEV_MODE === 'design') {
        console.log('> Using ' + config.designer.proxyTable['/signalr'] + ' API uri.')
        spinner.text = 'Ready to serve at http://localhost:8080 \n'
    } else {
        spinner.text = 'Ready to serve at http://localhost/headquarters/webinterview \n'
    }

    spinner.succeed();
    _resolve()
})

var server = app.listen(port)

module.exports = {
  ready: readyPromise,
  close: () => {
    server.close()
  }
}
