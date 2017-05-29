﻿import Vue from 'vue'
import VueResource from 'vue-resource'
import Typeahead from './../Typeahead.vue'
import AssignmentsTable from './AssignmentsTable.vue'
﻿import VeeValidate from 'vee-validate';

Vue.use(VueResource);

Vue.component("typeahead", Typeahead);
﻿Vue.use(VeeValidate);
Vue.component("assignments-table", AssignmentsTable);