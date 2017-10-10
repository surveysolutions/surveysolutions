const gulp = require("gulp")
const webpack = require("webpack")
const utils = require('gulp-util')
const merge = require('webpack-merge');
const rimraf = require("rimraf");
const debug = require('gulp-debug');
const plugins = require('gulp-load-plugins')();
const jest = require('jest-cli');

const config = {
    dist: 'dist',
    hqViews: '../Views/Shared',
}

config.resources = {
    source: '../',
    dest: "locale/.resources"
}

gulp.task("test", (done) => {
    var exec = require('child_process').exec;
    exec('yarn test --ci', (err, stdout, stderr) => {
        utils.log(stdout);
        utils.log(stderr);
        done(err);
      });
})

gulp.task('resx2json', () => {
    const files = gulp.src([
        "../**/*.resx",
        "../../../../Core/BoundedContexts/Headquarters/WB.Core.BoundedContexts.Headquarters/Resources/*.resx"
    ])
        //.pipe(debug())
        .pipe(plugins.resx2json())
        .pipe(plugins.rename((filePath) => {
            filePath.extname = ".json";
            if (!filePath.basename.includes('.')) {
                filePath.basename += ".en";
            }
        }))

    return files.pipe(
        gulp.dest(config.resources.dest)
    );
});

gulp.task('cleanup', (cb) => {
   // if (utils.env.production) {
        rimraf.sync(config.dist + "/**/*.*")
        rimraf.sync(config.resources.dest + "/**/*.*")
        rimraf.sync(config.hqViews + "/partial.*.cshtml")
    //}
    return cb();
});

gulp.task("build", (done) => {
    const opts = {
        plugins: []
    }

    if (!utils.env.production) {
        opts.plugins.push(new webpack.ProgressPlugin());
    } else {
        process.env.NODE_ENV = 'production';
    }

    return webpack(merge(require("./webpack.config.js"), opts), onBuild(done));
})

gulp.task("default", ['cleanup', 'resx2json', 
    utils.env.production ? "test" : null].filter((x) => x), () => {
   return gulp.start("build");
});

gulp.task("watch", ['resx2json'], () => {
    const compiler = webpack(merge(require("./webpack.config.js"), {
        plugins: [new webpack.ProgressPlugin()]
    }));

    utils.log(utils.colors.green('Building and waiting for file changes'));

    const watcher = compiler.watch({
        ignored: ["node_modules"]
    }, onBuild(null, utils.colors.green("Build done. Waiting for changes")));

    return watcher;
});

function onBuild(done, onBuildMessage) {
    const moment = require("moment")

    return function (err, stats) {
        if (err) {
            utils.log('Error', err);
        }
        else {
            const duration = moment.duration(stats.endTime - stats.startTime, "millisecond").asSeconds();
            utils.log(utils.colors.green("Build in"), utils.colors.magenta(duration + " s"));
            
            if(stats.hasErrors()){
                utils.log(stats.compilation.errors);
            }
            
            if(stats.hasWarnings()){
                utils.log(stats.compilation.warnings);
            }

            if (!stats.hasErrors && !stats.hasWarnings && onBuildMessage){
                utils.log(onBuildMessage);                
            } else {
                utils.log(utils.colors.bgRed("Build completed with warnings"));
            }

        }

        if (done) {
            done();
        }
    }
}
