declare var require: any

import * as Vue from "vue"
import * as VueRouter from "vue-router"
import { virtualPath } from "./../config"

Vue.use(VueRouter)

import Interview from "../components/Interview"
import Prefilled from "../components/Prefilled"

export default new VueRouter({
    base: virtualPath + "/",
    mode: "history",
    routes: [
        { path: "/start/:id", component: Interview },
        { path: "/prefilled/:id", component: Prefilled }
    ]
})
