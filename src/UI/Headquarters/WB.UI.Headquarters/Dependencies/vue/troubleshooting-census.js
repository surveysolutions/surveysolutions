import Vue from 'vue'
import VueResource from 'vue-resource'
import UserSelector from './UserSelector.vue'

Vue.use(VueResource);

Vue.component("user-selector", UserSelector);


var app = new Vue({
    data: {
        interviewerId: null,
        supervisorId: null
    },
    methods: {
        userSelected(newValue, id) {
            this[id] = newValue;
        }
    }
});

window.onload = function () {
    app.$mount('#app');
}

