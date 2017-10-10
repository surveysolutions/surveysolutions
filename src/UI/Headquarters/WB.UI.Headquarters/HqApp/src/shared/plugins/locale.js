import Vue from 'vue'
import VueI18n from 'vue-i18n'
import BaseFormatter from 'shared/localization/customFormatter'
Vue.use(VueI18n)

import api from 'shared/api'
import { browserLanguage } from "shared/helpers"

export default {
    initializeAsync() {
        const locale = browserLanguage.split('-')[0];
        return api.resources.locale(locale)
            .then(messages => {

                const options = {
                    locale,
                    formatter: new BaseFormatter(),
                    fallbackLocale: 'en',
                    messages: {
                        [locale]: messages.data
                    }
                }

                const i18n = new VueI18n(options);
                Vue.use(i18n);
                  // /*  expose a global API method  */
                Object.defineProperty(Vue, '$t', {
                    get() {
                        return i18n.t;
                    }
                })
                return i18n;
            });
    }
}