import { join } from "path";
import { src, dest, parallel, series } from "gulp";
import appendPrepend from "gulp-append-prepend";
import cache from "gulp-cache";
import concat from "gulp-concat";
import gulpif from "gulp-if";
import gulpInject from "gulp-inject";
import htmlmin from "gulp-htmlmin";
import imagemin from "gulp-imagemin";
import less from "gulp-less";
import ngAnnotate from "gulp-ng-annotate";
import rename from "gulp-rename";
import rev from "gulp-rev";
import templateCache from "gulp-angular-templatecache";
import terser from "gulp-terser";
import yargs from "yargs";

import manifest from "./plugins/manifest";
import resx2json from "./plugins/resx2json";

import { questionnaire, dist } from "./config.json";

const PRODUCTION = yargs.argv.production;

export const templates = () =>
  src(questionnaire.htmls)
    .pipe(htmlmin({ collapseWhitespace: false }))
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
    .pipe(manifest({ base: "resx" }))
    .pipe(dest("node_modules/.cache"));

export const styles = () =>
  src(questionnaire.markup)
    .pipe(appendPrepend.appendText('@icon-font-path: "/dist/fonts/";'))
    .pipe(less({ relativeUrls: true, rootpath: "dist/" }))
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
  src(["questionnaire/details/index.html"]).pipe(
    gulpInject(filesToInject("libs*.css"))
  );

export default series(
  parallel(templates, styles, scripts, staticContent, ResourcesFromResx)
  
);

function filesToInject(mask) {
  return src(join(dist, mask), { read: false });
}
