import Vue from 'vue'
import i18next from 'i18next'
import i18NextApi from "i18next-http-backend"
import { browserLanguage } from './helpers'

function install(Vue, callback, opt = {}) {
    const locale = browserLanguage.split('-')[0]
                    
    const options = {
        debug: false,
        nsSeparator: '.',
        lng: locale,
        fallbackLocale: 'en',
        fallbackLng: 'en',
        backend: {
            loadPath: function(languages) {
                var key = 'QuestionnaireEditor.' + languages[0] + '.json'
                return window.localization[key]
            },
            allowMultiLoading: false
        },
        load: 'languageOnly',
        saveMissing: true,
        useCookie: false,
        useLocalStorage: false,
        interpolation: { escapeValue: false },

        missingKeyHandler(lng, ns, key, fallbackValue) {
            console.warn('Missing translation for language', lng, 'key',ns + '.' + fallbackValue)
        },
        
        parseMissingKeyHandler(key) {
            return '[' + key + ']'
        }
    }

    i18next
        .use(i18NextApi)
        .init(options,
        (error, t) => {
            if (callback !== undefined)
                callback(i18next, t);                              
    })

    Vue.$t = function() {
        return i18next.t.apply(i18next, arguments)
    }

    Vue.prototype.$t = function() {
        return i18next.t.apply(i18next, arguments) 
    }            
}

export default {
    install
};

