const gulp = require('gulp');
const plugins = require('gulp-load-plugins')();
const path = require('path');
const mainBowerFiles = require('main-bower-files');
const runSequence = require('run-sequence');
const concat = require('gulp-concat');
const utils = require('gulp-util')
const debug = require('gulp-debug');
const appendPrepend = require('gulp-append-prepend');
const minifyHTML = require('gulp-htmlmin');
const templateCache = require("gulp-angular-templatecache");
const fs = require("fs");
const isDevMode = !utils.env.production

const paths = {
  scripts: ['details/scripts/**/*.js'],
  htmls: ['details/views/**/*.html'],
  vendor: [
    'node_modules/angular-block-ui/dist/angular-block-ui.css',
    'node_modules/angular-hotkeys/build/hotkeys.css',
    'node_modules/angular-loading-bar/build/loading-bar.css',
    'node_modules/perfect-scrollbar/dist/css/perfect-scrollbar.css',
    'node_modules/pnotify/dist/pnotify.css',
    'node_modules/pnotify/dist/pnotify.buttons.css',
    'node_modules/angular-ui-tree/dist/angular-ui-tree.css',
    'node_modules/jquery/dist/jquery.js',
    'node_modules/ace-builds/src-noconflict/ace.js',
    'node_modules/ace-builds/src-noconflict/ext-language_tools.js',
    'node_modules/angular/angular.js',
    'node_modules/angular-animate/angular-animate.js',
    'node_modules/angular-block-ui/dist/angular-block-ui.js',
    'node_modules/angular-ui-bootstrap/dist/ui-bootstrap.js',
    'node_modules/angular-ui-bootstrap/dist/ui-bootstrap-tpls.js',
    'node_modules/angular-cookies/angular-cookies.js',
    'node_modules/angular-elastic/elastic.js',
    'node_modules/angular-hotkeys/build/hotkeys.js',
    'node_modules/angular-loading-bar/build/loading-bar.js',
    'node_modules/perfect-scrollbar/dist/js/perfect-scrollbar.jquery.js',
    'node_modules/pnotify/dist/pnotify.js',
    'node_modules/pnotify/dist/pnotify.animate.js',
    'node_modules/pnotify/dist/pnotify.buttons.js',
    'node_modules/pnotify/dist/pnotify.callbacks.js',
    'node_modules/pnotify/dist/pnotify.confirm.js',
    'node_modules/pnotify/dist/pnotify.desktop.js',
    'node_modules/pnotify/dist/pnotify.history.js',
    'node_modules/pnotify/dist/pnotify.mobile.js',
    'node_modules/pnotify/dist/pnotify.nonblock.js',
    'node_modules/bootstrap/dist/js/bootstrap.js',
    'node_modules/angular-resource/angular-resource.js',
    'node_modules/angular-sanitize/angular-sanitize.js',
    'node_modules/angular-ui-tree/dist/angular-ui-tree.js',
    'node_modules/angular-ui-utils/modules/highlight/highlight.js',
    'details/scripts/modules/unsavedChanges.js',
    'node_modules/console-shim/console-shim.js',
    'node_modules/es5-shim/es5-shim.js',
    'node_modules/jquery-mousewheel/jquery.mousewheel.js',
    'node_modules/jquery-placeholder/jquery.placeholder.js',
    'node_modules/jquery.scrollTo/jquery.scrollTo.js',
    'node_modules/json3/lib/json3.js',
    'details/scripts/modules/ng-context-menu.js',
    'node_modules/ng-file-upload/dist/ng-file-upload.js',
    'node_modules/ng-file-upload/dist/ng-file-upload-shim.js',
    'node_modules/moment/moment.js',
    'node_modules/moment/locale/ru.js',
    'node_modules/moment/locale/fr.js',
    'node_modules/moment/locale/pt.js',
    'node_modules/moment/locale/zh-cn.js',
    'details/scripts/modules/positionCalculator.js',
    'node_modules/underscore/underscore.js',
    'node_modules/js-cookie/src/js.cookie.js',
    'node_modules/angular-ui-router/release/angular-ui-router.js',
    'node_modules/xss/dist/xss.js',
    'node_modules/html5shiv/dist/html5shiv.js',
    'details/scripts/modules/ui-ace.js',
    'external/jquery.validate.unobtrusive.bootstrap.js',
    'node_modules/jquery-validation/dist/jquery.validate.js',
    'details/scripts/modules/perfect_scrollbar.js',
    'node_modules/angular-pnotify/src/angular-pnotify.js',
    'node_modules/angular-moment/angular-moment.js',
    'node_modules/xss/dist/xss.js',
    'node_modules/i18next/i18next.min.js',
    'node_modules/i18next-xhr-backend/i18nextXHRBackend.min.js',
    'node_modules/ng-i18next/dist/ng-i18next.js',
    'node_modules/jquery-datepicker/jquery-datepicker.js'
  ]  
};

