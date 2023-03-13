const join = require("path").join;
const src = require("gulp").src;
const dest = require("gulp").dest;
const parallel = require("gulp").parallel;
const series = require("gulp").series;
const appendPrepend = require("gulp-append-prepend");
const cache = require("gulp-cache");
const concat = require("gulp-concat");
const gulpif = require("gulp-if");
const debug = require("gulp-debug");
const htmlmin = require("gulp-htmlmin");
const less = require("gulp-less");
const ngAnnotate = require("gulp-ng-annotate-patched");
const gulpInject = require("gulp-inject");
const rename = require("gulp-rename");
const rev = require("gulp-rev");
const templateCache = require("gulp-angular-templatecache");
const terser = require("gulp-terser");
const cleanCss = require("gulp-clean-css");
const manifest = require("./plugins/manifest");
const resx2json = require("./plugins/resx2json");

const questionnaire = require("./config.json").questionnaire;
const dist = require("./config.json").dist;
const injectSections = require("./plugins/helpers").injectSections;
const PRODUCTION = require("./plugins/helpers").PRODUCTION;

const templates = () =>
  src(questionnaire.htmls)
    .pipe(cache(htmlmin({ collapseWhitespace: false })))
    .pipe(templateCache({ root: "views" }))
    .pipe(gulpif(PRODUCTION, rev()))
    .pipe(dest(join(dist, "js")));

const ResourcesFromResx = () =>
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
    .pipe(manifest({ base: "../../resx/" }))
    .pipe(dest("node_modules/.cache"));

const styles = () =>
  src(questionnaire.markup)
    .pipe(appendPrepend.appendText('@icon-font-path: "/fonts/";'))
    .pipe(less({ relativeUrls: true, rootpath: "../" }))
    .pipe(cache(cleanCss()))
    .pipe(gulpif(PRODUCTION, rev()))
    .pipe(dest(join(dist, "css")));

const staticContent = () =>
  src(questionnaire.static, { base: "questionnaire/content" })
    .pipe(dest(dist));

const scripts = () =>
  src(questionnaire.scripts)
    .pipe(gulpif(PRODUCTION, ngAnnotate()))
    .pipe(gulpif(PRODUCTION, cache(terser())))
    .pipe(gulpif(PRODUCTION, concat("app.js")))
    .pipe(gulpif(PRODUCTION, rev()))
    .pipe(
      dest(
          join(dist, PRODUCTION ? "js" : "js/app")
        )
    );

const inject = () =>
injectSections(src(["questionnaire/index.cshtml", "questionnaire-app/index.html" /* temp hack to inject styles and scripts*/]), dist, {
    quiet: true,
    
    //addPrefix: PRODUCTION ? "~" : "~/src",
    transform(filepath, file) {

      if (filepath.endsWith(".js") && filepath.indexOf("cat__") >= 0) {
        var fileName = filepath.slice(0, -3).substring(1);
        var varName = fileName.split("-")[0].split("/").join("_");        
        return '<script> var ' + varName + '_path =\'' + fileName + '\';</script>';
      }
      else if (filepath.endsWith(".js") && filepath.indexOf("libs") < 0) {
        return '<script defer src="' + filepath + '"></script>';
      }

      return gulpInject.transform.apply(gulpInject.transform, arguments);
    }
  })
    .pipe(
      gulpInject(src("node_modules/.cache/rev-manifest.json"), {
        name: "manifest",
        transform(_, file) {
          const json = file.contents.toString("utf8").replace(/\"/g, '""');
          const razor = '@{ var locales_json = @"'+ json +'"; }'
          return (
            "<script>window.localization = " +
            file.contents.toString("utf8") +
            "</script>" + razor
          );
        }
      })
    )
    .pipe(dest(f => f.base));

module.exports = {
  templates,
  styles,
  scripts,
  staticContent,
  ResourcesFromResx,
  inject,
  default: series(
    parallel(templates, styles, scripts, staticContent, ResourcesFromResx),
    inject
  )
};
