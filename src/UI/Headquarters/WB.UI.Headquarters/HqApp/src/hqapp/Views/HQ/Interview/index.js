import Review from "./Review"
import appStore from "~/hqapp/store"
import localStore from "./store"

import { apiCaller, getInstance as hubProxy, queryString } from "~/webinterview/api"

function registerStore() {
    appStore.registerModule("webInterview", localStore);
}

async function beforeEnter(to, from, next) {
    registerStore();
    queryString["interviewId"] = to.params["interviewId"]

    const proxy = await hubProxy()
    proxy.state.sectionId = to.params["sectionId"]

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

    next();
}

export default {
    routes: [{
        path: '/Interview/Review/:interviewId',
        component: Review,
        beforeEnter
    }, {
        name: "section",
        path: '/Interview/Review/:interviewId/Section/:sectionId',
        component: Review,
        beforeEnter
    }]
}