const gulp = require("gulp")
const webpack = require("webpack")
const utils = require('gulp-util')
const merge = require('webpack-merge');
const rimraf = require("rimraf");
//const debug = require('gulp-debug');
const plugins = require('gulp-load-plugins')();

const config = {
    dist: 'dist',
}

config.resources = {
    source: '../',
    dest: `${config.dist}/resources`
}

gulp.task('resx2json', () => {
    const files = gulp.src(["../**/*.resx"])
        //        .pipe(debug())
        .pipe(plugins.resx2json())
        .pipe(plugins.rename(function (filePath) {
            filePath.extname = ".json";
            if (!filePath.basename.includes('.')) {
                filePath.basename += ".en";
            }
        }))
        .pipe(plugins.flatten());

    return files.pipe(
        gulp.dest(config.resources.dest)
    );
});

gulp.task('cleanup', (cb) => {
    if (utils.env.production) {
        rimraf.sync(config.dist + "/**/*.*")
    }
    return cb();
})

function onBuild(done, onBuildMessage) {
    const moment = require("moment")

    return function(err, stats) {
      if(err) {
        utils.log('Error', err);
      }
      else {
          const duration = moment.duration(stats.endTime - stats.startTime, "millisecond").asSeconds();
          utils.log(utils.colors.green("Build in"), utils.colors.magenta(duration + " s"));
          if(onBuildMessage) utils.log(onBuildMessage);
        //utils.log(stats.toString());
      }
  
      if(done) {
        done();
      }
    }
  }

gulp.task("build", (done) => {
    const opts = {
        plugins: []
    }

    if (!utils.env.production) {
        opts.plugins.push(
            new webpack.ProgressPlugin()
        );
    } else {
        process.env.NODE_ENV = 'production';
    }

    webpack(merge(require("./webpack.config.js"), opts), onBuild(done));
})

gulp.task("default", ['cleanup', 'resx2json', 'build',]);

gulp.task("watch", ['cleanup', 'resx2json'], (done) => {
    const compiler = webpack(merge(require("./webpack.config.js"), {
        plugins: [new webpack.ProgressPlugin()]
    }));
    
    utils.log(utils.colors.green('Building and waiting for file changes'));
    
    const watcher = compiler.watch({
       ignored: ["node_modules"] 
    }, onBuild(null, utils.colors.green("Build done. Waiting for changes")));

    return watcher;
});