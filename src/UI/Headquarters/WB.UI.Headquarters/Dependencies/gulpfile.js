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

var config = {
    production: !!util.env.production,
    buildDir: './build',
    loginPath: '../Views/Account/LogIn.cshtml',
    cssSource: './css/markup.scss',
    cssInjectSectionName: 'markup'
};

gulp.task('styles', function () {
    var target = gulp.src(config.cssSource);
    return target
        .pipe(sass())
        .pipe(autoprefixer('last 2 version'))
        .pipe(gulp.dest(config.buildDir))
        .pipe(rename({ suffix: '.min' }))
        .pipe(plugins.rev())
        .pipe(cssnano())
    	.pipe(gulp.dest(config.buildDir));
});

gulp.task('login', ['styles'], function () {
    var target = gulp.src(config.loginPath);
    var sources = gulp.src(config.buildDir + '/markup-*.min.css', { read: false });

    if (config.production) {
        return target
            .pipe(plugins.inject(sources, { relative: true, name: config.cssInjectSectionName }))
            .pipe(gulp.dest('../Views/Account/'));
    }

    return sources.pipe(debug());
});

gulp.task('clean', function () {
    return gulp.src(config.buildDir + '/*').pipe(plugins.clean());
});

gulp.task('default', ['clean'], function () {
    gulp.start('styles', 'login');
});