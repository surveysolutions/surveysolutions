import Vue from 'vue'
import Vuex from 'vuex'
import { concat, find } from 'lodash'

Vue.use(Vuex)

export default {
    state: {
        entities: [
            // { id: id, ...}
        ],
        total: 0,
        pageSize: 100,
        isLoaded: false,
    },

    actions: {
        async loadAdditionalInfo({ commit }, { id }) {
            const data = await Vue.$api.interview.get('overviewItemAdditionalInfo', { id })
            commit('SET_ADDITIONAL_INFO', {
                id,
                data,
            })
        },

        loadOverviewData({ dispatch, commit }) {
            commit('CLEAR_OVERVIEW')
            dispatch('loadOverview', { skip: 0 })
        },

        loadAllOverviewData({ dispatch, commit, state }) {
            if (!state.isLoaded) {
                dispatch('loadOverview', { skip: state.loaded, take: 99999999 })
            }
        },

        async loadOverview({ commit, dispatch, state }, { skip, take }) {
            const data = await Vue.$api.interview.get('overview', { skip, take: take || state.pageSize })

            commit('SET_OVERVIEW_RESPONSE', data)

            if (!data.isLastPage) {
                dispatch('loadOverview', {
                    skip: skip + data.count,
                    take: take || state.pageSize,
                })
            }
        },
    },

    mutations: {
        CLEAR_OVERVIEW(state) {
            state.total = 0
            state.entities = []
            state.additionalInfo = {}
            state.loaded = 0
            state.isLoaded = false
        },

        SET_OVERVIEW_RESPONSE(state, data) {
            state.total = data.total
            state.entities = concat(state.entities, data.items)
            state.loaded = state.entities.length
            if (data.isLastPage) state.isLoaded = true
        },

        SET_ADDITIONAL_INFO(state, additionalInfo) {
            let entity = find(state.entities, { id: additionalInfo.id })
            entity.additionalInfo = additionalInfo.data
        },
    },

    getters: {

    },
}
