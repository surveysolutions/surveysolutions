import * as Vue from "vue"
import * as VueRouter from "vue-router"
import { virtualPath } from "./../config"

Vue.use(VueRouter)

import { apiCaller, getInstance as hubProxy, queryString } from "../api"
import Complete from "../components/Complete"
import Section from "../components/Section"
import SideBar from "../components/Sidebar"
import store from "../store"

const router = new VueRouter({
    base: virtualPath + "/",
    mode: "history",
    routes: [
        {
            name: "prefilled",
            path: "/:interviewId/Cover",
            components: {
                default: Section,
                sideBar: SideBar
            }
        },
        {
            name: "cover",
            path: "/Cover/:interviewId",
            components: {
                default: Section,
                sideBar: SideBar
            }
        },
        {
            name: "section",
            path: "/:interviewId/Section/:sectionId",
            components: {
                default: Section,
                sideBar: SideBar
            }
        },
        {
            name: "complete",
            path: "/:interviewId/Complete",
            components: {
                default: Complete,
                sideBar: SideBar
            }
        },
        {
            name: "finish",
            path: "/Finish/:interviewId"
        }
    ],
    scrollBehavior(to, from, savedPosition) {
        if (savedPosition) {
            store.dispatch("sectionRequireScroll", { id: (from.params as any).sectionId })
        }
    }
})

// tslint:disable:no-string-literal
router.beforeEach(async (to, from, next) => {
    store.dispatch("onBeforeNavigate")
    queryString["interviewId"] = to.params["interviewId"]

    hubProxy().then((proxy) => {
        proxy.state.sectionId = to.params["sectionId"]
    })

    if (to.name === "section") {
        const isEnabled = await apiCaller(api => api.isEnabled(to.params["sectionId"]))
        if (!isEnabled) {
            next(false)
        } else {
            next()
        }
    } else {
        next()
    }
})

export default router
