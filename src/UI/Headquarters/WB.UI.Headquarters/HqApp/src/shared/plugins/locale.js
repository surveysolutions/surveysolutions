import i18next from "i18next"
import i18nextXHRBackend from "i18next-xhr-backend"
import i18nextBrowserLanguageDetector from "i18next-browser-languagedetector"
import moment from "moment"

/*  the Plugin */
var VueI18Next = {
    install: function (Vue, options) {
        /*  determine options  */
        i18next
            .use(i18nextXHRBackend)
            .use(i18nextBrowserLanguageDetector);

        i18next.init(Object.assign({
            debug: false,
            //lng: 'de', // If not given, i18n will detect the browser language.
            fallbackLng: 'en', // Default is dev
            nsSeparator: '.',
            keySeparator: ':',
            load: 'languageOnly',
            backend: {
                loadPath: Vue.$config.basePath + 'dist/resources/{{ns}}.{{lng}}.json'
            },
            useCookie: false,
            useLocalStorage: false
        }, options))

        i18next.on('languageChanged', function (lng) {
            moment.locale(lng);
        });

        function translate(key, options) {
            if (_.isArray(options)) {
                const res = i18next.t(key);

                // js will not replace all occurences, and we have resources with several {0} in one string
                return res
                    .replace("{0}", options[0])
                    .replace("{0}", options[0])
                    .replace("{0}", options[0])
                    .replace("{1}", options[1])
                    .replace("{1}", options[1])
                    .replace("{1}", options[1])
                    .replace("{2}", options[2])
                    .replace("{3}", options[3])
                    .replace("{4}", options[4])
            } else {
                return i18next.t(key, options);
            }
        }

        // /*  expose a global API method  */
        Object.defineProperty(Vue, '$t', {
            get() {
                return translate
            }
        })

        /*  expose a local API method  */
        Object.defineProperty(Vue.prototype, '$t', {
            get() {
                return translate
            }
        })
    }
}

/*  export API  */
export default VueI18Next
