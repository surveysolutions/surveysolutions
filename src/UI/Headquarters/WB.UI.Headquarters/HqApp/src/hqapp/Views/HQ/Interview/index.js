import Review from "./Review"
import localStore from "./store"
import Vue from 'vue'

export default class ReviewComponent {
    constructor(rootStore) {
        this.rootStore = rootStore;
    }

    get routes() {
        return [
            {
                path: '/Interview/Review/:interviewId',
                component: Review,
                props: true
            },
            {
                name: "section",
                path: '/Interview/Review/:interviewId/Section/:sectionId',
                component: Review
            }
        ]
    }

    async beforeEnter(to, from, next) {
        Vue.$api.queryString["interviewId"] = to.params["interviewId"]
        Vue.$api.queryString["review"] = true

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

        next();
    }

    initialize() {
        const installApi = require("~/webinterview/api").install
        installApi(Vue, { store: this.rootStore });
    }

    get modules() { return localStore; }
}
