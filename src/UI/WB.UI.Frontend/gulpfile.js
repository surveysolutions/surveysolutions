const path = require("path");
const baseDir = path.resolve(__dirname, "./");
const join = path.join.bind(path, baseDir);

const gulp = require("gulp");
const utils = require("gulp-util");
const rimraf = require("rimraf");
const plugins = require("gulp-load-plugins")();
const { spawn } = require("child_process");
const uiFolder = join("..");
const isDevMode = !utils.env.production;
const getChunks = require("./tools/chunks")

const config = {
    dist: "dist",
    hqViews: path.join(uiFolder, "WB.UI.Headquarters.Core/Views/Shared"),
    webTester: path.join(uiFolder, "WB.UI.WebTester/Content/Dist"),
    hq: path.join(uiFolder, "WB.UI.Headquarters.Core/wwwroot/static")
};

config.resources = {
    source: "../",
    dest: "locale/.resources"
};

gulp.task("hot", [], cb => {
    run("yarn", ["serve"], cb, );
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


gulp.task("hq", ["build"], function() {

    const filter = [
        "**/*.js",
        "**/locale/hq/*.json",
        "**/locale/webinterview/*.json",
        "!**/locale/webtester/*",
        isDevMode ? "**/*.map" : null,
        "**/*.css"
    ].filter(x => x);

    return gulp
        .src(config.dist + "/**/*.*")
        .pipe(plugins.filter(filter))
        .pipe(gulp.dest(config.hq));
});

gulp.task("default", ["cleanup",  "build", "webTester", "hq"].filter(x => x));

gulp.task("cleanup", cb => {
    if (!isDevMode || process.env.FORCE_CLEANUP) {
        rimraf.sync(config.dist + "/**/*.*");
        rimraf.sync(config.resources.dest + "/**/*.*");
        rimraf.sync(config.hqViews + "/partial.*.cshtml");
        rimraf.sync(config.webTester + "/**/*.*");
        rimraf.sync(config.hq + "/**/*.*");
    }
    return cb();
});

function run(command, args, cb, workingDir) {
    const cwd = workingDir == null ? process.cwd() : workingDir
    
    const cmd = spawn(command, args, {
        shell: true,
        stdio: "inherit",
        cwd
    });

    cmd.on("exit", code => cb(code));
}
