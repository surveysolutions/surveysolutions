import 'bootstrap'
import * as bootstrap from 'bootstrap'
//import "bootstrap/scss/_functions.scss";
//import "bootstrap/scss/_mixins.scss";
//import "bootstrap/scss/_variables.scss";
//import "bootstrap/scss/_nav.scss";
//import "bootstrap/scss/_navbar.scss";
//import "bootstrap/scss/bootstrap.scss";

import 'bootstrap-select'

import '../assets/css/markup.scss'
import '../assets/css/markup-specific.scss'

import { createApp } from 'vue'
//import { createPinia } from 'pinia'

import App from './App.vue';

//const pinia = createPinia()
const vue = createApp(App)
//vue.use(pinia)

import { setupErrorHandler } from '../shared/errorHandler.js'
setupErrorHandler(vue);

import Vuei18n from '~/shared/plugins/locale'
import { browserLanguage } from '~/shared/helpers'
const i18n = Vuei18n.initialize(browserLanguage, vue)

//import VueApollo from 'vue-apollo'
//vue.use(VueApollo)

//import { sync } from 'vuex-router-sync'
//TODO: MIGRATION, fix old usage of vuex-router-sync 

//plugin registration in Vue
import http from '~/shared/plugins/http'
import config from '~/shared/config'

import { registerStore } from './store'
const store = registerStore(vue)

import moment from 'moment'
moment.locale(browserLanguage)

import { registerComponents } from './components'
registerComponents(vue)

//TODO: MIGRATION. move it
import ProfileLayout from './Views/HQ/Users/ProfileLayout.vue'
vue.component('ProfileLayout', ProfileLayout)

import './compatibility.js'

import { registerGlobalComponents } from '~/webinterview/componentsRegistry'
registerGlobalComponents(vue)

import PortalVue from 'portal-vue'
vue.use(PortalVue)

import { Popover } from 'uiv'
vue.component('popover', Popover)

//register validations globaly
//add more rules if required 
//https://vee-validate.logaretm.com/v4/guide/global-validators
import { defineRule } from 'vee-validate'
import { required, email, integer, max_value, min, min_value, max, numeric, not_one_of, regex } from '@vee-validate/rules'
defineRule('required', required)
defineRule('email', email)
defineRule('integer', integer)
defineRule('max_value', max_value)
defineRule('min', min)
defineRule('min_value', min_value)
defineRule('max', max)
defineRule('numeric', numeric)
defineRule('not_one_of', not_one_of)
defineRule('regex', regex)

//import once it's impenemted
defineRule("required_if", (value, [target, targetValue], ctx) => {
    if (targetValue === ctx.form[target]) {
        return required(value);
    }
    return true;
});


import box from '@/shared/modal'
box.init(i18n, browserLanguage)

import 'flatpickr/dist/flatpickr.css'
import 'toastr/build/toastr.css'
import * as toastr from 'toastr'
toastr.options.escapeHtml = true

import * as poly from 'smoothscroll-polyfill'
poly.polyfill()

import hqApi from './api'
import apolloClient from './api/graphql'
import { createApolloProvider } from '@vue/apollo-option'
const apolloProvider = createApolloProvider({
    defaultClient: apolloClient,
})
vue.use(apolloProvider)

vue.use(config)
vue.use(http)
vue.use(hqApi)

import viewsProvider from './Views'
import Router from './router'

const views = viewsProvider(store)

const router = new Router({
    routes: views.routes,
    store: store
}).router


//import router from './router';
vue.use(router)

//sync(store, router)
//TODO: MIGRATION

import { pageTitle } from 'vue-page-title'
vue.use(pageTitle)

vue.config.globalProperties.$eventHub = vue


import emitter from '~/shared/emitter';
vue.config.globalProperties.$emitter = emitter;

//TODO: MIGRATION. 

// export default new Vue({
//     el: '#vueApp',
//     render: h => h('router-view'),
//     store,
//     router,
//     // apolloProvider: new VueApollo({
//     //     defaultClient: apolloClient,
//     // }),
// })


// Run!
router.isReady().then(() => {
    vue.mount('#vueApp');
});