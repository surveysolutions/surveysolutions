import Vue from 'vue'

import localStore from "./store"

const Review = () => import(/* webpackChunkName: "review" */"./Review")
const Cover = () => import(/* webpackChunkName: "review" */"~/webinterview/components/Cover")
const ReviewSection = () => import(/* webpackChunkName: "review" */"./ReviewSection")
const Overview = () => import(/* webpackChunkName: "review" */"./Overview")

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
                    navigateToPrefilled: true,
                    showHumburger: false
                },
                beforeEnter(to, from, next) {
                    self.changeSection(null, from.params.sectionId)
                    next()
                }
            },
            {
                path: 'Overview',
                name: 'Overview',
                component: Overview,
                props: {
                    navigateToPrefilled: true,
                    showHumburger: false
                },
                beforeEnter(to, from, next) {
                    self.changeSection(null, from.params.sectionId)
                    next()
                }
            },
            {
                path: 'Cover',
                name: 'prefilled',
                component: Cover,
                props: {
                    navigateToPrefilled: true,
                    showHumburger: false
                },
                beforeEnter(to, from, next) {
                    self.changeSection(null, from.params.sectionId)
                    next()
                }
            },
            {
                path: 'Section/:sectionId',
                name: 'section',
                component: ReviewSection,
                beforeEnter(to, from, next) {
                    self.changeSection(to.params.sectionId, from.params.sectionid)
                    next()
                }
            }
            ]
        }]
    }

    initialize() {
    }

    get modules() {
        return localStore;
    }
}
