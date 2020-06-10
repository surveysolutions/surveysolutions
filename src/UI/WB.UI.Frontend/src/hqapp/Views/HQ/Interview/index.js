import localStore from './store'

const Review = () => import(/* webpackChunkName: "review" */'./Review')
const Cover = () => import(/* webpackChunkName: "review" */'~/webinterview/components/Cover')
const ReviewSection = () => import(/* webpackChunkName: "review" */'./ReviewSection')
const Overview = () => import(/* webpackChunkName: "review" */'./Overview')

export default class ReviewComponent {
    constructor(rootStore) {
        this.rootStore = rootStore
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
            },
            {
                name: 'cover',
                path: 'Section/11111111111111111111111111111111',
                component: Cover,
                props: {
                    navigateToPrefilled: true,
                    showHumburger: false,
                },
                beforeEnter: (to, from, next) => {
                    to.params.sectionId = '11111111111111111111111111111111'
                    next()
                },
            },
            {
                path: 'Section/:sectionId',
                name: 'section',
                component: ReviewSection,
                beforeEnter: (to, from, next) => {
                    if (to.params.sectionId == '11111111111111111111111111111111')
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
