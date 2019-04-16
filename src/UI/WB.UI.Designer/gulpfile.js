const { series, parallel, watch } = require("gulp");
const path = require("path");

const vendor = require("./build/vendor");
const questionnaire = require("./build/questionnaire");
const bundler = require("./build/bundler");

const config = require("./build/config");
const rimraf = require("rimraf");
const helper = require("./build/plugins/helpers");

const cleanup = done => rimraf(path.join(config.dist, "**/*"), done);

module.exports = Object.assign(
  helper.flatten(
    {
      vendor,
      questionnaire,
      bundler
    }, ":", true
  ),
  {
    default: series(
      cleanup,
      parallel(vendor.default, bundler.default),
      questionnaire.default
    ),
    cleanup
  }
);
