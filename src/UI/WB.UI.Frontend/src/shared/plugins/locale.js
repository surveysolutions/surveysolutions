import Vue from 'vue'
import i18next from 'i18next'

export default {
    initialize(browserLanguage) {
        const locale = browserLanguage.split('-')[0]

        const options = {
            lng: locale,
            nsSeparator: '.',
            keySeparator: ':',
            fallbackLocale: 'en',
            resources: {
                [locale]: window.CONFIG.locale.data,
            },
            interpolation: { escapeValue: false, skipOnVariables: false },
            saveMissing: true,
            missingKeyHandler(lng, ns, key, fallbackValue) {
                console.warn('Missing translation for language', lng, 'key',ns + '.' + fallbackValue)
            },
            appendNamespaceToMissingKey: true,
            parseMissingKeyHandler(key) {
                return '[' + key + ']'
            },
        }
        i18next.init(options)

        Vue.$t = function() {
            return i18next.t.apply(i18next, arguments)
        }

        Vue.prototype.$t = function() {
            return i18next.t.apply(i18next, arguments)
        }

        return i18next
    },
}
