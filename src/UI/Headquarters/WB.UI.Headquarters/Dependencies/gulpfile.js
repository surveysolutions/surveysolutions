﻿
const source = require("vinyl-source-stream");

const gulp = require('gulp'),
    streamify = require('gulp-streamify'),
    fs = require('fs'),
    plugins = require('gulp-load-plugins')(),
    concat = require('gulp-concat'),
    uglify = require('gulp-uglify'),
    sass = require('gulp-sass'),
    autoprefixer = require('gulp-autoprefixer'),
    util = require('gulp-util'),
    debug = require('gulp-debug'),
    rename = require('gulp-rename'),
    glob = require('glob'),
    es = require('event-stream'),
    cleanCSS = require('gulp-clean-css'),
    _ = require('lodash');

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
        function onSuccess() { done(); }

        function onError(err) { done(err); }

        var outStream = taskFn(onSuccess, onError);
        if (outStream && typeof outStream.on === 'function') {
            outStream.on('end', onSuccess);
        }
    }
}

const config = {
    production: !!util.env.production,
    bootstrapFontFiles: './vendor/bootstrap-sass/assets/fonts/bootstrap/*.*',
    fontsDir: './fonts',
    buildDir: './build',
    buildDistDir: './dist',
    filesToInject: [
        {
            file: "LogOn.cshtml",
            folder: '../Views/Account/'
        },
        {
            file: "_AdminLayout.cshtml",
            folder: '../Views/Shared/'
        },
        {
            file: "_MainLayout.cshtml",
            folder: '../Views/Shared/'
        },
        {
            file: "_MainLayoutHq.cshtml",
            folder: '../Views/Shared/'
        },
        {
            file: "Review.cshtml",
            folder: '../Views/Interview/'
        },
        {
            file: "Index.cshtml",
            folder: '../Views/WebInterview/'
        }
    ],
    cssFilesToWatch: './css/*.scss"',
    cssSource: './css/markup.scss',
    cssAppInject: 'cssApp',
    cssLibsInject: 'cssLibs',
    jsLibsInject: 'jsLibs',
    css: {
        markup: "./css/markup.scss",
        ["markup-web-interview"]: "./css/markup-web-interview.scss",
        ["markup-specific"]: "./css/markup-specific.scss"
    }
};

config.sourceFiles = [
    'node_modules/jquery/dist/jquery.js',
    'node_modules/bootstrap-sass/assets/stylesheets/_bootstrap.scss',
    'node_modules/bootstrap-sass/assets/javascripts/bootstrap.js',
    config.production
        ? 'node_modules/knockout/build/output/knockout-latest.js'
        : 'node_modules/knockout/build/output/knockout-latest.debug.js',
    config.production
        ? 'node_modules/knockout-mapping/dist/knockout.mapping.min.js'
        : 'node_modules/knockout-mapping/dist/knockout.mapping.js',
    'node_modules/moment/min/moment-with-locales.min.js',
    'node_modules/lodash/lodash.js',
    'node_modules/datatables.net/js/jquery.dataTables.js',
    config.production
        ? 'node_modules/datatables.net-select/js/dataTables.select.min.js'
        : 'node_modules/datatables.net-select/js/dataTables.select.js',
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
];

function compressJs(error) {
    return config.production
        ? plugins.uglify().on('error', error)
        : util.noop()
}

function compressCss(error) {
    return config.production
        ? cleanCSS({ compatibility: 'ie9' }).on('error', error)
        : util.noop()
}

gulp.task("checkSources", (done, error) => {
    // check that all provided files exists on disk
    config.sourceFiles.forEach((f) => {
        fs.statSync(f);
    });
    done();
});

gulp.task('move-bootstrap-fonts', wrapPipe(function (success, error) {
    return gulp.src(config.bootstrapFontFiles)
        .pipe(gulp.dest(config.fontsDir).on('error', error));
}));

