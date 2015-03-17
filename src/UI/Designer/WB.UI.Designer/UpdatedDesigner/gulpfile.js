var gulp = require('gulp');
var plugins = require('gulp-load-plugins')();
var path = require('path');
var mainBowerFiles = require('main-bower-files');
var runSequence = require('run-sequence');
var concat = require('gulp-concat');
var debug = require('gulp-debug');

var paths = {
  scripts: ['app/scripts/**/*.js'],
  styles: ['content/markup.css.less']
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

gulp.task("bowerJs", function(){
    return gulp.src(mainBowerFiles())
        .pipe(plugins.filter(['*.js']))
        .pipe(debug({ title: 'unicorn:' }))
    	.pipe(plugins.ngAnnotate())
    	.pipe(plugins.uglify())
      	.pipe(concat('libs.js'))
      	.pipe(plugins.rev())
      	.pipe(gulp.dest('build'));
});

gulp.task('devJs', function () {
    return gulp.src(paths.scripts)
      .pipe(debug({ title: 'unicorn:' }))
      .pipe(plugins.jshint())
      .pipe(plugins.jshint.reporter('default'))
      .pipe(plugins.ngAnnotate())
      .pipe(plugins.uglify())
      .pipe(concat('app.js'))
      .pipe(plugins.rev())
      .pipe(gulp.dest('build'));
});

gulp.task('index', function () {
  var target = gulp.src('app/index.html');
  // It's not necessary to read the files (will speed up things), we're only after their paths:
  var sources = gulp.src(['./build/libs*.js',
  	'./build/app*.js',
  	'./build/*.js',
  	'./build/vendor*.css',
  	'./build/markup*.css',
  	'./build/*.css'], {read: false});

  return target.pipe(plugins.inject(sources, {relative: true}))
    		   .pipe(gulp.dest('./app'));
});

gulp.task('default', function(callback){
	runSequence('clean', 
		['devJs', 'bowerJs', 'styles'], 
		'index', 
		callback);
});