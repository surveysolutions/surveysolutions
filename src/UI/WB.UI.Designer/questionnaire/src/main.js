import { createApp } from 'vue';
import { createPinia } from 'pinia';
import App from './App.vue';
import router from './router';
import VueProgressBar from '@aacassandra/vue3-progressbar';
import VueDOMPurifyHTML from 'vue-dompurify-html';
import i18next from './plugins/localization';
import I18NextVue from 'i18next-vue';
import { vuetify } from './plugins/vuetify';
import { setupErrorHandler } from './plugins/errorHandler';

//import '../../questionnaire/content/markup.less';
//import 'bootstrap/dist/css/bootstrap.min.css';
import 'bootstrap';

import ConfirmDialog from './plugins/confirm';

import { PerfectScrollbarPlugin } from 'vue3-perfect-scrollbar';
import 'vue3-perfect-scrollbar/style.css';

import VueUploadComponent from 'vue-upload-component';
import Notifications from '@kyvg/vue3-notification';

import './extensions';

import directives from './directives/';

import emitter from './services/emitter';

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
vue.use(vuetify); //reqired by options component. consider either remove or use.
vue.use(PerfectScrollbarPlugin);
vue.use(VueDOMPurifyHTML);
vue.component('file-upload', VueUploadComponent);
vue.use(Notifications);

const options = {
    color: '#29d',
    failedColor: '#874b4b',
    thickness: '3px',
    transition: {
        speed: '0.2s',
        opacity: '0.6s',
        termination: 300
    },
    autoRevert: true,
    location: 'top',
    inverse: false,
    autoFinish: false
};

vue.use(VueProgressBar, options);

vue.use(ConfirmDialog);
vue.component('confirm-dialog', ConfirmDialog.default);

directives(vue);

import './views/Designer/pages/classifications/validationRules';
import store from './views/Designer/pages/classifications/store';
vue.use(store);

// Run!
router.isReady().then(() => {
    vue.mount('#app');
});
