var gulp            = require('gulp');
var babel           = require('gulp-babel');
var browserSync     = require('browser-sync');
var rename          = require('gulp-rename');
var vueComponent    = require('gulp-vue-single-file-component');

const path = require("path")
const dist = require("./config.json").dist;
const PRODUCTION = require("./plugins/helpers").PRODUCTION;

gulp.task('scripts', () => gulp.src('./js/*.js')
    .pipe(babel({ plugins: ['@babel/plugin-transform-modules-amd'] }))
    .pipe(gulp.dest(path.join(dist, "js/vue-app")))
    .pipe(browserSync.stream())
);

gulp.task('vue', () => gulp.src('./js/components/*.vue')
    .pipe(vueComponent({ debug: !PRODUCTION, loadCssMethod: 'loadCss' }))
    .pipe(babel({ plugins: ['@babel/plugin-transform-modules-amd'] }))
    .pipe(rename({ extname: '.js' }))
    .pipe(gulp.dest(path.join(dist, "js/vue-app/components")))
    .pipe(browserSync.stream())
);

gulp.task('watch', () => {
    browserSync.init({
        server: {
            baseDir: './public'
        }
    });

    gulp.watch('./js/*.js', gulp.parallel('scripts'));
    gulp.watch('./js/components/*.vue', gulp.parallel('vue'));
});

module.exports = {
    default: gulp.parallel('scripts', 'vue')
};
