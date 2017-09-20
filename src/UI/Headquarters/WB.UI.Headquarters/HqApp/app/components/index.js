import Vue from 'vue'

import Confirm from './Confirm'
import DataTables from "./DataTables"
import FilterBlock from "./FilterBlock"
import Filters from "./Filters"
import Layout from "./Layout"
import ModalFrame from "./ModalFrame"
import Typeahead from './Typeahead'

Vue.component("Confirm", Confirm)
Vue.component("DataTables", DataTables)
Vue.component("FilterBlock", FilterBlock)
Vue.component("Filters", Filters)
Vue.component("Layout", Layout)
Vue.component("ModalFrame", ModalFrame)
Vue.component("Typeahead", Typeahead)

export default {
    Confirm,
    DataTables,
    FilterBlock,
    Filters,
    Layout,
    ModalFrame,
    Typeahead
}