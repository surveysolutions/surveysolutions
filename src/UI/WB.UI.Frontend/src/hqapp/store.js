//import Vue from 'vue'
import Vuex from 'vuex'

Vue.use(Vuex)

export function registerStore(vue) {

    const store = new Vuex.Store({
        getters: {
            workspace() {
                return window.CONFIG.workspace
            },
        },
    })

    vue.use(store)

    return store
}
