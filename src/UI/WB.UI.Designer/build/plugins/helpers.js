const glob = require('glob');
const basename = require('path').basename;
const extname = require('path').extname;
const gulpInject = require('gulp-inject');
const src = require('gulp').src;
const yargs = require('yargs');
const fs = require('fs');

function flattenObject(ob, delimiter = '.', defaultAsRoot = false) {
    var toReturn = {};

    for (var i in ob) {
        if (!ob.hasOwnProperty(i)) continue;

        if (typeof ob[i] == 'object' && ob[i] !== null) {
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

const doInject = function(section, distFolder, options) {
    let name;  
    let spath;
    if(section.name != null) {
      name = section.name
      spath = section.source
    } else {
      name = section
      spath = section
    }

    const opts = Object.assign(
        {
            name,
            quiet: false,
            addRootSlash: false,
            addPrefix: '~',
            ignorePath: 'wwwroot'
        },
        options
    );

    const source = distFolder + '/**/' + spath + '*.*';

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
    let items = [];

    items = items.concat(glob.sync(distFolder + '/js/*'));
    items = items.concat(glob.sync(distFolder + '/css/*'));
    const set = new Set();

    items.forEach(file => {
        const name = basename(file, extname(file));

        if (fs.statSync(file).isDirectory()) {
            set.add({ name, source: name + '/**/'});
        } else {
            const match = /([\w+]+)-?[\w\d]*/.exec(name);
            if (match != null) {
                set.add(match[1]);
            } else {
                set.add(name);
            }
        }
    });

    return Array.from(set);
}

exports.flatten = flattenObject; //(obj) => Object.values(flattenObject(obj));
