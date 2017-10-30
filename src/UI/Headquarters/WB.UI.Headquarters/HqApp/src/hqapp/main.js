import "babel-polyfill"
import 'bootstrap'
import 'bootstrap-select'

import Vue from 'vue'
import { sync } from 'vuex-router-sync'
import Vuei18n from "~/shared/plugins/locale"
import http from "~/shared/plugins/http"
import config from "~/shared/config"
import store from "./store"

import './components'
import './compatibility.js'
import "~/webinterview/componentsRegistry"

import box from "~/webinterview/components/modal"
import 'flatpickr/dist/flatpickr.css'
import { browserLanguage } from "~/shared/helpers"
moment.locale(browserLanguage);

export default Vuei18n.initializeAsync(browserLanguage).then((i18n) => {
    Vue.use(config);
    Vue.use(http);
    Vue.use(Vuei18n);

    const viewsProvider = require("./views").default;
    const Router = require('./router').default;

    const views = viewsProvider(store);

    const router = new Router({
        routes: views.routes
    }).router;
    
    router.afterEach((to, from) => {
        store.dispatch("changeSection", to.params.sectionId)
        store.dispatch("onBeforeNavigate")
    })
    sync(store, router)

    box.init(i18n, browserLanguage);
    new Vue({
        el: "#vueApp",
        render: h => h('router-view'),
        store,
        router,
        i18n
    });
})
