import Vue from 'vue'
import VueRouter from 'vue-router'

import Assignments from "./views/Assignments"
import Interviews from "./views/Interviews"
import InterviewersAndDevices from "./views/Reports/InterviewersAndDevices"

Vue.use(VueRouter)

export default new VueRouter({
    base: Vue.$config.basePath + "/",
    mode: "history",
    routes: [{
        path: '/Reports/InterviewersAndDevices',
        component: InterviewersAndDevices
    },
    {
        path: '/CreateNew', component: Assignments
    }, {
        path: '/Rejected', component: Interviews
    }, {
        path: '/Completed', component: Interviews
    }, {
        path: '/Started', component: Interviews
    }
    ]
})