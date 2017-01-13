import * as Vue from "vue"
import * as VueRouter from "vue-router"
import { virtualPath } from "./../config"

Vue.use(VueRouter)

import { getInstance as hubProxy } from "../api"
import Section from "../components/Section"
import Start from "../components/Start"

const router = new VueRouter({
    base: virtualPath + "/",
    mode: "history",
    routes: [
        { name: "root", path: "/:questionnaireId", component: Start },
        { name: "prefilled", path: "/:interviewId/Cover", component: Section },
        { name: "section", path: "/:interviewId/Section/:sectionId", component: Section }
    ]
})

router.afterEach((to, from) => {
    hubProxy().then((proxy) => {
        // tslint:disable-next-line:no-string-literal
        proxy.state.interviewId = to.params["interviewId"]
        // tslint:disable-next-line:no-string-literal
        proxy.state.sectiondId = to.params["sectionId"]
    })
})

export default router
