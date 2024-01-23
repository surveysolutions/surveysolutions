import { createVuetify } from 'vuetify';
import { VDataTable } from 'vuetify/labs/VDataTable';
//import 'vuetify/styles';
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
    locale: {
        t: (key, ...params) => i18nInstance.t(key, params)
    },
    components: {
        VDataTable
    }
});

export { vuetify };
