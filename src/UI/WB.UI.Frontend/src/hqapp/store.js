//import Vue from 'vue'
import Vuex from 'vuex'
import routeParams from '../shared/stores/store.routeParams'

//Vue.use(Vuex)

export function registerStore(vue) {

    const store = new Vuex.Store({
        modules: { route: routeParams },
        getters: {
            workspace() {
                return window.CONFIG.workspace
            },
        },
    })

    vue.use(store)

    return store
}
