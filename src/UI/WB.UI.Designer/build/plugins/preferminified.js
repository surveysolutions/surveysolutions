const through2 = require("through2");
const vinylFile = require("vinyl-file");

module.exports = () =>
  through2.obj((file, _, done) => {
    const minFile = file.path.replace(".js", ".min.js");

    vinylFile
      .read(minFile)
      .then(f => done(null, f))
      .catch(error => {
        return done(null, file);
      });
  });
