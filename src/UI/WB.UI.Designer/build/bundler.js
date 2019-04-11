import { join } from "path";
import { src, dest, series } from "gulp";
import cache from "gulp-cache";
import concat from "gulp-concat";
import filter from "gulp-filter";
import gulpif from "gulp-if";
import less from "gulp-less";
import merge from "merge2";
import rev from "gulp-rev";
import terser from "gulp-terser";
import yargs from "yargs";
import config from "./config.json";
import ngAnnotate from "gulp-ng-annotate";

import { injectSections } from "./plugins/helpers";

const PRODUCTION = yargs.argv.production;

const bundler = () => {
  return merge(
    config.bundle.map(bundle => {
      return merge(
        src(bundle.inputFiles)
          .pipe(filter("**/*.js"))
          .pipe(gulpif(PRODUCTION, ngAnnotate()))
          .pipe(gulpif(PRODUCTION, cache(terser({mangle: true, compress: true}))))
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

const inject = () =>
  injectSections(src(config.injectTargets), config.dist)
    .pipe(dest(f => f.base));

export default series(bundler, inject);
