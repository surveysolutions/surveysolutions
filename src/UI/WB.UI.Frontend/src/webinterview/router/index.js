import Vue from 'vue'
import VueRouter from 'vue-router'

Vue.use(VueRouter)

import Complete from '../components/Complete'
import Cover from '../components/Cover'
import Section from '../components/Section'
import SideBar from '../components/Sidebar'
import Splash from '../components/Splash'

const Interview = () => import('~/webinterview/components/Interview.vue')

function NewRouter(store) {

    const router = new VueRouter({
        base: Vue.$config.virtualPath,
        mode: 'history',
        routes: [
            {
                name: 'finish',
                path: '/Finish/:interviewId',
            },
            {
                name: 'splash',
                path: '/run/:interviewId',
                components: {
                    default: Splash,
                    sideBar: SideBar,
                },
            },
            {
                name: 'WebInterview',
                path: '/:interviewId',
                components: {
                    default: Interview,
                    sideBar: SideBar,
                },
                children: [
                    {
                        name: 'prefilled',
                        path: 'Cover',
                        component: Cover,
                        beforeEnter: (to, from, next) => {
                            if (Vue.$config.coverPageId)
                                next({ name: 'cover', params: { interviewId: to.params.interviewId } })
                            else
                                next()
                        },
                    },
                    {
                        name: 'cover',
                        path: 'Section/' + (Vue.$config.coverPageId || 'newcover'),
                        component: Cover,
                        beforeEnter: (to, from, next) => {
                            if (Vue.$config.coverPageId)
                                to.params.sectionId = Vue.$config.coverPageId
                            next()
                        },
                    },
                    {
                        name: 'section',
                        path: 'Section/:sectionId',
                        component: Section,
                        beforeEnter: (to, from, next) => {
                            if (Vue.$config.coverPageId && to.params.sectionId == Vue.$config.coverPageId)
                                next({ name: 'cover' })
                            else
                                next()
                        },
                    },
                    {
                        name: 'complete',
                        path: 'Complete',
                        component: Complete,
                    },
                ],
            },
        ],
        scrollBehavior(to, from, savedPosition) {
            if (savedPosition) {
                store.dispatch('sectionRequireScroll', { id: (from.params).sectionId })
            } else {
                return { x: 0, y: 0 }
            }
        },
    })

    // tslint:disable:no-string-literal
    router.beforeEach(async (to, from, next) => {
        if (Vue.$config.splashScreen) { next(); return }
        next()
    })

    router.afterEach(() => {
        const hamburger = document.getElementById('sidebarHamburger')

        // check for button visibility.
        if (hamburger && hamburger.offsetParent != null) {
            store.dispatch('toggleSidebarPanel', false /* close sidebar panel */)
        }
    })

    store.subscribeAction((action, state) => {
        switch (action.type) {
            case 'finishInterview':
                location.replace(router.resolve({ name: 'finish', params: { interviewId: state.route.params.interviewId } }).href)
                break
            case 'navigateTo':
                router.replace(action.payload)
                break
        }
    })

    return router
}

export default NewRouter
