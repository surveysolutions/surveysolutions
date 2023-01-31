import Vue from 'vue';
import createVuetify from 'vuetify';
import VueI18n from 'vue-i18n';

//Vue.use(VueI18n);
//Vue.use(Vuetify);

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

// Create VueI18n instance with options
export const i18n = new VueI18n({
    locale: userLocale || import.meta.env.VUE_APP_I18N_LOCALE || 'en',
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
    }
});

// Import Vuetify
export const install = ({ app }) => {
    const vuetify = createVuetify({
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
        },
    })
  
    app.use(vuetify)
  }
