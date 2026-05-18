import { createApp } from 'vue';
import { createPinia } from 'pinia';
import App from './App.vue';
import router from './router';
import i18next from './plugins/localization';
import I18NextVue from 'i18next-vue';
import { setupErrorHandler } from './plugins/errorHandler';
import './plugins/dayjs';

// Import Material Design Icons
import '@mdi/font/css/materialdesignicons.css';

//import '../../questionnaire/content/markup.less';
//import 'bootstrap/dist/css/bootstrap.min.css';

import ConfirmDialog from './plugins/confirm';
import ConfirmPromptDialog from './plugins/confirmPrompt';

import { PerfectScrollbarPlugin } from 'vue3-perfect-scrollbar';
import 'vue3-perfect-scrollbar/style.css';

import VueUploadComponent from 'vue-upload-component';
import Notifications from '@kyvg/vue3-notification';

import './extensions';

import directives from './directives/';

import emitter from './services/emitter';
import { installServerGuards } from './services/serverGuard';

installServerGuards({ page: true, fetch: true });

const pinia = createPinia();
pinia.use(({ store }) => {
    if (store.setupListeners && typeof store.setupListeners === 'function') {
        store.setupListeners();
    }
});

const vue = createApp(App);

setupErrorHandler(vue);

vue.config.globalProperties.$emitter = emitter;

vue.use(router);
vue.use(pinia);
//vue.use(i18n);
vue.use(I18NextVue, { i18next });
//vue.use(uiv);
vue.use(PerfectScrollbarPlugin);
vue.component('file-upload', VueUploadComponent);
vue.use(Notifications);

vue.use(ConfirmDialog);
vue.component('confirm-dialog', ConfirmDialog.default);

vue.use(ConfirmPromptDialog);
vue.component('confirm-prompt-dialog', ConfirmPromptDialog.default);

directives(vue);

import './views/Designer/pages/classifications/validationRules';

// Lazy-install Vuetify only for OptionsEditor routes, keeping it out of the main bundle.
let vuetifyInstalled = false;
router.beforeEach(async (to) => {
    if (!vuetifyInstalled && to.path.includes('/questionnaire/edit')) {
        const { vuetify } = await import('./plugins/vuetify');
        vue.use(vuetify);
        vuetifyInstalled = true;
    }
});

// Run!
router.isReady().then(() => {
    vue.mount('#app');
});
