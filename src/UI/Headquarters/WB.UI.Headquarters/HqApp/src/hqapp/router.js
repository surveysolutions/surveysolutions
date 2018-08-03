import Vue from 'vue'
import VueRouter from 'vue-router'

Vue.use(VueRouter)

export default class HqRouter {
    constructor(options) {
        this.options = Object.assign({
            base: window.input.settings.config.basePath,
            mode: "history",
            scrollBehavior(to, from, savedPosition) {
                return { x: 0, y: 0 }
            }
        }, options);
    }

    get router() {
        return new VueRouter(this.options)
    }
}