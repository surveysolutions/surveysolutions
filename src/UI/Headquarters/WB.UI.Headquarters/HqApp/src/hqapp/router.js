import Vue from 'vue'
import VueRouter from 'vue-router'

Vue.use(VueRouter)

export default class HqRouter {
    constructor(options) {
        this.options = Object.assign({
            base: window.input.settings.config.basePath,
            mode: "history",
        }, options);
    }

    get router() {
        return new VueRouter(this.options)
    }
}