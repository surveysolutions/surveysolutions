const gulp = require("gulp");
const utils = require("gulp-util");
const rimraf = require("rimraf");
const plugins = require("gulp-load-plugins")();
const { spawn } = require("child_process");

const isDevMode = !utils.env.production;

const config = {
    dist: "dist",
    hqViews: "../Views/Shared",
    webTester: "../../../WB.UI.WebTester/Content/Dist"
};

config.resources = {
    source: "../",
    dest: "locale/.resources"
};

gulp.task("hot", [], cb => {
    run("yarn", ["serve"], cb);
});

gulp.task("build", [], cb => {
    run("yarn", isDevMode ? ["dev"] : ["build"], cb);
});

gulp.task("webTester", ["build"], function() {
    const filter = [
        "**/*.js",
        "**/locale/webtester/*.json",
        "!**/locale/hq/*",
        "!**/webinterview/*",
        "!**/hq.*.js",
        "!**/webinterview.*.js",
        "!**/review.*.js",
        isDevMode ? "**/*.map" : null,
        "**/*.css"
    ].filter(x => x);

    return gulp
        .src(config.dist + "/**/*.*")
        .pipe(plugins.filter(filter))
        .pipe(gulp.dest(config.webTester));
});

gulp.task("default", ["cleanup", "build", "webTester"].filter(x => x));

gulp.task("cleanup", cb => {
    if (!isDevMode || process.env.FORCE_CLEANUP) {
        rimraf.sync(config.dist + "/**/*.*");
        rimraf.sync(config.resources.dest + "/**/*.*");
        rimraf.sync(config.hqViews + "/partial.*.cshtml");
        rimraf.sync(config.webTester + "/**/*.*");
    }
    return cb();
});

function run(command, args, cb) {
    const cmd = spawn(command, args, {
        shell: true,
        stdio: "inherit"
    });

    cmd.on("exit", code => cb(code));
}
