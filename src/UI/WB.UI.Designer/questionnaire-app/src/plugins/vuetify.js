import Vue from 'vue';
import Vuetify from 'vuetify/lib';
import VueI18n from 'vue-i18n';

Vue.use(VueI18n);
Vue.use(Vuetify);

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

// Create VueI18n instance with options
export const i18n = new VueI18n({
    locale: import.meta.env.VUE_APP_I18N_LOCALE || 'en',
    fallbackLocale: import.meta.env.VUE_APP_I18N_FALLBACK_LOCALE || 'en',
    messages
});

export const vuetify = new Vuetify({
    theme: {
        options: {
            customProperties: true
        },
        lang: {
            t: (key, ...params) => i18n.t(key, params)
        },
        themes: {
            light: {
                primary: '#2a81cb'
            }
        }
        // themes: {
        //   light: {
        //     primary: '#ee44aa',
        //     secondary: '#424242',
        //     accent: '#82B1FF',
        //     error: '#FF5252',
        //     info: '#2196F3',
        //     success: '#4CAF50',
        //     warning: '#FFC107',
        //   },
        // },
    }
});
