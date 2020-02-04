import Vue from 'vue'
import VueRouter from 'vue-router'
import { assign } from 'lodash'

Vue.use(VueRouter)

export default class HqRouter {
    constructor(options) {
        this.options = assign({
            base: window.CONFIG.basePath,
            mode: 'history',
            scrollBehavior() {
                return { x: 0, y: 0 }
            },
        }, options)
    }

    get router() {
        return new VueRouter(this.options)
    }
}
