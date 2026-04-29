import i18next from 'i18next';
import LanguageDetector from 'i18next-browser-languagedetector';
import moment from 'moment';

function loadLocaleMessages() {
    const locales = import.meta.glob('../locale/*.json', { eager: true });

    const messages = {};

    for (const item in locales) {
        const matched = item.match(/([A-Za-z0-9-_]+)\./i);
        if (matched && matched.length > 1) {
            const locale = matched[1];

            messages[locale] = {
                translation: Object.assign(
                    messages[locale] || {},
                    locales[item]
                )
            };
        }
    }
    return messages;
}

const messages = loadLocaleMessages();

const userLang = navigator.language || navigator.userLanguage;
const userLocale = userLang.split('-')[0];

i18next.use(LanguageDetector).init({
    debug: false,
    lng: userLocale || import.meta.env.VUE_APP_I18N_LOCALE || 'en',
    fallbackLng: import.meta.env.VUE_APP_I18N_FALLBACK_LOCALE || 'en',
    /*backend: {
        loadPath: function(languages) {
            var key = 'QuestionnaireEditor.' + languages[0] + '.json';
            return window.localization[key];
        }
    },*/
    load: 'languageOnly',
    useCookie: false,
    useLocalStorage: false,
    resources: messages
});

// i18next v26 removed interpolation.format; register custom formatters via the Formatter API.
// These handle moment.js format strings used in locale keys (e.g. {{dateTime, H:mm}}).

// Custom (non-moment) formatters. Add new entries here as needed.
const customFormatters = {
    uppercase: (value) => String(value).toUpperCase(),
};

for (const [name, fn] of Object.entries(customFormatters)) {
    i18next.services.formatter.add(name, fn);
}

// Dynamically scan all loaded locale values for {{var, format}} patterns and register
// a moment-based formatter for every unique format spec found.  This way adding a new
// format string to any locale file works automatically without touching this file.
// null/undefined are returned as-is, matching the previous interpolation.format fallthrough.
// Formats already registered in customFormatters are excluded automatically.
function collectFormats(obj, found = new Set(), exclude = new Set()) {
    if (typeof obj === 'string') {
        const re = /\{\{[^,}]+,\s*([^}]+?)\s*\}\}/g;
        let m;
        while ((m = re.exec(obj)) !== null) {
            if (!exclude.has(m[1])) found.add(m[1]);
        }
    } else if (obj && typeof obj === 'object') {
        for (const v of Object.values(obj)) collectFormats(v, found, exclude);
    }
    return found;
}

const momentFormats = collectFormats(messages, new Set(), new Set(Object.keys(customFormatters)));
momentFormats.forEach((fmt) => {
    i18next.services.formatter.add(fmt, (value) => {
        if (moment.isDate(value) || moment.isMoment(value))
            return moment(value).format(fmt);
        return value;
    });
});

const instance = i18next;

export default instance;

export const i18n = instance;
