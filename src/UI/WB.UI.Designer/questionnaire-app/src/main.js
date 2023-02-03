import { createApp } from 'vue';
import App from './App.vue';
import router from './router';
import {vuetify, i18n} /*, install, i18n */ from './plugins/vuetify';

//Vue.config.productionTip = false;

/** Register Vue */
const vue = createApp(App);
vue.use(router);
//vue.use(store);
vue.use(vuetify);
vue.use(i18n);
//vue.use(install);

// Run!
router.isReady().then(() => {
  vue.mount('#app');
});



// new Vue({
//     router,
//     install,
//     i18n,
//     render: h => h(App)
// }).$mount('#app');
