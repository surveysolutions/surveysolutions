const glob = require("glob");
const basename = require("path").basename;
const extname = require("path").extname;
const gulpInject = require("gulp-inject");
const src = require("gulp").src;
const yargs = require("yargs");

function flattenObject(ob, delimiter = ".", defaultAsRoot = false) {
  var toReturn = {};

  for (var i in ob) {
    if (!ob.hasOwnProperty(i)) continue;

    if (typeof ob[i] == "object" && ob[i] !== null) {
      var flatObject = flattenObject(ob[i]);
      for (var x in flatObject) {
        if (!flatObject.hasOwnProperty(x)) continue;

        if (defaultAsRoot && x == 'default') {
          toReturn[i] = flatObject[x];
        } else {
          toReturn[i + delimiter + x] = flatObject[x];
        }
      }
    } else {
      toReturn[i] = ob[i];
    }
  }
  return toReturn;
}

const doInject = function(name, distFolder, options) {
  const opts = Object.assign(
    {
      name,
      quiet: true,
      ignorePath: "wwwroot"
    },
    options
  );

  const source = distFolder + "/**/" + name + "*.*";

  return gulpInject(src(source, { read: false }), opts);
};

exports.injectSections = (pipe, distFolder, options = {}) => {

  getSectionsList(distFolder).forEach(section => {
    pipe = pipe.pipe(doInject(section, distFolder, options));
  });

  return pipe;
};

exports.PRODUCTION = yargs.argv.production;

function getSectionsList(distFolder) {
  const items = glob.sync(distFolder + "/**/*.{js,css}");
  const set = new Set();

  items.forEach(file => {
    const name = basename(file, extname(file));
    const match = /([\w+]+)-?[\w\d]*/.exec(name);
    if (match != null) {
      set.add(match[1]);
    } else {
      set.add(name);
    }
  });

  return Array.from(set);
}

exports.flatten = flattenObject; //(obj) => Object.values(flattenObject(obj));
