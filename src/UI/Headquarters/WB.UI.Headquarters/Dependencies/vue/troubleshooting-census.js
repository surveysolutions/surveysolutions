﻿import Vue from 'vue'
import VueResource from 'vue-resource'
import UserSelector from './UserSelector.vue'
import DatePicker from './DatePicker.vue'
import VeeValidate from 'vee-validate';

Vue.use(VeeValidate);
Vue.use(VueResource);

Vue.component('Flatpickr', DatePicker);
Vue.component("user-selector", UserSelector);

var app = new Vue({
    data: {
        interviewerId: null,
        questionnaireId: null,
        dateFrom: null,
        dateTo: null,
        dateRangePickerOptions: {
            mode: "range",
            maxDate: "today",
            minDate: new Date().fp_incr(-30)
        }
    },
    methods: {
        userSelected(newValue) {
            this.interviewerId = newValue;
        },
        questionnaireSelected(newValue) {
            this.questionnaireId = newValue;
        },
        rangeSelected(textValue, from, to) {
            this.dateFrom = from;
            this.dateTo = to;
        },
        validateForm() {
            this.$validator.validateAll().then(result => {
                if (result) {
                    this.findInterviews();
                }
            });
        },
        findInterviews() {
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