import * as Vue from "vue"
import * as VueRouter from "vue-router"
import { virtualPath } from "./../config"

Vue.use(VueRouter)

import { getInstance as hubProxy, queryString } from "../api"
import Section from "../components/Section"
import store from "../store"

const router = new VueRouter({
    base: virtualPath + "/",
    mode: "history",
    routes: [
        { name: "prefilled", path: "/:interviewId/Cover", component: Section },
        { name: "section", path: "/:interviewId/Section/:sectionId", component: Section }
    ],
    scrollBehavior(to, from, savedPosition) {
        if (savedPosition) {
            store.dispatch("sectionRequireScroll", { id: (from.params as any).sectionId })
        }
    }
})

router.afterEach((to, from) => {
    // tslint:disable-next-line:no-string-literal
    queryString["interviewId"] = to.params["interviewId"]

    hubProxy().then((proxy) => {
        // tslint:disable-next-line:no-string-literal
        proxy.state.sectionId = to.params["sectionId"]
    })
})

export default router
