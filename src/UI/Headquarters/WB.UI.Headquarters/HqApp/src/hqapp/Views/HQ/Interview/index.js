import Review from "./Review"
import localStore from "./store"

import { apiCaller, getInstance as hubProxy, queryString } from "~/webinterview/api"

export default class ReviewComponent {
    constructor(rootStore) {
        this.rootStore = rootStore;
    }

    get routes() {
        const beforeEnter = (from, to, next) => this.beforeEnter(from, to, next);
        return [
            {
                path: '/Interview/Review/:interviewId',
                component: Review,
                beforeEnter: beforeEnter
            },
            {
                name: "section",
                path: '/Interview/Review/:interviewId/Section/:sectionId',
                component: Review,
                beforeEnter: beforeEnter
            }]
    }

    async beforeEnter(to, from, next) {
        this.initializeIfNeeded()

        queryString["interviewId"] = to.params["interviewId"]
        queryString["review"] = true

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

    initializeIfNeeded() {
        if (this.rootStore.state[this.moduleName] == null) {
            Object.keys(localStore).forEach((module) => {
                this.rootStore.registerModule(module, localStore[module]);
            });
            
        }
    }

    get moduleName() { return "review" }
}
