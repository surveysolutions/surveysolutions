import Layout from './Layout'
import Assignments from './HqAssignments'
import CreateNew from './CreateNew'
import Details from './Details'
import Upload from './Upload'
import localStore from './store'

import Vue from 'vue'
export default class AssignmentsComponent {
    constructor(rootStore) {
        this.rootStore = rootStore
    }

    get routes() {
        return [
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
                    }, 
                    {
                        path: 'Upload/:questionnaireId', component: Upload
                    }]
            }]
    }

    initialize() {
        const VeeValidate = require('vee-validate')
        Vue.use(VeeValidate)
    }

    get modules() {
        return localStore
    }
}
