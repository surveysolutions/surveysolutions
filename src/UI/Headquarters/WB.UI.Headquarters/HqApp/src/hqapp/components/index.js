import Vue from 'vue'

import Confirm from './Confirm'
import DataTables from "shared/components/DataTables"
import FilterBlock from "./FilterBlock"
import Filters from "./Filters"
import HqLayout from "./HqLayout"
import ModalFrame from "./ModalFrame"
import Typeahead from 'shared/components/Typeahead'

Vue.component("Confirm", Confirm)
Vue.component("DataTables", DataTables)
Vue.component("FilterBlock", FilterBlock)
Vue.component("Filters", Filters)
Vue.component("HqLayout", HqLayout)
Vue.component("ModalFrame", ModalFrame)
Vue.component("Typeahead", Typeahead)

export default {
    Confirm,
    DataTables,
    FilterBlock,
    Filters,
    HqLayout,
    ModalFrame,
    Typeahead
}