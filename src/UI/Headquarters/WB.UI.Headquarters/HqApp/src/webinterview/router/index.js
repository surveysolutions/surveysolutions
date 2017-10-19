import Vue from "vue"
import VueRouter from "vue-router"

Vue.use(VueRouter)

import Complete from "../components/Complete"
import Cover from "../components/Cover"
import Section from "../components/Section"
import SideBar from "../components/Sidebar"

function NewRouter(store) {

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
                store.dispatch("sectionRequireScroll", { id: (from.params).sectionId })
            } else {
                return { x: 0, y: 0 }
            }
        }
    })

    // tslint:disable:no-string-literal
    router.beforeEach(async (to, from, next) => {
        Vue.$api.queryString["interviewId"] = to.params["interviewId"]

        const proxy = await Vue.$api.hub()
        proxy.state.sectionId = to.params["sectionId"]

        if (to.name === "section") {
            const isEnabled = await Vue.$api.call(api => api.isEnabled(to.params["sectionId"]))
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

    return router;
}

export default NewRouter;
