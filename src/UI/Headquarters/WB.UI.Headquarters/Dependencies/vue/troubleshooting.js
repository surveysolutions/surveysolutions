import Vue from 'vue';
import VueResource from 'vue-resource';

Vue.use(VueResource);

Vue.component("user-selector", require('./userSelector.vue'));

$(function() {
    var app = new Vue({
        http: {
            headers: {
                Authorization: 'Basic ' + input.settings.acsrf.token
            }
        },
        el: '#app',
        data: {
            interviewerId: '6257701a-76ef-4e5b-b076-f9e79c89ae66',
            supervisorId: null
        },
        methods: {
            userSelected(newValue, id) {
                this[id] = newValue;
            }
        }
    });
});
