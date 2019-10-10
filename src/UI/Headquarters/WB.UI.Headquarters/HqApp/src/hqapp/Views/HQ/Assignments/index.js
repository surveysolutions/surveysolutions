import Layout from "./Layout"
import Assignments from "./HqAssignments"
import CreateNew from "./CreateNew"
import Details from "./Details"
import localStore from "./store"


import Vue from "vue"
export default class AssignmentsComponent {
    constructor(rootStore) {
        this.rootStore = rootStore;
    }

    get routes() {
        return[
            {
                path: '/HQ/TakeNewAssignment/:interviewId',
                component: CreateNew 
            },
            {
                path: '/Assignments', component: Layout,
                children: [
                    {
                        path: '', component: Assignments
                    },
                    {
                        path: ':assignmentId', component: Details
                    }]}];
    }

    initialize() {
        const VeeValidate = require('vee-validate');
        Vue.use(VeeValidate);
        const installApi = require("~/webinterview/api").install

        installApi(Vue, {
            store: this.rootStore
        });
    }

    get modules() {
        return localStore;
    }
}
