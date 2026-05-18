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

import Bootstrap5Popover from '~/shared/components/Bootstrap5Popover.vue'
vue.component('popover', Bootstrap5Popover)

import Vuei18n from '~/shared/plugins/locale'
import { browserLanguage } from '~/shared/helpers'
const i18n = Vuei18n.initialize(browserLanguage, vue)

// validatePageLoad is called after i18n is initialized to ensure $t() is available
// when the response arrives and the error modal may need to be rendered.
import { validatePageLoad } from '~/shared/serverValidator'
validatePageLoad()

import 'bootstrap'
import 'flatpickr/dist/flatpickr.css'
import 'toastr/build/toastr.css'

import moment from 'moment'
import 'moment/locale/ar'
import 'moment/locale/cs'
import 'moment/locale/es'
import 'moment/locale/fr'
import 'moment/locale/id'
import 'moment/locale/pt'
import 'moment/locale/ro'
import 'moment/locale/ru'
import 'moment/locale/uk'
import 'moment/locale/zh-cn'
// moment uses 'zh-cn' for generic Chinese ('zh')
const momentLocaleMap = { zh: 'zh-cn' }
moment.locale(momentLocaleMap[browserLanguage] || browserLanguage)

import * as poly from 'smoothscroll-polyfill'
poly.polyfill()

import box from '@/shared/modal'

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

import { registerGlobalComponents } from './componentsRegistry'
registerGlobalComponents(vue, { router, store })

box.init(i18n, browserLanguage)

window._api = {
    store,
    router,
}

router.isReady().then(() => {
    vue.mount('#app');
});
