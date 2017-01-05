declare var require: any

import * as Vue from "vue"
import * as VueRouter from "vue-router"
import { virtualPath } from "./../config"

Vue.use(VueRouter)

import Interview from "../components/Interview"
import Prefilled from "../components/Prefilled"
import Start from "../components/Start"

export default new VueRouter({
    base: virtualPath + "/",
    mode: "history",
    routes: [
        { path: "/", component: Start },
        { path: "/start/:id", component: Interview },
        { name: "prefilled", path: "/prefilled/:id", component: Prefilled }
    ]
})
