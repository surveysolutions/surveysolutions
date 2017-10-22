import "babel-polyfill";

import Vue from 'vue'
import Vuex from "vuex"
import { sync } from 'vuex-router-sync'
Vue.use(Vuex)

import config from "~/shared/config"
Vue.use(config)

import Vuei18n from "~/shared/plugins/locale"

import './init'
import "./misc/htmlPoly.js"

import "./errors"
import box from "./components/modal"

import { browserLanguage } from "~/shared/helpers"

export default Vuei18n.initializeAsync(browserLanguage).then((i18n) => {
    Vue.use(Vuei18n)

    require("./componentsRegistry")
    
    const createRouter = require("./router").default;
    
    const store = new Vuex.Store({
        modules: { 
            webinterview: require("./store").default
        }
    });

    const router = createRouter(store);

    sync(store, router)
    
    const App = require("./App").default;
    const installApi = require("./api").install
    
    installApi(Vue, { store })

    box.init(i18n, browserLanguage)

    return new Vue({
        el: "#app",
        render: h => h(App),
        components: { App },
        store,
        router,
        i18n
    });
})

