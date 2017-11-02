import Vue from 'vue'
import VueI18n from 'vue-i18n'
import BaseFormatter from '~/shared/localization/customFormatter'
Vue.use(VueI18n)

export default {
    initialize(browserLanguage) {
        const locale = browserLanguage.split('-')[0];

        const options = {
            locale,
            formatter: new BaseFormatter(),
            fallbackLocale: 'en',
            messages: {
                [locale]: window.CONFIG.locale.data
            }
        }

        const i18n = new VueI18n(options);
        Vue.use(i18n);

        // setting up global access to $t function
        Vue.$t = function() {
            return i18n.t.apply(i18n, arguments);
        }

        return i18n;
    }
}
