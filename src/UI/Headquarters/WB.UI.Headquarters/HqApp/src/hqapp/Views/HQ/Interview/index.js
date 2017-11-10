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
        var self = this;
        
        return [{
            path: '/Interview/Review/:interviewId',
            component: Review,
            children: [{
                    path: '',
                    component: Cover,
                    props: {
                        navigateToPrefilled: true
                    },
                    beforeEnter(to, from, next){
                        self.changeSection(null)
                        next()
                    }
                },
                {
                    path: 'Cover',
                    name: 'prefilled',
                    component: Cover,
                    props: {
                        navigateToPrefilled: true
                    },
                    beforeEnter(to, from, next){
                        self.changeSection(null)
                        next()
                    }
                },
                {
                    path: 'Section/:sectionId',
                    name: 'section',
                    component: ReviewSection,
                    beforeEnter(to, from, next){
                        self.changeSection(to.params.sectionId)
                        next()
                    }
                }
            ]
        }]
    }

    changeSection(sectionId){ 
        return this.rootStore.dispatch("changeSection", sectionId)
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
