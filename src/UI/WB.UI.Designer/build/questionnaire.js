import { join } from "path";
import { src, dest, parallel, series } from "gulp";
import appendPrepend from "gulp-append-prepend";
import cache from "gulp-cache";
import concat from "gulp-concat";
import gulpif from "gulp-if";
import debug from "gulp-debug";
import htmlmin from "gulp-htmlmin";
import imagemin from "gulp-imagemin";
import less from "gulp-less";
import ngAnnotate from "gulp-ng-annotate";
import gulpInject from "gulp-inject";
import rename from "gulp-rename";
import rev from "gulp-rev";
import templateCache from "gulp-angular-templatecache";
import terser from "gulp-terser";

import manifest from "./plugins/manifest";
import resx2json from "./plugins/resx2json";

import { questionnaire, dist } from "./config.json";
import { injectSections, PRODUCTION } from "./plugins/helpers";

export const templates = () =>
  src(questionnaire.htmls)
    .pipe(cache(htmlmin({ collapseWhitespace: false })))
    .pipe(templateCache({ root: "views" }))
    .pipe(gulpif(PRODUCTION, rev()))
    .pipe(dest(join(dist, "js")));

export const ResourcesFromResx = () =>
  src(questionnaire.resources)
    .pipe(resx2json())
    .pipe(
      rename(function(filePath) {
        filePath.extname = ".json";
        if (!filePath.basename.includes(".")) {
          filePath.basename += ".en";
        }
      })
    )
    .pipe(rev())
    .pipe(dest(join(dist, "resx")))
    .pipe(manifest({ base: "/resx/" }))
    .pipe(dest("node_modules/.cache"));

export const styles = () =>
  src(questionnaire.markup)
    .pipe(appendPrepend.appendText('@icon-font-path: "/fonts/";'))
    .pipe(cache(less({ relativeUrls: true, rootpath: "/" })))
    .pipe(gulpif(PRODUCTION, rev()))
    .pipe(dest(join(dist, "css")));

export const staticContent = () =>
  src(questionnaire.static, { base: "questionnaire/content" })
    .pipe(gulpif(f => f.extname == ".png", cache(imagemin())))
    .pipe(dest(dist));

export const scripts = () =>
  src(questionnaire.scripts)
    .pipe(gulpif(PRODUCTION, ngAnnotate()))
    .pipe(gulpif(PRODUCTION, cache(terser())))
    .pipe(concat("app.js"))
    .pipe(gulpif(PRODUCTION, rev()))
    .pipe(dest(join(dist, "js")));

export const inject = () =>
  injectSections(src("questionnaire/index.cshtml"), dist, { quiet: true })
  .pipe(
    gulpInject(src("node_modules/.cache/rev-manifest.json"), {
      name: "manifest",
      transform(_, file) {
        return (
          "<script>window.localization = " +
          file.contents.toString("utf8") +
          "</script>"
        );
      }
    })
  )
  .pipe(dest(f => f.base));

export default series(
  parallel(templates, styles, scripts, staticContent, ResourcesFromResx),
  inject//,
  //inject_manifest
);
