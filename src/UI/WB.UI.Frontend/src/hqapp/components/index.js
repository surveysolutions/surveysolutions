import Vue from 'vue'

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

Vue.component('Confirm', Confirm)
Vue.component('DataTables', () => import('./DataTables'))
Vue.component('FilterBlock', FilterBlock)
Vue.component('Filters', Filters)
Vue.component('HqLayout', HqLayout)
Vue.component('ModalFrame', ModalFrame)
Vue.component('Typeahead', Typeahead)
Vue.component('TextInput', TextInput)
Vue.component('Checkbox', Checkbox)
Vue.component('Radio', Radio)
Vue.component('DatePicker', DatePicker)
Vue.component('form-group', FormGroup)
Vue.component('FilterInput', FilterInput)
Vue.component('InlineSelector', InlineSelector)

// export default {
//     Confirm,
//     DataTables,
//     FilterBlock,
//     Filters,
//     HqLayout,
//     ModalFrame,
//     Typeahead,
//     TextInput,
//     Checkbox,
//     Radio,
//     DatePicker
// }
