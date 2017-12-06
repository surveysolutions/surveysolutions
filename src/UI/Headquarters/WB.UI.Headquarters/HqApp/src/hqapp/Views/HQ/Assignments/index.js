import Assignments from "./Assignments"
import Vue from "vue"
export default class AssignmentsComponent {
    get routes() {
        return [{
            path: '/Assignments/', component: Assignments
        }]
    }
    
    initialize() {
        const VeeValidate = require('vee-validate');
        Vue.use(VeeValidate);
    }
}