﻿import Vue from 'vue'
import VueResource from 'vue-resource'
import Typeahead from './../Typeahead.vue'
import AssignmentsTable from './AssignmentsTable.vue'

Vue.use(VueResource);

Vue.component("typeahead", Typeahead);
Vue.component("assignments-table", AssignmentsTable);