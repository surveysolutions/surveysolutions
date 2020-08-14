import Vue from 'vue'
import i18next from 'i18next'
import i18NextXHR from "i18next-xhr-backend"

export default {
    initialize(browserLanguage) {
        const locale = browserLanguage.split('-')[0]
       
        const options = {
            debug: false,
            lng: locale,
            fallbackLocale: 'en',
            fallbackLng: {'default': ['en']},
            backend: {
                loadPath: function(languages) {
                    var key = 'QuestionnaireEditor.' + languages[0] + '.json'
                    return window.localization[key]
                }
            },
            load: 'languageOnly',
            useCookie: false,
            useLocalStorage: false,
            interpolation: { escapeValue: false }
        }
        
        i18next.use(i18NextXHR).init(options)

        Vue.$t = function() {
            return i18next.t.apply(i18next, arguments)
        }

        Vue.prototype.$t = function() {
            return i18next.t.apply(i18next, arguments)
        }

        return i18next
    },
}
