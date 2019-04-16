const through2 = require("through2");
const Vinyl = require("vinyl");
const vinylFile = require("vinyl-file");
const path = require("path");

const getManifestFile = opts =>
  vinylFile.read(opts.path, opts).catch(error => {
    if (error.code === "ENOENT") {
      return new Vinyl(opts);
    }

    throw error;
  });

function manifest({ base }) {
  const content = {};
  const opts = Object.assign({
    path: "rev-manifest.json",
    base: null
  });

  return through2.obj(
    ({ revOrigPath, basename }, _, cb) => {
      content[path.basename(revOrigPath)] = base + basename;
      cb();
    },
    function(cb) {
      if (Object.keys(content).length === 0) {
        cb();
        return;
      }

      return getManifestFile(opts).then(
        file => {
          const json = JSON.stringify(content);
          const contents = Buffer.from(json);
          file.contents = contents;
          cb(null, file);
        });
    }
  );
}

module.exports = manifest;
