import Assignments from "./Assignments"
import CreateNew from "./CreateNew"
import Vue from "vue"
export default class AssignmentsComponent {
    get routes() {
        return [
        {
            path: '/Assignments/', component: Assignments
        },
        {
            path: '/HQ/TakeNew', component: CreateNew
        }
        ]
    }
    
    initialize() {
        const VeeValidate = require('vee-validate');
        Vue.use(VeeValidate);
    }
}
