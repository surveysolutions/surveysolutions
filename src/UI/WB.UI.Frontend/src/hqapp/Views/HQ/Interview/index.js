import localStore from './store'
import Vue from 'vue'

const Review = () => import('./Review')
const Cover = () => import('~/webinterview/components/Cover')
const ReviewSection = () => import('./ReviewSection')
const Overview = () => import('./Overview')

export default class ReviewComponent {
    constructor(rootStore) {
        this.rootStore = rootStore
        this.config = Vue.$config.model || {}
    }

    get routes() {
        return [{
            path: '/Interview/Review/:interviewId',
            component: Review,
            children: [{
                path: '',
                component: Cover,
                props: {
                    navigateToPrefilled: true,
                    showHumburger: false,
                },
                beforeEnter: (to, from, next) => {
                    if (this.config.coverPageId)
                        to.params.sectionId = this.config.coverPageId
                    next()
                },
            },
            {
                path: 'Overview',
                name: 'Overview',
                component: Overview,
                props: {
                    navigateToPrefilled: true,
                    showHumburger: false,
                },
            },
            {
                path: 'Cover',
                name: 'prefilled',
                component: Cover,
                props: {
                    navigateToPrefilled: true,
                    showHumburger: false,
                },
                beforeEnter: (to, from, next) => {
                    if (this.config.coverPageId)
                        next({ name: 'cover', params: { interviewId: to.params.interviewId } })
                    else
                        next()
                },
            },
            {
                name: 'cover',
                path: 'Section/' + (this.config.coverPageId || 'newcover'),
                component: Cover,
                props: {
                    navigateToPrefilled: true,
                    showHumburger: false,
                },
                beforeEnter: (to, from, next) => {
                    if (this.config.coverPageId)
                        to.params.sectionId = this.config.coverPageId
                    next()
                },
            },
            {
                path: 'Section/:sectionId',
                name: 'section',
                component: ReviewSection,
                beforeEnter: (to, from, next) => {
                    if (this.config.coverPageId && to.params.sectionId == this.config.coverPageId)
                        next({ name: 'cover' })
                    else
                        next()
                },
            },
            ],
        }]
    }

    initialize() {
    }

    get modules() {
        return localStore
    }
}
