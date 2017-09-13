import 'core-js/es6/promise'
import 'core-js/modules/es6.object.assign'
import 'bootstrap/js/dropdown.js'
import 'bootstrap/js/modal.js'
import 'bootstrap-select'
import "babel-polyfill"

import Vue from 'vue'
import Vuei18n from "./plugins/locale"
import VueAxios from "./plugins/http"
Vue.use(VueAxios);

import VeeValidate from 'vee-validate';
Vue.use(VeeValidate);

import store from "./store"

import config from "./config"

Vue.use(config)

Vue.use(Vuei18n, {
    nsSeparator: '.',
    keySeparator: ':',
    resources: {
        'en': Vue.$config.resources
    }
})

import './components'

const router = require("./router").default

export default new Vue({
    el: "#vueApp",
    render: h => h('router-view'),
    store,
    router
});

