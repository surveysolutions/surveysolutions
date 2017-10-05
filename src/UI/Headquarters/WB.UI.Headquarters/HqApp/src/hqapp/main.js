import 'core-js/es6/promise'
import 'core-js/modules/es6.object.assign'
import 'bootstrap/dist/js/bootstrap.js'
import 'bootstrap-select'
import "babel-polyfill"

import Vue from 'vue'
import Vuei18n from "shared/plugins/locale"
import http from "shared/plugins/http"
Vue.use(http);

import VeeValidate from 'vee-validate';
Vue.use(VeeValidate);

import store from "./store"

import config from "shared/config"

Vue.use(config)

Vue.use(Vuei18n, {
    nsSeparator: '.',
    keySeparator: ':',
    resources: {
        'en': Vue.$config.model.resources
    }
})

import './components'

import router from "./router"
export default new Vue({
    el: "#vueApp",
    render: h => h('router-view'),
    store,
    router
});

import './compatibility.js'