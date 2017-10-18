import 'core-js/es6/promise'
import 'core-js/modules/es6.object.assign'
import 'bootstrap/dist/js/bootstrap.js'
import 'bootstrap-select'
import "babel-polyfill"

import Vue from 'vue'
import VeeValidate from 'vee-validate';
import Vuei18n from "shared/plugins/locale"
import http from "shared/plugins/http"
import config from "shared/config"
import store from "./store"
import './components'
import './compatibility.js'
import box from "shared/modal"
import { browserLanguage } from "shared/helpers"

export default Vuei18n.initializeAsync(browserLanguage).then((i18n) => {
    Vue.use(config);
    Vue.use(http);
    Vue.use(VeeValidate);
    Vue.use(Vuei18n);
    const router = require('./router').default;
    box.init(i18n, browserLanguage);
    new Vue({
        el: "#vueApp",
        render: h => h('router-view'),
        store,
        router,
        i18n
    });
})
