var gulp = require('gulp'),
    plugins = require('gulp-load-plugins')(),
    concat = require('gulp-concat'),
    uglify = require('gulp-uglify'),
    mainBowerFiles = require('main-bower-files'),
    sass = require('gulp-sass'),
    autoprefixer = require('gulp-autoprefixer'),
    cssnano = require('gulp-cssnano'),
    util = require('gulp-util'),
    debug = require('gulp-debug'),
    rename = require('gulp-rename');

// error handling https://medium.com/@boriscoder/catching-errors-on-gulp-js-4682eee2669f#.rh86s4ad2
/**
 * Wrap gulp streams into fail-safe function for better error reporting
 * Usage:
 * gulp.task('less', wrapPipe(function(success, error) {
 *   return gulp.src('less/*.less')
 *      .pipe(less().on('error', error))
 *      .pipe(gulp.dest('app/css'));
 * }));
 */

function wrapPipe(taskFn) {
    return function (done) {
        var onSuccess = function () {
            done();
        };
        var onError = function (err) {
            done(err);
        }
        var outStream = taskFn(onSuccess, onError);
        if (outStream && typeof outStream.on === 'function') {
            outStream.on('end', onSuccess);
        }
    }
}

var config = {
    production: !!util.env.production,
    bootstrapFontFiles: './vendor/bootstrap-sass/assets/fonts/bootstrap/*.*',
    fontsDir: './fonts',
    buildDir: './build',
    filesToInject: [
        {
            file: "LogOn.cshtml",
            folder: '../Views/Account/'
        },
        {
            file: "_MainLayout.cshtml",
            folder: '../Views/Shared/'
        }
    ],
    cssFilesToWatch: './css/*.scss"',
    //cssSource: './css/markup.scss',
    cssSource: './css/should_cause_error.scss',
    cssAppInject: 'cssApp',
    cssLibsInject: 'cssLibs',
    jsLibsInject: 'jsLibs'
};

gulp.task('move-bootstrap-fonts', wrapPipe(function (success, error) {
    return gulp.src(config.bootstrapFontFiles).pipe(gulp.dest(config.fontsDir));
}));

gulp.task('styles', ['move-bootstrap-fonts'], wrapPipe(function (success, error) {
    return gulp.src(config.cssSource)
        .pipe(sass().on('error', error))
        .pipe(autoprefixer('last 2 version').on('error', error))
        .pipe(gulp.dest(config.buildDir).on('error', error))
        .pipe(rename({ suffix: '.min' }).on('error', error))
        .pipe(plugins.rev().on('error', error))
        .pipe(cssnano().on('error', error))
    	.pipe(gulp.dest(config.buildDir));
}));

gulp.task('watch-styles', function () {
    gulp.watch(config.cssFilesToWatch, ['styles']);
});

gulp.task('bowerJs', wrapPipe(function (success, error) {
    return gulp.src(mainBowerFiles('**/*.js'))
        .pipe(plugins.ngAnnotate().on('error', error))
      	.pipe(concat('libs.js').on('error', error))
        .pipe(gulp.dest(config.buildDir).on('error', error))
        .pipe(rename({ suffix: '.min' }).on('error', error))
        .pipe(plugins.uglify().on('error', error))
        .pipe(plugins.rev().on('error', error))
    	.pipe(gulp.dest(config.buildDir));
}));

gulp.task('bowerCss', wrapPipe(function (success, error) {
    return gulp.src(mainBowerFiles('**/*.css'))
        .pipe(autoprefixer('last 2 version').on('error', error))
        .pipe(concat('libs.css').on('error', error))
        .pipe(gulp.dest(config.buildDir).on('error', error))
        .pipe(rename({ suffix: '.min' }).on('error', error))
        .pipe(cssnano().on('error', error))
        .pipe(plugins.rev().on('error', error))
    	.pipe(gulp.dest(config.buildDir));
}));

gulp.task('inject', ['styles', 'bowerCss', 'bowerJs'], wrapPipe(function (success, error) {
    if (config.production) {
        var cssApp = gulp.src(config.buildDir + '/markup-*.min.css', { read: false });
        var cssLibs = gulp.src(config.buildDir + '/libs-*.min.css', { read: false });
        var jsLibs = gulp.src(config.buildDir + '/libs-*.min.js', { read: false });

        var tasks = config.filesToInject.map(function (fileToInject) {
            var target = gulp.src(fileToInject.folder + fileToInject.file);

            return target
                .pipe(plugins.inject(cssApp, { relative: true, name: config.cssAppInject }).on('error', error))
                .pipe(plugins.inject(cssLibs, { relative: true, name: config.cssLibsInject }).on('error', error))
                .pipe(plugins.inject(jsLibs, { relative: true, name: config.jsLibsInject }).on('error', error))
                .pipe(gulp.dest(fileToInject.folder));
        });

        return tasks;
    }
    // DEV: to include files not in batch
    //else {
    //    var cssLibs = gulp.src(mainBowerFiles('**/*.css'));
    //    var jsLibs = gulp.src(mainBowerFiles('**/*.js'));

    //    var tasks = config.filesToInject.map(function (fileToInject) {
    //        var target = gulp.src(fileToInject.folder + fileToInject.file);

    //        return target
    //            .pipe(plugins.inject(cssLibs, {
    //                relative: true, name: config.cssLibsInject, transform: function (filepath) {
    //                    var newFileName = '@Url.Content("~/' + filepath.substring(6) + '")';
    //                    return '<link rel="stylesheet" href="' + newFileName + '" />';
    //                }
    //            }))
    //            .pipe(plugins.inject(jsLibs, {
    //                relative: true, name: config.jsLibsInject, transform: function (filepath) {
    //                    var newFileName = '@Url.Content("~/' + filepath.substring(6) + '")';
    //                    return '<script src="' + newFileName + '"></script>';
    //                }
    //            }))
    //            .pipe(gulp.dest(fileToInject.folder));
    //    });

    //    return tasks;
    //}

    return util.noop();
}));

gulp.task('clean', function () {
    return gulp.src(config.buildDir + '/*').pipe(plugins.clean());
});

gulp.task('default', ['clean'], function () {
    gulp.start(/*'watch-styles', */'move-bootstrap-fonts', 'styles', 'bowerCss', 'bowerJs', 'inject');
});