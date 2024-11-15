import 'bootstrap'
import * as bootstrap from 'bootstrap'
window.bootstrap = bootstrap
window.Dropdown = bootstrap.Dropdown

import '../assets/css/markup.scss'
import '../assets/css/markup-specific.scss'

import { createApp } from 'vue'
import App from './App.vue';

const vue = createApp(App)

import { setupErrorHandler } from '../shared/errorHandler.js'
setupErrorHandler(vue);

import Vuei18n from '~/shared/plugins/locale'
import { browserLanguage } from '~/shared/helpers'
const i18n = Vuei18n.initialize(browserLanguage, vue)

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

import { registerGlobalComponents } from '~/webinterview/componentsRegistry'
registerGlobalComponents(vue)

import PortalVue from 'portal-vue'
vue.use(PortalVue)

import { Popover } from 'uiv'
vue.component('popover', Popover)

import './validate.js'

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

vue.use(router)

import { pageTitle } from 'vue-page-title'
vue.use(pageTitle)

vue.config.globalProperties.$eventHub = vue

import emitter from '~/shared/emitter'
vue.config.globalProperties.$emitter = emitter

router.isReady().then(() => {
    vue.mount('#vueApp');
});