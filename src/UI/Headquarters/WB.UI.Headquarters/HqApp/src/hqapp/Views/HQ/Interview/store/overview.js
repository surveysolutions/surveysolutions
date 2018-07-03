import Vue from "vue"
import Vuex from "vuex"

Vue.use(Vuex)

export default {
    state: {
        entities: [
            // { id: id, ...}
        ],
        total: 0,
        pageSize: 100,
        isLoaded: false
    },

    actions: {
        loadOverviewData({ dispatch, commit }) {
            commit("CLEAR_OVERVIEW")
            dispatch("loadOverview", { skip: 0 })
        },

        async loadOverview({ commit, dispatch, state }, { skip }) {
            const data = await Vue.$api.call(api => api.overview(skip, state.pageSize))

            commit("SET_OVERVIEW_RESPONSE", data)

            if (!data.isLastPage) {
                dispatch("loadOverview", {
                    skip: skip + data.count,
                    take: state.pageSize
                })
            }
        }
    },

    mutations: {
        CLEAR_OVERVIEW(state) {
            state.total = 0
            state.entities = []
            state.loaded = 0
            state.isLoaded = false
        },

        SET_OVERVIEW_RESPONSE(state, data) {
            state.total = data.total
            state.entities = _.concat(state.entities, data.items)
            state.loaded = state.entities.length
            if(data.isLastPage) state.isLoaded = true
        }
    },

    getters: {

    }
}
