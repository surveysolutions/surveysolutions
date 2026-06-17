//import Vue from 'vue'

import { defineAsyncComponent } from 'vue'
import Confirm from './Confirm'
import FilterBlock from './FilterBlock'
import Filters from './Filters'
import HqLayout from './HqLayout'
import ModalFrame from './ModalFrame'
import TextInput from './TextInput'
import Checkbox from './Checkbox'
import Radio from './Radio'
import FormGroup from './FormGroup'
import FilterInput from './FilterInput'
import InlineSelector from './InlineSelector'
import ExpandableList from './ExpandableList.vue'
import Select from './Select.vue'

const AsyncTypeahead = defineAsyncComponent(() => import('./Typeahead'))
const AsyncDatePicker = defineAsyncComponent(() => import('./DatePicker'))

import DataTables from './DataTables'

export function registerComponents(vue) {

    vue.component('Confirm', Confirm)
    vue.component('DataTables', DataTables)
    vue.component('FilterBlock', FilterBlock)
    vue.component('Filters', Filters)
    vue.component('HqLayout', HqLayout)
    vue.component('ModalFrame', ModalFrame)
    vue.component('Typeahead', AsyncTypeahead)
    vue.component('TextInput', TextInput)
    vue.component('Checkbox', Checkbox)
    vue.component('Radio', Radio)
    vue.component('DatePicker', AsyncDatePicker)
    vue.component('form-group', FormGroup)
    vue.component('FilterInput', FilterInput)
    vue.component('InlineSelector', InlineSelector)
    vue.component('ExpandableList', ExpandableList)
    vue.component('Select', Select)
}