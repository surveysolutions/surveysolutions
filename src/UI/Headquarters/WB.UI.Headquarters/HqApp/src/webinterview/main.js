import "babel-polyfill";

import Vue from 'vue'

import config from "shared/config"
Vue.use(config)

import Vuei18n from "shared/plugins/locale"

import './init'
import "./misc/audioRecorder.js"
import "./misc/htmlPoly.js"

import "./errors"
import box from "bootbox"

export default Vuei18n.initializeAsync().then((i18n) => {
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
    
    box.addLocale("ar", {
        OK: i18n.t("Common.Ok"),
        CANCEL: i18n.t("Common.Cancel"),
        CONFIRM: i18n.t("Common.Confirm")
    });

    return new Vue({
        el: "#app",
        render: h => h(App),
        components: { App },
        store,
        router,
        i18n
    });
})

