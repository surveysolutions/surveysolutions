var gulp = require('gulp'),
    plugins = require('gulp-load-plugins')(),
    concat = require('gulp-concat'),
    uglify = require('gulp-uglify'),
    del = require('del'),
    mainBowerFiles = require('main-bower-files'),
    sass = require('gulp-sass'),
    autoprefixer = require('gulp-autoprefixer'),
    cssnano = require('gulp-cssnano'),
    rename = require('gulp-rename');

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

gulp.task('login', function () {
    var target = gulp.src('../Views/Account/LogIn.cshtml');
    var sources = gulp.src('./dist/markup-*.min.css', { read: false }).pipe(plugins.debug());
    return target
        .pipe(plugins.inject(sources, { relative: true }))
    	.pipe(gulp.dest('../Views/Account/'));
});

gulp.task('clean', function () {
    return gulp.src('build/*').pipe(plugins.clean());
});

gulp.task('default', ['clean'], function () {
    gulp.start('styles', 'login');
});