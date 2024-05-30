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

import { registerSW } from 'virtual:pwa-register';

//import * as uiv from 'uiv';

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

// Run!
router.isReady().then(() => {
    vue.mount('#app');

    const updateSW = registerSW({
        onNeedRefresh() {
            console.log('New content available. Refresh?');
            if (confirm('New content available. Refresh?')) {
                updateSW(true);
            }
        },
        onOfflineReady() {
            console.log('App is ready to work offlin22222222222.');
        },
        scope: '/assets/',
        url: '/assets/sw.js'
    });

    /*if ('serviceWorker' in navigator) {
        navigator.serviceWorker
            .register('/assets/sw.js')
            .then(registration => {
                console.log(
                    'Service Worker registered with scope:',
                    registration.scope
                );
                console.log(
                    'Service Worker registered with url:',
                    registration.url
                );
            })
            .catch(error => {
                console.log('Service Worker registration failed:', error);
            });
    }*/

    if ('serviceWorker' in navigator) {
        navigator.serviceWorker
            .register('/assets/sw.js')
            .then(registration => {
                console.log(
                    'Service Worker registered with scope:',
                    registration.scope
                );
                registration.addEventListener('updatefound', () => {
                    const installingWorker = registration.installing;
                    installingWorker.onstatechange = () => {
                        if (installingWorker.state === 'installed') {
                            if (navigator.serviceWorker.controller) {
                                console.log(
                                    'New content is available; please refresh.'
                                );
                                if (
                                    confirm('New content available. Refresh?')
                                ) {
                                    window.location.reload();
                                }
                            } else {
                                console.log(
                                    'Content is cached for offline use.'
                                );
                            }
                        }
                    };
                });
            })
            .catch(error => {
                console.log('Service Worker registration failed:', error);
            });
    }
});
