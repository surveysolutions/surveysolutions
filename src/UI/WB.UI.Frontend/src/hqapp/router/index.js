import { createRouter, createWebHistory } from 'vue-router';

export default class HqRouter {
    constructor(routes) {
        this.routes = routes
        this.store = routes.store
    }
    get router() {
        const router = createRouter({
            history: createWebHistory(window.CONFIG.basePath),
            routes: Object.entries(this.routes)[0][1]
        })

        // tslint:disable:no-string-literal
        router.beforeEach(async (to, from, next) => {
            this.store.commit('SET_ROUTE', to);
            next()
        })

        return router
    }
}