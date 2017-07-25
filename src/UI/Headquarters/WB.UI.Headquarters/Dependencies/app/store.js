import Vue from 'vue'
import Vuex from 'vuex'

Vue.use(Vuex)

const config = JSON.parse(window.vueApp.getAttribute('configuration'))

const store = new Vuex.Store({
    state: {
        pendingHandle: null,
        pendingProgress: false,
        config
    },
    actions: {
        showProgress(context) {
            context.commit('SET_PROGRESS_TIMEOUT', setTimeout(() => {
                context.commit('SET_PROGRESS', true)
            }, 750))
        },
        hideProgress(context) {
            clearTimeout(context.state.pendingHandle)
            context.commit('SET_PROGRESS', false)
        },
    },
    mutations: {
        SET_PROGRESS(state, visibility) {
            state.pendingProgress = visibility
        },
        SET_PROGRESS_TIMEOUT(state, handle) {
            state.pendingHandle = handle
        }
    },
    getters: {
        config: state => {
            return state.config
        }
    }
})

export default store