const join = require("path").join;
const src = require("gulp").src;
const dest = require("gulp").dest;
const parallel = require("gulp").parallel;
const autoprefixer = require("autoprefixer");
const cache = require("gulp-cache");
const cleanCss = require("gulp-clean-css");
const merge = require("merge2");

const concat = require("gulp-concat");
const filter = require("gulp-filter");
const gulpif = require("gulp-if");
const postcss = require("gulp-postcss");
const rev = require("gulp-rev");
const sourcemaps = require("gulp-sourcemaps");
const terser = require("gulp-terser");
const config = require("./config.json");
const PRODUCTION = require("./plugins/helpers").PRODUCTION;

const bowerCss = () =>
  src(config.vendor.files)
    .pipe(filter(["**/*.css"]))
    .pipe(gulpif(!PRODUCTION, sourcemaps.init()))
    .pipe(gulpif(PRODUCTION, postcss([autoprefixer])))
    .pipe(gulpif(PRODUCTION, cleanCss({ compatibility: "ie8" })))
    .pipe(gulpif(!PRODUCTION, sourcemaps.write()))
    .pipe(concat("libs.css"))
    .pipe(gulpif(PRODUCTION, rev()))
    .pipe(dest(join(config.dist, "css")));

const bowerJs = () =>
  src(config.vendor.files)
    .pipe(filter(["**/*.js"]))
    .pipe(gulpif(PRODUCTION, cache(terser())))
    .pipe(concat("libs.js"))
    .pipe(gulpif(PRODUCTION, rev()))
    .pipe(dest(join(config.dist, "js")));

const bowerImages = () =>
  src(config.vendor.files)
    .pipe(filter(["**/*.png"]))
    .pipe(gulpif(PRODUCTION, rev()))
    .pipe(dest(join(config.dist, "images")));

const favicons = () =>
  src(config.vendor.favicons)
    .pipe(dest(config.dist));

const assets = () =>
  merge(
    src(config.assets.images)
      .pipe(dest(join(config.dist, "images"))),
    src(config.assets.fonts).pipe(dest(join(config.dist, "fonts"))),
    src(config.assets.qbank)
      .pipe(dest(join(config.dist, "qbank")))
  );

module.exports = {
  assets,
  favicons,
  bowerCss,
  bowerImages,
  bowerJs,
  default: parallel(bowerJs, favicons, bowerCss, bowerImages, assets)
};
