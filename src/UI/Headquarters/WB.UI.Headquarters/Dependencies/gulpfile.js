
const source = require("vinyl-source-stream");

const gulp = require('gulp'),
    streamify = require('gulp-streamify'),
    plugins = require('gulp-load-plugins')(),
    concat = require('gulp-concat'),
    uglify = require('gulp-uglify'),
    sass = require('gulp-sass'),
    autoprefixer = require('gulp-autoprefixer'),
    cssnano = require('gulp-cssnano'),
    util = require('gulp-util'),
    debug = require('gulp-debug'),
    rename = require('gulp-rename'),
    glob = require('glob'),
    es = require('event-stream');

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
    sourceFiles: [
        'node_modules/jquery/dist/jquery.js',
        'node_modules/bootstrap-sass/assets/stylesheets/_bootstrap.scss',
        'node_modules/bootstrap-sass/assets/javascripts/bootstrap.js',
        'node_modules/knockout/dist/knockout.js',
        'node_modules/knockout-mapping/build/output/knockout.mapping-latest.js',
        'node_modules/moment/min/moment-with-locales.min.js',
        'node_modules/lodash/lodash.js',
        'node_modules/datatables.net/js/jquery.dataTables.js',
        'node_modules/datatables.net-select/js/dataTables.select.js',
        'node_modules/pnotify/dist/pnotify.css',
        'node_modules/pnotify/dist/pnotify.js',
        'node_modules/pnotify/dist/pnotify.animate.js',
        'node_modules/pnotify/dist/pnotify.brighttheme.css',
        'node_modules/pnotify/dist/pnotify.buttons.css',
        'node_modules/pnotify/dist/pnotify.buttons.js',
        'node_modules/pnotify/dist/pnotify.callbacks.js',
        'node_modules/pnotify/dist/pnotify.confirm.js',
        'node_modules/pnotify/dist/pnotify.desktop.js',
        'node_modules/pnotify/dist/pnotify.history.css',
        'node_modules/pnotify/dist/pnotify.history.js',
        'node_modules/pnotify/dist/pnotify.mobile.css',
        'node_modules/pnotify/dist/pnotify.mobile.js',
        'node_modules/pnotify/dist/pnotify.nonblock.js',
        'node_modules/bootstrap-select/less/bootstrap-select.less',
        'node_modules/bootstrap-select/dist/css/bootstrap-select.css',
        'node_modules/bootstrap-select/dist/js/bootstrap-select.js',
        'node_modules/jQuery-contextMenu/dist/jquery.contextMenu.js',
        'node_modules/jQuery-contextMenu/dist/jquery.contextMenu.css',
        'node_modules/jquery-highlight/jquery.highlight.js',
        'node_modules/flatpickr/dist/flatpickr.js',
        'node_modules/flatpickr/dist/flatpickr.css',
        'node_modules/datatables.net-responsive/js/dataTables.responsive.js',
        'vendor/jquery.validate.unobtrusive.bootstrap/jquery.validate.unobtrusive.bootstrap.js'
    ],
    fontsDir: './fonts',
    buildDir: './build',
    buildDistDir: './dist',
    filesToInject: [
        {
            file: "LogOn.cshtml",
            folder: '../Views/Account/'
        },
        {
            file: "_MainLayout.cshtml",
            folder: '../Views/Shared/'
        },
        {
            file: "_MainLayoutVue.cshtml",
            folder: '../Views/Shared/'
        },
        {
            file: "Start.cshtml",
            folder: '../Views/WebInterview/'
        }
    ],
    cssFilesToWatch: './css/*.scss"',
    cssSource: './css/markup.scss',
    webInterviewSource: './css/webinterview.scss',
    cssAppInject: 'cssApp',
    cssLibsInject: 'cssLibs',
    jsLibsInject: 'jsLibs'
};

gulp.task('move-bootstrap-fonts', wrapPipe(function (success, error) {
    return gulp.src(config.bootstrapFontFiles)
        .pipe(gulp.dest(config.fontsDir).on('error', error));
}));

