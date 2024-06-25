//import Vue from 'vue'

import Confirm from './Confirm'
// import DataTables from "./DataTables"
import FilterBlock from './FilterBlock'
import Filters from './Filters'
import HqLayout from './HqLayout'
import ModalFrame from './ModalFrame'
import Typeahead from './Typeahead'
import TextInput from './TextInput'
import Checkbox from './Checkbox'
import Radio from './Radio'
import DatePicker from './DatePicker'
import FormGroup from './FormGroup'
import FilterInput from './FilterInput'
import InlineSelector from './InlineSelector'
import ExpandableList from './ExpandableList.vue'

export function registerComponents(vue) {

    vue.component('Confirm', Confirm)
    vue.component('DataTables', () => import('./DataTables'))
    vue.component('FilterBlock', FilterBlock)
    vue.component('Filters', Filters)
    vue.component('HqLayout', HqLayout)
    vue.component('ModalFrame', ModalFrame)
    vue.component('Typeahead', Typeahead)
    vue.component('TextInput', TextInput)
    vue.component('Checkbox', Checkbox)
    vue.component('Radio', Radio)
    vue.component('DatePicker', DatePicker)
    vue.component('form-group', FormGroup)
    vue.component('FilterInput', FilterInput)
    vue.component('InlineSelector', InlineSelector)
    vue.component('ExpandableList', ExpandableList)
}