import Vue from 'vue'

import localStore from "./store"

const Review = () => import(/* webpackChunkName: "review" */"./Review")
const Cover = () => import(/* webpackChunkName: "review" */"~/webinterview/components/Cover")
const ReviewSection = () => import(/* webpackChunkName: "review" */"./ReviewSection")


export default class ReviewComponent {
    constructor(rootStore) {
        this.rootStore = rootStore;
    }

    get routes() {
        return [{
            path: '/Interview/Review/:interviewId',
            component: Review,
            children: [{
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
        }]
    }

    async beforeEnter(to, from, next) {

        await Vue.$api.hub({
            interviewId: to.params["interviewId"],
            review: true
        })
        
        next();
    }

    initialize() {

        const installApi = require("~/webinterview/api").install
        installApi(Vue, {
            store: this.rootStore
        });
    }

    get modules() {
        return localStore;
    }
}
