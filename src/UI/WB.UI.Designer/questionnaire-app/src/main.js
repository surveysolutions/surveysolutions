import { createApp } from 'vue';
import { createPinia } from 'pinia';
import App from './App.vue';
import router from './router';
import VueProgressBar from "@aacassandra/vue3-progressbar";
import VueDOMPurifyHTML from 'vue-dompurify-html';
import i18n from './plugins/localization';
//import { vuetify /*, install, i18n */ } from './plugins/vuetify';

//import '../../questionnaire/content/markup.less';
//import 'bootstrap/dist/css/bootstrap.min.css';
import 'bootstrap';

import ConfirmDialog from './plugins/confirm';

import PerfectScrollbar from 'vue3-perfect-scrollbar';
import 'vue3-perfect-scrollbar/dist/vue3-perfect-scrollbar.css';

import VueUploadComponent from 'vue-upload-component';

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

vue.config.globalProperties.$emitter = emitter;

vue.use(router);
vue.use(pinia);
//vue.use(vuetify); //reqired by options component. consider either remove or use.
vue.use(i18n);
//vue.use(uiv);
vue.use(PerfectScrollbar);
vue.use(VueDOMPurifyHTML);
vue.component('file-upload', VueUploadComponent);

const options = {
    color: "#29d",
    failedColor: "#874b4b",
    thickness: "3px",
    transition: {
      speed: "0.2s",
      opacity: "0.6s",
      termination: 300,
    },
    autoRevert: true,
    location: "top",
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
});
