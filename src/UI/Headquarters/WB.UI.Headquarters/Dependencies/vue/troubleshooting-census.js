import Vue from 'vue'
import VueResource from 'vue-resource'
import UserSelector from './UserSelector.vue'

Vue.use(VueResource);

Vue.component("user-selector", UserSelector);

var app = new Vue({
    data: {
        interviewerId: null,
        questionnaireId: null
    },
    methods: {
        userSelected(newValue, id) {
            this.interviewerId = newValue;
        },
        questionnaireSelected(newValue, id) {
            this.questionnaireId = newValue;
        }
    }
});

window.onload = function () {
    Vue.http.headers.common['Authorization'] = input.settings.acsrf.token;

    app.$mount('#app');
}