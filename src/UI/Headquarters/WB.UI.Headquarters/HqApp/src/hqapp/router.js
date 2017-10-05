import Vue from 'vue'
import VueRouter from 'vue-router'

import views from "./views"

Vue.use(VueRouter)

const routes = _.chain(views).flattenDeep().map("routes").flatten().value();

routes.push({path: "*", component: {
    render:  (h) => h('h2', 
        "Route not found. Base path is window.input.settings.config.basePath: "
        + window.input.settings.config.basePath)
    }
})

export default new VueRouter({
    base: window.input.settings.config.basePath,
    mode: "history",
    routes
})