﻿import Vue from 'vue'
import VueResource from 'vue-resource'
import Typeahead from './Typeahead.vue'
import DatePicker from './DatePicker.vue'
import InterviewTable from './InterviewTable.vue'
import VeeValidate from 'vee-validate';

Vue.use(VeeValidate);
Vue.use(VueResource);

Vue.component('Flatpickr', DatePicker);
Vue.component("typeahead", Typeahead);
Vue.component("interview-table", InterviewTable);