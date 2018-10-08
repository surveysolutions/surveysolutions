import Assignments from "./Assignments"
import CreateNew from "./CreateNew"
import localStore from "./store"


import Vue from "vue"
export default class AssignmentsComponent {
    constructor(rootStore) {
        this.rootStore = rootStore;
    }

    get routes() {
        return [{
                path: '/Assignments/',
                component: Assignments
            },
            {
                path: '/HQ/TakeNewAssignment/:interviewId',
                component: CreateNew                
            }
        ]
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
