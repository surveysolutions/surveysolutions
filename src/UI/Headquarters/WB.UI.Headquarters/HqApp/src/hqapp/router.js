import Vue from 'vue'
import VueRouter from 'vue-router'
import store from './store'

import views from "./views"

import { apiCaller, getInstance as hubProxy, queryString } from "~/webinterview/api"

Vue.use(VueRouter)

const routes = _.chain(views).flattenDeep().map("routes").flatten().value();

routes.push({path: "*", component: {
    render:  (h) => h('h2', 
        "Route not found. Base path is window.input.settings.config.basePath: "
        + window.input.settings.config.basePath)
    }
})

var router =  new VueRouter({
    base: window.input.settings.config.basePath,
    mode: "history",
    routes
});

router.beforeEach(async function(to, from, next) {
    var str = to.meta.store;
    store.registerModule(str.key, str.value);
    queryString["interviewId"] = to.params["id"]
    
    const proxy = await hubProxy()
    proxy.state.sectionId = to.params["sectionId"]

    if (to.name === "section") {
        const isEnabled = await apiCaller(api => api.isEnabled(to.params["sectionId"]))
        if (!isEnabled) {
            next(false)
            return
        } else {
            next()
        }
    } else {
        next()
    }

    next();
});

export default router;