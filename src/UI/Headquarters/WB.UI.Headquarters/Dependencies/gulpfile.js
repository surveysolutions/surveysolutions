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
    loginDir: '../Views/Account/',
    loginPath: '../Views/Account/LogIn.cshtml',
    cssSource: './css/markup.scss',
    cssInjectSectionName: 'markup',
    jsInjectSectionName: 'libs'
};

gulp.task('styles', function () {
    return gulp.src(config.cssSource)
        .pipe(sass())
        .pipe(autoprefixer('last 2 version'))
        .pipe(gulp.dest(config.buildDir))
        .pipe(rename({ suffix: '.min' }))
        .pipe(plugins.rev())
        .pipe(cssnano())
    	.pipe(gulp.dest(config.buildDir));
});

gulp.task('bowerJs', function () {
    return gulp.src(mainBowerFiles('**/*.js'))
        .pipe(debug())
        .pipe(plugins.ngAnnotate())
      	.pipe(concat('libs.js'))
        .pipe(gulp.dest(config.buildDir))
        .pipe(rename({ suffix: '.min' }))
        .pipe(plugins.uglify())
        .pipe(plugins.rev())
    	.pipe(gulp.dest(config.buildDir));
});

gulp.task('login', ['styles', 'bowerJs'], function () {
    var target = gulp.src(config.loginPath);
    var cssMarkup = gulp.src(config.buildDir + '/markup-*.min.css', { read: false });
    var jsLibs = gulp.src(config.buildDir + '/libs-*.min.css', { read: false });

    if (config.production) {
        return target
            .pipe(plugins.inject(cssMarkup, { relative: true, name: config.cssInjectSectionName }))
            .pipe(plugins.inject(jsLibs, { relative: true, name: config.jsInjectSectionName }))
            .pipe(gulp.dest(config.loginDir));
    }

    return cssMarkup.pipe(debug());
});

gulp.task('clean', function () {
    return gulp.src(config.buildDir + '/*').pipe(plugins.clean());
});

gulp.task('default', ['clean'], function () {
    gulp.start('styles', 'bowerJs', 'login');
});