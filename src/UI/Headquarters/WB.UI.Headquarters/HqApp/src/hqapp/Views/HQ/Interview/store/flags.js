import Vue from "vue"
import Vuex from "vuex"

Vue.use(Vuex)

export default {
    state: {
        flagged: {}
    },

    actions: {
        async setFlag({ commit, rootState, dispatch }, { questionId, hasFlag }) {
            const interviewId = rootState.route.params.interviewId
            await Vue.$api.post('setFlag', {interviewId, questionId, hasFlag})
            commit("SET_FLAG", { questionId, hasFlag })
            dispatch("refreshSearchResults")
        },
        async fetchFlags({ commit, rootState }) {
            const interviewId = rootState.route.params.interviewId
            const flags = await Vue.$http.get('getFlags', {interviewId})
            commit("SET_FLAGS", { flags })
        }
    },

    mutations: {
        SET_FLAG(state, { questionId, hasFlag }) {

            if (hasFlag) {
                Vue.set(state.flagged, questionId, true)
            } else {
                Vue.delete(state.flagged, questionId);
            }
        },
        SET_FLAGS(state, { flags }) {
            state.flagged = {};
            flags.forEach(flag => state.flagged[flag] = true)
        }
    },
    
    getters: {
        flags(state) {
            return state.flagged
        }
    }
}
