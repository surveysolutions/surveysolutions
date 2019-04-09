import { join } from "path";
import { src, dest, lastRun, parallel } from "gulp";
import autoprefixer from "autoprefixer";
import cache from "gulp-cache";
import cleanCss from "gulp-clean-css";
import concat from "gulp-concat";
import filter from "gulp-filter";
import gulpif from "gulp-if";
import postcss from "gulp-postcss";
import rev from "gulp-rev";
import sourcemaps from "gulp-sourcemaps";
import terser from "gulp-terser";
import yargs from "yargs";

import config from "./config.json";

const PRODUCTION = yargs.argv.production;

export const bowerCss = () =>
  src(config.vendor.files)
    .pipe(filter(["**/*.css"]))
    .pipe(gulpif(!PRODUCTION, sourcemaps.init()))
    .pipe(gulpif(PRODUCTION, postcss([autoprefixer])))
    .pipe(gulpif(PRODUCTION, cleanCss({ compatibility: "ie8" })))
    .pipe(gulpif(!PRODUCTION, sourcemaps.write()))
    .pipe(concat("libs.css"))
    .pipe(gulpif(PRODUCTION, rev()))
    .pipe(dest(join(config.dist, "css")));

export const bowerJs = () =>
  src(config.vendor.files)
    .pipe(filter(["**/*.js"]))
    .pipe(gulpif(PRODUCTION, cache(terser())))
    .pipe(concat("libs.js"))
    .pipe(gulpif(PRODUCTION, rev()))
    .pipe(dest(join(config.dist, "js")));

export const bowerImages = () =>
  src(config.vendor.files)
    .pipe(filter(["**/*.png"]))
    .pipe(gulpif(PRODUCTION, rev()))
    .pipe(dest(join(config.dist, "images")));

export const favicons = () => src(config.vendor.favicons).pipe(dest(config.dist));

export default parallel(bowerJs, favicons, bowerCss, bowerImages);
