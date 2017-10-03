import Vue from 'vue'
import VueRouter from 'vue-router'

import views from "./views"

Vue.use(VueRouter)

const routes = _(views).chain().map("routes").concat({
    path: "*", component: {
        render:  (h) => h('h2', 
            "Route not found. Base path is window.input.settings.config.basePath: "
            + window.input.settings.config.basePath)
        }
    }
).flatten().value();

export default new VueRouter({
    base: window.input.settings.config.basePath,
    mode: "history",
    routes
})