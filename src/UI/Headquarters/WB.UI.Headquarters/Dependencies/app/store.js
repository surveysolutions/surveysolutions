import Vue from 'vue'
import Vuex from 'vuex'

Vue.use(Vuex)

const config = JSON.parse(window.vueApp.getAttribute('configuration'))

const store = new Vuex.Store({
    state: {
        pendingProgress: false,
        config
    },
    actions: {
        showProgress(context) {
            context.commit('SET_PROGRESS', true)
        },
        hideProgress(context) {
            context.commit('SET_PROGRESS', false)
        },
    },
    mutations: {
        SET_PROGRESS(state, visibility) {
            state.pendingProgress = visibility
        }
    },
    getters: {
        config: state => {
            return state.config
        }
    }
})

export default store