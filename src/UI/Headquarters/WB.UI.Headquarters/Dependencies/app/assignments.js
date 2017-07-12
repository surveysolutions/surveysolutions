import Vue from 'vue'
//import vueResource from 'vue-resource'

//Vue.use(vueResource)
//Vue.http.headers.common['Authorization'] = wnidow.input.settings.acsrf.token;

import Typeahead from './components/Typeahead'
import App from "./Interviewer/CreateNew.vue"
import Layout from "./components/Layout"
import Filters from "./components/Filters"
import FilterBlock from "./components/FilterBlock"
import AssignmentsTable from "./components/AssignmentsTable"

Vue.component("Layout", Layout)
Vue.component("Filters",Filters)
Vue.component("FilterBlock",FilterBlock)
Vue.component("AssignmentsTable", AssignmentsTable)

import configurator from "./plugins/configurator"
Vue.use(configurator)

import locale from "./plugins/locale"
Vue.use(locale)

const vueApp = new Vue({
    el: "#vueApp",
    render: h => h(App),
    components: { App }
})