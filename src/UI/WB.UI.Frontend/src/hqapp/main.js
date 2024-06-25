import 'bootstrap'
import 'bootstrap-select'

import '../assets/css/markup.scss'
import '../assets/css/markup-specific.scss'

import { createApp } from 'vue'
//import { createPinia } from 'pinia'

import App from './App.vue';

//import Vuei18n from '~/shared/plugins/locale'
//import { browserLanguage } from '~/shared/helpers'
//const i18n = Vuei18n.initialize(browserLanguage)

//const pinia = createPinia()
const app = createApp(App)
//vue.use(pinia)

import VueApollo from 'vue-apollo'
app.use(VueApollo)
//import { sync } from 'vuex-router-sync'
//TODO: MIGRATION, fix old usage of vuex-router-sync 

//plugin registration in Vue
import http from '~/shared/plugins/http'
import config from '~/shared/config'

import { registerStore } from './store'
const store = registerStore(app)

import moment from 'moment'
moment.locale(browserLanguage)

import { registerComponents } from './components'
registerComponents(app)

import './compatibility.js'
import '~/webinterview/componentsRegistry'

import VueTextareaAutosize from 'vue-textarea-autosize'
app.use(VueTextareaAutosize)

import PortalVue from 'portal-vue'
app.use(PortalVue)

import { Popover } from 'uiv'
app.component('popover', Popover)

import box from '@/shared/modal'
import 'flatpickr/dist/flatpickr.css'
import 'toastr/build/toastr.css'
import * as toastr from 'toastr'
toastr.options.escapeHtml = true

import * as poly from 'smoothscroll-polyfill'
poly.polyfill()

import hqApi from './api'
import apolloClient from './api/graphql'


app.use(config)
app.use(http)
app.use(hqApi)

import viewsProvider from './Views'
import Router from './router/index.js'

const views = viewsProvider(store)

const router = new Router({
    routes: views.routes,
}).router

app.use(router)

//sync(store, router)
//TODO: MIGRATION

import VuePageTitle from 'vue-page-title'
app.use(VuePageTitle, {})


box.init(i18n, browserLanguage)

app.config.globalProperties.$eventHub = app
//TODO: MIGRATION. 

export default new Vue({
    el: '#vueApp',
    render: h => h('router-view'),
    store,
    router,
    apolloProvider: new VueApollo({
        defaultClient: apolloClient,
    }),
})
