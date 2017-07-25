// https://github.com/shelljs/shelljs
// require('./check-versions')()

process.env.NODE_ENV = 'production'

var ora = require('ora')
var path = require('path')
var chalk = require('chalk')
var shell = require('shelljs')
var webpack = require('webpack')
var config = require('./config').current()
var webpackConfig = require('./webpack.prod.conf')

var spinner = ora('building for production...')
spinner.start()

var assetsPath = path.join(config.assetsRoot, config.assetsSubDirectory)
shell.rm('-rf', assetsPath)
shell.mkdir('-p', assetsPath)
shell.config.silent = true
shell.cp('-R', 'static/*', assetsPath)
shell.config.silent = false

var res = webpack(webpackConfig, function (err, stats) {
    spinner.stop()
    if (err) {
        throw err
    }
    process.stdout.write(stats.toString({
        colors: true,
        modules: false,
        children: false,
        chunks: false,
        chunkModules: false
    }) + '\n\n')

    if(stats.hasErrors()){
        console.log(chalk.red('  Build completed with errors'))
        process.exit(1)
    }

    console.log(chalk.cyan('  Build complete.\n'))
})
