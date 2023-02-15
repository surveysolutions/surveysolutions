import 'bootstrap'
import 'bootstrap-select'

import '../assets/css/markup.scss'
import '../assets/css/markup-specific.scss'

import Vue from 'vue'
import Vuei18n from '~/shared/plugins/locale'
import { browserLanguage } from '~/shared/helpers'
const i18n = Vuei18n.initialize(browserLanguage)

import VueApollo from 'vue-apollo'
Vue.use(VueApollo)
import { sync } from 'vuex-router-sync'

import http from '~/shared/plugins/http'
import config from '~/shared/config'
import store from './store'
import moment from 'moment'
moment.locale(browserLanguage)

import './components'
import './compatibility.js'
import '~/webinterview/componentsRegistry'

import VueTextareaAutosize from 'vue-textarea-autosize'
Vue.use(VueTextareaAutosize)

import PortalVue from 'portal-vue'
Vue.use(PortalVue)

import { Popover } from 'uiv'
Vue.component('popover', Popover)

import box from '@/shared/modal'
import 'flatpickr/dist/flatpickr.css'
import 'toastr/build/toastr.css'
import * as toastr from 'toastr'
toastr.options.escapeHtml = true

import * as poly from 'smoothscroll-polyfill'
poly.polyfill()

import hqApi from './api'
import apolloClient from './api/graphql'



Vue.use(config)
Vue.use(http)
Vue.use(hqApi)

//const viewsProvider = require('./Views').default
import viewsProvider from './Views'
//const Router = require('./router').default
import Router from './router'

const views = viewsProvider(store)

const router = new Router({
    routes: views.routes,
}).router

sync(store, router)

box.init(i18n, browserLanguage)

Vue.prototype.$eventHub = new Vue()

export default new Vue({
    el: '#vueApp',
    render: h => h('router-view'),
    store,
    router,
    apolloProvider: new VueApollo({
        defaultClient: apolloClient,
    }),
})
