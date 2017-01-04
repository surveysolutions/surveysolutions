declare var require: any
declare var INTERVIEW_APP_CONFIG:any

import Vue = require('vue')
import VueRouter = require('vue-router')

Vue.use(VueRouter)

const Interview = require('../components/Interview')

debugger

export default new VueRouter({
    base: INTERVIEW_APP_CONFIG.virtualPath + '/',
    mode: 'history',
    routes: [
        { path: '/start/:id', component: Interview } // questionier
    ]
})