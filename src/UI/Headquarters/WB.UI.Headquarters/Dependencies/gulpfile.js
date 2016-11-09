var gulp = require('gulp'),
    plugins = require('gulp-load-plugins')(),
    concat = require('gulp-concat'),
    uglify = require('gulp-uglify'),
    del = require('del'),
    mainBowerFiles = require('main-bower-files'),
    sass = require('gulp-sass'),
    autoprefixer = require('gulp-autoprefixer'),
    cssnano = require('gulp-cssnano'),
    util = require('gulp-util'),
    debug = require('gulp-debug'),
    rename = require('gulp-rename');

var config = {
    production: !!util.env.production
};

gulp.task('styles', function () {
    var target = gulp.src('./css/markup.scss');
    return target
        .pipe(sass())
        .pipe(autoprefixer('last 2 version'))
        .pipe(gulp.dest('./build'))
        .pipe(rename({ suffix: '.min' }))
        .pipe(plugins.rev())
        .pipe(cssnano())
    	.pipe(gulp.dest('./build'));
});

gulp.task('login', ['styles'], function () {
    var target = gulp.src('../Views/Account/LogIn.cshtml');
    var sources = gulp.src('./build/markup-*.min.css', { read: false });

    if (config.production) {
        return target
            .pipe(plugins.inject(sources, { relative: true, name: 'markup' }))
            .pipe(gulp.dest('../Views/Account/'));
    }

    return sources.pipe(debug());
});

gulp.task('clean', function () {
    return gulp.src('./build/*');//.pipe(plugins.clean());
});

gulp.task('default', ['clean'], function () {
    gulp.start('styles', 'login');
});