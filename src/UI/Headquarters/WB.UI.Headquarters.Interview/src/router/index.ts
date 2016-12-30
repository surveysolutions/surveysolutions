declare var require: any

import Vue = require('vue')
import VueRouter = require('vue-router')

Vue.use(VueRouter)

const Interview = require('../components/Interview')

export default new VueRouter({
    // mode: 'history',
    routes: [
        { path: '/start/:id', component: Interview } // questionier
    ]
})