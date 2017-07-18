import Vue from 'vue'
import Typeahead from './components/Typeahead'
import Layout from "./components/Layout"
import Filters from "./components/Filters"
import FilterBlock from "./components/FilterBlock"
import DataTables from "./components/DataTables"
import configurator from "./plugins/configurator"

import locale from "./plugins/locale"

Vue.use(configurator)
Vue.use(locale)

Vue.component("Layout", Layout)
Vue.component("Filters", Filters)
Vue.component("FilterBlock", FilterBlock)
Vue.component("Typeahead", Typeahead)
Vue.component("DataTables", DataTables)

import confirm from './components/Confirm'
Vue.component("Confirm", confirm)

export default (app, options) => {

    const vueApp = new Vue(_.assign({
        el: "#vueApp",
        render: h => h(app),
        components: { app }
    }, options));
}