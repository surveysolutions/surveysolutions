#[gulp](https://github.com/gulpjs/gulp)-rewrite-css

[![NPM version][npm-image]][npm-url]
[![Build Status][travis-image]][travis-url]
[![Coverage Status][coveralls-image]][coveralls-url]
[![Dependency Status][depstat-image]][depstat-url]
[![devDependency Status][devdepstat-image]][devdepstat-url]

A gulp plugin that allows rewriting url & @import references in CSS

## Information

| Package      | gulp-rewrite-css                                        |
|--------------|---------------------------------------------------------|
| Description  | Rewrite `url(…)` and `@import` references in CSS files. |
| Node version | >= 0.10                                                 |

## Installation

```console
npm install gulp-rewrite-css --save-dev
```

## Usage

```javascript
var gulp = require('gulp');
var rewriteCSS = require('gulp-rewrite-css');

gulp.task('my-rewrite', function() {
  var dest = './dist/';
  return gulp.src('./static/css/*.css')
    .pipe(rewriteCSS({destination:dest}))
    .pipe(gulp.dest(dest));
});
```
### Options
* `destination` (required, String) - the target directory for the processed CSS. Paths are rewritten relatively to that directory.
* `[debug]` (optional, boolean, defaults to false) - whether to log what gulp-rewrite-css is doing
* `[adaptPath]` (optional, Function, defaults to the internal rewriting method of gulp-rewrite-css) - will be passed a context hash that contains the following options:

	| key              | description                                                              |
	|------------------|--------------------------------------------------------------------------|
	| `sourceDir`      | the path in which the currently processed CSS file resides in            |
	| `sourceFile`     | the path to the currently processed CSS file                             |
	| `destinationDir` | the path of the target directory where the CSS file ends in              |
	| `targetFile`     | the path of the target file (e.g. the contents of `url(…)` or `@import`) |

## License

MIT (c) 2015 Joscha Feth <joscha@feth.com>

[npm-url]: https://npmjs.org/package/gulp-rewrite-css
[npm-image]: http://img.shields.io/npm/v/gulp-rewrite-css.svg

[travis-url]: https://travis-ci.org/joscha/gulp-rewrite-css
[travis-image]: http://img.shields.io/travis/joscha/gulp-rewrite-css.svg

[coveralls-url]: https://coveralls.io/r/joscha/gulp-rewrite-css
[coveralls-image]: http://img.shields.io/coveralls/joscha/gulp-rewrite-css.svg
[coveralls-original-image]: https://coveralls.io/repos/joscha/gulp-rewrite-css/badge.png

[depstat-url]: https://david-dm.org/joscha/gulp-rewrite-css
[depstat-image]: https://david-dm.org/joscha/gulp-rewrite-css.svg?theme=shields.io

[devdepstat-url]: https://david-dm.org/joscha/gulp-rewrite-css#info=devDependencies
[devdepstat-image]: https://david-dm.org/joscha/gulp-rewrite-css/dev-status.svg?theme=shields.io
