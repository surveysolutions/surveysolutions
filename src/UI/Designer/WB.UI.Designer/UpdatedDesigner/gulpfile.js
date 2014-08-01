var gulp = require('gulp');
var plugins = require('gulp-load-plugins')();
var path = require('path');
var mainBowerFiles = require('main-bower-files');
var runSequence = require('run-sequence');

var paths = {
  scripts: ['app/scripts/**/*.js'],
  styles: ['content/markup.css.less', 'content/vendor.css.less']
};

gulp.task('clean', function	(){
	gulp.src('build/*')
		.pipe(plugins.clean());
});

gulp.task("styles", function(){
	gulp.src(paths.styles)
	    .pipe(plugins.less({
	    	relativeUrls: true
	    }))
	    .pipe(plugins.rewriteCss({destination:'build'}))
	    .pipe(plugins.replace('\\', '/'))
	    .pipe(plugins.minifyCss())
	    .pipe(plugins.rev())
	    .pipe(gulp.dest('build'));
});

gulp.task("bowerJs", function(){
    gulp.src(mainBowerFiles())
    	.pipe(plugins.filter(['*.js']))
    	.pipe(plugins.ngAnnotate())
    	.pipe(plugins.uglify())
      	.pipe(plugins.concat('libs.js'))
      	.pipe(plugins.rev())
      	.pipe(gulp.dest('build'));
});

gulp.task('devJs', function () {
   return gulp.src(paths.scripts)
      //.pipe(plugins.jshint())
      //.pipe(plugins.jshint.reporter('default'))
      .pipe(plugins.ngAnnotate())
      .pipe(plugins.uglify())
      .pipe(plugins.concat('app.js'))
      .pipe(plugins.rev())
      .pipe(gulp.dest('build'));
});

gulp.task('index', ['clean', 'bowerJs', 'styles', 'devJs'], function () {
  var target = gulp.src('app/index.html');
  // It's not necessary to read the files (will speed up things), we're only after their paths:
  var sources = gulp.src(['./build/libs*.js',
  	//'./vendor/angular-block-ui/angular-block-ui.min.js',
  	'./build/app*.js',
  	'./build/vendor*.css',
  	'./build/markup*.css'], {read: false});

  return target.pipe(plugins.inject(sources, {relative: true}))
    .pipe(gulp.dest('./app'));
});

gulp.task('default', function(callback){
	runSequence('clean', ['devJs', 'bowerJs', 'styles'], 'index', callback);
});