import "babel-polyfill";

import Vue from 'vue'

import Typeahead from './components/Typeahead'
import Layout from "./components/Layout"
import Filters from "./components/Filters"
import FilterBlock from "./components/FilterBlock"
import DataTables from "./components/DataTables"
import ModalFrame from "./components/ModalFrame"
import Confirm from './components/Confirm'
import Vuei18n from "./plugins/locale"
import common_store from "./store"

const config = JSON.parse(window.vueApp.getAttribute('configuration'))

Vue.use(Vuei18n, {
    nsSeparator: '.',
    keySeparator: ':',
    resources: {
        'en': config.resources
    }
})

Vue.component("Layout", Layout)
Vue.component("Filters", Filters)
Vue.component("FilterBlock", FilterBlock)
Vue.component("Typeahead", Typeahead)
Vue.component("DataTables", DataTables)
Vue.component("ModalFrame", ModalFrame)
Vue.component("Confirm", Confirm)

export default ({ app, options}) => {
    const store = require("./store").default

    const vueApp = new Vue(_.assign({
        el: "#vueApp",
        render: h => h(app),
        components: { app },
        store
    }, options));
}
