import Vue from "vue"
import Vuex from "vuex"

Vue.use(Vuex)

export default {
    state: {
        entities: [
            // { id: id, ...}
        ]
    },

    actions: {
        async loadTakeNew({ commit}) {
            const data = await Vue.$api.call(api => api.getPrefilledEntities())
            commit("SET_TAKENEW_RESPONSE", data)
        }
    },

    mutations: {
        SET_TAKENEW_RESPONSE(state, data) {
            state.entities = data.entities
            state.isLoaded = true
        }
    },

    getters: {

    }
}
