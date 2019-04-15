import glob from "glob";
import { basename, extname } from "path";
import gulpInject from "gulp-inject";
import { src } from "gulp";
import yargs from "yargs";

function flattenObject(ob) {
  var toReturn = {};

  for (var i in ob) {
    if (!ob.hasOwnProperty(i)) continue;

    if (typeof ob[i] == "object" && ob[i] !== null) {
      var flatObject = flattenObject(ob[i]);
      for (var x in flatObject) {
        if (!flatObject.hasOwnProperty(x)) continue;

        toReturn[i + "." + x] = flatObject[x];
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

export const injectSections = (pipe, distFolder, options = {}) => {
  getSectionsList(distFolder).forEach(section => {
    pipe = pipe.pipe(doInject(section, distFolder, options));
  });

  return pipe;
};

export const PRODUCTION = yargs.argv.production;

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

export const flatten = flattenObject; //(obj) => Object.values(flattenObject(obj));
