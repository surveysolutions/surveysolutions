declare var require: any

import Vue = require('vue')
import VueRouter = require('vue-router')
import { virtualPath } from './../config'

Vue.use(VueRouter)

import Interview from '../components/Interview'

export default new VueRouter({
    base: virtualPath + '/',
    mode: 'history',
    routes: [
        { path: '/start/:id', component: Interview } // questionier
    ]
})