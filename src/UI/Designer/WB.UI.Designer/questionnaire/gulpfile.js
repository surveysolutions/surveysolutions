var gulp = require('gulp');
var plugins = require('gulp-load-plugins')();
var path = require('path');
var mainBowerFiles = require('main-bower-files');
var runSequence = require('run-sequence');
var concat = require('gulp-concat');
var debug = require('gulp-debug');

var minifyHTML = require('gulp-minify-html');
var templateCache = require("gulp-angular-templatecache");

var paths = {
  scripts: ['details/scripts/**/*.js'],
  styles: ['content/markup.css.less'],
  htmls: ['details/views/**/*.html']
};

gulp.task('clean', function	(){
	return gulp.src('build/*')
		.pipe(plugins.clean());
});

gulp.task("styles", function(){
	return gulp.src(paths.styles)
	    .pipe(plugins.less({
	    	relativeUrls: true
	    }))
      .pipe(plugins.cssUrlAdjuster({
        prepend: '../content/'
      }))
	    .pipe(plugins.rev())
	    .pipe(gulp.dest('build'));
});

gulp.task("bowerCss", function () {
    return gulp.src(mainBowerFiles('**/*.css'))
        .pipe(concat('libs.css'))
	    .pipe(plugins.rev())

	    .pipe(gulp.dest('build'));
    
});

gulp.task("bowerJs", function(){
    return gulp.src(mainBowerFiles('**/*.js'))
//        .pipe(debug({ title: 'unicorn:' }))
    	.pipe(plugins.ngAnnotate())
    	.pipe(plugins.uglify())
      	.pipe(concat('libs.js'))
      	.pipe(plugins.rev())
      	.pipe(gulp.dest('build'));
});

gulp.task('templates', function () {
    return gulp.src(paths.htmls)
      //.pipe(minifyHTML())
      .pipe(templateCache({ root: 'views' }))
      .pipe(plugins.rev())
      .pipe(gulp.dest('build'));
});

gulp.task('devJs', function () {
    return gulp.src(paths.scripts)
      //.pipe(debug({ title: 'unicorn:' }))
      .pipe(plugins.jshint())
      .pipe(plugins.jshint.reporter('default'))
      .pipe(plugins.ngAnnotate())
      .pipe(plugins.uglify())
      .pipe(concat('app.js'))
      .pipe(plugins.rev())
      .pipe(gulp.dest('build'));
});

gulp.task('index', function () {
  var target = gulp.src('details/index.cshtml');
  // It's not necessary to read the files (will speed up things), we're only after their paths:
  var sources = gulp.src(['./build/libs*.js',
  	'./build/app*.js',
  	'./build/*.js',
  	'./build/vendor*.css',
  	'./build/markup*.css',
  	'./build/*.css',
    '!./build/libs*.css' ], { read: false });

  return target.pipe(plugins.inject(gulp.src('./build/libs*.css', { read: false }), { name: 'libs' }))
               .pipe(plugins.inject(sources, { relative: true }))
    		   .pipe(gulp.dest('./details'));
});

gulp.task('default', function(callback){
	runSequence('clean', 
		['templates', 'devJs', 'bowerJs', 'styles', 'bowerCss'],
		'index', 
		callback);
});