function compileCss(sources, error) {
    return gulp.src(sources)
        .pipe(sass().on('error', error))
        .pipe(autoprefixer('last 2 version').on('error', error))
        .pipe(gulp.dest(config.buildDir).on('error', error))
        .pipe(rename({ suffix: '.min' }).on('error', error))
        .pipe(compressCss(error))
        .pipe(plugins.rev().on('error', error))
        .pipe(gulp.dest(config.buildDistDir));
}

gulp.task('styles:markup', ['move-bootstrap-fonts'], wrapPipe(function (success, error) {
    return compileCss(config.css.markup, error);
}));

gulp.task('styles:webinterview', ['move-bootstrap-fonts'], wrapPipe(function (success, error) {
    return compileCss(config.css["markup-web-interview"], error)
}));

gulp.task('styles:specific', ['move-bootstrap-fonts'], wrapPipe(function (success, error) {
    return compileCss(config.css["markup-specific"], error)
}));

gulp.task('styles', [
    'styles:markup',
    'styles:specific',
    'styles:webinterview',
    'libsCss']);

gulp.task('libsJs', ["checkSources"], wrapPipe((success, error) => {
    return gulp.src(config.sourceFiles)
        .pipe(plugins.filter(['**/*.js']))
        // .pipe(debug())
        .pipe(concat('libs.js').on('error', error))
        .pipe(gulp.dest(config.buildDir).on('error', error))
        .pipe(rename({ suffix: '.min' }).on('error', error))
        .pipe(compressJs(error))
        .pipe(plugins.rev().on('error', error))
        .pipe(gulp.dest(config.buildDistDir));
}));

gulp.task('libsCss', ["checkSources"], wrapPipe(function (success, error) {
    return gulp.src(config.sourceFiles)
        .pipe(plugins.filter('**/*.css'))
        .pipe(autoprefixer('last 2 version').on('error', error))
        .pipe(concat('libs.css').on('error', error))
        .pipe(gulp.dest(config.buildDir).on('error', error))
        .pipe(rename({ suffix: '.min' }).on('error', error))
        .pipe(cleanCSS({ compatibility: 'ie9' }))
        .pipe(plugins.rev().on('error', error))
        .pipe(gulp.dest(config.buildDistDir));
}));

gulp.task('inject', ['styles', 'libsJs'],
    wrapPipe((success, error) => {
        if (!config.production) return util.noop();

        function transform(filepath) {
            const url = '@Url.Content("~/Dependencies' + filepath + '")';

            if (filepath.endsWith(".css")) {
                return "<link rel='stylesheet' async href='" + url + "' >";
            }
            else if (filepath.endsWith(".js")) {
                return " <script type='text/javascript' src='" + url + "'></script>";
            }

            return inject.transform.apply(inject.transform, arguments);
        }

        var cssLibs = gulp.src(config.buildDistDir + '/libs-*.min.css', { read: false });
        var jsLibs = gulp.src(config.buildDistDir + '/libs-*.min.js', { read: false });

        function inject(files, name) {
            const options = { relative: false, name, transform };
            return plugins.inject(files, options).on('error', error);
        }

        var tasks = config.filesToInject.map(function (fileToInject) {
            var target = gulp.src(fileToInject.folder + fileToInject.file);

            var pipe = target
                .pipe(inject(cssLibs, config.cssLibsInject))
                .pipe(inject(jsLibs, config.jsLibsInject))

            Object.keys(config.css).forEach(key => {
                const conf = config.css[key];
                
                const injectKey = "css_" + _.camelCase(key); // css_markup, css_markupWebInterview, css_markupSpecific
                const cssApp = gulp.src(config.buildDistDir + '/' + key + '-[a-f0-9]*.min.css', { read: false });

                pipe = pipe.pipe(inject(cssApp, injectKey))
            })

            pipe = pipe.pipe(gulp.dest(fileToInject.folder));

            return pipe
        });

        return tasks;
    }));

gulp.task('clean', function () {
    return gulp.src(config.buildDistDir + '/*').pipe(plugins.clean());
});

gulp.task('default', ['clean'], function () {
    gulp.start('move-bootstrap-fonts', 'styles', 'libsJs', 'inject');
});
