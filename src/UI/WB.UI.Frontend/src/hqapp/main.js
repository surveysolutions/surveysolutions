import Dropdown from 'bootstrap/js/dist/dropdown'
import Modal from 'bootstrap/js/dist/modal'
import Tooltip from 'bootstrap/js/dist/tooltip'
window.bootstrap = { Dropdown, Modal, Tooltip }
window.Dropdown = Dropdown

import '../assets/css/markup.scss'
import '../assets/css/markup-specific.scss'

import { createApp } from 'vue'
import App from './App.vue'
import { validatePageLoad } from '~/shared/serverValidator'

const vue = createApp(App)

import { setupErrorHandler } from '../shared/errorHandler.js'
setupErrorHandler(vue)

import Vuei18n from '~/shared/plugins/locale'
import { browserLanguage } from '~/shared/helpers'
const i18n = Vuei18n.initialize(browserLanguage, vue)
// validatePageLoad is called after i18n is initialized to ensure $t() is available
// when the HEAD/GET response arrives and the error modal may need to be rendered.
// The global axios interceptor is installed by vue.use(http) below — no duplicate needed here.
validatePageLoad()

//plugin registration in Vue
import http from '~/shared/plugins/http'
import config from '~/shared/config'

import { registerStore } from './store'
const store = registerStore(vue)

import moment from 'moment'
moment.locale(browserLanguage)

import { registerComponents } from './components'
registerComponents(vue)

import ProfileLayout from './Views/HQ/Users/ProfileLayout.vue'
vue.component('ProfileLayout', ProfileLayout)

import './compatibility.js'

import VueDOMPurifyHTML from 'vue-dompurify-html'
vue.use(VueDOMPurifyHTML)

import Bootstrap5Popover from '~/shared/components/Bootstrap5Popover.vue'
vue.component('popover', Bootstrap5Popover)

import './validate.js'

import box from '@/shared/modal'
box.init(i18n, browserLanguage)

import 'flatpickr/dist/flatpickr.css'
import 'toastr/build/toastr.css'
import * as toastr from 'toastr'
toastr.options.escapeHtml = true

import hqApi from './api'

vue.use(config)
vue.use(http)
vue.use(hqApi)

import viewsProvider from './Views'
import Router from './router'

const views = viewsProvider(store)

const router = new Router({
    routes: views.routes,
    store: store,
}).router

router.beforeEach(async (to, from, next) => {
    try {
        next()
    } catch (error) {
        next(error)
    }
})

vue.use(router)

import { registerBaseGlobalComponents } from '~/webinterview/componentsRegistry'
registerBaseGlobalComponents(vue, { router, store })


vue.config.globalProperties.$eventHub = vue

import emitter from '~/shared/emitter'
vue.config.globalProperties.$emitter = emitter

router.isReady().then(() => {
    vue.mount('#vueApp')
})