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

var app = new Vue({
    data: {
        interviewerId: null,
        questionnaireId: null,
        changedFrom: null,
        changedTo: null,
        dateRangePickerOptions: {
            mode: "range",
            maxDate: "today",
            minDate: new Date().fp_incr(-30)
        },
        tableFilters: {}
    },
    computed: {
    },
    methods: {
        userSelected(newValue) {
            this.interviewerId = newValue;
        },
        questionnaireSelected(newValue) {
            this.questionnaireId = newValue;
        },
        rangeSelected(textValue, from, to) {
            this.changedFrom = from;
            this.changedTo = to;
        },
        validateForm() {
            this.$validator.validateAll().then(result => {
                if (result) {
                    this.findInterviews();
                }
            });
        },
        findInterviews() {
            this.tableFilters = {
                interviewerId: this.interviewerId,
                questionnaireId: this.questionnaireId,
                changedFrom: this.changedFrom,
                changedTo: this.changedTo
            };
            document.querySelector("main").classList.remove("search-wasnt-started");
        }
    },
    mounted: function() {
        document.querySelector("main").classList.remove("hold-transition");
    }
});

window.onload = function () {
    Vue.http.headers.common['Authorization'] = input.settings.acsrf.token;

    app.$mount('#app');
}