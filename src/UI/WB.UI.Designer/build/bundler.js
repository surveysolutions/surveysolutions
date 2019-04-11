import { join, resolve, basename, extname } from "path";
import { src, dest, series } from "gulp";
import cache from "gulp-cache";
import concat from "gulp-concat";
import filter from "gulp-filter";
import glob from "glob";
import gulpif from "gulp-if";
import gulpInject from "gulp-inject";
import less from "gulp-less";
import merge from "merge2";
import rev from "gulp-rev";
import terser from "gulp-terser";
import through2 from "through2";
import yargs from "yargs";
import config from "./config.json";
const rewriteCSS = require('gulp-rewrite-css');

const PRODUCTION = yargs.argv.production;

const bundler = () => {
  return merge(
    config.bundle.map(bundle => {
      return merge(
        src(bundle.inputFiles)
          .pipe(filter("**/*.js"))
          .pipe(gulpif(PRODUCTION, cache(terser())))
          .pipe(concat(bundle.name + ".js"))
          .pipe(gulpif(PRODUCTION, rev()))
          .pipe(dest(join(config.dist, "js"))),
        src(bundle.inputFiles)
          .pipe(filter(["**/*.css", "**/*.less"]))
          .pipe(less())
          .pipe(concat(bundle.name + ".css"))
          .pipe(gulpif(PRODUCTION, rev()))
          .pipe(dest(join(config.dist, "css")))
      );
    })
  );
};

const doInject = name =>
  gulpInject(src(config.dist + "/**/" + name + "*.*", { read: false }), {
    name, quiet: true,
    ignorePath: "wwwroot"
  });

function getSectionsList() {
  const items = glob.sync(config.dist + "/**/*.{js,css}");
  const set = new Set();
  items.forEach(file => {
    let name = basename(file, extname(file));
    let match = /([\w+]+)-?[\w\d]*/.exec(name);
    if (match != null) {
      set.add(match[1]);
    } else set.add(name);
  });
  return Array.from(set);
}

const inject = () => {
  var injections = src(config.inject);

  getSectionsList().forEach(section => {
    injections = injections.pipe(doInject(section));
  });

  return injections.pipe(dest(f => f.base));
};

export default series(bundler, inject);
