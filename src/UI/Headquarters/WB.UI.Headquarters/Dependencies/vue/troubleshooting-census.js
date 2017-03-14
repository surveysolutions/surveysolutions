import Vue from 'vue'
import VueResource from 'vue-resource'
import UserSelector from './UserSelector.vue'
import DatePicker from './DatePicker.vue'

Vue.component('Flatpickr', DatePicker);
Vue.use(VueResource);


Vue.component("user-selector", UserSelector);

var app = new Vue({
    data: {
        interviewerId: null,
        questionnaireId: null,
        dateStr: null,
        dateRangePickerOptions: {
            mode: "range",
            maxDate: "today",
        }
    },
    methods: {
        userSelected(newValue, id) {
            this.interviewerId = newValue;
        },
        questionnaireSelected(newValue, id) {
            this.questionnaireId = newValue;
        },
        rangeSelected(newValue, id) {
            console.log(newValue);
            console.log(id);
        }
    }
});

window.onload = function () {
    Vue.http.headers.common['Authorization'] = input.settings.acsrf.token;

    app.$mount('#app');
}