paths.vendor.forEach((f) => {
  fs.statSync(f); // ensure that all files are existing
});

gulp.task('clean', function	(){
	return gulp.src('build/*')
		.pipe(plugins.clean());
});

gulp.task("styles", function(){
  return gulp.src('content/markup.css.less')
      .pipe(appendPrepend.appendText('@icon-font-path: "/fonts/";'))
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
    return gulp.src(paths.vendor)
        .pipe(plugins.filter(['**/*.css']))
        //.pipe(debug())
        .pipe(concat('libs.css'))
        .pipe(plugins.rev())
        .pipe(gulp.dest('build'));
});

gulp.task("bowerJs", function(){
    return gulp.src(paths.vendor)
      .pipe(plugins.filter(['**/*.js']))
      //.pipe(debug({ title: 'unicorn:' }))
      .pipe(plugins.ngAnnotate())
      .pipe(plugins.uglify())
      .pipe(concat('libs.js'))
      .pipe(plugins.rev())
      .pipe(gulp.dest('build'));
});

gulp.task('templates', function () {
    return gulp.src(paths.htmls)
      .pipe(minifyHTML({collapseWhitespace: false}))
      .pipe(templateCache({ root: 'views' }))
      .pipe(plugins.rev())
      .pipe(gulp.dest('build'));
});

gulp.task('resx2json', function(){
    gulp.src([
        '../Resources/QuestionnaireEditor.resx',
        '../Resources/QuestionnaireEditor.*.resx'])
      .pipe(plugins.resx2json())
      .pipe(plugins.rename(function(filePath) {
          filePath.extname = ".json";
          if (!filePath.basename.includes('.')){
            filePath.basename += ".en";
          }
      }))
      .pipe(isDevMode ? utils.noop() : plugins.rev())
      .pipe(gulp.dest('build/resources'))
      .pipe(plugins.rev.manifest())
      .pipe(gulp.dest('build'));
});

gulp.task('copyFilesNeededForBundler', function(){
  gulp.src([
    'node_modules/perfect-scrollbar/dist/css/perfect-scrollbar.css',
    'node_modules/jquery/dist/jquery.min.js',
    'node_modules/bootstrap/dist/js/bootstrap.min.js',
    'node_modules/moment/min/moment-with-locales.min.js',
    'node_modules/jquery-validation/dist/jquery.validate.js',
    'node_modules/jquery-mousewheel/jquery.mousewheel.js',
    'node_modules/perfect-scrollbar/dist/js/perfect-scrollbar.jquery.js',
    'node_modules/jquery.fancytree/dist/jquery.fancytree-all-deps.min.js',
    'node_modules/jquery.fancytree/3rd-party/extensions/contextmenu/js/jquery.fancytree.contextMenu.js',
    'node_modules/jquery.fancytree/dist/skin-bootstrap/ui.fancytree.min.css',
    'node_modules/jquery-contextmenu/dist/jquery.contextMenu.min.js',
    'node_modules/jquery-contextmenu/dist/jquery.contextMenu.min.css',
    'node_modules/bootbox/bootbox.min.js',
	'node_modules/bootstrap-select/dist/js/bootstrap-select.min.js',
    'node_modules/bootstrap-select/dist/css/bootstrap-select.min.css'
  ])
    //.pipe(debug({ title: 'copyFilesNeededForBundler task:' }))
    .pipe(gulp.dest('../Content/plugins'));
});

gulp.task('copyFontsNeededForBundler', function(){
  gulp.src([
    'node_modules/jquery-contextmenu/dist/font/**'
  ])
    .pipe(gulp.dest('../Content/plugins/font'));
});

gulp.task('devJs', function () {
    return gulp.src(paths.scripts)
      //.pipe(debug({ title: 'unicorn:' }))
      //.pipe(plugins.jshint())
      //.pipe(plugins.jshint.reporter('default'))
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

  return target.pipe(plugins.inject(gulp.src('./build/libs*.css', { read: false }), { relative: true, name: 'libs' }))
               .pipe(plugins.inject(sources, { relative: true }))
               .pipe(plugins.inject(gulp.src("./build/rev-manifest.json"),
                {
                  name: 'manifest',
                  transform: function(filePath, file) {
                    return '<script>window.localization = ' + file.contents.toString('utf8') + '</script>'
                  }
               }))

    		   .pipe(gulp.dest('./details'));
});

gulp.task("dev", ["resx2json", "copyFilesNeededForBundler", "copyFontsNeededForBundler"]);

gulp.task('default', function(callback){
	runSequence('clean', 
		['templates', 'devJs', 'bowerJs', 'styles', 'bowerCss', 'resx2json', 'copyFilesNeededForBundler', 'copyFontsNeededForBundler'],
		'index', 
		callback);
});
