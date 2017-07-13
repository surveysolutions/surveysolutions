import Vue from 'vue'
import Typeahead from './components/Typeahead'
import App from "./Interviewer/CreateNew.vue"
import Layout from "./components/Layout"
import Filters from "./components/Filters"
import FilterBlock from "./components/FilterBlock"
import AssignmentsTable from "./components/AssignmentsTable"

import configurator from "./plugins/configurator"
import locale from "./plugins/locale"

Vue.use(configurator)
Vue.use(locale)

Vue.component("Layout", Layout)
Vue.component("Filters",Filters)
Vue.component("FilterBlock",FilterBlock)

Vue.component("AssignmentsTable", AssignmentsTable)

const vueApp = new Vue({
    el: "#vueApp",
    render: h => h(App),
    components: { App }
})