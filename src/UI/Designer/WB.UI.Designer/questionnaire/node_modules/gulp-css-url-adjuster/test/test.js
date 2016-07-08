var gutil = require('gulp-util');
var test = require('tap').test;
var fs = require('fs');
var path = require('path');

var cssAdjuster = require('../index');

var read = function(name) {
  return fs.readFileSync(path.join(__dirname, name));
};

var testContents = read('test.css');

test('prepends url', function(t) {
  var stream = cssAdjuster({
    prepend: 'prepend/'
  });

  stream.write(new gutil.File({
    contents: read('test.css')
  }));

  stream.once('data', function(file) {
    t.equal(file.contents.toString('utf8').trim(),
            read('expected-prepend.css').toString().trim(),
           'prepend');
  });

  stream.end();

  t.end();
  
});

test('appends url function', function(t) {
  var stream = cssAdjuster({
    append: function(s) { return s + '?append'; }
  });

  stream.write(new gutil.File({
    contents: read('test.css')
  }));

  stream.once('data', function(file) {
    t.equal(file.contents.toString('utf8').trim(),
            read('expected-append.css').toString().trim());
  });

  stream.end();

  t.end();
  
});

test('appends url', function(t) {
  var stream = cssAdjuster({
    append: '?append'
  });

  stream.write(new gutil.File({
    contents: read('test.css')
  }));

  stream.once('data', function(file) {
    t.equal(file.contents.toString('utf8').trim(),
            read('expected-append.css').toString().trim());
  });

  stream.end();

  t.end();
  
});

test('appends and prepend url', function(t) {
  var stream = cssAdjuster({
    prepend: 'prepend/',
    append: '?append'
  });

  stream.write(new gutil.File({
    contents: read('test.css')
  }));

  stream.once('data', function(file) {
    t.equal(file.contents.toString('utf8').trim(),
            read('expected-prepend-append.css').toString().trim());
  });

  stream.end();

  t.end();
  
});

test('prepends relative url', function(t) {
  var stream = cssAdjuster({
    prependRelative: 'prepend/',
  });

  stream.write(new gutil.File({
    contents: read('test.css')
  }));

  stream.once('data', function(file) {
    t.equal(file.contents.toString('utf8').trim(),
            read('expected-relative-prepend.css').toString().trim());
  });

  stream.end();

  t.end();
});


