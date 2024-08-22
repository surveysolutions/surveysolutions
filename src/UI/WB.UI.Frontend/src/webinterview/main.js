//import Vue from 'vue'
//import Vuex from 'vuex'

import { createApp } from 'vue';
//import { createPinia } from 'pinia';

import App from './App.vue';
import { setupErrorHandler } from './errors';

//import { sync } from 'vuex-router-sync'
//TODO: MIGRATION, fix old usage of vuex-router-sync

import * as toastr from 'toastr'
toastr.options.escapeHtml = true

//const pinia = createPinia();
// pinia.use(({ store }) => {
//     if (store.setupListeners && typeof store.setupListeners === 'function') {
//         store.setupListeners();
//     }
// });

const vue = createApp(App)
setupErrorHandler(vue)

//vue.use(Vuex)
//vue.use(pinia)

import config from '~/shared/config'
vue.use(config)


import VueTextareaAutosize from 'vue-textarea-autosize'
vue.use(VueTextareaAutosize)

import PortalVue from 'portal-vue'
vue.use(PortalVue)

import { Popover } from 'uiv'
vue.component('popover', Popover)

import Vuei18n from '~/shared/plugins/locale'
import { browserLanguage } from '~/shared/helpers'
const i18n = Vuei18n.initialize(browserLanguage, vue)

import './init'
import './errors'
import box from '@/shared/modal'

import { registerGlobalComponents } from './componentsRegistry'
registerGlobalComponents(vue)

import createRouter from './router'

import webinterviewStore from './stores'

import { createStore } from 'vuex';
const store = createStore({
    modules: {
        webinterview: webinterviewStore,
    },
})

import http from './api/http'
vue.use(http, { store })

const router = createRouter(store)

vue.use(store)

//sync(store, router)
//TODO: MIGRATION


box.init(i18n, browserLanguage)

window._api = {
    store,
    router,
}

// Run!
router.isReady().then(() => {
    vue.mount('#vueApp');
});
