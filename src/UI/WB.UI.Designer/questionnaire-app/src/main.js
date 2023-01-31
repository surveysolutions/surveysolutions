import Vue from 'vue';
import App from './App.vue';
import router from './router';
import { vuetify, install, i18n } from './plugins/vuetify';

Vue.config.productionTip = false;

new Vue({
    router,
    install,
    i18n,
    render: h => h(App)
}).$mount('#app');
