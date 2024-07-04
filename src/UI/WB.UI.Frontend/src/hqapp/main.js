import 'bootstrap'
import 'bootstrap-select'

import '../assets/css/markup.scss'
import '../assets/css/markup-specific.scss'

import { createApp } from 'vue'
//import { createPinia } from 'pinia'

import App from './App.vue';

//import Vuei18n from '~/shared/plugins/locale'
import { browserLanguage } from '~/shared/helpers'
//const i18n = Vuei18n.initialize(browserLanguage)


//const pinia = createPinia()
const vue = createApp(App)
//vue.use(pinia)

//temp reg
//remove after migration to vue 3 & i18next
vue.config.globalProperties.$t = function (literal) {
    return literal;
}


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

import './compatibility.js'
import '~/webinterview/componentsRegistry'

import VueTextareaAutosize from 'vue-textarea-autosize'
vue.use(VueTextareaAutosize)

import PortalVue from 'portal-vue'
vue.use(PortalVue)

import { Popover } from 'uiv'
vue.component('popover', Popover)

import box from '@/shared/modal'
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
    routes: views.routes
}).router


//import router from './router';
vue.use(router)

//sync(store, router)
//TODO: MIGRATION

import { pageTitle } from 'vue-page-title'
vue.use(pageTitle)


//box.init(i18n, browserLanguage)

vue.config.globalProperties.$eventHub = vue
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