gulp.task('styles', ['move-bootstrap-fonts'], wrapPipe(function (success, error) {
    return gulp.src(config.cssSource)
        .pipe(sass().on('error', error))
        .pipe(autoprefixer('last 2 version').on('error', error))
        .pipe(gulp.dest(config.buildDir).on('error', error))
        .pipe(rename({ suffix: '.min' }).on('error', error))
        .pipe(plugins.rev().on('error', error))
        //.pipe(cssnano().on('error', error))
        .pipe(gulp.dest(config.buildDistDir));
}));

gulp.task('styles.webinterview', ['move-bootstrap-fonts'], wrapPipe(function (success, error) {
    return gulp.src(config.webInterviewSource)
        .pipe(sass().on('error', error))
        .pipe(autoprefixer('last 2 version').on('error', error))
        .pipe(gulp.dest(config.buildDir).on('error', error))
        .pipe(rename({ suffix: '.min' }).on('error', error))
        .pipe(plugins.rev().on('error', error))
        //.pipe(cssnano().on('error', error))
        .pipe(gulp.dest(config.buildDistDir));
}));

gulp.task('libsJs', wrapPipe((success, error) => {
    return gulp.src(config.sourceFiles)
        .pipe(plugins.filter(['**/*.js', '!**/vue*.js', '!**/vee*.js']))
        .pipe(concat('libs.js').on('error', error))
        .pipe(gulp.dest(config.buildDir).on('error', error))
        .pipe(rename({ suffix: '.min' }).on('error', error))
        .pipe(config.production ? plugins.uglify().on('error', error) : util.noop())
        .pipe(plugins.rev().on('error', error))
        .pipe(gulp.dest(config.buildDistDir));
}));

gulp.task('libsCss', wrapPipe(function (success, error) {
    return gulp.src(config.sourceFiles)
        .pipe(plugins.filter('**/*.css'))
        .pipe(autoprefixer('last 2 version').on('error', error))
        //.pipe(cssnano().on('error', error))
        .pipe(concat('libs.css').on('error', error))
        .pipe(gulp.dest(config.buildDir).on('error', error))
        .pipe(rename({ suffix: '.min' }).on('error', error))
        .pipe(plugins.rev().on('error', error))
        .pipe(gulp.dest(config.buildDistDir));
}));

gulp.task('inject', ['styles', 'libsCss', 'libsJs'], wrapPipe(function (success, error) {
    if (config.production) {

        var toUrlContent = function (filepath) {
            const url = '@Url.Content("~/Dependencies' + filepath + '")';

            if (filepath.endsWith(".css")) {
                return "<link rel='stylesheet' href='" + url + "' >";
            }
            else if (filepath.endsWith(".js")) {
                return " <script type='text/javascript' src='" + url + "'></script>";
            }

            return inject.transform.apply(inject.transform, arguments);
        }

        var cssApp = gulp.src(config.buildDistDir + '/markup-*.min.css', { read: false });
        var cssLibs = gulp.src(config.buildDistDir + '/libs-*.min.css', { read: false });
        var jsLibs = gulp.src(config.buildDistDir + '/libs-*.min.js', { read: false });

        var tasks = config.filesToInject.map(function (fileToInject) {
            var target = gulp.src(fileToInject.folder + fileToInject.file);

            return target

                .pipe(plugins.inject(cssApp, { relative: false, name: config.cssAppInject, transform: toUrlContent }).on('error', error))
                .pipe(plugins.inject(cssLibs, { relative: false, name: config.cssLibsInject, transform: toUrlContent }).on('error', error))
                .pipe(plugins.inject(jsLibs, { relative: false, name: config.jsLibsInject, transform: toUrlContent }).on('error', error))
                .pipe(gulp.dest(fileToInject.folder));
        });

        return tasks;
    }
 
    return util.noop();
}));

gulp.task('clean', function () {
    return gulp.src(config.buildDistDir + '/*').pipe(plugins.clean());
});

gulp.task('css', ['styles', 'styles.webinterview']);

gulp.task('default', ['clean'], function () {
    gulp.start('move-bootstrap-fonts', 'styles', 'styles.webinterview', 'libsCss', 'libsJs', 'inject');
});
