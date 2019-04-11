import through2 from "through2";
import Vinyl from "vinyl";
import vinylFile from "vinyl-file";
import path from "path";

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
          const json = JSON.stringify(content, null, 4);
          const contents = Buffer.from(json);
          file.contents = contents;
          cb(null, file);
        });
    }
  );
}

export default manifest;
