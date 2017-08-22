import Vue from 'vue'
import VueRouter from 'vue-router'

import { Assignments, Interviews } from "./views/Interviewer"

import InterviewersAndDevices from "./views/Reports/InterviewersAndDevices"
import CountDaysOfInterviewInStatus from "./views/Reports/CountDaysOfInterviewInStatus"

Vue.use(VueRouter)

export default new VueRouter({
    base: Vue.$config.basePath,
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
    }]
})