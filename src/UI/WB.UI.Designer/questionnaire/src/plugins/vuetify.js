import { createVuetify } from 'vuetify';
import { aliases, mdi } from 'vuetify/iconsets/mdi';
import i18nInstance from './localization';

const myCustomLightTheme = {
    dark: false,
    colors: {
        primary: '2a81cb'
    }
};

// Import Vuetify
const vuetify = createVuetify({
    theme: {
        defaultTheme: 'myCustomLightTheme',
        themes: {
            myCustomLightTheme
        }
    },
    icons: {
        defaultSet: 'mdi',
        aliases,
        sets: {
            mdi,
        },
    },
    locale: {
        t: (key, ...params) => i18nInstance.t(key, params)
    }
});

export { vuetify };
