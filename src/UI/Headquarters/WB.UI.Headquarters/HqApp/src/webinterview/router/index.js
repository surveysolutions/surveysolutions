import Vue from "vue"
import VueRouter from "vue-router"

Vue.use(VueRouter)

import { apiCaller, getInstance as hubProxy, queryString } from "../api"
import Complete from "../components/Complete"
import Cover from "../components/Cover"
import Section from "../components/Section"
import SideBar from "../components/Sidebar"
import store from "../store"

const router = new VueRouter({
    base: Vue.$config.virtualPath + "/",
    mode: "history",
    routes: [
        {
            name: "prefilled",
            path: "/:interviewId/Cover",
            components: {
                default: Cover,
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
            store.dispatch("sectionRequireScroll", { id: (from.params ).sectionId })
        } else {
            return { x: 0, y: 0 }
        }
    }
})

// tslint:disable:no-string-literal
router.beforeEach(async (to, from, next) => {
    queryString["interviewId"] = to.params["interviewId"]

    hubProxy().then((proxy) => {
        proxy.state.sectionId = to.params["sectionId"]
    })

    if (to.name === "section") {
        const isEnabled = await apiCaller(api => api.isEnabled(to.params["sectionId"]))
        if (!isEnabled) {
            next(false)
            return
        } else {
            next()
        }
    } else {
        next()
    }

    // navigation could be canceled
    store.dispatch("onBeforeNavigate")

    const hamburger = document.getElementById("sidebarHamburger")

    // check for button visibility.
    if (hamburger && hamburger.offsetParent != null) {
        store.dispatch("toggleSidebarPanel", false /* close sidebar panel */)
    }
})

export default router
