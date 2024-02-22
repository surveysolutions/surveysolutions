import i18next from 'i18next';
import LanguageDetector from 'i18next-browser-languagedetector';
import moment from 'moment';

function loadLocaleMessages() {
    const locales = import.meta.glob('../locale/*.json', { eager: true });
    //const locales = import.meta.globEager('../locale/*.json');

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
    fallbackLng: userLocale || import.meta.env.VUE_APP_I18N_LOCALE || 'en',
    /*backend: {
        loadPath: function(languages) {
            var key = 'QuestionnaireEditor.' + languages[0] + '.json';
            return window.localization[key];
        }
    },*/
    load: 'languageOnly',
    useCookie: false,
    useLocalStorage: false,
    interpolation: {
        format: function(value, format, lng) {
            if (format === 'uppercase') return value.toUpperCase();
            if (moment.isDate(value) || moment.isMoment(value))
                return moment(value).format(format);
            return value;
        }
    },
    resources: messages
});

const instance = i18next;

export default instance;

export const i18n = instance;
