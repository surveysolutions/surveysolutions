import { createApp } from 'vue';
import App from './App.vue';
import router from './router';
import {vuetify, i18n} /*, install, i18n */ from './plugins/vuetify';

/** Register Vue */
const vue = createApp(App);
vue.use(router);
vue.use(vuetify);
vue.use(i18n);

// Run!
router.isReady().then(() => {
  vue.mount('#app');
});
