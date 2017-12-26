const gulp = require("gulp")
const webpack = require("webpack")
const utils = require('gulp-util')
const merge = require('webpack-merge');
const rimraf = require("rimraf");
const debug = require('gulp-debug');
const plugins = require('gulp-load-plugins')();
const jest = require('jest-cli');
const fs = require("fs")

const config = {
    dist: 'dist',
    hqViews: '../Views/Shared'
}

config.resources = {
    source: '../',
    dest: "locale/.resources"
}

gulp.task("webTester", ['build'], function(){
    return gulp.src(config.dist + "/**/*.*")
        .pipe(plugins.filter(['**/*.js', '**/*.css']))
        .pipe(gulp.dest('../../../WB.UI.WebTester/Content/Dist'));
});

gulp.task("default", ['cleanup', 'resx2json', 'build', 'webTester', 'test'].filter((x) => x));

gulp.task("build", ["resx2json", "vendor"], (done) => {
    const opts = {
        plugins: []
    }

    if (!utils.env.production) {
        opts.plugins.push(new webpack.ProgressPlugin());
    } else {
        process.env.NODE_ENV = 'production';
    }

    const config = require("./webpack.config.js")
    return webpack(merge(config, opts), onBuild(done));
})

gulp.task("hot", ["resx2json", "vendor"], (done) => {
    const webpackDevServer = require("webpack-dev-server")
    
    process.env.NODE_ENV = "hot"

    const config = require('./webpack.config.js');
    const options = config.devServer;

    webpackDevServer.addDevServerEntrypoints(config, options);
    const compiler = webpack(config);
    const server = new webpackDevServer(compiler, options);

    server.listen(config.devServer.port, 'localhost', () => {
        utils.log('Dev server listening on port ' + config.devServer.port + " for hot reload");
        utils.log(utils.colors.bgBlue("Waiting for webpack compilation"));
    });
})

gulp.task("test", (done) => {
    var exec = require('child_process').exec;
    exec('yarn test --ci', (err, stdout, stderr) => {
        utils.log(stdout);
        utils.log(stderr);
        done(err);
    });
})

gulp.task('resx2json', ["cleanup"], () => {
    return gulp.src([
        "../**/*.resx",
        "../../../../Core/BoundedContexts/Headquarters/WB.Core.BoundedContexts.Headquarters/Resources/*.resx"
    ])
        // .pipe(debug())
        .pipe(plugins.resx2json())
        .pipe(plugins.rename((filePath) => {
            filePath.extname = ".json";
            if (!filePath.basename.includes('.')) {
                filePath.basename += ".en";
            }
        }))
        .pipe(gulp.dest(config.resources.dest));
});

gulp.task('cleanup', (cb) => {
    if (utils.env.production || process.env.FORCE_CLEANUP) {
        rimraf.sync(config.dist + "/**/*.*")
        rimraf.sync(config.resources.dest + "/**/*.*")
        rimraf.sync(config.hqViews + "/partial.*.cshtml")
    }
    return cb();
});

gulp.task("vendor", ["cleanup"], (done) => {
    fs.stat("./dist/shared_vendor.manifest.json", (err) => {
        if (err) {
            utils.log(utils.colors.yellow("Building VENDOR DLL libs"));

            if (utils.env.production) {
                process.env.NODE_ENV = 'production';
            }

            const dllConfig = require("./webpack.vendor.config");

            webpack(dllConfig, done, "Shared vendor dll build done", true);
        } else {
            done()
        }
    });
});

gulp.task("watch", ['resx2json', 'vendor'], () => {
    const compiler = webpack(merge(require("./webpack.config.js"), {
        plugins: [new webpack.ProgressPlugin()]
    }));

    utils.log(utils.colors.green('Building and waiting for file changes'));

    const watcher = compiler.watch({
        ignored: ["node_modules"]
    }, onBuild(null, utils.colors.green("Build done. Waiting for changes"), false));

    return watcher;
});

function onBuild(done, onBuildMessage, exitOnError = true) {
    const moment = require("moment")

    return function (err, stats) {
        if (err) {
            utils.log('Error', err);
            if (exitOnError) throw "Build has failed"
        }
        else {
            const duration = moment.duration(stats.endTime - stats.startTime, "millisecond").asSeconds();
            utils.log(utils.colors.green("Build in"), utils.colors.magenta(duration + " s"));

            if (stats.hasErrors()) {
                utils.log(stats.toString());
                if (exitOnError) throw "Build has failed"
            }

            if (stats.hasWarnings()) {
                utils.log(stats.compilation.warnings);
            }

            if (stats.hasErrors() || stats.hasWarnings()) {
                utils.log(utils.colors.bgRed("Build completed with warnings"));
            } else if (onBuildMessage) {
                utils.log(onBuildMessage);
            }
        }

        if (done) {
            done();
        }
    }
}
