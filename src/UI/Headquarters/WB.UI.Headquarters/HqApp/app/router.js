import Vue from 'vue'
import VueRouter from 'vue-router'

import { Assignments, Interviews } from "./views/Interviewer"

import InterviewersAndDevices from "./views/Reports/InterviewersAndDevices"
import CountDaysOfInterviewInStatus from "./views/Reports/CountDaysOfInterviewInStatus"

Vue.use(VueRouter)

export default new VueRouter({
    base: window.input.settings.config.basePath,
    mode: "history",
    routes: [{
        path: '/Reports/InterviewersAndDevices',
        component: InterviewersAndDevices
    }, {
        path: '/Reports/CountDaysOfInterviewInStatus', component: CountDaysOfInterviewInStatus
    }, {
        path: '/InterviewerHq/CreateNew', component: Assignments
    }, {
        path: '/InterviewerHq/Rejected', component: Interviews
    }, {
        path: '/InterviewerHq/Completed', component: Interviews
    }, {
        path: '/InterviewerHq/Started', component: Interviews
    }, {
        path: "*", component: {
            render: function (createElement) {
                return createElement(
                    'h2', "Route not found. Base path is window.input.settings.config.basePath: " + window.input.settings.config.basePath
                )
            },
        }
    }
    ]
})