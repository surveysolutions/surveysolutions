import { createApp } from 'vue'
import App from './App.vue'
import { setupErrorHandler } from '../shared/errorHandler.js'
import * as toastr from 'toastr'
toastr.options.escapeHtml = true

const vue = createApp(App)
setupErrorHandler(vue)

import VueDOMPurifyHTML from 'vue-dompurify-html';
vue.use(VueDOMPurifyHTML)

import config from '~/shared/config'
vue.use(config)

import PortalVue from 'portal-vue'
vue.use(PortalVue)

import { Popover } from 'uiv'
vue.component('popover', Popover)

import Vuei18n from '~/shared/plugins/locale'
import { browserLanguage } from '~/shared/helpers'
const i18n = Vuei18n.initialize(browserLanguage, vue)

import './init'
import box from '@/shared/modal'
import { registerGlobalComponents } from './componentsRegistry'
registerGlobalComponents(vue)

import createRouter from './router'
import webinterviewStore from './stores'
import { createStore } from 'vuex';
import routeParams from '../shared/stores/store.routeParams.js'

const store = createStore({
    modules: {
        webinterview: webinterviewStore,
        route: routeParams
    },
})

import http from './api/http'
vue.use(http, { store })

const router = createRouter(store)

vue.use(store)
vue.use(router)

box.init(i18n, browserLanguage)

window._api = {
    store,
    router,
}

router.isReady().then(() => {
    vue.mount('#app');
});
