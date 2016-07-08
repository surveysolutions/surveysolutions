gulp-css-url-adjuster
=====================

This package allows gulp to change css urls

css file:

    .cool-background {
        background-image: url('coolImage.jpg');
    }

    var urlAdjuster = require('gulp-css-url-adjuster');

    gulp.src('style.css').
      pipe(urlAdjuster({
        prepend: '/image_directory/',
        append: '?version=1',
      }))
      .pipe(gulp.dest('modifiedStyle.css'));


    .cool-background {
        background-image: url('/image_directory/coolImage.jpg?version=1');
    }


only adjust relative paths:

    .cool-background {
        background-image: url('coolImage.jpg');
    }

    .neato-background {
        background-image: url('/images/neatoImage.jpg');
    }

    gulp.src('style.css').
      pipe(urlAdjuster({
        prependRelative: '/image_directory/',
      }))
      .pipe(gulp.dest('modifiedStyle.css'));


    .cool-background {
        background-image: url('/image_directory/coolImage.jpg');
    }

    .neato-background {
        background-image: url('/images/neatoImage.jpg');
    }

or replace path to another:

    .cool-background {
        background-image: url('/old/path/coolImage.jpg');
    }

    .neato-background {
        background-image: url('/old/path/images/neatoImage.jpg');
    }

    gulp.src('style.css').
      pipe(urlAdjuster({
        replace:  ['/old/path','/brand/new'],
      }))
      .pipe(gulp.dest('modifiedStyle.css'));


    .cool-background {
        background-image: url('/brand/new/coolImage.jpg');
    }

    .neato-background {
        background-image: url('/brand/new/images/neatoImage.jpg');
    }


