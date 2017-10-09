import Vue from 'vue'
import VueI18n from 'vue-i18n'

Vue.use(VueI18n)

import api from 'shared/api'
import { browserLanguage } from "shared/helpers"

export default {
    async initializeAsync() {
        const locale = browserLanguage.split('-')[0];
        const messages = await api.resources.locale(locale)
        
        const options = {
            locale,
            fallbackLocale: 'en',
            messages: {
                [locale]: messages.data
            }
        }

        const i18n = new VueI18n(options);
        Vue.use(i18n);

        return i18n;
    }
}