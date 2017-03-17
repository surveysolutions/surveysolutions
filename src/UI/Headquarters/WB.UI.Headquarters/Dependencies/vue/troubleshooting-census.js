﻿import Vue from 'vue'
import VueResource from 'vue-resource'
import UserSelector from './UserSelector.vue'
import DatePicker from './DatePicker.vue'
import InterviewTable from './InterviewTable.vue'
import VeeValidate from 'vee-validate';

Vue.use(VeeValidate);
Vue.use(VueResource);

Vue.component('Flatpickr', DatePicker);
Vue.component("user-selector", UserSelector);
Vue.component("interview-table", InterviewTable);