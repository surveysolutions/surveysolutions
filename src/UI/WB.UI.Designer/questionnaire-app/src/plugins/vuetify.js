import {createVuetify} from 'vuetify'
import { VDataTable } from 'vuetify/labs/VDataTable'
import 'vuetify/styles'

import {createI18n, useI18n } from 'vue-i18n'

//import messages from '@intlify/vite-plugin-vue-i18n/messages'

function loadLocaleMessages() {
    const locales = import.meta.globEager('../locale/*.json');

    const messages = {};

    for (const item in locales) {
        const matched = item.match(/([A-Za-z0-9-_]+)\./i);
        if (matched && matched.length > 1) {
            const locale = matched[1];

            messages[locale] = Object.assign(
                messages[locale] || {},
                locales[item]
            );
        }
    }
    return messages;
}

const messages = loadLocaleMessages();

var userLang = navigator.language || navigator.userLanguage;
const userLocale = userLang.split('-')[0];

//Create VueI18n instance with options
const i18n = new createI18n({
    locale: userLocale || import.meta.env.VUE_APP_I18N_LOCALE || 'en',
    fallbackLocale: import.meta.env.VUE_APP_I18N_FALLBACK_LOCALE || 'en',
    globalInjection: true,
    legacy: false,
    messages
});

const myCustomLightTheme = {
    dark: false,
    colors: {      
      primary: '2a81cb',      
    }
  }

// Import Vuetify
const vuetify = createVuetify({
        theme: {
            defaultTheme: 'myCustomLightTheme',            
            themes: {
                myCustomLightTheme
            }            
        },
        locale:{
            t: (key, ...params) => i18n.t(key, params)
        },
        components: {
            VDataTable,
        }
})

export {vuetify, i18n}
