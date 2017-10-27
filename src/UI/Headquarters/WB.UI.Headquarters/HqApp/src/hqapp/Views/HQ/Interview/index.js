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
                        component: Cover,
                        props: {
                            navigateToPrefilled: true
                        }
                    },
                    {
                        path: 'Cover',
                        name: 'prefilled',
                        component: Cover,
                        props: {
                            navigateToPrefilled: true
                        }
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

        Vue.$api.hub({
            interviewId: to.params["interviewId"],
            review: true
        }).then(() => {
            if (to.name === "section") {
                this.rootStore.dispatch("changeSection", to.params.sectionId)
                Vue.$api.call(api => api.isEnabled(to.params["sectionId"])).then((isEnabled) => {
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
