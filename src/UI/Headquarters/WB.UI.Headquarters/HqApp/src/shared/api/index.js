import axios from 'axios'
import config from 'shared/config'

function resolve() {
    const args = Array.prototype.slice.call(arguments);
    args.unshift(config.basePath);

    const result = args
        .filter((x) => x != null && x != '')
        .map((x) => trimChar(x.trim(), '/'));

    return "/" + result.join('/');
}

function trimChar(string, charToRemove) {
    while (string.charAt(0) == charToRemove) {
        string = string.substring(1);
    }

    while (string.charAt(string.length - 1) == charToRemove) {
        string = string.substring(0, string.length - 1);
    }

    return string;
}

export default {
    resources: {
        /**
        * Get localization for specified locale
        * @param {*} path path to content
        */
        async locale(locale) {
            let localizationFile = config.locale.locales[locale];

            if (localizationFile == null) {
                localizationFile = config.locale.locales['en'];
            }

            if(localizationFile == null){
                throw "Cannot find default EN localization in CONFIG: " + JSON.stringify(config.locale.locales.to)
            }

            return await axios.get(resolve('hqapp/dist', localizationFile));
        }
    }
}