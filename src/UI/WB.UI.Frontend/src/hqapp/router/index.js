// //import Vue from 'vue'

import { createRouter, createWebHistory } from 'vue-router';
// import VueRouter from 'vue-router'
// import { assign } from 'lodash'

// //Vue.use(VueRouter)

export default class HqRouter {
    constructor(routes) {
        this.routes = routes
        this.store = routes.store
    }
    get router() {
        const router = createRouter({
            history: createWebHistory(window.CONFIG.basePath),
            routes: Object.entries(this.routes)[0][1]
            //TODO: MIGRATION
        })

        console.log('routes init')

        // tslint:disable:no-string-literal
        router.beforeEach(async (to, from, next) => {
            console.log(to)
            this.store.commit('SET_ROUTE_PARAMS', to.params);
            next()
        })

        return router
    }
}

// export default class HqRouter {
//     constructor(options) {
//         this.options = assign({
//             base: window.CONFIG.basePath,
//             mode: 'history',
//             scrollBehavior() {
//                 return { x: 0, y: 0 }
//             },
//         }, options)
//     }

//     get router() {
//         return new VueRouter(this.options)
//     }
// }
