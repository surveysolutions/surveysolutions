import Review from "./Review"
import localStore from "./store"
import Vue from 'vue'
import Cover from "~/webinterview/components/Cover"
import ReviewSection from "./ReviewSection"

export default class ReviewComponent {
    constructor(rootStore) {
        this.rootStore = rootStore;
    }

    get routes() {
        return [
            {
                path: '/Interview/Review/:interviewId',
                component: Review,
                children: [
                    {
                        path: '',
                        component: Cover
                    },
                    {
                        path: 'Cover',
                        name: 'prefilled',
                        component: Cover
                    },
                    {
                        path: 'Section/:sectionId',
                        name: 'section',
                        component: ReviewSection
                    }
                ]
            }
        ]
    }

    beforeEnter(to, from, next) {
        Vue.$api.queryString["interviewId"] = to.params["interviewId"]
        Vue.$api.queryString["review"] = true

        this.rootStore.dispatch("navigatingTo", {
            interviewId: to.params["interviewId"],
            sectionId: to.params["sectionId"]
        })

        Vue.$api.hub().then((proxy) => {
            proxy.state.sectionId = to.params["sectionId"]
            if (to.name === "section") {
                const isEnabled = Vue.$api.call(api => api.isEnabled(to.params["sectionId"])).then((isEnabled) => {
                    if (!isEnabled) {
                        next(false);
                    } else {
                        next();
                    }
                });
            } else {
                next();
            }

            next();
        })
    }

    initialize() {
        const installApi = require("~/webinterview/api").install
        installApi(Vue, { store: this.rootStore });
    }

    get modules() { return localStore; }
}
