const fs = require("fs");
const path = require("path");
const crypto = require('crypto');

function getHash(content){
    return crypto.createHash('sha1').update(content).digest("hex").substring(0, 12);
}

function applyNamespace(namespace, messages) {
    const result = {};

    Object.keys(messages).forEach((key) => {
        result[namespace + "." + key] = messages[key];
    })

    return result;
}

function readFiles(dirname, onFileContent, onError) {
    var glob = require("glob");

    var files = glob.sync(dirname + "/**/*.json");
    files.forEach((file) => {
        const fileContent = fs.readFileSync(file, 'utf-8');
        onFileContent(path.basename(file), fileContent);
    })
}

function parseFilename(filename, content) {
    const split = filename.split('.');
    return {
        namespace: split[0],
        lang: split[1]
    }
}

function addDefaultLocaleValues(locales, def) {
    const defaultMessages = locales[def];

    Object.keys(locales).forEach((locale) => {
        if (locale == def) return;

        Object.keys(defaultMessages).forEach((namespace) => {
            if (!locales[locale][namespace]) {
                locales[locale][namespace] = defaultMessages[namespace];
            }

            Object.keys(defaultMessages[namespace]).forEach(key => {
                
                if (!locales[locale][namespace][key]) {
                    locales[locale][namespace][key] = defaultMessages[namespace][key];
                }
            })
        });
    });

    return locales;
}

function mergeNamespaces(namespace, initial, newone) {

    Object.keys(initial).forEach((key) => {
        if(newone[key] != null){
            console.error("Found conflicting resources with same resource name", namespace, key, initial[key], newone[key]);
        }
    });

    return Object.assign(initial, newone);
}

function readLocalizationData(locales) {
    const data = {};

    readFiles('./locale/.resources/', (filename, content) => {
        const { lang, namespace } = parseFilename(filename);

        if (!locales.includes(namespace)) return;

        data[lang] = data[lang] || {}
        data[lang][namespace] = mergeNamespaces(namespace, data[lang][namespace] || {}, JSON.parse(content));

    }, (err) => { throw err; });

    return data;
}

function ensureDirectoryExistence(filePath) {
    var dirname = path.dirname(filePath);
    if (fs.existsSync(dirname)) {
        return true;
    }
    ensureDirectoryExistence(dirname);
    fs.mkdirSync(dirname);
}

module.exports = {
    /**
     * 
     * @param {*} config 
     * @returns localization info
     */
    buildLocalizationFiles(config) {
        const localizationInfo = {}

        Object.keys(config).forEach((key) => {
            const entryResult = readLocalizationData(config[key].locales)
            localizationInfo[key] = {}
            addDefaultLocaleValues(entryResult, "en")

            Object.keys(entryResult).forEach(lang => {
                const content = JSON.stringify(entryResult[lang]);
                const filename = 
                    process.env.NODE_ENV == "production" 
                    ? `${key}/${lang}.${getHash(content)}.js`
                    : `${key}/${lang}.js`

                const jsonPath = `./dist/${filename}`;
                
                ensureDirectoryExistence(jsonPath);
                fs.writeFileSync(jsonPath, `__setLocaleData__(${content})`);

                localizationInfo[key][lang] = filename;
            });
        });

        return localizationInfo;
    }
}
