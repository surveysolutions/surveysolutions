import * as Vue from "vue"
import * as VueRouter from "vue-router"
import { virtualPath } from "./../config"

Vue.use(VueRouter)

import { getInstance as hubProxy } from "../api"
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
            // handling history back event, setting scroll requirement for `from` sectionId
            store.dispatch("fetch/sectionRequireScroll", { id: (from.params as any).sectionId })
        } else if (!(store.state as any).fetch.scroll) {
            store.dispatch("fetch/sectionRequireScroll", { top: 0 })
        }
    }
})

router.afterEach((to, from) => {
    hubProxy().then((proxy) => {
        // tslint:disable-next-line:no-string-literal
        proxy.state.interviewId = to.params["interviewId"]
        // tslint:disable-next-line:no-string-literal
        proxy.state.sectionId = to.params["sectionId"]
    })
})

export default router
