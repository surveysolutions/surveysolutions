import "babel-polyfill";

import Vue from 'vue'

import config from "shared/config"
Vue.use(config)

import Vuei18n from "shared/plugins/locale"

import './init'
import "./misc/htmlPoly.js"

import "./errors"
import box from "shared/modal"

import { browserLanguage } from "shared/helpers"

export default Vuei18n.initializeAsync(browserLanguage).then((i18n) => {
    Vue.use(Vuei18n)

    require("./components")
    require("shared/components/questions")
    require("shared/components/questions/parts")
    require("./directives")

    const router = require("./router").default;
    const store = require("./store").default;
    const App = require("./App").default;
    const installApi = require("./api").install
    
    installApi(Vue)

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

