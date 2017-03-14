import Vue from 'vue'
import VueResource from 'vue-resource'
import UserSelector from './UserSelector.vue'
import DatePicker from './DatePicker.vue'

Vue.use(VueResource);

Vue.component('Flatpickr', DatePicker);
Vue.component("user-selector", UserSelector);

var app = new Vue({
    data: {
        interviewerId: null,
        questionnaireId: null,
        dateStr: null,
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
        rangeSelected(textValue, left, right) {
            console.log(textValue);
            console.log("left:" + left);
            console.log("right:" + right);
        }
    }
});

window.onload = function () {
    Vue.http.headers.common['Authorization'] = input.settings.acsrf.token;

    app.$mount('#app');
}