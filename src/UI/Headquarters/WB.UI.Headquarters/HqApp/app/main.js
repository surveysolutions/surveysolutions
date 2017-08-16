import "babel-polyfill";
import Vue from 'vue'

import ExportButtons from './components/ExportButtons'
import Typeahead from './components/Typeahead'
import Layout from "./components/Layout"
import Filters from "./components/Filters"
import FilterBlock from "./components/FilterBlock"
import DataTables from "./components/DataTables"
import ModalFrame from "./components/ModalFrame"
import Confirm from './components/Confirm'
import Vuei18n from "./plugins/locale"
import store from "./store"
import App from "./App"
import config from "./config"

Vue.use(config)

Vue.use(Vuei18n, {
    nsSeparator: '.',
    keySeparator: ':',
    resources: {
        'en': Vue.$config.resources
    }
})

Vue.component("Layout", Layout)
Vue.component("Filters", Filters)
Vue.component("FilterBlock", FilterBlock)
Vue.component("Typeahead", Typeahead)
Vue.component("DataTables", DataTables)
Vue.component("ModalFrame", ModalFrame)
Vue.component("Confirm", Confirm)
Vue.component("ExportButtons", ExportButtons)

const router = require("./router").default

const vueApp = new Vue({
    el: "#vueApp",
    render: h => h(App),
    components: { App },
    store,
    router
});

